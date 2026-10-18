using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Obsolete("IInputAxisProvider is deprecated.  Use InputAxis and InputAxisController instead")]
public static class CinemachineInputProviderExtensions
{
	public static AxisState.IInputAxisProvider GetInputAxisProvider(this CinemachineVirtualCameraBase vcam)
	{
		MonoBehaviour[] componentsInChildren = vcam.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] is AxisState.IInputAxisProvider result)
			{
				return result;
			}
		}
		return null;
	}
}
