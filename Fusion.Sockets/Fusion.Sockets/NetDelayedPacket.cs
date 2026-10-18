namespace Fusion.Sockets;

internal struct NetDelayedPacket
{
	public unsafe NetDelayedPacket* Prev;

	public unsafe NetDelayedPacket* Next;

	public double DeliveryTime;

	public NetAddress Address;

	public unsafe byte* Data;

	public int DataLength;

	public unsafe static NetDelayedPacket* Create(int dataLength)
	{
		int num = Native.RoundToMaxAlignment(sizeof(NetDelayedPacket));
		int num2 = Native.RoundToMaxAlignment(dataLength);
		byte* ptr = (byte*)Native.MallocAndClear(num + num2);
		NetDelayedPacket* ptr2 = (NetDelayedPacket*)ptr;
		ptr2->Data = ptr + num;
		ptr2->DataLength = dataLength;
		return ptr2;
	}

	public unsafe static void Dispose(ref NetDelayedPacket* delayed)
	{
		Native.Free(ref delayed);
	}
}
