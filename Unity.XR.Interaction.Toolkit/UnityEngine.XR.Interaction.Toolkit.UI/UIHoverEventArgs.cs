namespace UnityEngine.XR.Interaction.Toolkit.UI;

public class UIHoverEventArgs
{
	public IUIInteractor interactorObject { get; set; }

	public TrackedDeviceModel deviceModel { get; set; }

	public GameObject uiObject { get; set; }
}
