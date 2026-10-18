using System;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class UserTranscriptionRequestEvent : UnityEvent<string, string>
{
}
