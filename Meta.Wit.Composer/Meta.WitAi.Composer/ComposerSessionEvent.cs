using System;
using UnityEngine.Events;

namespace Meta.WitAi.Composer;

[Serializable]
public class ComposerSessionEvent : UnityEvent<ComposerSessionData>
{
}
