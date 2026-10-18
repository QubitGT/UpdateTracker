namespace Meta.WitAi.Composer.Interfaces;

public interface IComposerActionHandler
{
	void PerformAction(ComposerSessionData sessionData);

	bool IsPerformingAction(ComposerSessionData sessionData);
}
