namespace UnityEngine.Splines;

public interface IInterpolator<T>
{
	T Interpolate(T from, T to, float t);
}
