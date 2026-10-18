using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterUInt16 : IElementReaderWriter<ushort>
{
	private static IElementReaderWriter<ushort> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ushort Read(byte* data, int index)
	{
		return *(ushort*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref ushort ReadRef(byte* data, int index)
	{
		return ref *(ushort*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, ushort val)
	{
		*(ushort*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(ushort val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<ushort> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterUInt16);
		}
		return _instance;
	}
}
