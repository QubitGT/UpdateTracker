namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("XR/XR UI Toolkit Manager", 11)]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.XRUIToolkitManager.html")]
public class XRUIToolkitManager : MonoBehaviour
{
	protected void OnEnable()
	{
		XRUIToolkitHandler.uiToolkitSupportEnabled = true;
	}

	protected void OnDisable()
	{
		XRUIToolkitHandler.uiToolkitSupportEnabled = false;
	}
}
