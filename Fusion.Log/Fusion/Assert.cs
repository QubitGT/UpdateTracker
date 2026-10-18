using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Fusion;

public static class Assert
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[DoesNotReturn]
	public static void Fail()
	{
		throw new AssertException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[DoesNotReturn]
	public static void Fail(string error)
	{
		throw new AssertException(error);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[DoesNotReturn]
	[StringFormatMethod("format")]
	public static void Fail(string format, params object[] args)
	{
		throw new AssertException(string.Format(format, args));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:null=>halt")]
	public static void Check(object condition)
	{
		if (condition == null)
		{
			throw new AssertException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:null=>halt")]
	public unsafe static void Check(void* condition)
	{
		if (condition == null)
		{
			throw new AssertException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check([DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			throw new AssertException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check([DoesNotReturnIf(false)] bool condition, string error)
	{
		if (!condition)
		{
			throw new AssertException(error);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	[StringFormatMethod("format")]
	public static void Check<T0>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	[StringFormatMethod("format")]
	public static void Check<T0, T1>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	[StringFormatMethod("format")]
	public static void Check<T0, T1, T2>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1, T2 arg2)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1, arg2));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	[StringFormatMethod("format")]
	public static void Check<T0, T1, T2, T3>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1, arg2, arg3));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check<T0>([DoesNotReturnIf(false)] bool condition, T0 arg0)
	{
		if (!condition)
		{
			throw new AssertException($"{arg0}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check<T0, T1>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check<T0, T1, T2>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1, T2 arg2)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1} arg2:{arg2}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check<T0, T1, T2, T3>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1} arg2:{arg2} arg3:{arg3}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Conditional("DEBUG")]
	[AssertionMethod]
	[ContractAnnotation("condition:false=>halt")]
	public static void Check<T0, T1, T2, T3, T4>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1} arg2:{arg2} arg3:{arg3} arg4:{arg4}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Obsolete("Use overload with a message instead")]
	[DoesNotReturn]
	public static void AlwaysFail()
	{
		throw new AssertException();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	public static void AlwaysFail(string error)
	{
		throw new AssertException(error);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	public static void AlwaysFail(object error)
	{
		throw new AssertException(error?.ToString());
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	public static void AlwaysFail<T>(T error) where T : struct
	{
		throw new AssertException(error.ToString());
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[Obsolete("Use overload with a message instead")]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always([DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			throw new AssertException();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always([DoesNotReturnIf(false)] bool condition, string error)
	{
		if (!condition)
		{
			throw new AssertException(error);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	[StringFormatMethod("format")]
	public static void Always<T0>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	[StringFormatMethod("format")]
	public static void Always<T0, T1>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	[StringFormatMethod("format")]
	public static void Always<T0, T1, T2>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1, T2 arg2)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1, arg2));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	[StringFormatMethod("format")]
	public static void Always<T0, T1, T2, T3>([DoesNotReturnIf(false)] bool condition, string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (!condition)
		{
			throw new AssertException(string.Format(format, arg0, arg1, arg2, arg3));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always<T0>([DoesNotReturnIf(false)] bool condition, T0 arg0)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always<T0, T1>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always<T0, T1, T2>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1, T2 arg2)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1} arg2:{arg2}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[ContractAnnotation("condition:false=>halt")]
	[AssertionMethod]
	public static void Always<T0, T1, T2, T3>([DoesNotReturnIf(false)] bool condition, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
	{
		if (!condition)
		{
			throw new AssertException($"arg0:{arg0} arg1:{arg1} arg2:{arg2} arg3:{arg3}");
		}
	}
}
