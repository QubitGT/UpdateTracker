using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.U2D;

namespace UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/2D/Pixel Perfect Camera")]
[RequireComponent(typeof(Camera))]
[MovedFrom(true, "UnityEngine.Experimental.Rendering.Universal", null, null)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/2d-pixelperfect.html%23properties")]
public class PixelPerfectCamera : MonoBehaviour, IPixelPerfectCamera, ISerializationCallbackReceiver
{
	public enum CropFrame
	{
		None,
		Pillarbox,
		Letterbox,
		Windowbox,
		StretchFill
	}

	public enum GridSnapping
	{
		None,
		PixelSnapping,
		UpscaleRenderTexture
	}

	public enum PixelPerfectFilterMode
	{
		RetroAA,
		Point
	}

	private enum ComponentVersions
	{
		Version_Unserialized,
		Version_1
	}

	[SerializeField]
	private int m_AssetsPPU = 100;

	[SerializeField]
	private int m_RefResolutionX = 320;

	[SerializeField]
	private int m_RefResolutionY = 180;

	[SerializeField]
	private CropFrame m_CropFrame;

	[SerializeField]
	private GridSnapping m_GridSnapping;

	[SerializeField]
	private PixelPerfectFilterMode m_FilterMode;

	private Camera m_Camera;

	private PixelPerfectCameraInternal m_Internal;

	private bool m_CinemachineCompatibilityMode;

	public CropFrame cropFrame
	{
		get
		{
			return m_CropFrame;
		}
		set
		{
			m_CropFrame = value;
		}
	}

	public GridSnapping gridSnapping
	{
		get
		{
			return m_GridSnapping;
		}
		set
		{
			m_GridSnapping = value;
		}
	}

	public float orthographicSize => m_Internal.orthoSize;

	public int assetsPPU
	{
		get
		{
			return m_AssetsPPU;
		}
		set
		{
			m_AssetsPPU = ((value <= 0) ? 1 : value);
		}
	}

	public int refResolutionX
	{
		get
		{
			return m_RefResolutionX;
		}
		set
		{
			m_RefResolutionX = ((value <= 0) ? 1 : value);
		}
	}

	public int refResolutionY
	{
		get
		{
			return m_RefResolutionY;
		}
		set
		{
			m_RefResolutionY = ((value <= 0) ? 1 : value);
		}
	}

	[Obsolete("Use gridSnapping instead", false)]
	public bool upscaleRT
	{
		get
		{
			return m_GridSnapping == GridSnapping.UpscaleRenderTexture;
		}
		set
		{
			m_GridSnapping = (value ? GridSnapping.UpscaleRenderTexture : GridSnapping.None);
		}
	}

	[Obsolete("Use gridSnapping instead", false)]
	public bool pixelSnapping
	{
		get
		{
			return m_GridSnapping == GridSnapping.PixelSnapping;
		}
		set
		{
			m_GridSnapping = (value ? GridSnapping.PixelSnapping : GridSnapping.None);
		}
	}

	[Obsolete("Use cropFrame instead", false)]
	public bool cropFrameX
	{
		get
		{
			if (m_CropFrame != CropFrame.StretchFill && m_CropFrame != CropFrame.Windowbox)
			{
				return m_CropFrame == CropFrame.Pillarbox;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (m_CropFrame == CropFrame.None)
				{
					m_CropFrame = CropFrame.Pillarbox;
				}
				else if (m_CropFrame == CropFrame.Letterbox)
				{
					m_CropFrame = CropFrame.Windowbox;
				}
			}
			else if (m_CropFrame == CropFrame.Pillarbox)
			{
				m_CropFrame = CropFrame.None;
			}
			else if (m_CropFrame == CropFrame.Windowbox || m_CropFrame == CropFrame.StretchFill)
			{
				m_CropFrame = CropFrame.Letterbox;
			}
		}
	}

	[Obsolete("Use cropFrame instead", false)]
	public bool cropFrameY
	{
		get
		{
			if (m_CropFrame != CropFrame.StretchFill && m_CropFrame != CropFrame.Windowbox)
			{
				return m_CropFrame == CropFrame.Letterbox;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (m_CropFrame == CropFrame.None)
				{
					m_CropFrame = CropFrame.Letterbox;
				}
				else if (m_CropFrame == CropFrame.Pillarbox)
				{
					m_CropFrame = CropFrame.Windowbox;
				}
			}
			else if (m_CropFrame == CropFrame.Letterbox)
			{
				m_CropFrame = CropFrame.None;
			}
			else if (m_CropFrame == CropFrame.Windowbox || m_CropFrame == CropFrame.StretchFill)
			{
				m_CropFrame = CropFrame.Pillarbox;
			}
		}
	}

	[Obsolete("Use cropFrame instead", false)]
	public bool stretchFill
	{
		get
		{
			return m_CropFrame == CropFrame.StretchFill;
		}
		set
		{
			if (value)
			{
				m_CropFrame = CropFrame.StretchFill;
			}
			else
			{
				m_CropFrame = CropFrame.Windowbox;
			}
		}
	}

	public int pixelRatio
	{
		get
		{
			if (m_CinemachineCompatibilityMode)
			{
				if (m_GridSnapping == GridSnapping.UpscaleRenderTexture)
				{
					return m_Internal.zoom * m_Internal.cinemachineVCamZoom;
				}
				return m_Internal.cinemachineVCamZoom;
			}
			return m_Internal.zoom;
		}
	}

	public bool requiresUpscalePass => m_Internal.requiresUpscaling;

	internal FilterMode finalBlitFilterMode
	{
		get
		{
			if (m_FilterMode != 0)
			{
				return FilterMode.Point;
			}
			return FilterMode.Bilinear;
		}
	}

	internal Vector2Int offscreenRTSize => new Vector2Int(m_Internal.offscreenRTWidth, m_Internal.offscreenRTHeight);

	private Vector2Int cameraRTSize
	{
		get
		{
			RenderTexture targetTexture = m_Camera.targetTexture;
			if (!(targetTexture == null))
			{
				return new Vector2Int(targetTexture.width, targetTexture.height);
			}
			return new Vector2Int(Screen.width, Screen.height);
		}
	}

	public Vector3 RoundToPixel(Vector3 position)
	{
		float unitsPerPixel = m_Internal.unitsPerPixel;
		if (unitsPerPixel == 0f)
		{
			return position;
		}
		Vector3 result = default(Vector3);
		result.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
		result.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;
		result.z = Mathf.Round(position.z / unitsPerPixel) * unitsPerPixel;
		return result;
	}

	public float CorrectCinemachineOrthoSize(float targetOrthoSize)
	{
		m_CinemachineCompatibilityMode = true;
		if (m_Internal == null)
		{
			return targetOrthoSize;
		}
		return m_Internal.CorrectCinemachineOrthoSize(targetOrthoSize);
	}

	private void PixelSnap()
	{
		Vector3 position = m_Camera.transform.position;
		Vector3 vector = RoundToPixel(position) - position;
		vector.z = 0f - vector.z;
		Matrix4x4 inverse = Matrix4x4.TRS(position + vector, Quaternion.identity, Vector3.one).inverse;
		Matrix4x4 inverse2 = Matrix4x4.Rotate(m_Camera.transform.rotation).inverse;
		Matrix4x4 matrix4x = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
		m_Camera.worldToCameraMatrix = matrix4x * inverse2 * inverse;
	}

	private void Awake()
	{
		m_Camera = GetComponent<Camera>();
		m_Internal = new PixelPerfectCameraInternal(this);
		UpdateCameraProperties();
	}

	private void UpdateCameraProperties()
	{
		Vector2Int vector2Int = cameraRTSize;
		m_Internal.CalculateCameraProperties(vector2Int.x, vector2Int.y);
		if (m_Internal.useOffscreenRT)
		{
			m_Camera.pixelRect = m_Internal.CalculateFinalBlitPixelRect(vector2Int.x, vector2Int.y);
		}
		else
		{
			m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
		}
	}

	private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera == m_Camera)
		{
			UpdateCameraProperties();
			PixelSnap();
			if (!m_CinemachineCompatibilityMode)
			{
				m_Camera.orthographicSize = m_Internal.orthoSize;
			}
			PixelPerfectRendering.pixelSnapSpacing = m_Internal.unitsPerPixel;
		}
	}

	private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera == m_Camera)
		{
			PixelPerfectRendering.pixelSnapSpacing = 0f;
		}
	}

	private void OnEnable()
	{
		m_CinemachineCompatibilityMode = false;
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
		RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
	}

	internal void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
		RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
		m_Camera.rect = new Rect(0f, 0f, 1f, 1f);
		m_Camera.ResetWorldToCameraMatrix();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
