#define DEBUG
#define TRACE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fusion.Async;

namespace Fusion.Sockets.Stun;

internal static class StunClient
{
	private static class TestIPs
	{
		public static readonly IPEndPoint TestNetIpv4 = new IPEndPoint(IPAddress.Parse("203.0.113.0"), 65530);

		public static readonly IPEndPoint TestNetIpv6 = new IPEndPoint(IPAddress.Parse("2001:db8::"), 65530);
	}

	private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<int, NetAddress>> PendingRequests = new ConcurrentDictionary<Guid, ConcurrentDictionary<int, NetAddress>>();

	public static void Reset()
	{
		PendingRequests.Clear();
	}

	public static async Task<StunResult> QueryReflexiveInfo(NetAddress boundLocalAddress, Func<byte[], NetAddress, bool> sendDataViaSocket, NetAddress? customPublicAddress, string customStunServer = null, bool extendedAttempts = false, Func<bool> keepRunning = null)
	{
		if (QueryLocalAddress(boundLocalAddress, out var localAddressFamily, out var localAddress))
		{
			InternalLogStreams.LogTraceStun?.Log($"Local Address: {localAddress}");
			NetAddress publicAddr1 = NetAddress.AnyIPv4Addr;
			NetAddress publicAddr2 = NetAddress.AnyIPv4Addr;
			if (customPublicAddress.HasValue)
			{
				NetAddress value;
				publicAddr2 = (value = customPublicAddress.Value);
				publicAddr1 = value;
				InternalLogStreams.LogDebug?.Log($"[STUN] Bypass Reflexive Info discovery, using {publicAddr1}");
			}
			else
			{
				await StunServers.SetupStunServers(customStunServer);
				int debugMultiplier = ((!extendedAttempts) ? 1 : 10);
				int stunTimeout = 1500 * debugMultiplier;
				Guid requestID = Guid.Empty;
				Stopwatch queryWatch = Stopwatch.StartNew();
				Stopwatch attemptWatch = Stopwatch.StartNew();
				while ((keepRunning == null || keepRunning()) && queryWatch.ElapsedMilliseconds < stunTimeout && (publicAddr1.Equals(NetAddress.AnyIPv4Addr) || publicAddr2.Equals(NetAddress.AnyIPv4Addr)))
				{
					if (QueryPublicAddress(sendDataViaSocket, localAddressFamily, ref requestID, out var skipNATDiscovery))
					{
						InternalLogStreams.LogTraceStun?.Log($"Request sent with ID: {requestID}");
						PendingRequests.TryAdd(requestID, new ConcurrentDictionary<int, NetAddress>());
						attemptWatch.Restart();
						while (attemptWatch.ElapsedMilliseconds < 150)
						{
							await TaskManager.Delay(15);
							if (PendingRequests.TryGetValue(requestID, out var addresses) && addresses.Count > 0)
							{
								NetAddress[] publicAddresses = addresses.Values.ToArray();
								if (publicAddresses.Length >= 1 && publicAddr1.Equals(NetAddress.AnyIPv4Addr))
								{
									publicAddr1 = publicAddresses[0];
									if (skipNATDiscovery)
									{
										publicAddr2 = publicAddr1;
									}
								}
								if (publicAddresses.Length >= 2 && publicAddr2.Equals(NetAddress.AnyIPv4Addr))
								{
									publicAddr2 = publicAddresses[1];
								}
								if (!publicAddr1.Equals(NetAddress.AnyIPv4Addr) && !publicAddr2.Equals(NetAddress.AnyIPv4Addr))
								{
									break;
								}
							}
							addresses = null;
						}
						continue;
					}
					InternalLogStreams.LogDebug?.Warn("[STUN] Unable to send STUN Requests to any STUN Server, aborting.");
					break;
				}
				if (queryWatch.ElapsedMilliseconds > stunTimeout)
				{
					InternalLogStreams.LogDebug?.Log("[STUN] Timeout reached, aborting STUN Query.");
				}
				PendingRequests.TryRemove(requestID, out var _);
			}
			if (publicAddr1.Equals(NetAddress.AnyIPv4Addr) && publicAddr2.Equals(NetAddress.AnyIPv4Addr) && boundLocalAddress.IsIPv6)
			{
				InternalLogStreams.LogDebug?.Log("[STUN] Fallback to using Local Address as Public Address (IPv6)");
				NetAddress value;
				publicAddr2 = (value = boundLocalAddress);
				publicAddr1 = value;
			}
			return StunResult.BuildStunResult(publicAddr1, publicAddr2, localAddress);
		}
		InternalLogStreams.LogDebug?.Warn("[STUN] Unable to resolve Local Address");
		return StunResult.Invalid;
	}

	public unsafe static bool TryParseAndStoreStunMessage(NetAddress* origin, byte* buffer, int bufferLength)
	{
		StunMessage stunMessage = StunMessage.TryParse(buffer, bufferLength);
		if (stunMessage?.MappedAddress == null)
		{
			InternalLogStreams.LogTraceStun?.Log("Invalid STUN Message, no Mapped Address found.");
			return false;
		}
		if (PendingRequests.TryGetValue(stunMessage.ID, out var value))
		{
			int port = stunMessage.MappedAddress.Port;
			string ip = stunMessage.MappedAddress.Address.ToString();
			NetAddress value2 = NetAddress.CreateFromIpPort(ip, (ushort)port);
			if (value2.IsValid && value.TryAdd(origin->GetHashCode(), value2))
			{
				InternalLogStreams.LogTraceStun?.Log($"Reply received (ID={stunMessage.ID}, STUN Server={origin->NativeAddress}): {value2.NativeAddress}");
				return true;
			}
		}
		InternalLogStreams.LogTraceStun?.Log($"Capture STUN Message from {origin->NativeAddress}");
		return true;
	}

	private static bool QueryLocalAddress(NetAddress boundLocalAddress, out AddressFamily addressFamily, out NetAddress localAddress)
	{
		AddressFamily addressFamily2 = (boundLocalAddress.IsIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
		if (boundLocalAddress.HasAddress)
		{
			InternalLogStreams.LogTraceStun?.Log($"Using Local Address ({boundLocalAddress}");
			localAddress = boundLocalAddress;
			addressFamily = addressFamily2;
			return true;
		}
		localAddress = NetAddress.AnyIPv4Addr;
		addressFamily = addressFamily2;
		InternalLogStreams.LogTraceStun?.Log($"Resolving Local Address ({addressFamily2})");
		if (GetLocalAddress(ref addressFamily, out var localIP))
		{
			if (addressFamily != addressFamily2)
			{
				InternalLogStreams.LogTraceStun?.Warn($"No Address of Family {addressFamily2} found, changed to {addressFamily}");
			}
			try
			{
				NetAddress netAddress = NetAddress.CreateFromIpPort(localIP.ToString(), boundLocalAddress.NativeAddress.Port);
				if (netAddress.IsValid)
				{
					localAddress = netAddress;
					return true;
				}
			}
			catch
			{
			}
		}
		InternalLogStreams.LogWarn?.Log("[STUN] Unable to resolve Local Address");
		return false;
	}

	private static bool QueryPublicAddress(Func<byte[], NetAddress, bool> sendAnyData, AddressFamily originalFamily, ref Guid requestID, out bool skipNATDiscovery)
	{
		skipNATDiscovery = false;
		if (sendAnyData == null)
		{
			return false;
		}
		bool flag = originalFamily == AddressFamily.InterNetworkV6;
		List<StunServers.StunServer> stunServer = StunServers.GetStunServer(flag);
		if (stunServer.Count == 0)
		{
			InternalLogStreams.LogWarn?.Log("[STUN] Unable to find any valid STUN Server, aborting Reflexive Address query.");
			return false;
		}
		if (stunServer.Count == 1)
		{
			InternalLogStreams.LogDebug?.Log("[STUN] Only one STUN Server found, skip NAT Type Discovery.");
			skipNATDiscovery = true;
		}
		StunMessage stunMessage = new StunMessage(requestID);
		byte[] arg = stunMessage.Serialize();
		bool flag2 = false;
		foreach (StunServers.StunServer item in stunServer)
		{
			try
			{
				NetAddress netAddress = (flag ? item.IPv6Addr : item.IPv4Addr);
				if (netAddress.IsValid)
				{
					if (sendAnyData(arg, netAddress))
					{
						flag2 = true;
						InternalLogStreams.LogTraceStun?.Log($"Request sent to {netAddress}");
					}
					else
					{
						InternalLogStreams.LogTraceStun?.Warn($"Unable to send request to {netAddress}");
					}
				}
			}
			catch (Exception message)
			{
				InternalLogStreams.LogTraceStun?.Error(message);
			}
		}
		if (!flag2)
		{
			return false;
		}
		requestID = stunMessage.ID;
		return true;
	}

	private static bool GetLocalAddress(ref AddressFamily addressFamily, out IPAddress localIP)
	{
		localIP = IPAddress.None;
		try
		{
			using Socket socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.IP);
			socket.Connect((addressFamily == AddressFamily.InterNetwork) ? TestIPs.TestNetIpv4 : TestIPs.TestNetIpv6);
			localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
		}
		catch
		{
			if (addressFamily != AddressFamily.InterNetworkV6)
			{
				return false;
			}
			addressFamily = AddressFamily.InterNetwork;
			InternalLogStreams.LogTraceStun?.Warn("No Address of Family InterNetworkV6 found, changed to InterNetwork");
			return GetLocalAddress(ref addressFamily, out localIP);
		}
		return !localIP.Equals(IPAddress.None);
	}
}
