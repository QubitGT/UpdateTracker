namespace Oculus.Voice.Core.Bindings.Interfaces;

public interface IVoiceSDKLogger
{
	bool IsUsingPlatformIntegration { get; set; }

	bool ShouldLogToConsole { get; set; }

	string WitApplication { get; set; }

	void LogInteractionStart(string requestId, string witApi);

	void LogInteractionEndSuccess();

	void LogInteractionEndFailure(string errorMessage);

	void LogInteractionPoint(string interactionPoint);

	void LogAnnotation(string annotationKey, string annotationValue);

	void LogFirstTranscriptionTime();
}
