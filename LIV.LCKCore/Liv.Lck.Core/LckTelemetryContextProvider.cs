using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Liv.Lck.Core.Serialization;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck.Core;

[Preserve]
internal class LckTelemetryContextProvider : ILckTelemetryContextProvider
{
	private readonly ILckSerializer _serializer = new LckMsgPackSerializer();

	[Preserve]
	public LckTelemetryContextProvider()
	{
	}

	public void SetTelemetryContext(LckTelemetryContextType contextType, Dictionary<string, object> context)
	{
		if (context == null || !context.Any())
		{
			ClearTelemetryContext(contextType);
			return;
		}
		byte[] array = _serializer.Serialize(context);
		IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
		try
		{
			Marshal.Copy(array, 0, intPtr, array.Length);
			TelemetryReturnCode telemetryReturnCode = LckCoreTelemetryNative.set_telemetry_context_from_serialized_data(contextType, intPtr, (UIntPtr)(ulong)array.Length, _serializer.SerializationType);
			if (telemetryReturnCode != 0)
			{
				Debug.LogError($"Failed to set telemetry context (return code={telemetryReturnCode})");
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to set telemetry context: {arg}");
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	public void ClearTelemetryContext(LckTelemetryContextType contextType)
	{
		TelemetryReturnCode telemetryReturnCode = LckCoreTelemetryNative.clear_context(contextType);
		if (telemetryReturnCode != 0)
		{
			Debug.LogError($"Failed to clear telemetry context (return code={telemetryReturnCode})");
		}
	}
}
