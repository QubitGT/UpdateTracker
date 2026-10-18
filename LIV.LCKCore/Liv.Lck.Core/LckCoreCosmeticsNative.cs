using System;
using System.Runtime.InteropServices;

namespace Liv.Lck.Core;

internal static class LckCoreCosmeticsNative
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void announce_player_presence_for_session_on_presence_expiry_received_delegate(ulong time_until_expiration_seconds);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void get_local_user_cosmetics_on_cosmetic_available_delegate(IntPtr serialized_cosmetic_data_ptr, UIntPtr serialized_data_length, SerializationType serialization_type);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void get_user_cosmetics_for_session_on_cosmetic_available_delegate(IntPtr serialized_cosmetic_data_ptr, UIntPtr serialized_data_length, SerializationType serialization_type);

	private const string __DllName = "lck_core";

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern CosmeticsReturnCode get_user_cosmetics_for_session(IntPtr player_ids_array_ptr, UIntPtr player_ids_len, IntPtr session_id, get_user_cosmetics_for_session_on_cosmetic_available_delegate on_cosmetic_available);

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern CosmeticsReturnCode get_local_user_cosmetics(get_local_user_cosmetics_on_cosmetic_available_delegate on_cosmetic_available);

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern CosmeticsReturnCode announce_player_presence_for_session(IntPtr player_id, IntPtr session_id, announce_player_presence_for_session_on_presence_expiry_received_delegate on_presence_expiry_received);
}
