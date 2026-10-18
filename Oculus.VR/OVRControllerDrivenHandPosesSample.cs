using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[HelpURL("https://developer.oculus.com/documentation/unity/move-body-tracking/#appendix-b-isdk-integration")]
public class OVRControllerDrivenHandPosesSample : MonoBehaviour
{
	[SerializeField]
	private Button buttonOff;

	[SerializeField]
	private Button buttonConforming;

	[SerializeField]
	private Button buttonNatural;

	public OVRCameraRig cameraRig;

	private void Awake()
	{
		switch (OVRManager.instance.controllerDrivenHandPosesType)
		{
		case OVRManager.ControllerDrivenHandPosesType.None:
			SetControllerDrivenHandPosesTypeToNone();
			break;
		case OVRManager.ControllerDrivenHandPosesType.ConformingToController:
			SetControllerDrivenHandPosesTypeToControllerConforming();
			break;
		case OVRManager.ControllerDrivenHandPosesType.Natural:
			SetControllerDrivenHandPosesTypeToNatural();
			break;
		}
	}

	public void SetControllerDrivenHandPosesTypeToNone()
	{
		OVRManager.instance.controllerDrivenHandPosesType = OVRManager.ControllerDrivenHandPosesType.None;
		buttonOff.interactable = false;
		buttonConforming.interactable = true;
		buttonNatural.interactable = true;
	}

	public void SetControllerDrivenHandPosesTypeToControllerConforming()
	{
		OVRManager.instance.controllerDrivenHandPosesType = OVRManager.ControllerDrivenHandPosesType.ConformingToController;
		buttonOff.interactable = true;
		buttonConforming.interactable = false;
		buttonNatural.interactable = true;
	}

	public void SetControllerDrivenHandPosesTypeToNatural()
	{
		OVRManager.instance.controllerDrivenHandPosesType = OVRManager.ControllerDrivenHandPosesType.Natural;
		buttonOff.interactable = true;
		buttonConforming.interactable = true;
		buttonNatural.interactable = false;
	}
}
