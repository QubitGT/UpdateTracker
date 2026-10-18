using System;
using System.Runtime.InteropServices;

namespace Liv.Lck.Core;

internal static class LckCoreTelemetryNative
{
	private const string __DllName = "lck_core";

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern TelemetryReturnCode send_telemetry_event_without_context(LckTelemetryEventType telemetry_event_type);

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern TelemetryReturnCode send_telemetry_event_with_context(LckTelemetryEventType telemetry_event_type, IntPtr serialized_context_data_ptr, UIntPtr len, SerializationType serialization_type);

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern TelemetryReturnCode clear_context(LckTelemetryContextType context_type);

	[DllImport("lck_core", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern TelemetryReturnCode set_telemetry_context_from_serialized_data(LckTelemetryContextType context_type, IntPtr serialized_context_data_ptr, UIntPtr len, SerializationType serialization_type);
}
