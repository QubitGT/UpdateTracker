using System;
using Meta.WitAi.Requests;
using UnityEngine.Events;

namespace Meta.WitAi.Composer;

[Serializable]
public class ComposerSessionRequestEvent : UnityEvent<ComposerSessionData, VoiceServiceRequest>
{
}
