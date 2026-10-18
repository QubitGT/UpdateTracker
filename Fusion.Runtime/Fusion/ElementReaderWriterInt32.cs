using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterInt32 : IElementReaderWriter<int>
{
	private static IElementReaderWriter<int> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe int Read(byte* data, int index)
	{
		return *(int*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref int ReadRef(byte* data, int index)
	{
		return ref *(int*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, int val)
	{
		*(int*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(int val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<int> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterInt32);
		}
		return _instance;
	}
}
