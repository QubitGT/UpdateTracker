using System;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit;

[Serializable]
[Obsolete("XRInteractorEvent has been deprecated. Use events specific to each state change instead.", true)]
public class XRInteractorEvent : UnityEvent<XRBaseInteractable>
{
}
