using System;
using Meta.WitAi.Composer.Data;

namespace Meta.WitAi.Composer.Interfaces;

public interface IComposerSession
{
	string SessionId { get; }

	ComposerContextMap ContextMap { get; }

	bool HasStarted { get; }

	DateTime SessionStart { get; }

	void StartSession();

	void EndSession();
}
