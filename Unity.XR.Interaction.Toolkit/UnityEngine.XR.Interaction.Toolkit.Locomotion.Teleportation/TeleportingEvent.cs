using System;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public sealed class TeleportingEvent : UnityEvent<TeleportingEventArgs>
{
}
