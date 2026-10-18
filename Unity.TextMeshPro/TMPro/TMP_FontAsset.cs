using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace TMPro;

[Serializable]
[ExcludeFromPreset]
public class TMP_FontAsset : TMP_Asset
{
	[SerializeField]
	internal string m_SourceFontFileGUID;

	[SerializeField]
	internal FontAssetCreationSettings m_CreationSettings;

	[SerializeField]
	private Font m_SourceFontFile;

	[SerializeField]
	private string m_SourceFontFilePath;

	[SerializeField]
	private AtlasPopulationMode m_AtlasPopulationMode;

	[SerializeField]
	internal bool InternalDynamicOS;

	private int m_FamilyNameHashCode;

	private int m_StyleNameHashCode;

	[SerializeField]
	internal List<Glyph> m_GlyphTable = new List<Glyph>();

	internal Dictionary<uint, Glyph> m_GlyphLookupDictionary;

	[SerializeField]
	internal List<TMP_Character> m_CharacterTable = new List<TMP_Character>();

	internal Dictionary<uint, TMP_Character> m_CharacterLookupDictionary;

	internal Texture2D m_AtlasTexture;

	[SerializeField]
	internal Texture2D[] m_AtlasTextures;

	[SerializeField]
	internal int m_AtlasTextureIndex;

	[SerializeField]
	private bool m_IsMultiAtlasTexturesEnabled;

	[SerializeField]
	private bool m_GetFontFeatures = true;

	[SerializeField]
	private bool m_ClearDynamicDataOnBuild;

	[SerializeField]
	internal int m_AtlasWidth;

	[SerializeField]
	internal int m_AtlasHeight;

	[SerializeField]
	internal int m_AtlasPadding;

	[SerializeField]
	internal GlyphRenderMode m_AtlasRenderMode;

	[SerializeField]
	private List<GlyphRect> m_UsedGlyphRects;

	[SerializeField]
	private List<GlyphRect> m_FreeGlyphRects;

	[SerializeField]
	internal TMP_FontFeatureTable m_FontFeatureTable = new TMP_FontFeatureTable();

	[SerializeField]
	internal bool m_ShouldReimportFontFeatures;

	[SerializeField]
	internal List<TMP_FontAsset> m_FallbackFontAssetTable;

	[SerializeField]
	private TMP_FontWeightPair[] m_FontWeightTable = new TMP_FontWeightPair[10];

	[SerializeField]
	private TMP_FontWeightPair[] fontWeights;

	public float normalStyle;

	public float normalSpacingOffset;

	public float boldStyle = 0.75f;

	public float boldSpacing = 7f;

	public byte italicStyle = 35;

	public byte tabSize = 10;

	internal bool IsFontAssetLookupTablesDirty;

	[SerializeField]
	private FaceInfo_Legacy m_fontInfo;

	[SerializeField]
	internal List<TMP_Glyph> m_glyphInfoList;

	[SerializeField]
	[FormerlySerializedAs("m_kerningInfo")]
	internal KerningTable m_KerningTable = new KerningTable();

	[SerializeField]
	private List<TMP_FontAsset> fallbackFontAssets;

	[SerializeField]
	public Texture2D atlas;

	private static readonly List<WeakReference<TMP_FontAsset>> s_CallbackInstances = new List<WeakReference<TMP_FontAsset>>();

	private static ProfilerMarker k_ReadFontAssetDefinitionMarker = new ProfilerMarker("TMP.ReadFontAssetDefinition");

	private static ProfilerMarker k_AddSynthesizedCharactersMarker = new ProfilerMarker("TMP.AddSynthesizedCharacters");

	private static ProfilerMarker k_TryAddGlyphMarker = new ProfilerMarker("TMP.TryAddGlyph");

	private static ProfilerMarker k_TryAddCharacterMarker = new ProfilerMarker("TMP.TryAddCharacter");

	private static ProfilerMarker k_TryAddCharactersMarker = new ProfilerMarker("TMP.TryAddCharacters");

	private static ProfilerMarker k_UpdateLigatureSubstitutionRecordsMarker = new ProfilerMarker("TMP.UpdateLigatureSubstitutionRecords");

	private static ProfilerMarker k_UpdateGlyphAdjustmentRecordsMarker = new ProfilerMarker("TMP.UpdateGlyphAdjustmentRecords");

	private static ProfilerMarker k_UpdateDiacriticalMarkAdjustmentRecordsMarker = new ProfilerMarker("TMP.UpdateDiacriticalAdjustmentRecords");

	private static ProfilerMarker k_ClearFontAssetDataMarker = new ProfilerMarker("TMP.ClearFontAssetData");

	private static ProfilerMarker k_UpdateFontAssetDataMarker = new ProfilerMarker("TMP.UpdateFontAssetData");

	private static string s_DefaultMaterialSuffix = " Atlas Material";

	private static HashSet<int> k_SearchedFontAssetLookup;

	private static List<TMP_FontAsset> k_FontAssets_FontFeaturesUpdateQueue = new List<TMP_FontAsset>();

	private static HashSet<int> k_FontAssets_FontFeaturesUpdateQueueLookup = new HashSet<int>();

	private static List<Texture2D> k_FontAssets_AtlasTexturesUpdateQueue = new List<Texture2D>();

	private static HashSet<int> k_FontAssets_AtlasTexturesUpdateQueueLookup = new HashSet<int>();

	private List<Glyph> m_GlyphsToRender = new List<Glyph>();

	private List<Glyph> m_GlyphsRendered = new List<Glyph>();

	private List<uint> m_GlyphIndexList = new List<uint>();

	private List<uint> m_GlyphIndexListNewlyAdded = new List<uint>();

	internal List<uint> m_GlyphsToAdd = new List<uint>();

	internal HashSet<uint> m_GlyphsToAddLookup = new HashSet<uint>();

	internal List<TMP_Character> m_CharactersToAdd = new List<TMP_Character>();

	internal HashSet<uint> m_CharactersToAddLookup = new HashSet<uint>();

	internal List<uint> s_MissingCharacterList = new List<uint>();

	internal HashSet<uint> m_MissingUnicodesFromFontFile = new HashSet<uint>();

	internal static uint[] k_GlyphIndexArray;

	public FontAssetCreationSettings creationSettings
	{
		get
		{
			return m_CreationSettings;
		}
		set
		{
			m_CreationSettings = value;
		}
	}

	public Font sourceFontFile
	{
		get
		{
			return m_SourceFontFile;
		}
		internal set
		{
			m_SourceFontFile = value;
		}
	}

	public AtlasPopulationMode atlasPopulationMode
	{
		get
		{
			return m_AtlasPopulationMode;
		}
		set
		{
			m_AtlasPopulationMode = value;
		}
	}

	internal int familyNameHashCode
	{
		get
		{
			if (m_FamilyNameHashCode == 0)
			{
				m_FamilyNameHashCode = TMP_TextUtilities.GetHashCode(m_FaceInfo.familyName);
			}
			return m_FamilyNameHashCode;
		}
		set
		{
			m_FamilyNameHashCode = value;
		}
	}

	internal int styleNameHashCode
	{
		get
		{
			if (m_StyleNameHashCode == 0)
			{
				m_StyleNameHashCode = TMP_TextUtilities.GetHashCode(m_FaceInfo.styleName);
			}
			return m_StyleNameHashCode;
		}
		set
		{
			m_StyleNameHashCode = value;
		}
	}

	public List<Glyph> glyphTable
	{
		get
		{
			return m_GlyphTable;
		}
		internal set
		{
			m_GlyphTable = value;
		}
	}

	public Dictionary<uint, Glyph> glyphLookupTable
	{
		get
		{
			if (m_GlyphLookupDictionary == null)
			{
				ReadFontAssetDefinition();
			}
			return m_GlyphLookupDictionary;
		}
	}

	public List<TMP_Character> characterTable
	{
		get
		{
			return m_CharacterTable;
		}
		internal set
		{
			m_CharacterTable = value;
		}
	}

	public Dictionary<uint, TMP_Character> characterLookupTable
	{
		get
		{
			if (m_CharacterLookupDictionary == null)
			{
				ReadFontAssetDefinition();
			}
			return m_CharacterLookupDictionary;
		}
	}

	public Texture2D atlasTexture
	{
		get
		{
			if (m_AtlasTexture == null)
			{
				m_AtlasTexture = atlasTextures[0];
			}
			return m_AtlasTexture;
		}
	}

	public Texture2D[] atlasTextures
	{
		get
		{
			_ = m_AtlasTextures;
			return m_AtlasTextures;
		}
		set
		{
			m_AtlasTextures = value;
		}
	}

	public int atlasTextureCount => m_AtlasTextureIndex + 1;

	public bool isMultiAtlasTexturesEnabled
	{
		get
		{
			return m_IsMultiAtlasTexturesEnabled;
		}
		set
		{
			m_IsMultiAtlasTexturesEnabled = value;
		}
	}

	public bool getFontFeatures
	{
		get
		{
			return m_GetFontFeatures;
		}
		set
		{
			m_GetFontFeatures = value;
		}
	}

	internal bool clearDynamicDataOnBuild
	{
		get
		{
			return m_ClearDynamicDataOnBuild;
		}
		set
		{
			m_ClearDynamicDataOnBuild = value;
		}
	}

	public int atlasWidth
	{
		get
		{
			return m_AtlasWidth;
		}
		internal set
		{
			m_AtlasWidth = value;
		}
	}

	public int atlasHeight
	{
		get
		{
			return m_AtlasHeight;
		}
		internal set
		{
			m_AtlasHeight = value;
		}
	}

	public int atlasPadding
	{
		get
		{
			return m_AtlasPadding;
		}
		internal set
		{
			m_AtlasPadding = value;
		}
	}

	public GlyphRenderMode atlasRenderMode
	{
		get
		{
			return m_AtlasRenderMode;
		}
		internal set
		{
			m_AtlasRenderMode = value;
		}
	}

	internal List<GlyphRect> usedGlyphRects
	{
		get
		{
			return m_UsedGlyphRects;
		}
		set
		{
			m_UsedGlyphRects = value;
		}
	}

	internal List<GlyphRect> freeGlyphRects
	{
		get
		{
			return m_FreeGlyphRects;
		}
		set
		{
			m_FreeGlyphRects = value;
		}
	}

	public TMP_FontFeatureTable fontFeatureTable
	{
		get
		{
			return m_FontFeatureTable;
		}
		internal set
		{
			m_FontFeatureTable = value;
		}
	}

	public List<TMP_FontAsset> fallbackFontAssetTable
	{
		get
		{
			return m_FallbackFontAssetTable;
		}
		set
		{
			m_FallbackFontAssetTable = value;
		}
	}

	public TMP_FontWeightPair[] fontWeightTable
	{
		get
		{
			return m_FontWeightTable;
		}
		internal set
		{
			m_FontWeightTable = value;
		}
	}

	[Obsolete("The fontInfo property and underlying type is now obsolete. Please use the faceInfo property and FaceInfo type instead.")]
	public FaceInfo_Legacy fontInfo => m_fontInfo;

	public static TMP_FontAsset CreateFontAsset(string familyName, string styleName, int pointSize = 90)
	{
		if (FontEngine.TryGetSystemFontReference(familyName, styleName, out var fontRef))
		{
			return CreateFontAsset(fontRef.filePath, fontRef.faceIndex, pointSize, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.DynamicOS);
		}
		Debug.Log("Unable to find a font file with the specified Family Name [" + familyName + "] and Style [" + styleName + "].");
		return null;
	}

	public static TMP_FontAsset CreateFontAsset(string fontFilePath, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight)
	{
		return CreateFontAsset(fontFilePath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight, AtlasPopulationMode.Dynamic);
	}

	private static TMP_FontAsset CreateFontAsset(string fontFilePath, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode, bool enableMultiAtlasSupport = true)
	{
		if (FontEngine.LoadFontFace(fontFilePath, samplingPointSize, faceIndex) != 0)
		{
			Debug.Log("Unable to load font face from [" + fontFilePath + "].");
			return null;
		}
		TMP_FontAsset tMP_FontAsset = CreateFontAssetInstance(null, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
		tMP_FontAsset.m_SourceFontFilePath = fontFilePath;
		return tMP_FontAsset;
	}

	public static TMP_FontAsset CreateFontAsset(Font font)
	{
		return CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
	}

	public static TMP_FontAsset CreateFontAsset(Font font, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic, bool enableMultiAtlasSupport = true)
	{
		return CreateFontAsset(font, 0, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
	}

	private static TMP_FontAsset CreateFontAsset(Font font, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic, bool enableMultiAtlasSupport = true)
	{
		if (FontEngine.LoadFontFace(font, samplingPointSize, faceIndex) != 0)
		{
			Debug.LogWarning("Unable to load font face for [" + font.name + "]. Make sure \"Include Font Data\" is enabled in the Font Import Settings.", font);
			return null;
		}
		return CreateFontAssetInstance(font, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
	}

	private static TMP_FontAsset CreateFontAssetInstance(Font font, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode, bool enableMultiAtlasSupport)
	{
		TMP_FontAsset tMP_FontAsset = ScriptableObject.CreateInstance<TMP_FontAsset>();
		tMP_FontAsset.m_Version = "1.1.0";
		tMP_FontAsset.faceInfo = FontEngine.GetFaceInfo();
		if (atlasPopulationMode == AtlasPopulationMode.Dynamic && font != null)
		{
			tMP_FontAsset.sourceFontFile = font;
		}
		tMP_FontAsset.atlasPopulationMode = atlasPopulationMode;
		tMP_FontAsset.clearDynamicDataOnBuild = TMP_Settings.clearDynamicDataOnBuild;
		tMP_FontAsset.atlasWidth = atlasWidth;
		tMP_FontAsset.atlasHeight = atlasHeight;
		tMP_FontAsset.atlasPadding = atlasPadding;
		tMP_FontAsset.atlasRenderMode = renderMode;
		tMP_FontAsset.atlasTextures = new Texture2D[1];
		TextureFormat textureFormat = (((renderMode & (GlyphRenderMode)65536) != (GlyphRenderMode)65536) ? TextureFormat.Alpha8 : TextureFormat.RGBA32);
		Texture2D texture2D = new Texture2D(1, 1, textureFormat, mipChain: false);
		tMP_FontAsset.atlasTextures[0] = texture2D;
		tMP_FontAsset.isMultiAtlasTexturesEnabled = enableMultiAtlasSupport;
		int num;
		if ((renderMode & (GlyphRenderMode)16) == (GlyphRenderMode)16)
		{
			Material material = null;
			num = 0;
			material = ((textureFormat != TextureFormat.Alpha8) ? new Material(Shader.Find("TextMeshPro/Sprite")) : new Material(ShaderUtilities.ShaderRef_MobileBitmap));
			material.SetTexture(ShaderUtilities.ID_MainTex, texture2D);
			material.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
			material.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);
			tMP_FontAsset.material = material;
		}
		else
		{
			num = 1;
			Material material2 = new Material(ShaderUtilities.ShaderRef_MobileSDF);
			material2.SetTexture(ShaderUtilities.ID_MainTex, texture2D);
			material2.SetFloat(ShaderUtilities.ID_TextureWidth, atlasWidth);
			material2.SetFloat(ShaderUtilities.ID_TextureHeight, atlasHeight);
			material2.SetFloat(ShaderUtilities.ID_GradientScale, atlasPadding + num);
			material2.SetFloat(ShaderUtilities.ID_WeightNormal, tMP_FontAsset.normalStyle);
			material2.SetFloat(ShaderUtilities.ID_WeightBold, tMP_FontAsset.boldStyle);
			tMP_FontAsset.material = material2;
		}
		tMP_FontAsset.freeGlyphRects = new List<GlyphRect>(8)
		{
			new GlyphRect(0, 0, atlasWidth - num, atlasHeight - num)
		};
		tMP_FontAsset.usedGlyphRects = new List<GlyphRect>(8);
		tMP_FontAsset.ReadFontAssetDefinition();
		return tMP_FontAsset;
	}

	private void RegisterCallbackInstance(TMP_FontAsset instance)
	{
		for (int i = 0; i < s_CallbackInstances.Count; i++)
		{
			if (s_CallbackInstances[i].TryGetTarget(out var target) && target == instance)
			{
				return;
			}
		}
		for (int j = 0; j < s_CallbackInstances.Count; j++)
		{
			if (!s_CallbackInstances[j].TryGetTarget(out var _))
			{
				s_CallbackInstances[j] = new WeakReference<TMP_FontAsset>(instance);
				return;
			}
		}
		s_CallbackInstances.Add(new WeakReference<TMP_FontAsset>(this));
	}

	private void OnDestroy()
	{
		DestroyAtlasTextures();
		UnityEngine.Object.DestroyImmediate(m_Material);
	}

	public void ReadFontAssetDefinition()
	{
		InitializeDictionaryLookupTables();
		AddSynthesizedCharactersAndFaceMetrics();
		if (m_FaceInfo.capLine == 0f && m_CharacterLookupDictionary.ContainsKey(88u))
		{
			uint glyphIndex = m_CharacterLookupDictionary[88u].glyphIndex;
			m_FaceInfo.capLine = m_GlyphLookupDictionary[glyphIndex].metrics.horizontalBearingY;
		}
		if (m_FaceInfo.meanLine == 0f && m_CharacterLookupDictionary.ContainsKey(120u))
		{
			uint glyphIndex2 = m_CharacterLookupDictionary[120u].glyphIndex;
			m_FaceInfo.meanLine = m_GlyphLookupDictionary[glyphIndex2].metrics.horizontalBearingY;
		}
		if (m_FaceInfo.scale == 0f)
		{
			m_FaceInfo.scale = 1f;
		}
		if (m_FaceInfo.strikethroughOffset == 0f)
		{
			m_FaceInfo.strikethroughOffset = m_FaceInfo.capLine / 2.5f;
		}
		if (m_AtlasPadding == 0 && base.material.HasProperty(ShaderUtilities.ID_GradientScale))
		{
			m_AtlasPadding = (int)base.material.GetFloat(ShaderUtilities.ID_GradientScale) - 1;
		}
		if (m_FaceInfo.unitsPerEM == 0 && atlasPopulationMode != 0)
		{
			if (!JobsUtility.IsExecutingJob)
			{
				m_FaceInfo.unitsPerEM = FontEngine.GetFaceInfo().unitsPerEM;
				Debug.Log("Font Asset [" + base.name + "] Units Per EM set to " + m_FaceInfo.unitsPerEM + ". Please commit the newly serialized value.");
			}
			else
			{
				Debug.LogError("Font Asset [" + base.name + "] is missing Units Per EM. Please select the 'Reset FaceInfo' menu item on Font Asset [" + base.name + "] to ensure proper serialization.");
			}
		}
		base.hashCode = TMP_TextUtilities.GetHashCode(base.name);
		familyNameHashCode = TMP_TextUtilities.GetHashCode(m_FaceInfo.familyName);
		styleNameHashCode = TMP_TextUtilities.GetHashCode(m_FaceInfo.styleName);
		base.materialHashCode = TMP_TextUtilities.GetSimpleHashCode(base.name + s_DefaultMaterialSuffix);
		TMP_ResourceManager.AddFontAsset(this);
		IsFontAssetLookupTablesDirty = false;
		RegisterCallbackInstance(this);
	}

	internal void InitializeDictionaryLookupTables()
	{
		InitializeGlyphLookupDictionary();
		InitializeCharacterLookupDictionary();
		if ((m_AtlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && m_ShouldReimportFontFeatures)
		{
			ImportFontFeatures();
		}
		InitializeLigatureSubstitutionLookupDictionary();
		InitializeGlyphPaidAdjustmentRecordsLookupDictionary();
		InitializeMarkToBaseAdjustmentRecordsLookupDictionary();
		InitializeMarkToMarkAdjustmentRecordsLookupDictionary();
	}

	internal void InitializeGlyphLookupDictionary()
	{
		if (m_GlyphLookupDictionary == null)
		{
			m_GlyphLookupDictionary = new Dictionary<uint, Glyph>();
		}
		else
		{
			m_GlyphLookupDictionary.Clear();
		}
		if (m_GlyphIndexList == null)
		{
			m_GlyphIndexList = new List<uint>();
		}
		else
		{
			m_GlyphIndexList.Clear();
		}
		if (m_GlyphIndexListNewlyAdded == null)
		{
			m_GlyphIndexListNewlyAdded = new List<uint>();
		}
		else
		{
			m_GlyphIndexListNewlyAdded.Clear();
		}
		int count = m_GlyphTable.Count;
		for (int i = 0; i < count; i++)
		{
			Glyph glyph = m_GlyphTable[i];
			uint index = glyph.index;
			if (!m_GlyphLookupDictionary.ContainsKey(index))
			{
				m_GlyphLookupDictionary.Add(index, glyph);
				m_GlyphIndexList.Add(index);
			}
		}
	}

	internal void InitializeCharacterLookupDictionary()
	{
		if (m_CharacterLookupDictionary == null)
		{
			m_CharacterLookupDictionary = new Dictionary<uint, TMP_Character>();
		}
		else
		{
			m_CharacterLookupDictionary.Clear();
		}
		for (int i = 0; i < m_CharacterTable.Count; i++)
		{
			TMP_Character tMP_Character = m_CharacterTable[i];
			uint unicode = tMP_Character.unicode;
			uint glyphIndex = tMP_Character.glyphIndex;
			if (!m_CharacterLookupDictionary.ContainsKey(unicode))
			{
				m_CharacterLookupDictionary.Add(unicode, tMP_Character);
				tMP_Character.textAsset = this;
				tMP_Character.glyph = m_GlyphLookupDictionary[glyphIndex];
			}
		}
		if (m_MissingUnicodesFromFontFile != null)
		{
			m_MissingUnicodesFromFontFile.Clear();
		}
	}

	internal void ClearFallbackCharacterTable()
	{
		List<uint> list = new List<uint>();
		foreach (KeyValuePair<uint, TMP_Character> item in m_CharacterLookupDictionary)
		{
			if (item.Value.textAsset != this)
			{
				list.Add(item.Key);
			}
		}
		foreach (uint item2 in list)
		{
			m_CharacterLookupDictionary.Remove(item2);
		}
	}

	internal void InitializeLigatureSubstitutionLookupDictionary()
	{
		if (m_FontFeatureTable.m_LigatureSubstitutionRecordLookup == null)
		{
			m_FontFeatureTable.m_LigatureSubstitutionRecordLookup = new Dictionary<uint, List<LigatureSubstitutionRecord>>();
		}
		else
		{
			m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.Clear();
		}
		List<LigatureSubstitutionRecord> ligatureSubstitutionRecords = m_FontFeatureTable.m_LigatureSubstitutionRecords;
		if (ligatureSubstitutionRecords == null)
		{
			return;
		}
		for (int i = 0; i < ligatureSubstitutionRecords.Count; i++)
		{
			LigatureSubstitutionRecord item = ligatureSubstitutionRecords[i];
			if (item.componentGlyphIDs != null && item.componentGlyphIDs.Length != 0)
			{
				uint key = item.componentGlyphIDs[0];
				if (!m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.ContainsKey(key))
				{
					m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.Add(key, new List<LigatureSubstitutionRecord> { item });
				}
				else
				{
					m_FontFeatureTable.m_LigatureSubstitutionRecordLookup[key].Add(item);
				}
			}
		}
	}

	internal void InitializeGlyphPaidAdjustmentRecordsLookupDictionary()
	{
		if (m_KerningTable != null && m_KerningTable.kerningPairs != null && m_KerningTable.kerningPairs.Count > 0)
		{
			UpgradeGlyphAdjustmentTableToFontFeatureTable();
		}
		if (m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup = new Dictionary<uint, GlyphPairAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Clear();
		}
		List<GlyphPairAdjustmentRecord> glyphPairAdjustmentRecords = m_FontFeatureTable.m_GlyphPairAdjustmentRecords;
		if (glyphPairAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < glyphPairAdjustmentRecords.Count; i++)
		{
			GlyphPairAdjustmentRecord value = glyphPairAdjustmentRecords[i];
			uint key = (value.secondAdjustmentRecord.glyphIndex << 16) | value.firstAdjustmentRecord.glyphIndex;
			if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void InitializeMarkToBaseAdjustmentRecordsLookupDictionary()
	{
		if (m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup = new Dictionary<uint, MarkToBaseAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.Clear();
		}
		List<MarkToBaseAdjustmentRecord> markToBaseAdjustmentRecords = m_FontFeatureTable.m_MarkToBaseAdjustmentRecords;
		if (markToBaseAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < markToBaseAdjustmentRecords.Count; i++)
		{
			MarkToBaseAdjustmentRecord value = markToBaseAdjustmentRecords[i];
			uint key = (value.markGlyphID << 16) | value.baseGlyphID;
			if (!m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void InitializeMarkToMarkAdjustmentRecordsLookupDictionary()
	{
		if (m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup = new Dictionary<uint, MarkToMarkAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.Clear();
		}
		List<MarkToMarkAdjustmentRecord> markToMarkAdjustmentRecords = m_FontFeatureTable.m_MarkToMarkAdjustmentRecords;
		if (markToMarkAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < markToMarkAdjustmentRecords.Count; i++)
		{
			MarkToMarkAdjustmentRecord value = markToMarkAdjustmentRecords[i];
			uint key = (value.combiningMarkGlyphID << 16) | value.baseMarkGlyphID;
			if (!m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void AddSynthesizedCharactersAndFaceMetrics()
	{
		bool flag = false;
		if (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS)
		{
			flag = LoadFontFace() == FontEngineError.Success;
			if (!flag && !InternalDynamicOS && TMP_Settings.warningsDisabled)
			{
				Debug.LogWarning("Unable to load font face for [" + base.name + "] font asset.", this);
			}
		}
		AddSynthesizedCharacter(3u, flag, addImmediately: true);
		AddSynthesizedCharacter(9u, flag, addImmediately: true);
		AddSynthesizedCharacter(10u, flag);
		AddSynthesizedCharacter(11u, flag);
		AddSynthesizedCharacter(13u, flag);
		AddSynthesizedCharacter(1564u, flag);
		AddSynthesizedCharacter(8203u, flag);
		AddSynthesizedCharacter(8206u, flag);
		AddSynthesizedCharacter(8207u, flag);
		AddSynthesizedCharacter(8232u, flag);
		AddSynthesizedCharacter(8233u, flag);
		AddSynthesizedCharacter(8288u, flag);
	}

	private void AddSynthesizedCharacter(uint unicode, bool isFontFaceLoaded, bool addImmediately = false)
	{
		if (m_CharacterLookupDictionary.ContainsKey(unicode))
		{
			return;
		}
		if (isFontFaceLoaded && FontEngine.GetGlyphIndex(unicode) != 0)
		{
			if (addImmediately)
			{
				GlyphLoadFlags flags = (((m_AtlasRenderMode & (GlyphRenderMode)4) == (GlyphRenderMode)4) ? (GlyphLoadFlags.LOAD_NO_HINTING | GlyphLoadFlags.LOAD_NO_BITMAP) : GlyphLoadFlags.LOAD_NO_BITMAP);
				if (FontEngine.TryGetGlyphWithUnicodeValue(unicode, flags, out var glyph))
				{
					m_CharacterLookupDictionary.Add(unicode, new TMP_Character(unicode, this, glyph));
				}
			}
		}
		else
		{
			Glyph glyph = new Glyph(0u, new GlyphMetrics(0f, 0f, 0f, 0f, 0f), GlyphRect.zero, 1f, 0);
			m_CharacterLookupDictionary.Add(unicode, new TMP_Character(unicode, this, glyph));
		}
	}

	internal void AddCharacterToLookupCache(uint unicode, TMP_Character character, FontStyles fontStyle = FontStyles.Normal, FontWeight fontWeight = FontWeight.Regular, bool isAlternativeTypeface = false)
	{
		uint key = unicode;
		if (fontStyle != 0 || fontWeight != FontWeight.Regular)
		{
			key = (((uint)((isAlternativeTypeface ? 128 : 0) | ((int)fontStyle << 4)) | ((uint)fontWeight / 100u)) << 24) | unicode;
		}
		m_CharacterLookupDictionary.TryAdd(key, character);
	}

	internal FontEngineError LoadFontFace()
	{
		if (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic)
		{
			if (FontEngine.LoadFontFace(m_SourceFontFile, m_FaceInfo.pointSize, m_FaceInfo.faceIndex) == FontEngineError.Success)
			{
				return FontEngineError.Success;
			}
			if (!string.IsNullOrEmpty(m_SourceFontFilePath))
			{
				return FontEngine.LoadFontFace(m_SourceFontFilePath, m_FaceInfo.pointSize, m_FaceInfo.faceIndex);
			}
			return FontEngineError.Invalid_Face;
		}
		return FontEngine.LoadFontFace(m_FaceInfo.familyName, m_FaceInfo.styleName, m_FaceInfo.pointSize);
	}

	internal void SortCharacterTable()
	{
		if (m_CharacterTable != null && m_CharacterTable.Count > 0)
		{
			m_CharacterTable = m_CharacterTable.OrderBy((TMP_Character c) => c.unicode).ToList();
		}
	}

	internal void SortGlyphTable()
	{
		if (m_GlyphTable != null && m_GlyphTable.Count > 0)
		{
			m_GlyphTable = m_GlyphTable.OrderBy((Glyph c) => c.index).ToList();
		}
	}

	internal void SortFontFeatureTable()
	{
		m_FontFeatureTable.SortGlyphPairAdjustmentRecords();
		m_FontFeatureTable.SortMarkToBaseAdjustmentRecords();
		m_FontFeatureTable.SortMarkToMarkAdjustmentRecords();
	}

	internal void SortAllTables()
	{
		SortGlyphTable();
		SortCharacterTable();
		SortFontFeatureTable();
	}

	public bool HasCharacter(int character)
	{
		if (characterLookupTable == null)
		{
			return false;
		}
		return m_CharacterLookupDictionary.ContainsKey((uint)character);
	}

	public bool HasCharacter(char character, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		if (characterLookupTable == null)
		{
			return false;
		}
		if (m_CharacterLookupDictionary.ContainsKey(character))
		{
			return true;
		}
		if (tryAddCharacter && (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(character, out var _))
		{
			return true;
		}
		if (searchFallbacks)
		{
			if (k_SearchedFontAssetLookup == null)
			{
				k_SearchedFontAssetLookup = new HashSet<int>();
			}
			else
			{
				k_SearchedFontAssetLookup.Clear();
			}
			k_SearchedFontAssetLookup.Add(GetInstanceID());
			if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
			{
				for (int i = 0; i < fallbackFontAssetTable.Count && fallbackFontAssetTable[i] != null; i++)
				{
					TMP_FontAsset tMP_FontAsset = fallbackFontAssetTable[i];
					int item = tMP_FontAsset.GetInstanceID();
					if (k_SearchedFontAssetLookup.Add(item) && tMP_FontAsset.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
					{
						return true;
					}
				}
			}
			if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
			{
				for (int j = 0; j < TMP_Settings.fallbackFontAssets.Count && TMP_Settings.fallbackFontAssets[j] != null; j++)
				{
					TMP_FontAsset tMP_FontAsset2 = TMP_Settings.fallbackFontAssets[j];
					int item2 = tMP_FontAsset2.GetInstanceID();
					if (k_SearchedFontAssetLookup.Add(item2) && tMP_FontAsset2.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
					{
						return true;
					}
				}
			}
			if (TMP_Settings.defaultFontAsset != null)
			{
				TMP_FontAsset defaultFontAsset = TMP_Settings.defaultFontAsset;
				int item3 = defaultFontAsset.GetInstanceID();
				if (k_SearchedFontAssetLookup.Add(item3) && defaultFontAsset.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasCharacter_Internal(uint character, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		if (m_CharacterLookupDictionary == null)
		{
			ReadFontAssetDefinition();
			if (m_CharacterLookupDictionary == null)
			{
				return false;
			}
		}
		if (m_CharacterLookupDictionary.ContainsKey(character))
		{
			return true;
		}
		if (tryAddCharacter && (atlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(character, out var _))
		{
			return true;
		}
		if (searchFallbacks)
		{
			if (fallbackFontAssetTable == null || fallbackFontAssetTable.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < fallbackFontAssetTable.Count && fallbackFontAssetTable[i] != null; i++)
			{
				TMP_FontAsset tMP_FontAsset = fallbackFontAssetTable[i];
				int item = tMP_FontAsset.GetInstanceID();
				if (k_SearchedFontAssetLookup.Add(item) && tMP_FontAsset.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasCharacters(string text, out List<char> missingCharacters)
	{
		if (characterLookupTable == null)
		{
			missingCharacters = null;
			return false;
		}
		missingCharacters = new List<char>();
		for (int i = 0; i < text.Length; i++)
		{
			uint codePoint = TMP_FontAssetUtilities.GetCodePoint(text, ref i);
			if (!m_CharacterLookupDictionary.ContainsKey(codePoint))
			{
				missingCharacters.Add((char)codePoint);
			}
		}
		if (missingCharacters.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool HasCharacters(string text, out uint[] missingCharacters, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		missingCharacters = null;
		if (characterLookupTable == null)
		{
			return false;
		}
		s_MissingCharacterList.Clear();
		for (int i = 0; i < text.Length; i++)
		{
			bool flag = true;
			uint codePoint = TMP_FontAssetUtilities.GetCodePoint(text, ref i);
			if (m_CharacterLookupDictionary.ContainsKey(codePoint) || (tryAddCharacter && (atlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(codePoint, out var _)))
			{
				continue;
			}
			if (searchFallbacks)
			{
				if (k_SearchedFontAssetLookup == null)
				{
					k_SearchedFontAssetLookup = new HashSet<int>();
				}
				else
				{
					k_SearchedFontAssetLookup.Clear();
				}
				k_SearchedFontAssetLookup.Add(GetInstanceID());
				if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
				{
					for (int j = 0; j < fallbackFontAssetTable.Count && fallbackFontAssetTable[j] != null; j++)
					{
						TMP_FontAsset tMP_FontAsset = fallbackFontAssetTable[j];
						int item = tMP_FontAsset.GetInstanceID();
						if (k_SearchedFontAssetLookup.Add(item) && tMP_FontAsset.HasCharacter_Internal(codePoint, searchFallbacks: true, tryAddCharacter))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag && TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
				{
					for (int k = 0; k < TMP_Settings.fallbackFontAssets.Count && TMP_Settings.fallbackFontAssets[k] != null; k++)
					{
						TMP_FontAsset tMP_FontAsset2 = TMP_Settings.fallbackFontAssets[k];
						int item2 = tMP_FontAsset2.GetInstanceID();
						if (k_SearchedFontAssetLookup.Add(item2) && tMP_FontAsset2.HasCharacter_Internal(codePoint, searchFallbacks: true, tryAddCharacter))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag && TMP_Settings.defaultFontAsset != null)
				{
					TMP_FontAsset defaultFontAsset = TMP_Settings.defaultFontAsset;
					int item3 = defaultFontAsset.GetInstanceID();
					if (k_SearchedFontAssetLookup.Add(item3) && defaultFontAsset.HasCharacter_Internal(codePoint, searchFallbacks: true, tryAddCharacter))
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				s_MissingCharacterList.Add(codePoint);
			}
		}
		if (s_MissingCharacterList.Count > 0)
		{
			missingCharacters = s_MissingCharacterList.ToArray();
			return false;
		}
		return true;
	}

	public bool HasCharacters(string text)
	{
		if (characterLookupTable == null)
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			uint codePoint = TMP_FontAssetUtilities.GetCodePoint(text, ref i);
			if (!m_CharacterLookupDictionary.ContainsKey(codePoint))
			{
				return false;
			}
		}
		return true;
	}

	public static string GetCharacters(TMP_FontAsset fontAsset)
	{
		string text = string.Empty;
		for (int i = 0; i < fontAsset.characterTable.Count; i++)
		{
			text += (char)fontAsset.characterTable[i].unicode;
		}
		return text;
	}

	public static int[] GetCharactersArray(TMP_FontAsset fontAsset)
	{
		int[] array = new int[fontAsset.characterTable.Count];
		for (int i = 0; i < fontAsset.characterTable.Count; i++)
		{
			array[i] = (int)fontAsset.characterTable[i].unicode;
		}
		return array;
	}

	internal uint GetGlyphIndex(uint unicode)
	{
		if (m_CharacterLookupDictionary.ContainsKey(unicode))
		{
			return m_CharacterLookupDictionary[unicode].glyphIndex;
		}
		if (LoadFontFace() != 0)
		{
			return 0u;
		}
		return FontEngine.GetGlyphIndex(unicode);
	}

	internal uint GetGlyphVariantIndex(uint unicode, uint variantSelectorUnicode)
	{
		if (LoadFontFace() != 0)
		{
			return 0u;
		}
		return FontEngine.GetVariantGlyphIndex(unicode, variantSelectorUnicode);
	}

	internal static void RegisterFontAssetForFontFeatureUpdate(TMP_FontAsset fontAsset)
	{
		int item = fontAsset.instanceID;
		if (k_FontAssets_FontFeaturesUpdateQueueLookup.Add(item))
		{
			k_FontAssets_FontFeaturesUpdateQueue.Add(fontAsset);
		}
	}

	internal static void UpdateFontFeaturesForFontAssetsInQueue()
	{
		int count = k_FontAssets_FontFeaturesUpdateQueue.Count;
		for (int i = 0; i < count; i++)
		{
			k_FontAssets_FontFeaturesUpdateQueue[i].UpdateGPOSFontFeaturesForNewlyAddedGlyphs();
		}
		if (count > 0)
		{
			k_FontAssets_FontFeaturesUpdateQueue.Clear();
			k_FontAssets_FontFeaturesUpdateQueueLookup.Clear();
		}
	}

	internal static void RegisterAtlasTextureForApply(Texture2D texture)
	{
		int item = texture.GetInstanceID();
		if (k_FontAssets_AtlasTexturesUpdateQueueLookup.Add(item))
		{
			k_FontAssets_AtlasTexturesUpdateQueue.Add(texture);
		}
	}

	internal static void UpdateAtlasTexturesInQueue()
	{
		int count = k_FontAssets_AtlasTexturesUpdateQueueLookup.Count;
		for (int i = 0; i < count; i++)
		{
			k_FontAssets_AtlasTexturesUpdateQueue[i].Apply(updateMipmaps: false, makeNoLongerReadable: false);
		}
		if (count > 0)
		{
			k_FontAssets_AtlasTexturesUpdateQueue.Clear();
			k_FontAssets_AtlasTexturesUpdateQueueLookup.Clear();
		}
	}

	internal static void UpdateFontAssetsInUpdateQueue()
	{
		UpdateAtlasTexturesInQueue();
		UpdateFontFeaturesForFontAssetsInQueue();
	}

	public bool TryAddCharacters(uint[] unicodes, bool includeFontFeatures = false)
	{
		uint[] missingUnicodes;
		return TryAddCharacters(unicodes, out missingUnicodes, includeFontFeatures);
	}

	public bool TryAddCharacters(uint[] unicodes, out uint[] missingUnicodes, bool includeFontFeatures = false)
	{
		if (unicodes == null || unicodes.Length == 0 || m_AtlasPopulationMode == AtlasPopulationMode.Static)
		{
			if (m_AtlasPopulationMode == AtlasPopulationMode.Static)
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because its AtlasPopulationMode is set to Static.", this);
			}
			else
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because the provided Unicode list is Null or Empty.", this);
			}
			missingUnicodes = null;
			return false;
		}
		if (LoadFontFace() != 0)
		{
			missingUnicodes = unicodes.ToArray();
			return false;
		}
		if (m_CharacterLookupDictionary == null || m_GlyphLookupDictionary == null)
		{
			ReadFontAssetDefinition();
		}
		m_GlyphsToAdd.Clear();
		m_GlyphsToAddLookup.Clear();
		m_CharactersToAdd.Clear();
		m_CharactersToAddLookup.Clear();
		s_MissingCharacterList.Clear();
		bool flag = false;
		int num = unicodes.Length;
		for (int i = 0; i < num; i++)
		{
			uint codePoint = TMP_FontAssetUtilities.GetCodePoint(unicodes, ref i);
			if (m_CharacterLookupDictionary.ContainsKey(codePoint))
			{
				continue;
			}
			uint glyphIndex = FontEngine.GetGlyphIndex(codePoint);
			if (glyphIndex == 0)
			{
				switch (codePoint)
				{
				case 160u:
					glyphIndex = FontEngine.GetGlyphIndex(32u);
					break;
				case 173u:
				case 8209u:
					glyphIndex = FontEngine.GetGlyphIndex(45u);
					break;
				}
				if (glyphIndex == 0)
				{
					s_MissingCharacterList.Add(codePoint);
					flag = true;
					continue;
				}
			}
			TMP_Character tMP_Character = new TMP_Character(codePoint, glyphIndex);
			if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
			{
				tMP_Character.glyph = m_GlyphLookupDictionary[glyphIndex];
				tMP_Character.textAsset = this;
				m_CharacterTable.Add(tMP_Character);
				m_CharacterLookupDictionary.Add(codePoint, tMP_Character);
				continue;
			}
			if (m_GlyphsToAddLookup.Add(glyphIndex))
			{
				m_GlyphsToAdd.Add(glyphIndex);
			}
			if (m_CharactersToAddLookup.Add(codePoint))
			{
				m_CharactersToAdd.Add(tMP_Character);
			}
		}
		if (m_GlyphsToAdd.Count == 0)
		{
			missingUnicodes = unicodes;
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width <= 1 || m_AtlasTextures[m_AtlasTextureIndex].height <= 1)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		Glyph[] glyphs;
		bool flag2 = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int j = 0; j < glyphs.Length && glyphs[j] != null; j++)
		{
			Glyph glyph = glyphs[j];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int k = 0; k < m_CharactersToAdd.Count; k++)
		{
			TMP_Character tMP_Character2 = m_CharactersToAdd[k];
			if (!m_GlyphLookupDictionary.TryGetValue(tMP_Character2.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(tMP_Character2.glyphIndex);
				continue;
			}
			tMP_Character2.glyph = value;
			tMP_Character2.textAsset = this;
			m_CharacterTable.Add(tMP_Character2);
			m_CharacterLookupDictionary.Add(tMP_Character2.unicode, tMP_Character2);
			m_CharactersToAdd.RemoveAt(k);
			k--;
		}
		if (m_IsMultiAtlasTexturesEnabled && !flag2)
		{
			while (!flag2)
			{
				flag2 = TryAddGlyphsToNewAtlasTexture();
			}
		}
		if (includeFontFeatures)
		{
			UpdateFontFeaturesForNewlyAddedGlyphs();
		}
		for (int l = 0; l < m_CharactersToAdd.Count; l++)
		{
			TMP_Character tMP_Character3 = m_CharactersToAdd[l];
			s_MissingCharacterList.Add(tMP_Character3.unicode);
		}
		missingUnicodes = null;
		if (s_MissingCharacterList.Count > 0)
		{
			missingUnicodes = s_MissingCharacterList.ToArray();
		}
		if (flag2)
		{
			return !flag;
		}
		return false;
	}

	public bool TryAddCharacters(string characters, bool includeFontFeatures = false)
	{
		string missingCharacters;
		return TryAddCharacters(characters, out missingCharacters, includeFontFeatures);
	}

	public bool TryAddCharacters(string characters, out string missingCharacters, bool includeFontFeatures = false)
	{
		if (string.IsNullOrEmpty(characters) || m_AtlasPopulationMode == AtlasPopulationMode.Static)
		{
			if (m_AtlasPopulationMode == AtlasPopulationMode.Static)
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because its AtlasPopulationMode is set to Static.", this);
			}
			else
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because the provided character list is Null or Empty.", this);
			}
			missingCharacters = characters;
			return false;
		}
		if (LoadFontFace() != 0)
		{
			missingCharacters = characters;
			return false;
		}
		if (m_CharacterLookupDictionary == null || m_GlyphLookupDictionary == null)
		{
			ReadFontAssetDefinition();
		}
		m_GlyphsToAdd.Clear();
		m_GlyphsToAddLookup.Clear();
		m_CharactersToAdd.Clear();
		m_CharactersToAddLookup.Clear();
		s_MissingCharacterList.Clear();
		bool flag = false;
		int length = characters.Length;
		for (int i = 0; i < length; i++)
		{
			uint num = characters[i];
			if (m_CharacterLookupDictionary.ContainsKey(num))
			{
				continue;
			}
			uint glyphIndex = FontEngine.GetGlyphIndex(num);
			if (glyphIndex == 0)
			{
				switch (num)
				{
				case 160u:
					glyphIndex = FontEngine.GetGlyphIndex(32u);
					break;
				case 173u:
				case 8209u:
					glyphIndex = FontEngine.GetGlyphIndex(45u);
					break;
				}
				if (glyphIndex == 0)
				{
					s_MissingCharacterList.Add(num);
					flag = true;
					continue;
				}
			}
			TMP_Character tMP_Character = new TMP_Character(num, glyphIndex);
			if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
			{
				tMP_Character.glyph = m_GlyphLookupDictionary[glyphIndex];
				tMP_Character.textAsset = this;
				m_CharacterTable.Add(tMP_Character);
				m_CharacterLookupDictionary.Add(num, tMP_Character);
				continue;
			}
			if (m_GlyphsToAddLookup.Add(glyphIndex))
			{
				m_GlyphsToAdd.Add(glyphIndex);
			}
			if (m_CharactersToAddLookup.Add(num))
			{
				m_CharactersToAdd.Add(tMP_Character);
			}
		}
		if (m_GlyphsToAdd.Count == 0)
		{
			missingCharacters = characters;
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width <= 1 || m_AtlasTextures[m_AtlasTextureIndex].height <= 1)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		Glyph[] glyphs;
		bool flag2 = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int j = 0; j < glyphs.Length && glyphs[j] != null; j++)
		{
			Glyph glyph = glyphs[j];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int k = 0; k < m_CharactersToAdd.Count; k++)
		{
			TMP_Character tMP_Character2 = m_CharactersToAdd[k];
			if (!m_GlyphLookupDictionary.TryGetValue(tMP_Character2.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(tMP_Character2.glyphIndex);
				continue;
			}
			tMP_Character2.glyph = value;
			tMP_Character2.textAsset = this;
			m_CharacterTable.Add(tMP_Character2);
			m_CharacterLookupDictionary.Add(tMP_Character2.unicode, tMP_Character2);
			m_CharactersToAdd.RemoveAt(k);
			k--;
		}
		if (m_IsMultiAtlasTexturesEnabled && !flag2)
		{
			while (!flag2)
			{
				flag2 = TryAddGlyphsToNewAtlasTexture();
			}
		}
		if (includeFontFeatures)
		{
			UpdateFontFeaturesForNewlyAddedGlyphs();
		}
		missingCharacters = string.Empty;
		for (int l = 0; l < m_CharactersToAdd.Count; l++)
		{
			TMP_Character tMP_Character3 = m_CharactersToAdd[l];
			s_MissingCharacterList.Add(tMP_Character3.unicode);
		}
		if (s_MissingCharacterList.Count > 0)
		{
			missingCharacters = s_MissingCharacterList.UintToString();
		}
		if (flag2)
		{
			return !flag;
		}
		return false;
	}

	internal bool AddGlyphInternal(uint glyphIndex)
	{
		Glyph glyph;
		return TryAddGlyphInternal(glyphIndex, out glyph);
	}

	internal bool TryAddGlyphInternal(uint glyphIndex, out Glyph glyph)
	{
		glyph = null;
		if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
		{
			glyph = m_GlyphLookupDictionary[glyphIndex];
			return true;
		}
		if (m_AtlasPopulationMode == AtlasPopulationMode.Static)
		{
			return false;
		}
		if (LoadFontFace() != 0)
		{
			return false;
		}
		if (!m_AtlasTextures[m_AtlasTextureIndex].isReadable)
		{
			Debug.LogWarning("Unable to add the requested glyph to font asset [" + base.name + "]'s atlas texture. Please make the texture [" + m_AtlasTextures[m_AtlasTextureIndex].name + "] readable.", m_AtlasTextures[m_AtlasTextureIndex]);
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width <= 1 || m_AtlasTextures[m_AtlasTextureIndex].height <= 1)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
		{
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(glyphIndex, glyph);
			m_GlyphIndexList.Add(glyphIndex);
			m_GlyphIndexListNewlyAdded.Add(glyphIndex);
			if (m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime)
			{
				UpdateGSUBFontFeaturesForNewGlyphIndex(glyphIndex);
				RegisterFontAssetForFontFeatureUpdate(this);
			}
			return true;
		}
		if (m_IsMultiAtlasTexturesEnabled && m_UsedGlyphRects.Count > 0)
		{
			SetupNewAtlasTexture();
			if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
			{
				glyph.atlasIndex = m_AtlasTextureIndex;
				m_GlyphTable.Add(glyph);
				m_GlyphLookupDictionary.Add(glyphIndex, glyph);
				m_GlyphIndexList.Add(glyphIndex);
				m_GlyphIndexListNewlyAdded.Add(glyphIndex);
				if (m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime)
				{
					UpdateGSUBFontFeaturesForNewGlyphIndex(glyphIndex);
					RegisterFontAssetForFontFeatureUpdate(this);
				}
				return true;
			}
		}
		return false;
	}

	internal bool TryAddCharacterInternal(uint unicode, out TMP_Character character)
	{
		character = null;
		if (m_MissingUnicodesFromFontFile.Contains(unicode))
		{
			return false;
		}
		if (LoadFontFace() != 0)
		{
			return false;
		}
		uint glyphIndex = FontEngine.GetGlyphIndex(unicode);
		if (glyphIndex == 0)
		{
			switch (unicode)
			{
			case 160u:
				glyphIndex = FontEngine.GetGlyphIndex(32u);
				break;
			case 173u:
			case 8209u:
				glyphIndex = FontEngine.GetGlyphIndex(45u);
				break;
			}
			if (glyphIndex == 0)
			{
				m_MissingUnicodesFromFontFile.Add(unicode);
				return false;
			}
		}
		if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
		{
			character = new TMP_Character(unicode, this, m_GlyphLookupDictionary[glyphIndex]);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			return true;
		}
		Glyph glyph = null;
		if (!m_AtlasTextures[m_AtlasTextureIndex].isReadable)
		{
			Debug.LogWarning("Unable to add the requested character to font asset [" + base.name + "]'s atlas texture. Please make the texture [" + m_AtlasTextures[m_AtlasTextureIndex].name + "] readable.", m_AtlasTextures[m_AtlasTextureIndex]);
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width <= 1 || m_AtlasTextures[m_AtlasTextureIndex].height <= 1)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
		{
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(glyphIndex, glyph);
			character = new TMP_Character(unicode, this, glyph);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			m_GlyphIndexList.Add(glyphIndex);
			m_GlyphIndexListNewlyAdded.Add(glyphIndex);
			if (m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime)
			{
				UpdateGSUBFontFeaturesForNewGlyphIndex(glyphIndex);
				RegisterFontAssetForFontFeatureUpdate(this);
			}
			return true;
		}
		if (m_IsMultiAtlasTexturesEnabled && m_UsedGlyphRects.Count > 0)
		{
			SetupNewAtlasTexture();
			if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
			{
				glyph.atlasIndex = m_AtlasTextureIndex;
				m_GlyphTable.Add(glyph);
				m_GlyphLookupDictionary.Add(glyphIndex, glyph);
				character = new TMP_Character(unicode, this, glyph);
				m_CharacterTable.Add(character);
				m_CharacterLookupDictionary.Add(unicode, character);
				m_GlyphIndexList.Add(glyphIndex);
				m_GlyphIndexListNewlyAdded.Add(glyphIndex);
				if (m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime)
				{
					UpdateGSUBFontFeaturesForNewGlyphIndex(glyphIndex);
					RegisterFontAssetForFontFeatureUpdate(this);
				}
				return true;
			}
		}
		return false;
	}

	internal bool TryGetCharacter_and_QueueRenderToTexture(uint unicode, out TMP_Character character)
	{
		character = null;
		if (m_MissingUnicodesFromFontFile.Contains(unicode))
		{
			return false;
		}
		if (LoadFontFace() != 0)
		{
			return false;
		}
		uint glyphIndex = FontEngine.GetGlyphIndex(unicode);
		if (glyphIndex == 0)
		{
			switch (unicode)
			{
			case 160u:
				glyphIndex = FontEngine.GetGlyphIndex(32u);
				break;
			case 173u:
			case 8209u:
				glyphIndex = FontEngine.GetGlyphIndex(45u);
				break;
			}
			if (glyphIndex == 0)
			{
				m_MissingUnicodesFromFontFile.Add(unicode);
				return false;
			}
		}
		if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
		{
			character = new TMP_Character(unicode, this, m_GlyphLookupDictionary[glyphIndex]);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			return true;
		}
		GlyphLoadFlags flags = ((((GlyphRenderMode)4 & m_AtlasRenderMode) == (GlyphRenderMode)4) ? (GlyphLoadFlags.LOAD_NO_HINTING | GlyphLoadFlags.LOAD_NO_BITMAP) : GlyphLoadFlags.LOAD_NO_BITMAP);
		Glyph glyph = null;
		if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, flags, out glyph))
		{
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(glyphIndex, glyph);
			character = new TMP_Character(unicode, this, glyph);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			m_GlyphIndexList.Add(glyphIndex);
			m_GlyphIndexListNewlyAdded.Add(glyphIndex);
			if (m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime)
			{
				UpdateGSUBFontFeaturesForNewGlyphIndex(glyphIndex);
				RegisterFontAssetForFontFeatureUpdate(this);
			}
			m_GlyphsToRender.Add(glyph);
			return true;
		}
		return false;
	}

	internal void TryAddGlyphsToAtlasTextures()
	{
	}

	private bool TryAddGlyphsToNewAtlasTexture()
	{
		SetupNewAtlasTexture();
		Glyph[] glyphs;
		bool result = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int i = 0; i < glyphs.Length && glyphs[i] != null; i++)
		{
			Glyph glyph = glyphs[i];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int j = 0; j < m_CharactersToAdd.Count; j++)
		{
			TMP_Character tMP_Character = m_CharactersToAdd[j];
			if (!m_GlyphLookupDictionary.TryGetValue(tMP_Character.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(tMP_Character.glyphIndex);
				continue;
			}
			tMP_Character.glyph = value;
			tMP_Character.textAsset = this;
			m_CharacterTable.Add(tMP_Character);
			m_CharacterLookupDictionary.Add(tMP_Character.unicode, tMP_Character);
			m_CharactersToAdd.RemoveAt(j);
			j--;
		}
		return result;
	}

	private void SetupNewAtlasTexture()
	{
		m_AtlasTextureIndex++;
		if (m_AtlasTextures.Length == m_AtlasTextureIndex)
		{
			Array.Resize(ref m_AtlasTextures, m_AtlasTextures.Length * 2);
		}
		TextureFormat textureFormat = (((m_AtlasRenderMode & (GlyphRenderMode)65536) != (GlyphRenderMode)65536) ? TextureFormat.Alpha8 : TextureFormat.RGBA32);
		m_AtlasTextures[m_AtlasTextureIndex] = new Texture2D(m_AtlasWidth, m_AtlasHeight, textureFormat, mipChain: false);
		FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		int num = (((m_AtlasRenderMode & (GlyphRenderMode)16) != (GlyphRenderMode)16) ? 1 : 0);
		m_FreeGlyphRects.Clear();
		m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - num, m_AtlasHeight - num));
		m_UsedGlyphRects.Clear();
	}

	internal void UpdateAtlasTexture()
	{
		if (m_GlyphsToRender.Count != 0)
		{
			if (m_AtlasTextures[m_AtlasTextureIndex].width <= 1 || m_AtlasTextures[m_AtlasTextureIndex].height <= 1)
			{
				m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
				FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
			}
			m_AtlasTextures[m_AtlasTextureIndex].Apply(updateMipmaps: false, makeNoLongerReadable: false);
		}
	}

	private void UpdateFontFeaturesForNewlyAddedGlyphs()
	{
		UpdateLigatureSubstitutionRecords();
		UpdateGlyphAdjustmentRecords();
		UpdateDiacriticalMarkAdjustmentRecords();
		m_GlyphIndexListNewlyAdded.Clear();
	}

	private void UpdateGPOSFontFeaturesForNewlyAddedGlyphs()
	{
		UpdateGlyphAdjustmentRecords();
		UpdateDiacriticalMarkAdjustmentRecords();
		m_GlyphIndexListNewlyAdded.Clear();
	}

	internal void ImportFontFeatures()
	{
		if (LoadFontFace() == FontEngineError.Success)
		{
			GlyphPairAdjustmentRecord[] allPairAdjustmentRecords = FontEngine.GetAllPairAdjustmentRecords();
			if (allPairAdjustmentRecords != null)
			{
				AddPairAdjustmentRecords(allPairAdjustmentRecords);
			}
			UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord[] allMarkToBaseAdjustmentRecords = FontEngine.GetAllMarkToBaseAdjustmentRecords();
			if (allMarkToBaseAdjustmentRecords != null)
			{
				AddMarkToBaseAdjustmentRecords(allMarkToBaseAdjustmentRecords);
			}
			UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord[] allMarkToMarkAdjustmentRecords = FontEngine.GetAllMarkToMarkAdjustmentRecords();
			if (allMarkToMarkAdjustmentRecords != null)
			{
				AddMarkToMarkAdjustmentRecords(allMarkToMarkAdjustmentRecords);
			}
			UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] allLigatureSubstitutionRecords = FontEngine.GetAllLigatureSubstitutionRecords();
			if (allLigatureSubstitutionRecords != null)
			{
				AddLigatureSubstitutionRecords(allLigatureSubstitutionRecords);
			}
			m_ShouldReimportFontFeatures = false;
		}
	}

	private void UpdateGSUBFontFeaturesForNewGlyphIndex(uint glyphIndex)
	{
		UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] ligatureSubstitutionRecords = FontEngine.GetLigatureSubstitutionRecords(glyphIndex);
		if (ligatureSubstitutionRecords != null)
		{
			AddLigatureSubstitutionRecords(ligatureSubstitutionRecords);
		}
	}

	internal void UpdateLigatureSubstitutionRecords()
	{
		UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] ligatureSubstitutionRecords = FontEngine.GetLigatureSubstitutionRecords(m_GlyphIndexListNewlyAdded);
		if (ligatureSubstitutionRecords != null)
		{
			AddLigatureSubstitutionRecords(ligatureSubstitutionRecords);
		}
	}

	private void AddLigatureSubstitutionRecords(UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord[] records)
	{
		for (int i = 0; i < records.Length; i++)
		{
			UnityEngine.TextCore.LowLevel.LigatureSubstitutionRecord ligatureSubstitutionRecord = records[i];
			if (records[i].componentGlyphIDs == null || records[i].ligatureGlyphID == 0)
			{
				break;
			}
			uint key = ligatureSubstitutionRecord.componentGlyphIDs[0];
			LigatureSubstitutionRecord ligatureSubstitutionRecord2 = default(LigatureSubstitutionRecord);
			ligatureSubstitutionRecord2.componentGlyphIDs = ligatureSubstitutionRecord.componentGlyphIDs;
			ligatureSubstitutionRecord2.ligatureGlyphID = ligatureSubstitutionRecord.ligatureGlyphID;
			LigatureSubstitutionRecord ligatureSubstitutionRecord3 = ligatureSubstitutionRecord2;
			if (m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.TryGetValue(key, out var value))
			{
				foreach (LigatureSubstitutionRecord item in value)
				{
					if (ligatureSubstitutionRecord3 == item)
					{
						return;
					}
				}
				m_FontFeatureTable.m_LigatureSubstitutionRecordLookup[key].Add(ligatureSubstitutionRecord3);
			}
			else
			{
				m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.Add(key, new List<LigatureSubstitutionRecord> { ligatureSubstitutionRecord3 });
			}
			m_FontFeatureTable.m_LigatureSubstitutionRecords.Add(ligatureSubstitutionRecord3);
		}
	}

	internal void UpdateGlyphAdjustmentRecords()
	{
		GlyphPairAdjustmentRecord[] pairAdjustmentRecords = FontEngine.GetPairAdjustmentRecords(m_GlyphIndexListNewlyAdded);
		if (pairAdjustmentRecords != null)
		{
			AddPairAdjustmentRecords(pairAdjustmentRecords);
		}
	}

	private void AddPairAdjustmentRecords(GlyphPairAdjustmentRecord[] records)
	{
		float num = m_FaceInfo.pointSize / (float)m_FaceInfo.unitsPerEM;
		for (int i = 0; i < records.Length; i++)
		{
			GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = records[i];
			GlyphAdjustmentRecord firstAdjustmentRecord = glyphPairAdjustmentRecord.firstAdjustmentRecord;
			GlyphAdjustmentRecord secondAdjustmentRecord = glyphPairAdjustmentRecord.secondAdjustmentRecord;
			uint glyphIndex = firstAdjustmentRecord.glyphIndex;
			uint glyphIndex2 = secondAdjustmentRecord.glyphIndex;
			if (glyphIndex == 0 && glyphIndex2 == 0)
			{
				break;
			}
			uint key = (glyphIndex2 << 16) | glyphIndex;
			if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
			{
				GlyphValueRecord glyphValueRecord = firstAdjustmentRecord.glyphValueRecord;
				glyphValueRecord.xAdvance *= num;
				glyphPairAdjustmentRecord.firstAdjustmentRecord = new GlyphAdjustmentRecord(glyphIndex, glyphValueRecord);
				m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Add(glyphPairAdjustmentRecord);
				m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, glyphPairAdjustmentRecord);
			}
		}
	}

	internal void UpdateGlyphAdjustmentRecords(uint[] glyphIndexes)
	{
		GlyphPairAdjustmentRecord[] glyphPairAdjustmentTable = FontEngine.GetGlyphPairAdjustmentTable(glyphIndexes);
		if (glyphPairAdjustmentTable == null || glyphPairAdjustmentTable.Length == 0)
		{
			return;
		}
		if (m_FontFeatureTable == null)
		{
			m_FontFeatureTable = new TMP_FontFeatureTable();
		}
		for (int i = 0; i < glyphPairAdjustmentTable.Length && glyphPairAdjustmentTable[i].firstAdjustmentRecord.glyphIndex != 0; i++)
		{
			uint key = (glyphPairAdjustmentTable[i].secondAdjustmentRecord.glyphIndex << 16) | glyphPairAdjustmentTable[i].firstAdjustmentRecord.glyphIndex;
			if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
			{
				GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = glyphPairAdjustmentTable[i];
				m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Add(glyphPairAdjustmentRecord);
				m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, glyphPairAdjustmentRecord);
			}
		}
	}

	internal void UpdateDiacriticalMarkAdjustmentRecords()
	{
		UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord[] markToBaseAdjustmentRecords = FontEngine.GetMarkToBaseAdjustmentRecords(m_GlyphIndexListNewlyAdded);
		if (markToBaseAdjustmentRecords != null)
		{
			AddMarkToBaseAdjustmentRecords(markToBaseAdjustmentRecords);
		}
		UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord[] markToMarkAdjustmentRecords = FontEngine.GetMarkToMarkAdjustmentRecords(m_GlyphIndexListNewlyAdded);
		if (markToMarkAdjustmentRecords != null)
		{
			AddMarkToMarkAdjustmentRecords(markToMarkAdjustmentRecords);
		}
	}

	private void AddMarkToBaseAdjustmentRecords(UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord[] records)
	{
		float num = m_FaceInfo.pointSize / (float)m_FaceInfo.unitsPerEM;
		for (int i = 0; i < records.Length; i++)
		{
			UnityEngine.TextCore.LowLevel.MarkToBaseAdjustmentRecord markToBaseAdjustmentRecord = records[i];
			if (records[i].baseGlyphID == 0 || records[i].markGlyphID == 0)
			{
				break;
			}
			uint key = (markToBaseAdjustmentRecord.markGlyphID << 16) | markToBaseAdjustmentRecord.baseGlyphID;
			if (!m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.ContainsKey(key))
			{
				MarkToBaseAdjustmentRecord markToBaseAdjustmentRecord2 = default(MarkToBaseAdjustmentRecord);
				markToBaseAdjustmentRecord2.baseGlyphID = markToBaseAdjustmentRecord.baseGlyphID;
				markToBaseAdjustmentRecord2.baseGlyphAnchorPoint = new GlyphAnchorPoint
				{
					xCoordinate = markToBaseAdjustmentRecord.baseGlyphAnchorPoint.xCoordinate * num,
					yCoordinate = markToBaseAdjustmentRecord.baseGlyphAnchorPoint.yCoordinate * num
				};
				markToBaseAdjustmentRecord2.markGlyphID = markToBaseAdjustmentRecord.markGlyphID;
				markToBaseAdjustmentRecord2.markPositionAdjustment = new MarkPositionAdjustment
				{
					xPositionAdjustment = markToBaseAdjustmentRecord.markPositionAdjustment.xPositionAdjustment * num,
					yPositionAdjustment = markToBaseAdjustmentRecord.markPositionAdjustment.yPositionAdjustment * num
				};
				MarkToBaseAdjustmentRecord markToBaseAdjustmentRecord3 = markToBaseAdjustmentRecord2;
				m_FontFeatureTable.MarkToBaseAdjustmentRecords.Add(markToBaseAdjustmentRecord3);
				m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.Add(key, markToBaseAdjustmentRecord3);
			}
		}
	}

	private void AddMarkToMarkAdjustmentRecords(UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord[] records)
	{
		float num = m_FaceInfo.pointSize / (float)m_FaceInfo.unitsPerEM;
		for (int i = 0; i < records.Length; i++)
		{
			UnityEngine.TextCore.LowLevel.MarkToMarkAdjustmentRecord markToMarkAdjustmentRecord = records[i];
			if (records[i].baseMarkGlyphID == 0 || records[i].combiningMarkGlyphID == 0)
			{
				break;
			}
			uint key = (markToMarkAdjustmentRecord.combiningMarkGlyphID << 16) | markToMarkAdjustmentRecord.baseMarkGlyphID;
			if (!m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.ContainsKey(key))
			{
				MarkToMarkAdjustmentRecord markToMarkAdjustmentRecord2 = default(MarkToMarkAdjustmentRecord);
				markToMarkAdjustmentRecord2.baseMarkGlyphID = markToMarkAdjustmentRecord.baseMarkGlyphID;
				markToMarkAdjustmentRecord2.baseMarkGlyphAnchorPoint = new GlyphAnchorPoint
				{
					xCoordinate = markToMarkAdjustmentRecord.baseMarkGlyphAnchorPoint.xCoordinate * num,
					yCoordinate = markToMarkAdjustmentRecord.baseMarkGlyphAnchorPoint.yCoordinate * num
				};
				markToMarkAdjustmentRecord2.combiningMarkGlyphID = markToMarkAdjustmentRecord.combiningMarkGlyphID;
				markToMarkAdjustmentRecord2.combiningMarkPositionAdjustment = new MarkPositionAdjustment
				{
					xPositionAdjustment = markToMarkAdjustmentRecord.combiningMarkPositionAdjustment.xPositionAdjustment * num,
					yPositionAdjustment = markToMarkAdjustmentRecord.combiningMarkPositionAdjustment.yPositionAdjustment * num
				};
				MarkToMarkAdjustmentRecord markToMarkAdjustmentRecord3 = markToMarkAdjustmentRecord2;
				m_FontFeatureTable.MarkToMarkAdjustmentRecords.Add(markToMarkAdjustmentRecord3);
				m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.Add(key, markToMarkAdjustmentRecord3);
			}
		}
	}

	private void CopyListDataToArray<T>(List<T> srcList, ref T[] dstArray)
	{
		int count = srcList.Count;
		if (dstArray == null)
		{
			dstArray = new T[count];
		}
		else
		{
			Array.Resize(ref dstArray, count);
		}
		for (int i = 0; i < count; i++)
		{
			dstArray[i] = srcList[i];
		}
	}

	internal void UpdateFontAssetData()
	{
		uint[] array = new uint[m_CharacterTable.Count];
		for (int i = 0; i < m_CharacterTable.Count; i++)
		{
			array[i] = m_CharacterTable[i].unicode;
		}
		ClearCharacterAndGlyphTables();
		ClearFontFeaturesTables();
		ClearAtlasTextures(setAtlasSizeToZero: true);
		ReadFontAssetDefinition();
		if (array.Length != 0)
		{
			TryAddCharacters(array, m_GetFontFeatures && TMP_Settings.getFontFeaturesAtRuntime);
		}
	}

	public void ClearFontAssetData(bool setAtlasSizeToZero = false)
	{
		ClearCharacterAndGlyphTables();
		ClearFontFeaturesTables();
		ClearAtlasTextures(setAtlasSizeToZero);
		ReadFontAssetDefinition();
		for (int i = 0; i < s_CallbackInstances.Count; i++)
		{
			if (s_CallbackInstances[i].TryGetTarget(out var target) && target != this)
			{
				target.ClearFallbackCharacterTable();
			}
		}
		TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(isChanged: true, this);
	}

	internal void ClearCharacterAndGlyphTablesInternal()
	{
		ClearCharacterAndGlyphTables();
		ClearAtlasTextures(setAtlasSizeToZero: true);
		ReadFontAssetDefinition();
	}

	internal void ClearFontFeaturesInternal()
	{
		ClearFontFeaturesTables();
		ReadFontAssetDefinition();
	}

	private void ClearCharacterAndGlyphTables()
	{
		if (m_GlyphTable != null)
		{
			m_GlyphTable.Clear();
		}
		if (m_CharacterTable != null)
		{
			m_CharacterTable.Clear();
		}
		if (m_UsedGlyphRects != null)
		{
			m_UsedGlyphRects.Clear();
		}
		if (m_FreeGlyphRects != null)
		{
			int num = (((m_AtlasRenderMode & (GlyphRenderMode)16) != (GlyphRenderMode)16) ? 1 : 0);
			m_FreeGlyphRects.Clear();
			m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - num, m_AtlasHeight - num));
		}
		if (m_GlyphsToRender != null)
		{
			m_GlyphsToRender.Clear();
		}
		if (m_GlyphsRendered != null)
		{
			m_GlyphsRendered.Clear();
		}
	}

	private void ClearFontFeaturesTables()
	{
		if (m_FontFeatureTable != null && m_FontFeatureTable.m_LigatureSubstitutionRecords != null)
		{
			m_FontFeatureTable.m_LigatureSubstitutionRecords.Clear();
		}
		if (m_FontFeatureTable != null && m_FontFeatureTable.m_GlyphPairAdjustmentRecords != null)
		{
			m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Clear();
		}
		if (m_FontFeatureTable != null && m_FontFeatureTable.m_MarkToBaseAdjustmentRecords != null)
		{
			m_FontFeatureTable.m_MarkToBaseAdjustmentRecords.Clear();
		}
		if (m_FontFeatureTable != null && m_FontFeatureTable.m_MarkToMarkAdjustmentRecords != null)
		{
			m_FontFeatureTable.m_MarkToMarkAdjustmentRecords.Clear();
		}
	}

	internal void ClearAtlasTextures(bool setAtlasSizeToZero = false)
	{
		m_AtlasTextureIndex = 0;
		if (m_AtlasTextures == null)
		{
			return;
		}
		Texture2D texture2D = null;
		for (int i = 1; i < m_AtlasTextures.Length; i++)
		{
			texture2D = m_AtlasTextures[i];
			if (!(texture2D == null))
			{
				UnityEngine.Object.DestroyImmediate(texture2D, allowDestroyingAssets: true);
			}
		}
		Array.Resize(ref m_AtlasTextures, 1);
		texture2D = (m_AtlasTexture = m_AtlasTextures[0]);
		_ = texture2D.isReadable;
		TextureFormat format = (((m_AtlasRenderMode & (GlyphRenderMode)65536) != (GlyphRenderMode)65536) ? TextureFormat.Alpha8 : TextureFormat.RGBA32);
		if (setAtlasSizeToZero)
		{
			texture2D.Reinitialize(1, 1, format, hasMipMap: false);
		}
		else if (texture2D.width != m_AtlasWidth || texture2D.height != m_AtlasHeight)
		{
			texture2D.Reinitialize(m_AtlasWidth, m_AtlasHeight, format, hasMipMap: false);
		}
		FontEngine.ResetAtlasTexture(texture2D);
		texture2D.Apply();
	}

	private void DestroyAtlasTextures()
	{
		if (m_AtlasTextures == null)
		{
			return;
		}
		for (int i = 0; i < m_AtlasTextures.Length; i++)
		{
			Texture2D texture2D = m_AtlasTextures[i];
			if (texture2D != null)
			{
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
		}
	}

	private void UpgradeGlyphAdjustmentTableToFontFeatureTable()
	{
		Debug.Log("Upgrading font asset [" + base.name + "] Glyph Adjustment Table.", this);
		if (m_FontFeatureTable == null)
		{
			m_FontFeatureTable = new TMP_FontFeatureTable();
		}
		int count = m_KerningTable.kerningPairs.Count;
		m_FontFeatureTable.m_GlyphPairAdjustmentRecords = new List<GlyphPairAdjustmentRecord>(count);
		for (int i = 0; i < count; i++)
		{
			KerningPair kerningPair = m_KerningTable.kerningPairs[i];
			uint glyphIndex = 0u;
			if (m_CharacterLookupDictionary.TryGetValue(kerningPair.firstGlyph, out var value))
			{
				glyphIndex = value.glyphIndex;
			}
			uint glyphIndex2 = 0u;
			if (m_CharacterLookupDictionary.TryGetValue(kerningPair.secondGlyph, out var value2))
			{
				glyphIndex2 = value2.glyphIndex;
			}
			GlyphAdjustmentRecord firstAdjustmentRecord = new GlyphAdjustmentRecord(glyphIndex, new GlyphValueRecord(kerningPair.firstGlyphAdjustments.xPlacement, kerningPair.firstGlyphAdjustments.yPlacement, kerningPair.firstGlyphAdjustments.xAdvance, kerningPair.firstGlyphAdjustments.yAdvance));
			GlyphAdjustmentRecord secondAdjustmentRecord = new GlyphAdjustmentRecord(glyphIndex2, new GlyphValueRecord(kerningPair.secondGlyphAdjustments.xPlacement, kerningPair.secondGlyphAdjustments.yPlacement, kerningPair.secondGlyphAdjustments.xAdvance, kerningPair.secondGlyphAdjustments.yAdvance));
			GlyphPairAdjustmentRecord item = new GlyphPairAdjustmentRecord(firstAdjustmentRecord, secondAdjustmentRecord);
			m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Add(item);
		}
		m_KerningTable.kerningPairs = null;
		m_KerningTable = null;
	}
}
