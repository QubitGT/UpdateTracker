using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Liv.Lck.Collections;
using Liv.NGFX;

namespace Liv.Lck.Encoding;

internal interface ILckEncoder : IDisposable
{
	bool IsActive();

	bool IsPaused();

	LckResult AcquireEncoder(EncoderConsumer consumer, CameraTrackDescriptor descriptor, IEnumerable<LckEncodedPacketHandler> handlers);

	Task<LckResult> ReleaseEncoderAsync(EncoderConsumer consumer, IEnumerable<LckEncodedPacketHandler> handlers);

	bool EncodeFrame(float videoTimeSeconds, AudioBuffer audioData, bool encodeVideo);

	void SetLogLevel(Liv.NGFX.LogLevel logLevel);

	EncoderSessionData GetCurrentSessionData();
}
