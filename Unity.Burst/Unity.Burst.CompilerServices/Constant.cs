namespace Unity.Burst.CompilerServices;

public static class Constant
{
	public static bool IsConstantExpression<T>(T t) where T : unmanaged
	{
		return false;
	}

	public unsafe static bool IsConstantExpression(void* t)
	{
		return false;
	}
}
