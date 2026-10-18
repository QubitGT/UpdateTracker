using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterUInt32 : IElementReaderWriter<uint>
{
	private static IElementReaderWriter<uint> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe uint Read(byte* data, int index)
	{
		return *(uint*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref uint ReadRef(byte* data, int index)
	{
		return ref *(uint*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, uint val)
	{
		*(uint*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(uint val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<uint> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterUInt32);
		}
		return _instance;
	}
}
