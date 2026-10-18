#define DEBUG
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using Fusion.Protocol;
using NanoSockets;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
public struct NetAddress : IEquatable<NetAddress>
{
	public sealed class EqualityComparer : IEqualityComparer<NetAddress>
	{
		public bool Equals(NetAddress x, NetAddress y)
		{
			return x.Block0 == y.Block0 && x.Block1 == y.Block1 && x.Block2 == y.Block2;
		}

		public int GetHashCode(NetAddress obj)
		{
			int hashCode = obj.Block0.GetHashCode();
			hashCode = (hashCode * 397) ^ obj.Block1.GetHashCode();
			return (hashCode * 397) ^ obj.Block2.GetHashCode();
		}
	}

	internal static class SubnetMask
	{
		public static NetAddress[] SubnetMasks { get; private set; } = new NetAddress[3]
		{
			CreateFromIpPort("255.0.0.0", 0),
			CreateFromIpPort("255.255.0.0", 0),
			CreateFromIpPort("255.255.255.0", 0)
		};


		public static bool IsSameSubNet(NetAddress addressA, NetAddress addressB)
		{
			if (!addressA.IsValid || !addressB.IsValid)
			{
				return false;
			}
			if (addressA.IsIPv6 ^ addressB.IsIPv6)
			{
				return false;
			}
			if (addressA.IsIPv6 && addressB.IsIPv6)
			{
				return true;
			}
			if (!addressA.IsIPv6 && !addressB.IsIPv6)
			{
				NetAddress[] subnetMasks = SubnetMasks;
				foreach (NetAddress subnetMask in subnetMasks)
				{
					NetAddress networkAddress = GetNetworkAddress(addressA, subnetMask);
					NetAddress networkAddress2 = GetNetworkAddress(addressB, subnetMask);
					if (networkAddress.Equals(networkAddress2))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static NetAddress GetNetworkAddress(NetAddress netAddress, NetAddress subnetMask)
		{
			NetAddress result = Any(0);
			result.Block0 = netAddress.Block0 & subnetMask.Block0;
			result.Block1 = netAddress.Block1 & subnetMask.Block1;
			result.Block2 = netAddress.Block2 & subnetMask.Block2;
			return result;
		}
	}

	[FieldOffset(0)]
	internal Address NativeAddress;

	[FieldOffset(0)]
	internal ulong Block0;

	[FieldOffset(8)]
	internal ulong Block1;

	[FieldOffset(16)]
	internal ulong Block2;

	[FieldOffset(20)]
	private int _actorId;

	internal static NetAddress AnyIPv4Addr = new NetAddress
	{
		NativeAddress = new Address
		{
			_address0 = 0uL,
			_address1 = 4294901760uL,
			Port = 0
		},
		_actorId = 0
	};

	internal static NetAddress AnyIPv6Addr = new NetAddress
	{
		NativeAddress = new Address
		{
			_address0 = 0uL,
			_address1 = 0uL,
			Port = 0
		},
		_actorId = 0
	};

	public int ActorId => _actorId - 1;

	public bool IsRelayAddr => ActorId >= 0;

	public bool IsIPv6 => !IsRelayAddr && (NativeAddress._address0 != 0L || (NativeAddress._address1 & 0xFFFF0000u) != 4294901760u);

	public bool IsIPv4 => !IsRelayAddr && (NativeAddress._address1 & 0xFFFF0000u) == 4294901760u;

	public bool IsValid => !Equals(AnyIPv4Addr) && !Equals(AnyIPv6Addr);

	public bool HasAddress
	{
		get
		{
			if (IsRelayAddr)
			{
				return false;
			}
			if (IsIPv6)
			{
				return NativeAddress._address0 != 0L || NativeAddress._address1 != 0;
			}
			if (IsIPv4)
			{
				return NativeAddress._address0 == 0L && NativeAddress._address1 >> 32 != 0;
			}
			return false;
		}
	}

	public static NetAddress FromActorId(int actorId)
	{
		Assert.Check(actorId >= 0, "ActorID must be 0 or greater");
		NetAddress result = default(NetAddress);
		result._actorId = actorId + 1;
		return result;
	}

	internal static ulong Hash64(NetAddress address)
	{
		ulong num = 17uL;
		num = num * 31 + address.Block0;
		num = num * 31 + address.Block1;
		return num * 31 + address.Block2;
	}

	public static NetAddress LocalhostIPv4(ushort port = 0)
	{
		return CreateFromIpPort("127.0.0.1", port);
	}

	public static NetAddress LocalhostIPv6(ushort port = 0)
	{
		return CreateFromIpPort("::1", port);
	}

	public static NetAddress Any(ushort port = 0)
	{
		return CreateFromIpPort("0.0.0.0", port);
	}

	public static NetAddress AnyIPv6(ushort port = 0)
	{
		return CreateFromIpPort("::", port);
	}

	public static NetAddress CreateFromIpPort(string ip, ushort port)
	{
		if (string.IsNullOrEmpty(ip) || !IPAddress.TryParse(ip, out var _))
		{
			throw new ArgumentException("IP/Port passed are invalid.");
		}
		ip = ip.Split('%')[0];
		Address address2 = default(Address);
		Assert.Always(UDP.SetIP(ref address2, ip) == Status.Ok, "Unable to parse IP. Verify if it represents a valid IP.");
		address2.Port = port;
		NetAddress result = default(NetAddress);
		result.NativeAddress = address2;
		result._actorId = 0;
		return result;
	}

	internal void Serialize(BitStream stream)
	{
		stream.Serialize(ref Block0);
		stream.Serialize(ref Block1);
		stream.Serialize(ref Block2);
	}

	public bool Equals(NetAddress other)
	{
		return Block0 == other.Block0 && Block1 == other.Block1 && Block2 == other.Block2;
	}

	public override bool Equals(object obj)
	{
		return obj is NetAddress other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hashCode = Block0.GetHashCode();
		hashCode = (hashCode * 397) ^ Block1.GetHashCode();
		return (hashCode * 397) ^ Block2.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}: {2}, {3}: {4}, {5}: {6}, {7}: {8}, {9}: {10}, [{11},{12},{13}]]", "NetAddress", "IsValid", IsValid, "ActorId", ActorId, "NativeAddress", NativeAddress, "IsIPv6", IsIPv6, "IsRelayAddr", IsRelayAddr, Block0, Block1, Block2);
	}
}
