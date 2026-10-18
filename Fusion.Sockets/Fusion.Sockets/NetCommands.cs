namespace Fusion.Sockets;

internal enum NetCommands : byte
{
	Connect = 1,
	Accepted,
	Refused,
	Disconnect,
	Ping
}
