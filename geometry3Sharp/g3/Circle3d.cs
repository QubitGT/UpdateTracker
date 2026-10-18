using System;

namespace g3;

public class Circle3d
{
	public Vector3d Center;

	public Vector3d Normal;

	public Vector3d PlaneX;

	public Vector3d PlaneY;

	public double Radius;

	public bool IsReversed;

	public bool IsClosed => true;

	public double ParamLength => 1.0;

	public bool HasArcLength => true;

	public double ArcLength => Math.PI * 2.0 * Radius;

	public double Circumference => Math.PI * 2.0 * Radius;

	public double Diameter => 2.0 * Radius;

	public double Area => Math.PI * Radius * Radius;

	public Circle3d(Vector3d center, double radius, Vector3d axis0, Vector3d axis1, Vector3d normal)
	{
		IsReversed = false;
		Center = center;
		Normal = normal;
		PlaneX = axis0;
		PlaneY = axis1;
		Radius = radius;
	}

	public Circle3d(Frame3f frame, double radius, int nNormalAxis = 1)
	{
		IsReversed = false;
		Center = frame.Origin;
		Normal = frame.GetAxis(nNormalAxis);
		PlaneX = frame.GetAxis((nNormalAxis + 1) % 3);
		PlaneY = frame.GetAxis((nNormalAxis + 2) % 3);
		Radius = radius;
	}

	public Circle3d(Vector3d center, double radius)
	{
		IsReversed = false;
		Center = center;
		Normal = Vector3d.AxisY;
		PlaneX = Vector3d.AxisX;
		PlaneY = Vector3d.AxisZ;
		Radius = radius;
	}

	public void Reverse()
	{
		IsReversed = !IsReversed;
	}

	public Vector3d SampleDeg(double degrees)
	{
		double num = degrees * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return Center + num2 * Radius * PlaneX + num3 * Radius * PlaneY;
	}

	public Vector3d SampleRad(double radians)
	{
		double num = Math.Cos(radians);
		double num2 = Math.Sin(radians);
		return Center + num * Radius * PlaneX + num2 * Radius * PlaneY;
	}

	public Vector3d SampleT(double t)
	{
		double num = (IsReversed ? ((0.0 - t) * (Math.PI * 2.0)) : (t * (Math.PI * 2.0)));
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return Center + num2 * Radius * PlaneX + num3 * Radius * PlaneY;
	}

	public Vector3d SampleArcLength(double a)
	{
		double num = a / ArcLength;
		double num2 = (IsReversed ? ((0.0 - num) * (Math.PI * 2.0)) : (num * (Math.PI * 2.0)));
		double num3 = Math.Cos(num2);
		double num4 = Math.Sin(num2);
		return Center + num3 * Radius * PlaneX + num4 * Radius * PlaneY;
	}
}
