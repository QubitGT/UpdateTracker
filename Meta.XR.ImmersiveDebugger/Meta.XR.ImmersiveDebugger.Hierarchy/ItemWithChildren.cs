using System.Collections.Generic;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal abstract class ItemWithChildren<TargetType, ChildType, ChildTargetType> : Item<TargetType> where ChildType : Item<ChildTargetType>, new()
{
	private readonly List<ChildType> _children = new List<ChildType>();

	protected abstract bool CompareChildren(ChildTargetType lhs, ChildTargetType rhs);

	protected abstract ChildTargetType[] FetchExpectedChildren();

	public override int ComputeNumberOfChildren()
	{
		return FetchExpectedChildren().Length;
	}

	private void MarkChildrenDirty()
	{
		foreach (ChildType child in _children)
		{
			child.Dirty = true;
		}
	}

	private void ClearDirtyChildren()
	{
		foreach (ChildType child in _children)
		{
			if (child.Dirty)
			{
				child.Clear();
			}
		}
	}

	private ChildType GetChild(ChildTargetType target)
	{
		foreach (ChildType child in _children)
		{
			if (CompareChildren(child.TypedOwner, target))
			{
				return child;
			}
		}
		return null;
	}

	public override void ClearChildren()
	{
		foreach (ChildType child in _children)
		{
			child.Clear();
		}
		_children.Clear();
		base.ClearChildren();
	}

	public override void BuildChildren()
	{
		if (!Valid)
		{
			Clear();
			return;
		}
		MarkChildrenDirty();
		BuildChildrenInternal();
		ClearDirtyChildren();
	}

	private void BuildChildrenInternal()
	{
		ChildTargetType[] array = FetchExpectedChildren();
		foreach (ChildTargetType val in array)
		{
			ChildType child = GetChild(val);
			if (child != null)
			{
				child.Dirty = false;
				continue;
			}
			child = new ChildType();
			child.Dirty = false;
			child.SetOwner(val);
			_children.Add(child);
			child.Register(this);
		}
	}

	public override bool ComputeNeedsRefresh()
	{
		ChildTargetType[] array = FetchExpectedChildren();
		foreach (ChildTargetType target in array)
		{
			if (GetChild(target) == null)
			{
				return true;
			}
		}
		return false;
	}
}
