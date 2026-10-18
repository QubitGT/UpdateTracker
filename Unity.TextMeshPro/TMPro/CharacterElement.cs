namespace TMPro;

internal struct CharacterElement
{
	private uint m_Unicode;

	private TMP_TextElement m_TextElement;

	public uint Unicode
	{
		get
		{
			return m_Unicode;
		}
		set
		{
			m_Unicode = value;
		}
	}

	public CharacterElement(TMP_TextElement textElement)
	{
		m_Unicode = textElement.unicode;
		m_TextElement = textElement;
	}
}
