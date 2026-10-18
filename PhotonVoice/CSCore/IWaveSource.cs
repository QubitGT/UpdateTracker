using System;

namespace CSCore;

public interface IWaveSource : IReadableAudioSource<byte>, IAudioSource, IDisposable
{
}
