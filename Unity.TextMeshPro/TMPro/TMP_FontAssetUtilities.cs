using System.Collections.Generic;

namespace TMPro;

public class TMP_FontAssetUtilities
{
	private static readonly TMP_FontAssetUtilities s_Instance;

	private static HashSet<int> k_SearchedAssets;

	public static TMP_FontAssetUtilities instance => s_Instance;

	static TMP_FontAssetUtilities()
	{
		s_Instance = new TMP_FontAssetUtilities();
	}

	public static TMP_Character GetCharacterFromFontAsset(uint unicode, TMP_FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
	{
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
		}
		return GetCharacterFromFontAsset_Internal(unicode, sourceFontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
	}

	private static TMP_Character GetCharacterFromFontAsset_Internal(uint unicode, TMP_FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		bool flag = (fontStyle & FontStyles.Italic) == FontStyles.Italic;
		TMP_Character value;
		AtlasPopulationMode atlasPopulationMode;
		if (flag || fontWeight != FontWeight.Regular)
		{
			uint key = (((uint)(0x80 | ((int)fontStyle << 4)) | ((uint)fontWeight / 100u)) << 24) | unicode;
			if (sourceFontAsset.characterLookupTable.TryGetValue(key, out value))
			{
				isAlternativeTypeface = true;
				if (value.textAsset != null)
				{
					return value;
				}
				sourceFontAsset.characterLookupTable.Remove(unicode);
			}
			TMP_FontWeightPair[] fontWeightTable = sourceFontAsset.fontWeightTable;
			int num = 4;
			switch (fontWeight)
			{
			case FontWeight.Thin:
				num = 1;
				break;
			case FontWeight.ExtraLight:
				num = 2;
				break;
			case FontWeight.Light:
				num = 3;
				break;
			case FontWeight.Regular:
				num = 4;
				break;
			case FontWeight.Medium:
				num = 5;
				break;
			case FontWeight.SemiBold:
				num = 6;
				break;
			case FontWeight.Bold:
				num = 7;
				break;
			case FontWeight.Heavy:
				num = 8;
				break;
			case FontWeight.Black:
				num = 9;
				break;
			}
			TMP_FontAsset tMP_FontAsset = (flag ? fontWeightTable[num].italicTypeface : fontWeightTable[num].regularTypeface);
			if (tMP_FontAsset != null)
			{
				if (tMP_FontAsset.characterLookupTable.TryGetValue(unicode, out value))
				{
					if (value.textAsset != null)
					{
						isAlternativeTypeface = true;
						return value;
					}
					tMP_FontAsset.characterLookupTable.Remove(unicode);
				}
				atlasPopulationMode = tMP_FontAsset.atlasPopulationMode;
				if ((atlasPopulationMode == AtlasPopulationMode.Dynamic || atlasPopulationMode == AtlasPopulationMode.DynamicOS) && tMP_FontAsset.TryAddCharacterInternal(unicode, out value))
				{
					isAlternativeTypeface = true;
					return value;
				}
			}
			if (includeFallbacks)
			{
				List<TMP_FontAsset> fallbackFontAssetTable = sourceFontAsset.fallbackFontAssetTable;
				if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
				{
					return SearchFallbacksForCharacter(unicode, sourceFontAsset, fontStyle, fontWeight, out isAlternativeTypeface);
				}
			}
			return null;
		}
		if (sourceFontAsset.characterLookupTable.TryGetValue(unicode, out value))
		{
			if (value.textAsset != null)
			{
				return value;
			}
			sourceFontAsset.characterLookupTable.Remove(unicode);
		}
		atlasPopulationMode = sourceFontAsset.atlasPopulationMode;
		if ((atlasPopulationMode == AtlasPopulationMode.Dynamic || atlasPopulationMode == AtlasPopulationMode.DynamicOS) && sourceFontAsset.TryAddCharacterInternal(unicode, out value))
		{
			return value;
		}
		if (includeFallbacks)
		{
			List<TMP_FontAsset> fallbackFontAssetTable = sourceFontAsset.fallbackFontAssetTable;
			if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
			{
				return SearchFallbacksForCharacter(unicode, sourceFontAsset, fontStyle, fontWeight, out isAlternativeTypeface);
			}
		}
		return null;
	}

	private static TMP_Character SearchFallbacksForCharacter(uint unicode, TMP_FontAsset sourceFontAsset, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		List<TMP_FontAsset> fallbackFontAssetTable = sourceFontAsset.fallbackFontAssetTable;
		int count = fallbackFontAssetTable.Count;
		if (count == 0)
		{
			return null;
		}
		for (int i = 0; i < count; i++)
		{
			TMP_FontAsset tMP_FontAsset = fallbackFontAssetTable[i];
			if (tMP_FontAsset == null)
			{
				continue;
			}
			int instanceID = tMP_FontAsset.instanceID;
			if (k_SearchedAssets.Add(instanceID))
			{
				TMP_Character characterFromFontAsset_Internal = GetCharacterFromFontAsset_Internal(unicode, tMP_FontAsset, includeFallbacks: true, fontStyle, fontWeight, out isAlternativeTypeface);
				if (characterFromFontAsset_Internal != null)
				{
					return characterFromFontAsset_Internal;
				}
			}
		}
		return null;
	}

	public static TMP_Character GetCharacterFromFontAssets(uint unicode, TMP_FontAsset sourceFontAsset, List<TMP_FontAsset> fontAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		if (fontAssets == null || fontAssets.Count == 0)
		{
			return null;
		}
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
		}
		int count = fontAssets.Count;
		for (int i = 0; i < count; i++)
		{
			TMP_FontAsset tMP_FontAsset = fontAssets[i];
			if (!(tMP_FontAsset == null))
			{
				TMP_Character characterFromFontAsset_Internal = GetCharacterFromFontAsset_Internal(unicode, tMP_FontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
				if (characterFromFontAsset_Internal != null)
				{
					return characterFromFontAsset_Internal;
				}
			}
		}
		return null;
	}

	internal static TMP_TextElement GetTextElementFromTextAssets(uint unicode, TMP_FontAsset sourceFontAsset, List<TMP_Asset> textAssets, bool includeFallbacks, FontStyles fontStyle, FontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		if (textAssets == null || textAssets.Count == 0)
		{
			return null;
		}
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
		}
		int count = textAssets.Count;
		for (int i = 0; i < count; i++)
		{
			TMP_Asset tMP_Asset = textAssets[i];
			if (tMP_Asset == null)
			{
				continue;
			}
			if (tMP_Asset.GetType() == typeof(TMP_FontAsset))
			{
				TMP_FontAsset sourceFontAsset2 = tMP_Asset as TMP_FontAsset;
				TMP_Character characterFromFontAsset_Internal = GetCharacterFromFontAsset_Internal(unicode, sourceFontAsset2, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
				if (characterFromFontAsset_Internal != null)
				{
					return characterFromFontAsset_Internal;
				}
			}
			else
			{
				TMP_SpriteAsset spriteAsset = tMP_Asset as TMP_SpriteAsset;
				TMP_SpriteCharacter spriteCharacterFromSpriteAsset_Internal = GetSpriteCharacterFromSpriteAsset_Internal(unicode, spriteAsset, includeFallbacks: true);
				if (spriteCharacterFromSpriteAsset_Internal != null)
				{
					return spriteCharacterFromSpriteAsset_Internal;
				}
			}
		}
		return null;
	}

	public static TMP_SpriteCharacter GetSpriteCharacterFromSpriteAsset(uint unicode, TMP_SpriteAsset spriteAsset, bool includeFallbacks)
	{
		if (spriteAsset == null)
		{
			return null;
		}
		if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out var value))
		{
			return value;
		}
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
			k_SearchedAssets.Add(spriteAsset.instanceID);
			List<TMP_SpriteAsset> fallbackSpriteAssets = spriteAsset.fallbackSpriteAssets;
			if (fallbackSpriteAssets != null && fallbackSpriteAssets.Count > 0)
			{
				int count = fallbackSpriteAssets.Count;
				for (int i = 0; i < count; i++)
				{
					TMP_SpriteAsset tMP_SpriteAsset = fallbackSpriteAssets[i];
					if (tMP_SpriteAsset == null)
					{
						continue;
					}
					int instanceID = tMP_SpriteAsset.instanceID;
					if (k_SearchedAssets.Add(instanceID))
					{
						value = GetSpriteCharacterFromSpriteAsset_Internal(unicode, tMP_SpriteAsset, includeFallbacks: true);
						if (value != null)
						{
							return value;
						}
					}
				}
			}
		}
		return null;
	}

	private static TMP_SpriteCharacter GetSpriteCharacterFromSpriteAsset_Internal(uint unicode, TMP_SpriteAsset spriteAsset, bool includeFallbacks)
	{
		if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out var value))
		{
			return value;
		}
		if (includeFallbacks)
		{
			List<TMP_SpriteAsset> fallbackSpriteAssets = spriteAsset.fallbackSpriteAssets;
			if (fallbackSpriteAssets != null && fallbackSpriteAssets.Count > 0)
			{
				int count = fallbackSpriteAssets.Count;
				for (int i = 0; i < count; i++)
				{
					TMP_SpriteAsset tMP_SpriteAsset = fallbackSpriteAssets[i];
					if (tMP_SpriteAsset == null)
					{
						continue;
					}
					int instanceID = tMP_SpriteAsset.instanceID;
					if (k_SearchedAssets.Add(instanceID))
					{
						value = GetSpriteCharacterFromSpriteAsset_Internal(unicode, tMP_SpriteAsset, includeFallbacks: true);
						if (value != null)
						{
							return value;
						}
					}
				}
			}
		}
		return null;
	}

	internal static uint GetCodePoint(string text, ref int index)
	{
		char c = text[index];
		if (char.IsHighSurrogate(c) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
		{
			int result = char.ConvertToUtf32(c, text[index + 1]);
			index++;
			return (uint)result;
		}
		return c;
	}

	internal static uint GetCodePoint(uint[] codesPoints, ref int index)
	{
		char c = (char)codesPoints[index];
		if (char.IsHighSurrogate(c) && index + 1 < codesPoints.Length && char.IsLowSurrogate((char)codesPoints[index + 1]))
		{
			int result = char.ConvertToUtf32(c, (char)codesPoints[index + 1]);
			index++;
			return (uint)result;
		}
		return c;
	}
}
