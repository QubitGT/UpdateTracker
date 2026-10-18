using System;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal class SceneItem : ItemWithChildren<Scene, GameObjectItem, GameObject>
{
	public override string Label
	{
		get
		{
			if (!string.IsNullOrEmpty(_owner.name))
			{
				return _owner.name;
			}
			return "Untitled";
		}
	}

	public override bool Valid => _owner.isLoaded;

	protected override bool CompareChildren(GameObject lhs, GameObject rhs)
	{
		return lhs == rhs;
	}

	protected override InstanceHandle BuildHandle()
	{
		return new InstanceHandle(_owner);
	}

	protected override GameObject[] FetchExpectedChildren()
	{
		if (!_owner.isLoaded)
		{
			return Array.Empty<GameObject>();
		}
		return _owner.GetRootGameObjects();
	}
}
