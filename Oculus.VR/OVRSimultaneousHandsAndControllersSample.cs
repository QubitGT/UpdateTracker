using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OVRSimultaneousHandsAndControllersSample : MonoBehaviour
{
	[SerializeField]
	private Button enableButton;

	[SerializeField]
	private Button disableButton;

	[SerializeField]
	public Text displayText;

	private void Update()
	{
		displayText.text = OVRInput.GetActiveController().ToString();
	}

	public void EnableSimultaneousHandsAndControllers()
	{
		OVRInput.EnableSimultaneousHandsAndControllers();
		enableButton.interactable = false;
		disableButton.interactable = true;
	}

	public void DisableSimultaneousHandsAndControllers()
	{
		OVRInput.DisableSimultaneousHandsAndControllers();
		enableButton.interactable = true;
		disableButton.interactable = false;
	}
}
