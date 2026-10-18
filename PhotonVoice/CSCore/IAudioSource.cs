using System;

namespace CSCore;

public interface IAudioSource : IDisposable
{
	bool CanSeek { get; }

	WaveFormat WaveFormat { get; }

	long Position { get; set; }

	long Length { get; }
}
