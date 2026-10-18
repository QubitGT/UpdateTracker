using System;

namespace g3;

public class Sphere3Generator_NormalizedCube : GridBox3Generator
{
	public enum NormalizationTypes
	{
		NormalizedVector,
		CubeMapping
	}

	public double Radius = 1.0;

	private NormalizationTypes NormalizeType = NormalizationTypes.CubeMapping;

	public override MeshGenerator Generate()
	{
		base.Generate();
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3d vector3d = vertices[i] - Box.Center;
			if (NormalizeType == NormalizationTypes.CubeMapping)
			{
				double num = vector3d.Dot(Box.AxisX) / Box.Extent.x;
				double num2 = vector3d.Dot(Box.AxisY) / Box.Extent.y;
				double num3 = vector3d.Dot(Box.AxisZ) / Box.Extent.z;
				double num4 = num * num;
				double num5 = num2 * num2;
				double num6 = num3 * num3;
				double num7 = num * Math.Sqrt(1.0 - num5 * 0.5 - num6 * 0.5 + num5 * num6 / 3.0);
				double num8 = num2 * Math.Sqrt(1.0 - num4 * 0.5 - num6 * 0.5 + num4 * num6 / 3.0);
				double num9 = num3 * Math.Sqrt(1.0 - num4 * 0.5 - num5 * 0.5 + num4 * num5 / 3.0);
				vector3d = num7 * Box.AxisX + num8 * Box.AxisY + num9 * Box.AxisZ;
			}
			vector3d.Normalize();
			vertices[i] = Box.Center + Radius * vector3d;
			normals[i] = (Vector3f)vector3d;
		}
		return this;
	}
}
