using System;
using Liv.NGFX;

namespace Liv.Lck.Streaming;

internal interface ILckStreamer : ILckCaptureStateProvider, IDisposable
{
	bool IsStreaming { get; }

	LckResult StartStreaming();

	LckResult StopStreaming(LckService.StopReason stopReason);

	LckResult<TimeSpan> GetStreamDuration();

	void SetLogLevel(Liv.NGFX.LogLevel logLevel);
}
