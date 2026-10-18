using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class MB3_TextureCombiner
{
	public class CreateAtlasesCoroutineResult
	{
		public bool success = true;

		public bool isFinished;
	}

	internal class TemporaryTexture
	{
		internal string property;

		internal Texture2D texture;

		public TemporaryTexture(string prop, Texture2D tex)
		{
			property = prop;
			texture = tex;
		}
	}

	public class CombineTexturesIntoAtlasesCoroutineResult
	{
		public bool success = true;

		public bool isFinished;
	}

	public const int TEMP_SOLID_COLOR_TEXTURE_SIZE = 16;

	public static Color NEUTRAL_NORMAL_MAP_COLOR_SWIZZLED = new Color(1f, 0.5f, 0.5f, 0.5f);

	public static Color NEUTRAL_NORMAL_MAP_COLOR_NON_SWIZZLED = new Color(0.5f, 0.5f, 1f, 0.5f);

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	[SerializeField]
	protected MB2_TextureBakeResults _textureBakeResults;

	[SerializeField]
	protected int _atlasPadding = 1;

	[SerializeField]
	protected int _maxAtlasSize = 1;

	[SerializeField]
	protected int _maxAtlasWidthOverride = 8192;

	[SerializeField]
	protected int _maxAtlasHeightOverride = 8192;

	[SerializeField]
	protected bool _useMaxAtlasWidthOverride;

	[SerializeField]
	protected bool _useMaxAtlasHeightOverride;

	[SerializeField]
	protected bool _resizePowerOfTwoTextures;

	[SerializeField]
	protected bool _fixOutOfBoundsUVs;

	[SerializeField]
	protected int _layerTexturePackerFastMesh = -1;

	[SerializeField]
	protected int _maxTilingBakeSize = 1024;

	[SerializeField]
	protected bool _saveAtlasesAsAssets;

	[SerializeField]
	protected MB2_TextureBakeResults.ResultType _resultType;

	[SerializeField]
	protected MB2_PackingAlgorithmEnum _packingAlgorithm;

	[SerializeField]
	protected bool _meshBakerTexturePackerForcePowerOfTwo = true;

	[SerializeField]
	protected List<ShaderTextureProperty> _customShaderPropNames = new List<ShaderTextureProperty>();

	[SerializeField]
	protected bool _normalizeTexelDensity;

	[SerializeField]
	protected bool _considerNonTextureProperties;

	protected bool _doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize;

	private List<TemporaryTexture> _temporaryTextures = new List<TemporaryTexture>();

	public static bool _RunCorutineWithoutPauseIsRunning = false;

	public MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return _textureBakeResults;
		}
		set
		{
			_textureBakeResults = value;
		}
	}

	public int atlasPadding
	{
		get
		{
			return _atlasPadding;
		}
		set
		{
			_atlasPadding = value;
		}
	}

	public int maxAtlasSize
	{
		get
		{
			return _maxAtlasSize;
		}
		set
		{
			_maxAtlasSize = value;
		}
	}

	public virtual int maxAtlasWidthOverride
	{
		get
		{
			return _maxAtlasWidthOverride;
		}
		set
		{
			_maxAtlasWidthOverride = value;
		}
	}

	public virtual int maxAtlasHeightOverride
	{
		get
		{
			return _maxAtlasHeightOverride;
		}
		set
		{
			_maxAtlasHeightOverride = value;
		}
	}

	public virtual bool useMaxAtlasWidthOverride
	{
		get
		{
			return _useMaxAtlasWidthOverride;
		}
		set
		{
			_useMaxAtlasWidthOverride = value;
		}
	}

	public virtual bool useMaxAtlasHeightOverride
	{
		get
		{
			return _useMaxAtlasHeightOverride;
		}
		set
		{
			_useMaxAtlasHeightOverride = value;
		}
	}

	public bool resizePowerOfTwoTextures
	{
		get
		{
			return _resizePowerOfTwoTextures;
		}
		set
		{
			_resizePowerOfTwoTextures = value;
		}
	}

	public bool fixOutOfBoundsUVs
	{
		get
		{
			return _fixOutOfBoundsUVs;
		}
		set
		{
			_fixOutOfBoundsUVs = value;
		}
	}

	public int layerTexturePackerFastMesh
	{
		get
		{
			return _layerTexturePackerFastMesh;
		}
		set
		{
			_layerTexturePackerFastMesh = value;
		}
	}

	public int maxTilingBakeSize
	{
		get
		{
			return _maxTilingBakeSize;
		}
		set
		{
			_maxTilingBakeSize = value;
		}
	}

	public bool saveAtlasesAsAssets
	{
		get
		{
			return _saveAtlasesAsAssets;
		}
		set
		{
			_saveAtlasesAsAssets = value;
		}
	}

	public MB2_TextureBakeResults.ResultType resultType
	{
		get
		{
			return _resultType;
		}
		set
		{
			_resultType = value;
		}
	}

	public MB2_PackingAlgorithmEnum packingAlgorithm
	{
		get
		{
			return _packingAlgorithm;
		}
		set
		{
			_packingAlgorithm = value;
		}
	}

	public bool meshBakerTexturePackerForcePowerOfTwo
	{
		get
		{
			return _meshBakerTexturePackerForcePowerOfTwo;
		}
		set
		{
			_meshBakerTexturePackerForcePowerOfTwo = value;
		}
	}

	public List<ShaderTextureProperty> customShaderPropNames
	{
		get
		{
			return _customShaderPropNames;
		}
		set
		{
			_customShaderPropNames = value;
		}
	}

	public bool considerNonTextureProperties
	{
		get
		{
			return _considerNonTextureProperties;
		}
		set
		{
			_considerNonTextureProperties = value;
		}
	}

	public bool doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize
	{
		get
		{
			return _doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize;
		}
		set
		{
			_doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize = value;
		}
	}

	public static void RunCorutineWithoutPause(IEnumerator cor, int recursionDepth)
	{
		if (recursionDepth == 0)
		{
			_RunCorutineWithoutPauseIsRunning = true;
		}
		if (recursionDepth > 20)
		{
			UnityEngine.Debug.LogError("Recursion Depth Exceeded.");
			return;
		}
		while (cor.MoveNext())
		{
			object current = cor.Current;
			if (!(current is YieldInstruction) && current != null && current is IEnumerator)
			{
				RunCorutineWithoutPause((IEnumerator)cor.Current, recursionDepth + 1);
			}
		}
		if (recursionDepth == 0)
		{
			_RunCorutineWithoutPauseIsRunning = false;
		}
	}

	public bool CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, List<string> texPropsToIgnore, MB2_EditorMethodsInterface textureEditorMethods = null, List<AtlasPackingResult> packingResults = null, bool onlyPackRects = false, bool splitAtlasWhenPackingIfTooBig = false)
	{
		CombineTexturesIntoAtlasesCoroutineResult combineTexturesIntoAtlasesCoroutineResult = new CombineTexturesIntoAtlasesCoroutineResult();
		RunCorutineWithoutPause(_CombineTexturesIntoAtlases(progressInfo, combineTexturesIntoAtlasesCoroutineResult, resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, texPropsToIgnore, textureEditorMethods, packingResults, onlyPackRects, splitAtlasWhenPackingIfTooBig), 0);
		if (!combineTexturesIntoAtlasesCoroutineResult.success)
		{
			UnityEngine.Debug.LogError("Failed to generate atlases.");
		}
		return combineTexturesIntoAtlasesCoroutineResult.success;
	}

	public IEnumerator CombineTexturesIntoAtlasesCoroutine(ProgressUpdateDelegate progressInfo, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, List<string> texPropsToIgnore, MB2_EditorMethodsInterface textureEditorMethods = null, CombineTexturesIntoAtlasesCoroutineResult coroutineResult = null, float maxTimePerFrame = 0.01f, List<AtlasPackingResult> packingResults = null, bool onlyPackRects = false, bool splitAtlasWhenPackingIfTooBig = false)
	{
		coroutineResult.success = true;
		coroutineResult.isFinished = false;
		if (maxTimePerFrame <= 0f)
		{
			UnityEngine.Debug.LogError("maxTimePerFrame must be a value greater than zero");
			coroutineResult.isFinished = true;
		}
		else
		{
			yield return _CombineTexturesIntoAtlases(progressInfo, coroutineResult, resultAtlasesAndRects, resultMaterial, objsToMesh, allowedMaterialsFilter, texPropsToIgnore, textureEditorMethods, packingResults, onlyPackRects, splitAtlasWhenPackingIfTooBig);
			coroutineResult.isFinished = true;
		}
	}

	private IEnumerator _CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects, Material resultMaterial, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, List<string> texPropsToIgnore, MB2_EditorMethodsInterface textureEditorMethods, List<AtlasPackingResult> atlasPackingResult, bool onlyPackRects, bool splitAtlasWhenPackingIfTooBig)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			_temporaryTextures.Clear();
			MeshBakerMaterialTexture.readyToBuildAtlases = false;
			if (textureEditorMethods != null)
			{
				textureEditorMethods.Clear();
				textureEditorMethods.OnPreTextureBake();
			}
			if (splitAtlasWhenPackingIfTooBig && !onlyPackRects)
			{
				UnityEngine.Debug.LogError("Can only use 'splitAtlasWhenPackingIfTooLarge' with 'onlyPackRects'");
				result.success = false;
				yield break;
			}
			if (objsToMesh == null || objsToMesh.Count == 0)
			{
				UnityEngine.Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				result.success = false;
				yield break;
			}
			if (_atlasPadding < 0)
			{
				UnityEngine.Debug.LogError("Atlas padding must be zero or greater.");
				result.success = false;
				yield break;
			}
			if (_maxTilingBakeSize < 2 || _maxTilingBakeSize > 8192)
			{
				UnityEngine.Debug.LogError("Invalid value for max tiling bake size.");
				result.success = false;
				yield break;
			}
			for (int i = 0; i < objsToMesh.Count; i++)
			{
				Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
				for (int j = 0; j < gOMaterials.Length; j++)
				{
					if (gOMaterials[j] == null)
					{
						UnityEngine.Debug.LogError("Game object " + objsToMesh[i]?.ToString() + " has a null material");
						result.success = false;
						yield break;
					}
				}
			}
			progressInfo?.Invoke("Collecting textures for " + objsToMesh.Count + " meshes.", 0.01f);
			MB3_TextureCombinerPipeline.TexturePipelineData texturePipelineData = LoadPipelineData(resultMaterial, new List<ShaderTextureProperty>(), objsToMesh, allowedMaterialsFilter, texPropsToIgnore, new List<MB_TexSet>());
			if (!MB3_TextureCombinerPipeline._CollectPropertyNames(texturePipelineData, LOG_LEVEL))
			{
				result.success = false;
				yield break;
			}
			if (_fixOutOfBoundsUVs && (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal || _packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical) && LOG_LEVEL >= MB2_LogLevel.info)
			{
				UnityEngine.Debug.LogWarning("'Consider Mesh UVs' is enabled but packing algorithm is MeshBakerTexturePacker_Horizontal or MeshBakerTexturePacker_Vertical. It is recommended to use these packers without using 'Consider Mesh UVs'");
			}
			texturePipelineData.nonTexturePropertyBlender.LoadTextureBlendersIfNeeded(texturePipelineData.resultMaterial);
			if (onlyPackRects)
			{
				yield return __RunTexturePackerOnly(result, resultAtlasesAndRects, texturePipelineData, splitAtlasWhenPackingIfTooBig, textureEditorMethods, atlasPackingResult);
			}
			else
			{
				yield return __CombineTexturesIntoAtlases(progressInfo, result, resultAtlasesAndRects, texturePipelineData, textureEditorMethods);
			}
		}
		finally
		{
			MB3_TextureCombiner mB3_TextureCombiner = this;
			mB3_TextureCombiner._destroyAllTemporaryTextures();
			mB3_TextureCombiner._restoreProceduralMaterials();
			if (textureEditorMethods != null)
			{
				textureEditorMethods.RestoreReadFlagsAndFormats(progressInfo);
				textureEditorMethods.OnPostTextureBake();
			}
			if (mB3_TextureCombiner.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("===== Done creating atlases for " + resultMaterial?.ToString() + " Total time to create atlases " + sw.Elapsed);
			}
		}
	}

	private MB3_TextureCombinerPipeline.TexturePipelineData LoadPipelineData(Material resultMaterial, List<ShaderTextureProperty> texPropertyNames, List<GameObject> objsToMesh, List<Material> allowedMaterialsFilter, List<string> texPropsToIgnore, List<MB_TexSet> distinctMaterialTextures)
	{
		MB3_TextureCombinerPipeline.TexturePipelineData texturePipelineData = new MB3_TextureCombinerPipeline.TexturePipelineData();
		texturePipelineData._textureBakeResults = _textureBakeResults;
		texturePipelineData._atlasPadding_pix = _atlasPadding;
		if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical && _useMaxAtlasHeightOverride)
		{
			texturePipelineData._maxAtlasHeight = _maxAtlasHeightOverride;
			texturePipelineData._useMaxAtlasHeightOverride = true;
		}
		else
		{
			texturePipelineData._maxAtlasHeight = _maxAtlasSize;
		}
		if (_packingAlgorithm == MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal && _useMaxAtlasWidthOverride)
		{
			texturePipelineData._maxAtlasWidth = _maxAtlasWidthOverride;
			texturePipelineData._useMaxAtlasWidthOverride = true;
		}
		else
		{
			texturePipelineData._maxAtlasWidth = _maxAtlasSize;
		}
		texturePipelineData._saveAtlasesAsAssets = _saveAtlasesAsAssets;
		texturePipelineData.resultType = _resultType;
		texturePipelineData._resizePowerOfTwoTextures = _resizePowerOfTwoTextures;
		texturePipelineData._fixOutOfBoundsUVs = _fixOutOfBoundsUVs;
		texturePipelineData._maxTilingBakeSize = _maxTilingBakeSize;
		texturePipelineData._packingAlgorithm = _packingAlgorithm;
		texturePipelineData._layerTexturePackerFastV2 = _layerTexturePackerFastMesh;
		texturePipelineData._meshBakerTexturePackerForcePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo;
		texturePipelineData._customShaderPropNames = _customShaderPropNames;
		texturePipelineData._normalizeTexelDensity = _normalizeTexelDensity;
		texturePipelineData._considerNonTextureProperties = _considerNonTextureProperties;
		texturePipelineData.doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize = _doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize;
		texturePipelineData.nonTexturePropertyBlender = new MB3_TextureCombinerNonTextureProperties(LOG_LEVEL, _considerNonTextureProperties);
		texturePipelineData.resultMaterial = resultMaterial;
		texturePipelineData.distinctMaterialTextures = distinctMaterialTextures;
		texturePipelineData.allObjsToMesh = objsToMesh;
		texturePipelineData.allowedMaterialsFilter = allowedMaterialsFilter;
		texturePipelineData.texPropertyNames = texPropertyNames;
		texturePipelineData.texPropNamesToIgnore = texPropsToIgnore;
		texturePipelineData.colorSpace = MBVersion.GetProjectColorSpace();
		return texturePipelineData;
	}

	private IEnumerator __CombineTexturesIntoAtlases(ProgressUpdateDelegate progressInfo, CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects, MB3_TextureCombinerPipeline.TexturePipelineData data, MB2_EditorMethodsInterface textureEditorMethods)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("__CombineTexturesIntoAtlases texture properties in shader:" + data.texPropertyNames.Count + " objsToMesh:" + data.allObjsToMesh.Count + " _fixOutOfBoundsUVs:" + data._fixOutOfBoundsUVs);
		}
		progressInfo?.Invoke("Collecting textures ", 0.01f);
		MB3_TextureCombinerPipeline pipeline = new MB3_TextureCombinerPipeline();
		List<GameObject> usedObjsToMesh = new List<GameObject>();
		yield return pipeline.__Step1_CollectDistinctMatTexturesAndUsedObjects(progressInfo, result, data, this, textureEditorMethods, usedObjsToMesh, LOG_LEVEL);
		if (!result.success)
		{
			yield break;
		}
		yield return pipeline.CalculateIdealSizesForTexturesInAtlasAndPadding(progressInfo, result, data, this, textureEditorMethods, LOG_LEVEL);
		if (!result.success)
		{
			yield break;
		}
		StringBuilder report = pipeline.GenerateReport(data);
		MB_ITextureCombinerPacker texturePaker = pipeline.CreatePacker(data.OnlyOneTextureInAtlasReuseTextures(), data._packingAlgorithm);
		if (!texturePaker.Validate(data))
		{
			result.success = false;
			yield break;
		}
		yield return texturePaker.ConvertTexturesToReadableFormats(progressInfo, result, data, this, textureEditorMethods, LOG_LEVEL);
		if (result.success)
		{
			AtlasPackingResult[] array = texturePaker.CalculateAtlasRectangles(data, doMultiAtlas: false, LOG_LEVEL);
			yield return pipeline.__Step3_BuildAndSaveAtlasesAndStoreResults(result, progressInfo, data, this, texturePaker, array[0], textureEditorMethods, resultAtlasesAndRects, report, LOG_LEVEL);
		}
	}

	private IEnumerator __RunTexturePackerOnly(CombineTexturesIntoAtlasesCoroutineResult result, MB_AtlasesAndRects resultAtlasesAndRects, MB3_TextureCombinerPipeline.TexturePipelineData data, bool splitAtlasWhenPackingIfTooBig, MB2_EditorMethodsInterface textureEditorMethods, List<AtlasPackingResult> packingResult)
	{
		MB3_TextureCombinerPipeline pipeline = new MB3_TextureCombinerPipeline();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("__RunTexturePacker texture properties in shader:" + data.texPropertyNames.Count + " objsToMesh:" + data.allObjsToMesh.Count + " _fixOutOfBoundsUVs:" + data._fixOutOfBoundsUVs);
		}
		List<GameObject> usedObjsToMesh = new List<GameObject>();
		yield return pipeline.__Step1_CollectDistinctMatTexturesAndUsedObjects(null, result, data, this, textureEditorMethods, usedObjsToMesh, LOG_LEVEL);
		if (!result.success)
		{
			yield break;
		}
		data.allTexturesAreNullAndSameColor = new MB3_TextureCombinerPipeline.CreateAtlasForProperty[data.texPropertyNames.Count];
		yield return pipeline.CalculateIdealSizesForTexturesInAtlasAndPadding(null, result, data, this, textureEditorMethods, LOG_LEVEL);
		if (result.success)
		{
			MB_ITextureCombinerPacker texturePacker = pipeline.CreatePacker(data.OnlyOneTextureInAtlasReuseTextures(), data._packingAlgorithm);
			AtlasPackingResult[] array = pipeline.RunTexturePackerOnly(data, splitAtlasWhenPackingIfTooBig, resultAtlasesAndRects, texturePacker, LOG_LEVEL);
			for (int i = 0; i < array.Length; i++)
			{
				packingResult.Add(array[i]);
			}
		}
	}

	internal int _getNumTemporaryTextures()
	{
		return _temporaryTextures.Count;
	}

	public Texture2D _createTemporaryTexture(string propertyName, int w, int h, TextureFormat texFormat, bool mipMaps, bool linear)
	{
		Texture2D texture2D = new Texture2D(w, h, texFormat, mipMaps, linear);
		texture2D.name = $"tmp{_temporaryTextures.Count}_{w}x{h}";
		MB_Utility.setSolidColor(texture2D, Color.clear);
		TemporaryTexture item = new TemporaryTexture(propertyName, texture2D);
		_temporaryTextures.Add(item);
		return texture2D;
	}

	internal void AddTemporaryTexture(TemporaryTexture tt)
	{
		_temporaryTextures.Add(tt);
	}

	internal Texture2D _createTextureCopy(ShaderTextureProperty propertyName, Texture2D t)
	{
		Texture2D texture2D = MB_Utility.createTextureCopy(t, propertyName.isGammaCorrected);
		texture2D.name = $"tmpCopy{_temporaryTextures.Count}_{texture2D.width}x{texture2D.height}";
		TemporaryTexture item = new TemporaryTexture(propertyName.name, texture2D);
		_temporaryTextures.Add(item);
		return texture2D;
	}

	internal Texture2D _resizeTexture(ShaderTextureProperty propertyName, Texture2D t, int w, int h)
	{
		Texture2D texture2D = MB_Utility.resampleTexture(t, propertyName.isGammaCorrected, w, h);
		texture2D.name = $"tmpResampled{_temporaryTextures.Count}_{w}x{h}";
		TemporaryTexture item = new TemporaryTexture(propertyName.name, texture2D);
		_temporaryTextures.Add(item);
		return texture2D;
	}

	internal void _destroyAllTemporaryTextures()
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Destroying " + _temporaryTextures.Count + " temporary textures");
		}
		for (int i = 0; i < _temporaryTextures.Count; i++)
		{
			MB_Utility.Destroy(_temporaryTextures[i].texture);
		}
		_temporaryTextures.Clear();
	}

	internal void _destroyTemporaryTextures(string propertyName)
	{
		int num = 0;
		for (int num2 = _temporaryTextures.Count - 1; num2 >= 0; num2--)
		{
			if (_temporaryTextures[num2].property.Equals(propertyName))
			{
				num++;
				MB_Utility.Destroy(_temporaryTextures[num2].texture);
				_temporaryTextures.RemoveAt(num2);
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Destroying " + num + " temporary textures " + propertyName + " num remaining " + _temporaryTextures.Count);
		}
	}

	public void _restoreProceduralMaterials()
	{
	}

	public void SuggestTreatment(List<GameObject> objsToMesh, Material[] resultMaterials, List<ShaderTextureProperty> _customShaderPropNames, List<string> texPropsToIgnore)
	{
		this._customShaderPropNames = _customShaderPropNames;
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<int, MB_Utility.MeshAnalysisResult[]> dictionary = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			GameObject gameObject = objsToMesh[i];
			if (gameObject == null)
			{
				continue;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
			if (gOMaterials.Length > 1)
			{
				stringBuilder.AppendFormat("\nObject {0} uses {1} materials. Possible treatments:\n", objsToMesh[i].name, gOMaterials.Length);
				stringBuilder.AppendFormat("  1) Collapse the submeshes together into one submesh in the combined mesh. Each of the original submesh materials will map to a different UV rectangle in the atlas(es) used by the combined material.\n");
				stringBuilder.AppendFormat("  2) Use the multiple materials feature to map submeshes in the source mesh to submeshes in the combined mesh.\n");
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			if (!dictionary.TryGetValue(mesh.GetInstanceID(), out var value))
			{
				value = new MB_Utility.MeshAnalysisResult[mesh.subMeshCount];
				MB_Utility.doSubmeshesShareVertsOrTris(mesh, ref value[0]);
				for (int j = 0; j < mesh.subMeshCount; j++)
				{
					MB_Utility.hasOutOfBoundsUVs(mesh, ref value[j], j);
					value[j].hasOverlappingSubmeshVerts = value[0].hasOverlappingSubmeshVerts;
				}
				dictionary.Add(mesh.GetInstanceID(), value);
			}
			for (int k = 0; k < gOMaterials.Length; k++)
			{
				if (value[k].hasOutOfBoundsUVs)
				{
					DRect dRect = new DRect(value[k].uvRect);
					stringBuilder.AppendFormat("\nObject {0} submesh={1} material={2} uses UVs outside the range 0,0 .. 1,1 to create tiling that tiles the box {3},{4} .. {5},{6}. This is a problem because the UVs outside the 0,0 .. 1,1 rectangle will pick up neighboring textures in the atlas. Possible Treatments:\n", gameObject, k, gOMaterials[k], dRect.x.ToString("G4"), dRect.y.ToString("G4"), (dRect.x + dRect.width).ToString("G4"), (dRect.y + dRect.height).ToString("G4"));
					stringBuilder.AppendFormat("    1) Ignore the problem. The tiling may not affect result significantly.\n");
					stringBuilder.AppendFormat("    2) Use the 'Consider Mesh UVs' feature to bake the tiling and scale the UVs to fit in the 0,0 .. 1,1 rectangle.\n");
					stringBuilder.AppendFormat("    3) Use the Multiple Materials feature to map the material on this submesh to its own submesh in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
					stringBuilder.AppendFormat("    4) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
				}
			}
			if (value[0].hasOverlappingSubmeshVerts)
			{
				stringBuilder.AppendFormat("\nObject {0} has submeshes that share vertices. This is a problem because each vertex can have only one UV coordinate and may be required to map to different positions in the various atlases that are generated. Possible treatments:\n", objsToMesh[i]);
				stringBuilder.AppendFormat(" 1) Ignore the problem. The vertices may not affect the result.\n");
				stringBuilder.AppendFormat(" 2) Use the Multiple Materials feature to map the submeshs that overlap to their own submeshs in the combined mesh. No other materials should map to this submesh. This will result in only one texture in the atlas(es) and the UVs should tile correctly.\n");
				stringBuilder.AppendFormat(" 3) Combine only meshes that use the same (or subset of) the set of materials on this mesh. The original material(s) can be applied to the result\n");
			}
		}
		Dictionary<Material, List<GameObject>> dictionary2 = new Dictionary<Material, List<GameObject>>();
		for (int l = 0; l < objsToMesh.Count; l++)
		{
			if (!(objsToMesh[l] != null))
			{
				continue;
			}
			Material[] gOMaterials2 = MB_Utility.GetGOMaterials(objsToMesh[l]);
			for (int m = 0; m < gOMaterials2.Length; m++)
			{
				if (gOMaterials2[m] != null)
				{
					if (!dictionary2.TryGetValue(gOMaterials2[m], out var value2))
					{
						value2 = new List<GameObject>();
						dictionary2.Add(gOMaterials2[m], value2);
					}
					if (!value2.Contains(objsToMesh[l]))
					{
						value2.Add(objsToMesh[l]);
					}
				}
			}
		}
		for (int n = 0; n < resultMaterials.Length; n++)
		{
			string shaderName = ((resultMaterials[n] != null) ? "None" : resultMaterials[n].shader.name);
			MB3_TextureCombinerPipeline.TexturePipelineData texturePipelineData = LoadPipelineData(resultMaterials[n], new List<ShaderTextureProperty>(), objsToMesh, new List<Material>(), texPropsToIgnore, new List<MB_TexSet>());
			MB3_TextureCombinerPipeline._CollectPropertyNames(texturePipelineData, LOG_LEVEL);
			foreach (Material key in dictionary2.Keys)
			{
				for (int num = 0; num < texturePipelineData.texPropertyNames.Count; num++)
				{
					if (!key.HasProperty(texturePipelineData.texPropertyNames[num].name))
					{
						continue;
					}
					Texture textureConsideringStandardShaderKeywords = MB3_TextureCombinerPipeline.GetTextureConsideringStandardShaderKeywords(shaderName, key, texturePipelineData.texPropertyNames[num].name);
					if (textureConsideringStandardShaderKeywords != null)
					{
						Vector2 textureOffset = key.GetTextureOffset(texturePipelineData.texPropertyNames[num].name);
						Vector3 vector = key.GetTextureScale(texturePipelineData.texPropertyNames[num].name);
						if (textureOffset.x < 0f || textureOffset.x + vector.x > 1f || textureOffset.y < 0f || textureOffset.y + vector.y > 1f)
						{
							stringBuilder.AppendFormat("\nMaterial {0} used by objects {1} uses texture {2} that is tiled (scale={3} offset={4}). If there is more than one texture in the atlas  then Mesh Baker will bake the tiling into the atlas. If the baked tiling is large then quality can be lost. Possible treatments:\n", key, PrintList(dictionary2[key]), textureConsideringStandardShaderKeywords, vector, textureOffset);
							stringBuilder.AppendFormat("  1) Use the baked tiling.\n");
							stringBuilder.AppendFormat("  2) Use the Multiple Materials feature to map the material on this object/submesh to its own submesh in the combined mesh. No other materials should map to this submesh. The original material can be applied to this submesh.\n");
							stringBuilder.AppendFormat("  3) Combine only meshes that use the same (or subset of) the set of textures on this mesh. The original material can be applied to the result.\n");
						}
					}
				}
			}
		}
		string text = "";
		text = ((stringBuilder.Length != 0) ? ("====== There are possible problems with these meshes that may prevent them from combining well. TREATMENT SUGGESTIONS (copy and paste to text editor if too big) =====\n" + stringBuilder.ToString()) : "====== No problems detected. These meshes should combine well ====\n  If there are problems with the combined meshes please report the problem to digitalOpus.ca so we can improve Mesh Baker.");
		UnityEngine.Debug.Log(text);
	}

	public static bool ShouldTextureBeLinear(ShaderTextureProperty shaderTextureProperty)
	{
		if (shaderTextureProperty.isNormalMap || !shaderTextureProperty.isGammaCorrected)
		{
			return true;
		}
		return false;
	}

	private string PrintList(List<GameObject> gos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < gos.Count; i++)
		{
			stringBuilder.Append(gos[i]?.ToString() + ",");
		}
		return stringBuilder.ToString();
	}
}
