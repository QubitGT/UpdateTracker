using System;

namespace UnityEngine.Animations.Rigging;

public struct Vector2Property : IAnimatableProperty<Vector2>
{
	public PropertyStreamHandle x;

	public PropertyStreamHandle y;

	public static Vector2Property Bind(Animator animator, Component component, string name)
	{
		Type type = component.GetType();
		Vector2Property result = default(Vector2Property);
		result.x = animator.BindStreamProperty(component.transform, type, name + ".x");
		result.y = animator.BindStreamProperty(component.transform, type, name + ".y");
		return result;
	}

	public static Vector2Property BindCustom(Animator animator, string name)
	{
		Vector2Property result = default(Vector2Property);
		result.x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Float);
		result.y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Float);
		return result;
	}

	public Vector2 Get(AnimationStream stream)
	{
		return new Vector2(x.GetFloat(stream), y.GetFloat(stream));
	}

	public void Set(AnimationStream stream, Vector2 value)
	{
		x.SetFloat(stream, value.x);
		y.SetFloat(stream, value.y);
	}
}
