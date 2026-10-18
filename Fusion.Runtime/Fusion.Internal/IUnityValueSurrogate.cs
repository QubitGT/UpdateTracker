namespace Fusion.Internal;

public interface IUnityValueSurrogate<T> : IUnitySurrogate
{
	T DataProperty { get; set; }
}
