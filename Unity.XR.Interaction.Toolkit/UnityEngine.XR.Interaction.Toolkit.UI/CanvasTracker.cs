namespace UnityEngine.XR.Interaction.Toolkit.UI;

[AddComponentMenu("")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.CanvasTracker.html")]
public class CanvasTracker : MonoBehaviour
{
	public bool transformDirty { get; set; }

	private void OnEnable()
	{
		transformDirty = true;
	}

	private void OnTransformParentChanged()
	{
		transformDirty = true;
	}
}
