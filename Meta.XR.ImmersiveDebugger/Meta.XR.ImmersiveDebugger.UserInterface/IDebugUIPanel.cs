using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal interface IDebugUIPanel
{
	IInspector RegisterInspector(InstanceHandle instance, Category category);

	void UnregisterInspector(InstanceHandle instance, Category category, bool allCategories);

	IInspector GetInspector(InstanceHandle instance, Category category);
}
