using UnityEngine;

namespace Oculus.Interaction.Input;

public class JointsRadiusFeature : MonoBehaviour
{
	[SerializeField]
	private Hand _hand;

	public float GetJointRadius(HandJointId id)
	{
		return _hand.GetData().JointRadii[(int)id];
	}
}
