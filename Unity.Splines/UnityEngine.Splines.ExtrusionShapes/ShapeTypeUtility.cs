using System;

namespace UnityEngine.Splines.ExtrusionShapes;

internal static class ShapeTypeUtility
{
	public static ShapeType GetShapeType(object obj)
	{
		if (!(obj is Circle))
		{
			if (!(obj is Square))
			{
				if (!(obj is Road))
				{
					if (obj is SplineShape)
					{
						return ShapeType.Spline;
					}
					throw new ArgumentException("obj is not a recognized shape", "obj");
				}
				return ShapeType.Road;
			}
			return ShapeType.Square;
		}
		return ShapeType.Circle;
	}

	public static IExtrudeShape CreateShape(ShapeType type)
	{
		return type switch
		{
			ShapeType.Square => new Square(), 
			ShapeType.Road => new Road(), 
			ShapeType.Spline => new SplineShape(), 
			ShapeType.Circle => new Circle(), 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}
}
