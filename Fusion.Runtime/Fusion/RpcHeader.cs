#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
public struct RpcHeader
{
	public const int SIZE = 8;

	[FieldOffset(0)]
	public NetworkId Object;

	[FieldOffset(4)]
	public ushort Behaviour;

	[FieldOffset(6)]
	public ushort Method;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("No longer used")]
	public unsafe static int Write(RpcHeader header, byte* data)
	{
		*(RpcHeader*)data = header;
		return 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("No longer used")]
	public unsafe static int ReadSize(byte* data)
	{
		return 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("No longer used")]
	public unsafe static RpcHeader Read(byte* data, out int size)
	{
		size = 8;
		return *(RpcHeader*)data;
	}

	public unsafe static RpcHeader Create(NetworkId id, int behaviour, int method)
	{
		Assert.Check(sizeof(RpcHeader) == 8);
		RpcHeader result = default(RpcHeader);
		result.Object = id;
		result.Behaviour = (ushort)behaviour;
		result.Method = (ushort)method;
		return result;
	}

	public unsafe static RpcHeader Create(int staticRpcKey)
	{
		Assert.Check(sizeof(RpcHeader) == 8);
		RpcHeader result = default(RpcHeader);
		result.Object = default(NetworkId);
		result.Behaviour = 0;
		result.Method = (ushort)staticRpcKey;
		return result;
	}

	public override string ToString()
	{
		return string.Format("[RpcHeader: {0}={1}, {2}={3}, {4}={5}]", "Object", Object, "Behaviour", Behaviour, "Method", Method);
	}
}
