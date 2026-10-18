using System;
using DigitalOpus.MB.Core;
using UnityEngine;

[Serializable]
public class MB_TextureArrayFormatSet
{
	public string name;

	public TextureFormat defaultFormat;

	[Tooltip("The ammount of time Unity takes exploring different compression options to find the compressed version of a texture that most closely matches the original art.This is only used For iOS (and some Android formats)")]
	public MB_TextureCompressionQuality defaultCompressionQuality = MB_TextureCompressionQuality.normal;

	[NonReorderable]
	public MB_TextureArrayFormat[] formatOverrides;

	public bool ValidateTextureImporterFormatsExistsForTextureFormats(MB2_EditorMethodsInterface editorMethods, int idx)
	{
		if (editorMethods == null)
		{
			return true;
		}
		if (!editorMethods.TextureImporterFormatExistsForTextureFormat(defaultFormat))
		{
			Debug.LogError("TextureImporter format does not exist for Texture Array Output Formats: " + idx + " Defaut Format " + defaultFormat);
			return false;
		}
		for (int i = 0; i < formatOverrides.Length; i++)
		{
			if (!editorMethods.TextureImporterFormatExistsForTextureFormat(formatOverrides[i].format))
			{
				Debug.LogError("TextureImporter format does not exist for Texture Array Output Formats: " + idx + " Format Overrides: " + i + " (" + formatOverrides[i].format.ToString() + ")");
				return false;
			}
		}
		return true;
	}

	public TextureFormat GetFormatForProperty(string propName, out MB_TextureCompressionQuality compressionQuality)
	{
		for (int i = 0; i < formatOverrides.Length; i++)
		{
			if (formatOverrides.Equals(formatOverrides[i].propertyName))
			{
				compressionQuality = formatOverrides[i].compressionQuality;
				return formatOverrides[i].format;
			}
		}
		compressionQuality = defaultCompressionQuality;
		return defaultFormat;
	}
}
