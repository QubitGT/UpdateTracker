using System;
using System.Runtime.InteropServices;

namespace NanoSockets;

[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct Address : IEquatable<Address>
{
	[FieldOffset(0)]
	public ulong _address0;

	[FieldOffset(8)]
	public ulong _address1;

	[FieldOffset(16)]
	public ushort Port;

	public bool Equals(Address other)
	{
		if (_address0 == other._address0 && _address1 == other._address1)
		{
			return Port == other.Port;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Address other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((17 * 31 + _address0.GetHashCode()) * 31 + _address1.GetHashCode()) * 31 + Port.GetHashCode();
	}

	public override string ToString()
	{
		Address address = this;
		int num = 64;
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		UDP.GetIP(ref address, intPtr, num);
		string arg = Marshal.PtrToStringAnsi(intPtr);
		Marshal.FreeHGlobal(intPtr);
		return string.Format("[{0} Ip={1} Port={2}]", "Address", arg, Port);
	}

	public static Address LocalhostIPv4(ushort port = 0)
	{
		return CreateFromIpPort("127.0.0.1", port);
	}

	public static Address Any(ushort port = 0)
	{
		return CreateFromIpPort("0.0.0.0", port);
	}

	public static Address CreateFromIpPort(string ip, ushort port)
	{
		Address address = default(Address);
		if (UDP.SetIP(ref address, ip) != 0)
		{
			throw new Exception("Can not CreateFromIpPort. IP not Set");
		}
		address.Port = port;
		return address;
	}
}
