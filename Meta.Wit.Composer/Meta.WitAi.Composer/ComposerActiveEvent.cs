using System;
using UnityEngine.Events;

namespace Meta.WitAi.Composer;

[Serializable]
public class ComposerActiveEvent : UnityEvent<ComposerService, bool>
{
}
