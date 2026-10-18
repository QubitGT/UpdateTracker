namespace Fusion.Sockets;

internal class NetSocketNull : INetSocket
{
	public bool SupportsMultiThreading { get; }

	public void Initialize(NetConfig config)
	{
	}

	public NetSocket Create(NetConfig config)
	{
		NetSocket result = default(NetSocket);
		result.Handle = 1L;
		return result;
	}

	public NetAddress Bind(NetSocket socket, NetConfig config)
	{
		return default(NetAddress);
	}

	public bool CanFragment(NetAddress address)
	{
		return false;
	}

	public bool Poll(NetSocket socket, long timeout)
	{
		return false;
	}

	public unsafe int Receive(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength)
	{
		return 0;
	}

	public unsafe int Send(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength, bool reliable = false)
	{
		return bufferLength;
	}

	public void Destroy(NetSocket netSocket)
	{
	}

	public void DeleteEncryptionKey(NetAddress address)
	{
	}

	public void SetupEncryption(byte[] key, byte[] encryptedKey)
	{
	}
}
