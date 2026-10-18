using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public interface IHandGrabInteractor : IHandGrabState
{
	IHand Hand { get; }

	Transform WristPoint { get; }

	Transform PinchPoint { get; }

	Transform PalmPoint { get; }

	HandGrabAPI HandGrabApi { get; }

	GrabTypeFlags SupportedGrabTypes { get; }

	IHandGrabInteractable TargetInteractable { get; }
}
