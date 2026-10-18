namespace Fusion.Sockets;

public interface INetSocket
{
	bool SupportsMultiThreading { get; }

	void Initialize(NetConfig config);

	NetSocket Create(NetConfig config);

	NetAddress Bind(NetSocket socket, NetConfig config);

	bool CanFragment(NetAddress address);

	bool Poll(NetSocket socket, long timeout);

	unsafe int Receive(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength);

	unsafe int Send(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength, bool reliable = false);

	void Destroy(NetSocket socket);

	void DeleteEncryptionKey(NetAddress address);

	void SetupEncryption(byte[] key, byte[] encryptedKey);
}
