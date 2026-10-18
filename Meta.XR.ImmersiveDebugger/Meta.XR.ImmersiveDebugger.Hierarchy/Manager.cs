using System.Reflection;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal class Manager : DebugManagerAddon<Manager>
{
	private readonly SceneRegistry _sceneRegistry = new SceneRegistry();

	protected override Telemetry.Method Method => Telemetry.Method.Hierarchy;

	public void ProcessItem(Item item)
	{
		InstanceHandle handle = item.Handle;
		_instanceCache.RegisterHandle(handle);
		DebugManagerAddon<Manager>._uiPanel?.RegisterInspector(handle, item.Category);
		if (!(item is ComponentItem componentItem))
		{
			return;
		}
		MemberInfo[] members = componentItem.TypedOwner.GetType().GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MemberInfo memberInfo in members)
		{
			if (!memberInfo.IsCompatibleWithDebugInspector() || (!memberInfo.IsPublic() && !RuntimeSettings.Instance.HierarchyViewShowsPrivateMembers))
			{
				continue;
			}
			foreach (IDebugManager subDebugManager in _subDebugManagers)
			{
				subDebugManager.ProcessTypeFromHierarchy(item, memberInfo);
			}
		}
	}

	public void UnprocessItem(Item item)
	{
		InstanceHandle handle = item.Handle;
		DebugManagerAddon<Manager>._uiPanel?.UnregisterInspector(handle, item.Category, allCategories: false);
		_instanceCache.UnregisterHandle(handle);
	}

	public void Refresh()
	{
		if (_sceneRegistry.ComputeNeedsRefresh())
		{
			_sceneRegistry.BuildChildren();
		}
	}
}
