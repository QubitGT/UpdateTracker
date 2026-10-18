#define DEBUG
using System;
using System.Collections.Generic;

namespace Fusion.Protocol;

internal class ProtocolSerializer
{
	private readonly BitStream _writeStream = new BitStream(new byte[8192]);

	private readonly BitStream _readStream = new BitStream(new byte[0]);

	private readonly Dictionary<Type, byte> _typeToId = new Dictionary<Type, byte>();

	private readonly Dictionary<byte, Message> _idToType = new Dictionary<byte, Message>();

	public ProtocolSerializer()
	{
		RegisterProtocolMsg(1, new Join());
		RegisterProtocolMsg(2, new NetworkConfigSync());
		RegisterProtocolMsg(3, new ReflexiveInfo());
		RegisterProtocolMsg(4, new Disconnect());
		RegisterProtocolMsg(5, new Start());
		RegisterProtocolMsg(6, new Snapshot());
		RegisterProtocolMsg(7, new HostMigration());
		RegisterProtocolMsg(8, new PlayerRefMapping());
		RegisterProtocolMsg(9, new ChangeMasterClient());
		RegisterProtocolMsg(10, new DummyTrafficSync());
	}

	public bool ConvertToMessages(byte[] data, List<Message> messages)
	{
		Assert.Check(data != null, "Data buffer can't be null to convert Messages");
		try
		{
			_readStream.SetBuffer(data, data.Length);
			_readStream.Reading = true;
			messages.Clear();
			Message msg;
			while (ReadNext(_readStream, out msg))
			{
				messages.Add(msg);
			}
			return messages.Count > 0;
		}
		catch (Exception message)
		{
			InternalLogStreams.LogDebug?.Error(message);
		}
		return false;
	}

	public bool ConvertToBuffer(Message message, out BitStream buffer)
	{
		try
		{
			_writeStream.Reset();
			_writeStream.Writing = true;
			if (PackNext(message, _writeStream))
			{
				buffer = _writeStream;
				return true;
			}
		}
		catch (IndexOutOfRangeException)
		{
			_writeStream.Expand();
		}
		catch (Exception message2)
		{
			InternalLogStreams.LogDebug?.Error(message2);
		}
		buffer = null;
		return false;
	}

	private void RegisterProtocolMsg(byte id, Message message)
	{
		_idToType.Add(id, message);
		_typeToId.Add(message.GetType(), id);
	}

	private bool PackNext(Message msg, BitStream stream)
	{
		int position = stream.Position;
		Assert.Check(stream.Writing);
		Assert.Check(_typeToId.ContainsKey(msg.GetType()), "Message {0} not registered", msg.GetType());
		stream.WriteByte(_typeToId[msg.GetType()]);
		msg.Serialize(stream);
		if (stream.Overflowing)
		{
			stream.Position = position;
			return false;
		}
		return true;
	}

	private bool ReadNext(BitStream stream, out Message msg)
	{
		try
		{
			Assert.Check(stream.Reading);
			if (!stream.CanRead(8))
			{
				msg = null;
				return false;
			}
			byte key = stream.ReadByte();
			msg = _idToType[key].Clone();
			msg.Serialize(stream);
			return true;
		}
		catch
		{
			msg = null;
			return false;
		}
	}
}
