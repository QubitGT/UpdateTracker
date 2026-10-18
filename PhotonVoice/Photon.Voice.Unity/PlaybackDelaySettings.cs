using System;

namespace Photon.Voice.Unity;

[Serializable]
public struct PlaybackDelaySettings
{
	public const int DEFAULT_LOW = 200;

	public const int DEFAULT_HIGH = 400;

	public const int DEFAULT_MAX = 1000;

	public int MinDelaySoft;

	public int MaxDelaySoft;

	public int MaxDelayHard;

	public override string ToString()
	{
		return $"[low={MinDelaySoft}ms,high={MaxDelaySoft}ms,max={MaxDelayHard}ms]";
	}
}
