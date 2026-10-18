#define DEBUG
using Fusion.Protocol;

namespace Fusion.Sockets;

internal class NetSocketHybrid : INetSocket
{
	private NetSocket _relayNetSocketRef;

	private NetAddress _relayAddress;

	private readonly NetSocketRelay _relaySocket;

	private readonly NetSocketNative _nativeSocket;

	private readonly ICommunicator _client;

	public bool SupportsMultiThreading => false;

	public NetSocketHybrid(ICommunicator client)
	{
		_client = client;
		_relaySocket = new NetSocketRelay(_client);
		_nativeSocket = new NetSocketNative();
	}

	public void Initialize(NetConfig config)
	{
		_relaySocket.Initialize(config);
		_nativeSocket.Initialize(config);
	}

	public NetSocket Create(NetConfig config)
	{
		_relayNetSocketRef = _relaySocket.Create(config);
		return _nativeSocket.Create(config);
	}

	public void Destroy(NetSocket netSocket)
	{
		_relaySocket.Destroy(_relayNetSocketRef);
		_nativeSocket.Destroy(netSocket);
	}

	public void DeleteEncryptionKey(NetAddress address)
	{
		_relaySocket.DeleteEncryptionKey(address);
		_nativeSocket.DeleteEncryptionKey(address);
	}

	public void SetupEncryption(byte[] key, byte[] encryptedKey)
	{
		_relaySocket.SetupEncryption(key, encryptedKey);
		_nativeSocket.SetupEncryption(key, encryptedKey);
	}

	public NetAddress Bind(NetSocket socket, NetConfig config)
	{
		_relayAddress = _relaySocket.Bind(_relayNetSocketRef, config);
		return _nativeSocket.Bind(socket, config);
	}

	public bool CanFragment(NetAddress address)
	{
		return address.IsRelayAddr ? _relaySocket.CanFragment(address) : _nativeSocket.CanFragment(address);
	}

	public bool Poll(NetSocket socket, long timeout)
	{
		return _relaySocket.Poll(_relayNetSocketRef, timeout) || _nativeSocket.Poll(socket, timeout);
	}

	public unsafe int Receive(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength)
	{
		Assert.Check(address->Equals(default(NetAddress)), address->ToString());
		int num = _nativeSocket.Receive(socket, address, buffer, bufferLength);
		if (num < 0)
		{
			num = _relaySocket.Receive(_relayNetSocketRef, address, buffer, bufferLength);
		}
		return num;
	}

	public unsafe int Send(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength, bool reliable = false)
	{
		if (address->IsRelayAddr)
		{
			return _relaySocket.Send(_relayNetSocketRef, address, buffer, bufferLength, reliable);
		}
		return _nativeSocket.Send(socket, address, buffer, bufferLength, reliable);
	}
}
