using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("Event/Tracked Device Physics Raycaster", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.TrackedDevicePhysicsRaycaster.html")]
public class TrackedDevicePhysicsRaycaster : BaseRaycaster
{
	private class RaycastHitArraySegment : IEnumerable<RaycastHit>, IEnumerable, IEnumerator<RaycastHit>, IEnumerator, IDisposable
	{
		private int m_Count;

		private readonly RaycastHit[] m_Hits;

		private int m_CurrentIndex;

		public int count
		{
			get
			{
				return m_Count;
			}
			set
			{
				m_Count = value;
			}
		}

		public RaycastHit Current => m_Hits[m_CurrentIndex];

		object IEnumerator.Current => Current;

		public RaycastHitArraySegment(RaycastHit[] raycastHits, int count)
		{
			m_Hits = raycastHits;
			m_Count = count;
		}

		public bool MoveNext()
		{
			m_CurrentIndex++;
			return m_CurrentIndex < m_Count;
		}

		public void Reset()
		{
			m_CurrentIndex = -1;
		}

		public void Dispose()
		{
		}

		public IEnumerator<RaycastHit> GetEnumerator()
		{
			Reset();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	private sealed class RaycastHitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit a, RaycastHit b)
		{
			return a.distance.CompareTo(b.distance);
		}
	}

	private const int k_EverythingLayerMask = -1;

	[SerializeField]
	[Tooltip("Specifies whether the ray cast should hit triggers. Use Global refers to the Queries Hit Triggers setting in Physics Project Settings.")]
	private QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

	[SerializeField]
	[Tooltip("Layer mask used to filter events. Always combined with the ray cast mask of the UI interactor.")]
	private LayerMask m_EventMask = -1;

	[SerializeField]
	[Tooltip("The max number of intersections allowed. Value will be clamped to greater than 0.")]
	private int m_MaxRayIntersections = 10;

	[SerializeField]
	[Tooltip("The event camera for this ray caster. The event camera is used to determine the screen position and display of the ray cast results.")]
	private Camera m_EventCamera;

	private bool m_HasWarnedEventCameraNull;

	private RaycastHit[] m_RaycastHits;

	private readonly RaycastHitComparer m_RaycastHitComparer = new RaycastHitComparer();

	private RaycastHitArraySegment m_RaycastArrayWrapper;

	private readonly List<RaycastHit> m_RaycastResultsCache = new List<RaycastHit>();

	private PhysicsScene m_LocalPhysicsScene;

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

	public LayerMask eventMask
	{
		get
		{
			return m_EventMask;
		}
		set
		{
			m_EventMask = value;
		}
	}

	public int maxRayIntersections
	{
		get
		{
			return m_MaxRayIntersections;
		}
		set
		{
			m_MaxRayIntersections = Math.Max(value, 1);
		}
	}

	public override Camera eventCamera
	{
		get
		{
			if (m_EventCamera == null)
			{
				m_EventCamera = GetComponent<Camera>();
			}
			if (!(m_EventCamera != null))
			{
				return Camera.main;
			}
			return m_EventCamera;
		}
	}

	public void SetEventCamera(Camera newEventCamera)
	{
		m_EventCamera = newEventCamera;
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData is TrackedDeviceEventData eventData2)
		{
			PerformRaycasts(eventData2, resultAppendList);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_LocalPhysicsScene = base.gameObject.scene.GetPhysicsScene();
		m_RaycastHits = new RaycastHit[m_MaxRayIntersections];
		m_RaycastArrayWrapper = new RaycastHitArraySegment(m_RaycastHits, 0);
	}

	private void PerformRaycasts(TrackedDeviceEventData eventData, List<RaycastResult> resultAppendList)
	{
		Camera camera = eventCamera;
		if (camera == null)
		{
			if (!m_HasWarnedEventCameraNull)
			{
				Debug.LogWarning("Event Camera must be set on TrackedDevicePhysicsRaycaster to determine screen space coordinates. UI events will not function correctly until it is set.", this);
				m_HasWarnedEventCameraNull = true;
			}
			return;
		}
		List<Vector3> rayPoints = eventData.rayPoints;
		int num = (int)eventData.layerMask & (int)m_EventMask;
		float existingHitLength = 0f;
		for (int i = 1; i < rayPoints.Count; i++)
		{
			Vector3 from = rayPoints[i - 1];
			Vector3 to = rayPoints[i];
			if (PerformRaycast(from, to, num, camera, resultAppendList, ref existingHitLength))
			{
				eventData.rayHitIndex = i;
				break;
			}
		}
	}

	private bool PerformRaycast(Vector3 from, Vector3 to, LayerMask layerMask, Camera currentEventCamera, List<RaycastResult> resultAppendList, ref float existingHitLength)
	{
		bool result = false;
		float num = Vector3.Distance(to, from);
		Ray ray = new Ray(from, (to - from).normalized * num);
		float num2 = num;
		m_MaxRayIntersections = Math.Max(m_MaxRayIntersections, 1);
		if (m_RaycastHits.Length != m_MaxRayIntersections)
		{
			Array.Resize(ref m_RaycastHits, m_MaxRayIntersections);
		}
		int count = m_LocalPhysicsScene.Raycast(ray.origin, ray.direction, m_RaycastHits, num2, layerMask, m_RaycastTriggerInteraction);
		m_RaycastArrayWrapper.count = count;
		m_RaycastResultsCache.Clear();
		m_RaycastResultsCache.AddRange(m_RaycastArrayWrapper);
		SortingHelpers.Sort(m_RaycastResultsCache, m_RaycastHitComparer);
		foreach (RaycastHit item2 in m_RaycastResultsCache)
		{
			GameObject gameObject = item2.collider.gameObject;
			Vector2 vector = currentEventCamera.WorldToScreenPoint(item2.point);
			int displayIndex = (int)Display.RelativeMouseAt(vector).z;
			if (item2.distance <= num2)
			{
				RaycastResult raycastResult = default(RaycastResult);
				raycastResult.gameObject = gameObject;
				raycastResult.module = this;
				raycastResult.distance = existingHitLength + item2.distance;
				raycastResult.index = resultAppendList.Count;
				raycastResult.depth = 0;
				raycastResult.sortingLayer = 0;
				raycastResult.sortingOrder = 0;
				raycastResult.worldPosition = item2.point;
				raycastResult.worldNormal = item2.normal;
				raycastResult.screenPosition = vector;
				raycastResult.displayIndex = displayIndex;
				RaycastResult item = raycastResult;
				resultAppendList.Add(item);
				result = true;
			}
		}
		existingHitLength += num2;
		return result;
	}
}
