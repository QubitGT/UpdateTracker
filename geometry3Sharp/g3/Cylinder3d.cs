using System;

namespace g3;

public class Cylinder3d
{
	public Line3d Axis;

	public double Radius;

	public double Height;

	public double Circumference => Math.PI * 2.0 * Radius;

	public double Diameter => 2.0 * Radius;

	public double Volume => Math.PI * Radius * Radius * Height;

	public Cylinder3d(Line3d axis, double radius, double height)
	{
		Axis = axis;
		Radius = radius;
		Height = height;
	}

	public Cylinder3d(Vector3d center, Vector3d axis, double radius, double height)
	{
		Axis = new Line3d(center, axis);
		Radius = radius;
		Height = height;
	}

	public Cylinder3d(Frame3f frame, double radius, double height, int nNormalAxis = 1)
	{
		Axis = new Line3d(frame.Origin, frame.GetAxis(nNormalAxis));
		Radius = radius;
		Height = height;
	}

	public Cylinder3d(double radius, double height)
	{
		Axis = new Line3d(Vector3d.Zero, Vector3d.AxisY);
		Radius = radius;
		Height = height;
	}
}
