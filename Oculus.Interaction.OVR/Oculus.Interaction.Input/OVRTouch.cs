using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class OVRTouch : MonoBehaviour, IButton
{
	[SerializeField]
	private OVRInput.Controller _controller;

	[SerializeField]
	private OVRInput.Touch _touch;

	public bool Value()
	{
		return OVRInput.Get(_touch, _controller);
	}
}
