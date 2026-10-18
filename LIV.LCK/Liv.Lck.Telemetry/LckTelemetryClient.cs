using System;
using System.Runtime.InteropServices;
using Liv.Lck.Core;
using Liv.Lck.Core.Serialization;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Telemetry;

internal class LckTelemetryClient : ILckTelemetryClient
{
	private readonly ILckSerializer _serializer;

	[Preserve]
	public LckTelemetryClient(ILckSerializer serializer)
	{
		_serializer = serializer;
	}

	public void SendTelemetry(LckTelemetryEvent lckTelemetryEvent)
	{
		if (!Application.isEditor)
		{
			SerializeAndSend(lckTelemetryEvent);
		}
	}

	private void SerializeAndSend(LckTelemetryEvent lckTelemetryEvent)
	{
		byte[] array = _serializer.Serialize(lckTelemetryEvent.Context);
		IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
		try
		{
			Marshal.Copy(array, 0, intPtr, array.Length);
			TelemetryReturnCode telemetryReturnCode = LckCoreTelemetryNative.send_telemetry_event_with_context(lckTelemetryEvent.EventType, intPtr, (UIntPtr)(ulong)array.Length, _serializer.SerializationType);
			if (telemetryReturnCode != 0)
			{
				LckLog.LogError($"Failed to send telemetry event: {lckTelemetryEvent.EventType} (return code={telemetryReturnCode})", "SerializeAndSend", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckTelemetryClient.cs", 51);
			}
		}
		catch (Exception arg)
		{
			LckLog.LogError($"Failed to send telemetry event: {lckTelemetryEvent.EventType}. Exception: {arg}", "SerializeAndSend", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckTelemetryClient.cs", 56);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
