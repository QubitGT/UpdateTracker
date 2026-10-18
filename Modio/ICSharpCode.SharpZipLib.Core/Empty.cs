using System;

namespace ICSharpCode.SharpZipLib.Core;

internal static class Empty
{
	public static T[] Array<T>()
	{
		return System.Array.Empty<T>();
	}
}
