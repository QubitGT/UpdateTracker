using UnityEngine;

namespace Modio.Unity.UI.Input;

public class ModioUIActionSender : MonoBehaviour
{
	[SerializeField]
	private ModioUIInput.ModioAction _action;

	public void PressedAction()
	{
		ModioUIInput.PressedAction(_action);
	}
}
