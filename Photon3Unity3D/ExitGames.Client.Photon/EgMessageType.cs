namespace ExitGames.Client.Photon;

internal enum EgMessageType : byte
{
	Init,
	InitResponse,
	Operation,
	OperationResponse,
	Event,
	DisconnectReason,
	InternalOperationRequest,
	InternalOperationResponse,
	Message,
	RawMessage
}
