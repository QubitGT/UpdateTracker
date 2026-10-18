using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modio.Errors;

namespace Modio.Customizations;

internal static class WssHandler
{
	private static ISocketConnection Socket = new SocketConnection();

	private static Dictionary<string, TaskCompletionSource<WssMessage>> WaitingForMessages = new Dictionary<string, TaskCompletionSource<WssMessage>>();

	private static Dictionary<string, Action<WssMessage>> SubscribedMessageListeners = new Dictionary<string, Action<WssMessage>>();

	private static Dictionary<string, WssMessage> UnhandledMessages = new Dictionary<string, WssMessage>();

	private static string GatewayUrl => string.Format("wss://g-{0}.ws.{1}.io/", ModioClient.Settings.GameId, Regex.Match(ModioClient.Settings.ServerURL, "https://[^.]+.(?<domain>.+).io").Groups["domain"]);

	public static async Task<(Error, WssMessage)> WaitForMessage(string messageOperation, bool checkPreviousUnhandledMessages = false)
	{
		if (checkPreviousUnhandledMessages && UnhandledMessages.ContainsKey(messageOperation))
		{
			(Error, WssMessage) result = (Error.None, UnhandledMessages[messageOperation]);
			UnhandledMessages.Remove(messageOperation);
			return result;
		}
		TaskCompletionSource<WssMessage> tcs = new TaskCompletionSource<WssMessage>();
		while (WaitingForMessages.ContainsKey(messageOperation))
		{
			await WaitingForMessages[messageOperation].Task;
		}
		WaitingForMessages.Add(messageOperation, tcs);
		await Task.WhenAny(tcs.Task, Task.Delay(900000));
		Error error = (tcs.Task.IsCompleted ? Error.None : new Error(ErrorCode.WSS_MESSAGE_TIMEOUT));
		WssMessage item = (tcs.Task.IsCompleted ? tcs.Task.Result : default(WssMessage));
		if (!error && string.IsNullOrEmpty(item.operation))
		{
			error = new Error(ErrorCode.OPERATION_CANCELLED);
		}
		return (error, item);
	}

	public static async Task<(Error, T)> DoMessageHandshake<T>(WssMessage message) where T : struct
	{
		Task<(Error, WssMessage)> task = WaitForMessage("device_login");
		Error error = await Send(message);
		ModioLog.Verbose?.Log("[Socket] SENT (" + message.operation + ")");
		if ((bool)error)
		{
			ModioLog.Verbose?.Log("[Socket] Failed to send");
			return (error, default(T));
		}
		ModioLog.Verbose?.Log("[Socket] Waiting for initial response");
		(Error, WssMessage) tuple = await task;
		if (!tuple.Item1)
		{
			ModioLog.Verbose?.Log("[Socket] deserializing initial response");
			if (tuple.Item2.TryGetValue<T>(out var output))
			{
				return (Error.None, output);
			}
			ModioLog.Verbose?.Log("[Socket] Failed to deserialize");
			return (new Error(ErrorCode.WSS_UNEXPECTED_MESSAGE), default(T));
		}
		return (tuple.Item1, default(T));
	}

	public static void CancelWaitingFor(string messageOperation)
	{
		if (WaitingForMessages.ContainsKey(messageOperation))
		{
			TaskCompletionSource<WssMessage> taskCompletionSource = WaitingForMessages[messageOperation];
			WaitingForMessages.Remove(messageOperation);
			taskCompletionSource.SetResult(default(WssMessage));
		}
	}

	private static void CancelAllAwaitingMessages()
	{
		List<TaskCompletionSource<WssMessage>> list = WaitingForMessages.Values.ToList();
		WaitingForMessages.Clear();
		foreach (TaskCompletionSource<WssMessage> item in list)
		{
			item.SetResult(default(WssMessage));
		}
	}

	public static async Task Shutdown()
	{
		await Socket.CloseConnection();
		CancelAllAwaitingMessages();
		UnhandledMessages.Clear();
		await Task.Yield();
	}

	private static async Task<Error> EnsureConnection()
	{
		if (!Socket.Connected())
		{
			return await Socket.SetupConnection(GatewayUrl, Receive, Disconnected);
		}
		return Error.None;
	}

	public static async Task<Error> Send(WssMessage message)
	{
		WssMessages messages = new WssMessages(message);
		Error error = await EnsureConnection();
		if ((bool)error)
		{
			return error;
		}
		return await Socket.SendData(messages);
	}

	private static void Receive(WssMessages messages)
	{
		WssMessage[] messages2 = messages.messages;
		for (int i = 0; i < messages2.Length; i++)
		{
			WssMessage wssMessage = messages2[i];
			if (wssMessage.operation == "failed_operation")
			{
				ProcessErrorObject(wssMessage);
				continue;
			}
			if (WaitingForMessages.ContainsKey(wssMessage.operation))
			{
				TaskCompletionSource<WssMessage> taskCompletionSource = WaitingForMessages[wssMessage.operation];
				WaitingForMessages.Remove(wssMessage.operation);
				taskCompletionSource.SetResult(wssMessage);
				continue;
			}
			ModioLog.Verbose?.Log("[Socket] Received unexpected message operation (" + wssMessage.operation + ").\nCaching it temporarily in case we listen for it immediately after.");
			if (UnhandledMessages.ContainsKey(wssMessage.operation))
			{
				UnhandledMessages[wssMessage.operation] = wssMessage;
			}
			else
			{
				UnhandledMessages.Add(wssMessage.operation, wssMessage);
			}
		}
	}

	private static void ProcessErrorObject(WssMessage message)
	{
		if (message.TryGetValue<WssErrorObject>(out var output))
		{
			ModioLog.Error?.Log("[Socket] Error received from WssMessages:\n" + $"Error: [{output.error.code}]" + $" [{output.error.error_ref}]" + " " + output.error.message);
			if (WaitingForMessages.ContainsKey(output.operation))
			{
				TaskCompletionSource<WssMessage> taskCompletionSource = WaitingForMessages[output.operation];
				WaitingForMessages.Remove(output.operation);
				taskCompletionSource.SetResult(default(WssMessage));
			}
			else if (SubscribedMessageListeners.ContainsKey(output.operation))
			{
				SubscribedMessageListeners[output.operation]?.Invoke(message);
				WaitingForMessages.Remove(output.operation);
			}
			else
			{
				ModioLog.Warning?.Log("[Socket:Internal] Could not find any matching listener for the error operation: " + output.operation);
			}
		}
		else
		{
			ModioLog.Error?.Log("[Socket:Internal] Failed to cast WssMessage (operation \"" + message.operation + "\") into WssErrorObject");
		}
	}

	private static async void Disconnected()
	{
		if (WaitingForMessages.Count > 0)
		{
			ModioLog.Warning?.Log("[Socket] Disconnected while the WssHandler was still waiting for messages. Attempting to reconnect the WebSocket.");
			CancelAllAwaitingMessages();
			if ((bool)(await EnsureConnection()))
			{
				ModioLog.Error?.Log("[Socket] Failed to re-establish Socket connection. Listeners for WssMessages have been cancelled");
			}
			else
			{
				ModioLog.Warning?.Log("[Socket] Re-established Socket connection. Listeners for WssMessages have been cancelled");
			}
		}
	}
}
