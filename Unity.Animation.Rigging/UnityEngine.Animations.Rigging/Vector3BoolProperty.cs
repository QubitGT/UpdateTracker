using System;

namespace UnityEngine.Animations.Rigging;

public struct Vector3BoolProperty : IAnimatableProperty<Vector3Bool>
{
	public PropertyStreamHandle x;

	public PropertyStreamHandle y;

	public PropertyStreamHandle z;

	public static Vector3BoolProperty Bind(Animator animator, Component component, string name)
	{
		Type type = component.GetType();
		Vector3BoolProperty result = default(Vector3BoolProperty);
		result.x = animator.BindStreamProperty(component.transform, type, name + ".x");
		result.y = animator.BindStreamProperty(component.transform, type, name + ".y");
		result.z = animator.BindStreamProperty(component.transform, type, name + ".z");
		return result;
	}

	public static Vector3BoolProperty BindCustom(Animator animator, string name)
	{
		Vector3BoolProperty result = default(Vector3BoolProperty);
		result.x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Bool);
		result.y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Bool);
		result.z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Bool);
		return result;
	}

	public Vector3Bool Get(AnimationStream stream)
	{
		return new Vector3Bool(x.GetBool(stream), y.GetBool(stream), z.GetBool(stream));
	}

	public void Set(AnimationStream stream, Vector3Bool value)
	{
		x.SetBool(stream, value.x);
		y.SetBool(stream, value.y);
		z.SetBool(stream, value.z);
	}
}
