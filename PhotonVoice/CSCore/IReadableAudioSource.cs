using System;

namespace CSCore;

public interface IReadableAudioSource<in T> : IAudioSource, IDisposable
{
	int Read(T[] buffer, int offset, int count);
}
