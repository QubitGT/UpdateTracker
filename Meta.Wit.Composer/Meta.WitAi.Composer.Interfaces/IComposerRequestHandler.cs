using Meta.WitAi.Requests;

namespace Meta.WitAi.Composer.Interfaces;

public interface IComposerRequestHandler
{
	void OnComposerRequestSetup(ComposerSessionData sessionData, VoiceServiceRequest request);
}
