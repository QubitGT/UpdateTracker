using System.Collections.Generic;
using UnityEngine;

public class ftStorageAsset : ScriptableObject
{
	[SerializeField]
	public List<Texture2D> maps = new List<Texture2D>();

	[SerializeField]
	public List<Texture2D> masks = new List<Texture2D>();

	[SerializeField]
	public List<Texture2D> dirMaps = new List<Texture2D>();

	[SerializeField]
	public List<Texture2D> rnmMaps0 = new List<Texture2D>();

	[SerializeField]
	public List<Texture2D> rnmMaps1 = new List<Texture2D>();

	[SerializeField]
	public List<Texture2D> rnmMaps2 = new List<Texture2D>();

	[SerializeField]
	public List<int> mapsMode = new List<int>();

	[SerializeField]
	public List<int> bakedIDs = new List<int>();

	[SerializeField]
	public List<Vector4> bakedScaleOffset = new List<Vector4>();

	[SerializeField]
	public List<Mesh> bakedVertexColorMesh = new List<Mesh>();

	[SerializeField]
	public List<int> bakedLightChannels = new List<int>();

	[SerializeField]
	public List<int> bakedIDsTerrain = new List<int>();

	[SerializeField]
	public List<Vector4> bakedScaleOffsetTerrain = new List<Vector4>();

	[SerializeField]
	public List<string> assetList = new List<string>();

	[SerializeField]
	public List<int> uvOverlapAssetList = new List<int>();

	[SerializeField]
	public int[] idremap;
}
