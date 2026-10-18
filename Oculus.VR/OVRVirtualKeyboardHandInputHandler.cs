using System;
using Meta.XR.Util;
using UnityEngine;

[Obsolete]
[ExecuteInEditMode]
[HelpURL("https://developer.oculus.com/documentation/unity/VK-unity-IntegratePrefab/")]
[Feature(Feature.VirtualKeyboard)]
public class OVRVirtualKeyboardHandInputHandler : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DestroyImmediate(this);
	}
}
