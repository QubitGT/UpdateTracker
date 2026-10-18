using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

[Feature(Feature.Scene)]
public static class Triangulator
{
	public unsafe static void TriangulatePoints(List<Vector2> vertices, List<List<Vector2>> holes, out Vector2[] outVertices, out int[] outIndices)
	{
		int num = ((holes == null) ? 1 : (holes.Count + 1));
		MRUKNativeFuncs.MrukPolygon2f[] array = new MRUKNativeFuncs.MrukPolygon2f[num];
		array[0].numPoints = (uint)vertices.Count;
		array[0].points = vertices.ToArray();
		if (holes != null)
		{
			for (int i = 0; i < holes.Count; i++)
			{
				array[i + 1].numPoints = (uint)holes[i].Count;
				array[i + 1].points = holes[i].ToArray();
			}
		}
		Mesh2fDisposer mesh2fDisposer = new Mesh2fDisposer(MRUKNativeFuncs.TriangulatePolygon(array, (uint)num));
		try
		{
			outVertices = new Vector2[mesh2fDisposer.Mesh.numVertices];
			for (uint num2 = 0u; num2 < mesh2fDisposer.Mesh.numVertices; num2++)
			{
				outVertices[num2] = mesh2fDisposer.Mesh.vertices[num2];
			}
			outIndices = new int[mesh2fDisposer.Mesh.numIndices];
			for (uint num3 = 0u; num3 < mesh2fDisposer.Mesh.numIndices; num3++)
			{
				outIndices[num3] = (int)mesh2fDisposer.Mesh.indices[num3];
			}
		}
		finally
		{
			((IDisposable)mesh2fDisposer).Dispose();
		}
	}
}
