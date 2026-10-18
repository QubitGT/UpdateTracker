using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterInt16 : IElementReaderWriter<short>
{
	private static IElementReaderWriter<short> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe short Read(byte* data, int index)
	{
		return *(short*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref short ReadRef(byte* data, int index)
	{
		return ref *(short*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, short val)
	{
		*(short*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(short val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<short> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterInt16);
		}
		return _instance;
	}
}
