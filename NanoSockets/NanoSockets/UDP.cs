using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NanoSockets;

[SuppressUnmanagedCodeSecurity]
public static class UDP
{
	private const string NativeLibrary = "nanosockets";

	public const int HostNameSize = 1025;

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_initialize")]
	public static extern Status Initialize();

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_deinitialize")]
	public static extern void Deinitialize();

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_create")]
	public static extern long Create(int sendBufferSize, int receiveBufferSize);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_destroy")]
	public static extern void Destroy(ref long socket);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_bind")]
	public static extern int Bind(long socket, IntPtr address);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_bind")]
	public static extern int Bind(long socket, ref Address address);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_connect")]
	public static extern int Connect(long socket, ref Address address);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_set_option")]
	public static extern Status SetOption(long socket, int level, int optionName, ref int optionValue, int optionLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_get_option")]
	public static extern Status GetOption(long socket, int level, int optionName, ref int optionValue, ref int optionLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_set_nonblocking")]
	public static extern Status SetNonBlocking(long socket);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_set_dontfragment")]
	public static extern Status SetDontFragment(long socket, byte df);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_send")]
	public static extern int Send(long socket, IntPtr address, IntPtr buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_send")]
	public static extern int Send(long socket, IntPtr address, byte[] buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_send")]
	public static extern int Send(long socket, ref Address address, IntPtr buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_send")]
	public static extern int Send(long socket, ref Address address, byte[] buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_send")]
	public unsafe static extern int Send(long socket, Address* address, byte* buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_receive")]
	public static extern int Receive(long socket, IntPtr address, IntPtr buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_receive")]
	public static extern int Receive(long socket, IntPtr address, byte[] buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_receive")]
	public static extern int Receive(long socket, ref Address address, IntPtr buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_receive")]
	public static extern int Receive(long socket, ref Address address, byte[] buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_receive")]
	public unsafe static extern int Receive(long socket, Address* address, byte* buffer, int bufferLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_poll")]
	public static extern int Poll(long socket, long timeout);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_get")]
	public static extern Status GetAddress(long socket, ref Address address);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_is_equal")]
	public static extern Status IsEqual(ref Address left, ref Address right);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_set_ip")]
	public static extern Status SetIP(ref Address address, IntPtr ip);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_set_ip")]
	public static extern Status SetIP(ref Address address, string ip);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_get_ip")]
	public static extern Status GetIP(ref Address address, IntPtr ip, int ipLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_get_ip")]
	public static extern Status GetIP(ref Address address, StringBuilder ip, int ipLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_set_hostname")]
	public static extern Status SetHostName(ref Address address, IntPtr name);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_set_hostname")]
	public static extern Status SetHostName(ref Address address, string name);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_get_hostname")]
	public static extern Status GetHostName(ref Address address, IntPtr name, int nameLength);

	[DllImport("nanosockets", CallingConvention = CallingConvention.Cdecl, EntryPoint = "nanosockets_address_get_hostname")]
	public static extern Status GetHostName(ref Address address, StringBuilder name, int nameLength);
}
