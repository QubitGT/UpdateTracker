using System;
using System.Collections.Generic;
using Technie.PhysicsCreator.QHull;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class QHullUtil
{
	public static Mesh FindConvexHull(string debugName, Mesh inputMesh, bool showErrorInLog)
	{
		Vector3[] vertices = inputMesh.vertices;
		int[] triangles = inputMesh.triangles;
		Point3d[] array = new Point3d[triangles.Length];
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 vector = vertices[triangles[i]];
			Vector3 vector2 = vertices[triangles[i + 1]];
			Vector3 vector3 = vertices[triangles[i + 2]];
			array[i] = new Point3d(vector.x, vector.y, vector.z);
			array[i + 1] = new Point3d(vector2.x, vector2.y, vector2.z);
			array[i + 2] = new Point3d(vector3.x, vector3.y, vector3.z);
		}
		QuickHull3D quickHull3D = new QuickHull3D();
		try
		{
			quickHull3D.build(array);
		}
		catch (Exception)
		{
			if (showErrorInLog)
			{
				Debug.LogError("Could not generate convex hull for " + debugName);
			}
		}
		Point3d[] vertices2 = quickHull3D.getVertices();
		int[][] faces = quickHull3D.getFaces();
		Vector3[] array2 = new Vector3[vertices2.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = new Vector3((float)vertices2[j].x, (float)vertices2[j].y, (float)vertices2[j].z);
		}
		List<int> list = new List<int>();
		for (int k = 0; k < faces.Length; k++)
		{
			int num = faces[k].Length;
			for (int l = 1; l < num - 1; l++)
			{
				list.Add(faces[k][0]);
				list.Add(faces[k][l]);
				list.Add(faces[k][l + 1]);
			}
		}
		int[] triangles2 = list.ToArray();
		return new Mesh
		{
			vertices = array2,
			triangles = triangles2
		};
	}

	public static void FindConvexHull(string debugName, int[] selectedFaces, Vector3[] meshVertices, int[] meshIndices, out Vector3[] hullVertices, out int[] hullIndices, bool showErrorInLog)
	{
		if (selectedFaces.Length == 0)
		{
			hullVertices = new Vector3[0];
			hullIndices = new int[0];
			return;
		}
		Point3d[] array = new Point3d[selectedFaces.Length * 3];
		for (int i = 0; i < selectedFaces.Length; i++)
		{
			int num = selectedFaces[i];
			Vector3 vector = meshVertices[meshIndices[num * 3]];
			Vector3 vector2 = meshVertices[meshIndices[num * 3 + 1]];
			Vector3 vector3 = meshVertices[meshIndices[num * 3 + 2]];
			array[i * 3] = new Point3d(vector.x, vector.y, vector.z);
			array[i * 3 + 1] = new Point3d(vector2.x, vector2.y, vector2.z);
			array[i * 3 + 2] = new Point3d(vector3.x, vector3.y, vector3.z);
		}
		QuickHull3D quickHull3D = new QuickHull3D();
		try
		{
			quickHull3D.build(array);
		}
		catch (Exception)
		{
			if (showErrorInLog)
			{
				Debug.LogError("Could not generate convex hull for " + debugName);
			}
		}
		Point3d[] vertices = quickHull3D.getVertices();
		int[][] faces = quickHull3D.getFaces();
		hullVertices = new Vector3[vertices.Length];
		for (int j = 0; j < hullVertices.Length; j++)
		{
			hullVertices[j] = new Vector3((float)vertices[j].x, (float)vertices[j].y, (float)vertices[j].z);
		}
		List<int> list = new List<int>();
		for (int k = 0; k < faces.Length; k++)
		{
			int num2 = faces[k].Length;
			for (int l = 1; l < num2 - 1; l++)
			{
				list.Add(faces[k][0]);
				list.Add(faces[k][l]);
				list.Add(faces[k][l + 1]);
			}
		}
		hullIndices = list.ToArray();
	}
}
