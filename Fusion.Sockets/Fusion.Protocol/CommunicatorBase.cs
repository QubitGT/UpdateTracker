#define DEBUG
#define TRACE
using System;
using System.Collections.Generic;

namespace Fusion.Protocol;

internal abstract class CommunicatorBase : ICommunicator
{
	protected readonly Dictionary<Type, Action<int, IMessage>> Callbacks = new Dictionary<Type, Action<int, IMessage>>();

	protected readonly Queue<(int, Message)> MessageSendQueue = new Queue<(int, Message)>(64);

	protected readonly Queue<(int, object)> RecvQueue = new Queue<(int, object)>();

	private readonly List<Message> _messageList = new List<Message>(64);

	private readonly ProtocolSerializer _protocolSerializer = new ProtocolSerializer();

	public abstract int CommunicatorID { get; }

	public bool Poll()
	{
		return RecvQueue.Count > 0;
	}

	public void PushPackage(int senderActor, int eventCode, object data)
	{
		senderActor = ((senderActor > 0) ? senderActor : 0);
		switch (eventCode)
		{
		case 101:
			RecvQueue.Enqueue((senderActor, data));
			break;
		case 100:
			HandleProtocolPackage(senderActor, data);
			break;
		case 102:
			InternalLogStreams.LogTraceDummyTraffic?.Log($"Received Dummy Traffic from [{senderActor}]");
			break;
		}
	}

	public unsafe void SendMessage(int targetActor, IMessage message)
	{
		if (_protocolSerializer.ConvertToBuffer((Message)message, out var buffer))
		{
			fixed (byte* buffer2 = buffer.Data)
			{
				if (SendPackage(100, targetActor, reliable: true, buffer2, buffer.BytesRequired))
				{
					InternalLogStreams.LogDebug?.Log($"Sending to [{targetActor}]: {message}");
				}
				else
				{
					MessageSendQueue.Enqueue((targetActor, (Message)message));
				}
			}
		}
		else
		{
			MessageSendQueue.Enqueue((targetActor, (Message)message));
		}
	}

	public virtual void Service()
	{
		if (MessageSendQueue.Count > 0)
		{
			var (targetActor, message) = MessageSendQueue.Dequeue();
			SendMessage(targetActor, message);
		}
	}

	private void HandleProtocolPackage(int actorNr, object data)
	{
		ConvertData(data, out var dataBuffer, out var _);
		if (dataBuffer == null || !_protocolSerializer.ConvertToMessages(dataBuffer, _messageList))
		{
			return;
		}
		foreach (Message message in _messageList)
		{
			if (Callbacks.TryGetValue(message.GetType(), out var value))
			{
				InternalLogStreams.LogDebug?.Log($"Received from [{actorNr}] :: {message}");
				value(actorNr, message);
			}
		}
	}

	public unsafe int ReceivePackage(out int senderActor, byte* buffer, int bufferLength)
	{
		if (Poll())
		{
			var (num, data) = RecvQueue.Dequeue();
			ConvertData(data, out var dataBuffer, out var maxLength);
			if (maxLength > 0)
			{
				senderActor = num;
				Assert.Always(maxLength <= bufferLength, "ReceivePackage overflow");
				fixed (byte* source = dataBuffer)
				{
					Native.MemCpy(buffer, source, maxLength);
				}
				return maxLength;
			}
		}
		senderActor = -1;
		return -1;
	}

	public unsafe abstract bool SendPackage(byte code, int targetActor, bool reliable, byte* buffer, int bufferLength);

	protected abstract void ConvertData(object data, out byte[] dataBuffer, out int maxLength);

	public void RegisterPackageCallback<K>(Action<int, K> callback) where K : IMessage
	{
		Callbacks.Add(typeof(K), delegate(int actor, IMessage msg)
		{
			callback(actor, (K)msg);
		});
	}
}
