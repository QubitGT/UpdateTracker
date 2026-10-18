namespace UnityEngine.XR.Interaction.Toolkit.UI;

public interface IUIHoverInteractor : IUIInteractor
{
	UIHoverEnterEvent uiHoverEntered { get; }

	UIHoverExitEvent uiHoverExited { get; }

	void OnUIHoverEntered(UIHoverEventArgs args);

	void OnUIHoverExited(UIHoverEventArgs args);
}
