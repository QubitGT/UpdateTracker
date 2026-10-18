using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ftLightmapsStorage : MonoBehaviour
{
	public const bool externalStorage = false;

	public List<Renderer> bakedRenderers = new List<Renderer>();

	public List<Renderer> nonBakedRenderers = new List<Renderer>();

	public List<Light> bakedLights = new List<Light>();

	public List<Terrain> bakedRenderersTerrain = new List<Terrain>();

	public List<Texture2D> maps = new List<Texture2D>();

	public List<Texture2D> masks = new List<Texture2D>();

	public List<Texture2D> dirMaps = new List<Texture2D>();

	public List<Texture2D> rnmMaps0 = new List<Texture2D>();

	public List<Texture2D> rnmMaps1 = new List<Texture2D>();

	public List<Texture2D> rnmMaps2 = new List<Texture2D>();

	public List<int> mapsMode = new List<int>();

	public List<int> bakedIDs = new List<int>();

	public List<Vector4> bakedScaleOffset = new List<Vector4>();

	public List<Mesh> bakedVertexColorMesh = new List<Mesh>();

	public List<int> bakedLightChannels = new List<int>();

	public List<int> bakedIDsTerrain = new List<int>();

	public List<Vector4> bakedScaleOffsetTerrain = new List<Vector4>();

	public List<string> assetList = new List<string>();

	public List<int> uvOverlapAssetList = new List<int>();

	public int[] idremap;

	public bool usesRealtimeGI;

	public Texture2D emptyDirectionTex;

	public bool anyVolumes;

	public bool compressedVolumes;

	private void Awake()
	{
		ftLightmaps.RefreshScene(base.gameObject.scene, this, updateNonBaked: false, incrementRefcount: true);
	}

	private void Start()
	{
		ftLightmaps.RefreshScene(base.gameObject.scene, this);
		ftLightmaps.RefreshScene2(base.gameObject.scene, this);
	}

	private void OnDestroy()
	{
		ftLightmaps.UnloadScene(this);
	}
}
