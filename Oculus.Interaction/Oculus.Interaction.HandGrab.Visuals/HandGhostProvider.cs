using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab.Visuals;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Authoring/Hand Ghost Provider")]
public class HandGhostProvider : ScriptableObject
{
	[SerializeField]
	private HandGhost _leftHand;

	[SerializeField]
	private HandGhost _rightHand;

	public HandGhost GetHand(Handedness handedness)
	{
		if (handedness != 0)
		{
			return _rightHand;
		}
		return _leftHand;
	}
}
