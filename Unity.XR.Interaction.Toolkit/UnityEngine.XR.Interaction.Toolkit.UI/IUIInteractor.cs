namespace UnityEngine.XR.Interaction.Toolkit.UI;

public interface IUIInteractor
{
	void UpdateUIModel(ref TrackedDeviceModel model);

	bool TryGetUIModel(out TrackedDeviceModel model);
}
