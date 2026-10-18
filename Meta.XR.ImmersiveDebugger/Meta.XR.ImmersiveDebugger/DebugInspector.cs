using System;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.Manager;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

[ExecuteInEditMode]
public class DebugInspector : MonoBehaviour
{
	[Serializable]
	internal class InspectionRegistry
	{
		[SerializeField]
		private List<InspectedHandle> handles = new List<InspectedHandle>();

		internal List<InspectedHandle> Handles => handles;

		internal void Initialize(DebugInspector owner)
		{
			foreach (InspectedHandle handle in handles)
			{
				handle.Initialize(owner);
			}
			Component[] components = owner.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (!(component == null))
				{
					Type type = component.GetType();
					if (!(type == typeof(DebugInspector)) && !TryGetHandle(component, out var inspectedHandle))
					{
						inspectedHandle = new InspectedHandle(owner, type);
						handles.Add(inspectedHandle);
					}
				}
			}
		}

		private bool TryGetHandle(Component component, out InspectedHandle inspectedHandle)
		{
			inspectedHandle = null;
			foreach (InspectedHandle handle in handles)
			{
				if (handle.InstanceHandle.Instance == component)
				{
					inspectedHandle = handle;
					break;
				}
			}
			return inspectedHandle != null;
		}
	}

	[Tooltip("Defines a default category for all inspected data handled by this component. These can still be overriden by specifying another category individually in the inspected data properties.")]
	[SerializeField]
	private string _category;

	[SerializeField]
	private InspectionRegistry registry = new InspectionRegistry();

	internal string Category => _category;

	internal InspectionRegistry Registry => registry;

	private void OnValidate()
	{
		Initialize();
	}

	internal void Initialize()
	{
		registry.Initialize(this);
	}

	private void OnEnable()
	{
		Initialize();
		if (Application.isPlaying)
		{
			DebugManagerAddon<DebugInspectorManager>.Instance.RegisterInspector(this);
		}
	}

	private void OnDisable()
	{
		DebugManagerAddon<DebugInspectorManager>.Instance.UnregisterInspector(this);
	}
}
