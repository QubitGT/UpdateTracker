using System;

namespace Unity.XR.CoreUtils;

public static class SerializableGuidUtil
{
	public static SerializableGuid Create(Guid guid)
	{
		guid.Decompose(out var low, out var high);
		return new SerializableGuid(low, high);
	}
}
