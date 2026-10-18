using Meta.WitAi.Composer.Integrations;

namespace Oculus.Voice.Composer;

public class AppComposerExperience : WitComposerService
{
	public AppVoiceExperience AppVoiceExperience => (AppVoiceExperience)base.VoiceService;
}
