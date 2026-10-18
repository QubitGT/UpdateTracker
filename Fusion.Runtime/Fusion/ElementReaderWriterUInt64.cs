using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterUInt64 : IElementReaderWriter<ulong>
{
	private static IElementReaderWriter<ulong> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ulong Read(byte* data, int index)
	{
		return *(ulong*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref ulong ReadRef(byte* data, int index)
	{
		return ref *(ulong*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, ulong val)
	{
		*(ulong*)(data + index * 8) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(ulong val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<ulong> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterUInt64);
		}
		return _instance;
	}
}
