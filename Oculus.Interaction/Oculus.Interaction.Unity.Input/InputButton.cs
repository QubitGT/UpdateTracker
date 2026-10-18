using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Unity.Input;

public class InputButton : MonoBehaviour, IButton
{
	[SerializeField]
	private string _buttonName;

	public bool Value()
	{
		return UnityEngine.Input.GetButton(_buttonName);
	}
}
