#define TRACE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fusion.Async;

namespace Fusion.Sockets.Stun;

internal static class StunServers
{
	public class StunServer
	{
		private sealed class Pv4AddrEqualityComparer : IEqualityComparer<StunServer>
		{
			public bool Equals(StunServer x, StunServer y)
			{
				if (x == y)
				{
					return true;
				}
				if (x == null)
				{
					return false;
				}
				if (y == null)
				{
					return false;
				}
				if (x.GetType() != y.GetType())
				{
					return false;
				}
				return x.IPv4Addr.Equals(y.IPv4Addr);
			}

			public int GetHashCode(StunServer obj)
			{
				return obj.IPv4Addr.GetHashCode();
			}
		}

		public NetAddress IPv4Addr;

		public NetAddress IPv6Addr;

		public bool HasIPv4Support => IPv4Addr.IsValid;

		public bool HasIPv6Support => IPv6Addr.IsValid;

		public static IEqualityComparer<StunServer> StunServerEqualityComparer { get; } = new Pv4AddrEqualityComparer();


		public override string ToString()
		{
			return string.Format("[{0}: {1}: {2}, {3}: {4}]", "StunServer", "IPv4Addr", IPv4Addr, "IPv6Addr", IPv6Addr);
		}
	}

	private static readonly string[] DefaultStunServerList;

	private static volatile StunServer[] _stunServers;

	private static volatile bool _runningResolution;

	static StunServers()
	{
		DefaultStunServerList = new string[4] { "stun1.l.google.com:19302", "stun2.l.google.com:19302", "stun3.l.google.com:19302", "stun4.l.google.com:19302" };
	}

	public static List<StunServer> GetStunServer(bool IPv6Support)
	{
		List<StunServer> list = new List<StunServer>();
		if (_stunServers == null)
		{
			return list;
		}
		StunServer[] stunServers = _stunServers;
		foreach (StunServer stunServer in stunServers)
		{
			if (stunServer != null && stunServer.HasIPv4Support && (!IPv6Support || stunServer.HasIPv6Support))
			{
				list.Add(stunServer);
			}
		}
		return list;
	}

	public static async Task SetupStunServers(string customStunServer = null)
	{
		while (_runningResolution)
		{
			await TaskManager.Delay(10);
		}
		if (_stunServers != null)
		{
			return;
		}
		_runningResolution = true;
		HashSet<StunServer> stunServers = new HashSet<StunServer>(StunServer.StunServerEqualityComparer);
		customStunServer = customStunServer?.Trim();
		if (!string.IsNullOrEmpty(customStunServer))
		{
			string[] customStunServers = (from s in customStunServer.Split(';')
				select s.Trim()).ToArray();
			string[] array = customStunServers;
			foreach (string stunServerAddress2 in array)
			{
				StunServer customStunServerResolved = await ResolveStunServerInfo(stunServerAddress2);
				if (customStunServerResolved != null)
				{
					stunServers.Add(customStunServerResolved);
				}
			}
		}
		if (_stunServers == null)
		{
			string[] defaultStunServerList = DefaultStunServerList;
			foreach (string stunServerAddress in defaultStunServerList)
			{
				StunServer server = await ResolveStunServerInfo(stunServerAddress);
				if (server != null)
				{
					stunServers.Add(server);
				}
			}
			_stunServers = stunServers.ToArray();
		}
		_runningResolution = false;
	}

	private static async Task<StunServer> ResolveStunServerInfo(string stunServerAddress)
	{
		if (string.IsNullOrEmpty(stunServerAddress))
		{
			return null;
		}
		string[] addressParts = stunServerAddress.Split(':');
		if (addressParts.Length == 2 && ushort.TryParse(addressParts[1], out var port) && port != 0)
		{
			string ipOrName = addressParts[0];
			StunServer stunServer = new StunServer();
			if (IPAddress.TryParse(ipOrName, out var address))
			{
				NetAddress netAddress = NetAddress.CreateFromIpPort(address.ToString(), port);
				if (address.AddressFamily == AddressFamily.InterNetwork)
				{
					stunServer.IPv4Addr = netAddress;
				}
			}
			else
			{
				try
				{
					IPAddress[] array = await Dns.GetHostAddressesAsync(ipOrName);
					foreach (IPAddress serverAddress in array)
					{
						InternalLogStreams.LogTraceStun?.Log($"Server {ipOrName} IP: {serverAddress}");
						if (!IPAddress.IsLoopback(serverAddress))
						{
							if (stunServer.IPv4Addr.Equals(default(NetAddress)) && serverAddress.AddressFamily == AddressFamily.InterNetwork)
							{
								stunServer.IPv4Addr = NetAddress.CreateFromIpPort(serverAddress.ToString(), port);
							}
							if (stunServer.IPv6Addr.Equals(default(NetAddress)) && serverAddress.AddressFamily == AddressFamily.InterNetworkV6)
							{
								stunServer.IPv6Addr = NetAddress.CreateFromIpPort(serverAddress.ToString(), port);
							}
							if (!stunServer.IPv4Addr.Equals(default(NetAddress)) && !stunServer.IPv6Addr.Equals(default(NetAddress)))
							{
								break;
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
			if (stunServer.HasIPv4Support)
			{
				InternalLogStreams.LogTraceStun?.Log($"Server {ipOrName} resolved as {stunServer}");
				return stunServer;
			}
			InternalLogStreams.LogTraceStun?.Warn("Unable to resolve Address for " + ipOrName);
			return null;
		}
		InternalLogStreams.LogTraceStun?.Warn("Unable to parse STUN Server Address");
		return null;
	}
}
