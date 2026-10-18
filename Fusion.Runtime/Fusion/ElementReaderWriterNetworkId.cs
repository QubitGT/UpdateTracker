using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterNetworkId : IElementReaderWriter<NetworkId>
{
	private static IElementReaderWriter<NetworkId> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe NetworkId Read(byte* data, int index)
	{
		return *(NetworkId*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref NetworkId ReadRef(byte* data, int index)
	{
		return ref *(NetworkId*)(data + index * 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, NetworkId val)
	{
		*(NetworkId*)(data + index * 4) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(NetworkId val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<NetworkId> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterNetworkId);
		}
		return _instance;
	}
}
