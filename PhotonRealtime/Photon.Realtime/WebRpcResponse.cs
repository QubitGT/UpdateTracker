using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

public class WebRpcResponse
{
	public string Name { get; private set; }

	public int ResultCode { get; private set; }

	[Obsolete("Use ResultCode instead")]
	public int ReturnCode => ResultCode;

	public string Message { get; private set; }

	[Obsolete("Use Message instead")]
	public string DebugMessage => Message;

	public Dictionary<string, object> Parameters { get; private set; }

	public WebRpcResponse(OperationResponse response)
	{
		if (response.Parameters.TryGetValue(209, out var value))
		{
			Name = value as string;
		}
		ResultCode = -1;
		if (response.Parameters.TryGetValue(207, out value))
		{
			ResultCode = (byte)value;
		}
		if (response.Parameters.TryGetValue(208, out value))
		{
			Parameters = value as Dictionary<string, object>;
		}
		if (response.Parameters.TryGetValue(206, out value))
		{
			Message = value as string;
		}
	}

	public string ToStringFull()
	{
		return string.Format("{0}={2}: {1} \"{3}\"", Name, SupportClass.DictionaryToString(Parameters), ResultCode, Message);
	}
}
