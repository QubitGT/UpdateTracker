using System;

namespace UnityEngine.Splines;

[Serializable]
internal class SplineDataKeyValuePair<T>
{
	public string Key;

	public SplineData<T> Value;
}
