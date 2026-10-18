namespace UnityEngine.Animations.Rigging;

public interface IAnimatableProperty<T>
{
	T Get(AnimationStream stream);

	void Set(AnimationStream stream, T value);
}
