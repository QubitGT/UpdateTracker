using Fusion.Sockets;

namespace Fusion;

public static class NetworkRunnerCallbackArgs
{
	public class ConnectRequest
	{
		internal OnConnectionRequestReply? Result;

		public NetAddress RemoteAddress { get; set; }

		public void Accept()
		{
			Result = OnConnectionRequestReply.Ok;
		}

		public void Refuse()
		{
			Result = OnConnectionRequestReply.Refuse;
		}

		public void Waiting()
		{
			Result = OnConnectionRequestReply.Waiting;
		}
	}
}
