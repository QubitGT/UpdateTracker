namespace Oculus.Interaction.Input;

public interface IOneEuroFilter<TData>
{
	TData Value { get; }

	void SetProperties(in OneEuroFilterPropertyBlock properties);

	TData Step(TData rawValue, float deltaTime = 1f / 60f);

	void Reset();
}
