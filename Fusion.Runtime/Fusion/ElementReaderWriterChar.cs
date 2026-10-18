using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterChar : IElementReaderWriter<char>
{
	private static IElementReaderWriter<char> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe char Read(byte* data, int index)
	{
		return *(char*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref char ReadRef(byte* data, int index)
	{
		return ref *(char*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, char val)
	{
		*(char*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(char val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<char> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterChar);
		}
		return _instance;
	}
}
