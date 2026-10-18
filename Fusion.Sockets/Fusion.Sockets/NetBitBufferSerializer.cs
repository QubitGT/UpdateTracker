using System.Text;

namespace Fusion.Sockets;

public struct NetBitBufferSerializer
{
	private bool _write;

	private unsafe NetBitBuffer* _buffer;

	public bool Writing => _write;

	public bool Reading => !_write;

	public unsafe NetBitBuffer* Buffer => _buffer;

	private unsafe NetBitBufferSerializer(NetBitBuffer* buffer, bool write)
	{
		_write = write;
		_buffer = buffer;
	}

	public unsafe static NetBitBufferSerializer Writer(NetBitBuffer* buffer)
	{
		return new NetBitBufferSerializer(buffer, write: true);
	}

	public unsafe static NetBitBufferSerializer Reader(NetBitBuffer* buffer)
	{
		return new NetBitBufferSerializer(buffer, write: false);
	}

	public unsafe bool Check(bool value)
	{
		if (_write)
		{
			return _buffer->WriteBoolean(value);
		}
		return _buffer->ReadBoolean();
	}

	public unsafe void Serialize(ref float value)
	{
		if (_write)
		{
			_buffer->WriteSingle(value);
		}
		else
		{
			value = _buffer->ReadSingle();
		}
	}

	public unsafe void Serialize(ref byte value)
	{
		if (_write)
		{
			_buffer->WriteByte(value);
		}
		else
		{
			value = _buffer->ReadByte();
		}
	}

	public unsafe void Serialize(ref bool value)
	{
		if (_write)
		{
			_buffer->WriteBoolean(value);
		}
		else
		{
			value = _buffer->ReadBoolean();
		}
	}

	public unsafe void Serialize(ref int value)
	{
		if (_write)
		{
			_buffer->WriteInt32(value);
		}
		else
		{
			value = _buffer->ReadInt32();
		}
	}

	public unsafe void Serialize(ref uint value)
	{
		if (_write)
		{
			_buffer->WriteUInt32(value);
		}
		else
		{
			value = _buffer->ReadUInt32();
		}
	}

	public unsafe void Serialize(ref ulong value)
	{
		if (_write)
		{
			_buffer->WriteUInt64(value);
		}
		else
		{
			value = _buffer->ReadUInt64();
		}
	}

	public unsafe void Serialize(ref string value)
	{
		if (_write)
		{
			_buffer->WriteString(value, Encoding.UTF8);
		}
		else
		{
			value = _buffer->ReadString(Encoding.UTF8);
		}
	}
}
