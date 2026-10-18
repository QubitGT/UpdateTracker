using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal class ComponentItem : Item<Component>
{
	public override string Label => base.Handle.Type.Name;

	public override bool Valid => _owner != null;

	public override Category Category
	{
		get
		{
			Category result = default(Category);
			result.Item = base.Parent;
			return result;
		}
	}

	protected override InstanceHandle BuildHandle()
	{
		return new InstanceHandle(_owner.GetType(), _owner);
	}
}
