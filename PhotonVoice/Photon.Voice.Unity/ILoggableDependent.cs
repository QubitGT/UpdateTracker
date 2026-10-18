namespace Photon.Voice.Unity;

public interface ILoggableDependent : ILoggable
{
	bool IgnoreGlobalLogLevel { get; set; }
}
