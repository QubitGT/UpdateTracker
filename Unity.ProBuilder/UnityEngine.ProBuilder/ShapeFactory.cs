using System;
using System.Reflection;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEngine.ProBuilder;

public static class ShapeFactory
{
	public static ProBuilderMesh Instantiate<T>(PivotLocation pivotType = PivotLocation.Center) where T : Shape, new()
	{
		return Instantiate(typeof(T));
	}

	public static ProBuilderMesh Instantiate(Type shapeType, PivotLocation pivotType = PivotLocation.Center)
	{
		if (shapeType == null)
		{
			throw new ArgumentNullException("shapeType", "Cannot instantiate a null shape.");
		}
		if (shapeType.IsAssignableFrom(typeof(Shape)))
		{
			throw new ArgumentException("Type needs to derive from Shape");
		}
		try
		{
			return Instantiate(Activator.CreateInstance(shapeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null, null) as Shape);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed creating shape \"{shapeType}\". Shapes must contain an empty constructor.\n{arg}");
		}
		return null;
	}

	public static ProBuilderMesh Instantiate(Shape shape)
	{
		if (shape == null)
		{
			throw new ArgumentNullException("shape", "Cannot instantiate a null shape.");
		}
		ProBuilderShape proBuilderShape = new GameObject("Shape").AddComponent<ProBuilderShape>();
		proBuilderShape.SetShape(shape);
		ProBuilderMesh mesh = proBuilderShape.mesh;
		mesh.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;
		if (Attribute.GetCustomAttribute(shape.GetType(), typeof(ShapeAttribute)) is ShapeAttribute shapeAttribute)
		{
			mesh.gameObject.name = shapeAttribute.name;
		}
		return mesh;
	}
}
