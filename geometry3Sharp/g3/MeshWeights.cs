using System;

namespace g3;

public static class MeshWeights
{
	public static Vector3d OneRingCentroid(DMesh3 mesh, int vID)
	{
		Vector3d zero = Vector3d.Zero;
		int num = 0;
		foreach (int item in mesh.VtxVerticesItr(vID))
		{
			zero += mesh.GetVertex(item);
			num++;
		}
		if (num == 0)
		{
			return mesh.GetVertex(vID);
		}
		double num2 = 1.0 / (double)num;
		zero.x *= num2;
		zero.y *= num2;
		zero.z *= num2;
		return zero;
	}

	public static Vector3d CotanCentroid(DMesh3 mesh, int v_i)
	{
		Vector3d zero = Vector3d.Zero;
		double num = 0.0;
		Vector3d vertex = mesh.GetVertex(v_i);
		int vOther = -1;
		int oppV = -1;
		int num2 = -1;
		int t = -1;
		int t2 = -1;
		bool flag = false;
		foreach (int item in mesh.VtxEdgesItr(v_i))
		{
			num2 = -1;
			mesh.GetVtxNbrhood(item, v_i, ref vOther, ref oppV, ref num2, ref t, ref t2);
			Vector3d vertex2 = mesh.GetVertex(vOther);
			Vector3d vertex3 = mesh.GetVertex(oppV);
			double num3 = MathUtil.VectorCot((vertex - vertex3).Normalized, (vertex2 - vertex3).Normalized);
			if (num3 == 0.0)
			{
				flag = true;
				break;
			}
			double num4 = num3;
			if (num2 != -1)
			{
				Vector3d vertex4 = mesh.GetVertex(num2);
				double num5 = MathUtil.VectorCot((vertex - vertex4).Normalized, (vertex2 - vertex4).Normalized);
				if (num5 == 0.0)
				{
					flag = true;
					break;
				}
				num4 += num5;
			}
			zero += num4 * vertex2;
			num += num4;
		}
		if (flag || Math.Abs(num) < 1E-08)
		{
			return vertex;
		}
		return zero / num;
	}

	public static double VoronoiArea(DMesh3 mesh, int v_i)
	{
		double num = 0.0;
		Vector3d vertex = mesh.GetVertex(v_i);
		foreach (int item in mesh.VtxTrianglesItr(v_i))
		{
			Index3i triangle = mesh.GetTriangle(item);
			int num2 = ((triangle[0] != v_i) ? ((triangle[1] == v_i) ? 1 : 2) : 0);
			Vector3d vertex2 = mesh.GetVertex(triangle[(num2 + 1) % 3]);
			Vector3d vertex3 = mesh.GetVertex(triangle[(num2 + 2) % 3]);
			if (MathUtil.IsObtuse(vertex, vertex2, vertex3))
			{
				Vector3d v = vertex2 - vertex;
				Vector3d v2 = vertex3 - vertex;
				v.Normalize();
				v2.Normalize();
				double num3 = 0.5 * v.Cross(v2).Length;
				num = ((!(Vector3d.AngleR(v, v2) > Math.PI / 2.0)) ? (num + num3 * 0.25) : (num + num3 * 0.5));
			}
			else
			{
				Vector3d v3 = vertex - vertex2;
				double num4 = v3.Normalize();
				Vector3d v4 = vertex - vertex3;
				double num5 = v4.Normalize();
				Vector3d normalized = (vertex2 - vertex3).Normalized;
				double num6 = MathUtil.VectorCot(v4, normalized);
				double num7 = MathUtil.VectorCot(v3, -normalized);
				num += num4 * num4 * num6 * 0.125;
				num += num5 * num5 * num7 * 0.125;
			}
		}
		return num;
	}

	public static Vector3d MeanValueCentroid(DMesh3 mesh, int v_i)
	{
		Vector3d zero = Vector3d.Zero;
		double num = 0.0;
		Vector3d vertex = mesh.GetVertex(v_i);
		int vOther = -1;
		int oppV = -1;
		int num2 = -1;
		int t = -1;
		int t2 = -1;
		foreach (int item in mesh.VtxEdgesItr(v_i))
		{
			num2 = -1;
			mesh.GetVtxNbrhood(item, v_i, ref vOther, ref oppV, ref num2, ref t, ref t2);
			Vector3d vertex2 = mesh.GetVertex(vOther);
			Vector3d a = vertex2 - vertex;
			double num3 = a.Normalize();
			if (!(num3 < 1E-08))
			{
				Vector3d normalized = (mesh.GetVertex(oppV) - vertex).Normalized;
				double num4 = VectorTanHalfAngle(a, normalized);
				if (num2 != -1)
				{
					Vector3d normalized2 = (mesh.GetVertex(num2) - vertex).Normalized;
					num4 += VectorTanHalfAngle(a, normalized2);
				}
				num4 /= num3;
				zero += num4 * vertex2;
				num += num4;
			}
		}
		if (num < 1E-08)
		{
			return vertex;
		}
		return zero / num;
	}

	public static double VectorTanHalfAngle(Vector3d a, Vector3d b)
	{
		double num = a.Dot(b);
		return Math.Sqrt(MathUtil.Clamp((1.0 - num) / (1.0 + num), 0.0, double.MaxValue));
	}
}
