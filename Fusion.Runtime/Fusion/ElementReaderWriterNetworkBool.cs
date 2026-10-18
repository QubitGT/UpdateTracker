using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterNetworkBool : IElementReaderWriter<NetworkBool>
{
	private static IElementReaderWriter<NetworkBool> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe NetworkBool Read(byte* data, int index)
	{
		return *(NetworkBool*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref NetworkBool ReadRef(byte* data, int index)
	{
		return ref *(NetworkBool*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, NetworkBool val)
	{
		*(NetworkBool*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(NetworkBool val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<NetworkBool> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterNetworkBool);
		}
		return _instance;
	}
}
