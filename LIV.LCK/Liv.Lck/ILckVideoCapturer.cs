using System;

namespace Liv.Lck;

internal interface ILckVideoCapturer : IDisposable
{
	bool ForceCaptureAllFrames { get; set; }

	bool IsCapturing { get; }

	void StartCapturing();

	void StopCapturing();

	bool HasCurrentFrameBeenCaptured();
}
