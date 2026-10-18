using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder;

public static class HandleUtility
{
	internal static Vector3 ScreenToGuiPoint(this Camera camera, Vector3 point, float pixelsPerPoint)
	{
		return new Vector3(point.x / pixelsPerPoint, ((float)camera.pixelHeight - point.y) / pixelsPerPoint, point.z);
	}

	internal static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, HashSet<Face> ignore = null)
	{
		return FaceRaycast(worldRay, mesh, out hit, float.PositiveInfinity, CullingMode.Back, ignore);
	}

	internal static bool FaceRaycast(Ray worldRay, ProBuilderMesh mesh, out RaycastHit hit, float distance, CullingMode cullingMode, HashSet<Face> ignore = null)
	{
		worldRay.origin -= mesh.transform.position;
		worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
		worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;
		Vector3[] positionsInternal = mesh.positionsInternal;
		Face[] facesInternal = mesh.facesInternal;
		float num = float.PositiveInfinity;
		int num2 = -1;
		Vector3 normal = Vector3.zero;
		int i = 0;
		for (int num3 = facesInternal.Length; i < num3; i++)
		{
			if (ignore != null && ignore.Contains(facesInternal[i]))
			{
				continue;
			}
			int[] indexesInternal = mesh.facesInternal[i].indexesInternal;
			int j = 0;
			for (int num4 = indexesInternal.Length; j < num4; j += 3)
			{
				Vector3 vector = positionsInternal[indexesInternal[j]];
				Vector3 vector2 = positionsInternal[indexesInternal[j + 1]];
				Vector3 vector3 = positionsInternal[indexesInternal[j + 2]];
				Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
				float num5 = Vector3.Dot(worldRay.direction, vector4);
				bool flag = false;
				switch (cullingMode)
				{
				case CullingMode.Front:
					if (num5 < 0f)
					{
						flag = true;
					}
					break;
				case CullingMode.Back:
					if (num5 > 0f)
					{
						flag = true;
					}
					break;
				}
				float OutDistance = 0f;
				if (!flag && Math.RayIntersectsTriangle(worldRay, vector, vector2, vector3, out OutDistance, out var _) && !(OutDistance > num) && !(OutDistance > distance))
				{
					normal = vector4;
					num2 = i;
					num = OutDistance;
				}
			}
		}
		hit = new RaycastHit(num, worldRay.GetPoint(num), normal, num2);
		return num2 > -1;
	}

	internal static bool FaceRaycastBothCullModes(Ray worldRay, ProBuilderMesh mesh, ref SimpleTuple<Face, Vector3> back, ref SimpleTuple<Face, Vector3> front)
	{
		worldRay.origin -= mesh.transform.position;
		worldRay.origin = mesh.transform.worldToLocalMatrix * worldRay.origin;
		worldRay.direction = mesh.transform.worldToLocalMatrix * worldRay.direction;
		Vector3[] positionsInternal = mesh.positionsInternal;
		Face[] facesInternal = mesh.facesInternal;
		back.item1 = null;
		front.item1 = null;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		int i = 0;
		for (int num3 = facesInternal.Length; i < num3; i++)
		{
			int[] indexesInternal = mesh.facesInternal[i].indexesInternal;
			int j = 0;
			for (int num4 = indexesInternal.Length; j < num4; j += 3)
			{
				Vector3 vector = positionsInternal[indexesInternal[j]];
				Vector3 vector2 = positionsInternal[indexesInternal[j + 1]];
				Vector3 vector3 = positionsInternal[indexesInternal[j + 2]];
				if (!Math.RayIntersectsTriangle(worldRay, vector, vector2, vector3, out var OutDistance, out var _) || (!(OutDistance < num) && !(OutDistance < num2)))
				{
					continue;
				}
				Vector3 rhs = Vector3.Cross(vector2 - vector, vector3 - vector);
				if (Vector3.Dot(worldRay.direction, rhs) < 0f)
				{
					if (OutDistance < num)
					{
						num = OutDistance;
						back.item1 = facesInternal[i];
					}
				}
				else if (OutDistance < num2)
				{
					num2 = OutDistance;
					front.item1 = facesInternal[i];
				}
			}
		}
		if (back.item1 != null)
		{
			back.item2 = worldRay.GetPoint(num);
		}
		if (front.item1 != null)
		{
			front.item2 = worldRay.GetPoint(num2);
		}
		if (back.item1 == null)
		{
			return front.item1 != null;
		}
		return true;
	}

	internal static bool FaceRaycast(Ray InWorldRay, ProBuilderMesh mesh, out List<RaycastHit> hits, CullingMode cullingMode, HashSet<Face> ignore = null)
	{
		InWorldRay.origin -= mesh.transform.position;
		InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
		InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;
		Vector3[] positionsInternal = mesh.positionsInternal;
		hits = new List<RaycastHit>();
		for (int i = 0; i < mesh.facesInternal.Length; i++)
		{
			if (ignore != null && ignore.Contains(mesh.facesInternal[i]))
			{
				continue;
			}
			int[] indexesInternal = mesh.facesInternal[i].indexesInternal;
			for (int j = 0; j < indexesInternal.Length; j += 3)
			{
				Vector3 vector = positionsInternal[indexesInternal[j]];
				Vector3 vector2 = positionsInternal[indexesInternal[j + 1]];
				Vector3 vector3 = positionsInternal[indexesInternal[j + 2]];
				float OutDistance = 0f;
				if (!Math.RayIntersectsTriangle(InWorldRay, vector, vector2, vector3, out OutDistance, out var _))
				{
					continue;
				}
				Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
				switch (cullingMode)
				{
				case CullingMode.Front:
					if (!(Vector3.Dot(InWorldRay.direction, vector4) > 0f))
					{
						continue;
					}
					break;
				case CullingMode.Back:
					if (!(Vector3.Dot(InWorldRay.direction, vector4) < 0f))
					{
						continue;
					}
					break;
				case CullingMode.FrontBack:
					break;
				default:
					continue;
				}
				hits.Add(new RaycastHit(OutDistance, InWorldRay.GetPoint(OutDistance), vector4, i));
			}
		}
		return hits.Count > 0;
	}

	internal static Ray InverseTransformRay(this Transform transform, Ray InWorldRay)
	{
		Vector3 origin = InWorldRay.origin;
		origin -= transform.position;
		origin = transform.worldToLocalMatrix * origin;
		Vector3 direction = transform.worldToLocalMatrix.MultiplyVector(InWorldRay.direction);
		return new Ray(origin, direction);
	}

	internal static bool MeshRaycast(Ray InWorldRay, GameObject gameObject, out RaycastHit hit, float distance = float.PositiveInfinity)
	{
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = ((component != null) ? component.sharedMesh : null);
		if (!mesh)
		{
			hit = null;
			return false;
		}
		return MeshRaycast(gameObject.transform.InverseTransformRay(InWorldRay), mesh.vertices, mesh.triangles, out hit, distance);
	}

	internal static bool MeshRaycast(Ray InRay, Vector3[] mesh, int[] triangles, out RaycastHit hit, float distance = float.PositiveInfinity)
	{
		float num = float.PositiveInfinity;
		Vector3 normal = Vector3.zero;
		Vector3 normal2 = Vector3.zero;
		int num2 = -1;
		Vector3 origin = InRay.origin;
		Vector3 direction = InRay.direction;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 vert = mesh[triangles[i]];
			Vector3 vert2 = mesh[triangles[i + 1]];
			Vector3 vert3 = mesh[triangles[i + 2]];
			if (Math.RayIntersectsTriangle2(origin, direction, vert, vert2, vert3, ref distance, ref normal2) && distance < num)
			{
				num2 = i / 3;
				num = distance;
				normal = normal2;
			}
		}
		hit = new RaycastHit(num, InRay.GetPoint(num), normal, num2);
		return num2 > -1;
	}

	internal static bool PointIsOccluded(Camera cam, ProBuilderMesh pb, Vector3 worldPoint)
	{
		Vector3 normalized = (cam.transform.position - worldPoint).normalized;
		RaycastHit hit;
		return FaceRaycast(new Ray(worldPoint + normalized * 0.0001f, normalized), pb, out hit, Vector3.Distance(cam.transform.position, worldPoint), CullingMode.Front);
	}

	public static Quaternion GetRotation(ProBuilderMesh mesh, IEnumerable<int> indices)
	{
		if (!mesh.HasArrays(MeshArrays.Normal))
		{
			Normals.CalculateNormals(mesh);
		}
		if (!mesh.HasArrays(MeshArrays.Tangent))
		{
			Normals.CalculateTangents(mesh);
		}
		Vector3[] normalsInternal = mesh.normalsInternal;
		Vector4[] tangentsInternal = mesh.tangentsInternal;
		Vector3 zero = Vector3.zero;
		Vector4 zero2 = Vector4.zero;
		float num = 0f;
		foreach (int index in indices)
		{
			Vector3 vector = normalsInternal[index];
			Vector4 vector2 = tangentsInternal[index];
			zero.x += vector.x;
			zero.y += vector.y;
			zero.z += vector.z;
			zero2.x += vector2.x;
			zero2.y += vector2.y;
			zero2.z += vector2.z;
			zero2.w += vector2.w;
			num += 1f;
		}
		zero.x /= num;
		zero.y /= num;
		zero.z /= num;
		zero2.x /= num;
		zero2.y /= num;
		zero2.z /= num;
		zero2.w /= num;
		if (zero == Vector3.zero || zero2 == Vector4.zero)
		{
			return mesh.transform.rotation;
		}
		Vector3 upwards = Vector3.Cross(zero, zero2 * zero2.w);
		return mesh.transform.rotation * Quaternion.LookRotation(zero, upwards);
	}

	public static Quaternion GetFaceRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Face> faces)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		return orientation switch
		{
			HandleOrientation.ActiveElement => GetFaceRotation(mesh, faces.Last()), 
			HandleOrientation.ActiveObject => mesh.transform.rotation, 
			_ => Quaternion.identity, 
		};
	}

	public static Quaternion GetFaceRotation(ProBuilderMesh mesh, Face face)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		if (face == null)
		{
			return mesh.transform.rotation;
		}
		Normal normal = Math.NormalTangentBitangent(mesh, face);
		if (normal.normal == Vector3.zero || normal.bitangent == Vector3.zero)
		{
			return mesh.transform.rotation;
		}
		return mesh.transform.rotation * Quaternion.LookRotation(normal.normal, normal.bitangent);
	}

	public static Quaternion GetEdgeRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<Edge> edges)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		return orientation switch
		{
			HandleOrientation.ActiveElement => GetEdgeRotation(mesh, edges.Last()), 
			HandleOrientation.ActiveObject => mesh.transform.rotation, 
			_ => Quaternion.identity, 
		};
	}

	public static Quaternion GetEdgeRotation(ProBuilderMesh mesh, Edge edge)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		return GetFaceRotation(mesh, mesh.GetFace(edge));
	}

	public static Quaternion GetVertexRotation(ProBuilderMesh mesh, HandleOrientation orientation, IEnumerable<int> vertices)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		if (orientation != HandleOrientation.ActiveObject)
		{
			if (orientation != HandleOrientation.ActiveElement)
			{
				return Quaternion.identity;
			}
			if (mesh.selectedVertexCount >= 1)
			{
				return GetRotation(mesh, vertices);
			}
		}
		return mesh.transform.rotation;
	}

	public static Quaternion GetVertexRotation(ProBuilderMesh mesh, int vertex)
	{
		if (mesh == null)
		{
			return Quaternion.identity;
		}
		if (vertex < 0)
		{
			return mesh.transform.rotation;
		}
		return GetRotation(mesh, new int[1] { vertex });
	}

	internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		return mesh.transform.TransformPoint(Math.GetBounds(mesh.positionsInternal, faces.Last().distinctIndexesInternal).center);
	}

	internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<Edge> edges)
	{
		Edge edge = edges.Last();
		return mesh.transform.TransformPoint(Math.GetBounds(mesh.positionsInternal, new int[2] { edge.a, edge.b }).center);
	}

	internal static Vector3 GetActiveElementPosition(ProBuilderMesh mesh, IEnumerable<int> vertices)
	{
		return mesh.transform.TransformPoint(mesh.positionsInternal[vertices.First()]);
	}
}
