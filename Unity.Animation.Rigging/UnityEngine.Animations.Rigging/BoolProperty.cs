namespace UnityEngine.Animations.Rigging;

public struct BoolProperty : IAnimatableProperty<bool>
{
	public PropertyStreamHandle value;

	public static BoolProperty Bind(Animator animator, Component component, string name)
	{
		BoolProperty result = default(BoolProperty);
		result.value = animator.BindStreamProperty(component.transform, component.GetType(), name);
		return result;
	}

	public static BoolProperty BindCustom(Animator animator, string property)
	{
		BoolProperty result = default(BoolProperty);
		result.value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Bool);
		return result;
	}

	public bool Get(AnimationStream stream)
	{
		return value.GetBool(stream);
	}

	public void Set(AnimationStream stream, bool v)
	{
		value.SetBool(stream, v);
	}
}
