using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterInt64 : IElementReaderWriter<long>
{
	private static IElementReaderWriter<long> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe long Read(byte* data, int index)
	{
		return *(long*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref long ReadRef(byte* data, int index)
	{
		return ref *(long*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, long val)
	{
		*(long*)(data + index * 8) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(long val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<long> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterInt64);
		}
		return _instance;
	}
}
