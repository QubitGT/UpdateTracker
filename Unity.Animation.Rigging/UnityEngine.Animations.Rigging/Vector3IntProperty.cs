using System;

namespace UnityEngine.Animations.Rigging;

public struct Vector3IntProperty : IAnimatableProperty<Vector3Int>
{
	public PropertyStreamHandle x;

	public PropertyStreamHandle y;

	public PropertyStreamHandle z;

	public static Vector3IntProperty Bind(Animator animator, Component component, string name)
	{
		Type type = component.GetType();
		Vector3IntProperty result = default(Vector3IntProperty);
		result.x = animator.BindStreamProperty(component.transform, type, name + ".x");
		result.y = animator.BindStreamProperty(component.transform, type, name + ".y");
		result.z = animator.BindStreamProperty(component.transform, type, name + ".z");
		return result;
	}

	public static Vector3IntProperty BindCustom(Animator animator, string name)
	{
		Vector3IntProperty result = default(Vector3IntProperty);
		result.x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Int);
		result.y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Int);
		result.z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Int);
		return result;
	}

	public Vector3Int Get(AnimationStream stream)
	{
		return new Vector3Int(x.GetInt(stream), y.GetInt(stream), z.GetInt(stream));
	}

	public void Set(AnimationStream stream, Vector3Int value)
	{
		x.SetInt(stream, value.x);
		y.SetInt(stream, value.y);
		z.SetInt(stream, value.z);
	}
}
