using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction;

[Feature(Feature.Interaction)]
public class OVRControllerMatchesProfileActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private OVRInput.Controller _controller;

	[SerializeField]
	private OVRInput.InteractionProfile _profile;

	public bool Active => OVRInput.GetCurrentInteractionProfile((!_controller.HasFlag(OVRInput.Controller.LTouch) && !_controller.HasFlag(OVRInput.Controller.LHand)) ? OVRInput.Hand.HandRight : OVRInput.Hand.HandLeft) == _profile;

	public void InjectAllOVRControllerSupportsPressure(OVRInput.Controller controller)
	{
		_controller = controller;
	}
}
