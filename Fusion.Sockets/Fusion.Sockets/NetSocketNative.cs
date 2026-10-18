#define DEBUG
#define TRACE
#define ENABLE_PROFILER
using System;
using Fusion.Encryption;
using Fusion.Sockets.Stun;
using NanoSockets;

namespace Fusion.Sockets;

internal class NetSocketNative : INetSocket
{
	private EncryptionManager<NetAddress, DataEncryptor> _encryptionManager;

	private EncryptionToken _encryptionToken;

	private NetAddress _remoteEncryptionHandler;

	private unsafe byte* _encryptionBuffer = null;

	private const int EncryptionBufferLength = 2048;

	public bool SupportsMultiThreading => true;

	public void Initialize(NetConfig config)
	{
		Assert.Always(UDP.Initialize() == Status.Ok, "Unable to initialize Socket");
	}

	public NetSocket Create(NetConfig config)
	{
		Socket socket = UDP.Create(config.SocketSendBuffer, config.SocketRecvBuffer);
		Assert.Always(socket.IsCreated, "Unable to create Socket");
		Assert.Always(UDP.SetNonBlocking(socket) == Status.Ok, "Unable to set Socket as NonBlocking");
		NetSocket result = default(NetSocket);
		result.NativeSocket = socket;
		return result;
	}

	public NetAddress Bind(NetSocket socket, NetConfig config)
	{
		Address address = config.Address.NativeAddress;
		if (UDP.Bind(socket.NativeSocket.handle, ref address) != 0)
		{
			UDP.Destroy(ref socket.NativeSocket.handle);
			throw new InvalidOperationException($"Failed to bind socket to {config.Address.NativeAddress}");
		}
		address = default(Address);
		if (UDP.GetAddress(socket.NativeSocket.handle, ref address) != 0)
		{
			UDP.Destroy(ref socket.NativeSocket.handle);
			throw new InvalidOperationException("Failed to resolve address for bound socket");
		}
		address._address0 = config.Address.NativeAddress._address0;
		address._address1 = config.Address.NativeAddress._address1;
		NetAddress result = default(NetAddress);
		result.NativeAddress = address;
		return result;
	}

	public bool CanFragment(NetAddress address)
	{
		return true;
	}

	public bool Poll(NetSocket socket, long timeout)
	{
		return UDP.Poll(socket.NativeSocket.handle, timeout) > 0;
	}

	public unsafe int Receive(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength)
	{
		int received = UDP.Receive(socket.NativeSocket.handle, &address->NativeAddress, buffer, bufferLength);
		if (received > 0 && StunMessage.IsStunMessage(buffer, bufferLength))
		{
			StunClient.TryParseAndStoreStunMessage(address, buffer, received);
			return -1;
		}
		EngineProfiler.Begin("Encryption.Socket.Receive");
		bool flag = HandleEncryptionIngoing(address, ref buffer, bufferLength, ref received);
		EngineProfiler.End();
		if (!flag)
		{
			return -1;
		}
		return received;
	}

	public unsafe int Send(NetSocket socket, NetAddress* address, byte* buffer, int bufferLength, bool reliable = false)
	{
		EngineProfiler.Begin("Encryption.Socket.Send");
		bool flag = HandleEncryptionOutgoing(address, ref buffer, ref bufferLength);
		EngineProfiler.End();
		if (!flag)
		{
			return -1;
		}
		return UDP.Send(socket.NativeSocket.handle, &address->NativeAddress, buffer, bufferLength);
	}

	public void Destroy(NetSocket netSocket)
	{
		ResetEncryption();
		UDP.Destroy(ref netSocket.NativeSocket.handle);
	}

	public unsafe void SetupEncryption(byte[] key, byte[] encryptedKey)
	{
		if (_encryptionManager != null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("SetupEncryption: already setup, ignoring...");
			return;
		}
		if (key == null || key.Length == 0 || Array.TrueForAll(key, (byte b) => b == 0))
		{
			InternalLogStreams.LogTraceEncryption?.Warn("SetupEncryption: no key, ignoring...");
			return;
		}
		_encryptionManager = new EncryptionManager<NetAddress, DataEncryptor>();
		_encryptionManager.RegisterEncryptionKey(default(NetAddress), key);
		_encryptionToken = new EncryptionToken
		{
			Key = key,
			KeyEncrypted = encryptedKey
		};
		_encryptionBuffer = Native.MallocAndClearArray<byte>(2048);
		InternalLogStreams.LogTraceEncryption?.Log($"SetupEncryption: {_encryptionToken}");
		InternalLogStreams.LogDebug?.Log("Encryption is enabled");
	}

	private unsafe bool HandleEncryptionOutgoing(NetAddress* address, ref byte* buffer, ref int bufferLength)
	{
		if (_encryptionManager != null && bufferLength > 1)
		{
			NetAddress netAddress = *address;
			int num = 0;
			Assert.Check(bufferLength <= 2048, "Buffer is too big");
			Native.MemClear(_encryptionBuffer, 2048);
			Native.MemCpy(_encryptionBuffer, buffer, Math.Min(bufferLength, 2048));
			buffer = _encryptionBuffer;
			if (!_encryptionManager.HasEncryptionForHandle(netAddress))
			{
				InternalLogStreams.LogTraceEncryption?.Warn($"Encryption Handler not found: {netAddress}");
				if (_encryptionToken.KeyEncrypted == null)
				{
					InternalLogStreams.LogTraceEncryption?.Warn("Encryption failed. Invalid encryption handler.");
					return false;
				}
				int num2 = _encryptionToken.KeyEncrypted.Length;
				Assert.Check(num2 <= 255, "KeyEncrypted is too big");
				num = num2 + 1;
				Native.MemMove(buffer + num, buffer, bufferLength);
				*buffer = (byte)num2;
				Native.CopyFromArray(buffer + 1, _encryptionToken.KeyEncrypted);
				_remoteEncryptionHandler = netAddress;
				netAddress = default(NetAddress);
				InternalLogStreams.LogTraceEncryption?.Log($"Sending encrypted key: {netAddress}");
			}
			if (!_encryptionManager.Wrap(netAddress, buffer + num, ref bufferLength, 2048))
			{
				InternalLogStreams.LogTraceEncryption?.Warn("Encryption failed. Unable to wrap data.");
				return false;
			}
			bufferLength += num;
		}
		return true;
	}

	private unsafe bool HandleEncryptionIngoing(NetAddress* address, ref byte* buffer, int bufferLength, ref int received)
	{
		if (_encryptionManager != null && received > 1)
		{
			NetAddress netAddress = *address;
			if (!_encryptionManager.HasEncryptionForHandle(netAddress))
			{
				InternalLogStreams.LogTraceEncryption?.Warn($"Encryption Handler not found: {address->ToString()}/{netAddress}");
				if (_encryptionToken.KeyEncrypted != null)
				{
					if (!_remoteEncryptionHandler.Equals(netAddress))
					{
						InternalLogStreams.LogTraceEncryption?.Warn("Encryption failed. Invalid encryption handler.");
						return false;
					}
					_encryptionManager.RegisterEncryptionKey(netAddress, _encryptionToken.Key);
					_remoteEncryptionHandler = default(NetAddress);
					InternalLogStreams.LogTraceEncryption?.Log($"Encryption Handler registered: {address->ToString()}/{netAddress}");
				}
				else
				{
					int length = *buffer;
					byte* ptr = buffer + 1;
					int num = length + 1;
					if (!_encryptionManager.Unwrap(default(NetAddress), ptr, ref length, length))
					{
						InternalLogStreams.LogTraceEncryption?.Warn("Encryption failed. Unable to unwrap keys.");
						return false;
					}
					byte[] array = new byte[length];
					Native.CopyToArray(array, ptr);
					_encryptionManager.RegisterEncryptionKey(netAddress, array);
					received -= num;
					Native.MemMove(buffer, buffer + num, received);
				}
			}
			if (!_encryptionManager.Unwrap(netAddress, buffer, ref received, bufferLength))
			{
				InternalLogStreams.LogTraceEncryption?.Warn("Encryption failed. Unable to unwrap data.");
				return false;
			}
		}
		return true;
	}

	private unsafe void ResetEncryption()
	{
		InternalLogStreams.LogTraceEncryption?.Log("ResetEncryption");
		_encryptionManager?.Dispose();
		_encryptionManager = null;
		_encryptionToken = null;
		_remoteEncryptionHandler = default(NetAddress);
		Native.Free(ref _encryptionBuffer);
	}

	public void DeleteEncryptionKey(NetAddress address)
	{
		_encryptionManager?.DeleteEncryptionKey(address);
	}
}
