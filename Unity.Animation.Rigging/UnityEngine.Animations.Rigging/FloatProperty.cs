namespace UnityEngine.Animations.Rigging;

public struct FloatProperty : IAnimatableProperty<float>
{
	public PropertyStreamHandle value;

	public static FloatProperty Bind(Animator animator, Component component, string name)
	{
		FloatProperty result = default(FloatProperty);
		result.value = animator.BindStreamProperty(component.transform, component.GetType(), name);
		return result;
	}

	public static FloatProperty BindCustom(Animator animator, string property)
	{
		FloatProperty result = default(FloatProperty);
		result.value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Float);
		return result;
	}

	public float Get(AnimationStream stream)
	{
		return value.GetFloat(stream);
	}

	public void Set(AnimationStream stream, float v)
	{
		value.SetFloat(stream, v);
	}
}
