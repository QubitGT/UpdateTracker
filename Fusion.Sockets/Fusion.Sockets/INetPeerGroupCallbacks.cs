namespace Fusion.Sockets;

public interface INetPeerGroupCallbacks
{
	unsafe void OnConnected(NetConnection* connection);

	unsafe void OnDisconnected(NetConnection* connection, NetDisconnectReason reason);

	unsafe void OnUnreliableData(NetConnection* connection, NetBitBuffer* buffer);

	unsafe void OnUnconnectedData(NetBitBuffer* buffer);

	unsafe void OnNotifyData(NetConnection* connection, NetBitBuffer* buffer);

	unsafe void OnNotifyLost(NetConnection* connection, ref NetSendEnvelope envelope);

	unsafe void OnNotifyDelivered(NetConnection* connection, ref NetSendEnvelope envelope);

	void OnNotifyDispose(ref NetSendEnvelope envelope);

	unsafe void OnReliableData(NetConnection* connection, ReliableId id, byte* data);

	OnConnectionRequestReply OnConnectionRequest(NetAddress remoteAddress, byte[] token, byte[] uniqueId);

	void OnConnectionFailed(NetAddress address, NetConnectFailedReason reason);

	unsafe void OnConnectionAttempt(NetConnection* connection, int attempts, int totalConnectAttempts);
}
