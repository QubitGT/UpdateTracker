using System;
using Meta.WitAi.Dictation.Data;
using UnityEngine.Events;

namespace Meta.WitAi.Dictation.Events;

[Serializable]
public class DictationSessionEvent : UnityEvent<DictationSession>
{
}
