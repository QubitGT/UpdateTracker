using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums;

public abstract class Datum<T> : ScriptableObject
{
	[Multiline]
	[SerializeField]
	private string m_Comments;

	[SerializeField]
	private bool m_ReadOnly = true;

	[SerializeField]
	private T m_Value;

	private readonly BindableVariableAlloc<T> m_BindableVariableReference = new BindableVariableAlloc<T>();

	public string Comments
	{
		get
		{
			return m_Comments;
		}
		set
		{
			m_Comments = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_ReadOnly;
		}
		set
		{
			m_ReadOnly = value;
		}
	}

	public IReadOnlyBindableVariable<T> BindableVariableReference => m_BindableVariableReference;

	public T Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (m_ReadOnly)
			{
				Debug.LogWarning($"{this} ValueDatum is set to read-only, variable can't be changed!", this);
				return;
			}
			m_Value = value;
			m_BindableVariableReference.Value = value;
		}
	}

	protected void OnEnable()
	{
		m_BindableVariableReference.Value = Value;
	}
}
