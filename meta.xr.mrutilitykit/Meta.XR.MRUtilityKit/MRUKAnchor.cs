using System;
using System.Collections.Generic;
using Meta.XR.Util;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_m_r_u_k_anchor")]
[Feature(Feature.Scene)]
public class MRUKAnchor : MonoBehaviour
{
	[Flags]
	public enum SceneLabels
	{
		FLOOR = 1,
		CEILING = 2,
		WALL_FACE = 4,
		TABLE = 8,
		COUCH = 0x10,
		DOOR_FRAME = 0x20,
		WINDOW_FRAME = 0x40,
		OTHER = 0x80,
		STORAGE = 0x100,
		BED = 0x200,
		SCREEN = 0x400,
		LAMP = 0x800,
		PLANT = 0x1000,
		WALL_ART = 0x2000,
		GLOBAL_MESH = 0x4000,
		INVISIBLE_WALL_FACE = 0x8000
	}

	[Flags]
	public enum ComponentType
	{
		None = 0,
		Plane = 1,
		Volume = 2,
		All = 3
	}

	private Mesh _mesh;

	[Obsolete("Use 'Label' instead.")]
	public List<string> AnchorLabels => Utilities.SceneLabelsEnumToList(Label);

	internal Pose InitialPose { get; set; } = Pose.identity;


	internal Pose DeltaPose
	{
		get
		{
			Quaternion quaternion = base.transform.rotation * Quaternion.Inverse(InitialPose.rotation);
			return new Pose(base.transform.position - quaternion * InitialPose.position, quaternion);
		}
	}

	public SceneLabels Label { get; internal set; }

	public Rect? PlaneRect { get; internal set; }

	public Bounds? VolumeBounds { get; internal set; }

	public List<Vector2> PlaneBoundary2D { get; internal set; }

	public OVRAnchor Anchor { get; internal set; } = OVRAnchor.Null;


	public MRUKRoom Room { get; internal set; }

	public MRUKAnchor ParentAnchor { get; internal set; }

	public List<MRUKAnchor> ChildAnchors { get; internal set; } = new List<MRUKAnchor>();


	[Obsolete("Use PlaneRect.HasValue instead.")]
	public bool HasPlane => PlaneRect.HasValue;

	[Obsolete("Use VolumeBounds.HasValue instead.")]
	public bool HasVolume => VolumeBounds.HasValue;

	[Obsolete("Use HasValidHandle instead.")]
	public bool IsLocal => HasValidHandle;

	public bool HasValidHandle => Anchor.Handle != 0;

	internal Mesh Mesh
	{
		get
		{
			return GlobalMesh;
		}
		set
		{
			_mesh = value;
		}
	}

	public Mesh GlobalMesh
	{
		get
		{
			if (!_mesh)
			{
				_mesh = LoadGlobalMeshTriangles();
			}
			return _mesh;
		}
		private set
		{
			_mesh = value;
		}
	}

	public bool Raycast(Ray ray, float maxDist, out RaycastHit hitInfo, ComponentType componentTypes = ComponentType.All)
	{
		Vector3 origin = base.transform.InverseTransformPoint(ray.origin);
		Vector3 direction = base.transform.InverseTransformDirection(ray.direction);
		Ray localRay = new Ray(origin, direction);
		RaycastHit hitInfo2 = default(RaycastHit);
		RaycastHit hitInfo3 = default(RaycastHit);
		bool flag = (componentTypes & ComponentType.Plane) != 0 && RaycastPlane(localRay, maxDist, out hitInfo2);
		bool flag2 = (componentTypes & ComponentType.Volume) != 0 && RaycastVolume(localRay, maxDist, out hitInfo3);
		if (flag && flag2)
		{
			if (hitInfo2.distance < hitInfo3.distance)
			{
				hitInfo = hitInfo2;
				return true;
			}
			hitInfo = hitInfo3;
			return true;
		}
		if (flag)
		{
			hitInfo = hitInfo2;
			return true;
		}
		if (flag2)
		{
			hitInfo = hitInfo3;
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public bool IsPositionInBoundary(Vector2 position)
	{
		if (PlaneBoundary2D == null || PlaneBoundary2D.Count == 0)
		{
			return false;
		}
		return Utilities.IsPositionInPolygon(position, PlaneBoundary2D);
	}

	public void AddChildReference(MRUKAnchor childObj)
	{
		if (childObj != null)
		{
			ChildAnchors.Add(childObj);
		}
	}

	public void ClearChildReferences()
	{
		ChildAnchors.Clear();
	}

	public float GetDistanceToSurface(Vector3 position, ComponentType componentTypes = ComponentType.All)
	{
		Vector3 closestPosition;
		Vector3 normal;
		return GetClosestSurfacePosition(position, out closestPosition, out normal, componentTypes);
	}

	public float GetClosestSurfacePosition(Vector3 testPosition, out Vector3 closestPosition, ComponentType componentTypes = ComponentType.All)
	{
		Vector3 normal;
		return GetClosestSurfacePosition(testPosition, out closestPosition, out normal, componentTypes);
	}

	public float GetClosestSurfacePosition(Vector3 testPosition, out Vector3 closestPosition, out Vector3 normal, ComponentType componentTypes = ComponentType.All)
	{
		float result = float.PositiveInfinity;
		closestPosition = Vector3.zero;
		normal = Vector3.zero;
		if ((componentTypes & ComponentType.Volume) != 0 && VolumeBounds.HasValue)
		{
			Bounds value = VolumeBounds.Value;
			Vector3 vector = base.transform.InverseTransformPoint(testPosition);
			if (value.Contains(vector))
			{
				vector -= value.center;
				float num = float.MaxValue;
				int num2 = -1;
				for (int i = 0; i < 3; i++)
				{
					float num3 = value.extents[i] - Mathf.Abs(vector[i]);
					if (num3 < num)
					{
						num = num3;
						num2 = i;
					}
				}
				float num4 = Mathf.Sign(vector[num2]);
				vector[num2] += num * num4;
				result = 0f - num;
				Vector3 vector2 = default(Vector3);
				int index = num2;
				vector2[index] = num4;
				Vector3 direction = vector2;
				normal = base.transform.TransformDirection(direction);
				closestPosition = base.transform.TransformPoint(vector + value.center);
			}
			else
			{
				closestPosition = value.ClosestPoint(vector);
				Vector3 vector3 = vector - closestPosition;
				if (Mathf.Abs(vector3.x) > Mathf.Abs(vector3.y) && Mathf.Abs(vector3.x) > Mathf.Abs(vector3.z))
				{
					normal = new Vector3(Mathf.Sign(vector3.x), 0f, 0f);
				}
				else if (Mathf.Abs(vector3.y) > Mathf.Abs(vector3.z))
				{
					normal = new Vector3(0f, Mathf.Sign(vector3.y), 0f);
				}
				else
				{
					normal = new Vector3(0f, 0f, Mathf.Sign(vector3.z));
				}
				closestPosition = base.transform.TransformPoint(closestPosition);
				normal = base.transform.TransformDirection(normal);
				result = Vector3.Distance(closestPosition, testPosition);
			}
		}
		else if ((componentTypes & ComponentType.Plane) != 0 && PlaneRect.HasValue)
		{
			Rect value2 = PlaneRect.Value;
			Vector3 position = base.transform.InverseTransformPoint(testPosition);
			position.z = 0f;
			if (position.x > value2.max.x)
			{
				position.x = value2.max.x;
			}
			else if (position.x < value2.min.x)
			{
				position.x = value2.min.x;
			}
			if (position.y > value2.max.y)
			{
				position.y = value2.max.y;
			}
			else if (position.y < value2.min.y)
			{
				position.y = value2.min.y;
			}
			closestPosition = base.transform.TransformPoint(position);
			result = Vector3.Distance(closestPosition, testPosition);
			normal = base.transform.forward;
		}
		return result;
	}

	public Vector3 GetAnchorCenter()
	{
		if (VolumeBounds.HasValue)
		{
			return base.transform.TransformPoint(VolumeBounds.Value.center);
		}
		return base.transform.position;
	}

	[Obsolete("Use PlaneRect and VolumeBounds properties instead")]
	public Vector3 GetAnchorSize()
	{
		Vector3 result = Vector3.one;
		if (HasPlane)
		{
			result = new Vector3(PlaneRect.Value.size.x, PlaneRect.Value.size.y, 1f);
		}
		if (HasVolume)
		{
			result = VolumeBounds.Value.size;
		}
		return result;
	}

	private bool RaycastPlane(Ray localRay, float maxDist, out RaycastHit hitInfo)
	{
		hitInfo = default(RaycastHit);
		if (!PlaneRect.HasValue)
		{
			return false;
		}
		if (localRay.direction.z >= 0f)
		{
			return false;
		}
		if (new Plane(Vector3.forward, 0f).Raycast(localRay, out var enter) && enter < maxDist)
		{
			Vector3 point = localRay.GetPoint(enter);
			if (IsPositionInBoundary(new Vector2(point.x, point.y)))
			{
				hitInfo.point = base.transform.TransformPoint(point);
				hitInfo.normal = base.transform.forward;
				hitInfo.distance = enter;
				return true;
			}
		}
		return false;
	}

	private bool RaycastVolume(Ray localRay, float maxDist, out RaycastHit hitInfo)
	{
		hitInfo = default(RaycastHit);
		if (!VolumeBounds.HasValue)
		{
			return false;
		}
		int index = 0;
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		Bounds value = VolumeBounds.Value;
		for (int i = 0; i < 3; i++)
		{
			if (Mathf.Abs(localRay.direction[i]) > Mathf.Epsilon)
			{
				float num3 = (value.min[i] - localRay.origin[i]) / localRay.direction[i];
				float num4 = (value.max[i] - localRay.origin[i]) / localRay.direction[i];
				if (num3 > num4)
				{
					float num5 = num4;
					float num6 = num3;
					num3 = num5;
					num4 = num6;
				}
				if (num3 > num2)
				{
					num2 = num3;
					index = i;
				}
				if (num4 < num)
				{
					num = num4;
				}
			}
			else if (localRay.origin[i] < value.min[i] || localRay.origin[i] > value.max[i])
			{
				num2 = float.PositiveInfinity;
				break;
			}
		}
		if (num2 >= 0f && num2 <= num && num2 < maxDist)
		{
			Vector3 point = localRay.GetPoint(num2);
			Vector3 zero = Vector3.zero;
			zero[index] = ((!(localRay.direction[index] > 0f)) ? 1 : (-1));
			hitInfo.point = base.transform.TransformPoint(point);
			hitInfo.normal = base.transform.TransformDirection(zero);
			hitInfo.distance = num2;
			return true;
		}
		return false;
	}

	public Vector3[] GetBoundsFaceCenters()
	{
		if (VolumeBounds.HasValue)
		{
			Vector3[] array = new Vector3[6];
			Vector3 size = VolumeBounds.Value.size;
			Vector3 vector = base.transform.position - base.transform.forward * size.z * 0.5f;
			array[0] = base.transform.position;
			array[1] = vector - base.transform.forward * size.z * 0.5f;
			array[2] = vector + base.transform.right * size.x * 0.5f;
			array[3] = vector - base.transform.right * size.x * 0.5f;
			array[4] = vector + base.transform.up * size.y * 0.5f;
			array[5] = vector - base.transform.up * size.y * 0.5f;
			return array;
		}
		if (PlaneRect.HasValue)
		{
			return new Vector3[1] { base.transform.position };
		}
		return null;
	}

	public bool IsPositionInVolume(Vector3 worldPosition, bool testVerticalBounds, float distanceBuffer = 0f)
	{
		if (!VolumeBounds.HasValue)
		{
			return false;
		}
		Vector3 point = base.transform.InverseTransformPoint(worldPosition);
		Bounds value = VolumeBounds.Value;
		value.Expand(distanceBuffer);
		if (testVerticalBounds)
		{
			return value.Contains(point);
		}
		if (point.x >= value.min.x && point.x <= value.max.x && point.z >= value.min.z)
		{
			return point.z <= value.max.z;
		}
		return false;
	}

	public Mesh LoadGlobalMeshTriangles()
	{
		if (!HasAnyLabel(SceneLabels.GLOBAL_MESH))
		{
			return null;
		}
		return LoadObjectMeshTriangles() ?? new Mesh();
	}

	internal Mesh LoadObjectMeshTriangles()
	{
		Anchor.TryGetComponent<OVRTriangleMesh>(out var component);
		if (!component.TryGetCounts(out var vertexCount, out var triangleCount))
		{
			return null;
		}
		Mesh mesh = new Mesh
		{
			indexFormat = ((vertexCount > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16)
		};
		using NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
		using NativeArray<int> indices = new NativeArray<int>(triangleCount * 3, Allocator.Temp);
		if (!component.TryGetMesh(nativeArray, indices))
		{
			return mesh;
		}
		mesh.SetVertices(nativeArray);
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);
		return mesh;
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public bool HasLabel(string label)
	{
		return HasAnyLabel(Utilities.StringLabelToEnum(label));
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public bool HasAnyLabel(List<string> labels)
	{
		return HasAnyLabel(Utilities.StringLabelsToEnum(labels));
	}

	public bool HasAnyLabel(SceneLabels labelFlags)
	{
		return (Label & labelFlags) != 0;
	}

	[Obsolete("Use 'Label' instead.")]
	public SceneLabels GetLabelsAsEnum()
	{
		return Label;
	}
}
