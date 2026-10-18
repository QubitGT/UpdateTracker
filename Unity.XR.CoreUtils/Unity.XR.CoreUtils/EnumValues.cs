using System;

namespace Unity.XR.CoreUtils;

public static class EnumValues<T>
{
	public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
}
