using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class OVRInputDeviceActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private List<OVRInput.Controller> _controllerTypes;

	public bool Active
	{
		get
		{
			foreach (OVRInput.Controller controllerType in _controllerTypes)
			{
				if (OVRInput.GetConnectedControllers() == controllerType)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void InjectAllOVRInputDeviceActiveState(List<OVRInput.Controller> controllerTypes)
	{
		InjectControllerTypes(controllerTypes);
	}

	public void InjectControllerTypes(List<OVRInput.Controller> controllerTypes)
	{
		_controllerTypes = controllerTypes;
	}
}
