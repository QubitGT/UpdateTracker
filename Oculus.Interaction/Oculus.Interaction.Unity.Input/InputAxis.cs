using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Unity.Input;

public class InputAxis : MonoBehaviour, IAxis1D
{
	[SerializeField]
	private string _axisName;

	public float Value()
	{
		return UnityEngine.Input.GetAxis(_axisName);
	}
}
