using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class MeshCutter
{
	private CuttableMesh inputMesh;

	private List<CuttableSubMesh> outputFrontSubMeshes;

	private List<CuttableSubMesh> outputBackSubMeshes;

	public void Cut(CuttableMesh input, Plane worldCutPlane)
	{
		inputMesh = input;
		outputFrontSubMeshes = new List<CuttableSubMesh>();
		outputBackSubMeshes = new List<CuttableSubMesh>();
		Transform transform = inputMesh.GetTransform();
		Plane cutPlane;
		if (transform != null)
		{
			Vector3 inPoint = transform.InverseTransformPoint(ClosestPointOnPlane(worldCutPlane, Vector3.zero));
			Vector3 inNormal = transform.InverseTransformDirection(worldCutPlane.normal);
			cutPlane = new Plane(inNormal, inPoint);
		}
		else
		{
			cutPlane = worldCutPlane;
		}
		foreach (CuttableSubMesh subMesh in input.GetSubMeshes())
		{
			Cut(subMesh, cutPlane);
		}
	}

	private static Vector3 ClosestPointOnPlane(Plane plane, Vector3 point)
	{
		return plane.ClosestPointOnPlane(point);
	}

	public CuttableMesh GetFrontOutput()
	{
		return new CuttableMesh(inputMesh, outputFrontSubMeshes);
	}

	public CuttableMesh GetBackOutput()
	{
		return new CuttableMesh(inputMesh, outputBackSubMeshes);
	}

	private void Cut(CuttableSubMesh inputSubMesh, Plane cutPlane)
	{
		bool hasNormals = inputSubMesh.HasNormals();
		bool hasColours = inputSubMesh.HasColours();
		bool hasUvs = inputSubMesh.HasUvs();
		bool hasUv = inputSubMesh.HasUv1();
		CuttableSubMesh cuttableSubMesh = new CuttableSubMesh(hasNormals, hasColours, hasUvs, hasUv);
		CuttableSubMesh cuttableSubMesh2 = new CuttableSubMesh(hasNormals, hasColours, hasUvs, hasUv);
		for (int i = 0; i < inputSubMesh.NumVertices(); i += 3)
		{
			int num = i;
			int num2 = i + 1;
			int num3 = i + 2;
			Vector3 vertex = inputSubMesh.GetVertex(num);
			Vector3 vertex2 = inputSubMesh.GetVertex(num2);
			Vector3 vertex3 = inputSubMesh.GetVertex(num3);
			VertexClassification vertexClassification = Classify(vertex, cutPlane);
			VertexClassification vertexClassification2 = Classify(vertex2, cutPlane);
			VertexClassification vertexClassification3 = Classify(vertex3, cutPlane);
			int numFront = 0;
			int numBehind = 0;
			CountSides(vertexClassification, ref numFront, ref numBehind);
			CountSides(vertexClassification2, ref numFront, ref numBehind);
			CountSides(vertexClassification3, ref numFront, ref numBehind);
			if (numFront > 0 && numBehind == 0)
			{
				KeepTriangle(num, num2, num3, inputSubMesh, cuttableSubMesh);
			}
			else if (numFront == 0 && numBehind > 0)
			{
				KeepTriangle(num, num2, num3, inputSubMesh, cuttableSubMesh2);
			}
			else if (numFront == 2 && numBehind == 1)
			{
				if (vertexClassification == VertexClassification.Back)
				{
					SplitA(num, num2, num3, inputSubMesh, cutPlane, cuttableSubMesh2, cuttableSubMesh);
				}
				else if (vertexClassification2 == VertexClassification.Back)
				{
					SplitA(num2, num3, num, inputSubMesh, cutPlane, cuttableSubMesh2, cuttableSubMesh);
				}
				else
				{
					SplitA(num3, num, num2, inputSubMesh, cutPlane, cuttableSubMesh2, cuttableSubMesh);
				}
			}
			else if (numFront == 1 && numBehind == 2)
			{
				if (vertexClassification == VertexClassification.Front)
				{
					SplitA(num, num2, num3, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
				}
				else if (vertexClassification2 == VertexClassification.Front)
				{
					SplitA(num2, num3, num, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
				}
				else
				{
					SplitA(num3, num, num2, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
				}
			}
			else if (numFront == 1 && numBehind == 1)
			{
				if (vertexClassification == VertexClassification.OnPlane)
				{
					if (vertexClassification3 == VertexClassification.Front)
					{
						SplitB(num3, num, num2, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					}
					else
					{
						SplitBFlipped(num2, num3, num, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					}
					continue;
				}
				switch (vertexClassification2)
				{
				case VertexClassification.OnPlane:
					if (vertexClassification == VertexClassification.Front)
					{
						SplitB(num, num2, num3, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					}
					else
					{
						SplitBFlipped(num3, num, num2, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					}
					break;
				case VertexClassification.Front:
					SplitB(num2, num3, num, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					break;
				default:
					SplitBFlipped(num, num2, num3, inputSubMesh, cutPlane, cuttableSubMesh, cuttableSubMesh2);
					break;
				}
			}
			else if (numFront == 0 && numBehind == 0)
			{
				Vector3 lhs = vertex2 - vertex;
				Vector3 rhs = vertex3 - vertex;
				if (Vector3.Dot(Vector3.Cross(lhs, rhs), cutPlane.normal) > 0f)
				{
					KeepTriangle(num, num2, num3, inputSubMesh, cuttableSubMesh2);
				}
				else
				{
					KeepTriangle(num, num2, num3, inputSubMesh, cuttableSubMesh);
				}
			}
		}
		outputFrontSubMeshes.Add(cuttableSubMesh);
		outputBackSubMeshes.Add(cuttableSubMesh2);
	}

	private VertexClassification Classify(Vector3 vertex, Plane cutPlane)
	{
		Vector3 point = new Vector3(vertex.x, vertex.y, vertex.z);
		float distanceToPoint = cutPlane.GetDistanceToPoint(point);
		double num = 9.999999747378752E-06;
		if ((double)distanceToPoint > 0.0 - num && (double)distanceToPoint < num)
		{
			return VertexClassification.OnPlane;
		}
		if (distanceToPoint > 0f)
		{
			return VertexClassification.Front;
		}
		return VertexClassification.Back;
	}

	private void CountSides(VertexClassification c, ref int numFront, ref int numBehind)
	{
		switch (c)
		{
		case VertexClassification.Front:
			numFront++;
			break;
		case VertexClassification.Back:
			numBehind++;
			break;
		}
	}

	private void KeepTriangle(int i0, int i1, int i2, CuttableSubMesh inputSubMesh, CuttableSubMesh destSubMesh)
	{
		destSubMesh.CopyVertex(i0, inputSubMesh);
		destSubMesh.CopyVertex(i1, inputSubMesh);
		destSubMesh.CopyVertex(i2, inputSubMesh);
	}

	private void SplitA(int i0, int i1, int i2, CuttableSubMesh inputSubMesh, Plane cutPlane, CuttableSubMesh frontSubMesh, CuttableSubMesh backSubMesh)
	{
		Vector3 vertex = inputSubMesh.GetVertex(i0);
		Vector3 vertex2 = inputSubMesh.GetVertex(i1);
		Vector3 vertex3 = inputSubMesh.GetVertex(i2);
		CalcIntersection(vertex, vertex2, cutPlane, out var weight);
		CalcIntersection(vertex3, vertex, cutPlane, out var weight2);
		frontSubMesh.CopyVertex(i0, inputSubMesh);
		frontSubMesh.AddInterpolatedVertex(i0, i1, weight, inputSubMesh);
		frontSubMesh.AddInterpolatedVertex(i2, i0, weight2, inputSubMesh);
		backSubMesh.AddInterpolatedVertex(i0, i1, weight, inputSubMesh);
		backSubMesh.CopyVertex(i1, inputSubMesh);
		backSubMesh.CopyVertex(i2, inputSubMesh);
		backSubMesh.CopyVertex(i2, inputSubMesh);
		backSubMesh.AddInterpolatedVertex(i2, i0, weight2, inputSubMesh);
		backSubMesh.AddInterpolatedVertex(i0, i1, weight, inputSubMesh);
	}

	private void SplitB(int i0, int i1, int i2, CuttableSubMesh inputSubMesh, Plane cutPlane, CuttableSubMesh frontSubMesh, CuttableSubMesh backSubMesh)
	{
		Vector3 vertex = inputSubMesh.GetVertex(i0);
		Vector3 vertex2 = inputSubMesh.GetVertex(i2);
		CalcIntersection(vertex2, vertex, cutPlane, out var weight);
		frontSubMesh.CopyVertex(i0, inputSubMesh);
		frontSubMesh.CopyVertex(i1, inputSubMesh);
		frontSubMesh.AddInterpolatedVertex(i2, i0, weight, inputSubMesh);
		backSubMesh.CopyVertex(i1, inputSubMesh);
		backSubMesh.CopyVertex(i2, inputSubMesh);
		backSubMesh.AddInterpolatedVertex(i2, i0, weight, inputSubMesh);
	}

	private void SplitBFlipped(int i0, int i1, int i2, CuttableSubMesh inputSubMesh, Plane cutPlane, CuttableSubMesh frontSubMesh, CuttableSubMesh backSubMesh)
	{
		Vector3 vertex = inputSubMesh.GetVertex(i0);
		Vector3 vertex2 = inputSubMesh.GetVertex(i1);
		CalcIntersection(vertex, vertex2, cutPlane, out var weight);
		frontSubMesh.CopyVertex(i0, inputSubMesh);
		frontSubMesh.AddInterpolatedVertex(i0, i1, weight, inputSubMesh);
		frontSubMesh.CopyVertex(i2, inputSubMesh);
		backSubMesh.CopyVertex(i1, inputSubMesh);
		backSubMesh.CopyVertex(i2, inputSubMesh);
		backSubMesh.AddInterpolatedVertex(i0, i1, weight, inputSubMesh);
	}

	private Vector3 CalcIntersection(Vector3 v0, Vector3 v1, Plane plane, out float weight)
	{
		Vector3 vector = v1 - v0;
		float magnitude = vector.magnitude;
		Ray ray = new Ray(v0, vector / magnitude);
		plane.Raycast(ray, out var enter);
		Vector3 result = ray.origin + ray.direction * enter;
		weight = enter / magnitude;
		return result;
	}
}
