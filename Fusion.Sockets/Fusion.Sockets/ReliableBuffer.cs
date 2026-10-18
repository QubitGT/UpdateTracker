#define DEBUG
namespace Fusion.Sockets;

internal struct ReliableBuffer
{
	public const int SEQ_BYTES = 4;

	private NetSequencer _sequencer;

	private ReliableList _receiveList;

	private ulong _receiveSequence;

	public int SequenceBits => _sequencer.Bits;

	public static ReliableBuffer Create()
	{
		ReliableBuffer result = default(ReliableBuffer);
		result._sequencer = new NetSequencer(4);
		return result;
	}

	public ulong NextSendSequence()
	{
		return _sequencer.Next();
	}

	public void Dispose()
	{
		_receiveList.Dispose();
	}

	public unsafe bool LateReceive(out void* root, out ReliableId id, out byte* data)
	{
		for (ReliableHeader* ptr = _receiveList.Head; ptr != null; ptr = ptr->Next)
		{
			if (_sequencer.Distance(ptr->Id.Sequence, _receiveSequence) == 1)
			{
				_receiveSequence = ptr->Id.Sequence;
				_receiveList.Remove(ptr);
				root = ptr;
				id = ptr->Id;
				data = (byte*)(ptr + 1);
				return true;
			}
		}
		id = default(ReliableId);
		root = null;
		data = null;
		return false;
	}

	public unsafe void LateFree(ref void* root)
	{
		Native.Free(ref root);
	}

	public unsafe bool Receive(NetBitBuffer* buffer, out ReliableId rid)
	{
		Assert.Always(sizeof(ReliableHeader) == 64, "ReliableHeader size mismatch {0}", sizeof(ReliableHeader));
		ReliableId reliableId = default(ReliableId);
		buffer->ReadBytesAligned(&reliableId, 48);
		if (_sequencer.Distance(reliableId.Sequence, _receiveSequence) == 1)
		{
			_receiveSequence = reliableId.Sequence;
			rid = reliableId;
			return true;
		}
		Assert.Check(buffer->IsOnEvenByte);
		reliableId.SliceLength = buffer->LengthBytes - buffer->OffsetBytes;
		byte* ptr = (byte*)Native.Malloc(reliableId.SliceLength + sizeof(ReliableHeader));
		Native.MemCpy(ptr + sizeof(ReliableHeader), buffer->PadToByteBoundaryAndGetPtr(), reliableId.SliceLength);
		ReliableHeader* ptr2 = (ReliableHeader*)ptr;
		ptr2->Id = reliableId;
		_receiveList.AddLast(ptr2);
		rid = default(ReliableId);
		return false;
	}
}
