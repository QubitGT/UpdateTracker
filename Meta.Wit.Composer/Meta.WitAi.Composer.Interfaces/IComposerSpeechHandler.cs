namespace Meta.WitAi.Composer.Interfaces;

public interface IComposerSpeechHandler
{
	void SpeakPhrase(ComposerSessionData sessionData);

	bool IsSpeaking(ComposerSessionData sessionData);
}
