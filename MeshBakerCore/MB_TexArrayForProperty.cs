using System;
using UnityEngine;

[Serializable]
public class MB_TexArrayForProperty
{
	public string texPropertyName;

	[NonReorderable]
	public MB_TextureArrayReference[] formats = new MB_TextureArrayReference[0];

	public MB_TexArrayForProperty(string name, MB_TextureArrayReference[] texRefs)
	{
		texPropertyName = name;
		formats = texRefs;
	}
}
