using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("Event/Tracked Device Graphic Raycaster", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster.html")]
public class TrackedDeviceGraphicRaycaster : BaseRaycaster, IPokeStateDataProvider, IMultiPokeStateDataProvider
{
	private readonly struct RaycastHitData
	{
		public Graphic graphic { get; }

		public Vector3 worldHitPosition { get; }

		public Vector2 screenPosition { get; }

		public float distance { get; }

		public int displayIndex { get; }

		public RaycastHitData(Graphic graphic, Vector3 worldHitPosition, Vector2 screenPosition, float distance, int displayIndex)
		{
			this.graphic = graphic;
			this.worldHitPosition = worldHitPosition;
			this.screenPosition = screenPosition;
			this.distance = distance;
			this.displayIndex = displayIndex;
		}
	}

	private sealed class RaycastHitComparer : IComparer<RaycastHitData>
	{
		public int Compare(RaycastHitData a, RaycastHitData b)
		{
			int num = b.graphic.canvas.sortingOrder.CompareTo(a.graphic.canvas.sortingOrder);
			if (num != 0)
			{
				return num;
			}
			return b.graphic.depth.CompareTo(a.graphic.depth);
		}
	}

	private const int k_MaxRaycastHits = 10;

	[SerializeField]
	[Tooltip("Whether Graphics facing away from the ray caster are checked for ray casts. Enable this to ignore backfacing Graphics.")]
	private bool m_IgnoreReversedGraphics;

	[SerializeField]
	[Tooltip("Whether or not 2D occlusion is checked when performing ray casts. Enable to make Graphics be blocked by 2D objects that exist in front of it.")]
	private bool m_CheckFor2DOcclusion;

	[SerializeField]
	[Tooltip("Whether or not 3D occlusion is checked when performing ray casts. Enable to make Graphics be blocked by 3D objects that exist in front of it.")]
	private bool m_CheckFor3DOcclusion;

	[SerializeField]
	[Tooltip("The layers of objects that are checked to determine if they block Graphic ray casts when checking for 2D or 3D occlusion.")]
	private LayerMask m_BlockingMask = -1;

	[SerializeField]
	[Tooltip("Specifies whether the ray cast should hit Triggers when checking for 3D occlusion. Use Global refers to the Queries Hit Triggers setting in Physics Project Settings.")]
	private QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

	private Canvas m_Canvas;

	private bool m_HasWarnedEventCameraNull;

	private readonly RaycastHit[] m_OcclusionHits3D = new RaycastHit[10];

	private readonly RaycastHit2D[] m_OcclusionHits2D = new RaycastHit2D[1];

	private static readonly RaycastHitComparer s_RaycastHitComparer = new RaycastHitComparer();

	private static readonly Vector3[] s_Corners = new Vector3[4];

	private readonly List<RaycastHitData> m_RaycastResultsCache = new List<RaycastHitData>();

	[NonSerialized]
	private static readonly List<RaycastHitData> s_SortedGraphics = new List<RaycastHitData>();

	[NonSerialized]
	private static readonly Dictionary<IUIInteractor, RaycastHitData> s_InteractorHitData = new Dictionary<IUIInteractor, RaycastHitData>();

	private XRPokeLogic m_PokeLogic;

	[NonSerialized]
	private static readonly Dictionary<IUIInteractor, TrackedDeviceGraphicRaycaster> s_InteractorRaycasters = new Dictionary<IUIInteractor, TrackedDeviceGraphicRaycaster>();

	[NonSerialized]
	private static readonly Dictionary<TrackedDeviceGraphicRaycaster, HashSet<IUIInteractor>> s_PokeHoverRaycasters = new Dictionary<TrackedDeviceGraphicRaycaster, HashSet<IUIInteractor>>();

	private BindingsGroup m_BindingsGroup = new BindingsGroup();

	private PhysicsScene m_LocalPhysicsScene;

	private PhysicsScene2D m_LocalPhysicsScene2D;

	public bool ignoreReversedGraphics
	{
		get
		{
			return m_IgnoreReversedGraphics;
		}
		set
		{
			m_IgnoreReversedGraphics = value;
		}
	}

	public bool checkFor2DOcclusion
	{
		get
		{
			return m_CheckFor2DOcclusion;
		}
		set
		{
			m_CheckFor2DOcclusion = value;
		}
	}

	public bool checkFor3DOcclusion
	{
		get
		{
			return m_CheckFor3DOcclusion;
		}
		set
		{
			m_CheckFor3DOcclusion = value;
		}
	}

	public LayerMask blockingMask
	{
		get
		{
			return m_BlockingMask;
		}
		set
		{
			m_BlockingMask = value;
		}
	}

	public QueryTriggerInteraction raycastTriggerInteraction
	{
		get
		{
			return m_RaycastTriggerInteraction;
		}
		set
		{
			m_RaycastTriggerInteraction = value;
		}
	}

	public override Camera eventCamera
	{
		get
		{
			if (!(canvas != null) || !(canvas.worldCamera != null))
			{
				return Camera.main;
			}
			return canvas.worldCamera;
		}
	}

	private Canvas canvas
	{
		get
		{
			if (m_Canvas != null)
			{
				return m_Canvas;
			}
			TryGetComponent<Canvas>(out m_Canvas);
			return m_Canvas;
		}
	}

	public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeLogic?.pokeStateData;

	private Dictionary<Transform, BindableVariable<PokeStateData>> pokeStateDataDictionary { get; } = new Dictionary<Transform, BindableVariable<PokeStateData>>();


	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData is TrackedDeviceEventData eventData2)
		{
			PerformRaycasts(eventData2, resultAppendList);
		}
	}

	public static bool IsPokeInteractingWithUI(IUIInteractor interactor)
	{
		foreach (HashSet<IUIInteractor> value in s_PokeHoverRaycasters.Values)
		{
			if (value.Contains(interactor))
			{
				return true;
			}
		}
		return false;
	}

	private void EndPokeInteraction(IUIInteractor interactor)
	{
		if (interactor != null)
		{
			m_PokeLogic.OnHoverExited(interactor);
			if (s_InteractorRaycasters.TryGetValue(interactor, out var value) && value != null && value == this)
			{
				s_InteractorRaycasters.Remove(interactor);
			}
			s_InteractorHitData.Remove(interactor);
			s_PokeHoverRaycasters[this].Remove(interactor);
		}
	}

	public static bool TryGetPokeStateDataForInteractor(IUIInteractor interactor, out PokeStateData data)
	{
		foreach (KeyValuePair<TrackedDeviceGraphicRaycaster, HashSet<IUIInteractor>> s_PokeHoverRaycaster in s_PokeHoverRaycasters)
		{
			if (s_PokeHoverRaycaster.Value.Contains(interactor))
			{
				TrackedDeviceGraphicRaycaster key = s_PokeHoverRaycaster.Key;
				data = key.pokeStateData.Value;
				return true;
			}
		}
		data = default(PokeStateData);
		return false;
	}

	public IReadOnlyBindableVariable<PokeStateData> GetPokeStateDataForTarget(Transform target)
	{
		if (!pokeStateDataDictionary.ContainsKey(target))
		{
			pokeStateDataDictionary[target] = new BindableVariable<PokeStateData>();
		}
		return pokeStateDataDictionary[target];
	}

	public static bool IsPokeSelectingWithUI(IUIInteractor interactor)
	{
		if (interactor != null && s_InteractorRaycasters.TryGetValue(interactor, out var value))
		{
			return value != null;
		}
		return false;
	}

	private static RaycastHit FindClosestHit(RaycastHit[] hits, int count)
	{
		int num = 0;
		float num2 = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			if (hits[i].distance < num2)
			{
				num2 = hits[i].distance;
				num = i;
			}
		}
		return hits[num];
	}

	protected override void Awake()
	{
		base.Awake();
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		m_LocalPhysicsScene2D = base.gameObject.scene.GetPhysicsScene2D();
		s_PokeHoverRaycasters.Add(this, new HashSet<IUIInteractor>());
		SetupPoke();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		HashSet<IUIInteractor> value;
		using (CollectionPool<HashSet<IUIInteractor>, IUIInteractor>.Get(out value))
		{
			foreach (KeyValuePair<IUIInteractor, TrackedDeviceGraphicRaycaster> s_InteractorRaycaster in s_InteractorRaycasters)
			{
				if (s_InteractorRaycaster.Value == this)
				{
					value.Add(s_InteractorRaycaster.Key);
				}
			}
			foreach (IUIInteractor item in s_PokeHoverRaycasters[this])
			{
				value.Add(item);
			}
			foreach (IUIInteractor item2 in value)
			{
				EndPokeInteraction(item2);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		s_PokeHoverRaycasters.Remove(this);
		pokeStateDataDictionary.Clear();
		m_BindingsGroup.Clear();
	}

	private void SetupPoke()
	{
		m_BindingsGroup.Clear();
		if (m_PokeLogic == null)
		{
			m_PokeLogic = new XRPokeLogic();
		}
		PokeThresholdData pokeThresholdData = new PokeThresholdData
		{
			pokeDirection = PokeAxis.Z,
			interactionDepthOffset = 0f,
			enablePokeAngleThreshold = true,
			pokeAngleThreshold = 89.9f
		};
		m_PokeLogic.Initialize(base.transform, pokeThresholdData, null);
		m_PokeLogic.SetPokeDepth(0.1f);
		m_BindingsGroup.AddBinding(m_PokeLogic.pokeStateData.SubscribeAndUpdate(delegate(PokeStateData data)
		{
			if (data.target != null)
			{
				if (!pokeStateDataDictionary.ContainsKey(data.target))
				{
					pokeStateDataDictionary[data.target] = new BindableVariable<PokeStateData>();
				}
				pokeStateDataDictionary[data.target].Value = data;
				return;
			}
			foreach (BindableVariable<PokeStateData> value in pokeStateDataDictionary.Values)
			{
				value.Value = data;
			}
		}));
	}

	private void PerformRaycasts(TrackedDeviceEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (canvas == null)
		{
			return;
		}
		Camera camera = eventCamera;
		if (camera == null)
		{
			if (!m_HasWarnedEventCameraNull)
			{
				Debug.LogWarning("Event Camera must be set on World Space Canvas to perform ray casts with tracked device. UI events will not function correctly until it is set.", this);
				m_HasWarnedEventCameraNull = true;
			}
			return;
		}
		LayerMask layerMask = eventData.layerMask;
		IUIInteractor interactor = eventData.interactor;
		if (interactor != null && interactor.TryGetUIModel(out var model) && model.interactionType == UIInteractionType.Poke)
		{
			if (PerformSpherecast(model.position, model.pokeDepth, layerMask, camera, resultAppendList) && resultAppendList.Count > 0)
			{
				eventData.rayHitIndex = 1;
				Transform transform = resultAppendList[0].gameObject.transform;
				m_PokeLogic.SetPokeDepth(model.pokeDepth);
				if (!s_PokeHoverRaycasters[this].Contains(interactor))
				{
					s_PokeHoverRaycasters[this].Add(interactor);
					m_PokeLogic.OnHoverEntered(interactor, new Pose(model.position, model.orientation), transform);
				}
				if (m_PokeLogic.MeetsRequirementsForSelectAction(interactor, transform.position, model.position, 0f, transform))
				{
					if (m_RaycastResultsCache.Count > 0)
					{
						RaycastHitData raycastHitData = m_RaycastResultsCache[0];
						if (!s_InteractorHitData.TryGetValue(interactor, out var value) || s_RaycastHitComparer.Compare(value, raycastHitData) < 0)
						{
							s_InteractorHitData[interactor] = raycastHitData;
							s_InteractorRaycasters[interactor] = this;
						}
					}
				}
				else
				{
					s_InteractorRaycasters.Remove(interactor);
				}
			}
			else
			{
				EndPokeInteraction(interactor);
			}
			return;
		}
		List<Vector3> rayPoints = eventData.rayPoints;
		float existingHitLength = 0f;
		for (int i = 1; i < rayPoints.Count; i++)
		{
			Vector3 from = rayPoints[i - 1];
			Vector3 to = rayPoints[i];
			if (PerformRaycast(from, to, layerMask, camera, resultAppendList, ref existingHitLength))
			{
				eventData.rayHitIndex = i;
				break;
			}
		}
	}

	private bool PerformSpherecast(Vector3 origin, float radius, LayerMask layerMask, Camera currentEventCamera, List<RaycastResult> resultAppendList)
	{
		m_RaycastResultsCache.Clear();
		SortedSpherecastGraphics(canvas, origin, radius, layerMask, currentEventCamera, m_RaycastResultsCache);
		if (m_RaycastResultsCache.Count <= 0)
		{
			return false;
		}
		RaycastHitData item = m_RaycastResultsCache[0];
		Ray ray = new Ray(origin, item.worldHitPosition - origin);
		m_RaycastResultsCache.Clear();
		m_RaycastResultsCache.Add(item);
		return ProcessSortedHitsResults(ray, float.PositiveInfinity, hitSomething: false, m_RaycastResultsCache, resultAppendList);
	}

	private bool PerformRaycast(Vector3 from, Vector3 to, LayerMask layerMask, Camera currentEventCamera, List<RaycastResult> resultAppendList, ref float existingHitLength)
	{
		bool hitSomething = false;
		float num = Vector3.Distance(to, from);
		Ray ray = new Ray(from, to - from);
		float num2 = num;
		if (m_CheckFor3DOcclusion)
		{
			int num3 = m_LocalPhysicsScene.Raycast(ray.origin, ray.direction, m_OcclusionHits3D, num2, m_BlockingMask, m_RaycastTriggerInteraction);
			if (num3 > 0)
			{
				RaycastHit raycastHit = FindClosestHit(m_OcclusionHits3D, num3);
				num2 = existingHitLength + raycastHit.distance;
				hitSomething = true;
			}
		}
		if (m_CheckFor2DOcclusion && m_LocalPhysicsScene2D.GetRayIntersection(ray, num2, m_OcclusionHits2D, m_BlockingMask) > 0)
		{
			num2 = m_OcclusionHits2D[0].distance;
			hitSomething = true;
		}
		m_RaycastResultsCache.Clear();
		SortedRaycastGraphics(canvas, ray, num2, layerMask, currentEventCamera, m_RaycastResultsCache);
		return ProcessSortedHitsResults(ray, num2, hitSomething, m_RaycastResultsCache, resultAppendList);
	}

	private bool ProcessSortedHitsResults(Ray ray, float hitDistance, bool hitSomething, List<RaycastHitData> raycastHitDatums, List<RaycastResult> resultAppendList)
	{
		foreach (RaycastHitData raycastHitDatum in raycastHitDatums)
		{
			bool flag = true;
			GameObject gameObject = raycastHitDatum.graphic.gameObject;
			if (m_IgnoreReversedGraphics)
			{
				Vector3 direction = ray.direction;
				Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
				flag = Vector3.Dot(direction, rhs) > 0f;
			}
			if (flag & (raycastHitDatum.distance <= hitDistance))
			{
				Vector3 forward = gameObject.transform.forward;
				RaycastResult raycastResult = default(RaycastResult);
				raycastResult.gameObject = gameObject;
				raycastResult.module = this;
				raycastResult.distance = raycastHitDatum.distance;
				raycastResult.index = resultAppendList.Count;
				raycastResult.depth = raycastHitDatum.graphic.depth;
				raycastResult.sortingLayer = canvas.sortingLayerID;
				raycastResult.sortingOrder = canvas.sortingOrder;
				raycastResult.worldPosition = raycastHitDatum.worldHitPosition;
				raycastResult.worldNormal = -forward;
				raycastResult.screenPosition = raycastHitDatum.screenPosition;
				raycastResult.displayIndex = raycastHitDatum.displayIndex;
				RaycastResult item = raycastResult;
				resultAppendList.Add(item);
				hitSomething = true;
			}
		}
		return hitSomething;
	}

	private static void SortedSpherecastGraphics(Canvas canvas, Vector3 origin, float radius, LayerMask layerMask, Camera eventCamera, List<RaycastHitData> results)
	{
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		s_SortedGraphics.Clear();
		for (int i = 0; i < graphicsForCanvas.Count; i++)
		{
			Graphic graphic = graphicsForCanvas[i];
			if (!ShouldTestGraphic(graphic, layerMask))
			{
				continue;
			}
			Vector4 raycastPadding = graphic.raycastPadding;
			if (SphereIntersectsRectTransform(graphic.rectTransform, raycastPadding, origin, out var worldPosition, out var distance) && distance <= radius)
			{
				Vector2 vector = eventCamera.WorldToScreenPoint(worldPosition);
				if (graphic.Raycast(vector, eventCamera))
				{
					s_SortedGraphics.Add(new RaycastHitData(graphic, worldPosition, vector, distance, eventCamera.targetDisplay));
				}
			}
		}
		SortingHelpers.Sort(s_SortedGraphics, s_RaycastHitComparer);
		results.AddRange(s_SortedGraphics);
	}

	private static void SortedRaycastGraphics(Canvas canvas, Ray ray, float maxDistance, LayerMask layerMask, Camera eventCamera, List<RaycastHitData> results)
	{
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		s_SortedGraphics.Clear();
		for (int i = 0; i < graphicsForCanvas.Count; i++)
		{
			Graphic graphic = graphicsForCanvas[i];
			if (!ShouldTestGraphic(graphic, layerMask))
			{
				continue;
			}
			Vector4 raycastPadding = graphic.raycastPadding;
			if (RayIntersectsRectTransform(graphic.rectTransform, raycastPadding, ray, out var worldPosition, out var distance) && distance <= maxDistance)
			{
				Vector2 vector = eventCamera.WorldToScreenPoint(worldPosition);
				if (graphic.Raycast(vector, eventCamera))
				{
					s_SortedGraphics.Add(new RaycastHitData(graphic, worldPosition, vector, distance, eventCamera.targetDisplay));
				}
			}
		}
		SortingHelpers.Sort(s_SortedGraphics, s_RaycastHitComparer);
		results.AddRange(s_SortedGraphics);
	}

	private static bool ShouldTestGraphic(Graphic graphic, LayerMask layerMask)
	{
		if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
		{
			return false;
		}
		if (((1 << graphic.gameObject.layer) & (int)layerMask) == 0)
		{
			return false;
		}
		return true;
	}

	private static bool SphereIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Vector3 from, out Vector3 worldPosition, out float distance)
	{
		Plane rectTransformPlane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
		Vector3 vector = rectTransformPlane.ClosestPointOnPlane(from);
		return RayIntersectsRectTransform(new Ray(from, vector - from), rectTransformPlane, out worldPosition, out distance);
	}

	private static bool RayIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Ray ray, out Vector3 worldPosition, out float distance)
	{
		Plane rectTransformPlane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
		return RayIntersectsRectTransform(ray, rectTransformPlane, out worldPosition, out distance);
	}

	private static bool RayIntersectsRectTransform(Ray ray, Plane plane, out Vector3 worldPosition, out float distance)
	{
		if (plane.Raycast(ray, out var enter))
		{
			Vector3 point = ray.GetPoint(enter);
			Vector3 rhs = s_Corners[3] - s_Corners[0];
			Vector3 rhs2 = s_Corners[1] - s_Corners[0];
			float num = Vector3.Dot(point - s_Corners[0], rhs);
			if (Vector3.Dot(point - s_Corners[0], rhs2) >= 0f && num >= 0f)
			{
				Vector3 rhs3 = s_Corners[1] - s_Corners[2];
				Vector3 rhs4 = s_Corners[3] - s_Corners[2];
				float num2 = Vector3.Dot(point - s_Corners[2], rhs3);
				float num3 = Vector3.Dot(point - s_Corners[2], rhs4);
				if (num2 >= 0f && num3 >= 0f)
				{
					worldPosition = point;
					distance = enter;
					return true;
				}
			}
		}
		worldPosition = Vector3.zero;
		distance = 0f;
		return false;
	}

	private static Plane GetRectTransformPlane(RectTransform transform, Vector4 raycastPadding, Vector3[] fourCornersArray)
	{
		GetRectTransformWorldCorners(transform, raycastPadding, fourCornersArray);
		return new Plane(fourCornersArray[0], fourCornersArray[1], fourCornersArray[2]);
	}

	private static void GetRectTransformWorldCorners(RectTransform transform, Vector4 offset, Vector3[] fourCornersArray)
	{
		if (fourCornersArray == null || fourCornersArray.Length < 4)
		{
			Debug.LogError("Calling GetRectTransformWorldCorners with an array that is null or has less than 4 elements.");
			return;
		}
		Rect rect = transform.rect;
		float x = rect.x + offset.x;
		float y = rect.y + offset.y;
		float x2 = rect.xMax - offset.z;
		float y2 = rect.yMax - offset.w;
		fourCornersArray[0] = new Vector3(x, y, 0f);
		fourCornersArray[1] = new Vector3(x, y2, 0f);
		fourCornersArray[2] = new Vector3(x2, y2, 0f);
		fourCornersArray[3] = new Vector3(x2, y, 0f);
		Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
		for (int i = 0; i < 4; i++)
		{
			fourCornersArray[i] = localToWorldMatrix.MultiplyPoint(fourCornersArray[i]);
		}
	}

	[Conditional("UNITY_EDITOR")]
	protected void OnDrawGizmosSelected()
	{
	}
}
