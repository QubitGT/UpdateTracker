using System;
using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator.Rigid;

[Serializable]
public class Hull : IHull
{
	public string name = "<unnamed hull>";

	public bool isVisible = true;

	public HullType type = HullType.ConvexHull;

	public Color colour = Color.white;

	public PhysicsMaterial material;

	public bool enableInflation;

	public float inflationAmount = 0.01f;

	public BoxFitMethod boxFitMethod = BoxFitMethod.MinimumVolume;

	public bool isTrigger;

	public bool isChildCollider;

	[SerializeField]
	private List<int> selectedFaces = new List<int>();

	public List<Vector3> cachedTriangleVertices = new List<Vector3>();

	public Mesh collisionMesh;

	public BoxDef collisionBox;

	public Sphere collisionSphere;

	public Mesh faceCollisionMesh;

	public Vector3 faceBoxCenter;

	public Vector3 faceBoxSize;

	public Quaternion faceAsBoxRotation;

	public CapsuleDef collisionCapsule;

	public Mesh[] autoMeshes = new Mesh[0];

	public bool hasColliderError;

	public int numColliderFaces;

	public bool noInputError;

	public string Name => name;

	public int NumSelectedTriangles => selectedFaces.Count;

	public Vector3[] CachedTriangleVertices
	{
		get
		{
			return cachedTriangleVertices.ToArray();
		}
		set
		{
			cachedTriangleVertices.Clear();
			cachedTriangleVertices.AddRange(value);
		}
	}

	public void Destroy()
	{
	}

	public bool ContainsAutoMesh(Mesh m)
	{
		if (autoMeshes != null)
		{
			Mesh[] array = autoMeshes;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == m)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsTriangleSelected(int triIndex, Renderer renderer, Mesh targetMesh)
	{
		return selectedFaces.Contains(triIndex);
	}

	public int[] GetSelectedFaces()
	{
		return selectedFaces.ToArray();
	}

	public void ClearSelectedFaces()
	{
		selectedFaces.Clear();
		cachedTriangleVertices.Clear();
	}

	public void AddToSelection(int newTriangleIndex, Mesh srcMesh)
	{
		if (!selectedFaces.Contains(newTriangleIndex))
		{
			selectedFaces.Add(newTriangleIndex);
			Utils.UpdateCachedVertices(this, srcMesh);
		}
	}

	public void RemoveFromSelection(int existingTriangleIndex, Mesh srcMesh)
	{
		selectedFaces.Remove(existingTriangleIndex);
		Utils.UpdateCachedVertices(this, srcMesh);
	}

	public void SetSelectedFaces(List<int> newSelectedFaceIndices, Mesh srcMesh)
	{
		selectedFaces.Clear();
		selectedFaces.AddRange(newSelectedFaceIndices);
		Utils.UpdateCachedVertices(this, srcMesh);
	}

	public int GetSelectedFaceIndex(int index)
	{
		return selectedFaces[index];
	}

	public void FindConvexHull(Vector3[] meshVertices, int[] meshIndices, out Vector3[] hullVertices, out int[] hullIndices, bool showErrorInLog)
	{
		QHullUtil.FindConvexHull(name, selectedFaces.ToArray(), meshVertices, meshIndices, out hullVertices, out hullIndices, showErrorInLog: false);
	}

	public List<Triangle> FindSelectedTriangles(Vector3[] meshVertices, int[] meshIndices)
	{
		List<Triangle> list = new List<Triangle>();
		foreach (int selectedFace in selectedFaces)
		{
			int num = meshIndices[selectedFace * 3];
			int num2 = meshIndices[selectedFace * 3 + 1];
			int num3 = meshIndices[selectedFace * 3 + 2];
			Vector3 p = meshVertices[num];
			Vector3 p2 = meshVertices[num2];
			Vector3 p3 = meshVertices[num3];
			Triangle item = new Triangle(p, p2, p3);
			list.Add(item);
		}
		return list;
	}

	public void FindTriangles(Vector3[] meshVertices, int[] meshIndices, out Vector3[] hullVertices, out int[] hullIndices)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (int selectedFace in selectedFaces)
		{
			int num = meshIndices[selectedFace * 3];
			int num2 = meshIndices[selectedFace * 3 + 1];
			int num3 = meshIndices[selectedFace * 3 + 2];
			Vector3 item = meshVertices[num];
			Vector3 item2 = meshVertices[num2];
			Vector3 item3 = meshVertices[num3];
			list.Add(item);
			list.Add(item2);
			list.Add(item3);
		}
		hullVertices = list.ToArray();
		hullIndices = new int[hullVertices.Length];
		for (int i = 0; i < hullIndices.Length; i++)
		{
			hullIndices[i] = i;
		}
	}

	public Vector3[] GetSelectedVertices(Vector3[] meshVertices, int[] meshIndices)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int selectedFace in selectedFaces)
		{
			int num = meshIndices[selectedFace * 3];
			int num2 = meshIndices[selectedFace * 3 + 1];
			int num3 = meshIndices[selectedFace * 3 + 2];
			dictionary[num] = num;
			dictionary[num2] = num2;
			dictionary[num3] = num3;
		}
		List<Vector3> list = new List<Vector3>();
		foreach (int key in dictionary.Keys)
		{
			list.Add(meshVertices[key]);
		}
		return list.ToArray();
	}

	public void GenerateCollisionMesh(Vector3[] meshVertices, int[] meshIndices, Mesh[] autoHulls, float faceThickness)
	{
		hasColliderError = false;
		noInputError = false;
		if (selectedFaces.Count == 0)
		{
			noInputError = true;
		}
		if (type == HullType.Box)
		{
			if (selectedFaces.Count <= 0)
			{
				return;
			}
			if (isChildCollider)
			{
				if (boxFitMethod == BoxFitMethod.MinimumVolume)
				{
					RotatedBoxFitter rotatedBoxFitter = new RotatedBoxFitter();
					collisionBox = rotatedBoxFitter.Fit(this, meshVertices, meshIndices);
				}
				else if (boxFitMethod == BoxFitMethod.AlignFaces)
				{
					new FaceAlignmentBoxFitter().Fit(this, meshVertices, meshIndices);
				}
				else if (boxFitMethod == BoxFitMethod.AxisAligned)
				{
					new AxisAlignedBoxFitter().Fit(this, meshVertices, meshIndices);
				}
				return;
			}
			Vector3 min;
			Vector3 max = (min = meshVertices[meshIndices[selectedFaces[0] * 3]]);
			for (int i = 0; i < selectedFaces.Count; i++)
			{
				int num = selectedFaces[i];
				Vector3 point = meshVertices[meshIndices[num * 3]];
				Vector3 point2 = meshVertices[meshIndices[num * 3 + 1]];
				Vector3 point3 = meshVertices[meshIndices[num * 3 + 2]];
				Utils.Inflate(point, ref min, ref max);
				Utils.Inflate(point2, ref min, ref max);
				Utils.Inflate(point3, ref min, ref max);
			}
			collisionBox.collisionBox.center = (min + max) * 0.5f;
			collisionBox.collisionBox.size = max - min;
			collisionBox.boxRotation = Quaternion.identity;
		}
		else if (type == HullType.Capsule)
		{
			if (isChildCollider)
			{
				RotatedCapsuleFitter rotatedCapsuleFitter = new RotatedCapsuleFitter();
				collisionCapsule = rotatedCapsuleFitter.Fit(this, meshVertices, meshIndices);
			}
			else
			{
				AlignedCapsuleFitter alignedCapsuleFitter = new AlignedCapsuleFitter();
				collisionCapsule = alignedCapsuleFitter.Fit(this, meshVertices, meshIndices);
			}
		}
		else if (type == HullType.Sphere)
		{
			SphereFitter sphereFitter = new SphereFitter();
			collisionSphere = sphereFitter.Fit(this, meshVertices, meshIndices);
		}
		else if (type == HullType.ConvexHull)
		{
			if (collisionMesh == null)
			{
				collisionMesh = new Mesh();
			}
			collisionMesh.name = name;
			collisionMesh.triangles = new int[0];
			collisionMesh.vertices = new Vector3[0];
			GenerateConvexHull(this, meshVertices, meshIndices, collisionMesh);
		}
		else if (type == HullType.Face)
		{
			if (faceCollisionMesh == null)
			{
				faceCollisionMesh = new Mesh();
			}
			faceCollisionMesh.name = name;
			faceCollisionMesh.triangles = new int[0];
			faceCollisionMesh.vertices = new Vector3[0];
			GenerateFace(this, meshVertices, meshIndices, faceThickness);
		}
		else if (type == HullType.FaceAsBox)
		{
			if (selectedFaces.Count <= 0)
			{
				return;
			}
			if (isChildCollider)
			{
				Vector3[] vertices = ExtractUniqueVertices(this, meshVertices, meshIndices);
				Vector3 vector = CalcPrimaryAxis(this, meshVertices, meshIndices, !isChildCollider);
				Vector3 rhs = ((Vector3.Dot(vector, Vector3.up) > 0.8f) ? Vector3.right : Vector3.up);
				Vector3 rhs2 = Vector3.Cross(vector, rhs);
				Vector3 primaryUp = Vector3.Cross(vector, rhs2);
				float num2 = 0f;
				float num3 = float.MaxValue;
				Vector3 vector2 = Vector3.zero;
				Vector3 vector3 = Vector3.zero;
				Quaternion quaternion = Quaternion.identity;
				float num4 = 5f;
				float num5 = 0.05f;
				for (float num6 = 0f; num6 <= 360f; num6 += num4)
				{
					Vector3 min2;
					Vector3 max2;
					Quaternion outBasis;
					float num7 = CalcRequiredArea(num6, vector, primaryUp, vertices, out min2, out max2, out outBasis);
					if (num7 < num3)
					{
						num2 = num6;
						num3 = num7;
						vector2 = min2;
						vector3 = max2;
						quaternion = outBasis;
					}
				}
				float num8 = num2 - num4;
				float num9 = num2 + num4;
				for (float num10 = num8; num10 <= num9; num10 += num5)
				{
					Vector3 min3;
					Vector3 max3;
					Quaternion outBasis2;
					float num11 = CalcRequiredArea(num10, vector, primaryUp, vertices, out min3, out max3, out outBasis2);
					if (num11 < num3)
					{
						num2 = num10;
						num3 = num11;
						vector2 = min3;
						vector3 = max3;
						quaternion = outBasis2;
					}
				}
				Vector3 vector4 = (vector2 + vector3) / 2f;
				Vector3 vector5 = vector3 - vector2;
				float num12 = vector5.z - faceThickness;
				vector4.z += num12 * 0.5f;
				vector5.z += num12;
				faceBoxCenter = quaternion * vector4;
				faceBoxSize = vector5;
				faceAsBoxRotation = quaternion;
			}
			else
			{
				Vector3[] array = ExtractUniqueVertices(this, meshVertices, meshIndices);
				Vector3 vector6 = CalcPrimaryAxis(this, meshVertices, meshIndices, !isChildCollider);
				Vector3 min4;
				Vector3 max4 = (min4 = array[0]);
				Vector3[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					Utils.Inflate(array2[j], ref min4, ref max4);
				}
				Vector3 vector7 = (min4 + max4) / 2f;
				Vector3 vector8 = max4 - min4;
				if (Mathf.Abs(vector6.x) > 0f)
				{
					float num13 = ((vector6.x > 0f) ? 1f : (-1f));
					float num14 = vector8.x - faceThickness;
					vector7.x += num14 * 0.5f * num13;
					vector8.x += num14;
				}
				else if (Mathf.Abs(vector6.y) > 0f)
				{
					float num15 = ((vector6.y > 0f) ? 1f : (-1f));
					float num16 = vector8.y - faceThickness;
					vector7.y += num16 * 0.5f * num15;
					vector8.y += num16;
				}
				else
				{
					float num17 = ((vector6.z > 0f) ? 1f : (-1f));
					float num18 = vector8.z - faceThickness;
					vector7.z += num18 * 0.5f * num17;
					vector8.z += num18;
				}
				faceBoxCenter = vector7;
				faceBoxSize = vector8;
				faceAsBoxRotation = Quaternion.identity;
			}
		}
		else
		{
			if (type != HullType.Auto)
			{
				return;
			}
			if (collisionMesh == null)
			{
				collisionMesh = new Mesh();
			}
			collisionMesh.name = $"{name} bounds";
			collisionMesh.triangles = new int[0];
			collisionMesh.vertices = new Vector3[0];
			GenerateConvexHull(this, meshVertices, meshIndices, collisionMesh);
			List<Mesh> list = new List<Mesh>();
			if (selectedFaces.Count == meshIndices.Length / 3)
			{
				list.AddRange(autoHulls);
			}
			else
			{
				foreach (Mesh inputMesh in autoHulls)
				{
					Mesh mesh = Utils.Clip(collisionMesh, inputMesh);
					if (mesh != null)
					{
						list.Add(mesh);
					}
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				list[k].name = $"{name}.{k + 1}";
			}
			List<Mesh> list2 = new List<Mesh>();
			if (autoMeshes != null)
			{
				list2.AddRange(autoMeshes);
			}
			while (list2.Count > list.Count)
			{
				list2.RemoveAt(list2.Count - 1);
			}
			while (list2.Count < list.Count)
			{
				list2.Add(new Mesh());
			}
			for (int l = 0; l < list.Count; l++)
			{
				list2[l].Clear();
				list2[l].name = list[l].name;
				list2[l].vertices = list[l].vertices;
				list2[l].triangles = list[l].triangles;
			}
			autoMeshes = list2.ToArray();
		}
	}

	private Vector3[] ExtractUniqueVertices(Hull hull, Vector3[] meshVertices, int[] meshIndices)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < hull.selectedFaces.Count; i++)
		{
			int num = hull.selectedFaces[i];
			Vector3 vector = meshVertices[meshIndices[num * 3]];
			Vector3 vector2 = meshVertices[meshIndices[num * 3 + 1]];
			Vector3 vector3 = meshVertices[meshIndices[num * 3 + 2]];
			if (!Contains(list, vector))
			{
				list.Add(vector);
			}
			if (!Contains(list, vector2))
			{
				list.Add(vector2);
			}
			if (!Contains(list, vector3))
			{
				list.Add(vector3);
			}
		}
		return list.ToArray();
	}

	private static bool Contains(List<Vector3> list, Vector3 p)
	{
		foreach (Vector3 item in list)
		{
			if (Vector3.Distance(item, p) < 0.0001f)
			{
				return true;
			}
		}
		return false;
	}

	private void GenerateConvexHull(Hull hull, Vector3[] meshVertices, int[] meshIndices, Mesh destMesh)
	{
		QHullUtil.FindConvexHull(hull.name, hull.selectedFaces.ToArray(), meshVertices, meshIndices, out var hullVertices, out var hullIndices, showErrorInLog: true);
		hull.numColliderFaces = hullIndices.Length / 3;
		Console.output.Log("Calculated collider for '" + hull.name + "' has " + hull.numColliderFaces + " faces");
		if (hull.numColliderFaces >= 256)
		{
			hull.hasColliderError = true;
			hull.enableInflation = true;
		}
		hull.collisionMesh.vertices = hullVertices;
		hull.collisionMesh.triangles = hullIndices;
		hull.collisionMesh.RecalculateBounds();
		hull.faceCollisionMesh = null;
	}

	private void GenerateFace(Hull hull, Vector3[] meshVertices, int[] meshIndices, float faceThickness)
	{
		int count = hull.selectedFaces.Count;
		Vector3[] array = new Vector3[count * 3 * 2];
		for (int i = 0; i < hull.selectedFaces.Count; i++)
		{
			int num = hull.selectedFaces[i];
			Vector3 vector = meshVertices[meshIndices[num * 3]];
			Vector3 vector2 = meshVertices[meshIndices[num * 3 + 1]];
			Vector3 vector3 = meshVertices[meshIndices[num * 3 + 2]];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 vector4 = Vector3.Cross((vector3 - vector).normalized, normalized);
			int num2 = i * 3 * 2;
			array[num2] = vector;
			array[num2 + 1] = vector2;
			array[num2 + 2] = vector3;
			array[num2 + 3] = vector + vector4 * faceThickness;
			array[num2 + 4] = vector2 + vector4 * faceThickness;
			array[num2 + 5] = vector3 + vector4 * faceThickness;
		}
		int[] array2 = new int[count * 3 * 2];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = j;
		}
		hull.faceCollisionMesh.vertices = array;
		hull.faceCollisionMesh.triangles = array2;
		hull.faceCollisionMesh.RecalculateBounds();
		hull.collisionMesh = null;
	}

	private static float CalcRequiredArea(float angleDeg, Vector3 primaryAxis, Vector3 primaryUp, Vector3[] vertices, out Vector3 min, out Vector3 max, out Quaternion outBasis)
	{
		if (vertices.Length == 0)
		{
			min = Vector3.zero;
			max = Vector3.zero;
			outBasis = Quaternion.identity;
			return 0f;
		}
		Vector3 upwards = Quaternion.AngleAxis(angleDeg, primaryAxis) * primaryUp;
		Quaternion quaternion = Quaternion.LookRotation(primaryAxis, upwards);
		Quaternion quaternion2 = Quaternion.Inverse(quaternion);
		max = (min = quaternion2 * vertices[0]);
		foreach (Vector3 vector in vertices)
		{
			Utils.Inflate(quaternion2 * vector, ref min, ref max);
		}
		outBasis = quaternion;
		Vector3 vector2 = max - min;
		return vector2.x * vector2.y;
	}

	private static Vector3 CalcPrimaryAxis(Hull hull, Vector3[] meshVertices, int[] meshIndices, bool snapToAxies)
	{
		int num = 0;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < hull.selectedFaces.Count; i++)
		{
			int num2 = hull.selectedFaces[i];
			Vector3 vector = meshVertices[meshIndices[num2 * 3]];
			Vector3 vector2 = meshVertices[meshIndices[num2 * 3 + 1]];
			Vector3 vector3 = meshVertices[meshIndices[num2 * 3 + 2]];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 normalized2 = (vector3 - vector).normalized;
			Vector3 vector4 = Vector3.Cross(normalized, normalized2);
			zero += vector4;
			num++;
		}
		Vector3 vector5 = zero / num;
		if (vector5.magnitude < 0.0001f)
		{
			return Vector3.up;
		}
		if (snapToAxies)
		{
			float num3 = Mathf.Abs(vector5.x);
			float num4 = Mathf.Abs(vector5.y);
			float num5 = Mathf.Abs(vector5.z);
			if (num3 > num4 && num3 > num5)
			{
				return new Vector3(((double)vector5.x > 0.0) ? 1f : (-1f), 0f, 0f);
			}
			if (num4 > num5)
			{
				return new Vector3(0f, ((double)vector5.y > 0.0) ? 1f : (-1f), 0f);
			}
			return new Vector3(0f, 0f, ((double)vector5.z > 0.0) ? 1f : (-1f));
		}
		return vector5.normalized;
	}
}
