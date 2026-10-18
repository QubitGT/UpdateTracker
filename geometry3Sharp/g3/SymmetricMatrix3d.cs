namespace g3;

internal class SymmetricMatrix3d
{
	public double[] entries = new double[6];

	public void SetATA(ref Matrix3d A)
	{
		Vector3d vector3d = A.Column(0);
		Vector3d v = A.Column(1);
		Vector3d v2 = A.Column(2);
		entries[0] = vector3d.LengthSquared;
		entries[1] = v.LengthSquared;
		entries[2] = v2.LengthSquared;
		entries[3] = vector3d.Dot(v);
		entries[4] = vector3d.Dot(v2);
		entries[5] = v.Dot(v2);
	}

	public void quatConjugate01(double c, double s)
	{
		double num = c * c - s * s;
		double num2 = 2.0 * s * c;
		double num3 = num2 * num;
		double num4 = num * num;
		double num5 = num2 * num2;
		double num6 = num4 * entries[0] + 2.0 * num3 * entries[3] + num5 * entries[1];
		double num7 = num5 * entries[0] - 2.0 * num3 * entries[3] + num4 * entries[1];
		double num8 = entries[3] * (num4 - num5) + num3 * (entries[1] - entries[0]);
		double num9 = num * entries[4] + num2 * entries[5];
		double num10 = num * entries[5] - num2 * entries[4];
		entries[0] = num6;
		entries[1] = num7;
		entries[3] = num8;
		entries[4] = num9;
		entries[5] = num10;
	}

	public void quatConjugate02(double c, double s)
	{
		double num = c * c - s * s;
		double num2 = 2.0 * s * c;
		double num3 = num2 * num;
		double num4 = num * num;
		double num5 = num2 * num2;
		double num6 = num4 * entries[0] - 2.0 * num3 * entries[4] + num5 * entries[2];
		double num7 = num5 * entries[0] + 2.0 * num3 * entries[4] + num4 * entries[2];
		double num8 = num * entries[3] - num2 * entries[5];
		double num9 = num3 * (entries[0] - entries[2]) + (num4 - num5) * entries[4];
		double num10 = num2 * entries[3] + num * entries[5];
		entries[0] = num6;
		entries[2] = num7;
		entries[3] = num8;
		entries[4] = num9;
		entries[5] = num10;
	}

	public void quatConjugate12(double c, double s)
	{
		double num = c * c - s * s;
		double num2 = 2.0 * s * c;
		double num3 = num2 * num;
		double num4 = num * num;
		double num5 = num2 * num2;
		double num6 = num4 * entries[1] + 2.0 * num3 * entries[5] + num5 * entries[2];
		double num7 = num5 * entries[1] - 2.0 * num3 * entries[5] + num4 * entries[2];
		double num8 = num * entries[3] + num2 * entries[4];
		double num9 = (0.0 - num2) * entries[3] + num * entries[4];
		double num10 = (num4 - num5) * entries[5] + num3 * (entries[2] - entries[1]);
		entries[1] = num6;
		entries[2] = num7;
		entries[3] = num8;
		entries[4] = num9;
		entries[5] = num10;
	}
}
