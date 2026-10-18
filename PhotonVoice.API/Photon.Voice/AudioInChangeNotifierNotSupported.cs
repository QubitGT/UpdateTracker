using System;

namespace Photon.Voice;

public class AudioInChangeNotifierNotSupported : IAudioInChangeNotifier, IDisposable
{
	public bool IsSupported => false;

	public string Error => "Current platform is not supported by AudioInChangeNotifier.";

	public AudioInChangeNotifierNotSupported(Action callback, ILogger logger)
	{
	}

	public void Dispose()
	{
	}
}
