using System;

namespace UnityEngine.VFX.Utility;

[Serializable]
public class ExposedProperty
{
	[SerializeField]
	private string m_Name;

	private int m_Id;

	public static implicit operator ExposedProperty(string name)
	{
		return new ExposedProperty(name);
	}

	public static explicit operator string(ExposedProperty parameter)
	{
		return parameter.m_Name;
	}

	public static implicit operator int(ExposedProperty parameter)
	{
		if (parameter.m_Id == 0 && !string.IsNullOrEmpty(parameter.m_Name))
		{
			throw new InvalidOperationException("Unexpected constructor has been called");
		}
		if (parameter.m_Id == -1)
		{
			parameter.m_Id = Shader.PropertyToID(parameter.m_Name);
		}
		return parameter.m_Id;
	}

	public static ExposedProperty operator +(ExposedProperty self, ExposedProperty other)
	{
		return new ExposedProperty(self.m_Name + other.m_Name);
	}

	public ExposedProperty()
	{
		m_Id = -1;
	}

	private ExposedProperty(string name)
	{
		m_Name = name;
		m_Id = -1;
	}

	public override string ToString()
	{
		return m_Name;
	}
}
