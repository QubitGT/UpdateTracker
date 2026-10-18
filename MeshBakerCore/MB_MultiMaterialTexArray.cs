using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MB_MultiMaterialTexArray
{
	public Material combinedMaterial;

	[NonReorderable]
	public List<MB_TexArraySlice> slices = new List<MB_TexArraySlice>();

	[NonReorderable]
	public List<MB_TexArrayForProperty> textureProperties = new List<MB_TexArrayForProperty>();
}
