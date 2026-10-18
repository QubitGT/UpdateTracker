using System.Collections.Generic;
using UnityEngine;

namespace TMPro;

public class TMP_ResourceManager
{
	private struct FontAssetRef
	{
		public int nameHashCode;

		public int familyNameHashCode;

		public int styleNameHashCode;

		public long familyNameAndStyleHashCode;

		public readonly TMP_FontAsset fontAsset;

		public FontAssetRef(int nameHashCode, int familyNameHashCode, int styleNameHashCode, TMP_FontAsset fontAsset)
		{
			this.nameHashCode = ((nameHashCode != 0) ? nameHashCode : familyNameHashCode);
			this.familyNameHashCode = familyNameHashCode;
			this.styleNameHashCode = styleNameHashCode;
			familyNameAndStyleHashCode = ((long)styleNameHashCode << 32) | (uint)familyNameHashCode;
			this.fontAsset = fontAsset;
		}
	}

	private static TMP_Settings s_TextSettings;

	private static readonly Dictionary<int, FontAssetRef> s_FontAssetReferences = new Dictionary<int, FontAssetRef>();

	private static readonly Dictionary<int, TMP_FontAsset> s_FontAssetNameReferenceLookup = new Dictionary<int, TMP_FontAsset>();

	private static readonly Dictionary<long, TMP_FontAsset> s_FontAssetFamilyNameAndStyleReferenceLookup = new Dictionary<long, TMP_FontAsset>();

	private static readonly List<int> s_FontAssetRemovalList = new List<int>(16);

	private static readonly int k_RegularStyleHashCode = TMP_TextUtilities.GetHashCode("Regular");

	internal static TMP_Settings GetTextSettings()
	{
		if (s_TextSettings == null)
		{
			s_TextSettings = Resources.Load<TMP_Settings>("TextSettings");
		}
		return s_TextSettings;
	}

	public static void AddFontAsset(TMP_FontAsset fontAsset)
	{
		int instanceID = fontAsset.instanceID;
		if (!s_FontAssetReferences.ContainsKey(instanceID))
		{
			FontAssetRef value = new FontAssetRef(fontAsset.hashCode, fontAsset.familyNameHashCode, fontAsset.styleNameHashCode, fontAsset);
			s_FontAssetReferences.Add(instanceID, value);
			if (!s_FontAssetNameReferenceLookup.ContainsKey(value.nameHashCode))
			{
				s_FontAssetNameReferenceLookup.Add(value.nameHashCode, fontAsset);
			}
			if (!s_FontAssetFamilyNameAndStyleReferenceLookup.ContainsKey(value.familyNameAndStyleHashCode))
			{
				s_FontAssetFamilyNameAndStyleReferenceLookup.Add(value.familyNameAndStyleHashCode, fontAsset);
			}
			return;
		}
		FontAssetRef value2 = s_FontAssetReferences[instanceID];
		if (value2.nameHashCode == fontAsset.hashCode && value2.familyNameHashCode == fontAsset.familyNameHashCode && value2.styleNameHashCode == fontAsset.styleNameHashCode)
		{
			return;
		}
		if (value2.nameHashCode != fontAsset.hashCode)
		{
			s_FontAssetNameReferenceLookup.Remove(value2.nameHashCode);
			value2.nameHashCode = fontAsset.hashCode;
			if (!s_FontAssetNameReferenceLookup.ContainsKey(value2.nameHashCode))
			{
				s_FontAssetNameReferenceLookup.Add(value2.nameHashCode, fontAsset);
			}
		}
		if (value2.familyNameHashCode != fontAsset.familyNameHashCode || value2.styleNameHashCode != fontAsset.styleNameHashCode)
		{
			s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(value2.familyNameAndStyleHashCode);
			value2.familyNameHashCode = fontAsset.familyNameHashCode;
			value2.styleNameHashCode = fontAsset.styleNameHashCode;
			value2.familyNameAndStyleHashCode = ((long)fontAsset.styleNameHashCode << 32) | (uint)fontAsset.familyNameHashCode;
			if (!s_FontAssetFamilyNameAndStyleReferenceLookup.ContainsKey(value2.familyNameAndStyleHashCode))
			{
				s_FontAssetFamilyNameAndStyleReferenceLookup.Add(value2.familyNameAndStyleHashCode, fontAsset);
			}
		}
		s_FontAssetReferences[instanceID] = value2;
	}

	public static void RemoveFontAsset(TMP_FontAsset fontAsset)
	{
		int instanceID = fontAsset.instanceID;
		if (s_FontAssetReferences.TryGetValue(instanceID, out var value))
		{
			s_FontAssetNameReferenceLookup.Remove(value.nameHashCode);
			s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(value.familyNameAndStyleHashCode);
			s_FontAssetReferences.Remove(instanceID);
		}
	}

	internal static bool TryGetFontAssetByName(int nameHashcode, out TMP_FontAsset fontAsset)
	{
		fontAsset = null;
		return s_FontAssetNameReferenceLookup.TryGetValue(nameHashcode, out fontAsset);
	}

	internal static bool TryGetFontAssetByFamilyName(int familyNameHashCode, int styleNameHashCode, out TMP_FontAsset fontAsset)
	{
		fontAsset = null;
		if (styleNameHashCode == 0)
		{
			styleNameHashCode = k_RegularStyleHashCode;
		}
		long key = ((long)styleNameHashCode << 32) | (uint)familyNameHashCode;
		return s_FontAssetFamilyNameAndStyleReferenceLookup.TryGetValue(key, out fontAsset);
	}

	public static void ClearFontAssetGlyphCache()
	{
		RebuildFontAssetCache();
	}

	internal static void RebuildFontAssetCache()
	{
		foreach (KeyValuePair<int, FontAssetRef> s_FontAssetReference in s_FontAssetReferences)
		{
			FontAssetRef value = s_FontAssetReference.Value;
			TMP_FontAsset fontAsset = value.fontAsset;
			if (fontAsset == null)
			{
				s_FontAssetNameReferenceLookup.Remove(value.nameHashCode);
				s_FontAssetFamilyNameAndStyleReferenceLookup.Remove(value.familyNameAndStyleHashCode);
				s_FontAssetRemovalList.Add(s_FontAssetReference.Key);
			}
			else
			{
				fontAsset.InitializeCharacterLookupDictionary();
				fontAsset.AddSynthesizedCharactersAndFaceMetrics();
			}
		}
		for (int i = 0; i < s_FontAssetRemovalList.Count; i++)
		{
			s_FontAssetReferences.Remove(s_FontAssetRemovalList[i]);
		}
		s_FontAssetRemovalList.Clear();
		TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(isChanged: true, null);
	}
}
