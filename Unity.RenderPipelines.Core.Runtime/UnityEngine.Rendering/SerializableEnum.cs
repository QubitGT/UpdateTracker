using System;

namespace UnityEngine.Rendering;

[Serializable]
public class SerializableEnum
{
	[SerializeField]
	private string m_EnumValueAsString;

	[SerializeField]
	private string m_EnumTypeAsString;

	public Enum value
	{
		get
		{
			if (string.IsNullOrEmpty(m_EnumTypeAsString) || !Enum.TryParse(Type.GetType(m_EnumTypeAsString), m_EnumValueAsString, out var result))
			{
				return null;
			}
			return (Enum)result;
		}
		set
		{
			m_EnumValueAsString = value.ToString();
		}
	}

	public SerializableEnum(Type enumType)
	{
		m_EnumTypeAsString = enumType.AssemblyQualifiedName;
		m_EnumValueAsString = Enum.GetNames(enumType)[0];
	}
}
