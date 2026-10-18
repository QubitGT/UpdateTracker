using System.Diagnostics;

namespace TMPro;

[DebuggerDisplay("{DebuggerDisplay()}")]
internal struct TextProcessingElement
{
	private TextProcessingElementType m_ElementType;

	private int m_StartIndex;

	private int m_Length;

	private CharacterElement m_CharacterElement;

	private MarkupElement m_MarkupElement;

	public TextProcessingElementType ElementType
	{
		get
		{
			return m_ElementType;
		}
		set
		{
			m_ElementType = value;
		}
	}

	public int StartIndex
	{
		get
		{
			return m_StartIndex;
		}
		set
		{
			m_StartIndex = value;
		}
	}

	public int Length
	{
		get
		{
			return m_Length;
		}
		set
		{
			m_Length = value;
		}
	}

	public CharacterElement CharacterElement => m_CharacterElement;

	public MarkupElement MarkupElement
	{
		get
		{
			return m_MarkupElement;
		}
		set
		{
			m_MarkupElement = value;
		}
	}

	public static TextProcessingElement Undefined
	{
		get
		{
			TextProcessingElement result = default(TextProcessingElement);
			result.ElementType = TextProcessingElementType.Undefined;
			return result;
		}
	}

	public TextProcessingElement(TextProcessingElementType elementType, int startIndex, int length)
	{
		m_ElementType = elementType;
		m_StartIndex = startIndex;
		m_Length = length;
		m_CharacterElement = default(CharacterElement);
		m_MarkupElement = default(MarkupElement);
	}

	public TextProcessingElement(TMP_TextElement textElement, int startIndex, int length)
	{
		m_ElementType = TextProcessingElementType.TextCharacterElement;
		m_StartIndex = startIndex;
		m_Length = length;
		m_CharacterElement = new CharacterElement(textElement);
		m_MarkupElement = default(MarkupElement);
	}

	public TextProcessingElement(CharacterElement characterElement, int startIndex, int length)
	{
		m_ElementType = TextProcessingElementType.TextCharacterElement;
		m_StartIndex = startIndex;
		m_Length = length;
		m_CharacterElement = characterElement;
		m_MarkupElement = default(MarkupElement);
	}

	public TextProcessingElement(MarkupElement markupElement)
	{
		m_ElementType = TextProcessingElementType.TextMarkupElement;
		m_StartIndex = markupElement.ValueStartIndex;
		m_Length = markupElement.ValueLength;
		m_CharacterElement = default(CharacterElement);
		m_MarkupElement = markupElement;
	}

	private string DebuggerDisplay()
	{
		if (m_ElementType != TextProcessingElementType.TextCharacterElement)
		{
			return $"Markup = {(MarkupTag)m_MarkupElement.NameHashCode}";
		}
		return $"Unicode ({m_CharacterElement.Unicode})   '{(char)m_CharacterElement.Unicode}' ";
	}
}
