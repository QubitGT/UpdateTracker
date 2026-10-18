using System;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Interfaces;

namespace Meta.WitAi.Composer;

[Serializable]
public class ComposerSessionData
{
	public IComposerSession session;

	public ComposerService composer;

	public ComposerResponseData responseData;

	public string sessionID => session.SessionId;

	public ComposerContextMap contextMap => session.ContextMap;
}
