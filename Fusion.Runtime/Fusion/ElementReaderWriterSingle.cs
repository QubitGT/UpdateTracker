using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterSingle : IElementReaderWriter<float>
{
	private static IElementReaderWriter<float> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe float Read(byte* data, int index)
	{
		return *(float*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref float ReadRef(byte* data, int index)
	{
		return ref *(float*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, float val)
	{
		*(float*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(float val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<float> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterSingle);
		}
		return _instance;
	}
}
