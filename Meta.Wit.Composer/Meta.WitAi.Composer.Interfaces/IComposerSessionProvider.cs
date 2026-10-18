using Meta.WitAi.Requests;

namespace Meta.WitAi.Composer.Interfaces;

public interface IComposerSessionProvider
{
	string GetComposerSessionId(ComposerService service, VoiceServiceRequest request);
}
