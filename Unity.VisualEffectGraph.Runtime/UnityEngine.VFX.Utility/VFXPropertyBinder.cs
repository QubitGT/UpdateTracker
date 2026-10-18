using System;
using System.Collections.Generic;

namespace UnityEngine.VFX.Utility;

[RequireComponent(typeof(VisualEffect))]
[DefaultExecutionOrder(1)]
[DisallowMultipleComponent]
[ExecuteAlways]
public class VFXPropertyBinder : MonoBehaviour
{
	[SerializeField]
	protected bool m_ExecuteInEditor = true;

	public List<VFXBinderBase> m_Bindings = new List<VFXBinderBase>();

	[SerializeField]
	protected VisualEffect m_VisualEffect;

	private void OnEnable()
	{
		Reload();
	}

	private void OnValidate()
	{
		Reload();
	}

	private static void SafeDestroy(Object toDelete)
	{
		Object.Destroy(toDelete);
	}

	private void Reload()
	{
		m_VisualEffect = GetComponent<VisualEffect>();
		m_Bindings = new List<VFXBinderBase>();
		m_Bindings.AddRange(base.gameObject.GetComponents<VFXBinderBase>());
	}

	private void Reset()
	{
		Reload();
		ClearPropertyBinders();
	}

	private void LateUpdate()
	{
		if (!m_ExecuteInEditor && Application.isEditor && !Application.isPlaying)
		{
			return;
		}
		for (int i = 0; i < m_Bindings.Count; i++)
		{
			VFXBinderBase vFXBinderBase = m_Bindings[i];
			if (vFXBinderBase == null)
			{
				Debug.LogWarning($"Parameter binder at index {i} of GameObject {base.gameObject.name} is null or missing");
			}
			else if (vFXBinderBase.IsValid(m_VisualEffect))
			{
				vFXBinderBase.UpdateBinding(m_VisualEffect);
			}
		}
	}

	public T AddPropertyBinder<T>() where T : VFXBinderBase
	{
		return base.gameObject.AddComponent<T>();
	}

	[Obsolete("Use AddPropertyBinder<T>() instead")]
	public T AddParameterBinder<T>() where T : VFXBinderBase
	{
		return AddPropertyBinder<T>();
	}

	public void ClearPropertyBinders()
	{
		VFXBinderBase[] components = GetComponents<VFXBinderBase>();
		for (int i = 0; i < components.Length; i++)
		{
			SafeDestroy(components[i]);
		}
	}

	[Obsolete("Please use ClearPropertyBinders() instead")]
	public void ClearParameterBinders()
	{
		ClearPropertyBinders();
	}

	public void RemovePropertyBinder(VFXBinderBase binder)
	{
		if (binder.gameObject == base.gameObject)
		{
			SafeDestroy(binder);
		}
	}

	[Obsolete("Please use RemovePropertyBinder() instead")]
	public void RemoveParameterBinder(VFXBinderBase binder)
	{
		RemovePropertyBinder(binder);
	}

	public void RemovePropertyBinders<T>() where T : VFXBinderBase
	{
		VFXBinderBase[] components = GetComponents<VFXBinderBase>();
		foreach (VFXBinderBase vFXBinderBase in components)
		{
			if (vFXBinderBase is T)
			{
				SafeDestroy(vFXBinderBase);
			}
		}
	}

	[Obsolete("Please use RemovePropertyBinders<T>() instead")]
	public void RemoveParameterBinders<T>() where T : VFXBinderBase
	{
		RemovePropertyBinders<T>();
	}

	public IEnumerable<T> GetPropertyBinders<T>() where T : VFXBinderBase
	{
		foreach (VFXBinderBase binding in m_Bindings)
		{
			if (binding is T)
			{
				yield return binding as T;
			}
		}
	}

	[Obsolete("Please use GetPropertyBinders<T>() instead")]
	public IEnumerable<T> GetParameterBinders<T>() where T : VFXBinderBase
	{
		return GetPropertyBinders<T>();
	}
}
