namespace UnityEngine.VFX.Utility;

[ExecuteAlways]
[RequireComponent(typeof(VFXPropertyBinder))]
public abstract class VFXBinderBase : MonoBehaviour
{
	protected VFXPropertyBinder binder;

	public abstract bool IsValid(VisualEffect component);

	public virtual void Reset()
	{
	}

	protected virtual void Awake()
	{
		binder = GetComponent<VFXPropertyBinder>();
	}

	protected virtual void OnEnable()
	{
		if (!binder.m_Bindings.Contains(this))
		{
			binder.m_Bindings.Add(this);
		}
		base.hideFlags = HideFlags.HideInInspector;
	}

	protected virtual void OnDisable()
	{
		if (binder.m_Bindings.Contains(this))
		{
			binder.m_Bindings.Remove(this);
		}
	}

	public abstract void UpdateBinding(VisualEffect component);

	public override string ToString()
	{
		return GetType().ToString();
	}
}
