namespace UnityEngine.Animations.Rigging;

public struct IntProperty : IAnimatableProperty<int>
{
	public PropertyStreamHandle value;

	public static IntProperty Bind(Animator animator, Component component, string name)
	{
		IntProperty result = default(IntProperty);
		result.value = animator.BindStreamProperty(component.transform, component.GetType(), name);
		return result;
	}

	public static IntProperty BindCustom(Animator animator, string property)
	{
		IntProperty result = default(IntProperty);
		result.value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Int);
		return result;
	}

	public int Get(AnimationStream stream)
	{
		return value.GetInt(stream);
	}

	public void Set(AnimationStream stream, int v)
	{
		value.SetInt(stream, v);
	}
}
