using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Hierarchy;

internal abstract class Item
{
	private Item _parent;

	private int _depth;

	protected InstanceHandle _handle;

	public Item Parent => _parent;

	public int Depth => _depth;

	public InstanceHandle Handle => _handle;

	public int Id => Handle.InstanceId;

	public virtual Category Category
	{
		get
		{
			Category result = default(Category);
			result.Item = this;
			return result;
		}
	}

	public bool Dirty { get; set; }

	public abstract object Owner { get; }

	public abstract string Label { get; }

	public abstract bool Valid { get; }

	public void Clear()
	{
		ClearContent();
		ClearChildren();
		Unregister();
	}

	public virtual void Unregister()
	{
		DebugManagerAddon<Manager>.Instance.UnprocessItem(this);
		_parent = null;
	}

	public virtual void Register(Item parent)
	{
		_parent = parent;
		_depth = (_parent?.Depth ?? (-1)) + 1;
		DebugManagerAddon<Manager>.Instance.ProcessItem(this);
	}

	public virtual int ComputeNumberOfChildren()
	{
		return 0;
	}

	public virtual bool ComputeNeedsRefresh()
	{
		return false;
	}

	public virtual void BuildContent()
	{
	}

	public virtual void ClearContent()
	{
	}

	public virtual void BuildChildren()
	{
	}

	public virtual void ClearChildren()
	{
	}
}
internal abstract class Item<T> : Item
{
	protected T _owner;

	public override object Owner => _owner;

	public T TypedOwner => _owner;

	public void SetOwner(T owner)
	{
		_owner = owner;
		_handle = BuildHandle();
	}

	protected abstract InstanceHandle BuildHandle();
}
