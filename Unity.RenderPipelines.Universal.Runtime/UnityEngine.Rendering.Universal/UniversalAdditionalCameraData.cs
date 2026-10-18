using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class UniversalAdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
{
	private enum Version
	{
		Initial = 0,
		DepthAndOpaqueTextureOptions = 2,
		Count = 3
	}

	private const string k_GizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/";

	private const string k_BaseCameraGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";

	private const string k_OverlayCameraGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";

	private const string k_PostProcessingGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_PostProcessing.png";

	[FormerlySerializedAs("renderShadows")]
	[SerializeField]
	private bool m_RenderShadows = true;

	[SerializeField]
	private CameraOverrideOption m_RequiresDepthTextureOption = CameraOverrideOption.UsePipelineSettings;

	[SerializeField]
	private CameraOverrideOption m_RequiresOpaqueTextureOption = CameraOverrideOption.UsePipelineSettings;

	[SerializeField]
	private CameraRenderType m_CameraType;

	[SerializeField]
	private List<Camera> m_Cameras = new List<Camera>();

	[SerializeField]
	private int m_RendererIndex = -1;

	[SerializeField]
	private LayerMask m_VolumeLayerMask = 1;

	[SerializeField]
	private Transform m_VolumeTrigger;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateModeOption = VolumeFrameworkUpdateMode.UsePipelineSettings;

	[SerializeField]
	private bool m_RenderPostProcessing;

	[SerializeField]
	private AntialiasingMode m_Antialiasing;

	[SerializeField]
	private AntialiasingQuality m_AntialiasingQuality = AntialiasingQuality.High;

	[SerializeField]
	private bool m_StopNaN;

	[SerializeField]
	private bool m_Dithering;

	[SerializeField]
	private bool m_ClearDepth = true;

	[SerializeField]
	private bool m_AllowXRRendering = true;

	[SerializeField]
	private bool m_AllowHDROutput = true;

	[SerializeField]
	private bool m_UseScreenCoordOverride;

	[SerializeField]
	private Vector4 m_ScreenSizeOverride;

	[SerializeField]
	private Vector4 m_ScreenCoordScaleBias;

	[NonSerialized]
	private Camera m_Camera;

	[FormerlySerializedAs("requiresDepthTexture")]
	[SerializeField]
	private bool m_RequiresDepthTexture;

	[FormerlySerializedAs("requiresColorTexture")]
	[SerializeField]
	private bool m_RequiresColorTexture;

	[NonSerialized]
	private MotionVectorsPersistentData m_MotionVectorsPersistentData = new MotionVectorsPersistentData();

	[NonSerialized]
	internal UniversalCameraHistory m_History = new UniversalCameraHistory();

	[SerializeField]
	internal TemporalAA.Settings m_TaaSettings = TemporalAA.Settings.Create();

	private static UniversalAdditionalCameraData s_DefaultAdditionalCameraData;

	private static List<VolumeStack> s_CachedVolumeStacks;

	private VolumeStack m_VolumeStack;

	[SerializeField]
	private Version m_Version = Version.Count;

	internal static UniversalAdditionalCameraData defaultAdditionalCameraData
	{
		get
		{
			if (s_DefaultAdditionalCameraData == null)
			{
				s_DefaultAdditionalCameraData = new UniversalAdditionalCameraData();
			}
			return s_DefaultAdditionalCameraData;
		}
	}

	internal Camera camera
	{
		get
		{
			if (!m_Camera)
			{
				base.gameObject.TryGetComponent<Camera>(out m_Camera);
			}
			return m_Camera;
		}
	}

	public bool renderShadows
	{
		get
		{
			return m_RenderShadows;
		}
		set
		{
			m_RenderShadows = value;
		}
	}

	public CameraOverrideOption requiresDepthOption
	{
		get
		{
			return m_RequiresDepthTextureOption;
		}
		set
		{
			m_RequiresDepthTextureOption = value;
		}
	}

	public CameraOverrideOption requiresColorOption
	{
		get
		{
			return m_RequiresOpaqueTextureOption;
		}
		set
		{
			m_RequiresOpaqueTextureOption = value;
		}
	}

	public CameraRenderType renderType
	{
		get
		{
			return m_CameraType;
		}
		set
		{
			m_CameraType = value;
		}
	}

	public List<Camera> cameraStack
	{
		get
		{
			if (renderType != 0)
			{
				Camera component = base.gameObject.GetComponent<Camera>();
				Debug.LogWarning($"{component.name}: This camera is of {renderType} type. Only Base cameras can have a camera stack.");
				return null;
			}
			if (!scriptableRenderer.SupportsCameraStackingType(CameraRenderType.Base))
			{
				Camera component2 = base.gameObject.GetComponent<Camera>();
				Debug.LogWarning($"{component2.name}: This camera has a ScriptableRenderer that doesn't support camera stacking. Camera stack is null.");
				return null;
			}
			return m_Cameras;
		}
	}

	public bool clearDepth => m_ClearDepth;

	public bool requiresDepthTexture
	{
		get
		{
			if (m_RequiresDepthTextureOption == CameraOverrideOption.UsePipelineSettings)
			{
				return UniversalRenderPipeline.asset.supportsCameraDepthTexture;
			}
			return m_RequiresDepthTextureOption == CameraOverrideOption.On;
		}
		set
		{
			m_RequiresDepthTextureOption = (value ? CameraOverrideOption.On : CameraOverrideOption.Off);
		}
	}

	public bool requiresColorTexture
	{
		get
		{
			if (m_RequiresOpaqueTextureOption == CameraOverrideOption.UsePipelineSettings)
			{
				return UniversalRenderPipeline.asset.supportsCameraOpaqueTexture;
			}
			return m_RequiresOpaqueTextureOption == CameraOverrideOption.On;
		}
		set
		{
			m_RequiresOpaqueTextureOption = (value ? CameraOverrideOption.On : CameraOverrideOption.Off);
		}
	}

	public ScriptableRenderer scriptableRenderer
	{
		get
		{
			if ((object)UniversalRenderPipeline.asset == null)
			{
				return null;
			}
			if (!UniversalRenderPipeline.asset.ValidateRendererData(m_RendererIndex))
			{
				int defaultRendererIndex = UniversalRenderPipeline.asset.m_DefaultRendererIndex;
				Debug.LogWarning("Renderer at <b>index " + m_RendererIndex + "</b> is missing for camera <b>" + camera.name + "</b>, falling back to Default Renderer. <b>" + UniversalRenderPipeline.asset.m_RendererDataList[defaultRendererIndex].name + "</b>", UniversalRenderPipeline.asset);
				return UniversalRenderPipeline.asset.GetRenderer(defaultRendererIndex);
			}
			return UniversalRenderPipeline.asset.GetRenderer(m_RendererIndex);
		}
	}

	public LayerMask volumeLayerMask
	{
		get
		{
			return m_VolumeLayerMask;
		}
		set
		{
			m_VolumeLayerMask = value;
		}
	}

	public Transform volumeTrigger
	{
		get
		{
			return m_VolumeTrigger;
		}
		set
		{
			m_VolumeTrigger = value;
		}
	}

	internal VolumeFrameworkUpdateMode volumeFrameworkUpdateMode
	{
		get
		{
			return m_VolumeFrameworkUpdateModeOption;
		}
		set
		{
			m_VolumeFrameworkUpdateModeOption = value;
		}
	}

	public bool requiresVolumeFrameworkUpdate
	{
		get
		{
			if (m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.UsePipelineSettings)
			{
				return UniversalRenderPipeline.asset.volumeFrameworkUpdateMode != VolumeFrameworkUpdateMode.ViaScripting;
			}
			return m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.EveryFrame;
		}
	}

	public VolumeStack volumeStack
	{
		get
		{
			return m_VolumeStack;
		}
		set
		{
			if (value == null && m_VolumeStack != null && m_VolumeStack.isValid)
			{
				if (s_CachedVolumeStacks == null)
				{
					s_CachedVolumeStacks = new List<VolumeStack>(4);
				}
				s_CachedVolumeStacks.Add(m_VolumeStack);
			}
			m_VolumeStack = value;
		}
	}

	public bool renderPostProcessing
	{
		get
		{
			return m_RenderPostProcessing;
		}
		set
		{
			m_RenderPostProcessing = value;
		}
	}

	public AntialiasingMode antialiasing
	{
		get
		{
			return m_Antialiasing;
		}
		set
		{
			m_Antialiasing = value;
		}
	}

	public AntialiasingQuality antialiasingQuality
	{
		get
		{
			return m_AntialiasingQuality;
		}
		set
		{
			m_AntialiasingQuality = value;
		}
	}

	public ref TemporalAA.Settings taaSettings => ref m_TaaSettings;

	public ICameraHistoryReadAccess history => m_History;

	internal UniversalCameraHistory historyManager => m_History;

	internal MotionVectorsPersistentData motionVectorsPersistentData => m_MotionVectorsPersistentData;

	public bool resetHistory
	{
		get
		{
			return m_TaaSettings.resetHistoryFrames != 0;
		}
		set
		{
			m_TaaSettings.resetHistoryFrames += (value ? 1 : 0);
			m_MotionVectorsPersistentData.Reset();
			m_TaaSettings.jitterFrameCountOffset = -Time.frameCount;
		}
	}

	public bool stopNaN
	{
		get
		{
			return m_StopNaN;
		}
		set
		{
			m_StopNaN = value;
		}
	}

	public bool dithering
	{
		get
		{
			return m_Dithering;
		}
		set
		{
			m_Dithering = value;
		}
	}

	public bool allowXRRendering
	{
		get
		{
			return m_AllowXRRendering;
		}
		set
		{
			m_AllowXRRendering = value;
		}
	}

	public bool useScreenCoordOverride
	{
		get
		{
			return m_UseScreenCoordOverride;
		}
		set
		{
			m_UseScreenCoordOverride = value;
		}
	}

	public Vector4 screenSizeOverride
	{
		get
		{
			return m_ScreenSizeOverride;
		}
		set
		{
			m_ScreenSizeOverride = value;
		}
	}

	public Vector4 screenCoordScaleBias
	{
		get
		{
			return m_ScreenCoordScaleBias;
		}
		set
		{
			m_ScreenCoordScaleBias = value;
		}
	}

	public bool allowHDROutput
	{
		get
		{
			return m_AllowHDROutput;
		}
		set
		{
			m_AllowHDROutput = value;
		}
	}

	[Obsolete("This field has been deprecated. #from(6000.2)", false)]
	public float version => (float)m_Version;

	private void Start()
	{
		if (m_CameraType == CameraRenderType.Overlay)
		{
			camera.clearFlags = CameraClearFlags.Nothing;
		}
	}

	internal void UpdateCameraStack()
	{
		int count = m_Cameras.Count;
		m_Cameras.RemoveAll((Camera cam) => cam == null);
		int count2 = m_Cameras.Count;
		int num = count - count2;
		if (num != 0)
		{
			Debug.LogWarning(base.name + ": " + num + " camera overlay" + ((num > 1) ? "s" : "") + " no longer exists and will be removed from the camera stack.");
		}
	}

	public void SetRenderer(int index)
	{
		m_RendererIndex = index;
	}

	internal void GetOrCreateVolumeStack()
	{
		if (s_CachedVolumeStacks != null && s_CachedVolumeStacks.Count > 0)
		{
			int index = s_CachedVolumeStacks.Count - 1;
			VolumeStack volumeStack = s_CachedVolumeStacks[index];
			s_CachedVolumeStacks.RemoveAt(index);
			if (volumeStack.isValid)
			{
				this.volumeStack = volumeStack;
			}
		}
		if (this.volumeStack == null)
		{
			this.volumeStack = VolumeManager.instance.CreateStack();
		}
	}

	public void OnValidate()
	{
		if (m_CameraType == CameraRenderType.Overlay && m_Camera != null)
		{
			m_Camera.clearFlags = CameraClearFlags.Nothing;
		}
	}

	public void OnDrawGizmos()
	{
		string value = "";
		Color white = Color.white;
		if (m_CameraType == CameraRenderType.Base)
		{
			value = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";
		}
		else if (m_CameraType == CameraRenderType.Overlay)
		{
			value = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";
		}
		if (!string.IsNullOrEmpty(value))
		{
			Gizmos.DrawIcon(base.transform.position, value, allowScaling: true, white);
		}
		if (renderPostProcessing)
		{
			Gizmos.DrawIcon(base.transform.position, "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_PostProcessing.png", allowScaling: true, white);
		}
	}

	public void OnDestroy()
	{
		m_Camera.DestroyVolumeStack(this);
		if (camera.cameraType != CameraType.SceneView)
		{
			GetRawRenderer()?.ReleaseRenderTargets();
		}
		m_History?.Dispose();
		m_History = null;
	}

	private ScriptableRenderer GetRawRenderer()
	{
		if ((object)UniversalRenderPipeline.asset == null)
		{
			return null;
		}
		ReadOnlySpan<ScriptableRenderer> renderers = UniversalRenderPipeline.asset.renderers;
		if (renderers == null || renderers.IsEmpty)
		{
			return null;
		}
		if (m_RendererIndex >= renderers.Length || m_RendererIndex < 0)
		{
			return null;
		}
		return renderers[m_RendererIndex];
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (m_Version == Version.Count)
		{
			m_Version = Version.DepthAndOpaqueTextureOptions;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_Version == Version.Count)
		{
			m_Version = Version.Initial;
		}
		if (m_Version < Version.DepthAndOpaqueTextureOptions)
		{
			m_RequiresDepthTextureOption = (m_RequiresDepthTexture ? CameraOverrideOption.On : CameraOverrideOption.Off);
			m_RequiresOpaqueTextureOption = (m_RequiresColorTexture ? CameraOverrideOption.On : CameraOverrideOption.Off);
			m_Version = Version.DepthAndOpaqueTextureOptions;
		}
	}
}
