namespace Unity.Burst.CompilerServices;

public static class Aliasing
{
	public unsafe static void ExpectAliased(void* a, void* b)
	{
	}

	public static void ExpectAliased<A, B>(in A a, in B b) where A : struct where B : struct
	{
	}

	public unsafe static void ExpectAliased<B>(void* a, in B b) where B : struct
	{
	}

	public unsafe static void ExpectAliased<A>(in A a, void* b) where A : struct
	{
	}

	public unsafe static void ExpectNotAliased(void* a, void* b)
	{
	}

	public static void ExpectNotAliased<A, B>(in A a, in B b) where A : struct where B : struct
	{
	}

	public unsafe static void ExpectNotAliased<B>(void* a, in B b) where B : struct
	{
	}

	public unsafe static void ExpectNotAliased<A>(in A a, void* b) where A : struct
	{
	}
}
