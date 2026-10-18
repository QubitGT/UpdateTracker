namespace TMPro;

internal struct MarkupAttribute
{
	private int m_NameHashCode;

	private int m_ValueHashCode;

	private int m_ValueStartIndex;

	private int m_ValueLength;

	public int NameHashCode
	{
		get
		{
			return m_NameHashCode;
		}
		set
		{
			m_NameHashCode = value;
		}
	}

	public int ValueHashCode
	{
		get
		{
			return m_ValueHashCode;
		}
		set
		{
			m_ValueHashCode = value;
		}
	}

	public int ValueStartIndex
	{
		get
		{
			return m_ValueStartIndex;
		}
		set
		{
			m_ValueStartIndex = value;
		}
	}

	public int ValueLength
	{
		get
		{
			return m_ValueLength;
		}
		set
		{
			m_ValueLength = value;
		}
	}
}
