using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public sealed class VolumeProfile : ScriptableObject
{
	[Flags]
	internal enum DirtyState
	{
		None = 0,
		DirtyByComponentChange = 1,
		DirtyByProfileReset = 2,
		Other = 4
	}

	public List<VolumeComponent> components = new List<VolumeComponent>();

	internal DirtyState dirtyState;

	[Obsolete("This field was only public for editor access. #from(6000.0)")]
	public bool isDirty
	{
		get
		{
			return dirtyState != DirtyState.None;
		}
		set
		{
			if (value)
			{
				dirtyState |= DirtyState.Other;
			}
			else
			{
				dirtyState &= ~DirtyState.Other;
			}
		}
	}

	private void OnEnable()
	{
		components.RemoveAll((VolumeComponent x) => x == null);
	}

	internal void OnDisable()
	{
		if (components == null)
		{
			return;
		}
		for (int i = 0; i < components.Count; i++)
		{
			if (components[i] != null)
			{
				components[i].Release();
			}
		}
	}

	public void Reset()
	{
		dirtyState |= DirtyState.DirtyByProfileReset;
	}

	public T Add<T>(bool overrides = false) where T : VolumeComponent
	{
		return (T)Add(typeof(T), overrides);
	}

	public VolumeComponent Add(Type type, bool overrides = false)
	{
		if (Has(type))
		{
			throw new InvalidOperationException("Component already exists in the volume");
		}
		VolumeComponent volumeComponent = (VolumeComponent)ScriptableObject.CreateInstance(type);
		volumeComponent.SetAllOverridesTo(overrides);
		components.Add(volumeComponent);
		dirtyState |= DirtyState.DirtyByComponentChange;
		return volumeComponent;
	}

	public void Remove<T>() where T : VolumeComponent
	{
		Remove(typeof(T));
	}

	public void Remove(Type type)
	{
		int num = -1;
		for (int i = 0; i < components.Count; i++)
		{
			if (components[i].GetType() == type)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			components.RemoveAt(num);
			dirtyState |= DirtyState.DirtyByComponentChange;
		}
	}

	public bool Has<T>() where T : VolumeComponent
	{
		return Has(typeof(T));
	}

	public bool Has(Type type)
	{
		foreach (VolumeComponent component in components)
		{
			if (component.GetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSubclassOf(Type type)
	{
		foreach (VolumeComponent component in components)
		{
			if (component.GetType().IsSubclassOf(type))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGet<T>(out T component) where T : VolumeComponent
	{
		return TryGet<T>(typeof(T), out component);
	}

	public bool TryGet<T>(Type type, out T component) where T : VolumeComponent
	{
		component = null;
		foreach (VolumeComponent component2 in components)
		{
			if (component2.GetType() == type)
			{
				component = (T)component2;
				return true;
			}
		}
		return false;
	}

	public bool TryGetSubclassOf<T>(Type type, out T component) where T : VolumeComponent
	{
		component = null;
		foreach (VolumeComponent component2 in components)
		{
			if (component2.GetType().IsSubclassOf(type))
			{
				component = (T)component2;
				return true;
			}
		}
		return false;
	}

	public bool TryGetAllSubclassOf<T>(Type type, List<T> result) where T : VolumeComponent
	{
		int count = result.Count;
		foreach (VolumeComponent component in components)
		{
			if (component.GetType().IsSubclassOf(type))
			{
				result.Add((T)component);
			}
		}
		return count != result.Count;
	}

	public override int GetHashCode()
	{
		int num = 17;
		for (int i = 0; i < components.Count; i++)
		{
			num = num * 23 + components[i].GetHashCode();
		}
		return num;
	}

	internal int GetComponentListHashCode()
	{
		int num = 17;
		for (int i = 0; i < components.Count; i++)
		{
			num = num * 23 + components[i].GetType().GetHashCode();
		}
		return num;
	}

	internal void Sanitize()
	{
		for (int num = components.Count - 1; num >= 0; num--)
		{
			if (components[num] == null)
			{
				components.RemoveAt(num);
			}
		}
	}
}
