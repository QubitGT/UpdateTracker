using System;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class FixedString
{
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, int arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, int arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, float arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, float arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, string arg2, int arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, string arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, T1 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, T2 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, T3 arg2, int arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in arg2, in fs);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, int arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, int arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, float arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, float arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, string arg2, float arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, string arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, T1 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, T2 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, T3 arg2, float arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in arg2, in fs);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, int arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, int arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, float arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, float arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, int arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, int arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, int arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, int arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, float arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, float arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, float arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, float arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, int arg0, string arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, float arg0, string arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format(FixedString512Bytes formatString, string arg0, string arg1, string arg2, string arg3)
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedString32Bytes fs4 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs4, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in fs4);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, T1 arg0, string arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, T1 arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, T1 arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, T1 arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, T2 arg1, string arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, T1 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, T2 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, T3 arg2, string arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg3);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in arg2, in fs);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, int arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, int arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, int arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, float arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, float arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, float arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, int arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, int arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, int arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, int arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, float arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, float arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, float arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, float arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, int arg0, string arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, float arg0, string arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1>(FixedString512Bytes formatString, string arg0, string arg1, string arg2, T1 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, T1 arg0, string arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, T1 arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, T1 arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, T1 arg1, string arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, T2 arg1, string arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, int arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, int arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, int arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, int arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, float arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, float arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, float arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, float arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, int arg0, string arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, float arg0, string arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2>(FixedString512Bytes formatString, string arg0, string arg1, T1 arg2, T2 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, T1 arg0, string arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, int arg0, T1 arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, float arg0, T1 arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in arg3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString512Bytes Format<T1, T2, T3>(FixedString512Bytes formatString, string arg0, T1 arg1, T2 arg2, T3 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2, in arg3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString512Bytes Format<T1, T2, T3, T4>(FixedString512Bytes formatString, T1 arg0, T2 arg1, T3 arg2, T4 arg3) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes where T4 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString512Bytes dest = default(FixedString512Bytes);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in arg2, in arg3);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, int arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, int arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, int arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, int arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, float arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, float arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, float arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, float arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, string arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, string arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, string arg1, int arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, string arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, T1 arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, T1 arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, T1 arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, T2 arg1, int arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, int arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, int arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, int arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, int arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, float arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, float arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, float arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, float arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, string arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, string arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, string arg1, float arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, string arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, T1 arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, T1 arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, T1 arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, T2 arg1, float arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, int arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, int arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, int arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, int arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, float arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, float arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, float arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, float arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, string arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, string arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, string arg1, string arg2)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedString32Bytes fs3 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs3, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in fs3);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, string arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, T1 arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, T1 arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, T1 arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, T2 arg1, string arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg2);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in fs);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, int arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, int arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, int arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, int arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, float arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, float arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, float arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, float arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, string arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, string arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, string arg1, T1 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, string arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, int arg0, T1 arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, float arg0, T1 arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, string arg0, T1 arg1, T2 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1, in arg2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2, T3>(FixedString128Bytes formatString, T1 arg0, T2 arg1, T3 arg2) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes where T3 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1, in arg2);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, int arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, int arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, int arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, int arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, float arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, float arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, float arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, float arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0, string arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0, string arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0, string arg1)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedString32Bytes fs2 = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs2, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in fs2);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0, string arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg1);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in fs);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, int arg0, T1 arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, float arg0, T1 arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, string arg0, T1 arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs, in arg1);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(FixedString32Bytes),
		typeof(FixedString32Bytes)
	})]
	public static FixedString128Bytes Format<T1, T2>(FixedString128Bytes formatString, T1 arg0, T2 arg1) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes where T2 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0, in arg1);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, int arg0)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs);
		return dest;
	}

	public static FixedString128Bytes Format(FixedString128Bytes formatString, float arg0)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs);
		return dest;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static FixedString128Bytes Format(FixedString128Bytes formatString, string arg0)
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedString32Bytes fs = default(FixedString32Bytes);
		FixedStringMethods.Append(ref fs, arg0);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in fs);
		return dest;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(FixedString32Bytes) })]
	public static FixedString128Bytes Format<T1>(FixedString128Bytes formatString, T1 arg0) where T1 : unmanaged, INativeList<byte>, IUTF8Bytes
	{
		FixedString128Bytes dest = default(FixedString128Bytes);
		FixedStringMethods.AppendFormat(ref dest, in formatString, in arg0);
		return dest;
	}
}
