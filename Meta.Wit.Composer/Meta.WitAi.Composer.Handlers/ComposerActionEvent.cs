using System;
using UnityEngine.Events;

namespace Meta.WitAi.Composer.Handlers;

[Serializable]
public class ComposerActionEvent : UnityEvent<ComposerSessionData>
{
}
