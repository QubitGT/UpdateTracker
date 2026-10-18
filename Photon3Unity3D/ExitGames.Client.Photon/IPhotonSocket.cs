using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ExitGames.Client.Photon;

public abstract class IPhotonSocket
{
	protected internal PeerBase peerBase;

	protected readonly ConnectionProtocol Protocol;

	public bool PollReceive;

	public string ConnectAddress;

	protected IPhotonPeerListener Listener => peerBase.Listener;

	protected internal int MTU => peerBase.mtu;

	public PhotonSocketState State { get; protected set; }

	public int SocketErrorCode { get; protected set; }

	public bool Connected => State == PhotonSocketState.Connected;

	public string ServerAddress { get; protected set; }

	public string ProxyServerAddress { get; protected set; }

	public static string ServerIpAddress { get; protected set; }

	public int ServerPort { get; protected set; }

	public bool AddressResolvedAsIpv6 { get; protected internal set; }

	public string UrlProtocol { get; protected set; }

	public string UrlPath { get; protected set; }

	protected internal string SerializationProtocol
	{
		get
		{
			if (peerBase == null || peerBase.photonPeer == null)
			{
				return "GpBinaryV18";
			}
			return Enum.GetName(typeof(SerializationProtocol), peerBase.photonPeer.SerializationProtocolType);
		}
	}

	public IPhotonSocket(PeerBase peerBase)
	{
		if (peerBase == null)
		{
			throw new Exception("Can't init without peer");
		}
		Protocol = peerBase.usedTransportProtocol;
		this.peerBase = peerBase;
		ConnectAddress = this.peerBase.ServerAddress;
	}

	public virtual bool Connect()
	{
		if (State != 0)
		{
			if ((int)peerBase.debugOut >= 1)
			{
				peerBase.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: connection in State: " + State);
			}
			return false;
		}
		if (peerBase == null || Protocol != peerBase.usedTransportProtocol)
		{
			return false;
		}
		if (!TryParseAddress(peerBase.ServerAddress, out var host, out var port, out var scheme, out var absolutePath))
		{
			if ((int)peerBase.debugOut >= 1)
			{
				peerBase.Listener.DebugReturn(DebugLevel.ERROR, "Failed parsing address: " + peerBase.ServerAddress);
			}
			return false;
		}
		ServerIpAddress = string.Empty;
		ServerAddress = host;
		ServerPort = port;
		UrlProtocol = scheme;
		UrlPath = absolutePath;
		if ((int)peerBase.debugOut >= 5)
		{
			Listener.DebugReturn(DebugLevel.ALL, "IPhotonSocket.Connect() " + ServerAddress + ":" + ServerPort + " this.Protocol: " + Protocol);
		}
		return true;
	}

	public abstract bool Disconnect();

	public abstract PhotonSocketError Send(byte[] data, int length);

	public abstract PhotonSocketError Receive(out byte[] data);

	public void HandleReceivedDatagram(byte[] inBuffer, int length, bool willBeReused)
	{
		ITrafficRecorder trafficRecorder = peerBase.photonPeer.TrafficRecorder;
		if (trafficRecorder != null && trafficRecorder.Enabled)
		{
			trafficRecorder.Record(inBuffer, length, incoming: true, peerBase.peerID, this);
		}
		if (peerBase.NetworkSimulationSettings.IsSimulationEnabled)
		{
			if (willBeReused)
			{
				byte[] array = new byte[length];
				Buffer.BlockCopy(inBuffer, 0, array, 0, length);
				peerBase.ReceiveNetworkSimulated(array);
			}
			else
			{
				peerBase.ReceiveNetworkSimulated(inBuffer);
			}
		}
		else
		{
			peerBase.ReceiveIncomingCommands(inBuffer, length);
		}
	}

	public bool ReportDebugOfLevel(DebugLevel levelOfMessage)
	{
		return (int)peerBase.debugOut >= (int)levelOfMessage;
	}

	public void EnqueueDebugReturn(DebugLevel debugLevel, string message)
	{
		peerBase.EnqueueDebugReturn(debugLevel, message);
	}

	protected internal void HandleException(StatusCode statusCode)
	{
		State = PhotonSocketState.Disconnecting;
		peerBase.EnqueueStatusCallback(statusCode);
		peerBase.EnqueueActionForDispatch(delegate
		{
			peerBase.Disconnect();
		});
	}

	protected internal bool TryParseAddress(string url, out string host, out ushort port, out string scheme, out string absolutePath)
	{
		host = string.Empty;
		port = 0;
		scheme = string.Empty;
		absolutePath = string.Empty;
		if (string.IsNullOrEmpty(url))
		{
			return false;
		}
		bool flag = url.Contains("://");
		string uriString = (flag ? url : ("net.tcp://" + url));
		Uri result;
		bool flag2 = Uri.TryCreate(uriString, UriKind.Absolute, out result);
		if (flag2)
		{
			host = result.Host;
			port = (ushort)((flag || url.Contains($":{result.Port}")) ? ((ushort)result.Port) : 0);
			scheme = (flag ? result.Scheme : string.Empty);
			absolutePath = ("/".Equals(result.AbsolutePath) ? string.Empty : result.AbsolutePath);
		}
		return flag2;
	}

	private bool IpAddressTryParse(string strIP, out IPAddress address)
	{
		address = null;
		if (string.IsNullOrEmpty(strIP))
		{
			return false;
		}
		string[] array = strIP.Split(new char[1] { '.' });
		if (array.Length != 4)
		{
			return false;
		}
		byte[] array2 = new byte[4];
		for (int i = 0; i < array.Length; i++)
		{
			string s = array[i];
			byte result = 0;
			if (!byte.TryParse(s, out result))
			{
				return false;
			}
			array2[i] = result;
		}
		if (array2[0] == 0)
		{
			return false;
		}
		address = new IPAddress(array2);
		return true;
	}

	protected internal IPAddress[] GetIpAddresses(string hostname)
	{
		IPAddress address = null;
		if (IPAddress.TryParse(hostname, out address))
		{
			if (address.AddressFamily == AddressFamily.InterNetworkV6 || IpAddressTryParse(hostname, out address))
			{
				return new IPAddress[1] { address };
			}
			HandleException(StatusCode.ServerAddressInvalid);
			return null;
		}
		IPAddress[] array;
		try
		{
			array = Dns.GetHostAddresses(ServerAddress);
		}
		catch (Exception ex)
		{
			try
			{
				IPHostEntry hostByName = Dns.GetHostByName(ServerAddress);
				array = hostByName.AddressList;
			}
			catch (Exception ex2)
			{
				if (ReportDebugOfLevel(DebugLevel.WARNING))
				{
					EnqueueDebugReturn(DebugLevel.WARNING, "GetHostAddresses and GetHostEntry() failed for: " + ServerAddress + ". Caught and handled exceptions:\n" + ex?.ToString() + "\n" + ex2);
				}
				HandleException(StatusCode.DnsExceptionOnConnect);
				return null;
			}
		}
		Array.Sort(array, AddressSortComparer);
		if (ReportDebugOfLevel(DebugLevel.INFO))
		{
			string[] array2 = array.Select((IPAddress x) => x.ToString() + " (" + x.AddressFamily.ToString() + "(" + (int)x.AddressFamily + "))").ToArray();
			string text = string.Join(", ", array2);
			if (ReportDebugOfLevel(DebugLevel.INFO))
			{
				EnqueueDebugReturn(DebugLevel.INFO, ServerAddress + " resolved to " + array2.Length + " address(es): " + text);
			}
		}
		return array;
	}

	private int AddressSortComparer(IPAddress x, IPAddress y)
	{
		if (x.AddressFamily == y.AddressFamily)
		{
			return 0;
		}
		return (x.AddressFamily != AddressFamily.InterNetworkV6) ? 1 : (-1);
	}

	[Obsolete("Use GetIpAddresses instead.")]
	protected internal static IPAddress GetIpAddress(string address)
	{
		IPAddress address2 = null;
		if (IPAddress.TryParse(address, out address2))
		{
			return address2;
		}
		IPHostEntry hostEntry = Dns.GetHostEntry(address);
		IPAddress[] addressList = hostEntry.AddressList;
		IPAddress[] array = addressList;
		foreach (IPAddress iPAddress in array)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
			{
				ServerIpAddress = iPAddress.ToString();
				return iPAddress;
			}
			if (address2 == null && iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				address2 = iPAddress;
			}
		}
		ServerIpAddress = ((address2 != null) ? address2.ToString() : (address + " not resolved"));
		return address2;
	}
}
