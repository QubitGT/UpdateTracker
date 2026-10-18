using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class OVRNearTouch : MonoBehaviour, IButton
{
	[SerializeField]
	private OVRInput.Controller _controller;

	[SerializeField]
	private OVRInput.NearTouch _nearTouch;

	public bool Value()
	{
		return OVRInput.Get(_nearTouch, _controller);
	}
}
