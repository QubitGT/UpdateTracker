using System;

namespace UnityEngine.Animations.Rigging;

public struct Vector4Property : IAnimatableProperty<Vector4>
{
	public PropertyStreamHandle x;

	public PropertyStreamHandle y;

	public PropertyStreamHandle z;

	public PropertyStreamHandle w;

	public static Vector4Property Bind(Animator animator, Component component, string name)
	{
		Type type = component.GetType();
		Vector4Property result = default(Vector4Property);
		result.x = animator.BindStreamProperty(component.transform, type, name + ".x");
		result.y = animator.BindStreamProperty(component.transform, type, name + ".y");
		result.z = animator.BindStreamProperty(component.transform, type, name + ".z");
		result.w = animator.BindStreamProperty(component.transform, type, name + ".w");
		return result;
	}

	public static Vector4Property BindCustom(Animator animator, string name)
	{
		Vector4Property result = default(Vector4Property);
		result.x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Float);
		result.y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Float);
		result.z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Float);
		result.w = animator.BindCustomStreamProperty(name + ".w", CustomStreamPropertyType.Float);
		return result;
	}

	public Vector4 Get(AnimationStream stream)
	{
		return new Vector4(x.GetFloat(stream), y.GetFloat(stream), z.GetFloat(stream), w.GetFloat(stream));
	}

	public void Set(AnimationStream stream, Vector4 value)
	{
		x.SetFloat(stream, value.x);
		y.SetFloat(stream, value.y);
		z.SetFloat(stream, value.z);
		w.SetFloat(stream, value.w);
	}
}
