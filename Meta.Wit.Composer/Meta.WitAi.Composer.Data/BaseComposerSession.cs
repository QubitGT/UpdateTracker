using System;
using Meta.WitAi.Composer.Interfaces;

namespace Meta.WitAi.Composer.Data;

public class BaseComposerSession : IComposerSession
{
	public string SessionId { get; }

	public ComposerContextMap ContextMap { get; }

	public bool HasStarted { get; private set; }

	public DateTime SessionStart { get; private set; }

	public BaseComposerSession(string sessionId, ComposerContextMap contextMap)
	{
		HasStarted = false;
		SessionId = sessionId;
		ContextMap = contextMap;
		SessionStart = DateTime.UtcNow;
	}

	public void StartSession()
	{
		if (!HasStarted)
		{
			HasStarted = true;
			SessionStart = DateTime.UtcNow;
		}
	}

	public void EndSession()
	{
		if (HasStarted)
		{
			HasStarted = false;
		}
	}
}
