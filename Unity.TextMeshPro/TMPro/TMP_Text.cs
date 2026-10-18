using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TMPro;

public abstract class TMP_Text : MaskableGraphic
{
	public delegate void MissingCharacterEventCallback(int unicode, int stringIndex, string text, TMP_FontAsset fontAsset, TMP_Text textComponent);

	protected struct CharacterSubstitution
	{
		public int index;

		public uint unicode;

		public CharacterSubstitution(int index, uint unicode)
		{
			this.index = index;
			this.unicode = unicode;
		}
	}

	internal enum TextInputSources
	{
		TextInputBox,
		SetText,
		SetTextArray,
		TextString
	}

	[DebuggerDisplay("Unicode ({unicode})  '{(char)unicode}'")]
	internal struct TextProcessingElement
	{
		public TextProcessingElementType elementType;

		public uint unicode;

		public int stringIndex;

		public int length;
	}

	protected struct SpecialCharacter
	{
		public TMP_Character character;

		public TMP_FontAsset fontAsset;

		public Material material;

		public int materialIndex;

		public SpecialCharacter(TMP_Character character, int materialIndex)
		{
			this.character = character;
			fontAsset = character.textAsset as TMP_FontAsset;
			material = ((fontAsset != null) ? fontAsset.material : null);
			this.materialIndex = materialIndex;
		}
	}

	private struct TextBackingContainer
	{
		private uint[] m_Array;

		private int m_Index;

		public uint[] Text => m_Array;

		public int Capacity => m_Array.Length;

		public int Count
		{
			get
			{
				return m_Index;
			}
			set
			{
				m_Index = value;
			}
		}

		public uint this[int index]
		{
			get
			{
				return m_Array[index];
			}
			set
			{
				if (index >= m_Array.Length)
				{
					Resize(index);
				}
				m_Array[index] = value;
			}
		}

		public TextBackingContainer(int size)
		{
			m_Array = new uint[size];
			m_Index = 0;
		}

		public void Resize(int size)
		{
			size = Mathf.NextPowerOfTwo(size + 1);
			Array.Resize(ref m_Array, size);
		}
	}

	[SerializeField]
	[TextArea(5, 10)]
	protected string m_text;

	private bool m_IsTextBackingStringDirty;

	[SerializeField]
	protected ITextPreprocessor m_TextPreprocessor;

	[SerializeField]
	protected bool m_isRightToLeft;

	[SerializeField]
	protected TMP_FontAsset m_fontAsset;

	protected TMP_FontAsset m_currentFontAsset;

	protected bool m_isSDFShader;

	[SerializeField]
	protected Material m_sharedMaterial;

	protected Material m_currentMaterial;

	protected static MaterialReference[] m_materialReferences = new MaterialReference[4];

	protected static Dictionary<int, int> m_materialReferenceIndexLookup = new Dictionary<int, int>();

	protected static TMP_TextProcessingStack<MaterialReference> m_materialReferenceStack = new TMP_TextProcessingStack<MaterialReference>(new MaterialReference[16]);

	protected int m_currentMaterialIndex;

	[SerializeField]
	protected Material[] m_fontSharedMaterials;

	[SerializeField]
	protected Material m_fontMaterial;

	[SerializeField]
	protected Material[] m_fontMaterials;

	protected bool m_isMaterialDirty;

	[SerializeField]
	protected Color32 m_fontColor32 = Color.white;

	[SerializeField]
	protected Color m_fontColor = Color.white;

	protected static Color32 s_colorWhite = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	protected Color32 m_underlineColor = s_colorWhite;

	protected Color32 m_strikethroughColor = s_colorWhite;

	internal HighlightState m_HighlightState = new HighlightState(s_colorWhite, TMP_Offset.zero);

	internal bool m_ConvertToLinearSpace;

	[SerializeField]
	protected bool m_enableVertexGradient;

	[SerializeField]
	protected ColorMode m_colorMode = ColorMode.FourCornersGradient;

	[SerializeField]
	protected VertexGradient m_fontColorGradient = new VertexGradient(Color.white);

	[SerializeField]
	protected TMP_ColorGradient m_fontColorGradientPreset;

	[SerializeField]
	protected TMP_SpriteAsset m_spriteAsset;

	[SerializeField]
	protected bool m_tintAllSprites;

	protected bool m_tintSprite;

	protected Color32 m_spriteColor;

	[SerializeField]
	protected TMP_StyleSheet m_StyleSheet;

	internal TMP_Style m_TextStyle;

	[SerializeField]
	protected int m_TextStyleHashCode;

	[SerializeField]
	protected bool m_overrideHtmlColors;

	[SerializeField]
	protected Color32 m_faceColor = Color.white;

	protected Color32 m_outlineColor = Color.black;

	protected float m_outlineWidth;

	protected Vector3 m_currentEnvMapRotation;

	protected bool m_hasEnvMapProperty;

	[SerializeField]
	protected float m_fontSize = -99f;

	protected float m_currentFontSize;

	[SerializeField]
	protected float m_fontSizeBase = 36f;

	protected TMP_TextProcessingStack<float> m_sizeStack = new TMP_TextProcessingStack<float>(16);

	[SerializeField]
	protected FontWeight m_fontWeight = FontWeight.Regular;

	protected FontWeight m_FontWeightInternal = FontWeight.Regular;

	protected TMP_TextProcessingStack<FontWeight> m_FontWeightStack = new TMP_TextProcessingStack<FontWeight>(8);

	[SerializeField]
	protected bool m_enableAutoSizing;

	protected float m_maxFontSize;

	protected float m_minFontSize;

	protected int m_AutoSizeIterationCount;

	protected int m_AutoSizeMaxIterationCount = 100;

	protected bool m_IsAutoSizePointSizeSet;

	[SerializeField]
	protected float m_fontSizeMin;

	[SerializeField]
	protected float m_fontSizeMax;

	[SerializeField]
	protected FontStyles m_fontStyle;

	protected FontStyles m_FontStyleInternal;

	protected TMP_FontStyleStack m_fontStyleStack;

	protected bool m_isUsingBold;

	[SerializeField]
	protected HorizontalAlignmentOptions m_HorizontalAlignment = HorizontalAlignmentOptions.Left;

	[SerializeField]
	protected VerticalAlignmentOptions m_VerticalAlignment = VerticalAlignmentOptions.Top;

	[SerializeField]
	[FormerlySerializedAs("m_lineJustification")]
	protected TextAlignmentOptions m_textAlignment = TextAlignmentOptions.Converted;

	protected HorizontalAlignmentOptions m_lineJustification;

	protected TMP_TextProcessingStack<HorizontalAlignmentOptions> m_lineJustificationStack = new TMP_TextProcessingStack<HorizontalAlignmentOptions>(new HorizontalAlignmentOptions[16]);

	protected Vector3[] m_textContainerLocalCorners = new Vector3[4];

	[SerializeField]
	protected float m_characterSpacing;

	protected float m_cSpacing;

	protected float m_monoSpacing;

	protected bool m_duoSpace;

	[SerializeField]
	protected float m_wordSpacing;

	[SerializeField]
	protected float m_lineSpacing;

	protected float m_lineSpacingDelta;

	protected float m_lineHeight = -32767f;

	protected bool m_IsDrivenLineSpacing;

	[SerializeField]
	protected float m_lineSpacingMax;

	[SerializeField]
	protected float m_paragraphSpacing;

	[SerializeField]
	protected float m_charWidthMaxAdj;

	protected float m_charWidthAdjDelta;

	[SerializeField]
	[FormerlySerializedAs("m_enableWordWrapping")]
	protected TextWrappingModes m_TextWrappingMode;

	protected bool m_isCharacterWrappingEnabled;

	protected bool m_isNonBreakingSpace;

	protected bool m_isIgnoringAlignment;

	[SerializeField]
	protected float m_wordWrappingRatios = 0.4f;

	[SerializeField]
	protected TextOverflowModes m_overflowMode;

	protected int m_firstOverflowCharacterIndex = -1;

	[SerializeField]
	protected TMP_Text m_linkedTextComponent;

	[SerializeField]
	internal TMP_Text parentLinkedComponent;

	protected bool m_isTextTruncated;

	[SerializeField]
	protected bool m_enableKerning;

	protected int m_LastBaseGlyphIndex;

	[SerializeField]
	protected List<OTL_FeatureTag> m_ActiveFontFeatures = new List<OTL_FeatureTag> { (OTL_FeatureTag)0u };

	[SerializeField]
	protected bool m_enableExtraPadding;

	[SerializeField]
	protected bool checkPaddingRequired;

	[SerializeField]
	protected bool m_isRichText = true;

	[SerializeField]
	private bool m_EmojiFallbackSupport = true;

	[SerializeField]
	protected bool m_parseCtrlCharacters = true;

	protected bool m_isOverlay;

	[SerializeField]
	protected bool m_isOrthographic;

	[SerializeField]
	protected bool m_isCullingEnabled;

	protected bool m_isMaskingEnabled;

	protected bool isMaskUpdateRequired;

	protected bool m_ignoreCulling = true;

	[SerializeField]
	protected TextureMappingOptions m_horizontalMapping;

	[SerializeField]
	protected TextureMappingOptions m_verticalMapping;

	[SerializeField]
	protected float m_uvLineOffset;

	protected TextRenderFlags m_renderMode = TextRenderFlags.Render;

	[SerializeField]
	protected VertexSortingOrder m_geometrySortingOrder;

	[SerializeField]
	protected bool m_IsTextObjectScaleStatic;

	[SerializeField]
	protected bool m_VertexBufferAutoSizeReduction;

	protected int m_firstVisibleCharacter;

	protected int m_maxVisibleCharacters = 99999;

	protected int m_maxVisibleWords = 99999;

	protected int m_maxVisibleLines = 99999;

	[SerializeField]
	protected bool m_useMaxVisibleDescender = true;

	[SerializeField]
	protected int m_pageToDisplay = 1;

	protected bool m_isNewPage;

	[SerializeField]
	protected Vector4 m_margin = new Vector4(0f, 0f, 0f, 0f);

	protected float m_marginLeft;

	protected float m_marginRight;

	protected float m_marginWidth;

	protected float m_marginHeight;

	protected float m_width = -1f;

	protected TMP_TextInfo m_textInfo;

	protected bool m_havePropertiesChanged;

	[SerializeField]
	protected bool m_isUsingLegacyAnimationComponent;

	protected Transform m_transform;

	protected RectTransform m_rectTransform;

	protected Vector2 m_PreviousRectTransformSize;

	protected Vector2 m_PreviousPivotPosition;

	protected bool m_autoSizeTextContainer;

	protected Mesh m_mesh;

	[SerializeField]
	protected bool m_isVolumetricText;

	protected TMP_SpriteAnimator m_spriteAnimator;

	protected float m_flexibleHeight = -1f;

	protected float m_flexibleWidth = -1f;

	protected float m_minWidth;

	protected float m_minHeight;

	protected float m_maxWidth;

	protected float m_maxHeight;

	protected LayoutElement m_LayoutElement;

	protected float m_preferredWidth;

	protected float m_RenderedWidth;

	protected bool m_isPreferredWidthDirty;

	protected float m_preferredHeight;

	protected float m_RenderedHeight;

	protected bool m_isPreferredHeightDirty;

	protected bool m_isCalculatingPreferredValues;

	protected int m_layoutPriority;

	protected bool m_isLayoutDirty;

	protected bool m_isAwake;

	internal bool m_isWaitingOnResourceLoad;

	internal TextInputSources m_inputSource;

	protected float m_fontScaleMultiplier;

	private static char[] m_htmlTag = new char[128];

	private static RichTextTagAttribute[] m_xmlAttribute = new RichTextTagAttribute[8];

	private static float[] m_attributeParameterValues = new float[16];

	protected float tag_LineIndent;

	protected float tag_Indent;

	protected TMP_TextProcessingStack<float> m_indentStack = new TMP_TextProcessingStack<float>(new float[16]);

	protected bool tag_NoParsing;

	protected bool m_isTextLayoutPhase;

	protected Quaternion m_FXRotation;

	protected Vector3 m_FXScale;

	internal TextProcessingElement[] m_TextProcessingArray = new TextProcessingElement[8];

	internal int m_InternalTextProcessingArraySize;

	private TMP_CharacterInfo[] m_internalCharacterInfo;

	protected int m_totalCharacterCount;

	internal static WordWrapState m_SavedWordWrapState = default(WordWrapState);

	internal static WordWrapState m_SavedLineState = default(WordWrapState);

	internal static WordWrapState m_SavedEllipsisState = default(WordWrapState);

	internal static WordWrapState m_SavedLastValidState = default(WordWrapState);

	internal static WordWrapState m_SavedSoftLineBreakState = default(WordWrapState);

	internal static TMP_TextProcessingStack<WordWrapState> m_EllipsisInsertionCandidateStack = new TMP_TextProcessingStack<WordWrapState>(8, 8);

	protected int m_characterCount;

	protected int m_firstCharacterOfLine;

	protected int m_firstVisibleCharacterOfLine;

	protected int m_lastCharacterOfLine;

	protected int m_lastVisibleCharacterOfLine;

	protected int m_lineNumber;

	protected int m_lineVisibleCharacterCount;

	protected int m_lineVisibleSpaceCount;

	protected int m_pageNumber;

	protected float m_PageAscender;

	protected float m_maxTextAscender;

	protected float m_maxCapHeight;

	protected float m_ElementAscender;

	protected float m_ElementDescender;

	protected float m_maxLineAscender;

	protected float m_maxLineDescender;

	protected float m_startOfLineAscender;

	protected float m_startOfLineDescender;

	protected float m_lineOffset;

	protected Extents m_meshExtents;

	protected Color32 m_htmlColor = new Color(255f, 255f, 255f, 128f);

	protected TMP_TextProcessingStack<Color32> m_colorStack = new TMP_TextProcessingStack<Color32>(new Color32[16]);

	protected TMP_TextProcessingStack<Color32> m_underlineColorStack = new TMP_TextProcessingStack<Color32>(new Color32[16]);

	protected TMP_TextProcessingStack<Color32> m_strikethroughColorStack = new TMP_TextProcessingStack<Color32>(new Color32[16]);

	protected TMP_TextProcessingStack<HighlightState> m_HighlightStateStack = new TMP_TextProcessingStack<HighlightState>(new HighlightState[16]);

	protected TMP_ColorGradient m_colorGradientPreset;

	protected TMP_TextProcessingStack<TMP_ColorGradient> m_colorGradientStack = new TMP_TextProcessingStack<TMP_ColorGradient>(new TMP_ColorGradient[16]);

	protected bool m_colorGradientPresetIsTinted;

	protected float m_tabSpacing;

	protected float m_spacing;

	protected TMP_TextProcessingStack<int>[] m_TextStyleStacks = new TMP_TextProcessingStack<int>[8];

	protected int m_TextStyleStackDepth;

	protected TMP_TextProcessingStack<int> m_ItalicAngleStack = new TMP_TextProcessingStack<int>(new int[16]);

	protected int m_ItalicAngle;

	protected TMP_TextProcessingStack<int> m_actionStack = new TMP_TextProcessingStack<int>(new int[16]);

	protected float m_padding;

	protected float m_baselineOffset;

	protected TMP_TextProcessingStack<float> m_baselineOffsetStack = new TMP_TextProcessingStack<float>(new float[16]);

	protected float m_xAdvance;

	protected TMP_TextElementType m_textElementType;

	protected TMP_TextElement m_cached_TextElement;

	protected SpecialCharacter m_Ellipsis;

	protected SpecialCharacter m_Underline;

	protected TMP_SpriteAsset m_defaultSpriteAsset;

	protected TMP_SpriteAsset m_currentSpriteAsset;

	protected int m_spriteCount;

	protected int m_spriteIndex;

	protected int m_spriteAnimationID;

	private static ProfilerMarker k_ParseTextMarker = new ProfilerMarker("TMP Parse Text");

	private static ProfilerMarker k_InsertNewLineMarker = new ProfilerMarker("TMP.InsertNewLine");

	protected bool m_ignoreActiveState;

	private TextBackingContainer m_TextBackingArray = new TextBackingContainer(4);

	private readonly decimal[] k_Power = new decimal[10] { 0.5m, 0.05m, 0.005m, 0.0005m, 0.00005m, 0.000005m, 0.0000005m, 0.00000005m, 0.000000005m, 0.0000000005m };

	protected static Vector2 k_LargePositiveVector2 = new Vector2(2.1474836E+09f, 2.1474836E+09f);

	protected static Vector2 k_LargeNegativeVector2 = new Vector2(-2.1474836E+09f, -2.1474836E+09f);

	protected static float k_LargePositiveFloat = 32767f;

	protected static float k_LargeNegativeFloat = -32767f;

	protected static int k_LargePositiveInt = int.MaxValue;

	protected static int k_LargeNegativeInt = -2147483647;

	public virtual string text
	{
		get
		{
			if (m_IsTextBackingStringDirty)
			{
				return InternalTextBackingArrayToString();
			}
			return m_text;
		}
		set
		{
			if (m_IsTextBackingStringDirty || m_text == null || value == null || m_text.Length != value.Length || !(m_text == value))
			{
				m_IsTextBackingStringDirty = false;
				m_text = value;
				m_inputSource = TextInputSources.TextString;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public ITextPreprocessor textPreprocessor
	{
		get
		{
			return m_TextPreprocessor;
		}
		set
		{
			m_TextPreprocessor = value;
		}
	}

	public bool isRightToLeftText
	{
		get
		{
			return m_isRightToLeft;
		}
		set
		{
			if (m_isRightToLeft != value)
			{
				m_isRightToLeft = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public TMP_FontAsset font
	{
		get
		{
			return m_fontAsset;
		}
		set
		{
			if (!(m_fontAsset == value))
			{
				m_fontAsset = value;
				LoadFontAsset();
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public virtual Material fontSharedMaterial
	{
		get
		{
			return m_sharedMaterial;
		}
		set
		{
			if (!(m_sharedMaterial == value))
			{
				SetSharedMaterial(value);
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
	}

	public virtual Material[] fontSharedMaterials
	{
		get
		{
			return GetSharedMaterials();
		}
		set
		{
			SetSharedMaterials(value);
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetMaterialDirty();
		}
	}

	public Material fontMaterial
	{
		get
		{
			return GetMaterial(m_sharedMaterial);
		}
		set
		{
			if (!(m_sharedMaterial != null) || m_sharedMaterial.GetInstanceID() != value.GetInstanceID())
			{
				m_sharedMaterial = value;
				m_padding = GetPaddingForMaterial();
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
	}

	public virtual Material[] fontMaterials
	{
		get
		{
			return GetMaterials(m_fontSharedMaterials);
		}
		set
		{
			SetSharedMaterials(value);
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetMaterialDirty();
		}
	}

	public override Color color
	{
		get
		{
			return m_fontColor;
		}
		set
		{
			if (!(m_fontColor == value))
			{
				m_havePropertiesChanged = true;
				m_fontColor = value;
				SetVerticesDirty();
			}
		}
	}

	public float alpha
	{
		get
		{
			return m_fontColor.a;
		}
		set
		{
			if (m_fontColor.a != value)
			{
				m_fontColor.a = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public bool enableVertexGradient
	{
		get
		{
			return m_enableVertexGradient;
		}
		set
		{
			if (m_enableVertexGradient != value)
			{
				m_havePropertiesChanged = true;
				m_enableVertexGradient = value;
				SetVerticesDirty();
			}
		}
	}

	public VertexGradient colorGradient
	{
		get
		{
			return m_fontColorGradient;
		}
		set
		{
			m_havePropertiesChanged = true;
			m_fontColorGradient = value;
			SetVerticesDirty();
		}
	}

	public TMP_ColorGradient colorGradientPreset
	{
		get
		{
			return m_fontColorGradientPreset;
		}
		set
		{
			m_havePropertiesChanged = true;
			m_fontColorGradientPreset = value;
			SetVerticesDirty();
		}
	}

	public TMP_SpriteAsset spriteAsset
	{
		get
		{
			return m_spriteAsset;
		}
		set
		{
			m_spriteAsset = value;
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}
	}

	public bool tintAllSprites
	{
		get
		{
			return m_tintAllSprites;
		}
		set
		{
			if (m_tintAllSprites != value)
			{
				m_tintAllSprites = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public TMP_StyleSheet styleSheet
	{
		get
		{
			return m_StyleSheet;
		}
		set
		{
			m_StyleSheet = value;
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}
	}

	public TMP_Style textStyle
	{
		get
		{
			m_TextStyle = GetStyle(m_TextStyleHashCode);
			if (m_TextStyle == null)
			{
				m_TextStyle = TMP_Style.NormalStyle;
				m_TextStyleHashCode = m_TextStyle.hashCode;
			}
			return m_TextStyle;
		}
		set
		{
			m_TextStyle = value;
			m_TextStyleHashCode = m_TextStyle.hashCode;
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}
	}

	public bool overrideColorTags
	{
		get
		{
			return m_overrideHtmlColors;
		}
		set
		{
			if (m_overrideHtmlColors != value)
			{
				m_havePropertiesChanged = true;
				m_overrideHtmlColors = value;
				SetVerticesDirty();
			}
		}
	}

	public Color32 faceColor
	{
		get
		{
			if (m_sharedMaterial == null)
			{
				return m_faceColor;
			}
			m_faceColor = m_sharedMaterial.GetColor(ShaderUtilities.ID_FaceColor);
			return m_faceColor;
		}
		set
		{
			if (!m_faceColor.Compare(value))
			{
				SetFaceColor(value);
				m_havePropertiesChanged = true;
				m_faceColor = value;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}
	}

	public Color32 outlineColor
	{
		get
		{
			if (m_sharedMaterial == null)
			{
				return m_outlineColor;
			}
			m_outlineColor = m_sharedMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
			return m_outlineColor;
		}
		set
		{
			if (!m_outlineColor.Compare(value))
			{
				SetOutlineColor(value);
				m_havePropertiesChanged = true;
				m_outlineColor = value;
				SetVerticesDirty();
			}
		}
	}

	public float outlineWidth
	{
		get
		{
			if (m_sharedMaterial == null)
			{
				return m_outlineWidth;
			}
			m_outlineWidth = m_sharedMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth);
			return m_outlineWidth;
		}
		set
		{
			if (m_outlineWidth != value)
			{
				SetOutlineThickness(value);
				m_havePropertiesChanged = true;
				m_outlineWidth = value;
				SetVerticesDirty();
			}
		}
	}

	public float fontSize
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			if (m_fontSize != value)
			{
				m_havePropertiesChanged = true;
				m_fontSize = value;
				if (!m_enableAutoSizing)
				{
					m_fontSizeBase = m_fontSize;
				}
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public FontWeight fontWeight
	{
		get
		{
			return m_fontWeight;
		}
		set
		{
			if (m_fontWeight != value)
			{
				m_fontWeight = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			Canvas canvas = base.canvas;
			if (!canvas)
			{
				return 1f;
			}
			if (!font)
			{
				return canvas.scaleFactor;
			}
			if (m_currentFontAsset == null || m_currentFontAsset.faceInfo.pointSize <= 0f || m_fontSize <= 0f)
			{
				return 1f;
			}
			return m_fontSize / m_currentFontAsset.faceInfo.pointSize;
		}
	}

	public bool enableAutoSizing
	{
		get
		{
			return m_enableAutoSizing;
		}
		set
		{
			if (m_enableAutoSizing != value)
			{
				m_enableAutoSizing = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float fontSizeMin
	{
		get
		{
			return m_fontSizeMin;
		}
		set
		{
			if (m_fontSizeMin != value)
			{
				m_fontSizeMin = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float fontSizeMax
	{
		get
		{
			return m_fontSizeMax;
		}
		set
		{
			if (m_fontSizeMax != value)
			{
				m_fontSizeMax = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public FontStyles fontStyle
	{
		get
		{
			return m_fontStyle;
		}
		set
		{
			if (m_fontStyle != value)
			{
				m_fontStyle = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool isUsingBold => m_isUsingBold;

	public HorizontalAlignmentOptions horizontalAlignment
	{
		get
		{
			return m_HorizontalAlignment;
		}
		set
		{
			if (m_HorizontalAlignment != value)
			{
				m_HorizontalAlignment = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public VerticalAlignmentOptions verticalAlignment
	{
		get
		{
			return m_VerticalAlignment;
		}
		set
		{
			if (m_VerticalAlignment != value)
			{
				m_VerticalAlignment = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public TextAlignmentOptions alignment
	{
		get
		{
			return (TextAlignmentOptions)((int)m_HorizontalAlignment | (int)m_VerticalAlignment);
		}
		set
		{
			HorizontalAlignmentOptions horizontalAlignmentOptions = (HorizontalAlignmentOptions)(value & (TextAlignmentOptions)255);
			VerticalAlignmentOptions verticalAlignmentOptions = (VerticalAlignmentOptions)(value & (TextAlignmentOptions)65280);
			if (m_HorizontalAlignment != horizontalAlignmentOptions || m_VerticalAlignment != verticalAlignmentOptions)
			{
				m_HorizontalAlignment = horizontalAlignmentOptions;
				m_VerticalAlignment = verticalAlignmentOptions;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public float characterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			if (m_characterSpacing != value)
			{
				m_havePropertiesChanged = true;
				m_characterSpacing = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float wordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			if (m_wordSpacing != value)
			{
				m_havePropertiesChanged = true;
				m_wordSpacing = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float lineSpacing
	{
		get
		{
			return m_lineSpacing;
		}
		set
		{
			if (m_lineSpacing != value)
			{
				m_havePropertiesChanged = true;
				m_lineSpacing = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float lineSpacingAdjustment
	{
		get
		{
			return m_lineSpacingMax;
		}
		set
		{
			if (m_lineSpacingMax != value)
			{
				m_havePropertiesChanged = true;
				m_lineSpacingMax = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float paragraphSpacing
	{
		get
		{
			return m_paragraphSpacing;
		}
		set
		{
			if (m_paragraphSpacing != value)
			{
				m_havePropertiesChanged = true;
				m_paragraphSpacing = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float characterWidthAdjustment
	{
		get
		{
			return m_charWidthMaxAdj;
		}
		set
		{
			if (m_charWidthMaxAdj != value)
			{
				m_havePropertiesChanged = true;
				m_charWidthMaxAdj = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public TextWrappingModes textWrappingMode
	{
		get
		{
			return m_TextWrappingMode;
		}
		set
		{
			if (m_TextWrappingMode != value)
			{
				m_havePropertiesChanged = true;
				m_TextWrappingMode = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	[Obsolete("The enabledWordWrapping property is now obsolete. Please use the textWrappingMode property instead.")]
	public bool enableWordWrapping
	{
		get
		{
			if (m_TextWrappingMode != TextWrappingModes.Normal)
			{
				return textWrappingMode == TextWrappingModes.PreserveWhitespace;
			}
			return true;
		}
		set
		{
			TextWrappingModes textWrappingModes = (value ? TextWrappingModes.Normal : TextWrappingModes.NoWrap);
			if (m_TextWrappingMode != textWrappingModes)
			{
				m_havePropertiesChanged = true;
				m_TextWrappingMode = textWrappingModes;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public float wordWrappingRatios
	{
		get
		{
			return m_wordWrappingRatios;
		}
		set
		{
			if (m_wordWrappingRatios != value)
			{
				m_wordWrappingRatios = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public TextOverflowModes overflowMode
	{
		get
		{
			return m_overflowMode;
		}
		set
		{
			if (m_overflowMode != value)
			{
				m_overflowMode = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool isTextOverflowing
	{
		get
		{
			if (m_firstOverflowCharacterIndex != -1)
			{
				return true;
			}
			return false;
		}
	}

	public int firstOverflowCharacterIndex => m_firstOverflowCharacterIndex;

	public TMP_Text linkedTextComponent
	{
		get
		{
			return m_linkedTextComponent;
		}
		set
		{
			if (value == null)
			{
				ReleaseLinkedTextComponent(m_linkedTextComponent);
				m_linkedTextComponent = value;
			}
			else
			{
				if (IsSelfOrLinkedAncestor(value))
				{
					return;
				}
				ReleaseLinkedTextComponent(m_linkedTextComponent);
				m_linkedTextComponent = value;
				m_linkedTextComponent.parentLinkedComponent = this;
			}
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}
	}

	public bool isTextTruncated => m_isTextTruncated;

	[Obsolete("The \"enableKerning\" property has been deprecated. Use the \"fontFeatures\" property to control what features are enabled on the text component.")]
	public bool enableKerning
	{
		get
		{
			return m_ActiveFontFeatures.Contains(OTL_FeatureTag.kern);
		}
		set
		{
			if (m_ActiveFontFeatures.Contains(OTL_FeatureTag.kern))
			{
				if (value)
				{
					return;
				}
				m_ActiveFontFeatures.Remove(OTL_FeatureTag.kern);
				m_enableKerning = false;
			}
			else
			{
				if (!value)
				{
					return;
				}
				m_ActiveFontFeatures.Add(OTL_FeatureTag.kern);
				m_enableKerning = true;
			}
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}
	}

	public List<OTL_FeatureTag> fontFeatures
	{
		get
		{
			return m_ActiveFontFeatures;
		}
		set
		{
			if (value != null)
			{
				m_havePropertiesChanged = true;
				m_ActiveFontFeatures = value;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool extraPadding
	{
		get
		{
			return m_enableExtraPadding;
		}
		set
		{
			if (m_enableExtraPadding != value)
			{
				m_havePropertiesChanged = true;
				m_enableExtraPadding = value;
				UpdateMeshPadding();
				SetVerticesDirty();
			}
		}
	}

	public bool richText
	{
		get
		{
			return m_isRichText;
		}
		set
		{
			if (m_isRichText != value)
			{
				m_isRichText = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool emojiFallbackSupport
	{
		get
		{
			return m_EmojiFallbackSupport;
		}
		set
		{
			if (m_EmojiFallbackSupport != value)
			{
				m_EmojiFallbackSupport = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool parseCtrlCharacters
	{
		get
		{
			return m_parseCtrlCharacters;
		}
		set
		{
			if (m_parseCtrlCharacters != value)
			{
				m_parseCtrlCharacters = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public bool isOverlay
	{
		get
		{
			return m_isOverlay;
		}
		set
		{
			if (m_isOverlay != value)
			{
				m_isOverlay = value;
				SetShaderDepth();
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public bool isOrthographic
	{
		get
		{
			return m_isOrthographic;
		}
		set
		{
			if (m_isOrthographic != value)
			{
				m_havePropertiesChanged = true;
				m_isOrthographic = value;
				SetVerticesDirty();
			}
		}
	}

	public bool enableCulling
	{
		get
		{
			return m_isCullingEnabled;
		}
		set
		{
			if (m_isCullingEnabled != value)
			{
				m_isCullingEnabled = value;
				SetCulling();
				m_havePropertiesChanged = true;
			}
		}
	}

	public bool ignoreVisibility
	{
		get
		{
			return m_ignoreCulling;
		}
		set
		{
			if (m_ignoreCulling != value)
			{
				m_havePropertiesChanged = true;
				m_ignoreCulling = value;
			}
		}
	}

	public TextureMappingOptions horizontalMapping
	{
		get
		{
			return m_horizontalMapping;
		}
		set
		{
			if (m_horizontalMapping != value)
			{
				m_havePropertiesChanged = true;
				m_horizontalMapping = value;
				SetVerticesDirty();
			}
		}
	}

	public TextureMappingOptions verticalMapping
	{
		get
		{
			return m_verticalMapping;
		}
		set
		{
			if (m_verticalMapping != value)
			{
				m_havePropertiesChanged = true;
				m_verticalMapping = value;
				SetVerticesDirty();
			}
		}
	}

	public float mappingUvLineOffset
	{
		get
		{
			return m_uvLineOffset;
		}
		set
		{
			if (m_uvLineOffset != value)
			{
				m_havePropertiesChanged = true;
				m_uvLineOffset = value;
				SetVerticesDirty();
			}
		}
	}

	public TextRenderFlags renderMode
	{
		get
		{
			return m_renderMode;
		}
		set
		{
			if (m_renderMode != value)
			{
				m_renderMode = value;
				m_havePropertiesChanged = true;
			}
		}
	}

	public VertexSortingOrder geometrySortingOrder
	{
		get
		{
			return m_geometrySortingOrder;
		}
		set
		{
			m_geometrySortingOrder = value;
			m_havePropertiesChanged = true;
			SetVerticesDirty();
		}
	}

	public bool isTextObjectScaleStatic
	{
		get
		{
			return m_IsTextObjectScaleStatic;
		}
		set
		{
			m_IsTextObjectScaleStatic = value;
			if (base.isActiveAndEnabled)
			{
				if (m_IsTextObjectScaleStatic)
				{
					TMP_UpdateManager.UnRegisterTextObjectForUpdate(this);
				}
				else
				{
					TMP_UpdateManager.RegisterTextObjectForUpdate(this);
				}
			}
		}
	}

	public bool vertexBufferAutoSizeReduction
	{
		get
		{
			return m_VertexBufferAutoSizeReduction;
		}
		set
		{
			m_VertexBufferAutoSizeReduction = value;
			m_havePropertiesChanged = true;
			SetVerticesDirty();
		}
	}

	public int firstVisibleCharacter
	{
		get
		{
			return m_firstVisibleCharacter;
		}
		set
		{
			if (m_firstVisibleCharacter != value)
			{
				m_havePropertiesChanged = true;
				m_firstVisibleCharacter = value;
				SetVerticesDirty();
			}
		}
	}

	public int maxVisibleCharacters
	{
		get
		{
			return m_maxVisibleCharacters;
		}
		set
		{
			if (m_maxVisibleCharacters != value)
			{
				m_havePropertiesChanged = true;
				m_maxVisibleCharacters = value;
				SetVerticesDirty();
			}
		}
	}

	public int maxVisibleWords
	{
		get
		{
			return m_maxVisibleWords;
		}
		set
		{
			if (m_maxVisibleWords != value)
			{
				m_havePropertiesChanged = true;
				m_maxVisibleWords = value;
				SetVerticesDirty();
			}
		}
	}

	public int maxVisibleLines
	{
		get
		{
			return m_maxVisibleLines;
		}
		set
		{
			if (m_maxVisibleLines != value)
			{
				m_havePropertiesChanged = true;
				m_maxVisibleLines = value;
				SetVerticesDirty();
			}
		}
	}

	public bool useMaxVisibleDescender
	{
		get
		{
			return m_useMaxVisibleDescender;
		}
		set
		{
			if (m_useMaxVisibleDescender != value)
			{
				m_havePropertiesChanged = true;
				m_useMaxVisibleDescender = value;
				SetVerticesDirty();
			}
		}
	}

	public int pageToDisplay
	{
		get
		{
			return m_pageToDisplay;
		}
		set
		{
			if (m_pageToDisplay != value)
			{
				m_havePropertiesChanged = true;
				m_pageToDisplay = value;
				SetVerticesDirty();
			}
		}
	}

	public virtual Vector4 margin
	{
		get
		{
			return m_margin;
		}
		set
		{
			if (!(m_margin == value))
			{
				m_margin = value;
				ComputeMarginSize();
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}
	}

	public TMP_TextInfo textInfo
	{
		get
		{
			if (m_textInfo == null)
			{
				m_textInfo = new TMP_TextInfo(this);
			}
			return m_textInfo;
		}
	}

	public bool havePropertiesChanged
	{
		get
		{
			return m_havePropertiesChanged;
		}
		set
		{
			if (m_havePropertiesChanged != value)
			{
				m_havePropertiesChanged = value;
				SetAllDirty();
			}
		}
	}

	public bool isUsingLegacyAnimationComponent
	{
		get
		{
			return m_isUsingLegacyAnimationComponent;
		}
		set
		{
			m_isUsingLegacyAnimationComponent = value;
		}
	}

	public new Transform transform
	{
		get
		{
			if (m_transform == null)
			{
				m_transform = GetComponent<Transform>();
			}
			return m_transform;
		}
	}

	public new RectTransform rectTransform
	{
		get
		{
			if (m_rectTransform == null)
			{
				m_rectTransform = GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public virtual bool autoSizeTextContainer { get; set; }

	public virtual Mesh mesh => m_mesh;

	public bool isVolumetricText
	{
		get
		{
			return m_isVolumetricText;
		}
		set
		{
			if (m_isVolumetricText != value)
			{
				m_havePropertiesChanged = value;
				m_textInfo.ResetVertexLayout(value);
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}
	}

	public Bounds bounds
	{
		get
		{
			if (m_mesh == null)
			{
				return default(Bounds);
			}
			return GetCompoundBounds();
		}
	}

	public Bounds textBounds
	{
		get
		{
			if (m_textInfo == null)
			{
				return default(Bounds);
			}
			return GetTextBounds();
		}
	}

	protected TMP_SpriteAnimator spriteAnimator
	{
		get
		{
			if (m_spriteAnimator == null)
			{
				m_spriteAnimator = GetComponent<TMP_SpriteAnimator>();
				if (m_spriteAnimator == null)
				{
					m_spriteAnimator = base.gameObject.AddComponent<TMP_SpriteAnimator>();
				}
			}
			return m_spriteAnimator;
		}
	}

	public float flexibleHeight => m_flexibleHeight;

	public float flexibleWidth => m_flexibleWidth;

	public float minWidth => m_minWidth;

	public float minHeight => m_minHeight;

	public float maxWidth => m_maxWidth;

	public float maxHeight => m_maxHeight;

	protected LayoutElement layoutElement
	{
		get
		{
			if (m_LayoutElement == null)
			{
				m_LayoutElement = GetComponent<LayoutElement>();
			}
			return m_LayoutElement;
		}
	}

	public virtual float preferredWidth
	{
		get
		{
			m_preferredWidth = GetPreferredWidth();
			return m_preferredWidth;
		}
	}

	public virtual float preferredHeight
	{
		get
		{
			m_preferredHeight = GetPreferredHeight();
			return m_preferredHeight;
		}
	}

	public virtual float renderedWidth => GetRenderedWidth();

	public virtual float renderedHeight => GetRenderedHeight();

	public int layoutPriority => m_layoutPriority;

	public static event Func<int, string, TMP_FontAsset> OnFontAssetRequest;

	public static event Func<int, string, TMP_SpriteAsset> OnSpriteAssetRequest;

	public static event MissingCharacterEventCallback OnMissingCharacter;

	public virtual event Action<TMP_TextInfo> OnPreRenderText = delegate
	{
	};

	protected virtual void LoadFontAsset()
	{
	}

	protected virtual void SetSharedMaterial(Material mat)
	{
	}

	protected virtual Material GetMaterial(Material mat)
	{
		return null;
	}

	protected virtual void SetFontBaseMaterial(Material mat)
	{
	}

	protected virtual Material[] GetSharedMaterials()
	{
		return null;
	}

	protected virtual void SetSharedMaterials(Material[] materials)
	{
	}

	protected virtual Material[] GetMaterials(Material[] mats)
	{
		return null;
	}

	protected virtual Material CreateMaterialInstance(Material source)
	{
		Material obj = new Material(source)
		{
			shaderKeywords = source.shaderKeywords
		};
		obj.name += " (Instance)";
		return obj;
	}

	protected void SetVertexColorGradient(TMP_ColorGradient gradient)
	{
		if (!(gradient == null))
		{
			m_fontColorGradient.bottomLeft = gradient.bottomLeft;
			m_fontColorGradient.bottomRight = gradient.bottomRight;
			m_fontColorGradient.topLeft = gradient.topLeft;
			m_fontColorGradient.topRight = gradient.topRight;
			SetVerticesDirty();
		}
	}

	protected void SetTextSortingOrder(VertexSortingOrder order)
	{
	}

	protected void SetTextSortingOrder(int[] order)
	{
	}

	protected virtual void SetFaceColor(Color32 color)
	{
	}

	protected virtual void SetOutlineColor(Color32 color)
	{
	}

	protected virtual void SetOutlineThickness(float thickness)
	{
	}

	protected virtual void SetShaderDepth()
	{
	}

	protected virtual void SetCulling()
	{
	}

	internal virtual void UpdateCulling()
	{
	}

	protected virtual float GetPaddingForMaterial()
	{
		ShaderUtilities.GetShaderPropertyIDs();
		if (m_sharedMaterial == null)
		{
			return 0f;
		}
		m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
		m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
		m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);
		return m_padding;
	}

	protected virtual float GetPaddingForMaterial(Material mat)
	{
		if (mat == null)
		{
			return 0f;
		}
		m_padding = ShaderUtilities.GetPadding(mat, m_enableExtraPadding, m_isUsingBold);
		m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
		m_isSDFShader = mat.HasProperty(ShaderUtilities.ID_WeightNormal);
		return m_padding;
	}

	protected virtual Vector3[] GetTextContainerLocalCorners()
	{
		return null;
	}

	public virtual void ForceMeshUpdate(bool ignoreActiveState = false, bool forceTextReparsing = false)
	{
	}

	public virtual void UpdateGeometry(Mesh mesh, int index)
	{
	}

	public virtual void UpdateVertexData(TMP_VertexDataUpdateFlags flags)
	{
	}

	public virtual void UpdateVertexData()
	{
	}

	public virtual void SetVertices(Vector3[] vertices)
	{
	}

	public virtual void UpdateMeshPadding()
	{
	}

	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
		base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
		InternalCrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
	}

	public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
	{
		base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
		InternalCrossFadeAlpha(alpha, duration, ignoreTimeScale);
	}

	protected virtual void InternalCrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
	}

	protected virtual void InternalCrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
	{
	}

	protected void ParseInputText()
	{
		switch (m_inputSource)
		{
		case TextInputSources.TextInputBox:
		case TextInputSources.TextString:
			PopulateTextBackingArray((m_TextPreprocessor == null) ? m_text : m_TextPreprocessor.PreprocessText(m_text));
			PopulateTextProcessingArray();
			break;
		}
		SetArraySizes(m_TextProcessingArray);
	}

	private void PopulateTextBackingArray(string sourceText)
	{
		int length = sourceText?.Length ?? 0;
		PopulateTextBackingArray(sourceText, 0, length);
	}

	private void PopulateTextBackingArray(string sourceText, int start, int length)
	{
		int num = 0;
		int i;
		if (sourceText == null)
		{
			i = 0;
			length = 0;
		}
		else
		{
			i = Mathf.Clamp(start, 0, sourceText.Length);
			length = Mathf.Clamp(length, 0, (start + length < sourceText.Length) ? length : (sourceText.Length - start));
		}
		if (length >= m_TextBackingArray.Capacity)
		{
			m_TextBackingArray.Resize(length);
		}
		for (int num2 = i + length; i < num2; i++)
		{
			m_TextBackingArray[num] = sourceText[i];
			num++;
		}
		m_TextBackingArray[num] = 0u;
		m_TextBackingArray.Count = num;
	}

	private void PopulateTextBackingArray(StringBuilder sourceText, int start, int length)
	{
		int num = 0;
		int i;
		if (sourceText == null)
		{
			i = 0;
			length = 0;
		}
		else
		{
			i = Mathf.Clamp(start, 0, sourceText.Length);
			length = Mathf.Clamp(length, 0, (start + length < sourceText.Length) ? length : (sourceText.Length - start));
		}
		if (length >= m_TextBackingArray.Capacity)
		{
			m_TextBackingArray.Resize(length);
		}
		for (int num2 = i + length; i < num2; i++)
		{
			m_TextBackingArray[num] = sourceText[i];
			num++;
		}
		m_TextBackingArray[num] = 0u;
		m_TextBackingArray.Count = num;
	}

	private void PopulateTextBackingArray(char[] sourceText, int start, int length)
	{
		int num = 0;
		int i;
		if (sourceText == null)
		{
			i = 0;
			length = 0;
		}
		else
		{
			i = Mathf.Clamp(start, 0, sourceText.Length);
			length = Mathf.Clamp(length, 0, (start + length < sourceText.Length) ? length : (sourceText.Length - start));
		}
		if (length >= m_TextBackingArray.Capacity)
		{
			m_TextBackingArray.Resize(length);
		}
		for (int num2 = i + length; i < num2; i++)
		{
			m_TextBackingArray[num] = sourceText[i];
			num++;
		}
		m_TextBackingArray[num] = 0u;
		m_TextBackingArray.Count = num;
	}

	private void PopulateTextProcessingArray()
	{
		TMP_TextProcessingStack<int>.SetDefault(m_TextStyleStacks, 0);
		int count = m_TextBackingArray.Count;
		int num = count + (textStyle.styleOpeningDefinition?.Length ?? 0);
		if (m_TextProcessingArray.Length < num)
		{
			ResizeInternalArray(ref m_TextProcessingArray, num);
		}
		m_TextStyleStackDepth = 0;
		int writeIndex = 0;
		if (textStyle.hashCode != -1183493901)
		{
			InsertOpeningStyleTag(m_TextStyle, ref m_TextProcessingArray, ref writeIndex);
		}
		tag_NoParsing = false;
		for (int i = 0; i < count; i++)
		{
			uint num2 = m_TextBackingArray[i];
			if (num2 == 0)
			{
				break;
			}
			if (num2 == 92 && i < count - 1)
			{
				switch (m_TextBackingArray[i + 1])
				{
				case 92u:
					if (m_parseCtrlCharacters)
					{
						i++;
					}
					break;
				case 110u:
					if (m_parseCtrlCharacters)
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 1,
							unicode = 10u
						};
						i++;
						writeIndex++;
						continue;
					}
					break;
				case 114u:
					if (m_parseCtrlCharacters)
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 1,
							unicode = 13u
						};
						i++;
						writeIndex++;
						continue;
					}
					break;
				case 116u:
					if (m_parseCtrlCharacters)
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 1,
							unicode = 9u
						};
						i++;
						writeIndex++;
						continue;
					}
					break;
				case 118u:
					if (m_parseCtrlCharacters)
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 1,
							unicode = 11u
						};
						i++;
						writeIndex++;
						continue;
					}
					break;
				case 117u:
					if (count > i + 5 && IsValidUTF16(m_TextBackingArray, i + 2))
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 6,
							unicode = GetUTF16(m_TextBackingArray, i + 2)
						};
						i += 5;
						writeIndex++;
						continue;
					}
					break;
				case 85u:
					if (count > i + 9 && IsValidUTF32(m_TextBackingArray, i + 2))
					{
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 10,
							unicode = GetUTF32(m_TextBackingArray, i + 2)
						};
						i += 9;
						writeIndex++;
						continue;
					}
					break;
				}
			}
			if (num2 >= 55296 && num2 <= 56319 && count > i + 1 && m_TextBackingArray[i + 1] >= 56320 && m_TextBackingArray[i + 1] <= 57343)
			{
				m_TextProcessingArray[writeIndex] = new TextProcessingElement
				{
					elementType = TextProcessingElementType.TextCharacterElement,
					stringIndex = i,
					length = 2,
					unicode = TMP_TextParsingUtilities.ConvertToUTF32(num2, m_TextBackingArray[i + 1])
				};
				i++;
				writeIndex++;
				continue;
			}
			if (num2 == 60 && m_isRichText)
			{
				switch ((MarkupTag)GetMarkupTagHashCode(m_TextBackingArray, i + 1))
				{
				case MarkupTag.NO_PARSE:
					tag_NoParsing = true;
					break;
				case MarkupTag.SLASH_NO_PARSE:
					tag_NoParsing = false;
					break;
				case MarkupTag.BR:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 4,
							unicode = 10u
						};
						writeIndex++;
						i += 3;
						continue;
					}
					break;
				case MarkupTag.CR:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 4,
							unicode = 13u
						};
						writeIndex++;
						i += 3;
						continue;
					}
					break;
				case MarkupTag.NBSP:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 6,
							unicode = 160u
						};
						writeIndex++;
						i += 5;
						continue;
					}
					break;
				case MarkupTag.ZWSP:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 6,
							unicode = 8203u
						};
						writeIndex++;
						i += 5;
						continue;
					}
					break;
				case MarkupTag.ZWJ:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 5,
							unicode = 8205u
						};
						writeIndex++;
						i += 4;
						continue;
					}
					break;
				case MarkupTag.SHY:
					if (!tag_NoParsing)
					{
						if (writeIndex == m_TextProcessingArray.Length)
						{
							ResizeInternalArray(ref m_TextProcessingArray);
						}
						m_TextProcessingArray[writeIndex] = new TextProcessingElement
						{
							elementType = TextProcessingElementType.TextCharacterElement,
							stringIndex = i,
							length = 5,
							unicode = 173u
						};
						writeIndex++;
						i += 4;
						continue;
					}
					break;
				case MarkupTag.A:
					if (m_TextBackingArray.Count > i + 4 && m_TextBackingArray[i + 3] == 104 && m_TextBackingArray[i + 4] == 114)
					{
						InsertOpeningTextStyle(GetStyle(65), ref m_TextProcessingArray, ref writeIndex);
					}
					break;
				case MarkupTag.STYLE:
				{
					if (tag_NoParsing)
					{
						break;
					}
					int k = writeIndex;
					if (ReplaceOpeningStyleTag(ref m_TextBackingArray, i, out var srcOffset, ref m_TextProcessingArray, ref writeIndex))
					{
						for (; k < writeIndex; k++)
						{
							m_TextProcessingArray[k].stringIndex = i;
							m_TextProcessingArray[k].length = srcOffset - i + 1;
						}
						i = srcOffset;
						continue;
					}
					break;
				}
				case MarkupTag.SLASH_A:
					InsertClosingTextStyle(GetStyle(65), ref m_TextProcessingArray, ref writeIndex);
					break;
				case MarkupTag.SLASH_STYLE:
					if (!tag_NoParsing)
					{
						int j = writeIndex;
						ReplaceClosingStyleTag(ref m_TextProcessingArray, ref writeIndex);
						for (; j < writeIndex; j++)
						{
							m_TextProcessingArray[j].stringIndex = i;
							m_TextProcessingArray[j].length = 8;
						}
						i += 7;
						continue;
					}
					break;
				}
			}
			if (writeIndex == m_TextProcessingArray.Length)
			{
				ResizeInternalArray(ref m_TextProcessingArray);
			}
			m_TextProcessingArray[writeIndex] = new TextProcessingElement
			{
				elementType = TextProcessingElementType.TextCharacterElement,
				stringIndex = i,
				length = 1,
				unicode = num2
			};
			writeIndex++;
		}
		m_TextStyleStackDepth = 0;
		if (textStyle.hashCode != -1183493901)
		{
			InsertClosingStyleTag(ref m_TextProcessingArray, ref writeIndex);
		}
		if (writeIndex == m_TextProcessingArray.Length)
		{
			ResizeInternalArray(ref m_TextProcessingArray);
		}
		m_TextProcessingArray[writeIndex].unicode = 0u;
		m_InternalTextProcessingArraySize = writeIndex;
	}

	private void SetTextInternal(string sourceText)
	{
		int length = sourceText?.Length ?? 0;
		PopulateTextBackingArray(sourceText, 0, length);
		TextInputSources inputSource = m_inputSource;
		m_inputSource = TextInputSources.TextString;
		PopulateTextProcessingArray();
		m_inputSource = inputSource;
	}

	public void SetText(string sourceText)
	{
		int length = sourceText?.Length ?? 0;
		PopulateTextBackingArray(sourceText, 0, length);
		m_text = sourceText;
		m_inputSource = TextInputSources.TextString;
		PopulateTextProcessingArray();
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	[Obsolete("Use the SetText(string) function instead.")]
	public void SetText(string sourceText, bool syncTextInputBox = true)
	{
		int length = sourceText?.Length ?? 0;
		PopulateTextBackingArray(sourceText, 0, length);
		m_text = sourceText;
		m_inputSource = TextInputSources.TextString;
		PopulateTextProcessingArray();
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	public void SetText(string sourceText, float arg0)
	{
		SetText(sourceText, arg0, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1)
	{
		SetText(sourceText, arg0, arg1, 0f, 0f, 0f, 0f, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2)
	{
		SetText(sourceText, arg0, arg1, arg2, 0f, 0f, 0f, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2, float arg3)
	{
		SetText(sourceText, arg0, arg1, arg2, arg3, 0f, 0f, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2, float arg3, float arg4)
	{
		SetText(sourceText, arg0, arg1, arg2, arg3, arg4, 0f, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2, float arg3, float arg4, float arg5)
	{
		SetText(sourceText, arg0, arg1, arg2, arg3, arg4, arg5, 0f, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2, float arg3, float arg4, float arg5, float arg6)
	{
		SetText(sourceText, arg0, arg1, arg2, arg3, arg4, arg5, arg6, 0f);
	}

	public void SetText(string sourceText, float arg0, float arg1, float arg2, float arg3, float arg4, float arg5, float arg6, float arg7)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int i = 0;
		int writeIndex = 0;
		for (; i < sourceText.Length; i++)
		{
			char c = sourceText[i];
			switch (c)
			{
			case '{':
				num4 = 1;
				continue;
			case '}':
				switch (num)
				{
				case 0:
					AddFloatToInternalTextBackingArray(arg0, num2, num3, ref writeIndex);
					break;
				case 1:
					AddFloatToInternalTextBackingArray(arg1, num2, num3, ref writeIndex);
					break;
				case 2:
					AddFloatToInternalTextBackingArray(arg2, num2, num3, ref writeIndex);
					break;
				case 3:
					AddFloatToInternalTextBackingArray(arg3, num2, num3, ref writeIndex);
					break;
				case 4:
					AddFloatToInternalTextBackingArray(arg4, num2, num3, ref writeIndex);
					break;
				case 5:
					AddFloatToInternalTextBackingArray(arg5, num2, num3, ref writeIndex);
					break;
				case 6:
					AddFloatToInternalTextBackingArray(arg6, num2, num3, ref writeIndex);
					break;
				case 7:
					AddFloatToInternalTextBackingArray(arg7, num2, num3, ref writeIndex);
					break;
				}
				num = 0;
				num4 = 0;
				num2 = 0;
				num3 = 0;
				continue;
			}
			if (num4 == 1 && c >= '0' && c <= '8')
			{
				num = c - 48;
				num4 = 2;
				continue;
			}
			if (num4 == 2)
			{
				switch (c)
				{
				case '.':
					num4 = 3;
					continue;
				case '0':
					num2++;
					continue;
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					num3 = c - 48;
					continue;
				case '#':
				case ',':
				case ':':
					continue;
				}
			}
			if (num4 == 3 && c == '0')
			{
				num3++;
				continue;
			}
			m_TextBackingArray[writeIndex] = c;
			writeIndex++;
		}
		m_TextBackingArray[writeIndex] = 0u;
		m_TextBackingArray.Count = writeIndex;
		m_IsTextBackingStringDirty = true;
		m_inputSource = TextInputSources.SetText;
		PopulateTextProcessingArray();
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	public void SetText(StringBuilder sourceText)
	{
		int length = sourceText?.Length ?? 0;
		SetText(sourceText, 0, length);
	}

	private void SetText(StringBuilder sourceText, int start, int length)
	{
		PopulateTextBackingArray(sourceText, start, length);
		m_IsTextBackingStringDirty = true;
		m_inputSource = TextInputSources.SetTextArray;
		PopulateTextProcessingArray();
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	public void SetText(char[] sourceText)
	{
		int length = ((sourceText != null) ? sourceText.Length : 0);
		SetCharArray(sourceText, 0, length);
	}

	public void SetText(char[] sourceText, int start, int length)
	{
		SetCharArray(sourceText, start, length);
	}

	public void SetCharArray(char[] sourceText)
	{
		int length = ((sourceText != null) ? sourceText.Length : 0);
		SetCharArray(sourceText, 0, length);
	}

	public void SetCharArray(char[] sourceText, int start, int length)
	{
		PopulateTextBackingArray(sourceText, start, length);
		m_IsTextBackingStringDirty = true;
		m_inputSource = TextInputSources.SetTextArray;
		PopulateTextProcessingArray();
		m_havePropertiesChanged = true;
		SetVerticesDirty();
		SetLayoutDirty();
	}

	private TMP_Style GetStyle(int hashCode)
	{
		TMP_Style tMP_Style = null;
		if (m_StyleSheet != null)
		{
			tMP_Style = m_StyleSheet.GetStyle(hashCode);
			if (tMP_Style != null)
			{
				return tMP_Style;
			}
		}
		if (TMP_Settings.defaultStyleSheet != null)
		{
			tMP_Style = TMP_Settings.defaultStyleSheet.GetStyle(hashCode);
		}
		return tMP_Style;
	}

	private void InsertOpeningTextStyle(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		m_TextStyleStackDepth++;
		m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);
		uint[] styleOpeningTagArray = style.styleOpeningTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray);
		m_TextStyleStackDepth--;
	}

	private void InsertClosingTextStyle(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		m_TextStyleStackDepth++;
		m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);
		uint[] styleClosingTagArray = style.styleClosingTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray);
		m_TextStyleStackDepth--;
	}

	private void InsertTextStyleInTextProcessingArray(ref TextProcessingElement[] charBuffer, ref int writeIndex, uint[] styleDefinition)
	{
		int num = styleDefinition.Length;
		if (writeIndex + num >= charBuffer.Length)
		{
			ResizeInternalArray(ref charBuffer, writeIndex + num);
		}
		for (int i = 0; i < num; i++)
		{
			uint num2 = styleDefinition[i];
			if (num2 == 92 && i + 1 < num)
			{
				switch (styleDefinition[i + 1])
				{
				case 92u:
					i++;
					break;
				case 110u:
					num2 = 10u;
					i++;
					break;
				case 117u:
					if (i + 5 < num)
					{
						num2 = GetUTF16(styleDefinition, i + 2);
						i += 5;
					}
					break;
				case 85u:
					if (i + 9 < num)
					{
						num2 = GetUTF32(styleDefinition, i + 2);
						i += 9;
					}
					break;
				}
			}
			if (num2 == 60)
			{
				switch ((MarkupTag)GetMarkupTagHashCode(styleDefinition, i + 1))
				{
				case MarkupTag.NO_PARSE:
					tag_NoParsing = true;
					break;
				case MarkupTag.SLASH_NO_PARSE:
					tag_NoParsing = false;
					break;
				case MarkupTag.BR:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 10u;
						writeIndex++;
						i += 3;
						continue;
					}
					break;
				case MarkupTag.CR:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 13u;
						writeIndex++;
						i += 3;
						continue;
					}
					break;
				case MarkupTag.NBSP:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 160u;
						writeIndex++;
						i += 5;
						continue;
					}
					break;
				case MarkupTag.ZWSP:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 8203u;
						writeIndex++;
						i += 5;
						continue;
					}
					break;
				case MarkupTag.ZWJ:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 8205u;
						writeIndex++;
						i += 4;
						continue;
					}
					break;
				case MarkupTag.SHY:
					if (!tag_NoParsing)
					{
						charBuffer[writeIndex].unicode = 173u;
						writeIndex++;
						i += 4;
						continue;
					}
					break;
				case MarkupTag.STYLE:
				{
					if (!tag_NoParsing && ReplaceOpeningStyleTag(ref styleDefinition, i, out var srcOffset, ref charBuffer, ref writeIndex))
					{
						int num3 = num - srcOffset;
						i = srcOffset;
						if (writeIndex + num3 >= charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer, writeIndex + num3);
						}
						continue;
					}
					break;
				}
				case MarkupTag.SLASH_STYLE:
					if (!tag_NoParsing)
					{
						ReplaceClosingStyleTag(ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
					break;
				}
			}
			charBuffer[writeIndex].unicode = num2;
			writeIndex++;
		}
	}

	private bool ReplaceOpeningStyleTag(ref TextBackingContainer sourceText, int srcIndex, out int srcOffset, ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
		TMP_Style style = GetStyle(styleHashCode);
		if (style == null || srcOffset == 0)
		{
			return false;
		}
		m_TextStyleStackDepth++;
		m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);
		uint[] styleOpeningTagArray = style.styleOpeningTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray);
		m_TextStyleStackDepth--;
		return true;
	}

	private bool ReplaceOpeningStyleTag(ref uint[] sourceText, int srcIndex, out int srcOffset, ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
		TMP_Style style = GetStyle(styleHashCode);
		if (style == null || srcOffset == 0)
		{
			return false;
		}
		m_TextStyleStackDepth++;
		m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);
		uint[] styleOpeningTagArray = style.styleOpeningTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray);
		m_TextStyleStackDepth--;
		return true;
	}

	private void ReplaceClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		int hashCode = m_TextStyleStacks[m_TextStyleStackDepth + 1].Pop();
		TMP_Style style = GetStyle(hashCode);
		if (style != null)
		{
			m_TextStyleStackDepth++;
			uint[] styleClosingTagArray = style.styleClosingTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray);
			m_TextStyleStackDepth--;
		}
	}

	private void InsertOpeningStyleTag(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		if (style != null)
		{
			m_TextStyleStacks[0].Push(style.hashCode);
			uint[] styleOpeningTagArray = style.styleOpeningTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray);
			m_TextStyleStackDepth = 0;
		}
	}

	private void InsertClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex)
	{
		int hashCode = m_TextStyleStacks[0].Pop();
		uint[] styleClosingTagArray = GetStyle(hashCode).styleClosingTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray);
		m_TextStyleStackDepth = 0;
	}

	private int GetMarkupTagHashCode(uint[] styleDefinition, int readIndex)
	{
		int num = 0;
		int num2 = readIndex + 16;
		int num3 = styleDefinition.Length;
		while (readIndex < num2 && readIndex < num3)
		{
			uint num4 = styleDefinition[readIndex];
			if (num4 == 62 || num4 == 61 || num4 == 32)
			{
				return num;
			}
			num = ((num << 5) + num) ^ (int)TMP_TextParsingUtilities.ToUpperASCIIFast(num4);
			readIndex++;
		}
		return num;
	}

	private int GetMarkupTagHashCode(TextBackingContainer styleDefinition, int readIndex)
	{
		int num = 0;
		int num2 = readIndex + 16;
		int capacity = styleDefinition.Capacity;
		while (readIndex < num2 && readIndex < capacity)
		{
			uint num3 = styleDefinition[readIndex];
			if (num3 == 62 || num3 == 61 || num3 == 32)
			{
				return num;
			}
			num = ((num << 5) + num) ^ (int)TMP_TextParsingUtilities.ToUpperASCIIFast(num3);
			readIndex++;
		}
		return num;
	}

	private int GetStyleHashCode(ref uint[] text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Length; i++)
		{
			if (text[i] != 34)
			{
				if (text[i] == 62)
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ TMP_TextParsingUtilities.ToUpperASCIIFast((char)text[i]);
			}
		}
		return num;
	}

	private int GetStyleHashCode(ref TextBackingContainer text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Capacity; i++)
		{
			if (text[i] != 34)
			{
				if (text[i] == 62)
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ TMP_TextParsingUtilities.ToUpperASCIIFast((char)text[i]);
			}
		}
		return num;
	}

	private void ResizeInternalArray<T>(ref T[] array)
	{
		int newSize = Mathf.NextPowerOfTwo(array.Length + 1);
		Array.Resize(ref array, newSize);
	}

	private void ResizeInternalArray<T>(ref T[] array, int size)
	{
		size = Mathf.NextPowerOfTwo(size + 1);
		Array.Resize(ref array, size);
	}

	private void AddFloatToInternalTextBackingArray(float value, int padding, int precision, ref int writeIndex)
	{
		if (value < 0f)
		{
			m_TextBackingArray[writeIndex] = 45u;
			writeIndex++;
			value = 0f - value;
		}
		decimal num = (decimal)value;
		if (padding == 0 && precision == 0)
		{
			precision = 9;
		}
		else
		{
			num += k_Power[Mathf.Min(9, precision)];
		}
		long num2 = (long)num;
		AddIntegerToInternalTextBackingArray(num2, padding, ref writeIndex);
		if (precision <= 0)
		{
			return;
		}
		num -= (decimal)num2;
		if (!(num != 0m))
		{
			return;
		}
		m_TextBackingArray[writeIndex++] = 46u;
		for (int i = 0; i < precision; i++)
		{
			num *= 10m;
			long num3 = (long)num;
			m_TextBackingArray[writeIndex++] = (ushort)(num3 + 48);
			num -= (decimal)num3;
			if (num == 0m)
			{
				i = precision;
			}
		}
	}

	private void AddIntegerToInternalTextBackingArray(double number, int padding, ref int writeIndex)
	{
		int num = 0;
		int num2 = writeIndex;
		do
		{
			m_TextBackingArray[num2++] = (ushort)(number % 10.0 + 48.0);
			number /= 10.0;
			num++;
		}
		while (number > 0.999999999999999 || num < padding);
		int num3 = num2;
		while (writeIndex + 1 < num2)
		{
			num2--;
			uint value = m_TextBackingArray[writeIndex];
			m_TextBackingArray[writeIndex] = m_TextBackingArray[num2];
			m_TextBackingArray[num2] = value;
			writeIndex++;
		}
		writeIndex = num3;
	}

	private string InternalTextBackingArrayToString()
	{
		char[] array = new char[m_TextBackingArray.Count];
		for (int i = 0; i < m_TextBackingArray.Capacity; i++)
		{
			char c = (char)m_TextBackingArray[i];
			if (c == '\0')
			{
				break;
			}
			array[i] = c;
		}
		m_IsTextBackingStringDirty = false;
		return new string(array);
	}

	internal virtual int SetArraySizes(TextProcessingElement[] unicodeChars)
	{
		return 0;
	}

	public Vector2 GetPreferredValues()
	{
		m_isPreferredWidthDirty = true;
		float x = GetPreferredWidth();
		m_isPreferredHeightDirty = true;
		float y = GetPreferredHeight();
		m_isPreferredWidthDirty = true;
		m_isPreferredHeightDirty = true;
		return new Vector2(x, y);
	}

	public Vector2 GetPreferredValues(float width, float height)
	{
		m_isCalculatingPreferredValues = true;
		ParseInputText();
		Vector2 vector = new Vector2(width, height);
		float x = GetPreferredWidth(vector);
		float y = GetPreferredHeight(vector);
		return new Vector2(x, y);
	}

	public Vector2 GetPreferredValues(string text)
	{
		m_isCalculatingPreferredValues = true;
		SetTextInternal(text);
		SetArraySizes(m_TextProcessingArray);
		Vector2 vector = k_LargePositiveVector2;
		float x = GetPreferredWidth(vector);
		float y = GetPreferredHeight(vector);
		return new Vector2(x, y);
	}

	public Vector2 GetPreferredValues(string text, float width, float height)
	{
		m_isCalculatingPreferredValues = true;
		SetTextInternal(text);
		SetArraySizes(m_TextProcessingArray);
		Vector2 vector = new Vector2(width, height);
		float x = GetPreferredWidth(vector, m_TextWrappingMode);
		float y = GetPreferredHeight(vector);
		return new Vector2(x, y);
	}

	protected float GetPreferredWidth()
	{
		if (TMP_Settings.instance == null)
		{
			return 0f;
		}
		if (!m_isPreferredWidthDirty)
		{
			return m_preferredWidth;
		}
		float num = (m_enableAutoSizing ? m_fontSizeMax : m_fontSize);
		m_minFontSize = m_fontSizeMin;
		m_maxFontSize = m_fontSizeMax;
		m_charWidthAdjDelta = 0f;
		Vector2 marginSize = k_LargePositiveVector2;
		m_isCalculatingPreferredValues = true;
		ParseInputText();
		m_AutoSizeIterationCount = 0;
		TextWrappingModes textWrapMode = ((m_TextWrappingMode != TextWrappingModes.Normal && m_TextWrappingMode != 0) ? TextWrappingModes.PreserveWhitespaceNoWrap : TextWrappingModes.NoWrap);
		float x = CalculatePreferredValues(ref num, marginSize, isTextAutoSizingEnabled: false, textWrapMode).x;
		m_isPreferredWidthDirty = false;
		return x;
	}

	private float GetPreferredWidth(Vector2 margin)
	{
		float num = (m_enableAutoSizing ? m_fontSizeMax : m_fontSize);
		m_minFontSize = m_fontSizeMin;
		m_maxFontSize = m_fontSizeMax;
		m_charWidthAdjDelta = 0f;
		m_AutoSizeIterationCount = 0;
		TextWrappingModes textWrapMode = ((m_TextWrappingMode != TextWrappingModes.Normal && m_TextWrappingMode != 0) ? TextWrappingModes.PreserveWhitespaceNoWrap : TextWrappingModes.NoWrap);
		return CalculatePreferredValues(ref num, margin, isTextAutoSizingEnabled: false, textWrapMode).x;
	}

	private float GetPreferredWidth(Vector2 margin, TextWrappingModes wrapMode)
	{
		float num = (m_enableAutoSizing ? m_fontSizeMax : m_fontSize);
		m_minFontSize = m_fontSizeMin;
		m_maxFontSize = m_fontSizeMax;
		m_charWidthAdjDelta = 0f;
		m_AutoSizeIterationCount = 0;
		return CalculatePreferredValues(ref num, margin, isTextAutoSizingEnabled: false, wrapMode).x;
	}

	protected float GetPreferredHeight()
	{
		if (TMP_Settings.instance == null)
		{
			return 0f;
		}
		if (!m_isPreferredHeightDirty)
		{
			return m_preferredHeight;
		}
		float num = (m_enableAutoSizing ? m_fontSizeMax : m_fontSize);
		m_minFontSize = m_fontSizeMin;
		m_maxFontSize = m_fontSizeMax;
		m_charWidthAdjDelta = 0f;
		Vector2 marginSize = new Vector2((m_marginWidth != 0f) ? m_marginWidth : k_LargePositiveFloat, k_LargePositiveFloat);
		m_isCalculatingPreferredValues = true;
		ParseInputText();
		m_IsAutoSizePointSizeSet = false;
		m_AutoSizeIterationCount = 0;
		float result = 0f;
		while (!m_IsAutoSizePointSizeSet)
		{
			result = CalculatePreferredValues(ref num, marginSize, m_enableAutoSizing, m_TextWrappingMode).y;
			m_AutoSizeIterationCount++;
		}
		m_isPreferredHeightDirty = false;
		return result;
	}

	private float GetPreferredHeight(Vector2 margin)
	{
		float num = (m_enableAutoSizing ? m_fontSizeMax : m_fontSize);
		m_minFontSize = m_fontSizeMin;
		m_maxFontSize = m_fontSizeMax;
		m_charWidthAdjDelta = 0f;
		m_IsAutoSizePointSizeSet = false;
		m_AutoSizeIterationCount = 0;
		float result = 0f;
		while (!m_IsAutoSizePointSizeSet)
		{
			result = CalculatePreferredValues(ref num, margin, m_enableAutoSizing, m_TextWrappingMode).y;
			m_AutoSizeIterationCount++;
		}
		return result;
	}

	public Vector2 GetRenderedValues()
	{
		return GetTextBounds().size;
	}

	public Vector2 GetRenderedValues(bool onlyVisibleCharacters)
	{
		return GetTextBounds(onlyVisibleCharacters).size;
	}

	private float GetRenderedWidth()
	{
		return GetRenderedValues().x;
	}

	protected float GetRenderedWidth(bool onlyVisibleCharacters)
	{
		return GetRenderedValues(onlyVisibleCharacters).x;
	}

	private float GetRenderedHeight()
	{
		return GetRenderedValues().y;
	}

	protected float GetRenderedHeight(bool onlyVisibleCharacters)
	{
		return GetRenderedValues(onlyVisibleCharacters).y;
	}

	protected virtual Vector2 CalculatePreferredValues(ref float fontSize, Vector2 marginSize, bool isTextAutoSizingEnabled, TextWrappingModes textWrapMode)
	{
		if (m_fontAsset == null || m_fontAsset.characterLookupTable == null)
		{
			UnityEngine.Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + GetInstanceID());
			m_IsAutoSizePointSizeSet = true;
			return Vector2.zero;
		}
		if (m_TextProcessingArray == null || m_TextProcessingArray.Length == 0 || m_TextProcessingArray[0].unicode == 0)
		{
			m_IsAutoSizePointSizeSet = true;
			return Vector2.zero;
		}
		m_currentFontAsset = m_fontAsset;
		m_currentMaterial = m_sharedMaterial;
		m_currentMaterialIndex = 0;
		m_materialReferenceStack.SetDefault(new MaterialReference(0, m_currentFontAsset, null, m_currentMaterial, m_padding));
		int totalCharacterCount = m_totalCharacterCount;
		if (m_internalCharacterInfo == null || totalCharacterCount > m_internalCharacterInfo.Length)
		{
			m_internalCharacterInfo = new TMP_CharacterInfo[(totalCharacterCount > 1024) ? (totalCharacterCount + 256) : Mathf.NextPowerOfTwo(totalCharacterCount)];
		}
		float num = (m_isOrthographic ? 1f : 0.1f);
		float num2 = fontSize / m_fontAsset.faceInfo.pointSize * m_fontAsset.faceInfo.scale * num;
		float num3 = num2;
		float num4 = fontSize * 0.01f * num;
		m_fontScaleMultiplier = 1f;
		m_currentFontSize = fontSize;
		m_sizeStack.SetDefault(m_currentFontSize);
		float num5 = 0f;
		m_FontStyleInternal = m_fontStyle;
		m_lineJustification = m_HorizontalAlignment;
		m_lineJustificationStack.SetDefault(m_lineJustification);
		m_baselineOffset = 0f;
		m_baselineOffsetStack.Clear();
		m_FXScale = Vector3.one;
		m_lineOffset = 0f;
		m_lineHeight = -32767f;
		float num6 = m_currentFontAsset.faceInfo.lineHeight - (m_currentFontAsset.faceInfo.ascentLine - m_currentFontAsset.faceInfo.descentLine);
		m_cSpacing = 0f;
		m_monoSpacing = 0f;
		m_xAdvance = 0f;
		tag_LineIndent = 0f;
		tag_Indent = 0f;
		m_indentStack.SetDefault(0f);
		tag_NoParsing = false;
		m_characterCount = 0;
		m_firstCharacterOfLine = 0;
		m_maxLineAscender = k_LargeNegativeFloat;
		m_maxLineDescender = k_LargePositiveFloat;
		m_lineNumber = 0;
		m_startOfLineAscender = 0f;
		m_IsDrivenLineSpacing = false;
		m_LastBaseGlyphIndex = int.MinValue;
		float x = marginSize.x;
		m_marginLeft = 0f;
		m_marginRight = 0f;
		m_width = -1f;
		float num7 = x + 0.0001f - m_marginLeft - m_marginRight;
		m_RenderedWidth = 0f;
		m_RenderedHeight = 0f;
		float num8 = 0f;
		m_isCalculatingPreferredValues = true;
		m_maxCapHeight = 0f;
		m_maxTextAscender = 0f;
		m_ElementDescender = 0f;
		bool flag = false;
		bool flag2 = true;
		m_isNonBreakingSpace = false;
		bool flag3 = false;
		CharacterSubstitution characterSubstitution = new CharacterSubstitution(-1, 0u);
		bool flag4 = false;
		WordWrapState state = default(WordWrapState);
		WordWrapState state2 = default(WordWrapState);
		WordWrapState state3 = default(WordWrapState);
		m_AutoSizeIterationCount++;
		for (int i = 0; i < m_TextProcessingArray.Length && m_TextProcessingArray[i].unicode != 0; i++)
		{
			uint num9 = m_TextProcessingArray[i].unicode;
			if (num9 == 26)
			{
				continue;
			}
			if (m_isRichText && num9 == 60)
			{
				m_isTextLayoutPhase = true;
				m_textElementType = TMP_TextElementType.Character;
				if (ValidateHtmlTag(m_TextProcessingArray, i + 1, out var endIndex))
				{
					i = endIndex;
					if (m_textElementType == TMP_TextElementType.Character)
					{
						continue;
					}
				}
			}
			else
			{
				m_textElementType = m_textInfo.characterInfo[m_characterCount].elementType;
				m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
				m_currentFontAsset = m_textInfo.characterInfo[m_characterCount].fontAsset;
			}
			int currentMaterialIndex = m_currentMaterialIndex;
			bool isUsingAlternateTypeface = m_textInfo.characterInfo[m_characterCount].isUsingAlternateTypeface;
			m_isTextLayoutPhase = false;
			bool flag5 = false;
			if (characterSubstitution.index == m_characterCount)
			{
				num9 = characterSubstitution.unicode;
				m_textElementType = TMP_TextElementType.Character;
				flag5 = true;
				switch (num9)
				{
				case 3u:
					m_internalCharacterInfo[m_characterCount].textElement = m_currentFontAsset.characterLookupTable[3u];
					m_isTextTruncated = true;
					break;
				case 8230u:
					m_internalCharacterInfo[m_characterCount].textElement = m_Ellipsis.character;
					m_internalCharacterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
					m_internalCharacterInfo[m_characterCount].fontAsset = m_Ellipsis.fontAsset;
					m_internalCharacterInfo[m_characterCount].material = m_Ellipsis.material;
					m_internalCharacterInfo[m_characterCount].materialReferenceIndex = m_Ellipsis.materialIndex;
					m_isTextTruncated = true;
					characterSubstitution.index = m_characterCount + 1;
					characterSubstitution.unicode = 3u;
					break;
				}
			}
			if (m_characterCount < m_firstVisibleCharacter && num9 != 3)
			{
				m_internalCharacterInfo[m_characterCount].isVisible = false;
				m_internalCharacterInfo[m_characterCount].character = '\u200b';
				m_internalCharacterInfo[m_characterCount].lineNumber = 0;
				m_characterCount++;
				continue;
			}
			float num10 = 1f;
			if (m_textElementType == TMP_TextElementType.Character)
			{
				if ((m_FontStyleInternal & FontStyles.UpperCase) == FontStyles.UpperCase)
				{
					if (char.IsLower((char)num9))
					{
						num9 = char.ToUpper((char)num9);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.LowerCase) == FontStyles.LowerCase)
				{
					if (char.IsUpper((char)num9))
					{
						num9 = char.ToLower((char)num9);
					}
				}
				else if ((m_FontStyleInternal & FontStyles.SmallCaps) == FontStyles.SmallCaps && char.IsLower((char)num9))
				{
					num10 = 0.8f;
					num9 = char.ToUpper((char)num9);
				}
			}
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			FaceInfo faceInfo = m_currentFontAsset.faceInfo;
			if (m_textElementType == TMP_TextElementType.Sprite)
			{
				TMP_SpriteCharacter tMP_SpriteCharacter = (TMP_SpriteCharacter)m_textInfo.characterInfo[m_characterCount].textElement;
				if (tMP_SpriteCharacter == null)
				{
					continue;
				}
				m_currentSpriteAsset = tMP_SpriteCharacter.textAsset as TMP_SpriteAsset;
				m_spriteIndex = (int)tMP_SpriteCharacter.glyphIndex;
				if (num9 == 60)
				{
					num9 = (uint)(57344 + m_spriteIndex);
				}
				FaceInfo faceInfo2 = m_currentSpriteAsset.faceInfo;
				if (faceInfo2.pointSize > 0f)
				{
					float num14 = m_currentFontSize / faceInfo2.pointSize * faceInfo2.scale * num;
					num3 = tMP_SpriteCharacter.scale * tMP_SpriteCharacter.glyph.scale * num14;
					num12 = faceInfo2.ascentLine;
					num13 = faceInfo2.descentLine;
				}
				else
				{
					float num15 = m_currentFontSize / faceInfo.pointSize * faceInfo.scale * num;
					num3 = faceInfo.ascentLine / tMP_SpriteCharacter.glyph.metrics.height * tMP_SpriteCharacter.scale * tMP_SpriteCharacter.glyph.scale * num15;
					float num16 = ((num3 != 0f) ? (num15 / num3) : 0f);
					num12 = faceInfo.ascentLine * num16;
					num13 = faceInfo.descentLine * num16;
				}
				m_cached_TextElement = tMP_SpriteCharacter;
				m_internalCharacterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;
				m_internalCharacterInfo[m_characterCount].scale = num3;
				m_currentMaterialIndex = currentMaterialIndex;
			}
			else if (m_textElementType == TMP_TextElementType.Character)
			{
				m_cached_TextElement = m_textInfo.characterInfo[m_characterCount].textElement;
				if (m_cached_TextElement == null)
				{
					continue;
				}
				m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
				float num17 = ((!flag5 || m_TextProcessingArray[i].unicode != 10 || m_characterCount == m_firstCharacterOfLine) ? (m_currentFontSize * num10 / faceInfo.pointSize * faceInfo.scale * num) : (m_textInfo.characterInfo[m_characterCount - 1].pointSize * num10 / faceInfo.pointSize * faceInfo.scale * num));
				if (flag5 && num9 == 8230)
				{
					num12 = 0f;
					num13 = 0f;
				}
				else
				{
					num12 = faceInfo.ascentLine;
					num13 = faceInfo.descentLine;
				}
				num3 = num17 * m_fontScaleMultiplier * m_cached_TextElement.scale * m_cached_TextElement.m_Glyph.scale;
				m_internalCharacterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
			}
			float num18 = num3;
			if (num9 == 173 || num9 == 3)
			{
				num3 = 0f;
			}
			m_internalCharacterInfo[m_characterCount].character = (char)num9;
			GlyphMetrics glyphMetrics = m_textInfo.characterInfo[m_characterCount].alternativeGlyph?.metrics ?? m_cached_TextElement.m_Glyph.metrics;
			bool flag6 = num9 <= 65535 && char.IsWhiteSpace((char)num9);
			GlyphValueRecord glyphValueRecord = default(GlyphValueRecord);
			float num19 = m_characterSpacing;
			if (m_enableKerning && m_textElementType == TMP_TextElementType.Character)
			{
				uint glyphIndex = m_cached_TextElement.m_GlyphIndex;
				GlyphPairAdjustmentRecord value;
				if (m_characterCount < totalCharacterCount - 1 && m_textInfo.characterInfo[m_characterCount + 1].elementType == TMP_TextElementType.Character)
				{
					uint key = (m_textInfo.characterInfo[m_characterCount + 1].textElement.m_GlyphIndex << 16) | glyphIndex;
					if (m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key, out value))
					{
						glyphValueRecord = value.firstAdjustmentRecord.glyphValueRecord;
						num19 = (((value.featureLookupFlags & UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) == UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) ? 0f : num19);
					}
				}
				if (m_characterCount >= 1)
				{
					uint glyphIndex2 = m_textInfo.characterInfo[m_characterCount - 1].textElement.m_GlyphIndex;
					uint key2 = (glyphIndex << 16) | glyphIndex2;
					if (textInfo.characterInfo[m_characterCount - 1].elementType == TMP_TextElementType.Character && m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key2, out value))
					{
						glyphValueRecord += value.secondAdjustmentRecord.glyphValueRecord;
						num19 = (((value.featureLookupFlags & UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) == UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments) ? 0f : num19);
					}
				}
				m_internalCharacterInfo[m_characterCount].adjustedHorizontalAdvance = glyphValueRecord.xAdvance;
			}
			bool flag7 = TMP_TextParsingUtilities.IsBaseGlyph(num9);
			if (flag7)
			{
				m_LastBaseGlyphIndex = m_characterCount;
			}
			if (m_characterCount > 0 && !flag7)
			{
				if (m_LastBaseGlyphIndex != int.MinValue && m_LastBaseGlyphIndex == m_characterCount - 1)
				{
					uint index = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph.index;
					uint key3 = (m_cached_TextElement.glyphIndex << 16) | index;
					if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key3, out var value2))
					{
						float num20 = (m_internalCharacterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) / num3;
						glyphValueRecord.xPlacement = num20 + value2.baseGlyphAnchorPoint.xCoordinate - value2.markPositionAdjustment.xPositionAdjustment;
						glyphValueRecord.yPlacement = value2.baseGlyphAnchorPoint.yCoordinate - value2.markPositionAdjustment.yPositionAdjustment;
						num19 = 0f;
					}
				}
				else
				{
					bool flag8 = false;
					int num21 = m_characterCount - 1;
					while (num21 >= 0 && num21 != m_LastBaseGlyphIndex)
					{
						uint index2 = m_textInfo.characterInfo[num21].textElement.glyph.index;
						uint key4 = (m_cached_TextElement.glyphIndex << 16) | index2;
						if (m_currentFontAsset.fontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.TryGetValue(key4, out var value3))
						{
							float num22 = (m_textInfo.characterInfo[num21].origin - m_xAdvance) / num3;
							float num23 = num11 - m_lineOffset + m_baselineOffset;
							float num24 = (m_internalCharacterInfo[num21].baseLine - num23) / num3;
							glyphValueRecord.xPlacement = num22 + value3.baseMarkGlyphAnchorPoint.xCoordinate - value3.combiningMarkPositionAdjustment.xPositionAdjustment;
							glyphValueRecord.yPlacement = num24 + value3.baseMarkGlyphAnchorPoint.yCoordinate - value3.combiningMarkPositionAdjustment.yPositionAdjustment;
							num19 = 0f;
							flag8 = true;
							break;
						}
						num21--;
					}
					if (m_LastBaseGlyphIndex != int.MinValue && !flag8)
					{
						uint index3 = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph.index;
						uint key5 = (m_cached_TextElement.glyphIndex << 16) | index3;
						if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key5, out var value4))
						{
							float num25 = (m_internalCharacterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) / num3;
							glyphValueRecord.xPlacement = num25 + value4.baseGlyphAnchorPoint.xCoordinate - value4.markPositionAdjustment.xPositionAdjustment;
							glyphValueRecord.yPlacement = value4.baseGlyphAnchorPoint.yCoordinate - value4.markPositionAdjustment.yPositionAdjustment;
							num19 = 0f;
						}
					}
				}
			}
			num12 += glyphValueRecord.yPlacement;
			num13 += glyphValueRecord.yPlacement;
			float num26 = 0f;
			if (m_monoSpacing != 0f)
			{
				num26 = (m_monoSpacing / 2f - (m_cached_TextElement.glyph.metrics.width / 2f + m_cached_TextElement.glyph.metrics.horizontalBearingX) * num3) * (1f - m_charWidthAdjDelta);
				m_xAdvance += num26;
			}
			float num27 = 0f;
			if (m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && (m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold)
			{
				num27 = m_currentFontAsset.boldSpacing;
			}
			m_internalCharacterInfo[m_characterCount].origin = m_xAdvance + glyphValueRecord.xPlacement * num3;
			m_internalCharacterInfo[m_characterCount].baseLine = num11 - m_lineOffset + m_baselineOffset + glyphValueRecord.yPlacement * num3;
			float num28 = ((m_textElementType == TMP_TextElementType.Character) ? (num12 * num3 / num10 + m_baselineOffset) : (num12 * num3 + m_baselineOffset));
			float num29 = ((m_textElementType == TMP_TextElementType.Character) ? (num13 * num3 / num10 + m_baselineOffset) : (num13 * num3 + m_baselineOffset));
			float num30 = num28;
			float num31 = num29;
			bool flag9 = m_characterCount == m_firstCharacterOfLine;
			if (flag9 || !flag6)
			{
				if (m_baselineOffset != 0f)
				{
					num30 = Mathf.Max((num28 - m_baselineOffset) / m_fontScaleMultiplier, num30);
					num31 = Mathf.Min((num29 - m_baselineOffset) / m_fontScaleMultiplier, num31);
				}
				m_maxLineAscender = Mathf.Max(num30, m_maxLineAscender);
				m_maxLineDescender = Mathf.Min(num31, m_maxLineDescender);
			}
			if (flag9 || !flag6)
			{
				m_internalCharacterInfo[m_characterCount].adjustedAscender = num30;
				m_internalCharacterInfo[m_characterCount].adjustedDescender = num31;
				m_ElementAscender = (m_internalCharacterInfo[m_characterCount].ascender = num28 - m_lineOffset);
				m_ElementDescender = (m_internalCharacterInfo[m_characterCount].descender = num29 - m_lineOffset);
			}
			else
			{
				m_internalCharacterInfo[m_characterCount].adjustedAscender = m_maxLineAscender;
				m_internalCharacterInfo[m_characterCount].adjustedDescender = m_maxLineDescender;
				m_ElementAscender = (m_internalCharacterInfo[m_characterCount].ascender = m_maxLineAscender - m_lineOffset);
				m_ElementDescender = (m_internalCharacterInfo[m_characterCount].descender = m_maxLineDescender - m_lineOffset);
			}
			if ((m_lineNumber == 0 || m_isNewPage) && (flag9 || !flag6))
			{
				m_maxTextAscender = m_maxLineAscender;
				m_maxCapHeight = Mathf.Max(m_maxCapHeight, m_currentFontAsset.m_FaceInfo.capLine * num3 / num10);
			}
			if (m_lineOffset == 0f && (!flag6 || m_characterCount == m_firstCharacterOfLine))
			{
				m_PageAscender = ((m_PageAscender > num28) ? m_PageAscender : num28);
			}
			bool flag10 = (m_lineJustification & HorizontalAlignmentOptions.Flush) == HorizontalAlignmentOptions.Flush || (m_lineJustification & HorizontalAlignmentOptions.Justified) == HorizontalAlignmentOptions.Justified;
			if (num9 == 9 || ((textWrapMode == TextWrappingModes.PreserveWhitespace || textWrapMode == TextWrappingModes.PreserveWhitespaceNoWrap) && (flag6 || num9 == 8203)) || (!flag6 && num9 != 8203 && num9 != 173 && num9 != 3) || (num9 == 173 && !flag4) || m_textElementType == TMP_TextElementType.Sprite)
			{
				num7 = ((m_width != -1f) ? Mathf.Min(x + 0.0001f - m_marginLeft - m_marginRight, m_width) : (x + 0.0001f - m_marginLeft - m_marginRight));
				num8 = Mathf.Abs(m_xAdvance) + glyphMetrics.horizontalAdvance * (1f - m_charWidthAdjDelta) * ((num9 == 173) ? num18 : num3);
				_ = m_characterCount;
				if (flag7 && num8 > num7 * (flag10 ? 1.05f : 1f) && textWrapMode != 0 && textWrapMode != TextWrappingModes.PreserveWhitespaceNoWrap && m_characterCount != m_firstCharacterOfLine)
				{
					i = RestoreWordWrappingState(ref state);
					if (m_internalCharacterInfo[m_characterCount - 1].character == '\u00ad' && !flag4 && m_overflowMode == TextOverflowModes.Overflow)
					{
						characterSubstitution.index = m_characterCount - 1;
						characterSubstitution.unicode = 45u;
						i--;
						m_characterCount--;
						continue;
					}
					flag4 = false;
					if (m_internalCharacterInfo[m_characterCount].character == '\u00ad')
					{
						flag4 = true;
						continue;
					}
					if (isTextAutoSizingEnabled && flag2)
					{
						if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
						{
							float num32 = num8;
							if (m_charWidthAdjDelta > 0f)
							{
								num32 /= 1f - m_charWidthAdjDelta;
							}
							float num33 = num8 - (num7 - 0.0001f) * (flag10 ? 1.05f : 1f);
							m_charWidthAdjDelta += num33 / num32;
							m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100f);
							return Vector2.zero;
						}
						if (fontSize > m_fontSizeMin && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
						{
							m_maxFontSize = fontSize;
							float num34 = Mathf.Max((fontSize - m_minFontSize) / 2f, 0.05f);
							fontSize -= num34;
							fontSize = Mathf.Max((float)(int)(fontSize * 20f + 0.5f) / 20f, m_fontSizeMin);
							return Vector2.zero;
						}
					}
					float num35 = m_maxLineAscender - m_startOfLineAscender;
					if (m_lineOffset > 0f && Math.Abs(num35) > 0.01f && !m_IsDrivenLineSpacing && !m_isNewPage)
					{
						m_ElementDescender -= num35;
						m_lineOffset += num35;
					}
					_ = m_maxLineAscender;
					_ = m_lineOffset;
					float num36 = m_maxLineDescender - m_lineOffset;
					m_ElementDescender = ((m_ElementDescender < num36) ? m_ElementDescender : num36);
					if (!flag)
					{
						_ = m_ElementDescender;
					}
					if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
					{
						flag = true;
					}
					m_firstCharacterOfLine = m_characterCount;
					m_lineVisibleCharacterCount = 0;
					SaveWordWrappingState(ref state2, i, m_characterCount - 1);
					m_lineNumber++;
					float adjustedAscender = m_internalCharacterInfo[m_characterCount].adjustedAscender;
					if (m_lineHeight == -32767f)
					{
						m_lineOffset += 0f - m_maxLineDescender + adjustedAscender + (num6 + m_lineSpacingDelta) * num2 + m_lineSpacing * num4;
						m_IsDrivenLineSpacing = false;
					}
					else
					{
						m_lineOffset += m_lineHeight + m_lineSpacing * num4;
						m_IsDrivenLineSpacing = true;
					}
					m_maxLineAscender = k_LargeNegativeFloat;
					m_maxLineDescender = k_LargePositiveFloat;
					m_startOfLineAscender = adjustedAscender;
					m_xAdvance = 0f + tag_Indent;
					flag2 = true;
					continue;
				}
				m_RenderedWidth = Mathf.Max(m_RenderedWidth, num8 + m_marginLeft + m_marginRight);
				m_RenderedHeight = Mathf.Max(m_RenderedHeight, m_maxTextAscender - m_ElementDescender);
			}
			if (m_lineOffset > 0f && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && !m_IsDrivenLineSpacing && !m_isNewPage)
			{
				float num37 = m_maxLineAscender - m_startOfLineAscender;
				m_ElementDescender -= num37;
				m_lineOffset += num37;
				m_RenderedHeight += num37;
				m_startOfLineAscender += num37;
				state.lineOffset = m_lineOffset;
				state.startOfLineAscender = m_startOfLineAscender;
			}
			if (num9 == 9)
			{
				float num38 = m_currentFontAsset.faceInfo.tabWidth * (float)(int)m_currentFontAsset.tabSize * num3;
				float num39 = Mathf.Ceil(m_xAdvance / num38) * num38;
				m_xAdvance = ((num39 > m_xAdvance) ? num39 : (m_xAdvance + num38));
			}
			else if (m_monoSpacing != 0f)
			{
				m_xAdvance += (m_monoSpacing - num26 + (m_currentFontAsset.normalSpacingOffset + num19) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (flag6 || num9 == 8203)
				{
					m_xAdvance += m_wordSpacing * num4;
				}
			}
			else
			{
				m_xAdvance += ((glyphMetrics.horizontalAdvance * m_FXScale.x + glyphValueRecord.xAdvance) * num3 + (m_currentFontAsset.normalSpacingOffset + num19 + num27) * num4 + m_cSpacing) * (1f - m_charWidthAdjDelta);
				if (flag6 || num9 == 8203)
				{
					m_xAdvance += m_wordSpacing * num4;
				}
			}
			if (num9 == 13)
			{
				m_xAdvance = 0f + tag_Indent;
			}
			if (num9 == 10 || num9 == 11 || num9 == 3 || num9 == 8232 || num9 == 8233 || m_characterCount == totalCharacterCount - 1)
			{
				float num40 = m_maxLineAscender - m_startOfLineAscender;
				if (m_lineOffset > 0f && Math.Abs(num40) > 0.01f && !m_IsDrivenLineSpacing && !m_isNewPage)
				{
					m_ElementDescender -= num40;
					m_lineOffset += num40;
				}
				m_isNewPage = false;
				float num41 = m_maxLineDescender - m_lineOffset;
				m_ElementDescender = ((m_ElementDescender < num41) ? m_ElementDescender : num41);
				if (num9 == 10 || num9 == 11 || (num9 == 45 && flag5) || num9 == 8232 || num9 == 8233)
				{
					SaveWordWrappingState(ref state2, i, m_characterCount);
					SaveWordWrappingState(ref state, i, m_characterCount);
					m_lineNumber++;
					m_firstCharacterOfLine = m_characterCount + 1;
					flag2 = true;
					float adjustedAscender2 = m_internalCharacterInfo[m_characterCount].adjustedAscender;
					if (m_lineHeight == -32767f)
					{
						float num42 = 0f - m_maxLineDescender + adjustedAscender2 + (num6 + m_lineSpacingDelta) * num2 + (m_lineSpacing + ((num9 == 10 || num9 == 8233) ? m_paragraphSpacing : 0f)) * num4;
						m_lineOffset += num42;
						m_IsDrivenLineSpacing = false;
					}
					else
					{
						m_lineOffset += m_lineHeight + (m_lineSpacing + ((num9 == 10 || num9 == 8233) ? m_paragraphSpacing : 0f)) * num4;
						m_IsDrivenLineSpacing = true;
					}
					m_maxLineAscender = k_LargeNegativeFloat;
					m_maxLineDescender = k_LargePositiveFloat;
					m_startOfLineAscender = adjustedAscender2;
					m_xAdvance = 0f + tag_LineIndent + tag_Indent;
					m_characterCount++;
					continue;
				}
				if (num9 == 3)
				{
					i = m_TextProcessingArray.Length;
				}
			}
			if ((textWrapMode != 0 && textWrapMode != TextWrappingModes.PreserveWhitespaceNoWrap) || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
			{
				bool flag11 = false;
				bool flag12 = false;
				uint num43 = ((m_characterCount + 1 < totalCharacterCount) ? m_textInfo.characterInfo[m_characterCount + 1].character : '\0');
				if ((flag6 || num9 == 8203 || num9 == 45 || num9 == 173) && (!m_isNonBreakingSpace || flag3) && num9 != 160 && num9 != 8199 && num9 != 8209 && num9 != 8239 && num9 != 8288)
				{
					if (num9 != 45 || m_characterCount <= 0 || !char.IsWhiteSpace(m_textInfo.characterInfo[m_characterCount - 1].character) || m_textInfo.characterInfo[m_characterCount - 1].lineNumber != m_lineNumber)
					{
						flag2 = false;
						flag11 = true;
						state3.previous_WordBreak = -1;
					}
				}
				else if (!m_isNonBreakingSpace && ((TMP_TextParsingUtilities.IsHangul(num9) && !TMP_Settings.useModernHangulLineBreakingRules) || TMP_TextParsingUtilities.IsCJK(num9)))
				{
					bool num44 = TMP_Settings.linebreakingRules.leadingCharacters.Contains(num9);
					bool flag13 = m_characterCount < totalCharacterCount - 1 && TMP_Settings.linebreakingRules.followingCharacters.Contains(num43);
					if (!num44)
					{
						if (!flag13)
						{
							flag2 = false;
							flag11 = true;
						}
						if (flag2)
						{
							if (flag6)
							{
								flag12 = true;
							}
							flag11 = true;
						}
					}
					else if (flag2 && flag9)
					{
						if (flag6)
						{
							flag12 = true;
						}
						flag11 = true;
					}
				}
				else if (!m_isNonBreakingSpace && TMP_TextParsingUtilities.IsCJK(num43) && !TMP_Settings.linebreakingRules.followingCharacters.Contains(num43))
				{
					flag11 = true;
				}
				else if (flag2)
				{
					if ((flag6 && num9 != 160) || (num9 == 173 && !flag4))
					{
						flag12 = true;
					}
					flag11 = true;
				}
				if (flag11)
				{
					SaveWordWrappingState(ref state, i, m_characterCount);
				}
				if (flag12)
				{
					SaveWordWrappingState(ref state3, i, m_characterCount);
				}
			}
			m_characterCount++;
		}
		num5 = m_maxFontSize - m_minFontSize;
		if (isTextAutoSizingEnabled && num5 > 0.051f && fontSize < m_fontSizeMax && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
		{
			if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
			{
				m_charWidthAdjDelta = 0f;
			}
			m_minFontSize = fontSize;
			float num45 = Mathf.Max((m_maxFontSize - fontSize) / 2f, 0.05f);
			fontSize += num45;
			fontSize = Mathf.Min((float)(int)(fontSize * 20f + 0.5f) / 20f, m_fontSizeMax);
			return Vector2.zero;
		}
		m_IsAutoSizePointSizeSet = true;
		m_isCalculatingPreferredValues = false;
		m_RenderedWidth += ((m_margin.x > 0f) ? m_margin.x : 0f);
		m_RenderedWidth += ((m_margin.z > 0f) ? m_margin.z : 0f);
		m_RenderedHeight += ((m_margin.y > 0f) ? m_margin.y : 0f);
		m_RenderedHeight += ((m_margin.w > 0f) ? m_margin.w : 0f);
		m_RenderedWidth = (float)(int)(m_RenderedWidth * 100f + 1f) / 100f;
		m_RenderedHeight = (float)(int)(m_RenderedHeight * 100f + 1f) / 100f;
		return new Vector2(m_RenderedWidth, m_RenderedHeight);
	}

	protected virtual Bounds GetCompoundBounds()
	{
		return default(Bounds);
	}

	internal virtual Rect GetCanvasSpaceClippingRect()
	{
		return Rect.zero;
	}

	protected Bounds GetTextBounds()
	{
		if (m_textInfo == null || m_textInfo.characterCount > m_textInfo.characterInfo.Length)
		{
			return default(Bounds);
		}
		Extents extents = new Extents(k_LargePositiveVector2, k_LargeNegativeVector2);
		for (int i = 0; i < m_textInfo.characterCount && i < m_textInfo.characterInfo.Length; i++)
		{
			if (m_textInfo.characterInfo[i].isVisible)
			{
				extents.min.x = Mathf.Min(extents.min.x, m_textInfo.characterInfo[i].origin);
				extents.min.y = Mathf.Min(extents.min.y, m_textInfo.characterInfo[i].descender);
				extents.max.x = Mathf.Max(extents.max.x, m_textInfo.characterInfo[i].xAdvance);
				extents.max.y = Mathf.Max(extents.max.y, m_textInfo.characterInfo[i].ascender);
			}
		}
		Vector2 vector = default(Vector2);
		vector.x = extents.max.x - extents.min.x;
		vector.y = extents.max.y - extents.min.y;
		return new Bounds((extents.min + extents.max) / 2f, vector);
	}

	protected Bounds GetTextBounds(bool onlyVisibleCharacters)
	{
		if (m_textInfo == null)
		{
			return default(Bounds);
		}
		Extents extents = new Extents(k_LargePositiveVector2, k_LargeNegativeVector2);
		for (int i = 0; i < m_textInfo.characterCount && !((i > maxVisibleCharacters || m_textInfo.characterInfo[i].lineNumber > m_maxVisibleLines) && onlyVisibleCharacters); i++)
		{
			if (!onlyVisibleCharacters || m_textInfo.characterInfo[i].isVisible)
			{
				extents.min.x = Mathf.Min(extents.min.x, m_textInfo.characterInfo[i].origin);
				extents.min.y = Mathf.Min(extents.min.y, m_textInfo.characterInfo[i].descender);
				extents.max.x = Mathf.Max(extents.max.x, m_textInfo.characterInfo[i].xAdvance);
				extents.max.y = Mathf.Max(extents.max.y, m_textInfo.characterInfo[i].ascender);
			}
		}
		Vector2 vector = default(Vector2);
		vector.x = extents.max.x - extents.min.x;
		vector.y = extents.max.y - extents.min.y;
		return new Bounds((extents.min + extents.max) / 2f, vector);
	}

	protected void AdjustLineOffset(int startIndex, int endIndex, float offset)
	{
		Vector3 vector = new Vector3(0f, offset, 0f);
		for (int i = startIndex; i <= endIndex; i++)
		{
			m_textInfo.characterInfo[i].bottomLeft -= vector;
			m_textInfo.characterInfo[i].topLeft -= vector;
			m_textInfo.characterInfo[i].topRight -= vector;
			m_textInfo.characterInfo[i].bottomRight -= vector;
			m_textInfo.characterInfo[i].ascender -= vector.y;
			m_textInfo.characterInfo[i].baseLine -= vector.y;
			m_textInfo.characterInfo[i].descender -= vector.y;
			if (m_textInfo.characterInfo[i].isVisible)
			{
				m_textInfo.characterInfo[i].vertex_BL.position -= vector;
				m_textInfo.characterInfo[i].vertex_TL.position -= vector;
				m_textInfo.characterInfo[i].vertex_TR.position -= vector;
				m_textInfo.characterInfo[i].vertex_BR.position -= vector;
			}
		}
	}

	protected void ResizeLineExtents(int size)
	{
		size = ((size > 1024) ? (size + 256) : Mathf.NextPowerOfTwo(size + 1));
		TMP_LineInfo[] array = new TMP_LineInfo[size];
		for (int i = 0; i < size; i++)
		{
			if (i < m_textInfo.lineInfo.Length)
			{
				array[i] = m_textInfo.lineInfo[i];
				continue;
			}
			array[i].lineExtents.min = k_LargePositiveVector2;
			array[i].lineExtents.max = k_LargeNegativeVector2;
			array[i].ascender = k_LargeNegativeFloat;
			array[i].descender = k_LargePositiveFloat;
		}
		m_textInfo.lineInfo = array;
	}

	public virtual TMP_TextInfo GetTextInfo(string text)
	{
		return null;
	}

	public virtual void ComputeMarginSize()
	{
	}

	internal void InsertNewLine(int i, float baseScale, float currentElementScale, float currentEmScale, float boldSpacingAdjustment, float characterSpacingAdjustment, float width, float lineGap, ref bool isMaxVisibleDescenderSet, ref float maxVisibleDescender)
	{
		float num = m_maxLineAscender - m_startOfLineAscender;
		if (m_lineOffset > 0f && Math.Abs(num) > 0.01f && !m_IsDrivenLineSpacing && !m_isNewPage)
		{
			AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, num);
			m_ElementDescender -= num;
			m_lineOffset += num;
		}
		float num2 = m_maxLineAscender - m_lineOffset;
		float num3 = m_maxLineDescender - m_lineOffset;
		m_ElementDescender = ((m_ElementDescender < num3) ? m_ElementDescender : num3);
		if (!isMaxVisibleDescenderSet)
		{
			maxVisibleDescender = m_ElementDescender;
		}
		if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
		{
			isMaxVisibleDescenderSet = true;
		}
		m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
		m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = (m_firstVisibleCharacterOfLine = ((m_firstCharacterOfLine > m_firstVisibleCharacterOfLine) ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine));
		m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = (m_lastCharacterOfLine = ((m_characterCount - 1 > 0) ? (m_characterCount - 1) : 0));
		m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = (m_lastVisibleCharacterOfLine = ((m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine) ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine));
		m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
		m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
		m_textInfo.lineInfo[m_lineNumber].visibleSpaceCount = m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex + 1 - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex - m_lineVisibleCharacterCount;
		m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, num3);
		m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, num2);
		m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x;
		m_textInfo.lineInfo[m_lineNumber].width = width;
		float num4 = (m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].adjustedHorizontalAdvance * currentElementScale + (m_currentFontAsset.normalSpacingOffset + characterSpacingAdjustment + boldSpacingAdjustment) * currentEmScale + m_cSpacing) * (1f - m_charWidthAdjDelta);
		float xAdvance = (m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance + (m_isRightToLeft ? num4 : (0f - num4)));
		m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance = xAdvance;
		m_textInfo.lineInfo[m_lineNumber].baseline = 0f - m_lineOffset;
		m_textInfo.lineInfo[m_lineNumber].ascender = num2;
		m_textInfo.lineInfo[m_lineNumber].descender = num3;
		m_textInfo.lineInfo[m_lineNumber].lineHeight = num2 - num3 + lineGap * baseScale;
		m_firstCharacterOfLine = m_characterCount;
		m_lineVisibleCharacterCount = 0;
		m_lineVisibleSpaceCount = 0;
		SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount - 1);
		m_lineNumber++;
		if (m_lineNumber >= m_textInfo.lineInfo.Length)
		{
			ResizeLineExtents(m_lineNumber);
		}
		if (m_lineHeight == -32767f)
		{
			float adjustedAscender = m_textInfo.characterInfo[m_characterCount].adjustedAscender;
			float num5 = 0f - m_maxLineDescender + adjustedAscender + (lineGap + m_lineSpacingDelta) * baseScale + m_lineSpacing * currentEmScale;
			m_lineOffset += num5;
			m_startOfLineAscender = adjustedAscender;
		}
		else
		{
			m_lineOffset += m_lineHeight + m_lineSpacing * currentEmScale;
		}
		m_maxLineAscender = k_LargeNegativeFloat;
		m_maxLineDescender = k_LargePositiveFloat;
		m_xAdvance = 0f + tag_Indent;
	}

	internal void SaveWordWrappingState(ref WordWrapState state, int index, int count)
	{
		state.currentFontAsset = m_currentFontAsset;
		state.currentSpriteAsset = m_currentSpriteAsset;
		state.currentMaterial = m_currentMaterial;
		state.currentMaterialIndex = m_currentMaterialIndex;
		state.previous_WordBreak = index;
		state.total_CharacterCount = count;
		state.visible_CharacterCount = m_lineVisibleCharacterCount;
		state.visibleSpaceCount = m_lineVisibleSpaceCount;
		state.visible_LinkCount = m_textInfo.linkCount;
		state.firstCharacterIndex = m_firstCharacterOfLine;
		state.firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine;
		state.lastVisibleCharIndex = m_lastVisibleCharacterOfLine;
		state.fontStyle = m_FontStyleInternal;
		state.italicAngle = m_ItalicAngle;
		state.fontScaleMultiplier = m_fontScaleMultiplier;
		state.currentFontSize = m_currentFontSize;
		state.xAdvance = m_xAdvance;
		state.maxCapHeight = m_maxCapHeight;
		state.maxAscender = m_maxTextAscender;
		state.maxDescender = m_ElementDescender;
		state.startOfLineAscender = m_startOfLineAscender;
		state.maxLineAscender = m_maxLineAscender;
		state.maxLineDescender = m_maxLineDescender;
		state.pageAscender = m_PageAscender;
		state.preferredWidth = m_preferredWidth;
		state.preferredHeight = m_preferredHeight;
		state.renderedWidth = m_RenderedWidth;
		state.renderedHeight = m_RenderedHeight;
		state.meshExtents = m_meshExtents;
		state.lineNumber = m_lineNumber;
		state.lineOffset = m_lineOffset;
		state.baselineOffset = m_baselineOffset;
		state.isDrivenLineSpacing = m_IsDrivenLineSpacing;
		state.lastBaseGlyphIndex = m_LastBaseGlyphIndex;
		state.cSpace = m_cSpacing;
		state.mSpace = m_monoSpacing;
		state.horizontalAlignment = m_lineJustification;
		state.marginLeft = m_marginLeft;
		state.marginRight = m_marginRight;
		state.vertexColor = m_htmlColor;
		state.underlineColor = m_underlineColor;
		state.strikethroughColor = m_strikethroughColor;
		state.highlightState = m_HighlightState;
		state.isNonBreakingSpace = m_isNonBreakingSpace;
		state.tagNoParsing = tag_NoParsing;
		state.fxRotation = m_FXRotation;
		state.fxScale = m_FXScale;
		state.basicStyleStack = m_fontStyleStack;
		state.italicAngleStack = m_ItalicAngleStack;
		state.colorStack = m_colorStack;
		state.underlineColorStack = m_underlineColorStack;
		state.strikethroughColorStack = m_strikethroughColorStack;
		state.highlightStateStack = m_HighlightStateStack;
		state.colorGradientStack = m_colorGradientStack;
		state.sizeStack = m_sizeStack;
		state.indentStack = m_indentStack;
		state.fontWeightStack = m_FontWeightStack;
		state.baselineStack = m_baselineOffsetStack;
		state.actionStack = m_actionStack;
		state.materialReferenceStack = m_materialReferenceStack;
		state.lineJustificationStack = m_lineJustificationStack;
		state.spriteAnimationID = m_spriteAnimationID;
		if (m_lineNumber < m_textInfo.lineInfo.Length)
		{
			state.lineInfo = m_textInfo.lineInfo[m_lineNumber];
		}
	}

	internal int RestoreWordWrappingState(ref WordWrapState state)
	{
		int previous_WordBreak = state.previous_WordBreak;
		m_currentFontAsset = state.currentFontAsset;
		m_currentSpriteAsset = state.currentSpriteAsset;
		m_currentMaterial = state.currentMaterial;
		m_currentMaterialIndex = state.currentMaterialIndex;
		m_characterCount = state.total_CharacterCount + 1;
		m_lineVisibleCharacterCount = state.visible_CharacterCount;
		m_lineVisibleSpaceCount = state.visibleSpaceCount;
		m_textInfo.linkCount = state.visible_LinkCount;
		m_firstCharacterOfLine = state.firstCharacterIndex;
		m_firstVisibleCharacterOfLine = state.firstVisibleCharacterIndex;
		m_lastVisibleCharacterOfLine = state.lastVisibleCharIndex;
		m_FontStyleInternal = state.fontStyle;
		m_ItalicAngle = state.italicAngle;
		m_fontScaleMultiplier = state.fontScaleMultiplier;
		m_currentFontSize = state.currentFontSize;
		m_xAdvance = state.xAdvance;
		m_maxCapHeight = state.maxCapHeight;
		m_maxTextAscender = state.maxAscender;
		m_ElementDescender = state.maxDescender;
		m_startOfLineAscender = state.startOfLineAscender;
		m_maxLineAscender = state.maxLineAscender;
		m_maxLineDescender = state.maxLineDescender;
		m_PageAscender = state.pageAscender;
		m_preferredWidth = state.preferredWidth;
		m_preferredHeight = state.preferredHeight;
		m_RenderedWidth = state.renderedWidth;
		m_RenderedHeight = state.renderedHeight;
		m_meshExtents = state.meshExtents;
		m_lineNumber = state.lineNumber;
		m_lineOffset = state.lineOffset;
		m_baselineOffset = state.baselineOffset;
		m_IsDrivenLineSpacing = state.isDrivenLineSpacing;
		m_LastBaseGlyphIndex = state.lastBaseGlyphIndex;
		m_cSpacing = state.cSpace;
		m_monoSpacing = state.mSpace;
		m_lineJustification = state.horizontalAlignment;
		m_marginLeft = state.marginLeft;
		m_marginRight = state.marginRight;
		m_htmlColor = state.vertexColor;
		m_underlineColor = state.underlineColor;
		m_strikethroughColor = state.strikethroughColor;
		m_HighlightState = state.highlightState;
		m_isNonBreakingSpace = state.isNonBreakingSpace;
		tag_NoParsing = state.tagNoParsing;
		m_FXRotation = state.fxRotation;
		m_FXScale = state.fxScale;
		m_fontStyleStack = state.basicStyleStack;
		m_ItalicAngleStack = state.italicAngleStack;
		m_colorStack = state.colorStack;
		m_underlineColorStack = state.underlineColorStack;
		m_strikethroughColorStack = state.strikethroughColorStack;
		m_HighlightStateStack = state.highlightStateStack;
		m_colorGradientStack = state.colorGradientStack;
		m_sizeStack = state.sizeStack;
		m_indentStack = state.indentStack;
		m_FontWeightStack = state.fontWeightStack;
		m_baselineOffsetStack = state.baselineStack;
		m_actionStack = state.actionStack;
		m_materialReferenceStack = state.materialReferenceStack;
		m_lineJustificationStack = state.lineJustificationStack;
		m_spriteAnimationID = state.spriteAnimationID;
		if (m_lineNumber < m_textInfo.lineInfo.Length)
		{
			m_textInfo.lineInfo[m_lineNumber] = state.lineInfo;
		}
		return previous_WordBreak;
	}

	protected virtual void SaveGlyphVertexInfo(float padding, float style_padding, Color32 vertexColor)
	{
		m_textInfo.characterInfo[m_characterCount].vertex_BL.position = m_textInfo.characterInfo[m_characterCount].bottomLeft;
		m_textInfo.characterInfo[m_characterCount].vertex_TL.position = m_textInfo.characterInfo[m_characterCount].topLeft;
		m_textInfo.characterInfo[m_characterCount].vertex_TR.position = m_textInfo.characterInfo[m_characterCount].topRight;
		m_textInfo.characterInfo[m_characterCount].vertex_BR.position = m_textInfo.characterInfo[m_characterCount].bottomRight;
		vertexColor.a = ((m_fontColor32.a < vertexColor.a) ? m_fontColor32.a : vertexColor.a);
		bool flag = (m_currentFontAsset.m_AtlasRenderMode & (GlyphRenderMode)65536) == (GlyphRenderMode)65536;
		if (!m_enableVertexGradient || flag)
		{
			vertexColor = (flag ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, vertexColor.a) : vertexColor);
			m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
		}
		else if (!m_overrideHtmlColors && m_colorStack.index > 1)
		{
			m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
		}
		else if (m_fontColorGradientPreset != null)
		{
			m_textInfo.characterInfo[m_characterCount].vertex_BL.color = m_fontColorGradientPreset.bottomLeft * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.color = m_fontColorGradientPreset.topLeft * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.color = m_fontColorGradientPreset.topRight * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.color = m_fontColorGradientPreset.bottomRight * vertexColor;
		}
		else
		{
			m_textInfo.characterInfo[m_characterCount].vertex_BL.color = m_fontColorGradient.bottomLeft * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.color = m_fontColorGradient.topLeft * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.color = m_fontColorGradient.topRight * vertexColor;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.color = m_fontColorGradient.bottomRight * vertexColor;
		}
		if (m_colorGradientPreset != null && !flag)
		{
			if (m_colorGradientPresetIsTinted)
			{
				ref Color32 reference = ref m_textInfo.characterInfo[m_characterCount].vertex_BL.color;
				reference *= m_colorGradientPreset.bottomLeft;
				ref Color32 reference2 = ref m_textInfo.characterInfo[m_characterCount].vertex_TL.color;
				reference2 *= m_colorGradientPreset.topLeft;
				ref Color32 reference3 = ref m_textInfo.characterInfo[m_characterCount].vertex_TR.color;
				reference3 *= m_colorGradientPreset.topRight;
				ref Color32 reference4 = ref m_textInfo.characterInfo[m_characterCount].vertex_BR.color;
				reference4 *= m_colorGradientPreset.bottomRight;
			}
			else
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = m_colorGradientPreset.bottomLeft.MinAlpha(vertexColor);
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = m_colorGradientPreset.topLeft.MinAlpha(vertexColor);
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = m_colorGradientPreset.topRight.MinAlpha(vertexColor);
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = m_colorGradientPreset.bottomRight.MinAlpha(vertexColor);
			}
		}
		if (!m_isSDFShader)
		{
			style_padding = 0f;
		}
		GlyphRect glyphRect = m_textInfo.characterInfo[m_characterCount].alternativeGlyph?.glyphRect ?? m_cached_TextElement.m_Glyph.glyphRect;
		Vector2 vector = default(Vector2);
		vector.x = ((float)glyphRect.x - padding - style_padding) / (float)m_currentFontAsset.m_AtlasWidth;
		vector.y = ((float)glyphRect.y - padding - style_padding) / (float)m_currentFontAsset.m_AtlasHeight;
		Vector2 vector2 = default(Vector2);
		vector2.x = vector.x;
		vector2.y = ((float)glyphRect.y + padding + style_padding + (float)glyphRect.height) / (float)m_currentFontAsset.m_AtlasHeight;
		Vector2 vector3 = default(Vector2);
		vector3.x = ((float)glyphRect.x + padding + style_padding + (float)glyphRect.width) / (float)m_currentFontAsset.m_AtlasWidth;
		vector3.y = vector2.y;
		Vector2 vector4 = default(Vector2);
		vector4.x = vector3.x;
		vector4.y = vector.y;
		m_textInfo.characterInfo[m_characterCount].vertex_BL.uv = vector;
		m_textInfo.characterInfo[m_characterCount].vertex_TL.uv = vector2;
		m_textInfo.characterInfo[m_characterCount].vertex_TR.uv = vector3;
		m_textInfo.characterInfo[m_characterCount].vertex_BR.uv = vector4;
	}

	protected virtual void SaveSpriteVertexInfo(Color32 vertexColor)
	{
		m_textInfo.characterInfo[m_characterCount].vertex_BL.position = m_textInfo.characterInfo[m_characterCount].bottomLeft;
		m_textInfo.characterInfo[m_characterCount].vertex_TL.position = m_textInfo.characterInfo[m_characterCount].topLeft;
		m_textInfo.characterInfo[m_characterCount].vertex_TR.position = m_textInfo.characterInfo[m_characterCount].topRight;
		m_textInfo.characterInfo[m_characterCount].vertex_BR.position = m_textInfo.characterInfo[m_characterCount].bottomRight;
		if (m_tintAllSprites)
		{
			m_tintSprite = true;
		}
		Color32 color = (m_tintSprite ? m_spriteColor.Multiply(vertexColor) : m_spriteColor);
		color.a = ((color.a >= m_fontColor32.a) ? m_fontColor32.a : ((color.a < vertexColor.a) ? color.a : vertexColor.a));
		Color32 color2 = color;
		Color32 color3 = color;
		Color32 color4 = color;
		Color32 color5 = color;
		if (m_enableVertexGradient)
		{
			if (m_fontColorGradientPreset != null)
			{
				color2 = (m_tintSprite ? color2.Multiply(m_fontColorGradientPreset.bottomLeft) : color2);
				color3 = (m_tintSprite ? color3.Multiply(m_fontColorGradientPreset.topLeft) : color3);
				color4 = (m_tintSprite ? color4.Multiply(m_fontColorGradientPreset.topRight) : color4);
				color5 = (m_tintSprite ? color5.Multiply(m_fontColorGradientPreset.bottomRight) : color5);
			}
			else
			{
				color2 = (m_tintSprite ? color2.Multiply(m_fontColorGradient.bottomLeft) : color2);
				color3 = (m_tintSprite ? color3.Multiply(m_fontColorGradient.topLeft) : color3);
				color4 = (m_tintSprite ? color4.Multiply(m_fontColorGradient.topRight) : color4);
				color5 = (m_tintSprite ? color5.Multiply(m_fontColorGradient.bottomRight) : color5);
			}
		}
		if (m_colorGradientPreset != null)
		{
			color2 = (m_tintSprite ? color2.Multiply(m_colorGradientPreset.bottomLeft) : color2);
			color3 = (m_tintSprite ? color3.Multiply(m_colorGradientPreset.topLeft) : color3);
			color4 = (m_tintSprite ? color4.Multiply(m_colorGradientPreset.topRight) : color4);
			color5 = (m_tintSprite ? color5.Multiply(m_colorGradientPreset.bottomRight) : color5);
		}
		m_tintSprite = false;
		m_textInfo.characterInfo[m_characterCount].vertex_BL.color = color2;
		m_textInfo.characterInfo[m_characterCount].vertex_TL.color = color3;
		m_textInfo.characterInfo[m_characterCount].vertex_TR.color = color4;
		m_textInfo.characterInfo[m_characterCount].vertex_BR.color = color5;
		GlyphRect glyphRect = m_cached_TextElement.m_Glyph.glyphRect;
		Vector2 vector = new Vector2((float)glyphRect.x / (float)m_currentSpriteAsset.spriteSheet.width, (float)glyphRect.y / (float)m_currentSpriteAsset.spriteSheet.height);
		Vector2 vector2 = new Vector2(vector.x, (float)(glyphRect.y + glyphRect.height) / (float)m_currentSpriteAsset.spriteSheet.height);
		Vector2 vector3 = new Vector2((float)(glyphRect.x + glyphRect.width) / (float)m_currentSpriteAsset.spriteSheet.width, vector2.y);
		Vector2 vector4 = new Vector2(vector3.x, vector.y);
		m_textInfo.characterInfo[m_characterCount].vertex_BL.uv = vector;
		m_textInfo.characterInfo[m_characterCount].vertex_TL.uv = vector2;
		m_textInfo.characterInfo[m_characterCount].vertex_TR.uv = vector3;
		m_textInfo.characterInfo[m_characterCount].vertex_BR.uv = vector4;
	}

	protected virtual void FillCharacterVertexBuffers(int i)
	{
		int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
		int vertexCount = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
		if (vertexCount >= m_textInfo.meshInfo[materialReferenceIndex].vertices.Length)
		{
			m_textInfo.meshInfo[materialReferenceIndex].ResizeMeshInfo(Mathf.NextPowerOfTwo((vertexCount + 4) / 4));
		}
		TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
		m_textInfo.characterInfo[i].vertexIndex = vertexCount;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = characterInfo[i].vertex_BL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = characterInfo[i].vertex_TL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = characterInfo[i].vertex_TR.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = characterInfo[i].vertex_BR.position;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[vertexCount] = characterInfo[i].vertex_BL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + vertexCount] = characterInfo[i].vertex_TL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + vertexCount] = characterInfo[i].vertex_TR.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + vertexCount] = characterInfo[i].vertex_BR.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexCount] = characterInfo[i].vertex_BL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + vertexCount] = characterInfo[i].vertex_TL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + vertexCount] = characterInfo[i].vertex_TR.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + vertexCount] = characterInfo[i].vertex_BR.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].colors32[vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_BL.color.GammaToLinear() : characterInfo[i].vertex_BL.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_TL.color.GammaToLinear() : characterInfo[i].vertex_TL.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_TR.color.GammaToLinear() : characterInfo[i].vertex_TR.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_BR.color.GammaToLinear() : characterInfo[i].vertex_BR.color);
		m_textInfo.meshInfo[materialReferenceIndex].vertexCount = vertexCount + 4;
	}

	protected virtual void FillCharacterVertexBuffers(int i, bool isVolumetric)
	{
		int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
		int vertexCount = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
		if (vertexCount >= m_textInfo.meshInfo[materialReferenceIndex].vertices.Length)
		{
			m_textInfo.meshInfo[materialReferenceIndex].ResizeMeshInfo(Mathf.NextPowerOfTwo((vertexCount + (isVolumetric ? 8 : 4)) / 4));
		}
		TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
		m_textInfo.characterInfo[i].vertexIndex = vertexCount;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = characterInfo[i].vertex_BL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = characterInfo[i].vertex_TL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = characterInfo[i].vertex_TR.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = characterInfo[i].vertex_BR.position;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[vertexCount] = characterInfo[i].vertex_BL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + vertexCount] = characterInfo[i].vertex_TL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + vertexCount] = characterInfo[i].vertex_TR.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + vertexCount] = characterInfo[i].vertex_BR.uv;
		if (isVolumetric)
		{
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[4 + vertexCount] = characterInfo[i].vertex_BL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[5 + vertexCount] = characterInfo[i].vertex_TL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[6 + vertexCount] = characterInfo[i].vertex_TR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[7 + vertexCount] = characterInfo[i].vertex_BR.uv;
		}
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexCount] = characterInfo[i].vertex_BL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + vertexCount] = characterInfo[i].vertex_TL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + vertexCount] = characterInfo[i].vertex_TR.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + vertexCount] = characterInfo[i].vertex_BR.uv2;
		if (isVolumetric)
		{
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[4 + vertexCount] = characterInfo[i].vertex_BL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[5 + vertexCount] = characterInfo[i].vertex_TL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[6 + vertexCount] = characterInfo[i].vertex_TR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[7 + vertexCount] = characterInfo[i].vertex_BR.uv2;
		}
		m_textInfo.meshInfo[materialReferenceIndex].colors32[vertexCount] = characterInfo[i].vertex_BL.color;
		m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + vertexCount] = characterInfo[i].vertex_TL.color;
		m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + vertexCount] = characterInfo[i].vertex_TR.color;
		m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + vertexCount] = characterInfo[i].vertex_BR.color;
		if (isVolumetric)
		{
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, 128, byte.MaxValue);
			m_textInfo.meshInfo[materialReferenceIndex].colors32[4 + vertexCount] = color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[5 + vertexCount] = color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[6 + vertexCount] = color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[7 + vertexCount] = color;
		}
		m_textInfo.meshInfo[materialReferenceIndex].vertexCount = vertexCount + ((!isVolumetric) ? 4 : 8);
	}

	protected virtual void FillSpriteVertexBuffers(int i)
	{
		int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
		int vertexCount = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
		if (vertexCount >= m_textInfo.meshInfo[materialReferenceIndex].vertices.Length)
		{
			m_textInfo.meshInfo[materialReferenceIndex].ResizeMeshInfo(Mathf.NextPowerOfTwo((vertexCount + 4) / 4));
		}
		TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
		m_textInfo.characterInfo[i].vertexIndex = vertexCount;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = characterInfo[i].vertex_BL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = characterInfo[i].vertex_TL.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = characterInfo[i].vertex_TR.position;
		m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = characterInfo[i].vertex_BR.position;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[vertexCount] = characterInfo[i].vertex_BL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + vertexCount] = characterInfo[i].vertex_TL.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + vertexCount] = characterInfo[i].vertex_TR.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + vertexCount] = characterInfo[i].vertex_BR.uv;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexCount] = characterInfo[i].vertex_BL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + vertexCount] = characterInfo[i].vertex_TL.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + vertexCount] = characterInfo[i].vertex_TR.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + vertexCount] = characterInfo[i].vertex_BR.uv2;
		m_textInfo.meshInfo[materialReferenceIndex].colors32[vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_BL.color.GammaToLinear() : characterInfo[i].vertex_BL.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_TL.color.GammaToLinear() : characterInfo[i].vertex_TL.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_TR.color.GammaToLinear() : characterInfo[i].vertex_TR.color);
		m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + vertexCount] = (m_ConvertToLinearSpace ? characterInfo[i].vertex_BR.color.GammaToLinear() : characterInfo[i].vertex_BR.color);
		m_textInfo.meshInfo[materialReferenceIndex].vertexCount = vertexCount + 4;
	}

	protected virtual void DrawUnderlineMesh(Vector3 start, Vector3 end, ref int index, float startScale, float endScale, float maxScale, float sdfScale, Color32 underlineColor)
	{
		GetUnderlineSpecialCharacter(m_fontAsset);
		if (m_Underline.character == null)
		{
			if (!TMP_Settings.warningsDisabled)
			{
				UnityEngine.Debug.LogWarning("Unable to add underline or strikethrough since the character [0x5F] used by these features is not present in the Font Asset assigned to this text object.", this);
			}
			return;
		}
		int materialIndex = m_Underline.materialIndex;
		int num = index + 12;
		if (num > m_textInfo.meshInfo[materialIndex].vertices.Length)
		{
			m_textInfo.meshInfo[materialIndex].ResizeMeshInfo(num / 4);
		}
		start.y = Mathf.Min(start.y, end.y);
		end.y = Mathf.Min(start.y, end.y);
		GlyphMetrics metrics = m_Underline.character.glyph.metrics;
		GlyphRect glyphRect = m_Underline.character.glyph.glyphRect;
		float num2 = metrics.width / 2f * maxScale;
		if (end.x - start.x < metrics.width * maxScale)
		{
			num2 = (end.x - start.x) / 2f;
		}
		float num3 = m_padding * startScale / maxScale;
		float num4 = m_padding * endScale / maxScale;
		float underlineThickness = m_Underline.fontAsset.faceInfo.underlineThickness;
		Vector3[] vertices = m_textInfo.meshInfo[materialIndex].vertices;
		vertices[index] = start + new Vector3(0f, 0f - (underlineThickness + m_padding) * maxScale, 0f);
		vertices[index + 1] = start + new Vector3(0f, m_padding * maxScale, 0f);
		vertices[index + 2] = vertices[index + 1] + new Vector3(num2, 0f, 0f);
		vertices[index + 3] = vertices[index] + new Vector3(num2, 0f, 0f);
		vertices[index + 4] = vertices[index + 3];
		vertices[index + 5] = vertices[index + 2];
		vertices[index + 6] = end + new Vector3(0f - num2, m_padding * maxScale, 0f);
		vertices[index + 7] = end + new Vector3(0f - num2, (0f - (underlineThickness + m_padding)) * maxScale, 0f);
		vertices[index + 8] = vertices[index + 7];
		vertices[index + 9] = vertices[index + 6];
		vertices[index + 10] = end + new Vector3(0f, m_padding * maxScale, 0f);
		vertices[index + 11] = end + new Vector3(0f, (0f - (underlineThickness + m_padding)) * maxScale, 0f);
		Vector4[] uvs = m_textInfo.meshInfo[materialIndex].uvs0;
		int atlasWidth = m_Underline.fontAsset.atlasWidth;
		int atlasHeight = m_Underline.fontAsset.atlasHeight;
		float w = Mathf.Abs(sdfScale);
		Vector4 vector = new Vector4(((float)glyphRect.x - num3) / (float)atlasWidth, ((float)glyphRect.y - m_padding) / (float)atlasHeight, 0f, w);
		Vector4 vector2 = new Vector4(vector.x, ((float)(glyphRect.y + glyphRect.height) + m_padding) / (float)atlasHeight, 0f, w);
		Vector4 vector3 = new Vector4(((float)glyphRect.x - num3 + (float)glyphRect.width / 2f) / (float)atlasWidth, vector2.y, 0f, w);
		Vector4 vector4 = new Vector4(vector3.x, vector.y, 0f, w);
		Vector4 vector5 = new Vector4(((float)glyphRect.x + num4 + (float)glyphRect.width / 2f) / (float)atlasWidth, vector2.y, 0f, w);
		Vector4 vector6 = new Vector4(vector5.x, vector.y, 0f, w);
		Vector4 vector7 = new Vector4(((float)glyphRect.x + num4 + (float)glyphRect.width) / (float)atlasWidth, vector2.y, 0f, w);
		Vector4 vector8 = new Vector4(vector7.x, vector.y, 0f, w);
		uvs[index] = vector;
		uvs[1 + index] = vector2;
		uvs[2 + index] = vector3;
		uvs[3 + index] = vector4;
		uvs[4 + index] = new Vector4(vector3.x - vector3.x * 0.001f, vector.y, 0f, w);
		uvs[5 + index] = new Vector4(vector3.x - vector3.x * 0.001f, vector2.y, 0f, w);
		uvs[6 + index] = new Vector4(vector3.x + vector3.x * 0.001f, vector2.y, 0f, w);
		uvs[7 + index] = new Vector4(vector3.x + vector3.x * 0.001f, vector.y, 0f, w);
		uvs[8 + index] = vector6;
		uvs[9 + index] = vector5;
		uvs[10 + index] = vector7;
		uvs[11 + index] = vector8;
		float num5 = 0f;
		float x = (vertices[index + 2].x - start.x) / (end.x - start.x);
		Vector2[] uvs2 = m_textInfo.meshInfo[materialIndex].uvs2;
		uvs2[index] = new Vector2(0f, 0f);
		uvs2[1 + index] = new Vector2(0f, 1f);
		uvs2[2 + index] = new Vector2(x, 1f);
		uvs2[3 + index] = new Vector2(x, 0f);
		num5 = (vertices[index + 4].x - start.x) / (end.x - start.x);
		x = (vertices[index + 6].x - start.x) / (end.x - start.x);
		uvs2[4 + index] = new Vector2(num5, 0f);
		uvs2[5 + index] = new Vector2(num5, 1f);
		uvs2[6 + index] = new Vector2(x, 1f);
		uvs2[7 + index] = new Vector2(x, 0f);
		num5 = (vertices[index + 8].x - start.x) / (end.x - start.x);
		uvs2[8 + index] = new Vector2(num5, 0f);
		uvs2[9 + index] = new Vector2(num5, 1f);
		uvs2[10 + index] = new Vector2(1f, 1f);
		uvs2[11 + index] = new Vector2(1f, 0f);
		underlineColor.a = ((m_fontColor32.a < underlineColor.a) ? m_fontColor32.a : underlineColor.a);
		Color32[] colors = m_textInfo.meshInfo[materialIndex].colors32;
		colors[index] = underlineColor;
		colors[1 + index] = underlineColor;
		colors[2 + index] = underlineColor;
		colors[3 + index] = underlineColor;
		colors[4 + index] = underlineColor;
		colors[5 + index] = underlineColor;
		colors[6 + index] = underlineColor;
		colors[7 + index] = underlineColor;
		colors[8 + index] = underlineColor;
		colors[9 + index] = underlineColor;
		colors[10 + index] = underlineColor;
		colors[11 + index] = underlineColor;
		index += 12;
	}

	protected virtual void DrawTextHighlight(Vector3 start, Vector3 end, ref int index, Color32 highlightColor)
	{
		if (m_Underline.character == null)
		{
			GetUnderlineSpecialCharacter(m_fontAsset);
			if (m_Underline.character == null)
			{
				if (!TMP_Settings.warningsDisabled)
				{
					UnityEngine.Debug.LogWarning("Unable to add highlight since the primary Font Asset doesn't contain the underline character.", this);
				}
				return;
			}
		}
		int materialIndex = m_Underline.materialIndex;
		int num = index + 4;
		if (num > m_textInfo.meshInfo[materialIndex].vertices.Length)
		{
			m_textInfo.meshInfo[materialIndex].ResizeMeshInfo(num / 4);
		}
		Vector3[] vertices = m_textInfo.meshInfo[materialIndex].vertices;
		vertices[index] = start;
		vertices[index + 1] = new Vector3(start.x, end.y, 0f);
		vertices[index + 2] = end;
		vertices[index + 3] = new Vector3(end.x, start.y, 0f);
		Vector4[] uvs = m_textInfo.meshInfo[materialIndex].uvs0;
		int atlasWidth = m_Underline.fontAsset.atlasWidth;
		int atlasHeight = m_Underline.fontAsset.atlasHeight;
		GlyphRect glyphRect = m_Underline.character.glyph.glyphRect;
		Vector2 vector = new Vector2(((float)glyphRect.x + (float)glyphRect.width / 2f) / (float)atlasWidth, ((float)glyphRect.y + (float)glyphRect.height / 2f) / (float)atlasHeight);
		Vector2 vector2 = new Vector2(1f / (float)atlasWidth, 1f / (float)atlasHeight);
		uvs[index] = vector - vector2;
		uvs[index + 1] = vector + new Vector2(0f - vector2.x, vector2.y);
		uvs[index + 2] = vector + vector2;
		uvs[index + 3] = vector + new Vector2(vector2.x, 0f - vector2.y);
		Vector2[] uvs2 = m_textInfo.meshInfo[materialIndex].uvs2;
		Vector2 vector3 = new Vector2(0f, 1f);
		uvs2[index] = vector3;
		uvs2[index + 1] = vector3;
		uvs2[index + 2] = vector3;
		uvs2[index + 3] = vector3;
		highlightColor.a = ((m_fontColor32.a < highlightColor.a) ? m_fontColor32.a : highlightColor.a);
		Color32[] colors = m_textInfo.meshInfo[materialIndex].colors32;
		colors[index] = highlightColor;
		colors[index + 1] = highlightColor;
		colors[index + 2] = highlightColor;
		colors[index + 3] = highlightColor;
		index += 4;
	}

	protected void LoadDefaultSettings()
	{
		if (m_fontSize == -99f || m_isWaitingOnResourceLoad)
		{
			m_rectTransform = rectTransform;
			if (TMP_Settings.autoSizeTextContainer)
			{
				autoSizeTextContainer = true;
			}
			else if (GetType() == typeof(TextMeshPro))
			{
				if (m_rectTransform.sizeDelta == new Vector2(100f, 100f))
				{
					m_rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProTextContainerSize;
				}
			}
			else if (m_rectTransform.sizeDelta == new Vector2(100f, 100f))
			{
				m_rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProUITextContainerSize;
			}
			m_TextWrappingMode = TMP_Settings.textWrappingMode;
			m_ActiveFontFeatures = new List<OTL_FeatureTag>(TMP_Settings.fontFeatures);
			m_enableExtraPadding = TMP_Settings.enableExtraPadding;
			m_tintAllSprites = TMP_Settings.enableTintAllSprites;
			m_parseCtrlCharacters = TMP_Settings.enableParseEscapeCharacters;
			m_fontSize = (m_fontSizeBase = TMP_Settings.defaultFontSize);
			m_fontSizeMin = m_fontSize * TMP_Settings.defaultTextAutoSizingMinRatio;
			m_fontSizeMax = m_fontSize * TMP_Settings.defaultTextAutoSizingMaxRatio;
			m_isWaitingOnResourceLoad = false;
			raycastTarget = TMP_Settings.enableRaycastTarget;
			m_IsTextObjectScaleStatic = TMP_Settings.isTextObjectScaleStatic;
		}
		else
		{
			if (m_textAlignment < (TextAlignmentOptions)255)
			{
				m_textAlignment = TMP_Compatibility.ConvertTextAlignmentEnumValues(m_textAlignment);
			}
			if (m_ActiveFontFeatures.Count == 1 && m_ActiveFontFeatures[0] == (OTL_FeatureTag)0u)
			{
				m_ActiveFontFeatures.Clear();
				if (m_enableKerning)
				{
					m_ActiveFontFeatures.Add(OTL_FeatureTag.kern);
				}
			}
		}
		if (m_textAlignment != TextAlignmentOptions.Converted)
		{
			m_HorizontalAlignment = (HorizontalAlignmentOptions)(m_textAlignment & (TextAlignmentOptions)255);
			m_VerticalAlignment = (VerticalAlignmentOptions)(m_textAlignment & (TextAlignmentOptions)65280);
			m_textAlignment = TextAlignmentOptions.Converted;
		}
	}

	protected void GetSpecialCharacters(TMP_FontAsset fontAsset)
	{
		GetEllipsisSpecialCharacter(fontAsset);
		GetUnderlineSpecialCharacter(fontAsset);
	}

	protected void GetEllipsisSpecialCharacter(TMP_FontAsset fontAsset)
	{
		bool isAlternativeTypeface;
		TMP_Character tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(8230u, fontAsset, includeFallbacks: false, m_FontStyleInternal, m_FontWeightInternal, out isAlternativeTypeface);
		if (tMP_Character == null && fontAsset.m_FallbackFontAssetTable != null && fontAsset.m_FallbackFontAssetTable.Count > 0)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(8230u, fontAsset, fontAsset.m_FallbackFontAssetTable, includeFallbacks: true, m_FontStyleInternal, m_FontWeightInternal, out isAlternativeTypeface);
		}
		if (tMP_Character == null && TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(8230u, fontAsset, TMP_Settings.fallbackFontAssets, includeFallbacks: true, m_FontStyleInternal, m_FontWeightInternal, out isAlternativeTypeface);
		}
		if (tMP_Character == null && TMP_Settings.defaultFontAsset != null)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(8230u, TMP_Settings.defaultFontAsset, includeFallbacks: true, m_FontStyleInternal, m_FontWeightInternal, out isAlternativeTypeface);
		}
		if (tMP_Character != null)
		{
			m_Ellipsis = new SpecialCharacter(tMP_Character, 0);
		}
	}

	protected void GetUnderlineSpecialCharacter(TMP_FontAsset fontAsset)
	{
		bool isAlternativeTypeface;
		TMP_Character characterFromFontAsset = TMP_FontAssetUtilities.GetCharacterFromFontAsset(95u, fontAsset, includeFallbacks: false, FontStyles.Normal, FontWeight.Regular, out isAlternativeTypeface);
		if (characterFromFontAsset != null)
		{
			m_Underline = new SpecialCharacter(characterFromFontAsset, 0);
		}
	}

	protected void ReplaceTagWithCharacter(int[] chars, int insertionIndex, int tagLength, char c)
	{
		chars[insertionIndex] = c;
		for (int i = insertionIndex + tagLength; i < chars.Length; i++)
		{
			chars[i - 3] = chars[i];
		}
	}

	protected TMP_FontAsset GetFontAssetForWeight(int fontWeight)
	{
		bool num = (m_FontStyleInternal & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic;
		TMP_FontAsset tMP_FontAsset = null;
		int num2 = fontWeight / 100;
		if (num)
		{
			return m_currentFontAsset.fontWeightTable[num2].italicTypeface;
		}
		return m_currentFontAsset.fontWeightTable[num2].regularTypeface;
	}

	internal TMP_TextElement GetTextElement(uint unicode, TMP_FontAsset fontAsset, FontStyles fontStyle, FontWeight fontWeight, out bool isUsingAlternativeTypeface)
	{
		TMP_Character tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, fontAsset, includeFallbacks: true, fontStyle, fontWeight, out isUsingAlternativeTypeface);
		if (tMP_Character != null)
		{
			fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, fontStyle, fontWeight, isUsingAlternativeTypeface);
			return tMP_Character;
		}
		if (fontAsset.instanceID != m_fontAsset.instanceID)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, m_fontAsset, includeFallbacks: false, fontStyle, fontWeight, out isUsingAlternativeTypeface);
			if (tMP_Character != null)
			{
				fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, fontStyle, fontWeight, isUsingAlternativeTypeface);
				return tMP_Character;
			}
			if (m_fontAsset.m_FallbackFontAssetTable != null && m_fontAsset.m_FallbackFontAssetTable.Count > 0)
			{
				tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset, m_fontAsset.m_FallbackFontAssetTable, includeFallbacks: true, fontStyle, fontWeight, out isUsingAlternativeTypeface);
			}
			if (tMP_Character != null)
			{
				fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, fontStyle, fontWeight, isUsingAlternativeTypeface);
				return tMP_Character;
			}
		}
		if (fontStyle != 0 || fontWeight != FontWeight.Regular)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, fontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);
			if (tMP_Character != null)
			{
				fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, FontStyles.Normal, FontWeight.Regular, isUsingAlternativeTypeface);
				return tMP_Character;
			}
			if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
			{
				tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset, TMP_Settings.fallbackFontAssets, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);
			}
			if (tMP_Character != null)
			{
				fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, FontStyles.Normal, FontWeight.Regular, isUsingAlternativeTypeface);
				return tMP_Character;
			}
			if (TMP_Settings.defaultFontAsset != null)
			{
				tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, TMP_Settings.defaultFontAsset, includeFallbacks: true, FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);
			}
			if (tMP_Character != null)
			{
				fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, FontStyles.Normal, FontWeight.Regular, isUsingAlternativeTypeface);
				return tMP_Character;
			}
		}
		if (m_spriteAsset != null)
		{
			TMP_SpriteCharacter spriteCharacterFromSpriteAsset = TMP_FontAssetUtilities.GetSpriteCharacterFromSpriteAsset(unicode, m_spriteAsset, includeFallbacks: true);
			if (spriteCharacterFromSpriteAsset != null)
			{
				return spriteCharacterFromSpriteAsset;
			}
		}
		if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset, TMP_Settings.fallbackFontAssets, includeFallbacks: true, fontStyle, fontWeight, out isUsingAlternativeTypeface);
		}
		if (tMP_Character != null)
		{
			fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, fontStyle, fontWeight, isUsingAlternativeTypeface);
			return tMP_Character;
		}
		if (TMP_Settings.defaultFontAsset != null)
		{
			tMP_Character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, TMP_Settings.defaultFontAsset, includeFallbacks: true, fontStyle, fontWeight, out isUsingAlternativeTypeface);
		}
		if (tMP_Character != null)
		{
			fontAsset.AddCharacterToLookupCache(unicode, tMP_Character, fontStyle, fontWeight, isUsingAlternativeTypeface);
			return tMP_Character;
		}
		if (TMP_Settings.defaultSpriteAsset != null)
		{
			TMP_SpriteCharacter spriteCharacterFromSpriteAsset2 = TMP_FontAssetUtilities.GetSpriteCharacterFromSpriteAsset(unicode, TMP_Settings.defaultSpriteAsset, includeFallbacks: true);
			if (spriteCharacterFromSpriteAsset2 != null)
			{
				return spriteCharacterFromSpriteAsset2;
			}
		}
		return null;
	}

	protected virtual void SetActiveSubMeshes(bool state)
	{
	}

	protected virtual void DestroySubMeshObjects()
	{
	}

	public virtual void ClearMesh()
	{
	}

	public virtual void ClearMesh(bool uploadGeometry)
	{
	}

	public virtual string GetParsedText()
	{
		if (m_textInfo == null)
		{
			return string.Empty;
		}
		int characterCount = m_textInfo.characterCount;
		char[] array = new char[characterCount];
		for (int i = 0; i < characterCount && i < m_textInfo.characterInfo.Length; i++)
		{
			array[i] = m_textInfo.characterInfo[i].character;
		}
		return new string(array);
	}

	internal bool IsSelfOrLinkedAncestor(TMP_Text targetTextComponent)
	{
		if (targetTextComponent == null)
		{
			return true;
		}
		if (parentLinkedComponent != null && parentLinkedComponent.IsSelfOrLinkedAncestor(targetTextComponent))
		{
			return true;
		}
		if (GetInstanceID() == targetTextComponent.GetInstanceID())
		{
			return true;
		}
		return false;
	}

	internal void ReleaseLinkedTextComponent(TMP_Text targetTextComponent)
	{
		if (!(targetTextComponent == null))
		{
			TMP_Text tMP_Text = targetTextComponent.linkedTextComponent;
			if (tMP_Text != null)
			{
				ReleaseLinkedTextComponent(tMP_Text);
			}
			targetTextComponent.text = string.Empty;
			targetTextComponent.firstVisibleCharacter = 0;
			targetTextComponent.linkedTextComponent = null;
			targetTextComponent.parentLinkedComponent = null;
		}
	}

	protected void DoMissingGlyphCallback(int unicode, int stringIndex, TMP_FontAsset fontAsset)
	{
		TMP_Text.OnMissingCharacter?.Invoke(unicode, stringIndex, m_text, fontAsset, this);
	}

	protected Vector2 PackUV(float x, float y, float scale)
	{
		Vector2 result = default(Vector2);
		result.x = (int)(x * 511f);
		result.y = (int)(y * 511f);
		result.x = result.x * 4096f + result.y;
		result.y = scale;
		return result;
	}

	protected float PackUV(float x, float y)
	{
		double num = (int)(x * 511f);
		double num2 = (int)(y * 511f);
		return (float)(num * 4096.0 + num2);
	}

	internal virtual void InternalUpdate()
	{
	}

	protected uint HexToInt(char hex)
	{
		return hex switch
		{
			'0' => 0u, 
			'1' => 1u, 
			'2' => 2u, 
			'3' => 3u, 
			'4' => 4u, 
			'5' => 5u, 
			'6' => 6u, 
			'7' => 7u, 
			'8' => 8u, 
			'9' => 9u, 
			'A' => 10u, 
			'B' => 11u, 
			'C' => 12u, 
			'D' => 13u, 
			'E' => 14u, 
			'F' => 15u, 
			'a' => 10u, 
			'b' => 11u, 
			'c' => 12u, 
			'd' => 13u, 
			'e' => 14u, 
			'f' => 15u, 
			_ => 15u, 
		};
	}

	private bool IsValidUTF16(TextBackingContainer text, int index)
	{
		for (int i = 0; i < 4; i++)
		{
			uint num = text[index + i];
			if ((num < 48 || num > 57) && (num < 97 || num > 102) && (num < 65 || num > 70))
			{
				return false;
			}
		}
		return true;
	}

	private uint GetUTF16(uint[] text, int i)
	{
		return 0 + (HexToInt((char)text[i]) << 12) + (HexToInt((char)text[i + 1]) << 8) + (HexToInt((char)text[i + 2]) << 4) + HexToInt((char)text[i + 3]);
	}

	private uint GetUTF16(TextBackingContainer text, int i)
	{
		return 0 + (HexToInt((char)text[i]) << 12) + (HexToInt((char)text[i + 1]) << 8) + (HexToInt((char)text[i + 2]) << 4) + HexToInt((char)text[i + 3]);
	}

	private bool IsValidUTF32(TextBackingContainer text, int index)
	{
		for (int i = 0; i < 8; i++)
		{
			uint num = text[index + i];
			if ((num < 48 || num > 57) && (num < 97 || num > 102) && (num < 65 || num > 70))
			{
				return false;
			}
		}
		return true;
	}

	private uint GetUTF32(uint[] text, int i)
	{
		return 0 + (HexToInt((char)text[i]) << 28) + (HexToInt((char)text[i + 1]) << 24) + (HexToInt((char)text[i + 2]) << 20) + (HexToInt((char)text[i + 3]) << 16) + (HexToInt((char)text[i + 4]) << 12) + (HexToInt((char)text[i + 5]) << 8) + (HexToInt((char)text[i + 6]) << 4) + HexToInt((char)text[i + 7]);
	}

	private uint GetUTF32(TextBackingContainer text, int i)
	{
		return 0 + (HexToInt((char)text[i]) << 28) + (HexToInt((char)text[i + 1]) << 24) + (HexToInt((char)text[i + 2]) << 20) + (HexToInt((char)text[i + 3]) << 16) + (HexToInt((char)text[i + 4]) << 12) + (HexToInt((char)text[i + 5]) << 8) + (HexToInt((char)text[i + 6]) << 4) + HexToInt((char)text[i + 7]);
	}

	protected Color32 HexCharsToColor(char[] hexChars, int tagCount)
	{
		switch (tagCount)
		{
		case 4:
		{
			byte r8 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
			byte g8 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
			byte b8 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
			return new Color32(r8, g8, b8, byte.MaxValue);
		}
		case 5:
		{
			byte r7 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
			byte g7 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
			byte b7 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
			byte a4 = (byte)(HexToInt(hexChars[4]) * 16 + HexToInt(hexChars[4]));
			return new Color32(r7, g7, b7, a4);
		}
		case 7:
		{
			byte r6 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
			byte g6 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
			byte b6 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
			return new Color32(r6, g6, b6, byte.MaxValue);
		}
		case 9:
		{
			byte r5 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
			byte g5 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
			byte b5 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
			byte a3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			return new Color32(r5, g5, b5, a3);
		}
		case 10:
		{
			byte r4 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
			byte g4 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
			byte b4 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
			return new Color32(r4, g4, b4, byte.MaxValue);
		}
		case 11:
		{
			byte r3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
			byte g3 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
			byte b3 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
			byte a2 = (byte)(HexToInt(hexChars[10]) * 16 + HexToInt(hexChars[10]));
			return new Color32(r3, g3, b3, a2);
		}
		case 13:
		{
			byte r2 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			byte g2 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
			byte b2 = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
			return new Color32(r2, g2, b2, byte.MaxValue);
		}
		case 15:
		{
			byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			byte g = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
			byte b = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
			byte a = (byte)(HexToInt(hexChars[13]) * 16 + HexToInt(hexChars[14]));
			return new Color32(r, g, b, a);
		}
		default:
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
	}

	protected Color32 HexCharsToColor(char[] hexChars, int startIndex, int length)
	{
		switch (length)
		{
		case 7:
		{
			byte r2 = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
			byte g2 = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
			byte b2 = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
			return new Color32(r2, g2, b2, byte.MaxValue);
		}
		case 9:
		{
			byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
			byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
			byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
			byte a = (byte)(HexToInt(hexChars[startIndex + 7]) * 16 + HexToInt(hexChars[startIndex + 8]));
			return new Color32(r, g, b, a);
		}
		default:
			return s_colorWhite;
		}
	}

	private int GetAttributeParameters(char[] chars, int startIndex, int length, ref float[] parameters)
	{
		int lastIndex = startIndex;
		int num = 0;
		while (lastIndex < startIndex + length)
		{
			parameters[num] = ConvertToFloat(chars, startIndex, length, out lastIndex);
			length -= lastIndex - startIndex + 1;
			startIndex = lastIndex + 1;
			num++;
		}
		return num;
	}

	protected float ConvertToFloat(char[] chars, int startIndex, int length)
	{
		int lastIndex;
		return ConvertToFloat(chars, startIndex, length, out lastIndex);
	}

	protected float ConvertToFloat(char[] chars, int startIndex, int length, out int lastIndex)
	{
		if (startIndex == 0)
		{
			lastIndex = 0;
			return -32768f;
		}
		int num = startIndex + length;
		bool flag = true;
		float num2 = 0f;
		int num3 = 1;
		if (chars[startIndex] == '+')
		{
			num3 = 1;
			startIndex++;
		}
		else if (chars[startIndex] == '-')
		{
			num3 = -1;
			startIndex++;
		}
		float num4 = 0f;
		for (int i = startIndex; i < num; i++)
		{
			uint num5 = chars[i];
			if (num5 < 48 || num5 > 57)
			{
				switch (num5)
				{
				case 46u:
					break;
				case 44u:
					if (i + 1 < num && chars[i + 1] == ' ')
					{
						lastIndex = i + 1;
					}
					else
					{
						lastIndex = i;
					}
					if (num4 > 32767f)
					{
						return -32768f;
					}
					return num4;
				default:
					continue;
				}
			}
			if (num5 == 46)
			{
				flag = false;
				num2 = 0.1f;
			}
			else if (flag)
			{
				num4 = num4 * 10f + (float)((num5 - 48) * num3);
			}
			else
			{
				num4 += (float)(num5 - 48) * num2 * (float)num3;
				num2 *= 0.1f;
			}
		}
		lastIndex = num;
		if (num4 > 32767f)
		{
			return -32768f;
		}
		return num4;
	}

	private void ClearMarkupTagAttributes()
	{
		int num = m_xmlAttribute.Length;
		for (int i = 0; i < num; i++)
		{
			m_xmlAttribute[i] = default(RichTextTagAttribute);
		}
	}

	internal bool ValidateHtmlTag(TextProcessingElement[] chars, int startIndex, out int endIndex)
	{
		int num = 0;
		byte b = 0;
		int num2 = 0;
		ClearMarkupTagAttributes();
		TagValueType tagValueType = TagValueType.None;
		TagUnitType tagUnitType = TagUnitType.Pixels;
		endIndex = startIndex;
		bool flag = false;
		bool flag2 = false;
		for (int i = startIndex; i < chars.Length && chars[i].unicode != 0; i++)
		{
			if (num >= m_htmlTag.Length)
			{
				break;
			}
			if (chars[i].unicode == 60)
			{
				break;
			}
			uint unicode = chars[i].unicode;
			if (unicode == 62)
			{
				flag2 = true;
				endIndex = i;
				m_htmlTag[num] = '\0';
				break;
			}
			m_htmlTag[num] = (char)unicode;
			num++;
			if (b == 1)
			{
				switch (tagValueType)
				{
				case TagValueType.None:
					switch (unicode)
					{
					case 43u:
					case 45u:
					case 46u:
					case 48u:
					case 49u:
					case 50u:
					case 51u:
					case 52u:
					case 53u:
					case 54u:
					case 55u:
					case 56u:
					case 57u:
						tagUnitType = TagUnitType.Pixels;
						tagValueType = (m_xmlAttribute[num2].valueType = TagValueType.NumericalValue);
						m_xmlAttribute[num2].valueStartIndex = num - 1;
						m_xmlAttribute[num2].valueLength++;
						break;
					default:
						switch (unicode)
						{
						case 35u:
							tagUnitType = TagUnitType.Pixels;
							tagValueType = (m_xmlAttribute[num2].valueType = TagValueType.ColorValue);
							m_xmlAttribute[num2].valueStartIndex = num - 1;
							m_xmlAttribute[num2].valueLength++;
							break;
						case 34u:
							tagUnitType = TagUnitType.Pixels;
							tagValueType = (m_xmlAttribute[num2].valueType = TagValueType.StringValue);
							m_xmlAttribute[num2].valueStartIndex = num;
							break;
						default:
							tagUnitType = TagUnitType.Pixels;
							tagValueType = (m_xmlAttribute[num2].valueType = TagValueType.StringValue);
							m_xmlAttribute[num2].valueStartIndex = num - 1;
							m_xmlAttribute[num2].valueHashCode = ((m_xmlAttribute[num2].valueHashCode << 5) + m_xmlAttribute[num2].valueHashCode) ^ TMP_TextUtilities.ToUpperFast((char)unicode);
							m_xmlAttribute[num2].valueLength++;
							break;
						}
						break;
					}
					break;
				case TagValueType.NumericalValue:
					if (unicode == 112 || unicode == 101 || unicode == 37 || unicode == 32)
					{
						b = 2;
						tagValueType = TagValueType.None;
						tagUnitType = unicode switch
						{
							101u => m_xmlAttribute[num2].unitType = TagUnitType.FontUnits, 
							37u => m_xmlAttribute[num2].unitType = TagUnitType.Percentage, 
							_ => m_xmlAttribute[num2].unitType = TagUnitType.Pixels, 
						};
						num2++;
						m_xmlAttribute[num2].nameHashCode = 0;
						m_xmlAttribute[num2].valueHashCode = 0;
						m_xmlAttribute[num2].valueType = TagValueType.None;
						m_xmlAttribute[num2].unitType = TagUnitType.Pixels;
						m_xmlAttribute[num2].valueStartIndex = 0;
						m_xmlAttribute[num2].valueLength = 0;
					}
					else
					{
						m_xmlAttribute[num2].valueLength++;
					}
					break;
				case TagValueType.ColorValue:
					if (unicode != 32)
					{
						m_xmlAttribute[num2].valueLength++;
						break;
					}
					b = 2;
					tagValueType = TagValueType.None;
					tagUnitType = TagUnitType.Pixels;
					num2++;
					m_xmlAttribute[num2].nameHashCode = 0;
					m_xmlAttribute[num2].valueType = TagValueType.None;
					m_xmlAttribute[num2].unitType = TagUnitType.Pixels;
					m_xmlAttribute[num2].valueHashCode = 0;
					m_xmlAttribute[num2].valueStartIndex = 0;
					m_xmlAttribute[num2].valueLength = 0;
					break;
				case TagValueType.StringValue:
					if (unicode != 34)
					{
						m_xmlAttribute[num2].valueHashCode = ((m_xmlAttribute[num2].valueHashCode << 5) + m_xmlAttribute[num2].valueHashCode) ^ TMP_TextUtilities.ToUpperFast((char)unicode);
						m_xmlAttribute[num2].valueLength++;
						break;
					}
					b = 2;
					tagValueType = TagValueType.None;
					tagUnitType = TagUnitType.Pixels;
					num2++;
					m_xmlAttribute[num2].nameHashCode = 0;
					m_xmlAttribute[num2].valueType = TagValueType.None;
					m_xmlAttribute[num2].unitType = TagUnitType.Pixels;
					m_xmlAttribute[num2].valueHashCode = 0;
					m_xmlAttribute[num2].valueStartIndex = 0;
					m_xmlAttribute[num2].valueLength = 0;
					break;
				}
			}
			if (unicode == 61)
			{
				b = 1;
			}
			if (b == 0 && unicode == 32)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
				b = 2;
				tagValueType = TagValueType.None;
				tagUnitType = TagUnitType.Pixels;
				num2++;
				m_xmlAttribute[num2].nameHashCode = 0;
				m_xmlAttribute[num2].valueType = TagValueType.None;
				m_xmlAttribute[num2].unitType = TagUnitType.Pixels;
				m_xmlAttribute[num2].valueHashCode = 0;
				m_xmlAttribute[num2].valueStartIndex = 0;
				m_xmlAttribute[num2].valueLength = 0;
			}
			if (b == 0)
			{
				m_xmlAttribute[num2].nameHashCode = ((m_xmlAttribute[num2].nameHashCode << 5) + m_xmlAttribute[num2].nameHashCode) ^ TMP_TextUtilities.ToUpperFast((char)unicode);
			}
			if (b == 2 && unicode == 32)
			{
				b = 0;
			}
		}
		if (!flag2)
		{
			return false;
		}
		if (tag_NoParsing && m_xmlAttribute[0].nameHashCode != -294095813)
		{
			return false;
		}
		if (m_xmlAttribute[0].nameHashCode == -294095813)
		{
			tag_NoParsing = false;
			return true;
		}
		if (m_htmlTag[0] == '#' && num == 4)
		{
			m_htmlColor = HexCharsToColor(m_htmlTag, num);
			m_colorStack.Add(m_htmlColor);
			return true;
		}
		if (m_htmlTag[0] == '#' && num == 5)
		{
			m_htmlColor = HexCharsToColor(m_htmlTag, num);
			m_colorStack.Add(m_htmlColor);
			return true;
		}
		if (m_htmlTag[0] == '#' && num == 7)
		{
			m_htmlColor = HexCharsToColor(m_htmlTag, num);
			m_colorStack.Add(m_htmlColor);
			return true;
		}
		if (m_htmlTag[0] == '#' && num == 9)
		{
			m_htmlColor = HexCharsToColor(m_htmlTag, num);
			m_colorStack.Add(m_htmlColor);
			return true;
		}
		float num3 = 0f;
		Material currentMaterial;
		switch ((MarkupTag)m_xmlAttribute[0].nameHashCode)
		{
		case MarkupTag.BOLD:
			m_FontStyleInternal |= FontStyles.Bold;
			m_fontStyleStack.Add(FontStyles.Bold);
			m_FontWeightInternal = FontWeight.Bold;
			return true;
		case MarkupTag.SLASH_BOLD:
			if ((m_fontStyle & FontStyles.Bold) != FontStyles.Bold && m_fontStyleStack.Remove(FontStyles.Bold) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.Bold;
				m_FontWeightInternal = m_FontWeightStack.Peek();
			}
			return true;
		case MarkupTag.ITALIC:
			m_FontStyleInternal |= FontStyles.Italic;
			m_fontStyleStack.Add(FontStyles.Italic);
			if (m_xmlAttribute[1].nameHashCode == 75347905)
			{
				m_ItalicAngle = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
				if (m_ItalicAngle < -180 || m_ItalicAngle > 180)
				{
					return false;
				}
			}
			else
			{
				m_ItalicAngle = m_currentFontAsset.italicStyle;
			}
			m_ItalicAngleStack.Add(m_ItalicAngle);
			return true;
		case MarkupTag.SLASH_ITALIC:
			if ((m_fontStyle & FontStyles.Italic) != FontStyles.Italic)
			{
				m_ItalicAngle = m_ItalicAngleStack.Remove();
				if (m_fontStyleStack.Remove(FontStyles.Italic) == 0)
				{
					m_FontStyleInternal &= ~FontStyles.Italic;
				}
			}
			return true;
		case MarkupTag.STRIKETHROUGH:
			m_FontStyleInternal |= FontStyles.Strikethrough;
			m_fontStyleStack.Add(FontStyles.Strikethrough);
			if (m_xmlAttribute[1].nameHashCode == 81999901)
			{
				m_strikethroughColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
				m_strikethroughColor.a = ((m_htmlColor.a < m_strikethroughColor.a) ? m_htmlColor.a : m_strikethroughColor.a);
			}
			else
			{
				m_strikethroughColor = m_htmlColor;
			}
			m_strikethroughColorStack.Add(m_strikethroughColor);
			return true;
		case MarkupTag.SLASH_STRIKETHROUGH:
			if ((m_fontStyle & FontStyles.Strikethrough) != FontStyles.Strikethrough && m_fontStyleStack.Remove(FontStyles.Strikethrough) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.Strikethrough;
			}
			m_strikethroughColor = m_strikethroughColorStack.Remove();
			return true;
		case MarkupTag.UNDERLINE:
			m_FontStyleInternal |= FontStyles.Underline;
			m_fontStyleStack.Add(FontStyles.Underline);
			if (m_xmlAttribute[1].nameHashCode == 81999901)
			{
				m_underlineColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
				m_underlineColor.a = ((m_htmlColor.a < m_underlineColor.a) ? m_htmlColor.a : m_underlineColor.a);
			}
			else
			{
				m_underlineColor = m_htmlColor;
			}
			m_underlineColorStack.Add(m_underlineColor);
			return true;
		case MarkupTag.SLASH_UNDERLINE:
			if ((m_fontStyle & FontStyles.Underline) != FontStyles.Underline && m_fontStyleStack.Remove(FontStyles.Underline) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.Underline;
			}
			m_underlineColor = m_underlineColorStack.Remove();
			return true;
		case MarkupTag.MARK:
		{
			m_FontStyleInternal |= FontStyles.Highlight;
			m_fontStyleStack.Add(FontStyles.Highlight);
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, 0, 64);
			TMP_Offset padding = TMP_Offset.zero;
			for (int j = 0; j < m_xmlAttribute.Length && m_xmlAttribute[j].nameHashCode != 0; j++)
			{
				switch ((MarkupTag)m_xmlAttribute[j].nameHashCode)
				{
				case MarkupTag.MARK:
					if (m_xmlAttribute[j].valueType == TagValueType.ColorValue)
					{
						color = HexCharsToColor(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
					}
					break;
				case MarkupTag.COLOR:
					color = HexCharsToColor(m_htmlTag, m_xmlAttribute[j].valueStartIndex, m_xmlAttribute[j].valueLength);
					break;
				case MarkupTag.PADDING:
					if (GetAttributeParameters(m_htmlTag, m_xmlAttribute[j].valueStartIndex, m_xmlAttribute[j].valueLength, ref m_attributeParameterValues) != 4)
					{
						return false;
					}
					padding = new TMP_Offset(m_attributeParameterValues[0], m_attributeParameterValues[1], m_attributeParameterValues[2], m_attributeParameterValues[3]);
					padding *= m_fontSize * 0.01f * (m_isOrthographic ? 1f : 0.1f);
					break;
				}
			}
			color.a = ((m_htmlColor.a < color.a) ? m_htmlColor.a : color.a);
			m_HighlightState = new HighlightState(color, padding);
			m_HighlightStateStack.Push(m_HighlightState);
			return true;
		}
		case MarkupTag.SLASH_MARK:
			if ((m_fontStyle & FontStyles.Highlight) != FontStyles.Highlight)
			{
				m_HighlightStateStack.Remove();
				m_HighlightState = m_HighlightStateStack.current;
				if (m_fontStyleStack.Remove(FontStyles.Highlight) == 0)
				{
					m_FontStyleInternal &= ~FontStyles.Highlight;
				}
			}
			return true;
		case MarkupTag.SUBSCRIPT:
		{
			m_fontScaleMultiplier *= ((m_currentFontAsset.faceInfo.subscriptSize > 0f) ? m_currentFontAsset.faceInfo.subscriptSize : 1f);
			m_baselineOffsetStack.Push(m_baselineOffset);
			m_materialReferenceStack.Push(m_materialReferences[m_currentMaterialIndex]);
			float num4 = m_currentFontSize / m_currentFontAsset.faceInfo.pointSize * m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1f : 0.1f);
			m_baselineOffset += m_currentFontAsset.faceInfo.subscriptOffset * num4 * m_fontScaleMultiplier;
			m_fontStyleStack.Add(FontStyles.Subscript);
			m_FontStyleInternal |= FontStyles.Subscript;
			return true;
		}
		case MarkupTag.SLASH_SUBSCRIPT:
			if ((m_FontStyleInternal & FontStyles.Subscript) == FontStyles.Subscript)
			{
				TMP_FontAsset fontAsset = m_materialReferenceStack.Pop().fontAsset;
				if (m_fontScaleMultiplier < 1f)
				{
					m_baselineOffset = m_baselineOffsetStack.Pop();
					m_fontScaleMultiplier /= ((fontAsset.faceInfo.subscriptSize > 0f) ? fontAsset.faceInfo.subscriptSize : 1f);
				}
				if (m_fontStyleStack.Remove(FontStyles.Subscript) == 0)
				{
					m_FontStyleInternal &= ~FontStyles.Subscript;
				}
			}
			return true;
		case MarkupTag.SUPERSCRIPT:
		{
			m_fontScaleMultiplier *= ((m_currentFontAsset.faceInfo.superscriptSize > 0f) ? m_currentFontAsset.faceInfo.superscriptSize : 1f);
			m_baselineOffsetStack.Push(m_baselineOffset);
			m_materialReferenceStack.Push(m_materialReferences[m_currentMaterialIndex]);
			float num4 = m_currentFontSize / m_currentFontAsset.faceInfo.pointSize * m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1f : 0.1f);
			m_baselineOffset += m_currentFontAsset.faceInfo.superscriptOffset * num4 * m_fontScaleMultiplier;
			m_fontStyleStack.Add(FontStyles.Superscript);
			m_FontStyleInternal |= FontStyles.Superscript;
			return true;
		}
		case MarkupTag.SLASH_SUPERSCRIPT:
			if ((m_FontStyleInternal & FontStyles.Superscript) == FontStyles.Superscript)
			{
				TMP_FontAsset fontAsset2 = m_materialReferenceStack.Pop().fontAsset;
				if (m_fontScaleMultiplier < 1f)
				{
					m_baselineOffset = m_baselineOffsetStack.Pop();
					m_fontScaleMultiplier /= ((fontAsset2.faceInfo.superscriptSize > 0f) ? fontAsset2.faceInfo.superscriptSize : 1f);
				}
				if (m_fontStyleStack.Remove(FontStyles.Superscript) == 0)
				{
					m_FontStyleInternal &= ~FontStyles.Superscript;
				}
			}
			return true;
		case MarkupTag.FONT_WEIGHT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch ((int)num3)
			{
			case 100:
				m_FontWeightInternal = FontWeight.Thin;
				break;
			case 200:
				m_FontWeightInternal = FontWeight.ExtraLight;
				break;
			case 300:
				m_FontWeightInternal = FontWeight.Light;
				break;
			case 400:
				m_FontWeightInternal = FontWeight.Regular;
				break;
			case 500:
				m_FontWeightInternal = FontWeight.Medium;
				break;
			case 600:
				m_FontWeightInternal = FontWeight.SemiBold;
				break;
			case 700:
				m_FontWeightInternal = FontWeight.Bold;
				break;
			case 800:
				m_FontWeightInternal = FontWeight.Heavy;
				break;
			case 900:
				m_FontWeightInternal = FontWeight.Black;
				break;
			}
			m_FontWeightStack.Add(m_FontWeightInternal);
			return true;
		case MarkupTag.SLASH_FONT_WEIGHT:
			m_FontWeightStack.Remove();
			if (m_FontStyleInternal == FontStyles.Bold)
			{
				m_FontWeightInternal = FontWeight.Bold;
			}
			else
			{
				m_FontWeightInternal = m_FontWeightStack.Peek();
			}
			return true;
		case MarkupTag.POSITION:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_xAdvance = num3 * (m_isOrthographic ? 1f : 0.1f);
				return true;
			case TagUnitType.FontUnits:
				m_xAdvance = num3 * m_currentFontSize * (m_isOrthographic ? 1f : 0.1f);
				return true;
			case TagUnitType.Percentage:
				m_xAdvance = m_marginWidth * num3 / 100f;
				return true;
			default:
				return false;
			}
		case MarkupTag.SLASH_POSITION:
			m_isIgnoringAlignment = false;
			return true;
		case MarkupTag.VERTICAL_OFFSET:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_baselineOffset = num3 * (m_isOrthographic ? 1f : 0.1f);
				return true;
			case TagUnitType.FontUnits:
				m_baselineOffset = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				return true;
			case TagUnitType.Percentage:
				return false;
			default:
				return false;
			}
		case MarkupTag.SLASH_VERTICAL_OFFSET:
			m_baselineOffset = 0f;
			return true;
		case MarkupTag.PAGE:
			if (m_overflowMode == TextOverflowModes.Page)
			{
				m_xAdvance = 0f + tag_LineIndent + tag_Indent;
				m_lineOffset = 0f;
				m_pageNumber++;
				m_isNewPage = true;
			}
			return true;
		case MarkupTag.NO_BREAK:
			m_isNonBreakingSpace = true;
			return true;
		case MarkupTag.SLASH_NO_BREAK:
			m_isNonBreakingSpace = false;
			return true;
		case MarkupTag.SIZE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				if (m_htmlTag[5] == '+')
				{
					m_currentFontSize = m_fontSize + num3;
					m_sizeStack.Add(m_currentFontSize);
					return true;
				}
				if (m_htmlTag[5] == '-')
				{
					m_currentFontSize = m_fontSize + num3;
					m_sizeStack.Add(m_currentFontSize);
					return true;
				}
				m_currentFontSize = num3;
				m_sizeStack.Add(m_currentFontSize);
				return true;
			case TagUnitType.FontUnits:
				m_currentFontSize = m_fontSize * num3;
				m_sizeStack.Add(m_currentFontSize);
				return true;
			case TagUnitType.Percentage:
				m_currentFontSize = m_fontSize * num3 / 100f;
				m_sizeStack.Add(m_currentFontSize);
				return true;
			default:
				return false;
			}
		case MarkupTag.SLASH_SIZE:
			m_currentFontSize = m_sizeStack.Remove();
			return true;
		case MarkupTag.FONT:
		{
			int valueHashCode3 = m_xmlAttribute[0].valueHashCode;
			int nameHashCode = m_xmlAttribute[1].nameHashCode;
			int valueHashCode2 = m_xmlAttribute[1].valueHashCode;
			if (valueHashCode3 == -620974005)
			{
				m_currentFontAsset = m_materialReferences[0].fontAsset;
				m_currentMaterial = m_materialReferences[0].material;
				m_currentMaterialIndex = 0;
				m_materialReferenceStack.Add(m_materialReferences[0]);
				return true;
			}
			MaterialReferenceManager.TryGetFontAsset(valueHashCode3, out var fontAsset3);
			if (fontAsset3 == null)
			{
				fontAsset3 = TMP_Text.OnFontAssetRequest?.Invoke(valueHashCode3, new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
				if (fontAsset3 == null)
				{
					fontAsset3 = Resources.Load<TMP_FontAsset>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
				}
				if (fontAsset3 == null)
				{
					return false;
				}
				MaterialReferenceManager.AddFontAsset(fontAsset3);
			}
			if (nameHashCode == 0 && valueHashCode2 == 0)
			{
				m_currentMaterial = fontAsset3.material;
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset3, ref m_materialReferences, m_materialReferenceIndexLookup);
				m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
			}
			else
			{
				if (nameHashCode != 825491659)
				{
					return false;
				}
				if (MaterialReferenceManager.TryGetMaterial(valueHashCode2, out currentMaterial))
				{
					m_currentMaterial = currentMaterial;
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset3, ref m_materialReferences, m_materialReferenceIndexLookup);
					m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
				}
				else
				{
					currentMaterial = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength));
					if (currentMaterial == null)
					{
						return false;
					}
					MaterialReferenceManager.AddFontMaterial(valueHashCode2, currentMaterial);
					m_currentMaterial = currentMaterial;
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset3, ref m_materialReferences, m_materialReferenceIndexLookup);
					m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
				}
			}
			m_currentFontAsset = fontAsset3;
			return true;
		}
		case MarkupTag.SLASH_FONT:
		{
			MaterialReference materialReference = m_materialReferenceStack.Remove();
			m_currentFontAsset = materialReference.fontAsset;
			m_currentMaterial = materialReference.material;
			m_currentMaterialIndex = materialReference.index;
			return true;
		}
		case MarkupTag.MATERIAL:
		{
			int valueHashCode2 = m_xmlAttribute[0].valueHashCode;
			if (valueHashCode2 == -620974005)
			{
				m_currentMaterial = m_materialReferences[0].material;
				m_currentMaterialIndex = 0;
				m_materialReferenceStack.Add(m_materialReferences[0]);
				return true;
			}
			if (MaterialReferenceManager.TryGetMaterial(valueHashCode2, out currentMaterial))
			{
				m_currentMaterial = currentMaterial;
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
				m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
			}
			else
			{
				currentMaterial = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
				if (currentMaterial == null)
				{
					return false;
				}
				MaterialReferenceManager.AddFontMaterial(valueHashCode2, currentMaterial);
				m_currentMaterial = currentMaterial;
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
				m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
			}
			return true;
		}
		case MarkupTag.SLASH_MATERIAL:
		{
			MaterialReference materialReference2 = m_materialReferenceStack.Remove();
			m_currentMaterial = materialReference2.material;
			m_currentMaterialIndex = materialReference2.index;
			return true;
		}
		case MarkupTag.SPACE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_xAdvance += num3 * (m_isOrthographic ? 1f : 0.1f);
				return true;
			case TagUnitType.FontUnits:
				m_xAdvance += num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				return true;
			case TagUnitType.Percentage:
				return false;
			default:
				return false;
			}
		case MarkupTag.ALPHA:
			if (m_xmlAttribute[0].valueLength != 3)
			{
				return false;
			}
			m_htmlColor.a = (byte)(HexToInt(m_htmlTag[7]) * 16 + HexToInt(m_htmlTag[8]));
			return true;
		case MarkupTag.A:
			if (m_isTextLayoutPhase && !m_isCalculatingPreferredValues && m_xmlAttribute[1].nameHashCode == 2535353)
			{
				int linkCount = m_textInfo.linkCount;
				if (linkCount + 1 > m_textInfo.linkInfo.Length)
				{
					TMP_TextInfo.Resize(ref m_textInfo.linkInfo, linkCount + 1);
				}
				m_textInfo.linkInfo[linkCount].textComponent = this;
				m_textInfo.linkInfo[linkCount].hashCode = 2535353;
				m_textInfo.linkInfo[linkCount].linkTextfirstCharacterIndex = m_characterCount;
				m_textInfo.linkInfo[linkCount].SetLinkID(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
			}
			return true;
		case MarkupTag.SLASH_A:
			if (m_isTextLayoutPhase && !m_isCalculatingPreferredValues)
			{
				int linkCount3 = m_textInfo.linkCount;
				m_textInfo.linkInfo[linkCount3].linkTextLength = m_characterCount - m_textInfo.linkInfo[linkCount3].linkTextfirstCharacterIndex;
				m_textInfo.linkCount++;
			}
			return true;
		case MarkupTag.LINK:
			if (m_isTextLayoutPhase && !m_isCalculatingPreferredValues)
			{
				int linkCount2 = m_textInfo.linkCount;
				if (linkCount2 + 1 > m_textInfo.linkInfo.Length)
				{
					TMP_TextInfo.Resize(ref m_textInfo.linkInfo, linkCount2 + 1);
				}
				m_textInfo.linkInfo[linkCount2].textComponent = this;
				m_textInfo.linkInfo[linkCount2].hashCode = m_xmlAttribute[0].valueHashCode;
				m_textInfo.linkInfo[linkCount2].linkTextfirstCharacterIndex = m_characterCount;
				m_textInfo.linkInfo[linkCount2].linkIdFirstCharacterIndex = startIndex + m_xmlAttribute[0].valueStartIndex;
				m_textInfo.linkInfo[linkCount2].linkIdLength = m_xmlAttribute[0].valueLength;
				m_textInfo.linkInfo[linkCount2].SetLinkID(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			}
			return true;
		case MarkupTag.SLASH_LINK:
			if (m_isTextLayoutPhase && !m_isCalculatingPreferredValues && m_textInfo.linkCount < m_textInfo.linkInfo.Length)
			{
				m_textInfo.linkInfo[m_textInfo.linkCount].linkTextLength = m_characterCount - m_textInfo.linkInfo[m_textInfo.linkCount].linkTextfirstCharacterIndex;
				m_textInfo.linkCount++;
			}
			return true;
		case MarkupTag.ALIGN:
			switch ((MarkupTag)m_xmlAttribute[0].valueHashCode)
			{
			case MarkupTag.LEFT:
				m_lineJustification = HorizontalAlignmentOptions.Left;
				m_lineJustificationStack.Add(m_lineJustification);
				return true;
			case MarkupTag.RIGHT:
				m_lineJustification = HorizontalAlignmentOptions.Right;
				m_lineJustificationStack.Add(m_lineJustification);
				return true;
			case MarkupTag.CENTER:
				m_lineJustification = HorizontalAlignmentOptions.Center;
				m_lineJustificationStack.Add(m_lineJustification);
				return true;
			case MarkupTag.JUSTIFIED:
				m_lineJustification = HorizontalAlignmentOptions.Justified;
				m_lineJustificationStack.Add(m_lineJustification);
				return true;
			case MarkupTag.FLUSH:
				m_lineJustification = HorizontalAlignmentOptions.Flush;
				m_lineJustificationStack.Add(m_lineJustification);
				return true;
			default:
				return false;
			}
		case MarkupTag.SLASH_ALIGN:
			m_lineJustification = m_lineJustificationStack.Remove();
			return true;
		case MarkupTag.WIDTH:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_width = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				return false;
			case TagUnitType.Percentage:
				m_width = m_marginWidth * num3 / 100f;
				break;
			}
			return true;
		case MarkupTag.SLASH_WIDTH:
			m_width = -1f;
			return true;
		case MarkupTag.COLOR:
			if (m_htmlTag[6] == '#' && num == 10)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[6] == '#' && num == 11)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[6] == '#' && num == 13)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[6] == '#' && num == 15)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			switch (m_xmlAttribute[0].valueHashCode)
			{
			case 91635:
				m_htmlColor = Color.red;
				m_colorStack.Add(m_htmlColor);
				return true;
			case 341063360:
				m_htmlColor = new Color32(173, 216, 230, byte.MaxValue);
				m_colorStack.Add(m_htmlColor);
				return true;
			case 2457214:
				m_htmlColor = Color.blue;
				m_colorStack.Add(m_htmlColor);
				return true;
			case 2638345:
				m_htmlColor = new Color32(128, 128, 128, byte.MaxValue);
				m_colorStack.Add(m_htmlColor);
				return true;
			case 81074727:
				m_htmlColor = Color.black;
				m_colorStack.Add(m_htmlColor);
				return true;
			case 87065851:
				m_htmlColor = Color.green;
				m_colorStack.Add(m_htmlColor);
				return true;
			case 105680263:
				m_htmlColor = Color.white;
				m_colorStack.Add(m_htmlColor);
				return true;
			case -1108587920:
				m_htmlColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);
				m_colorStack.Add(m_htmlColor);
				return true;
			case -1250222130:
				m_htmlColor = new Color32(160, 32, 240, byte.MaxValue);
				m_colorStack.Add(m_htmlColor);
				return true;
			case -882444668:
				m_htmlColor = Color.yellow;
				m_colorStack.Add(m_htmlColor);
				return true;
			default:
				return false;
			}
		case MarkupTag.GRADIENT:
		{
			int valueHashCode5 = m_xmlAttribute[0].valueHashCode;
			if (MaterialReferenceManager.TryGetColorGradientPreset(valueHashCode5, out var gradientPreset))
			{
				m_colorGradientPreset = gradientPreset;
			}
			else
			{
				if (gradientPreset == null)
				{
					gradientPreset = Resources.Load<TMP_ColorGradient>(TMP_Settings.defaultColorGradientPresetsPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
				}
				if (gradientPreset == null)
				{
					return false;
				}
				MaterialReferenceManager.AddColorGradientPreset(valueHashCode5, gradientPreset);
				m_colorGradientPreset = gradientPreset;
			}
			m_colorGradientPresetIsTinted = false;
			for (int m = 1; m < m_xmlAttribute.Length && m_xmlAttribute[m].nameHashCode != 0; m++)
			{
				if (m_xmlAttribute[m].nameHashCode == 2960519)
				{
					m_colorGradientPresetIsTinted = ConvertToFloat(m_htmlTag, m_xmlAttribute[m].valueStartIndex, m_xmlAttribute[m].valueLength) != 0f;
				}
			}
			m_colorGradientStack.Add(m_colorGradientPreset);
			return true;
		}
		case MarkupTag.SLASH_GRADIENT:
			m_colorGradientPreset = m_colorGradientStack.Remove();
			return true;
		case MarkupTag.CHARACTER_SPACE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_cSpacing = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				m_cSpacing = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				return false;
			}
			return true;
		case MarkupTag.SLASH_CHARACTER_SPACE:
			if (!m_isTextLayoutPhase)
			{
				return true;
			}
			if (m_characterCount > 0)
			{
				m_xAdvance -= m_cSpacing;
				m_textInfo.characterInfo[m_characterCount - 1].xAdvance = m_xAdvance;
			}
			m_cSpacing = 0f;
			return true;
		case MarkupTag.MONOSPACE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (m_xmlAttribute[0].unitType)
			{
			case TagUnitType.Pixels:
				m_monoSpacing = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				m_monoSpacing = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				return false;
			}
			if (m_xmlAttribute[1].nameHashCode == 582810522)
			{
				m_duoSpace = ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength) != 0f;
			}
			return true;
		case MarkupTag.SLASH_MONOSPACE:
			m_monoSpacing = 0f;
			m_duoSpace = false;
			return true;
		case MarkupTag.CLASS:
			return false;
		case MarkupTag.SLASH_COLOR:
			m_htmlColor = m_colorStack.Remove();
			return true;
		case MarkupTag.INDENT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				tag_Indent = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				tag_Indent = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				tag_Indent = m_marginWidth * num3 / 100f;
				break;
			}
			m_indentStack.Add(tag_Indent);
			m_xAdvance = tag_Indent;
			return true;
		case MarkupTag.SLASH_INDENT:
			tag_Indent = m_indentStack.Remove();
			return true;
		case MarkupTag.LINE_INDENT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				tag_LineIndent = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				tag_LineIndent = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				tag_LineIndent = m_marginWidth * num3 / 100f;
				break;
			}
			m_xAdvance += tag_LineIndent;
			return true;
		case MarkupTag.SLASH_LINE_INDENT:
			tag_LineIndent = 0f;
			return true;
		case MarkupTag.SPRITE:
		{
			int valueHashCode4 = m_xmlAttribute[0].valueHashCode;
			m_spriteIndex = -1;
			TMP_SpriteAsset tMP_SpriteAsset;
			if (m_xmlAttribute[0].valueType == TagValueType.None || m_xmlAttribute[0].valueType == TagValueType.NumericalValue)
			{
				if (m_spriteAsset != null)
				{
					m_currentSpriteAsset = m_spriteAsset;
				}
				else if (m_defaultSpriteAsset != null)
				{
					m_currentSpriteAsset = m_defaultSpriteAsset;
				}
				else if (m_defaultSpriteAsset == null)
				{
					if (TMP_Settings.defaultSpriteAsset != null)
					{
						m_defaultSpriteAsset = TMP_Settings.defaultSpriteAsset;
					}
					else
					{
						m_defaultSpriteAsset = Resources.Load<TMP_SpriteAsset>("Sprite Assets/Default Sprite Asset");
					}
					m_currentSpriteAsset = m_defaultSpriteAsset;
				}
				if (m_currentSpriteAsset == null)
				{
					return false;
				}
			}
			else if (MaterialReferenceManager.TryGetSpriteAsset(valueHashCode4, out tMP_SpriteAsset))
			{
				m_currentSpriteAsset = tMP_SpriteAsset;
			}
			else
			{
				if (tMP_SpriteAsset == null)
				{
					tMP_SpriteAsset = TMP_Text.OnSpriteAssetRequest?.Invoke(valueHashCode4, new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
					if (tMP_SpriteAsset == null)
					{
						tMP_SpriteAsset = Resources.Load<TMP_SpriteAsset>(TMP_Settings.defaultSpriteAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
					}
				}
				if (tMP_SpriteAsset == null)
				{
					return false;
				}
				MaterialReferenceManager.AddSpriteAsset(valueHashCode4, tMP_SpriteAsset);
				m_currentSpriteAsset = tMP_SpriteAsset;
			}
			if (m_xmlAttribute[0].valueType == TagValueType.NumericalValue)
			{
				int num5 = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num5 == -32768)
				{
					return false;
				}
				if (num5 > m_currentSpriteAsset.spriteCharacterTable.Count - 1)
				{
					return false;
				}
				m_spriteIndex = num5;
			}
			m_spriteColor = s_colorWhite;
			m_tintSprite = false;
			for (int l = 0; l < m_xmlAttribute.Length && m_xmlAttribute[l].nameHashCode != 0; l++)
			{
				int nameHashCode2 = m_xmlAttribute[l].nameHashCode;
				int spriteIndex = 0;
				switch ((MarkupTag)nameHashCode2)
				{
				case MarkupTag.NAME:
					m_currentSpriteAsset = TMP_SpriteAsset.SearchForSpriteByHashCode(m_currentSpriteAsset, m_xmlAttribute[l].valueHashCode, includeFallbacks: true, out spriteIndex);
					if (spriteIndex == -1)
					{
						return false;
					}
					m_spriteIndex = spriteIndex;
					break;
				case MarkupTag.INDEX:
					spriteIndex = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
					if (spriteIndex == -32768)
					{
						return false;
					}
					if (spriteIndex > m_currentSpriteAsset.spriteCharacterTable.Count - 1)
					{
						return false;
					}
					m_spriteIndex = spriteIndex;
					break;
				case MarkupTag.TINT:
					m_tintSprite = ConvertToFloat(m_htmlTag, m_xmlAttribute[l].valueStartIndex, m_xmlAttribute[l].valueLength) != 0f;
					break;
				case MarkupTag.COLOR:
					m_spriteColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[l].valueStartIndex, m_xmlAttribute[l].valueLength);
					break;
				case MarkupTag.ANIM:
					if (GetAttributeParameters(m_htmlTag, m_xmlAttribute[l].valueStartIndex, m_xmlAttribute[l].valueLength, ref m_attributeParameterValues) != 3)
					{
						return false;
					}
					m_spriteIndex = (int)m_attributeParameterValues[0];
					if (m_isTextLayoutPhase)
					{
						spriteAnimator.DoSpriteAnimation(m_characterCount, m_currentSpriteAsset, m_spriteIndex, (int)m_attributeParameterValues[1], (int)m_attributeParameterValues[2]);
					}
					break;
				default:
					if (nameHashCode2 != -991527447)
					{
						return false;
					}
					break;
				}
			}
			if (m_spriteIndex == -1)
			{
				return false;
			}
			m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentSpriteAsset.material, m_currentSpriteAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
			m_textElementType = TMP_TextElementType.Sprite;
			return true;
		}
		case MarkupTag.LOWERCASE:
			m_FontStyleInternal |= FontStyles.LowerCase;
			m_fontStyleStack.Add(FontStyles.LowerCase);
			return true;
		case MarkupTag.SLASH_LOWERCASE:
			if ((m_fontStyle & FontStyles.LowerCase) != FontStyles.LowerCase && m_fontStyleStack.Remove(FontStyles.LowerCase) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.LowerCase;
			}
			return true;
		case MarkupTag.UPPERCASE:
		case MarkupTag.ALLCAPS:
			m_FontStyleInternal |= FontStyles.UpperCase;
			m_fontStyleStack.Add(FontStyles.UpperCase);
			return true;
		case MarkupTag.SLASH_ALLCAPS:
		case MarkupTag.SLASH_UPPERCASE:
			if ((m_fontStyle & FontStyles.UpperCase) != FontStyles.UpperCase && m_fontStyleStack.Remove(FontStyles.UpperCase) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.UpperCase;
			}
			return true;
		case MarkupTag.SMALLCAPS:
			m_FontStyleInternal |= FontStyles.SmallCaps;
			m_fontStyleStack.Add(FontStyles.SmallCaps);
			return true;
		case MarkupTag.SLASH_SMALLCAPS:
			if ((m_fontStyle & FontStyles.SmallCaps) != FontStyles.SmallCaps && m_fontStyleStack.Remove(FontStyles.SmallCaps) == 0)
			{
				m_FontStyleInternal &= ~FontStyles.SmallCaps;
			}
			return true;
		case MarkupTag.MARGIN:
			switch (m_xmlAttribute[0].valueType)
			{
			case TagValueType.NumericalValue:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -32768f)
				{
					return false;
				}
				switch (tagUnitType)
				{
				case TagUnitType.Pixels:
					m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f);
					break;
				case TagUnitType.FontUnits:
					m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
					break;
				case TagUnitType.Percentage:
					m_marginLeft = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * num3 / 100f;
					break;
				}
				m_marginLeft = ((m_marginLeft >= 0f) ? m_marginLeft : 0f);
				m_marginRight = m_marginLeft;
				return true;
			case TagValueType.None:
			{
				for (int k = 1; k < m_xmlAttribute.Length && m_xmlAttribute[k].nameHashCode != 0; k++)
				{
					switch ((MarkupTag)m_xmlAttribute[k].nameHashCode)
					{
					case MarkupTag.LEFT:
						num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[k].valueStartIndex, m_xmlAttribute[k].valueLength);
						if (num3 == -32768f)
						{
							return false;
						}
						switch (m_xmlAttribute[k].unitType)
						{
						case TagUnitType.Pixels:
							m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f);
							break;
						case TagUnitType.FontUnits:
							m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
							break;
						case TagUnitType.Percentage:
							m_marginLeft = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * num3 / 100f;
							break;
						}
						m_marginLeft = ((m_marginLeft >= 0f) ? m_marginLeft : 0f);
						break;
					case MarkupTag.RIGHT:
						num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[k].valueStartIndex, m_xmlAttribute[k].valueLength);
						if (num3 == -32768f)
						{
							return false;
						}
						switch (m_xmlAttribute[k].unitType)
						{
						case TagUnitType.Pixels:
							m_marginRight = num3 * (m_isOrthographic ? 1f : 0.1f);
							break;
						case TagUnitType.FontUnits:
							m_marginRight = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
							break;
						case TagUnitType.Percentage:
							m_marginRight = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * num3 / 100f;
							break;
						}
						m_marginRight = ((m_marginRight >= 0f) ? m_marginRight : 0f);
						break;
					}
				}
				return true;
			}
			default:
				return false;
			}
		case MarkupTag.SLASH_MARGIN:
			m_marginLeft = 0f;
			m_marginRight = 0f;
			return true;
		case MarkupTag.MARGIN_LEFT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				m_marginLeft = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				m_marginLeft = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * num3 / 100f;
				break;
			}
			m_marginLeft = ((m_marginLeft >= 0f) ? m_marginLeft : 0f);
			return true;
		case MarkupTag.MARGIN_RIGHT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_marginRight = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				m_marginRight = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
				m_marginRight = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * num3 / 100f;
				break;
			}
			m_marginRight = ((m_marginRight >= 0f) ? m_marginRight : 0f);
			return true;
		case MarkupTag.LINE_HEIGHT:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			switch (tagUnitType)
			{
			case TagUnitType.Pixels:
				m_lineHeight = num3 * (m_isOrthographic ? 1f : 0.1f);
				break;
			case TagUnitType.FontUnits:
				m_lineHeight = num3 * (m_isOrthographic ? 1f : 0.1f) * m_currentFontSize;
				break;
			case TagUnitType.Percentage:
			{
				float num4 = m_currentFontSize / m_currentFontAsset.faceInfo.pointSize * m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1f : 0.1f);
				m_lineHeight = m_fontAsset.faceInfo.lineHeight * num3 / 100f * num4;
				break;
			}
			}
			return true;
		case MarkupTag.SLASH_LINE_HEIGHT:
			m_lineHeight = -32767f;
			return true;
		case MarkupTag.NO_PARSE:
			tag_NoParsing = true;
			return true;
		case MarkupTag.ACTION:
		{
			int valueHashCode = m_xmlAttribute[0].valueHashCode;
			if (m_isTextLayoutPhase)
			{
				m_actionStack.Add(valueHashCode);
				UnityEngine.Debug.Log("Action ID: [" + valueHashCode + "] First character index: " + m_characterCount);
			}
			return true;
		}
		case MarkupTag.SLASH_ACTION:
			if (m_isTextLayoutPhase)
			{
				UnityEngine.Debug.Log("Action ID: [" + m_actionStack.CurrentItem() + "] Last character index: " + (m_characterCount - 1));
			}
			m_actionStack.Remove();
			return true;
		case MarkupTag.SCALE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			m_FXScale = new Vector3(num3, 1f, 1f);
			return true;
		case MarkupTag.SLASH_SCALE:
			m_FXScale = Vector3.one;
			return true;
		case MarkupTag.ROTATE:
			num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
			if (num3 == -32768f)
			{
				return false;
			}
			m_FXRotation = Quaternion.Euler(0f, 0f, num3);
			return true;
		case MarkupTag.SLASH_ROTATE:
			m_FXRotation = Quaternion.identity;
			return true;
		case MarkupTag.TABLE:
			return false;
		case MarkupTag.SLASH_TABLE:
			return false;
		case MarkupTag.TR:
			return false;
		case MarkupTag.SLASH_TR:
			return false;
		case MarkupTag.TH:
			return false;
		case MarkupTag.SLASH_TH:
			return false;
		case MarkupTag.TD:
			return false;
		case MarkupTag.SLASH_TD:
			return false;
		default:
			return false;
		}
	}
}
