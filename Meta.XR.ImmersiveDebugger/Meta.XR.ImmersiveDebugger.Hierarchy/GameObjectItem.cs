using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal class GameObjectItem : ItemWithChildren<GameObject, GameObjectItem, GameObject>
{
	private readonly List<ComponentItem> _components = new List<ComponentItem>();

	public override string Label => _owner.name;

	public override bool Valid => _owner != null;

	protected override InstanceHandle BuildHandle()
	{
		return new InstanceHandle(typeof(GameObject), _owner);
	}

	protected override bool CompareChildren(GameObject lhs, GameObject rhs)
	{
		return lhs == rhs;
	}

	protected override GameObject[] FetchExpectedChildren()
	{
		Transform transform = _owner.transform;
		int childCount = transform.childCount;
		GameObject[] array = new GameObject[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = transform.GetChild(i).gameObject;
		}
		return array;
	}

	public override void BuildContent()
	{
		if (!Valid)
		{
			Clear();
		}
		else
		{
			BuildContentInternal();
		}
	}

	private void BuildContentInternal()
	{
		Component[] components = _owner.GetComponents<Component>();
		foreach (Component owner in components)
		{
			ComponentItem componentItem = new ComponentItem();
			componentItem.SetOwner(owner);
			_components.Add(componentItem);
			componentItem.Register(this);
		}
	}

	public override void ClearContent()
	{
		foreach (ComponentItem component in _components)
		{
			component.Clear();
		}
		_components.Clear();
		base.ClearContent();
	}
}
