using System;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class TranscriptionRequestEvent : UnityEvent<string>
{
}
