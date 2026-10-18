using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums;

[Serializable]
public abstract class DatumProperty<TValue, TDatum> where TDatum : Datum<TValue>
{
	[SerializeField]
	private bool m_UseConstant;

	[SerializeField]
	private TValue m_ConstantValue;

	[SerializeField]
	private TDatum m_Variable;

	public TValue Value
	{
		get
		{
			if (!m_UseConstant)
			{
				if (!(Datum != null))
				{
					return default(TValue);
				}
				return Datum.Value;
			}
			return m_ConstantValue;
		}
		set
		{
			if (m_UseConstant)
			{
				m_ConstantValue = value;
			}
			else
			{
				Datum.Value = value;
			}
		}
	}

	protected Datum<TValue> Datum => m_Variable;

	protected TValue ConstantValue => m_ConstantValue;

	protected DatumProperty()
	{
		m_UseConstant = false;
	}

	protected DatumProperty(TValue value)
	{
		m_UseConstant = true;
		m_ConstantValue = value;
	}

	protected DatumProperty(TDatum datum)
	{
		m_UseConstant = false;
		m_Variable = datum;
	}

	public static implicit operator TValue(DatumProperty<TValue, TDatum> datumProperty)
	{
		return datumProperty.Value;
	}
}
