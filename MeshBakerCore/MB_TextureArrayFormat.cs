using System;
using DigitalOpus.MB.Core;
using UnityEngine;

[Serializable]
public class MB_TextureArrayFormat
{
	public string propertyName;

	public TextureFormat format;

	[Tooltip("The ammount of time Unity takes exploring different compression options to find the compressed version of a texture that most closely matches the original art.This is only used For iOS (and some Android formats)")]
	public MB_TextureCompressionQuality compressionQuality = MB_TextureCompressionQuality.normal;
}
