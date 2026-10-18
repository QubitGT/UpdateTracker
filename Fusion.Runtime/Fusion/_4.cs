using System;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 16)]
[NetworkStructWeaved(4)]
public struct _4 : INetworkStruct, IFixedStorage
{
	public const int SIZE = 16;

	[FieldOffset(0)]
	public unsafe fixed uint Data[4];

	[FieldOffset(0)]
	[NonSerialized]
	private uint _data0;

	[FieldOffset(4)]
	[NonSerialized]
	private uint _data1;

	[FieldOffset(8)]
	[NonSerialized]
	private uint _data2;

	[FieldOffset(12)]
	[NonSerialized]
	private uint _data3;
}
