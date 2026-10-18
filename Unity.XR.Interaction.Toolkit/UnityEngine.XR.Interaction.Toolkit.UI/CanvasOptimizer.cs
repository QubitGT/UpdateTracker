using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("Event/Canvas Optimizer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.CanvasOptimizer.html")]
public class CanvasOptimizer : MonoBehaviour
{
	private class CanvasState
	{
		private class CanvasSettings
		{
			private AdditionalCanvasShaderChannels m_AdditionalShaderChannels;

			private float m_NormalizedSortingGridSize;

			private bool m_OverridePixelPerfect;

			private bool m_OverrideSorting;

			private float m_PlaneDistance;

			private float m_ReferencePixelsPerUnit;

			private RenderMode m_RenderMode;

			private float m_ScaleFactor;

			private int m_SortingLayerID;

			private string m_SortingLayerName;

			private int m_SortingOrder;

			private int m_TargetDisplay;

			public bool present { get; set; }

			public void CopyFrom(Canvas source)
			{
				m_AdditionalShaderChannels = source.additionalShaderChannels;
				m_NormalizedSortingGridSize = source.normalizedSortingGridSize;
				m_OverridePixelPerfect = source.overridePixelPerfect;
				m_OverrideSorting = source.overrideSorting;
				m_PlaneDistance = source.planeDistance;
				m_ReferencePixelsPerUnit = source.referencePixelsPerUnit;
				m_RenderMode = source.renderMode;
				m_ScaleFactor = source.scaleFactor;
				m_SortingLayerID = source.sortingLayerID;
				m_SortingLayerName = source.sortingLayerName;
				m_SortingOrder = source.sortingOrder;
				m_TargetDisplay = source.targetDisplay;
			}

			public void CopyTo(Canvas dest)
			{
				dest.additionalShaderChannels = m_AdditionalShaderChannels;
				dest.normalizedSortingGridSize = m_NormalizedSortingGridSize;
				dest.overridePixelPerfect = m_OverridePixelPerfect;
				dest.overrideSorting = m_OverrideSorting;
				dest.planeDistance = m_PlaneDistance;
				dest.referencePixelsPerUnit = m_ReferencePixelsPerUnit;
				dest.renderMode = m_RenderMode;
				dest.scaleFactor = m_ScaleFactor;
				dest.sortingLayerID = m_SortingLayerID;
				dest.sortingLayerName = m_SortingLayerName;
				dest.sortingOrder = m_SortingOrder;
				dest.targetDisplay = m_TargetDisplay;
			}
		}

		private class CanvasScalerSettings
		{
			private float m_DefaultSpriteDPI;

			private float m_DynamicPixelsPerUnit;

			private float m_FallbackScreenDPI;

			private float m_MatchWidthOrHeight;

			private CanvasScaler.Unit m_PhysicalUnit;

			private float m_ReferencePixelsPerUnit;

			private Vector2 m_ReferenceResolution;

			private float m_ScaleFactor;

			private CanvasScaler.ScreenMatchMode m_ScreenMatchMode;

			private CanvasScaler.ScaleMode m_UiScaleMode;

			public bool present { get; set; }

			public void CopyFrom(CanvasScaler source)
			{
				m_DefaultSpriteDPI = source.defaultSpriteDPI;
				m_DynamicPixelsPerUnit = source.dynamicPixelsPerUnit;
				m_FallbackScreenDPI = source.fallbackScreenDPI;
				m_MatchWidthOrHeight = source.matchWidthOrHeight;
				m_PhysicalUnit = source.physicalUnit;
				m_ReferencePixelsPerUnit = source.referencePixelsPerUnit;
				m_ReferenceResolution = source.referenceResolution;
				m_ScaleFactor = source.scaleFactor;
				m_ScreenMatchMode = source.screenMatchMode;
				m_UiScaleMode = source.uiScaleMode;
			}

			public void CopyTo(CanvasScaler dest)
			{
				dest.defaultSpriteDPI = m_DefaultSpriteDPI;
				dest.dynamicPixelsPerUnit = m_DynamicPixelsPerUnit;
				dest.fallbackScreenDPI = m_FallbackScreenDPI;
				dest.matchWidthOrHeight = m_MatchWidthOrHeight;
				dest.physicalUnit = m_PhysicalUnit;
				dest.referencePixelsPerUnit = m_ReferencePixelsPerUnit;
				dest.referenceResolution = m_ReferenceResolution;
				dest.scaleFactor = m_ScaleFactor;
				dest.screenMatchMode = m_ScreenMatchMode;
				dest.uiScaleMode = m_UiScaleMode;
			}
		}

		private class GraphicRaycasterSettings
		{
			private LayerMask m_BlockingMask;

			private GraphicRaycaster.BlockingObjects m_BlockingObjects;

			private bool m_IgnoreReversedGraphics;

			public bool present { get; set; }

			public void CopyFrom(GraphicRaycaster source)
			{
				m_BlockingMask = source.blockingMask;
				m_BlockingObjects = source.blockingObjects;
				m_IgnoreReversedGraphics = source.ignoreReversedGraphics;
			}

			public void CopyTo(GraphicRaycaster dest)
			{
				dest.blockingMask = m_BlockingMask;
				dest.blockingObjects = m_BlockingObjects;
				dest.ignoreReversedGraphics = m_IgnoreReversedGraphics;
			}
		}

		private const float k_CanvasCheckInterval = 0.5f;

		private CanvasTracker m_Tracker;

		private readonly CanvasSettings m_CanvasSettings = new CanvasSettings();

		private readonly CanvasScalerSettings m_CanvasScalerSettings = new CanvasScalerSettings();

		private readonly GraphicRaycasterSettings m_GraphicRaycasterSettings = new GraphicRaycasterSettings();

		private bool m_WasNested;

		private bool m_Nested;

		private bool m_RaysDisabled;

		private Canvas m_Canvas;

		private GraphicRaycaster m_Raycaster;

		private TrackedDeviceGraphicRaycaster m_TrackedDeviceGraphicRaycaster;

		private float m_CheckTimer;

		internal void Initialize(CanvasTracker tracker)
		{
			m_Tracker = tracker;
			GameObject gameObject = m_Tracker.gameObject;
			gameObject.TryGetComponent<Canvas>(out m_Canvas);
			gameObject.TryGetComponent<GraphicRaycaster>(out m_Raycaster);
			CheckForNestedChanges(force: true);
		}

		internal void CheckForNestedChanges(bool force = false)
		{
			if (!m_Tracker.transformDirty && !force)
			{
				return;
			}
			m_Tracker.transformDirty = false;
			Transform transform = m_Tracker.transform;
			Transform parent = transform.parent;
			Canvas canvas = ((parent != null) ? parent.GetComponentInParent<Canvas>() : null);
			m_Nested = canvas != null;
			if (m_Nested && (!m_WasNested || force))
			{
				if (transform.TryGetComponent<CanvasScaler>(out var component))
				{
					m_CanvasScalerSettings.present = true;
					m_CanvasScalerSettings.CopyFrom(component);
					Object.Destroy(component);
				}
				else
				{
					m_CanvasScalerSettings.present = false;
				}
				if (transform.TryGetComponent<GraphicRaycaster>(out var component2))
				{
					m_GraphicRaycasterSettings.present = true;
					m_GraphicRaycasterSettings.CopyFrom(component2);
					Object.Destroy(component2);
				}
				else
				{
					m_GraphicRaycasterSettings.present = false;
				}
				if (transform.TryGetComponent<Canvas>(out var component3))
				{
					m_CanvasSettings.present = true;
					m_CanvasSettings.CopyFrom(component3);
					Object.Destroy(component3);
				}
				else
				{
					m_CanvasSettings.present = false;
				}
				if (transform.TryGetComponent<TrackedDeviceGraphicRaycaster>(out m_TrackedDeviceGraphicRaycaster))
				{
					if (!canvas.TryGetComponent<TrackedDeviceGraphicRaycaster>(out var _))
					{
						Debug.LogWarning("Tracked device raycaster not present on parent canvas: " + parent.name + ". Tracked device input will likely not work on: " + transform.name, transform);
					}
					m_TrackedDeviceGraphicRaycaster.enabled = false;
				}
			}
			if (!m_Nested && (m_WasNested || force) && m_CanvasSettings.present)
			{
				GameObject gameObject = transform.gameObject;
				m_Canvas = gameObject.AddComponent<Canvas>();
				m_CanvasSettings.CopyTo(m_Canvas);
				if (m_CanvasScalerSettings.present)
				{
					CanvasScaler dest = gameObject.AddComponent<CanvasScaler>();
					m_CanvasScalerSettings.CopyTo(dest);
				}
				if (m_GraphicRaycasterSettings.present)
				{
					m_Raycaster = gameObject.AddComponent<GraphicRaycaster>();
					m_GraphicRaycasterSettings.CopyTo(m_Raycaster);
				}
				if (m_TrackedDeviceGraphicRaycaster != null)
				{
					m_TrackedDeviceGraphicRaycaster.enabled = true;
				}
			}
			m_WasNested = m_Nested;
		}

		internal void CheckForOutOfView(Transform gazeSource, float fovAngle, float facingAngle, float maxDistance)
		{
			if (m_Nested || m_Canvas.renderMode != RenderMode.WorldSpace)
			{
				return;
			}
			m_CheckTimer += Time.deltaTime;
			if (m_CheckTimer < 0.5f)
			{
				return;
			}
			m_CheckTimer = 0f;
			Transform transform = m_Canvas.transform;
			Vector3 position = gazeSource.position;
			Vector3 forward = gazeSource.forward;
			Vector3 position2 = transform.position;
			Vector3 forward2 = transform.forward;
			float3 gazePosition = position;
			float3 gazeDirection = forward;
			float3 targetPosition = position2;
			int num;
			if (!BurstGazeUtility.IsOutsideGaze(in gazePosition, in gazeDirection, in targetPosition, fovAngle))
			{
				float3 gazeDirection2 = forward;
				float3 targetDirection = forward2;
				if (!BurstGazeUtility.IsAlignedToGazeForward(in gazeDirection2, in targetDirection, facingAngle))
				{
					float3 gazePosition2 = position;
					float3 targetPosition2 = position2;
					num = (BurstGazeUtility.IsOutsideDistanceRange(in gazePosition2, in targetPosition2, maxDistance) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
			}
			else
			{
				num = 1;
			}
			bool flag = (byte)num != 0;
			if (m_RaysDisabled != flag)
			{
				m_RaysDisabled = flag;
				if (m_Raycaster != null)
				{
					m_Raycaster.enabled = !m_RaysDisabled;
				}
				if (m_TrackedDeviceGraphicRaycaster != null)
				{
					m_TrackedDeviceGraphicRaycaster.enabled = !m_RaysDisabled;
				}
			}
		}
	}

	[SerializeField]
	[Tooltip("How wide of an field-of-view to use when determining if a canvas is in view.")]
	private float m_RayPositionIgnoreAngle = 45f;

	[SerializeField]
	[Tooltip("How much the camera and canvas rotate away from one another and still be considered facing.")]
	private float m_RayFacingIgnoreAngle = 75f;

	[SerializeField]
	[Tooltip("How far away a canvas can be from this camera and still receive input.")]
	private float m_RayPositionIgnoreDistance = 25f;

	private Camera m_CullingCamera;

	private Transform m_CullingCameraTransform;

	private readonly Dictionary<CanvasTracker, CanvasState> m_CanvasTrackers = new Dictionary<CanvasTracker, CanvasState>();

	public float rayPositionIgnoreAngle
	{
		get
		{
			return m_RayPositionIgnoreAngle;
		}
		set
		{
			m_RayPositionIgnoreAngle = value;
		}
	}

	public float rayFacingIgnoreAngle
	{
		get
		{
			return m_RayFacingIgnoreAngle;
		}
		set
		{
			m_RayFacingIgnoreAngle = value;
		}
	}

	public float rayPositionIgnoreDistance
	{
		get
		{
			return m_RayPositionIgnoreDistance;
		}
		set
		{
			m_RayPositionIgnoreDistance = value;
		}
	}

	protected void Awake()
	{
		if (ComponentLocatorUtility<CanvasOptimizer>.FindComponent() != this)
		{
			Debug.LogWarning("Duplicate Canvas Optimizer " + base.gameObject.name + " found. Only one Canvas Optimizer is allowed in the scene at a time.", this);
			Object.Destroy(this);
			base.enabled = false;
			return;
		}
		FindCullingCamera();
		Canvas[] array = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (Canvas canvas in array)
		{
			RegisterCanvas(canvas);
		}
	}

	protected void Update()
	{
		CheckForNestedCanvasChanges();
		CheckForOutOfViewCanvases();
	}

	public void RegisterCanvas(Canvas canvas)
	{
		CanvasTracker canvasTracker = InitializeCanvasTracking(canvas);
		if (!m_CanvasTrackers.ContainsKey(canvasTracker))
		{
			CanvasState canvasState = new CanvasState();
			canvasState.Initialize(canvasTracker);
			m_CanvasTrackers.Add(canvasTracker, canvasState);
		}
	}

	public void UnregisterCanvas(Canvas canvas)
	{
		if (canvas.TryGetComponent<CanvasTracker>(out var component))
		{
			m_CanvasTrackers.Remove(component);
		}
	}

	private static CanvasTracker InitializeCanvasTracking(Canvas target)
	{
		if (!target.gameObject.TryGetComponent<CanvasTracker>(out var component))
		{
			component = target.gameObject.AddComponent<CanvasTracker>();
			component.hideFlags = HideFlags.HideAndDontSave;
		}
		return component;
	}

	private void CheckForNestedCanvasChanges()
	{
		foreach (CanvasState value in m_CanvasTrackers.Values)
		{
			value.CheckForNestedChanges();
		}
	}

	private void CheckForOutOfViewCanvases()
	{
		if (m_CullingCamera == null || !m_CullingCamera.enabled)
		{
			FindCullingCamera();
			if (m_CullingCameraTransform == null)
			{
				return;
			}
		}
		foreach (CanvasState value in m_CanvasTrackers.Values)
		{
			value.CheckForOutOfView(m_CullingCameraTransform, m_RayPositionIgnoreAngle, m_RayFacingIgnoreAngle, m_RayPositionIgnoreDistance);
		}
	}

	private void FindCullingCamera()
	{
		m_CullingCamera = Camera.main;
		m_CullingCameraTransform = ((m_CullingCamera != null) ? m_CullingCamera.transform : null);
	}
}
