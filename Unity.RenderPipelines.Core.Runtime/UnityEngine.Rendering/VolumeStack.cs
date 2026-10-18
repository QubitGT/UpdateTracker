using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public sealed class VolumeStack : IDisposable
{
	internal readonly Dictionary<Type, VolumeComponent> components = new Dictionary<Type, VolumeComponent>();

	internal VolumeParameter[] parameters;

	internal bool requiresReset = true;

	internal bool requiresResetForAllProperties = true;

	public bool isValid { get; private set; }

	internal VolumeStack()
	{
	}

	internal void Clear()
	{
		foreach (KeyValuePair<Type, VolumeComponent> component in components)
		{
			CoreUtils.Destroy(component.Value);
		}
		components.Clear();
		parameters = null;
	}

	internal void Reload(Type[] componentTypes)
	{
		Clear();
		requiresReset = true;
		requiresResetForAllProperties = true;
		List<VolumeParameter> list = new List<VolumeParameter>();
		foreach (Type type in componentTypes)
		{
			VolumeComponent volumeComponent = (VolumeComponent)ScriptableObject.CreateInstance(type);
			components.Add(type, volumeComponent);
			list.AddRange(volumeComponent.parameters);
		}
		parameters = list.ToArray();
		isValid = true;
	}

	public T GetComponent<T>() where T : VolumeComponent
	{
		return (T)GetComponent(typeof(T));
	}

	public VolumeComponent GetComponent(Type type)
	{
		components.TryGetValue(type, out var value);
		return value;
	}

	public void Dispose()
	{
		Clear();
		isValid = false;
	}
}
