using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Unity.Input;

public class InputKey : MonoBehaviour, IButton
{
	[SerializeField]
	private KeyCode _key;

	public bool Value()
	{
		return UnityEngine.Input.GetKey(_key);
	}
}
