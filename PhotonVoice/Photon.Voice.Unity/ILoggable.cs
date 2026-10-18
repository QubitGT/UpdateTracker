using ExitGames.Client.Photon;

namespace Photon.Voice.Unity;

public interface ILoggable
{
	DebugLevel LogLevel { get; set; }

	VoiceLogger Logger { get; }
}
