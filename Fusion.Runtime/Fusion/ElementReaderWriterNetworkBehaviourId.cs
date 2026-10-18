using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterNetworkBehaviourId : IElementReaderWriter<NetworkBehaviourId>
{
	private static IElementReaderWriter<NetworkBehaviourId> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe NetworkBehaviourId Read(byte* data, int index)
	{
		return *(NetworkBehaviourId*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref NetworkBehaviourId ReadRef(byte* data, int index)
	{
		return ref *(NetworkBehaviourId*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, NetworkBehaviourId val)
	{
		*(NetworkBehaviourId*)(data + index * 8) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(NetworkBehaviourId val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<NetworkBehaviourId> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterNetworkBehaviourId);
		}
		return _instance;
	}
}
