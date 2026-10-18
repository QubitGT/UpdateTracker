namespace Fusion;

public static class FixedStorage
{
	public unsafe static int GetWordCount<T>() where T : unmanaged, IFixedStorage
	{
		return sizeof(T) / 4;
	}
}
