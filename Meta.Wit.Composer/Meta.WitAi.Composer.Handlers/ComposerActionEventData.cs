using System;

namespace Meta.WitAi.Composer.Handlers;

[Serializable]
public struct ComposerActionEventData
{
	public string actionID;

	public ComposerActionEvent actionEvent;
}
