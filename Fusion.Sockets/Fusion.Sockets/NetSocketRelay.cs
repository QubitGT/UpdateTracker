#define DEBUG
using System;
using Fusion.Protocol;

namespace Fusion.Sockets;

internal class NetSocketRelay : INetSocket
{
	private long _handle;

	private readonly ICommunicator _communicator;

	public bool SupportsMultiThreading => false;

	public NetAddress LocalAddress => NetAddress.FromActorId(_communicator.CommunicatorID);

	public NetSocketRelay(ICommunicator communicator)
	{
		_communicator = communicator;
	}

	public NetAddress Bind(NetSocket socket, NetConfig config)
	{
		Assert.Check(socket.Handle == _handle);
		return LocalAddress;
	}

	public bool CanFragment(NetAddress address)
	{
		return true;
	}

	public NetSocket Create(NetConfig config)
	{
		NetSocket result = default(NetSocket);
		result.Handle = _handle;
		return result;
	}

	public void Destroy(NetSocket netSocket)
	{
		_handle = 0L;
	}

	public void DeleteEncryptionKey(NetAddress address)
	{
	}

	public void SetupEncryption(byte[] key, byte[] encryptedKey)
	{
	}

	public void Initialize(NetConfig config)
	{
		_handle = Environment.TickCount;
	}

	public bool Poll(NetSocket socket, long timeout)
	{
		Assert.Check(_communicator != null);
		Assert.Check(_handle != 0);
		Assert.Check(_handle == socket.Handle);
		return _communicator.Poll();
	}

	public unsafe int Receive(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength)
	{
		Assert.Check(_communicator != null);
		Assert.Check(_handle != 0);
		Assert.Check(_handle == socket.Handle);
		int senderActor;
		int num = _communicator.ReceivePackage(out senderActor, buffer, bufferLength);
		if (num > 0)
		{
			*address = NetAddress.FromActorId(senderActor);
		}
		return num;
	}

	public unsafe int Send(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength, bool reliable = false)
	{
		Assert.Check(_communicator != null);
		Assert.Check(_handle != 0);
		Assert.Check(_handle == socket.Handle);
		if (address->IsRelayAddr && _communicator != null && _communicator.SendPackage(101, address->ActorId, reliable, buffer, bufferLength))
		{
			return bufferLength;
		}
		return -1;
	}
}
