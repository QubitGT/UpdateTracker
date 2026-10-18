using System;
using System.Collections.Generic;

namespace g3;

public class CurveResampler
{
	private double[] lengths;

	public List<Vector3d> SplitResample(ISampledCurve3d curve, double fMaxEdgeLen)
	{
		double num = fMaxEdgeLen * fMaxEdgeLen;
		int vertexCount = curve.VertexCount;
		int num2 = (curve.Closed ? (vertexCount + 1) : vertexCount);
		if (lengths == null || lengths.Length < num2)
		{
			lengths = new double[num2];
		}
		bool flag = false;
		for (int i = 0; i < num2; i++)
		{
			lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % vertexCount));
			if (lengths[i] > num)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return null;
		}
		List<Vector3d> list = new List<Vector3d>();
		Vector3d vector3d = curve.GetVertex(0);
		list.Add(vector3d);
		for (int j = 0; j < num2 - 1; j++)
		{
			Vector3d vertex = curve.GetVertex((j + 1) % vertexCount);
			if (lengths[j] > num)
			{
				int num3 = (int)(Math.Sqrt(lengths[j]) / fMaxEdgeLen) + 1;
				for (int k = 1; k < num3; k++)
				{
					double t = (double)k / (double)num3;
					Vector3d item = Vector3d.Lerp(vector3d, vertex, t);
					list.Add(item);
				}
			}
			list.Add(vertex);
			vector3d = vertex;
		}
		return list;
	}

	public List<Vector3d> SplitCollapseResample(ISampledCurve3d curve, double fMaxEdgeLen, double fMinEdgeLen)
	{
		double num = fMaxEdgeLen * fMaxEdgeLen;
		double num2 = fMinEdgeLen * fMinEdgeLen;
		int vertexCount = curve.VertexCount;
		int num3 = (curve.Closed ? (vertexCount + 1) : vertexCount);
		if (lengths == null || lengths.Length < num3)
		{
			lengths = new double[num3];
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < num3 - 1; i++)
		{
			lengths[i] = curve.GetVertex(i).DistanceSquared(curve.GetVertex((i + 1) % vertexCount));
			if (lengths[i] > num)
			{
				flag = true;
			}
			else if (lengths[i] < num2)
			{
				flag2 = true;
			}
		}
		if (!flag && !flag2)
		{
			return null;
		}
		List<Vector3d> list = new List<Vector3d>();
		Vector3d vector3d = curve.GetVertex(0);
		list.Add(vector3d);
		double num4 = 0.0;
		for (int j = 0; j < num3 - 1; j++)
		{
			Vector3d vertex = curve.GetVertex((j + 1) % vertexCount);
			if (lengths[j] < num2)
			{
				num4 += Math.Sqrt(lengths[j]);
				if (num4 > fMinEdgeLen)
				{
					num4 = 0.0;
					list.Add(vertex);
				}
				vector3d = vertex;
				continue;
			}
			if (num4 > 0.0)
			{
				list.Add(vector3d);
				num4 = 0.0;
			}
			if (lengths[j] > num)
			{
				int num5 = (int)(Math.Sqrt(lengths[j]) / fMaxEdgeLen) + 1;
				for (int k = 1; k < num5; k++)
				{
					double t = (double)k / (double)num5;
					Vector3d item = Vector3d.Lerp(vector3d, vertex, t);
					list.Add(item);
				}
			}
			list.Add(vertex);
			vector3d = vertex;
		}
		return list;
	}
}
