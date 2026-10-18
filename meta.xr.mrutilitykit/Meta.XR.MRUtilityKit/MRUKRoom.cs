using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_m_r_u_k_room")]
[Feature(Feature.Scene)]
public class MRUKRoom : MonoBehaviour
{
	public struct CouchSeat
	{
		public MRUKAnchor couchAnchor { get; internal set; }

		public List<Pose> couchPoses { get; internal set; }
	}

	private struct Surface
	{
		public MRUKAnchor Anchor;

		public float UsableArea;

		public bool IsPlane;

		public Rect Bounds;

		public Matrix4x4 Transform;
	}

	private Bounds _roomBounds;

	private List<Vector3> _corners = new List<Vector3>();

	private Pose? _prevRoomPose;

	public OVRAnchor Anchor { get; internal set; } = OVRAnchor.Null;


	public bool IsLocal => Anchor.Handle != 0;

	internal Pose InitialPose { get; set; } = Pose.identity;


	internal Pose DeltaPose
	{
		get
		{
			Quaternion quaternion = base.transform.rotation * Quaternion.Inverse(InitialPose.rotation);
			return new Pose(base.transform.position - quaternion * InitialPose.position, quaternion);
		}
	}

	public List<MRUKAnchor> Anchors { get; } = new List<MRUKAnchor>();


	public List<MRUKAnchor> WallAnchors { get; } = new List<MRUKAnchor>();


	public MRUKAnchor FloorAnchor { get; internal set; }

	public MRUKAnchor CeilingAnchor { get; internal set; }

	public MRUKAnchor GlobalMeshAnchor { get; internal set; }

	public List<CouchSeat> SeatPoses { get; } = new List<CouchSeat>();


	public UnityEvent<MRUKAnchor> AnchorCreatedEvent { get; private set; } = new UnityEvent<MRUKAnchor>();


	public UnityEvent<MRUKAnchor> AnchorUpdatedEvent { get; private set; } = new UnityEvent<MRUKAnchor>();


	public UnityEvent<MRUKAnchor> AnchorRemovedEvent { get; private set; } = new UnityEvent<MRUKAnchor>();


	[Obsolete("Use UnityEvent AnchorCreatedEvent directly instead")]
	public void RegisterAnchorCreatedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorCreatedEvent.AddListener(callback);
	}

	[Obsolete("Use UnityEvent AnchorUpdatedEvent directly instead")]
	public void RegisterAnchorUpdatedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorUpdatedEvent.AddListener(callback);
	}

	[Obsolete("Use UnityEvent AnchorRemovedEvent directly instead")]
	public void RegisterAnchorRemovedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorRemovedEvent.AddListener(callback);
	}

	[Obsolete("Use UnityEvent AnchorCreatedEvent directly instead")]
	public void UnRegisterAnchorCreatedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorCreatedEvent.RemoveListener(callback);
	}

	[Obsolete("Use UnityEvent AnchorUpdatedEvent directly instead")]
	public void UnRegisterAnchorUpdatedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorUpdatedEvent.RemoveListener(callback);
	}

	[Obsolete("Use UnityEvent AnchorRemovedEvent directly instead")]
	public void UnRegisterAnchorRemovedCallback(UnityAction<MRUKAnchor> callback)
	{
		AnchorRemovedEvent.RemoveListener(callback);
	}

	public async OVRTask<OVRResult<OVRAnchor.ShareResult>> ShareRoomAsync(Guid groupUuid)
	{
		if (Anchor == OVRAnchor.Null)
		{
			throw new InvalidOperationException("Anchor must not be Null");
		}
		if (Guid.Empty == groupUuid)
		{
			throw new ArgumentException("groupUuid");
		}
		if (Anchor.TryGetComponent<OVRSharable>(out var component))
		{
			await component.SetEnabledAsync(enabled: true);
		}
		return await Anchor.ShareAsync(groupUuid);
	}

	internal MRUKAnchor FindAnchorByUuid(Guid uuid)
	{
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (anchor.Anchor.Uuid == uuid)
			{
				return anchor;
			}
		}
		return null;
	}

	internal void ComputeRoomInfo()
	{
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (anchor.HasAnyLabel(MRUKAnchor.SceneLabels.GLOBAL_MESH))
			{
				GlobalMeshAnchor = anchor;
				break;
			}
		}
		CalculateSeatPoses();
		CalculateHierarchyReferences();
	}

	[Obsolete("Use Anchors property instead")]
	public List<MRUKAnchor> GetRoomAnchors()
	{
		return Anchors;
	}

	public void RemoveAndDestroyAnchor(MRUKAnchor anchor)
	{
		Anchors.Remove(anchor);
		WallAnchors.Remove(anchor);
		if (CeilingAnchor == anchor)
		{
			CeilingAnchor = null;
		}
		if (FloorAnchor == anchor)
		{
			FloorAnchor = null;
		}
		Utilities.DestroyGameObjectAndChildren(anchor.gameObject);
	}

	[Obsolete("Use FloorAnchor property instead")]
	public MRUKAnchor GetFloorAnchor()
	{
		return FloorAnchor;
	}

	[Obsolete("Use CeilingAnchor property instead")]
	public MRUKAnchor GetCeilingAnchor()
	{
		return CeilingAnchor;
	}

	public MRUKAnchor GetGlobalMeshAnchor()
	{
		return GlobalMeshAnchor;
	}

	[Obsolete("Use WallAnchors property instead")]
	public List<MRUKAnchor> GetWallAnchors()
	{
		return WallAnchors;
	}

	private void CalculateSeatPoses()
	{
		SeatPoses.Clear();
		float seatWidth = MRUK.Instance.SceneSettings.SeatWidth;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (!anchor.HasAnyLabel(MRUKAnchor.SceneLabels.COUCH))
			{
				continue;
			}
			CouchSeat couchSeat = default(CouchSeat);
			couchSeat.couchAnchor = anchor;
			couchSeat.couchPoses = new List<Pose>();
			CouchSeat item = couchSeat;
			Vector2 vector = anchor.PlaneRect?.size ?? Vector2.one;
			float num = vector.x / vector.y;
			Vector3 normal = GetFacingDirection(anchor);
			Vector3 tangent = Vector3.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Quaternion quaternion = Quaternion.Inverse(anchor.transform.rotation);
			if (num < 2f && num > 0.5f)
			{
				Pose item2 = new Pose(Vector3.zero, quaternion * Quaternion.LookRotation(normal, tangent));
				item.couchPoses.Add(item2);
				SeatPoses.Add(item);
				continue;
			}
			bool flag = vector.x > vector.y;
			float num2 = (flag ? vector.x : vector.y);
			float num3 = Mathf.Floor(num2 / seatWidth);
			float num4 = (num2 - num3 * seatWidth) / num3;
			for (int i = 0; (float)i < num3; i++)
			{
				Vector3 vector2 = (flag ? anchor.transform.right : anchor.transform.up);
				Vector3 zero = Vector3.zero;
				zero -= vector2 * num2 * 0.5f;
				zero += vector2 * num4 * 0.5f;
				zero += vector2 * seatWidth * 0.5f;
				zero += vector2 * seatWidth * i;
				zero += vector2 * num4 * i;
				Pose item3 = new Pose(quaternion * zero, quaternion * Quaternion.LookRotation(normal, tangent));
				item.couchPoses.Add(item3);
				SeatPoses.Add(item);
			}
		}
	}

	public List<Vector3> GetRoomOutline()
	{
		CalculateRoomOutlineAndBounds();
		return _corners;
	}

	public MRUKAnchor GetKeyWall(out Vector2 wallScale, float tolerance = 0.1f)
	{
		wallScale = Vector3.one;
		List<MRUKAnchor> walls = new List<MRUKAnchor>(WallAnchors);
		MRUKAnchor result = null;
		walls = SortWallsByWidth(walls);
		List<Vector3> roomOutline = GetRoomOutline();
		for (int num = walls.Count - 1; num >= 0; num--)
		{
			bool flag = true;
			for (int i = 0; i < roomOutline.Count; i++)
			{
				Vector3 rhs = roomOutline[i] - walls[num].transform.position;
				rhs += walls[num].transform.forward * tolerance;
				flag &= Vector3.Dot(walls[num].transform.forward, rhs) >= 0f;
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				wallScale = walls[num].PlaneRect.Value.size;
				result = walls[num];
				break;
			}
		}
		return result;
	}

	public static List<MRUKAnchor> SortWallsByWidth(List<MRUKAnchor> walls)
	{
		List<MRUKAnchor> list = new List<MRUKAnchor>();
		for (int i = 0; i < walls.Count; i++)
		{
			for (int j = i + 1; j < walls.Count; j++)
			{
				if (walls[i].PlaneRect.Value.size.x > walls[j].PlaneRect.Value.size.x)
				{
					int index = i;
					int index2 = j;
					MRUKAnchor mRUKAnchor = walls[j];
					MRUKAnchor mRUKAnchor2 = walls[i];
					MRUKAnchor mRUKAnchor4 = (walls[index] = mRUKAnchor);
					mRUKAnchor4 = (walls[index2] = mRUKAnchor2);
				}
			}
		}
		list.AddRange(walls);
		return list;
	}

	public bool RaycastAll(Ray ray, float maxDist, LabelFilter labelFilter, List<RaycastHit> raycastHits, List<MRUKAnchor> anchorList)
	{
		raycastHits.Clear();
		anchorList.Clear();
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (labelFilter.PassesFilter(anchor.Label) && anchor.Raycast(ray, maxDist, out var hitInfo, labelFilter.ComponentTypes ?? MRUKAnchor.ComponentType.All))
			{
				raycastHits.Add(hitInfo);
				anchorList.Add(anchor);
			}
		}
		return raycastHits.Count > 0;
	}

	public bool Raycast(Ray ray, float maxDist, LabelFilter labelFilter, out RaycastHit hit, out MRUKAnchor outAnchor)
	{
		hit = default(RaycastHit);
		outAnchor = null;
		bool result = false;
		float maxDist2 = maxDist;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (labelFilter.PassesFilter(anchor.Label) && anchor.Raycast(ray, maxDist2, out var hitInfo, labelFilter.ComponentTypes ?? MRUKAnchor.ComponentType.All))
			{
				maxDist2 = hitInfo.distance;
				hit = hitInfo;
				outAnchor = anchor;
				result = true;
			}
		}
		return result;
	}

	public bool Raycast(Ray ray, float maxDist, out RaycastHit hit, out MRUKAnchor anchor)
	{
		return Raycast(ray, maxDist, default(LabelFilter), out hit, out anchor);
	}

	public bool Raycast(Ray ray, float maxDist, LabelFilter labelFilter, out RaycastHit hit)
	{
		MRUKAnchor outAnchor;
		return Raycast(ray, maxDist, labelFilter, out hit, out outAnchor);
	}

	public bool Raycast(Ray ray, float maxDist, out RaycastHit hit)
	{
		MRUKAnchor outAnchor;
		return Raycast(ray, maxDist, default(LabelFilter), out hit, out outAnchor);
	}

	public Pose GetBestPoseFromRaycast(Ray ray, float maxDist, LabelFilter labelFilter, out MRUKAnchor sceneAnchor, out Vector3 surfaceNormal, MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT)
	{
		sceneAnchor = null;
		Pose result = default(Pose);
		surfaceNormal = Vector3.up;
		if (Raycast(ray, maxDist, labelFilter, out var hit, out sceneAnchor))
		{
			Vector3 position = hit.point;
			surfaceNormal = hit.normal;
			Vector3 up = Vector3.up;
			Vector3 vector = hit.normal;
			if (Vector3.Dot(hit.normal, Vector3.up) >= 0.9f && sceneAnchor.VolumeBounds.HasValue)
			{
				Vector3 tangent = ray.origin - sceneAnchor.transform.position;
				Vector3 vector2 = ((Vector3.Dot(sceneAnchor.transform.up, tangent) > 0f) ? sceneAnchor.transform.up : (-sceneAnchor.transform.up));
				Vector3 vector3 = ((Vector3.Dot(sceneAnchor.transform.right, tangent) > 0f) ? sceneAnchor.transform.right : (-sceneAnchor.transform.right));
				Vector3 normal = sceneAnchor.transform.forward;
				Vector2 vector4 = sceneAnchor.VolumeBounds.Value.size;
				Vector3 from = sceneAnchor.transform.position + vector3 * vector4.x * 0.5f + vector2 * vector4.y * 0.5f;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				from -= sceneAnchor.transform.position;
				bool num = Vector3.Angle(tangent, vector2) > Vector3.Angle(from, vector2);
				vector = (num ? vector3 : vector2);
				float num2 = (num ? vector4.x : vector4.y);
				switch (positioningMethod)
				{
				case MRUK.PositioningMethod.CENTER:
					position = sceneAnchor.transform.position;
					break;
				case MRUK.PositioningMethod.EDGE:
					position = sceneAnchor.transform.position + vector * num2 * 0.5f;
					break;
				}
			}
			else if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) >= 0.9f)
			{
				vector = new Vector3(ray.origin.x - hit.point.x, 0f, ray.origin.z - hit.point.z).normalized;
			}
			result.position = position;
			result.rotation = Quaternion.LookRotation(vector, up);
		}
		else
		{
			Debug.Log("Best pose not found, no surface anchor detected.");
		}
		return result;
	}

	public Pose GetBestPoseFromRaycast(Ray ray, float maxDist, LabelFilter labelFilter, out MRUKAnchor sceneAnchor, MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT)
	{
		Vector3 surfaceNormal;
		return GetBestPoseFromRaycast(ray, maxDist, labelFilter, out sceneAnchor, out surfaceNormal, positioningMethod);
	}

	public bool IsPositionInRoom(Vector3 queryPosition, bool testVerticalBounds = true)
	{
		if (FloorAnchor == null)
		{
			return false;
		}
		bool flag = false;
		Vector3 vector = FloorAnchor.transform.InverseTransformPoint(queryPosition);
		flag |= FloorAnchor.IsPositionInBoundary(vector);
		if (testVerticalBounds)
		{
			Bounds roomBounds = GetRoomBounds();
			flag &= TestVerticalBounds(queryPosition, roomBounds);
		}
		return flag;
	}

	private bool TestVerticalBounds(Vector3 queryPosition, Bounds roomBounds)
	{
		if (queryPosition.y <= roomBounds.max.y)
		{
			return queryPosition.y >= roomBounds.min.y;
		}
		return false;
	}

	public Bounds GetRoomBounds()
	{
		CalculateRoomOutlineAndBounds();
		return _roomBounds;
	}

	private void CalculateRoomOutlineAndBounds()
	{
		if (!FloorAnchor || !CeilingAnchor)
		{
			Debug.LogWarning("Floor or Ceiling anchor not found");
			return;
		}
		Pose? prevRoomPose = _prevRoomPose;
		if (prevRoomPose.HasValue)
		{
			Pose valueOrDefault = prevRoomPose.GetValueOrDefault();
			if (base.transform.position == valueOrDefault.position && base.transform.rotation == valueOrDefault.rotation)
			{
				return;
			}
		}
		_prevRoomPose = new Pose(base.transform.position, base.transform.rotation);
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = float.PositiveInfinity;
		float num6 = float.NegativeInfinity;
		_corners.Clear();
		foreach (Vector2 item2 in FloorAnchor.PlaneBoundary2D)
		{
			Vector3 item = FloorAnchor.transform.TransformPoint(new Vector3(item2.x, item2.y, 0f));
			num = Mathf.Min(num, item.x);
			num2 = Mathf.Max(num2, item.x);
			num3 = Mathf.Min(num3, item.y);
			num4 = Mathf.Max(num4, item.y);
			num5 = Mathf.Min(num5, item.z);
			num6 = Mathf.Max(num6, item.z);
			_corners.Add(item);
		}
		foreach (Vector2 item3 in CeilingAnchor.PlaneBoundary2D)
		{
			Vector3 vector = CeilingAnchor.transform.TransformPoint(new Vector3(item3.x, item3.y, 0f));
			num = Mathf.Min(num, vector.x);
			num2 = Mathf.Max(num2, vector.x);
			num3 = Mathf.Min(num3, vector.y);
			num4 = Mathf.Max(num4, vector.y);
			num5 = Mathf.Min(num5, vector.z);
			num6 = Mathf.Max(num6, vector.z);
		}
		_roomBounds.center = new Vector3((num2 + num) * 0.5f, (num4 + num3) * 0.5f, (num6 + num5) * 0.5f);
		_roomBounds.size = new Vector3(num2 - num, num4 - num3, num6 - num5);
	}

	public bool IsPositionInSceneVolume(Vector3 worldPosition, out MRUKAnchor sceneObject, bool testVerticalBounds, float distanceBuffer = 0f)
	{
		bool result = false;
		sceneObject = null;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (anchor.IsPositionInVolume(worldPosition, testVerticalBounds, distanceBuffer))
			{
				result = true;
				sceneObject = anchor;
				break;
			}
		}
		return result;
	}

	public Vector3 GetFacingDirection(MRUKAnchor anchor)
	{
		if (!anchor.VolumeBounds.HasValue)
		{
			return anchor.transform.forward;
		}
		int cardinalAxisIndex;
		return GetDirectionAwayFromClosestWall(anchor, out cardinalAxisIndex);
	}

	internal Vector3 GetDirectionAwayFromClosestWall(MRUKAnchor anchor, out int cardinalAxisIndex, List<int> excludedAxes = null)
	{
		float maxDist = float.PositiveInfinity;
		Vector3 result = anchor.transform.up;
		cardinalAxisIndex = 0;
		for (int i = 0; i < 4; i++)
		{
			if (excludedAxes != null && excludedAxes.Contains(i))
			{
				continue;
			}
			Vector3 vector = Quaternion.Euler(0f, 90f * (float)i, 0f) * -anchor.transform.up;
			foreach (MRUKAnchor wallAnchor in WallAnchors)
			{
				if (wallAnchor.Raycast(new Ray(anchor.transform.position, vector), maxDist, out var hitInfo))
				{
					maxDist = hitInfo.distance;
					cardinalAxisIndex = i;
					result = -vector;
				}
			}
		}
		return result;
	}

	public bool IsPositionInSceneVolume(Vector3 worldPosition, float distanceBuffer = 0f)
	{
		MRUKAnchor sceneObject;
		return IsPositionInSceneVolume(worldPosition, out sceneObject, testVerticalBounds: true, distanceBuffer);
	}

	public bool IsPositionInSceneVolume(Vector3 worldPosition, bool testVerticalBounds, float distanceBuffer = 0f)
	{
		MRUKAnchor sceneObject;
		return IsPositionInSceneVolume(worldPosition, out sceneObject, testVerticalBounds, distanceBuffer);
	}

	public bool TryGetClosestSeatPose(Ray ray, out Pose seatPose, out MRUKAnchor couch)
	{
		Pose pose = default(Pose);
		couch = null;
		float num = -1f;
		for (int i = 0; i < SeatPoses.Count; i++)
		{
			Quaternion rotation = SeatPoses[i].couchAnchor.transform.rotation;
			Vector3 position = SeatPoses[i].couchAnchor.transform.position;
			for (int j = 0; j < SeatPoses[i].couchPoses.Count; j++)
			{
				Vector3 vector = position + rotation * SeatPoses[i].couchPoses[j].position;
				Vector3 normalized = (vector - ray.origin).normalized;
				float num2 = Vector3.Dot(ray.direction, normalized);
				if (num2 > num)
				{
					num = num2;
					pose.position = vector;
					pose.rotation = rotation * SeatPoses[i].couchPoses[j].rotation;
					couch = SeatPoses[i].couchAnchor;
				}
			}
		}
		seatPose.position = pose.position;
		seatPose.rotation = pose.rotation;
		return SeatPoses.Count > 0;
	}

	public Pose[] GetSeatPoses()
	{
		List<Pose> list = new List<Pose>();
		for (int i = 0; i < SeatPoses.Count; i++)
		{
			Quaternion rotation = SeatPoses[i].couchAnchor.transform.rotation;
			Vector3 position = SeatPoses[i].couchAnchor.transform.position;
			for (int j = 0; j < SeatPoses[i].couchPoses.Count; j++)
			{
				Pose item = new Pose(position + rotation * SeatPoses[i].couchPoses[j].position, rotation * SeatPoses[i].couchPoses[j].rotation);
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	[Obsolete("Use ParentAnchor property instead")]
	public bool TryGetAnchorParent(MRUKAnchor queryAnchor, out MRUKAnchor parentAnchor)
	{
		parentAnchor = queryAnchor.ParentAnchor;
		return parentAnchor != null;
	}

	[Obsolete("Use ChildAnchors property instead")]
	public bool TryGetAnchorChildren(MRUKAnchor queryAnchor, out MRUKAnchor[] childAnchors)
	{
		childAnchors = queryAnchor.ChildAnchors?.ToArray();
		if (childAnchors != null)
		{
			return childAnchors.Length != 0;
		}
		return false;
	}

	private void CalculateHierarchyReferences()
	{
		for (int i = 0; i < Anchors.Count; i++)
		{
			Anchors[i].ClearChildReferences();
			Anchors[i].ParentAnchor = null;
		}
		for (int j = 0; j < Anchors.Count; j++)
		{
			if (Anchors[j].HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE) && Anchors[j].PlaneRect.HasValue)
			{
				for (int k = 0; k < Anchors.Count; k++)
				{
					if (!(Anchors[k] == Anchors[j]) && Anchors[k].PlaneRect.HasValue && !Anchors[k].VolumeBounds.HasValue)
					{
						bool num = Vector3.Angle(Anchors[k].transform.right, Anchors[j].transform.right) <= 5f;
						Vector3 vector = Anchors[j].transform.InverseTransformPoint(Anchors[k].transform.position);
						bool flag = Mathf.Abs(vector.z) <= 0.1f;
						bool flag2 = vector.x <= Anchors[j].PlaneRect.Value.max.x && vector.x >= Anchors[j].PlaneRect.Value.min.x;
						if (num && flag && flag2)
						{
							Anchors[j].AddChildReference(Anchors[k]);
							Anchors[k].ParentAnchor = Anchors[j];
						}
					}
				}
			}
			else if (Anchors[j].HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
			{
				for (int l = 0; l < Anchors.Count; l++)
				{
					if (Anchors[l].VolumeBounds.HasValue && (Anchors[l].transform.position + Vector3.up * Anchors[l].VolumeBounds.Value.min.z).y - Anchors[j].transform.position.y <= 0.1f)
					{
						Anchors[j].AddChildReference(Anchors[l]);
						Anchors[l].ParentAnchor = Anchors[j];
					}
				}
			}
			else
			{
				if (!Anchors[j].VolumeBounds.HasValue)
				{
					continue;
				}
				Bounds value = Anchors[j].VolumeBounds.Value;
				for (int m = 0; m < Anchors.Count; m++)
				{
					if (Anchors[m] == Anchors[j] || !Anchors[m].VolumeBounds.HasValue)
					{
						continue;
					}
					Bounds value2 = Anchors[m].VolumeBounds.Value;
					Vector3 vector2 = Anchors[m].transform.position + Vector3.up * Anchors[m].VolumeBounds.Value.min.z;
					Vector3 vector3 = Anchors[j].transform.position + Vector3.up * Anchors[j].VolumeBounds.Value.max.z;
					if (!(Mathf.Abs(vector2.y - vector3.y) <= 0.1f))
					{
						continue;
					}
					bool flag3 = false;
					for (int n = 0; n < 4; n++)
					{
						Vector3 position = new Vector3((n < 2) ? value2.min.x : value2.max.x, (n % 2 == 0) ? value2.min.y : value2.max.y, 0f);
						position = Anchors[m].transform.TransformPoint(position);
						Vector3 vector4 = Anchors[j].transform.InverseTransformPoint(position);
						bool num2 = 0.001f + (vector4.x - value.min.x) >= 0f;
						bool flag4 = 0.001f + (value.max.x - vector4.x) >= 0f;
						bool flag5 = 0.001f + (vector4.y - value.min.y) >= 0f;
						bool flag6 = 0.001f + (value.max.y - vector4.y) >= 0f;
						if (num2 && flag4 && flag5 && flag6)
						{
							flag3 = true;
							break;
						}
					}
					if (flag3 && (!(Anchors[m].ParentAnchor != null) || !Anchors[m].ParentAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR)))
					{
						Anchors[j].AddChildReference(Anchors[m]);
						Anchors[m].ParentAnchor = Anchors[j];
					}
				}
			}
		}
	}

	[Obsolete("Use 'HasAllLabels()' instead.")]
	public bool DoesRoomHave(string[] labels)
	{
		return HasAllLabels(Utilities.StringLabelsToEnum(labels));
	}

	public bool HasAllLabels(MRUKAnchor.SceneLabels labelFlags)
	{
		foreach (MRUKAnchor anchor in Anchors)
		{
			labelFlags &= ~anchor.Label;
			if (labelFlags == (MRUKAnchor.SceneLabels)0)
			{
				return true;
			}
		}
		return false;
	}

	public float TryGetClosestSurfacePosition(Vector3 worldPosition, out Vector3 surfacePosition, out MRUKAnchor closestAnchor, LabelFilter labelFilter = default(LabelFilter))
	{
		Vector3 normal;
		return TryGetClosestSurfacePosition(worldPosition, out surfacePosition, out closestAnchor, out normal, labelFilter);
	}

	public float TryGetClosestSurfacePosition(Vector3 worldPosition, out Vector3 surfacePosition, out MRUKAnchor closestAnchor, out Vector3 normal, LabelFilter labelFilter = default(LabelFilter))
	{
		float num = float.PositiveInfinity;
		surfacePosition = Vector3.zero;
		closestAnchor = null;
		normal = Vector3.zero;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (labelFilter.PassesFilter(anchor.Label))
			{
				Vector3 closestPosition;
				Vector3 normal2;
				float closestSurfacePosition = anchor.GetClosestSurfacePosition(worldPosition, out closestPosition, out normal2, labelFilter.ComponentTypes ?? MRUKAnchor.ComponentType.All);
				if (closestSurfacePosition < num)
				{
					num = closestSurfacePosition;
					surfacePosition = closestPosition;
					normal = normal2;
					closestAnchor = anchor.GetComponent<MRUKAnchor>();
				}
			}
		}
		return num;
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public MRUKAnchor FindLargestSurface(string anchorLabel)
	{
		return FindLargestSurface(Utilities.StringLabelToEnum(anchorLabel));
	}

	public MRUKAnchor FindLargestSurface(MRUKAnchor.SceneLabels labelFlags)
	{
		MRUKAnchor result = null;
		float num = 0f;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (anchor.HasAnyLabel(labelFlags))
			{
				float num2 = 0f;
				if (anchor.PlaneRect.HasValue)
				{
					Vector2 size = anchor.PlaneRect.Value.size;
					num2 = size.x * size.y;
				}
				else if (anchor.VolumeBounds.HasValue)
				{
					Vector3 size2 = anchor.VolumeBounds.Value.size;
					num2 = size2.x * size2.y;
				}
				if (num2 > num)
				{
					num = num2;
					result = anchor;
				}
			}
		}
		return result;
	}

	public Vector3? GenerateRandomPositionInRoom(float minDistanceToSurface, bool avoidVolumes)
	{
		if (!FloorAnchor)
		{
			return null;
		}
		Vector3 extents = GetRoomBounds().extents;
		float num = Mathf.Min(extents.x, extents.y, extents.z);
		if (minDistanceToSurface > num)
		{
			return null;
		}
		for (int i = 0; i < 1000; i++)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(_roomBounds.min.x + minDistanceToSurface, _roomBounds.max.x - minDistanceToSurface), UnityEngine.Random.Range(_roomBounds.min.y + minDistanceToSurface, _roomBounds.max.y - minDistanceToSurface), UnityEngine.Random.Range(_roomBounds.min.z + minDistanceToSurface, _roomBounds.max.z - minDistanceToSurface));
			if (IsPositionInRoom(vector))
			{
				LabelFilter labelFilter = new LabelFilter(MRUKAnchor.SceneLabels.WALL_FACE);
				if (!(TryGetClosestSurfacePosition(vector, out var _, out var _, labelFilter) <= minDistanceToSurface) && (!avoidVolumes || !IsPositionInSceneVolume(vector, minDistanceToSurface)))
				{
					return vector;
				}
			}
		}
		return null;
	}

	public bool GenerateRandomPositionOnSurface(MRUK.SurfaceType surfaceTypes, float minDistanceToEdge, LabelFilter labelFilter, out Vector3 position, out Vector3 normal)
	{
		List<Surface> list = new List<Surface>();
		float num = 0f;
		float num2 = 2f * minDistanceToEdge;
		position = Vector3.zero;
		normal = Vector3.zero;
		foreach (MRUKAnchor anchor in Anchors)
		{
			if (!labelFilter.PassesFilter(anchor.Label))
			{
				continue;
			}
			if (anchor.PlaneRect.HasValue)
			{
				bool flag = false;
				if (anchor.transform.forward.y >= Utilities.InvSqrt2)
				{
					if ((surfaceTypes & MRUK.SurfaceType.FACING_UP) == 0)
					{
						flag = true;
					}
				}
				else if (anchor.transform.forward.y <= 0f - Utilities.InvSqrt2)
				{
					if ((surfaceTypes & MRUK.SurfaceType.FACING_DOWN) == 0)
					{
						flag = true;
					}
				}
				else if ((surfaceTypes & MRUK.SurfaceType.VERTICAL) == 0)
				{
					flag = true;
				}
				if (!flag)
				{
					Vector2 size = anchor.PlaneRect.Value.size;
					if (size.x > num2 && size.y > num2)
					{
						float num3 = (size.x - num2) * (size.y - num2);
						num += num3;
						list.Add(new Surface
						{
							Anchor = anchor,
							UsableArea = num3,
							IsPlane = true,
							Bounds = anchor.PlaneRect.Value,
							Transform = anchor.transform.localToWorldMatrix
						});
					}
				}
			}
			if (!anchor.VolumeBounds.HasValue)
			{
				continue;
			}
			for (int i = 0; i < 6; i++)
			{
				switch (i)
				{
				case 0:
					if ((surfaceTypes & MRUK.SurfaceType.FACING_UP) == 0)
					{
						continue;
					}
					break;
				case 1:
					if ((surfaceTypes & MRUK.SurfaceType.FACING_DOWN) == 0)
					{
						continue;
					}
					break;
				default:
					if ((surfaceTypes & MRUK.SurfaceType.VERTICAL) == 0)
					{
						continue;
					}
					break;
				}
				Rect bounds;
				Matrix4x4 matrix4x;
				switch (i)
				{
				case 0:
				{
					Rect rect = default(Rect);
					rect.xMin = anchor.VolumeBounds.Value.min.x;
					rect.xMax = anchor.VolumeBounds.Value.max.x;
					rect.yMin = anchor.VolumeBounds.Value.min.y;
					rect.yMax = anchor.VolumeBounds.Value.max.y;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(0f, 0f, anchor.VolumeBounds.Value.max.z), Quaternion.identity, Vector3.one);
					break;
				}
				case 1:
				{
					Rect rect = default(Rect);
					rect.xMin = 0f - anchor.VolumeBounds.Value.max.x;
					rect.xMax = 0f - anchor.VolumeBounds.Value.min.x;
					rect.yMin = anchor.VolumeBounds.Value.min.y;
					rect.yMax = anchor.VolumeBounds.Value.max.y;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(0f, 0f, anchor.VolumeBounds.Value.min.z), Quaternion.Euler(0f, 180f, 0f), Vector3.one);
					break;
				}
				case 2:
				{
					Rect rect = default(Rect);
					rect.xMin = 0f - anchor.VolumeBounds.Value.max.z;
					rect.xMax = 0f - anchor.VolumeBounds.Value.min.z;
					rect.yMin = anchor.VolumeBounds.Value.min.y;
					rect.yMax = anchor.VolumeBounds.Value.max.y;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(anchor.VolumeBounds.Value.max.x, 0f, 0f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
					break;
				}
				case 3:
				{
					Rect rect = default(Rect);
					rect.xMin = anchor.VolumeBounds.Value.min.z;
					rect.xMax = anchor.VolumeBounds.Value.max.z;
					rect.yMin = anchor.VolumeBounds.Value.min.y;
					rect.yMax = anchor.VolumeBounds.Value.max.y;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(anchor.VolumeBounds.Value.min.x, 0f, 0f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
					break;
				}
				case 4:
				{
					Rect rect = default(Rect);
					rect.xMin = anchor.VolumeBounds.Value.min.x;
					rect.xMax = anchor.VolumeBounds.Value.max.x;
					rect.yMin = 0f - anchor.VolumeBounds.Value.max.z;
					rect.yMax = 0f - anchor.VolumeBounds.Value.min.z;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(0f, anchor.VolumeBounds.Value.max.y, 0f), Quaternion.Euler(-90f, 0f, 0f), Vector3.one);
					break;
				}
				case 5:
				{
					Rect rect = default(Rect);
					rect.xMin = anchor.VolumeBounds.Value.min.x;
					rect.xMax = anchor.VolumeBounds.Value.max.x;
					rect.yMin = anchor.VolumeBounds.Value.min.z;
					rect.yMax = anchor.VolumeBounds.Value.max.z;
					bounds = rect;
					matrix4x = Matrix4x4.TRS(new Vector3(0f, anchor.VolumeBounds.Value.min.y, 0f), Quaternion.Euler(90f, 0f, 0f), Vector3.one);
					break;
				}
				default:
					throw new SwitchExpressionException();
				}
				Vector2 size2 = bounds.size;
				if (size2.x > num2 && size2.y > num2)
				{
					float num4 = (size2.x - num2) * (size2.y - num2);
					num += num4;
					list.Add(new Surface
					{
						Anchor = anchor,
						UsableArea = num4,
						IsPlane = false,
						Bounds = bounds,
						Transform = anchor.transform.localToWorldMatrix * matrix4x
					});
				}
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		for (int j = 0; j < 1000; j++)
		{
			float num5 = UnityEngine.Random.Range(0f, num);
			int k;
			for (k = 0; k < list.Count - 1; k++)
			{
				num5 -= list[k].UsableArea;
				if (num5 <= 0f)
				{
					break;
				}
			}
			Surface surface = list[k];
			Rect bounds2 = surface.Bounds;
			Vector2 position2 = new Vector2(UnityEngine.Random.Range(bounds2.xMin + minDistanceToEdge, bounds2.xMax - minDistanceToEdge), UnityEngine.Random.Range(bounds2.yMin + minDistanceToEdge, bounds2.yMax - minDistanceToEdge));
			if (!surface.IsPlane || surface.Anchor.IsPositionInBoundary(position2))
			{
				position = surface.Transform.MultiplyPoint3x4(new Vector3(position2.x, position2.y, 0f));
				normal = surface.Transform.MultiplyVector(Vector3.forward);
				return true;
			}
		}
		return false;
	}

	private void OnDestroy()
	{
		MRUK.Instance?.OnRoomDestroyed(this);
		AnchorCreatedEvent.RemoveAllListeners();
		AnchorRemovedEvent.RemoveAllListeners();
		AnchorUpdatedEvent.RemoveAllListeners();
	}
}
