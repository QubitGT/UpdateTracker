using Meta.WitAi.Composer.Interfaces;

namespace Meta.WitAi.Composer.Integrations;

public class WitComposerService : ComposerService
{
	private WitComposerRequestHandler _requestHandler;

	protected override IComposerRequestHandler GetRequestHandler()
	{
		return _requestHandler;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_requestHandler == null)
		{
			_requestHandler = new WitComposerRequestHandler(base.VoiceService.WitConfiguration);
		}
	}
}
