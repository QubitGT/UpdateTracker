using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Unity.Input;

public class InputMouseButton : MonoBehaviour, IButton
{
	[SerializeField]
	private int _button;

	public bool Value()
	{
		return UnityEngine.Input.GetMouseButton(_button);
	}
}
