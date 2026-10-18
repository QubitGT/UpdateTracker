using System;
using Liv.NGFX;
using UnityEngine.Scripting;

namespace Liv.Lck.Streaming;

internal class NullLckStreamer : ILckStreamer, ILckCaptureStateProvider, IDisposable
{
	public LckCaptureState CurrentCaptureState => LckCaptureState.Idle;

	public bool IsStreaming => false;

	public LckResult<bool> IsPaused()
	{
		return LckResult<bool>.NewError(LckError.StreamerNotImplemented, "LCK: No valid streamer module has been loaded. Ensure tv.liv.lck-streaming is installed.");
	}

	[Preserve]
	public NullLckStreamer()
	{
	}

	public void Dispose()
	{
	}

	public LckResult StartStreaming()
	{
		return LckResult.NewError(LckError.StreamerNotImplemented, "LCK: No valid streamer module has been loaded. Ensure tv.liv.lck-streaming is installed.");
	}

	public LckResult StopStreaming(LckService.StopReason stopReason)
	{
		return LckResult.NewError(LckError.StreamerNotImplemented, "LCK: No valid streamer module has been loaded. Ensure tv.liv.lck-streaming is installed.");
	}

	public LckResult<TimeSpan> GetStreamDuration()
	{
		return LckResult<TimeSpan>.NewError(LckError.StreamerNotImplemented, "LCK: No valid streamer module has been loaded. Ensure tv.liv.lck-streaming is installed.");
	}

	public void SetLogLevel(Liv.NGFX.LogLevel logLevel)
	{
	}
}
