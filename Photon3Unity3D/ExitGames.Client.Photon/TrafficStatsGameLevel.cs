using System;
using System.Diagnostics;

namespace ExitGames.Client.Photon;

public class TrafficStatsGameLevel
{
	private Stopwatch watch;

	private int timeOfLastDispatchCall;

	private int timeOfLastSendCall;

	public int OperationByteCount { get; set; }

	public int OperationCount { get; set; }

	public int ResultByteCount { get; set; }

	public int ResultCount { get; set; }

	public int EventByteCount { get; set; }

	public int EventCount { get; set; }

	public int LongestOpResponseCallback { get; set; }

	public byte LongestOpResponseCallbackOpCode { get; set; }

	public int LongestEventCallback { get; set; }

	public int LongestMessageCallback { get; set; }

	public int LongestRawMessageCallback { get; set; }

	public byte LongestEventCallbackCode { get; set; }

	public int LongestDeltaBetweenDispatching { get; set; }

	public int LongestDeltaBetweenSending { get; set; }

	[Obsolete("Use DispatchIncomingCommandsCalls, which has proper naming.")]
	public int DispatchCalls => DispatchIncomingCommandsCalls;

	public int DispatchIncomingCommandsCalls { get; set; }

	public int SendOutgoingCommandsCalls { get; set; }

	public int TotalByteCount => OperationByteCount + ResultByteCount + EventByteCount;

	public int TotalMessageCount => OperationCount + ResultCount + EventCount;

	public int TotalIncomingByteCount => ResultByteCount + EventByteCount;

	public int TotalIncomingMessageCount => ResultCount + EventCount;

	public int TotalOutgoingByteCount => OperationByteCount;

	public int TotalOutgoingMessageCount => OperationCount;

	internal TrafficStatsGameLevel(Stopwatch sw)
	{
		watch = sw;
	}

	internal void CountOperation(int operationBytes)
	{
		OperationByteCount += operationBytes;
		OperationCount++;
	}

	internal void CountResult(int resultBytes)
	{
		ResultByteCount += resultBytes;
		ResultCount++;
	}

	internal void CountEvent(int eventBytes)
	{
		EventByteCount += eventBytes;
		EventCount++;
	}

	internal void TimeForResponseCallback(byte code, int time)
	{
		if (time > LongestOpResponseCallback)
		{
			LongestOpResponseCallback = time;
			LongestOpResponseCallbackOpCode = code;
		}
	}

	internal void TimeForEventCallback(byte code, int time)
	{
		if (time > LongestEventCallback)
		{
			LongestEventCallback = time;
			LongestEventCallbackCode = code;
		}
	}

	internal void TimeForMessageCallback(int time)
	{
		if (time > LongestMessageCallback)
		{
			LongestMessageCallback = time;
		}
	}

	internal void TimeForRawMessageCallback(int time)
	{
		if (time > LongestRawMessageCallback)
		{
			LongestRawMessageCallback = time;
		}
	}

	internal void DispatchIncomingCommandsCalled()
	{
		if (timeOfLastDispatchCall != 0)
		{
			int num = (int)watch.ElapsedMilliseconds - timeOfLastDispatchCall;
			if (num > LongestDeltaBetweenDispatching)
			{
				LongestDeltaBetweenDispatching = num;
			}
		}
		DispatchIncomingCommandsCalls++;
		timeOfLastDispatchCall = (int)watch.ElapsedMilliseconds;
	}

	internal void SendOutgoingCommandsCalled()
	{
		if (timeOfLastSendCall != 0)
		{
			int num = (int)watch.ElapsedMilliseconds - timeOfLastSendCall;
			if (num > LongestDeltaBetweenSending)
			{
				LongestDeltaBetweenSending = num;
			}
		}
		SendOutgoingCommandsCalls++;
		timeOfLastSendCall = (int)watch.ElapsedMilliseconds;
	}

	public void ResetMaximumCounters()
	{
		LongestDeltaBetweenDispatching = 0;
		LongestDeltaBetweenSending = 0;
		LongestEventCallback = 0;
		LongestEventCallbackCode = 0;
		LongestOpResponseCallback = 0;
		LongestOpResponseCallbackOpCode = 0;
		timeOfLastDispatchCall = 0;
		timeOfLastSendCall = 0;
	}

	public override string ToString()
	{
		return $"OperationByteCount: {OperationByteCount} ResultByteCount: {ResultByteCount} EventByteCount: {EventByteCount}";
	}

	public string ToStringVitalStats()
	{
		return string.Format("Longest delta between Send: {0}ms Dispatch: {1}ms. Longest callback OnEv: {3}={2}ms OnResp: {5}={4}ms. Calls of Send: {6} Dispatch: {7}.", LongestDeltaBetweenSending, LongestDeltaBetweenDispatching, LongestEventCallback, LongestEventCallbackCode, LongestOpResponseCallback, LongestOpResponseCallbackOpCode, SendOutgoingCommandsCalls, DispatchIncomingCommandsCalls);
	}
}
