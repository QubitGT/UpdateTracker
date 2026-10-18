using System;
using System.Net.Sockets;

namespace Photon.Realtime;

public class PingMono : PhotonPing
{
	private Socket sock;

	public override bool StartPing(string ip)
	{
		Init();
		try
		{
			if (sock == null)
			{
				if (ip.Contains("."))
				{
					sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				}
				else
				{
					sock = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				}
				sock.ReceiveTimeout = 5000;
				int port = ((RegionHandler.PortToPingOverride != 0) ? RegionHandler.PortToPingOverride : 5055);
				sock.Connect(ip, port);
			}
			PingBytes[PingBytes.Length - 1] = PingId;
			sock.Send(PingBytes);
			PingBytes[PingBytes.Length - 1] = (byte)(PingId + 1);
		}
		catch (Exception value)
		{
			sock = null;
			Console.WriteLine(value);
		}
		return false;
	}

	public override bool Done()
	{
		if (GotResult || sock == null)
		{
			return true;
		}
		int num = 0;
		try
		{
			if (!sock.Poll(0, SelectMode.SelectRead))
			{
				return false;
			}
			num = sock.Receive(PingBytes, SocketFlags.None);
		}
		catch (Exception ex)
		{
			if (sock != null)
			{
				sock.Close();
				sock = null;
			}
			DebugString = DebugString + " Exception of socket! " + ex.GetType()?.ToString() + " ";
			return true;
		}
		bool flag = PingBytes[PingBytes.Length - 1] == PingId && num == PingLength;
		if (!flag)
		{
			DebugString += " ReplyMatch is false! ";
		}
		Successful = flag;
		GotResult = true;
		return true;
	}

	public override void Dispose()
	{
		try
		{
			sock.Close();
		}
		catch
		{
		}
		sock = null;
	}
}
