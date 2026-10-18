using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
public class Variable<T> : IVariableValueChanged, IVariable
{
	[Serializable]
	[CompilerGenerated]
	public class UxmlSerializedData : UnityEngine.UIElements.UxmlSerializedData
	{
		[UxmlAttribute("value")]
		[SerializeField]
		private T ValueUXML;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags ValueUXML_UxmlAttributeFlags;

		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[1]
			{
				new UxmlAttributeNames("ValueUXML", "value", null)
			});
		}

		public override object CreateInstance()
		{
			return new Variable<T>();
		}

		public override void Deserialize(object obj)
		{
			Variable<T> variable = (Variable<T>)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(ValueUXML_UxmlAttributeFlags))
			{
				variable.ValueUXML = ValueUXML;
			}
		}
	}

	[SerializeField]
	private T m_Value;

	[UxmlAttribute("value")]
	public T ValueUXML
	{
		get
		{
			return Value;
		}
		set
		{
			Value = value;
		}
	}

	public T Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (m_Value == null || !m_Value.Equals(value))
			{
				m_Value = value;
				SendValueChangedEvent();
			}
		}
	}

	public event Action<IVariable> ValueChanged;

	public object GetSourceValue(ISelectorInfo _)
	{
		return Value;
	}

	private void SendValueChangedEvent()
	{
		this.ValueChanged?.Invoke(this);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
