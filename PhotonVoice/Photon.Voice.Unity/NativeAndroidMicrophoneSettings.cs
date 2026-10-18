using System;

namespace Photon.Voice.Unity;

[Serializable]
public struct NativeAndroidMicrophoneSettings
{
	public bool AcousticEchoCancellation;

	public bool AutomaticGainControl;

	public bool NoiseSuppression;
}
