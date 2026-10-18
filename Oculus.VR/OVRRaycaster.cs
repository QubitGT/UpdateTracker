using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-dronerage-example-scenes/")]
public class OVRRaycaster : GraphicRaycaster, IPointerEnterHandler, IEventSystemHandler
{
	private struct RaycastHit
	{
		public Graphic graphic;

		public Vector3 worldPos;

		public bool fromMouse;
	}

	[Tooltip("A world space pointer for this canvas")]
	public GameObject pointer;

	public int sortOrder;

	[NonSerialized]
	private Canvas m_Canvas;

	[NonSerialized]
	private OVRRayTransformer m_RayTransformer;

	[NonSerialized]
	private List<RaycastHit> m_RaycastResults = new List<RaycastHit>();

	[NonSerialized]
	private static readonly List<RaycastHit> s_SortedGraphics = new List<RaycastHit>();

	private static readonly Vector3[] _corners = new Vector3[4];

	private Canvas canvas
	{
		get
		{
			if (m_Canvas != null)
			{
				return m_Canvas;
			}
			m_Canvas = GetComponent<Canvas>();
			m_RayTransformer = GetComponent<OVRRayTransformer>();
			return m_Canvas;
		}
	}

	private OVRRayTransformer rayTransformer => m_RayTransformer;

	public override Camera eventCamera => canvas.worldCamera;

	public override int sortOrderPriority => sortOrder;

	protected OVRRaycaster()
	{
	}

	protected override void Start()
	{
		if (!canvas.worldCamera)
		{
			Debug.Log("Canvas does not have an event camera attached. Attaching OVRCameraRig.centerEyeAnchor as default.");
			OVRCameraRig oVRCameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
			if ((bool)oVRCameraRig)
			{
				canvas.worldCamera = oVRCameraRig.centerEyeAnchor.gameObject.GetComponent<Camera>();
			}
		}
	}

	private void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList, Ray ray, bool checkForBlocking, bool checkOnlyRaycastable)
	{
		if (canvas == null)
		{
			return;
		}
		float num = float.MaxValue;
		if (checkForBlocking && base.blockingObjects != 0)
		{
			float farClipPlane = eventCamera.farClipPlane;
			if (base.blockingObjects == BlockingObjects.ThreeD || base.blockingObjects == BlockingObjects.All)
			{
				UnityEngine.RaycastHit[] array = Physics.RaycastAll(ray, farClipPlane, m_BlockingMask);
				if (array.Length != 0 && array[0].distance < num)
				{
					num = array[0].distance;
				}
			}
			if (base.blockingObjects == BlockingObjects.TwoD || base.blockingObjects == BlockingObjects.All)
			{
				RaycastHit2D[] rayIntersectionAll = Physics2D.GetRayIntersectionAll(ray, farClipPlane, m_BlockingMask);
				if (rayIntersectionAll.Length != 0 && rayIntersectionAll[0].fraction * farClipPlane < num)
				{
					num = rayIntersectionAll[0].fraction * farClipPlane;
				}
			}
		}
		m_RaycastResults.Clear();
		GraphicRaycast(canvas, rayTransformer, ray, m_RaycastResults, checkOnlyRaycastable);
		for (int i = 0; i < m_RaycastResults.Count; i++)
		{
			GameObject gameObject = m_RaycastResults[i].graphic.gameObject;
			bool flag = true;
			if (base.ignoreReversedGraphics)
			{
				Vector3 direction = ray.direction;
				Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
				flag = Vector3.Dot(direction, rhs) > 0f;
			}
			if (eventCamera.transform.InverseTransformPoint(m_RaycastResults[i].worldPos).z <= 0f)
			{
				flag = false;
			}
			if (flag)
			{
				float num2 = Vector3.Distance(ray.origin, m_RaycastResults[i].worldPos);
				if (!(num2 >= num))
				{
					RaycastResult raycastResult = default(RaycastResult);
					raycastResult.gameObject = gameObject;
					raycastResult.module = this;
					raycastResult.distance = num2;
					raycastResult.index = resultAppendList.Count;
					raycastResult.depth = m_RaycastResults[i].graphic.depth;
					raycastResult.worldPosition = m_RaycastResults[i].worldPos;
					RaycastResult item = raycastResult;
					resultAppendList.Add(item);
				}
			}
		}
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData.IsVRPointer())
		{
			Raycast(eventData, resultAppendList, eventData.GetRay(), checkForBlocking: true, checkOnlyRaycastable: false);
		}
	}

	internal void RaycastOnRaycastableGraphics(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventData.IsVRPointer())
		{
			Raycast(eventData, resultAppendList, eventData.GetRay(), checkForBlocking: false, checkOnlyRaycastable: true);
		}
	}

	public void RaycastPointer(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (pointer != null && pointer.activeInHierarchy)
		{
			Raycast(eventData, resultAppendList, new Ray(eventCamera.transform.position, (pointer.transform.position - eventCamera.transform.position).normalized), checkForBlocking: false, checkOnlyRaycastable: false);
		}
	}

	private void GraphicRaycast(Canvas canvas, OVRRayTransformer rayTransformer, Ray ray, List<RaycastHit> results, bool checkOnlyRaycastableGraphics)
	{
		if (rayTransformer != null)
		{
			ray = rayTransformer.TransformRay(ray);
		}
		IList<Graphic> list = (checkOnlyRaycastableGraphics ? GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas) : GraphicRegistry.GetGraphicsForCanvas(canvas));
		s_SortedGraphics.Clear();
		RaycastHit item = default(RaycastHit);
		for (int i = 0; i < list.Count; i++)
		{
			Graphic graphic = list[i];
			if (graphic.depth != -1 && !(pointer == graphic.gameObject) && RayIntersectsRectTransform(graphic.rectTransform, ray, out var worldPos))
			{
				Vector2 sp = eventCamera.WorldToScreenPoint(worldPos);
				if (graphic.Raycast(sp, eventCamera))
				{
					item.graphic = graphic;
					item.worldPos = worldPos;
					item.fromMouse = false;
					s_SortedGraphics.Add(item);
				}
			}
		}
		s_SortedGraphics.Sort((RaycastHit g1, RaycastHit g2) => g2.graphic.depth.CompareTo(g1.graphic.depth));
		for (int j = 0; j < s_SortedGraphics.Count; j++)
		{
			results.Add(s_SortedGraphics[j]);
		}
	}

	public Vector2 GetScreenPosition(RaycastResult raycastResult)
	{
		return eventCamera.WorldToScreenPoint(raycastResult.worldPosition);
	}

	private static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos)
	{
		rectTransform.GetWorldCorners(_corners);
		if (!new Plane(_corners[0], _corners[1], _corners[2]).Raycast(ray, out var enter))
		{
			worldPos = Vector3.zero;
			return false;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector = _corners[3] - _corners[0];
		Vector3 vector2 = _corners[1] - _corners[0];
		float num = Vector3.Dot(point - _corners[0], vector);
		float num2 = Vector3.Dot(point - _corners[0], vector2);
		if (num < vector.sqrMagnitude && num2 < vector2.sqrMagnitude && num >= 0f && num2 >= 0f)
		{
			worldPos = _corners[0] + num2 * vector2 / vector2.sqrMagnitude + num * vector / vector.sqrMagnitude;
			return true;
		}
		worldPos = Vector3.zero;
		return false;
	}

	public virtual bool IsFocussed()
	{
		OVRInputModule oVRInputModule = EventSystem.current.currentInputModule as OVRInputModule;
		if ((bool)oVRInputModule)
		{
			return oVRInputModule.activeGraphicRaycaster == this;
		}
		return false;
	}

	public virtual void OnPointerEnter(PointerEventData e)
	{
		if (e.IsVRPointer())
		{
			OVRInputModule oVRInputModule = EventSystem.current.currentInputModule as OVRInputModule;
			if (oVRInputModule != null)
			{
				oVRInputModule.activeGraphicRaycaster = this;
			}
		}
	}
}
