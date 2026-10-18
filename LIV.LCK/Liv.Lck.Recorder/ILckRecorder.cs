using System;
using Liv.NGFX;

namespace Liv.Lck.Recorder;

internal interface ILckRecorder : ILckCaptureStateProvider, IDisposable
{
	LckResult StartRecording();

	LckResult StopRecording(LckService.StopReason stopReason);

	LckResult PauseRecording();

	LckResult ResumeRecording();

	LckResult<TimeSpan> GetRecordingDuration();

	LckResult<bool> IsRecording();

	void SetLogLevel(Liv.NGFX.LogLevel logLevel);
}
