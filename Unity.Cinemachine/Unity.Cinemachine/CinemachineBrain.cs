using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[AddComponentMenu("Cinemachine/Cinemachine Brain")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineBrain.html")]
public class CinemachineBrain : MonoBehaviour, ICameraOverrideStack, ICinemachineMixer, ICinemachineCamera
{
	public enum UpdateMethods
	{
		FixedUpdate,
		LateUpdate,
		SmartUpdate,
		ManualUpdate
	}

	public enum BrainUpdateMethods
	{
		FixedUpdate,
		LateUpdate
	}

	[Serializable]
	public struct LensModeOverrideSettings
	{
		[Tooltip("If set, enables CinemachineCameras to override the lens mode of the camera")]
		public bool Enabled;

		[Tooltip("Lens mode to use when no mode override is active")]
		public LensSettings.OverrideModes DefaultMode;
	}

	[Tooltip("When enabled, the current camera and blend are indicated in the game window, for debugging")]
	[FormerlySerializedAs("m_ShowDebugText")]
	public bool ShowDebugText;

	[Tooltip("When enabled, shows the camera's frustum at all times in the Scene view")]
	[FormerlySerializedAs("m_ShowCameraFrustum")]
	public bool ShowCameraFrustum = true;

	[Tooltip("When enabled, the cameras always respond in real-time to user input and damping, even if the game is running in slow motion")]
	[FormerlySerializedAs("m_IgnoreTimeScale")]
	public bool IgnoreTimeScale;

	[Tooltip("If set, this GameObject's Y axis defines the world-space Up vector for all the CinemachineCameras.  This is useful for instance in top-down game environments.  If not set, Up is world-space Y.  Setting this appropriately is important, because CinemachineCameras don't like looking straight up or straight down.")]
	[FormerlySerializedAs("m_WorldUpOverride")]
	public Transform WorldUpOverride;

	[Tooltip("The CinemachineBrain finds the highest-priority CinemachineCamera that outputs to any of the channels selected.  CinemachineCameras that do not output to one of these channels are ignored.  Use this in situations where multiple CinemachineBrains are needed (for example, Split-screen).")]
	public OutputChannels ChannelMask = (OutputChannels)(-1);

	[Tooltip("The update time for the CinemachineCameras.  Use FixedUpdate if all your targets are animated during FixedUpdate (e.g. RigidBodies), LateUpdate if all your targets are animated during the normal Update loop, and SmartUpdate if you want Cinemachine to do the appropriate thing on a per-target basis.  SmartUpdate is the recommended setting")]
	[FormerlySerializedAs("m_UpdateMethod")]
	public UpdateMethods UpdateMethod = UpdateMethods.SmartUpdate;

	[Tooltip("The update time for the Brain, i.e. when the blends are evaluated and the brain's transform is updated")]
	[FormerlySerializedAs("m_BlendUpdateMethod")]
	public BrainUpdateMethods BlendUpdateMethod = BrainUpdateMethods.LateUpdate;

	[FoldoutWithEnabledButton("Enabled")]
	public LensModeOverrideSettings LensModeOverride = new LensModeOverrideSettings
	{
		DefaultMode = LensSettings.OverrideModes.Perspective
	};

	[Tooltip("The blend that is used in cases where you haven't explicitly defined a blend between two CinemachineCameras")]
	[FormerlySerializedAs("m_DefaultBlend")]
	public CinemachineBlendDefinition DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2f);

	[Tooltip("This is the asset that contains custom settings for blends between specific CinemachineCameras in your Scene")]
	[FormerlySerializedAs("m_CustomBlends")]
	[EmbeddedBlenderSettingsProperty]
	public CinemachineBlenderSettings CustomBlends;

	private Camera m_OutputCamera;

	private GameObject m_TargetOverride;

	private int m_LastFrameUpdated;

	private Coroutine m_PhysicsCoroutine;

	private readonly WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

	private readonly BlendManager m_BlendManager = new BlendManager();

	private static readonly List<CinemachineBrain> s_ActiveBrains = new List<CinemachineBrain>();

	private CameraState m_CameraState;

	public Vector3 DefaultWorldUp
	{
		get
		{
			if (!(WorldUpOverride != null))
			{
				return Vector3.up;
			}
			return WorldUpOverride.transform.up;
		}
	}

	public string Name => base.name;

	public string Description
	{
		get
		{
			if (ActiveVirtualCamera == null)
			{
				return "(none)";
			}
			if (IsBlending)
			{
				return ActiveBlend.Description;
			}
			StringBuilder stringBuilder = CinemachineDebug.SBFromPool();
			stringBuilder.Append(ActiveVirtualCamera.Name);
			stringBuilder.Append(" ");
			stringBuilder.Append(ActiveVirtualCamera.Description);
			string result = stringBuilder.ToString();
			CinemachineDebug.ReturnToPool(stringBuilder);
			return result;
		}
	}

	public CameraState State => m_CameraState;

	public bool IsValid => this != null;

	public ICinemachineMixer ParentCamera => null;

	public static int ActiveBrainCount => s_ActiveBrains.Count;

	public GameObject ControlledObject
	{
		get
		{
			if (!(m_TargetOverride == null))
			{
				return m_TargetOverride;
			}
			return base.gameObject;
		}
		set
		{
			if ((object)m_TargetOverride != value)
			{
				m_TargetOverride = value;
				ControlledObject.TryGetComponent<Camera>(out m_OutputCamera);
			}
		}
	}

	public Camera OutputCamera
	{
		get
		{
			if (m_OutputCamera == null && !Application.isPlaying)
			{
				ControlledObject.TryGetComponent<Camera>(out m_OutputCamera);
			}
			return m_OutputCamera;
		}
	}

	public ICinemachineCamera ActiveVirtualCamera => CinemachineCore.SoloCamera ?? m_BlendManager.ActiveVirtualCamera;

	public bool IsBlending => m_BlendManager.IsBlending;

	public CinemachineBlend ActiveBlend
	{
		get
		{
			return m_BlendManager.ActiveBlend;
		}
		set
		{
			m_BlendManager.ActiveBlend = value;
		}
	}

	private void OnValidate()
	{
		DefaultBlend.Time = Mathf.Max(0f, DefaultBlend.Time);
	}

	private void Reset()
	{
		DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2f);
		CustomBlends = null;
		ShowDebugText = false;
		ShowCameraFrustum = true;
		IgnoreTimeScale = false;
		WorldUpOverride = null;
		ChannelMask = (OutputChannels)(-1);
		UpdateMethod = UpdateMethods.SmartUpdate;
		BlendUpdateMethod = BrainUpdateMethods.LateUpdate;
		LensModeOverride = new LensModeOverrideSettings
		{
			DefaultMode = LensSettings.OverrideModes.Perspective
		};
	}

	private void Awake()
	{
		ControlledObject.TryGetComponent<Camera>(out m_OutputCamera);
	}

	private void Start()
	{
		m_LastFrameUpdated = -1;
		UpdateVirtualCameras(CameraUpdateManager.UpdateFilter.Late, -1f);
	}

	private void OnEnable()
	{
		m_BlendManager.OnEnable();
		m_BlendManager.LookupBlendDelegate = LookupBlend;
		s_ActiveBrains.Add(this);
		m_PhysicsCoroutine = StartCoroutine(AfterPhysics());
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
		s_ActiveBrains.Remove(this);
		m_BlendManager.OnDisable();
		StopCoroutine(m_PhysicsCoroutine);
		UpdateTracker.ForgetContext(this);
		CameraUpdateManager.ForgetContext(this);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (Time.frameCount == m_LastFrameUpdated && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
		{
			DoNonFixedUpdate(Time.frameCount);
		}
	}

	private void OnSceneUnloaded(Scene scene)
	{
		if (Time.frameCount == m_LastFrameUpdated && m_BlendManager.IsInitialized && UpdateMethod != UpdateMethods.ManualUpdate)
		{
			DoNonFixedUpdate(Time.frameCount);
		}
	}

	private void LateUpdate()
	{
		if (UpdateMethod != UpdateMethods.ManualUpdate)
		{
			DoNonFixedUpdate(Time.frameCount);
		}
	}

	private IEnumerator AfterPhysics()
	{
		while (true)
		{
			yield return m_WaitForFixedUpdate;
			DoFixedUpdate();
		}
	}

	public int SetCameraOverride(int overrideId, int priority, ICinemachineCamera camA, ICinemachineCamera camB, float weightB, float deltaTime)
	{
		return m_BlendManager.SetCameraOverride(overrideId, priority, camA, camB, weightB, deltaTime);
	}

	public void ReleaseCameraOverride(int overrideId)
	{
		m_BlendManager.ReleaseCameraOverride(overrideId);
	}

	public void UpdateCameraState(Vector3 up, float deltaTime)
	{
	}

	public void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
	}

	public bool IsLiveChild(ICinemachineCamera cam, bool dominantChildOnly = false)
	{
		if (CinemachineCore.SoloCamera == cam || m_BlendManager.IsLive(cam))
		{
			return true;
		}
		ICinemachineMixer parentCamera = cam.ParentCamera;
		if (parentCamera != null && parentCamera.IsLiveChild(cam, dominantChildOnly))
		{
			return IsLiveChild(parentCamera, dominantChildOnly);
		}
		return false;
	}

	public static CinemachineBrain GetActiveBrain(int index)
	{
		return s_ActiveBrains[index];
	}

	public void ResetState()
	{
		m_BlendManager.ResetRootFrame();
	}

	public bool IsValidChannel(CinemachineVirtualCameraBase vcam)
	{
		if (vcam != null)
		{
			return (vcam.OutputChannel & ChannelMask) != 0;
		}
		return false;
	}

	public bool IsLiveInBlend(ICinemachineCamera cam)
	{
		if (m_BlendManager.IsLiveInBlend(cam))
		{
			return true;
		}
		ICinemachineMixer parentCamera = cam.ParentCamera;
		if (parentCamera != null && parentCamera.IsLiveChild(cam))
		{
			return IsLiveInBlend(parentCamera);
		}
		return false;
	}

	public void ManualUpdate(int currentFrame, float deltaTime)
	{
		float uniformDeltaTimeOverride = CinemachineCore.UniformDeltaTimeOverride;
		CinemachineCore.UniformDeltaTimeOverride = deltaTime;
		DoNonFixedUpdate(currentFrame);
		CinemachineCore.UniformDeltaTimeOverride = uniformDeltaTimeOverride;
	}

	public void ManualUpdate()
	{
		DoNonFixedUpdate(Time.frameCount);
	}

	private void DoNonFixedUpdate(int updateFrame)
	{
		m_LastFrameUpdated = (CinemachineCore.CurrentUpdateFrame = updateFrame);
		float effectiveDeltaTime = GetEffectiveDeltaTime(fixedDelta: false);
		if (Application.isPlaying && (UpdateMethod == UpdateMethods.FixedUpdate || Time.inFixedTimeStep))
		{
			CameraUpdateManager.s_CurrentUpdateFilter = CameraUpdateManager.UpdateFilter.Fixed;
			if (BlendUpdateMethod != 0 && CinemachineCore.SoloCamera == null)
			{
				m_BlendManager.RefreshCurrentCameraState(DefaultWorldUp, GetEffectiveDeltaTime(fixedDelta: true));
			}
		}
		else
		{
			CameraUpdateManager.UpdateFilter updateFilter = CameraUpdateManager.UpdateFilter.Late;
			if (UpdateMethod == UpdateMethods.SmartUpdate)
			{
				UpdateTracker.OnUpdate(UpdateTracker.UpdateClock.Late, this);
				updateFilter = CameraUpdateManager.UpdateFilter.SmartLate;
			}
			UpdateVirtualCameras(updateFilter, effectiveDeltaTime);
		}
		if (!Application.isPlaying || BlendUpdateMethod != 0)
		{
			m_BlendManager.UpdateRootFrame(this, TopCameraFromPriorityQueue(), DefaultWorldUp, effectiveDeltaTime);
		}
		m_BlendManager.ComputeCurrentBlend();
		if (!Application.isPlaying || BlendUpdateMethod != 0)
		{
			ProcessActiveCamera(effectiveDeltaTime);
		}
	}

	private void DoFixedUpdate()
	{
		if (UpdateMethod == UpdateMethods.FixedUpdate || UpdateMethod == UpdateMethods.SmartUpdate)
		{
			CameraUpdateManager.UpdateFilter updateFilter = CameraUpdateManager.UpdateFilter.Fixed;
			if (UpdateMethod == UpdateMethods.SmartUpdate)
			{
				UpdateTracker.OnUpdate(UpdateTracker.UpdateClock.Fixed, this);
				updateFilter = CameraUpdateManager.UpdateFilter.SmartFixed;
			}
			UpdateVirtualCameras(updateFilter, GetEffectiveDeltaTime(fixedDelta: true));
		}
		if (BlendUpdateMethod == BrainUpdateMethods.FixedUpdate)
		{
			m_BlendManager.UpdateRootFrame(this, TopCameraFromPriorityQueue(), DefaultWorldUp, Time.fixedDeltaTime);
			ProcessActiveCamera(Time.fixedDeltaTime);
		}
	}

	private float GetEffectiveDeltaTime(bool fixedDelta)
	{
		if (CinemachineCore.UniformDeltaTimeOverride >= 0f)
		{
			return CinemachineCore.UniformDeltaTimeOverride;
		}
		if (CinemachineCore.SoloCamera != null)
		{
			return Time.unscaledDeltaTime;
		}
		if (!Application.isPlaying)
		{
			return m_BlendManager.GetDeltaTimeOverride();
		}
		if (IgnoreTimeScale)
		{
			if (!fixedDelta)
			{
				return Time.unscaledDeltaTime;
			}
			return Time.fixedDeltaTime;
		}
		if (!fixedDelta)
		{
			return Time.deltaTime;
		}
		return Time.fixedDeltaTime;
	}

	private void UpdateVirtualCameras(CameraUpdateManager.UpdateFilter updateFilter, float deltaTime)
	{
		CameraUpdateManager.s_CurrentUpdateFilter = updateFilter;
		CameraUpdateManager.UpdateAllActiveVirtualCameras((uint)ChannelMask, DefaultWorldUp, deltaTime, this);
		if (CinemachineCore.SoloCamera != null)
		{
			CinemachineCore.SoloCamera.UpdateCameraState(DefaultWorldUp, deltaTime);
		}
		m_BlendManager.RefreshCurrentCameraState(DefaultWorldUp, deltaTime);
		updateFilter = CameraUpdateManager.UpdateFilter.Late;
		if (Application.isPlaying)
		{
			if (UpdateMethod == UpdateMethods.SmartUpdate)
			{
				updateFilter |= CameraUpdateManager.UpdateFilter.Smart;
			}
			else if (UpdateMethod == UpdateMethods.FixedUpdate)
			{
				updateFilter = CameraUpdateManager.UpdateFilter.Fixed;
			}
		}
		CameraUpdateManager.s_CurrentUpdateFilter = updateFilter;
	}

	protected virtual ICinemachineCamera TopCameraFromPriorityQueue()
	{
		int virtualCameraCount = CameraUpdateManager.VirtualCameraCount;
		for (int i = 0; i < virtualCameraCount; i++)
		{
			CinemachineVirtualCameraBase virtualCamera = CameraUpdateManager.GetVirtualCamera(i);
			if (IsValidChannel(virtualCamera))
			{
				return virtualCamera;
			}
		}
		return null;
	}

	private CinemachineBlendDefinition LookupBlend(ICinemachineCamera fromKey, ICinemachineCamera toKey)
	{
		return CinemachineBlenderSettings.LookupBlend(fromKey, toKey, DefaultBlend, CustomBlends, this);
	}

	private void ProcessActiveCamera(float deltaTime)
	{
		if (CinemachineCore.SoloCamera != null)
		{
			CameraState state = CinemachineCore.SoloCamera.State;
			PushStateToUnityCamera(ref state);
			return;
		}
		if (m_BlendManager.ProcessActiveCamera(this, DefaultWorldUp, deltaTime) != null)
		{
			CameraState state2 = m_BlendManager.CameraState;
			PushStateToUnityCamera(ref state2);
			return;
		}
		CameraState state3 = CameraState.Default;
		Transform transform = ControlledObject.transform;
		state3.RawPosition = transform.position;
		state3.RawOrientation = transform.rotation;
		state3.Lens = LensSettings.FromCamera(m_OutputCamera);
		state3.BlendHint |= (CameraState.BlendHints)458752;
		PushStateToUnityCamera(ref state3);
	}

	private void PushStateToUnityCamera(ref CameraState state)
	{
		m_CameraState = state;
		Transform obj = ControlledObject.transform;
		Vector3 pos = obj.position;
		Quaternion rot = obj.rotation;
		if ((state.BlendHint & CameraState.BlendHints.NoPosition) == 0)
		{
			pos = state.GetFinalPosition();
		}
		if ((state.BlendHint & CameraState.BlendHints.NoOrientation) == 0)
		{
			rot = state.GetFinalOrientation();
		}
		obj.ConservativeSetPositionAndRotation(pos, rot);
		if ((state.BlendHint & CameraState.BlendHints.NoLens) == 0)
		{
			Camera outputCamera = OutputCamera;
			if (outputCamera != null)
			{
				bool flag = outputCamera.usePhysicalProperties;
				outputCamera.nearClipPlane = state.Lens.NearClipPlane;
				outputCamera.farClipPlane = state.Lens.FarClipPlane;
				outputCamera.orthographicSize = state.Lens.OrthographicSize;
				outputCamera.fieldOfView = state.Lens.FieldOfView;
				if (LensModeOverride.Enabled)
				{
					if (state.Lens.ModeOverride != 0)
					{
						flag = state.Lens.IsPhysicalCamera;
						outputCamera.orthographic = state.Lens.ModeOverride == LensSettings.OverrideModes.Orthographic;
					}
					else if (LensModeOverride.DefaultMode != 0)
					{
						flag = LensModeOverride.DefaultMode == LensSettings.OverrideModes.Physical;
						outputCamera.orthographic = LensModeOverride.DefaultMode == LensSettings.OverrideModes.Orthographic;
					}
					outputCamera.usePhysicalProperties = flag;
				}
				if (flag)
				{
					outputCamera.sensorSize = state.Lens.PhysicalProperties.SensorSize;
					outputCamera.gateFit = state.Lens.PhysicalProperties.GateFit;
					outputCamera.focalLength = Camera.FieldOfViewToFocalLength(state.Lens.FieldOfView, state.Lens.PhysicalProperties.SensorSize.y);
					outputCamera.lensShift = state.Lens.PhysicalProperties.LensShift;
					outputCamera.focusDistance = state.Lens.PhysicalProperties.FocusDistance;
					outputCamera.iso = state.Lens.PhysicalProperties.Iso;
					outputCamera.shutterSpeed = state.Lens.PhysicalProperties.ShutterSpeed;
					outputCamera.aperture = state.Lens.PhysicalProperties.Aperture;
					outputCamera.bladeCount = state.Lens.PhysicalProperties.BladeCount;
					outputCamera.curvature = state.Lens.PhysicalProperties.Curvature;
					outputCamera.barrelClipping = state.Lens.PhysicalProperties.BarrelClipping;
					outputCamera.anamorphism = state.Lens.PhysicalProperties.Anamorphism;
				}
			}
		}
		CinemachineCore.CameraUpdatedEvent.Invoke(this);
	}
}
