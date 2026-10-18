using System;

namespace UnityEngine.ProBuilder.Shapes;

[AttributeUsage(AttributeTargets.Class)]
public class ShapeAttribute : Attribute
{
	public string name;

	public ShapeAttribute(string n)
	{
		name = n;
	}
}
