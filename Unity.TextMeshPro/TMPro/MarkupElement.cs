namespace TMPro;

internal struct MarkupElement
{
	private MarkupAttribute[] m_Attributes;

	public int NameHashCode
	{
		get
		{
			if (m_Attributes != null)
			{
				return m_Attributes[0].NameHashCode;
			}
			return 0;
		}
		set
		{
			if (m_Attributes == null)
			{
				m_Attributes = new MarkupAttribute[8];
			}
			m_Attributes[0].NameHashCode = value;
		}
	}

	public int ValueHashCode
	{
		get
		{
			if (m_Attributes != null)
			{
				return m_Attributes[0].ValueHashCode;
			}
			return 0;
		}
		set
		{
			m_Attributes[0].ValueHashCode = value;
		}
	}

	public int ValueStartIndex
	{
		get
		{
			if (m_Attributes != null)
			{
				return m_Attributes[0].ValueStartIndex;
			}
			return 0;
		}
		set
		{
			m_Attributes[0].ValueStartIndex = value;
		}
	}

	public int ValueLength
	{
		get
		{
			if (m_Attributes != null)
			{
				return m_Attributes[0].ValueLength;
			}
			return 0;
		}
		set
		{
			m_Attributes[0].ValueLength = value;
		}
	}

	public MarkupAttribute[] Attributes
	{
		get
		{
			return m_Attributes;
		}
		set
		{
			m_Attributes = value;
		}
	}

	public MarkupElement(int nameHashCode, int startIndex, int length)
	{
		m_Attributes = new MarkupAttribute[8];
		m_Attributes[0].NameHashCode = nameHashCode;
		m_Attributes[0].ValueStartIndex = startIndex;
		m_Attributes[0].ValueLength = length;
	}
}
