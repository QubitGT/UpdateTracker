using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TMPro;

internal class TMP_DynamicFontAssetUtilities
{
	public struct FontReference
	{
		public string familyName;

		public string styleName;

		public int faceIndex;

		public string filePath;

		public ulong hashCode;

		public FontReference(string fontFilePath, string faceNameAndStyle, int index)
		{
			familyName = null;
			styleName = null;
			faceIndex = index;
			uint num = 0u;
			uint num2 = 0u;
			filePath = fontFilePath;
			int length = faceNameAndStyle.Length;
			char[] array = new char[length];
			int num3 = 0;
			int length2 = 0;
			for (int i = 0; i < length; i++)
			{
				char c = faceNameAndStyle[i];
				switch (num3)
				{
				case 0:
					if (i + 2 < length && c == ' ' && faceNameAndStyle[i + 1] == '-' && faceNameAndStyle[i + 2] == ' ')
					{
						num3 = 1;
						familyName = new string(array, 0, length2);
						i += 2;
						length2 = 0;
					}
					else
					{
						num = ((num << 5) + num) ^ TMP_TextUtilities.ToUpperFast(c);
						array[length2++] = c;
					}
					break;
				case 1:
					num2 = ((num2 << 5) + num2) ^ TMP_TextUtilities.ToUpperFast(c);
					array[length2++] = c;
					if (i + 1 == length)
					{
						styleName = new string(array, 0, length2);
					}
					break;
				}
			}
			hashCode = ((ulong)num2 << 32) | num;
		}
	}

	private static TMP_DynamicFontAssetUtilities s_Instance = new TMP_DynamicFontAssetUtilities();

	private Dictionary<ulong, FontReference> s_SystemFontLookup;

	private string[] s_SystemFontPaths;

	private uint s_RegularStyleNameHashCode = 1291372090u;

	private void InitializeSystemFontReferenceCache()
	{
		if (s_SystemFontLookup == null)
		{
			s_SystemFontLookup = new Dictionary<ulong, FontReference>();
		}
		else
		{
			s_SystemFontLookup.Clear();
		}
		if (s_SystemFontPaths == null)
		{
			s_SystemFontPaths = Font.GetPathsToOSFonts();
		}
		for (int i = 0; i < s_SystemFontPaths.Length; i++)
		{
			FontEngineError fontEngineError = FontEngine.LoadFontFace(s_SystemFontPaths[i]);
			if (fontEngineError != 0)
			{
				Debug.LogWarning("Error [" + fontEngineError.ToString() + "] trying to load the font at path [" + s_SystemFontPaths[i] + "].");
				continue;
			}
			string[] fontFaces = FontEngine.GetFontFaces();
			for (int j = 0; j < fontFaces.Length; j++)
			{
				FontReference value = new FontReference(s_SystemFontPaths[i], fontFaces[j], j);
				if (!s_SystemFontLookup.ContainsKey(value.hashCode))
				{
					s_SystemFontLookup.Add(value.hashCode, value);
					Debug.Log("[" + i + "] Family Name [" + value.familyName + "]   Style Name [" + value.styleName + "]   Index [" + value.faceIndex + "]   HashCode [" + value.hashCode + "]    Path [" + value.filePath + "].");
				}
			}
			FontEngine.UnloadFontFace();
		}
	}

	public static bool TryGetSystemFontReference(string familyName, out FontReference fontRef)
	{
		return s_Instance.TryGetSystemFontReferenceInternal(familyName, null, out fontRef);
	}

	public static bool TryGetSystemFontReference(string familyName, string styleName, out FontReference fontRef)
	{
		return s_Instance.TryGetSystemFontReferenceInternal(familyName, styleName, out fontRef);
	}

	private bool TryGetSystemFontReferenceInternal(string familyName, string styleName, out FontReference fontRef)
	{
		if (s_SystemFontLookup == null)
		{
			InitializeSystemFontReferenceCache();
		}
		fontRef = default(FontReference);
		uint hashCodeCaseInSensitive = TMP_TextUtilities.GetHashCodeCaseInSensitive(familyName);
		uint num = (string.IsNullOrEmpty(styleName) ? s_RegularStyleNameHashCode : TMP_TextUtilities.GetHashCodeCaseInSensitive(styleName));
		ulong key = ((ulong)num << 32) | hashCodeCaseInSensitive;
		if (s_SystemFontLookup.ContainsKey(key))
		{
			fontRef = s_SystemFontLookup[key];
			return true;
		}
		if (num != s_RegularStyleNameHashCode)
		{
			return false;
		}
		foreach (KeyValuePair<ulong, FontReference> item in s_SystemFontLookup)
		{
			if (item.Value.familyName == familyName)
			{
				fontRef = item.Value;
				return true;
			}
		}
		return false;
	}
}
