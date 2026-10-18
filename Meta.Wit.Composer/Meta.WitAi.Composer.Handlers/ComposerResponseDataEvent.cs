using System;
using Meta.WitAi.Composer.Data;
using UnityEngine.Events;

namespace Meta.WitAi.Composer.Handlers;

[Serializable]
public class ComposerResponseDataEvent : UnityEvent<ComposerResponseData>
{
}
