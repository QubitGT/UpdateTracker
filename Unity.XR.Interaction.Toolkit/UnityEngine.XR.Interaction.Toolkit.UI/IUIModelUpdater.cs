namespace UnityEngine.XR.Interaction.Toolkit.UI;

public interface IUIModelUpdater
{
	bool UpdateUIModel(ref TrackedDeviceModel uiModel, bool isSelectActive, in Vector2 scrollDelta);
}
