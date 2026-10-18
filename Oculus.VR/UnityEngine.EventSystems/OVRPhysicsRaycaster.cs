using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-dronerage-example-scenes/")]
public class OVRPhysicsRaycaster : BaseRaycaster
{
	protected const int kNoEventMaskSet = -1;

	[SerializeField]
	protected LayerMask m_EventMask = -1;

	public int sortOrder;

	public override Camera eventCamera => GetComponent<OVRCameraRig>().leftEyeCamera;

	public virtual int depth
	{
		get
		{
			if (!(eventCamera != null))
			{
				return 16777215;
			}
			return (int)eventCamera.depth;
		}
	}

	public override int sortOrderPriority => sortOrder;

	public int finalEventMask
	{
		get
		{
			if (!(eventCamera != null))
			{
				return -1;
			}
			return eventCamera.cullingMask & (int)m_EventMask;
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

	protected OVRPhysicsRaycaster()
	{
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (eventCamera == null || !eventData.IsVRPointer())
		{
			return;
		}
		Ray ray = eventData.GetRay();
		float maxDistance = eventCamera.farClipPlane - eventCamera.nearClipPlane;
		RaycastHit[] array = Physics.RaycastAll(ray, maxDistance, finalEventMask);
		if (array.Length > 1)
		{
			Array.Sort(array, (RaycastHit r1, RaycastHit r2) => r1.distance.CompareTo(r2.distance));
		}
		if (array.Length != 0)
		{
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				RaycastResult raycastResult = default(RaycastResult);
				raycastResult.gameObject = array[i].collider.gameObject;
				raycastResult.module = this;
				raycastResult.distance = array[i].distance;
				raycastResult.index = resultAppendList.Count;
				raycastResult.worldPosition = array[0].point;
				raycastResult.worldNormal = array[0].normal;
				RaycastResult item = raycastResult;
				resultAppendList.Add(item);
			}
		}
	}

	public void Spherecast(PointerEventData eventData, List<RaycastResult> resultAppendList, float radius)
	{
		if (eventCamera == null || !eventData.IsVRPointer())
		{
			return;
		}
		Ray ray = eventData.GetRay();
		float maxDistance = eventCamera.farClipPlane - eventCamera.nearClipPlane;
		RaycastHit[] array = Physics.SphereCastAll(ray, radius, maxDistance, finalEventMask);
		if (array.Length > 1)
		{
			Array.Sort(array, (RaycastHit r1, RaycastHit r2) => r1.distance.CompareTo(r2.distance));
		}
		if (array.Length != 0)
		{
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				RaycastResult raycastResult = default(RaycastResult);
				raycastResult.gameObject = array[i].collider.gameObject;
				raycastResult.module = this;
				raycastResult.distance = array[i].distance;
				raycastResult.index = resultAppendList.Count;
				raycastResult.worldPosition = array[0].point;
				raycastResult.worldNormal = array[0].normal;
				RaycastResult item = raycastResult;
				resultAppendList.Add(item);
			}
		}
	}

	public Vector2 GetScreenPos(Vector3 worldPosition)
	{
		return eventCamera.WorldToScreenPoint(worldPosition);
	}
}
