using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger;

internal class DebugInspectorManager : DebugManagerAddon<DebugInspectorManager>
{
	private readonly List<DebugInspector> _inspectors = new List<DebugInspector>();

	protected override Telemetry.Method Method => Telemetry.Method.DebugInspector;

	public void RegisterInspector(DebugInspector inspector)
	{
		_inspectors.Add(inspector);
		ProcessInspector(inspector);
	}

	public void UnregisterInspector(DebugInspector inspector)
	{
		UnprocessInspector(inspector);
		_inspectors.Remove(inspector);
	}

	protected override void OnReadyInternal()
	{
		foreach (DebugInspector inspector in _inspectors)
		{
			ProcessInspector(inspector);
		}
	}

	private void ProcessInspector(DebugInspector inspector)
	{
		if (DebugManagerAddon<DebugInspectorManager>._uiPanel == null)
		{
			return;
		}
		foreach (InspectedHandle handle in inspector.Registry.Handles)
		{
			if (!handle.Visible)
			{
				continue;
			}
			InstanceHandle instanceHandle = handle.InstanceHandle;
			_instanceCache.RegisterHandle(instanceHandle);
			foreach (InspectedMember inspectedMember in handle.inspectedMembers)
			{
				if (!inspectedMember.Visible)
				{
					continue;
				}
				MemberInfo memberInfo = inspectedMember.MemberInfo;
				if (memberInfo == null)
				{
					continue;
				}
				DebugMember attribute = inspectedMember.attribute;
				if (attribute == null)
				{
					continue;
				}
				UpdateCategory(attribute, inspector);
				DebugManagerAddon<DebugInspectorManager>._uiPanel.RegisterInspector(instanceHandle, FetchCategory(attribute));
				foreach (IDebugManager subDebugManager in _subDebugManagers)
				{
					subDebugManager.ProcessTypeFromInspector(instanceHandle.Type, instanceHandle, memberInfo, attribute);
				}
			}
		}
	}

	private void UnprocessInspector(DebugInspector inspector)
	{
		if (DebugManagerAddon<DebugInspectorManager>._uiPanel == null)
		{
			return;
		}
		foreach (InspectedHandle handle in inspector.Registry.Handles)
		{
			InstanceHandle instanceHandle = handle.InstanceHandle;
			foreach (InspectedMember inspectedMember in handle.inspectedMembers)
			{
				DebugMember attribute = inspectedMember.attribute;
				if (attribute != null)
				{
					DebugManagerAddon<DebugInspectorManager>._uiPanel.UnregisterInspector(instanceHandle, FetchCategory(attribute), allCategories: false);
				}
			}
			_instanceCache.UnregisterHandle(instanceHandle);
		}
	}

	private void UpdateCategory(DebugMember attribute, DebugInspector inspector)
	{
		if (string.IsNullOrEmpty(attribute.Category))
		{
			attribute.Category = inspector.Category;
		}
	}

	private static Category FetchCategory(DebugMember attribute)
	{
		Category result = default(Category);
		result.Id = attribute.Category;
		return result;
	}
}
