using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.OVR.Input;

[Feature(Feature.Interaction)]
public class OVRButtonActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private OVRInput.Button _button;

	public bool Active => OVRInput.Get(_button);
}
