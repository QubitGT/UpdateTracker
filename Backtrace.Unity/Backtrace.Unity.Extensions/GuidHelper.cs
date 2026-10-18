using System;

namespace Backtrace.Unity.Extensions;

public static class GuidHelper
{
	public static Guid FromLong(long source)
	{
		byte[] array = new byte[16];
		Array.Copy(BitConverter.GetBytes(source), array, 8);
		return new Guid(array);
	}

	public static bool IsNullOrEmpty(string guid)
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return guid == "00000000-0000-0000-0000-000000000000";
		}
		return true;
	}
}
