using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TextureBaker : MB3_MeshBakerRoot
{
	public delegate void OnCombinedTexturesCoroutineSuccess();

	public delegate void OnCombinedTexturesCoroutineFail();

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	[SerializeField]
	protected MB2_TextureBakeResults _textureBakeResults;

	[SerializeField]
	protected int _atlasPadding = 1;

	[SerializeField]
	protected int _maxAtlasSize = 8192;

	[SerializeField]
	protected bool _useMaxAtlasWidthOverride;

	[SerializeField]
	protected int _maxAtlasWidthOverride = 8192;

	[SerializeField]
	protected bool _useMaxAtlasHeightOverride;

	[SerializeField]
	protected int _maxAtlasHeightOverride = 8192;

	[SerializeField]
	protected bool _resizePowerOfTwoTextures;

	[SerializeField]
	protected bool _fixOutOfBoundsUVs;

	[SerializeField]
	protected int _maxTilingBakeSize = 1024;

	[SerializeField]
	protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;

	[SerializeField]
	protected int _layerTexturePackerFastMesh = -1;

	[SerializeField]
	protected bool _meshBakerTexturePackerForcePowerOfTwo = true;

	[SerializeField]
	[NonReorderable]
	protected List<ShaderTextureProperty> _customShaderProperties = new List<ShaderTextureProperty>();

	[SerializeField]
	[NonReorderable]
	protected List<string> _texturePropNamesToIgnore = new List<string>();

	[SerializeField]
	protected List<string> _customShaderPropNames_Depricated = new List<string>();

	[SerializeField]
	protected MB2_TextureBakeResults.ResultType _resultType;

	[SerializeField]
	protected bool _doMultiMaterial;

	[SerializeField]
	protected bool _doMultiMaterialSplitAtlasesIfTooBig = true;

	[SerializeField]
	protected bool _doMultiMaterialSplitAtlasesIfOBUVs = true;

	[SerializeField]
	protected Material _resultMaterial;

	[SerializeField]
	protected bool _considerNonTextureProperties;

	[SerializeField]
	protected bool _doSuggestTreatment = true;

	private MB3_TextureCombiner.CreateAtlasesCoroutineResult _coroutineResult;

	[NonReorderable]
	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];

	[NonReorderable]
	public MB_MultiMaterialTexArray[] resultMaterialsTexArray = new MB_MultiMaterialTexArray[0];

	[NonReorderable]
	public MB_TextureArrayFormatSet[] textureArrayOutputFormats;

	[NonReorderable]
	public List<GameObject> objsToMesh;

	public OnCombinedTexturesCoroutineSuccess onBuiltAtlasesSuccess;

	public OnCombinedTexturesCoroutineFail onBuiltAtlasesFail;

	public MB_AtlasesAndRects[] OnCombinedTexturesCoroutineAtlasesAndRects;

	public override MB2_TextureBakeResults textureBakeResults
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

	public virtual int atlasPadding
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

	public virtual int maxAtlasSize
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

	public virtual bool resizePowerOfTwoTextures
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

	public virtual bool fixOutOfBoundsUVs
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

	public virtual int maxTilingBakeSize
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

	public virtual MB2_PackingAlgorithmEnum packingAlgorithm
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

	public virtual int layerForTexturePackerFastMesh
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

	public virtual List<ShaderTextureProperty> customShaderProperties
	{
		get
		{
			return _customShaderProperties;
		}
		set
		{
			_customShaderProperties = value;
		}
	}

	public virtual List<string> texturePropNamesToIgnore
	{
		get
		{
			return _texturePropNamesToIgnore;
		}
		set
		{
			_texturePropNamesToIgnore = value;
		}
	}

	public virtual List<string> customShaderPropNames
	{
		get
		{
			return _customShaderPropNames_Depricated;
		}
		set
		{
			_customShaderPropNames_Depricated = value;
		}
	}

	public virtual MB2_TextureBakeResults.ResultType resultType
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

	public virtual bool doMultiMaterial
	{
		get
		{
			return _doMultiMaterial;
		}
		set
		{
			_doMultiMaterial = value;
		}
	}

	public virtual bool doMultiMaterialSplitAtlasesIfTooBig
	{
		get
		{
			return _doMultiMaterialSplitAtlasesIfTooBig;
		}
		set
		{
			_doMultiMaterialSplitAtlasesIfTooBig = value;
		}
	}

	public virtual bool doMultiMaterialSplitAtlasesIfOBUVs
	{
		get
		{
			return _doMultiMaterialSplitAtlasesIfOBUVs;
		}
		set
		{
			_doMultiMaterialSplitAtlasesIfOBUVs = value;
		}
	}

	public virtual Material resultMaterial
	{
		get
		{
			return _resultMaterial;
		}
		set
		{
			_resultMaterial = value;
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

	public bool doSuggestTreatment
	{
		get
		{
			return _doSuggestTreatment;
		}
		set
		{
			_doSuggestTreatment = value;
		}
	}

	public MB3_TextureCombiner.CreateAtlasesCoroutineResult CoroutineResult => _coroutineResult;

	public override List<GameObject> GetObjectsToCombine()
	{
		if (objsToMesh == null)
		{
			objsToMesh = new List<GameObject>();
		}
		return objsToMesh;
	}

	[ContextMenu("Purge Objects to Combine of null references")]
	public override void PurgeNullsFromObjectsToCombine()
	{
		if (objsToMesh == null)
		{
			objsToMesh = new List<GameObject>();
		}
		Debug.Log($"Purged {objsToMesh.RemoveAll((GameObject obj) => obj == null)} null references from objects to combine list.");
	}

	public MB_AtlasesAndRects[] CreateAtlases()
	{
		return CreateAtlases(null);
	}

	public IEnumerator CreateAtlasesCoroutine(ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		yield return _CreateAtlasesCoroutine(progressInfo, coroutineResult, saveAtlasesAsAssets, editorMethods, maxTimePerFrame);
		if (coroutineResult.success && onBuiltAtlasesSuccess != null)
		{
			onBuiltAtlasesSuccess();
		}
		if (!coroutineResult.success && onBuiltAtlasesFail != null)
		{
			onBuiltAtlasesFail();
		}
	}

	private IEnumerator _CreateAtlasesCoroutineAtlases(MB3_TextureCombiner combiner, ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		int num = 1;
		if (_doMultiMaterial)
		{
			num = resultMaterials.Length;
		}
		OnCombinedTexturesCoroutineAtlasesAndRects = new MB_AtlasesAndRects[num];
		for (int j = 0; j < OnCombinedTexturesCoroutineAtlasesAndRects.Length; j++)
		{
			OnCombinedTexturesCoroutineAtlasesAndRects[j] = new MB_AtlasesAndRects();
		}
		for (int i = 0; i < OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
		{
			List<Material> allowedMaterialsFilter = null;
			Material combinedMaterial;
			if (_doMultiMaterial)
			{
				allowedMaterialsFilter = resultMaterials[i].sourceMaterials;
				combinedMaterial = resultMaterials[i].combinedMaterial;
				combiner.fixOutOfBoundsUVs = resultMaterials[i].considerMeshUVs;
			}
			else
			{
				combinedMaterial = _resultMaterial;
			}
			MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult coroutineResult2 = new MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult();
			yield return combiner.CombineTexturesIntoAtlasesCoroutine(progressInfo, OnCombinedTexturesCoroutineAtlasesAndRects[i], combinedMaterial, objsToMesh, allowedMaterialsFilter, texturePropNamesToIgnore, editorMethods, coroutineResult2, maxTimePerFrame);
			coroutineResult.success = coroutineResult2.success;
			if (!coroutineResult.success)
			{
				coroutineResult.isFinished = true;
				yield break;
			}
		}
		unpackMat2RectMap(OnCombinedTexturesCoroutineAtlasesAndRects);
		if (coroutineResult.success)
		{
			editorMethods?.GetMaterialPrimaryKeysIfAddressables(textureBakeResults);
		}
		textureBakeResults.resultType = MB2_TextureBakeResults.ResultType.atlas;
		textureBakeResults.resultMaterialsTexArray = new MB_MultiMaterialTexArray[0];
		textureBakeResults.doMultiMaterial = _doMultiMaterial;
		if (_doMultiMaterial)
		{
			textureBakeResults.resultMaterials = resultMaterials;
		}
		else
		{
			MB_MultiMaterial[] array = new MB_MultiMaterial[1]
			{
				new MB_MultiMaterial()
			};
			array[0].combinedMaterial = _resultMaterial;
			array[0].considerMeshUVs = _fixOutOfBoundsUVs;
			array[0].sourceMaterials = new List<Material>();
			for (int k = 0; k < textureBakeResults.materialsAndUVRects.Length; k++)
			{
				array[0].sourceMaterials.Add(textureBakeResults.materialsAndUVRects[k].material);
			}
			textureBakeResults.resultMaterials = array;
		}
		if (LOG_LEVEL >= MB2_LogLevel.info)
		{
			Debug.Log("Created Atlases");
		}
	}

	internal IEnumerator _CreateAtlasesCoroutineTextureArray(MB3_TextureCombiner combiner, ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		if (textureArrayOutputFormats == null || textureArrayOutputFormats.Length == 0)
		{
			Debug.LogError("No Texture Array Output Formats. There must be at least one entry.");
			coroutineResult.isFinished = true;
			yield break;
		}
		for (int i = 0; i < textureArrayOutputFormats.Length; i++)
		{
			if (!textureArrayOutputFormats[i].ValidateTextureImporterFormatsExistsForTextureFormats(editorMethods, i))
			{
				Debug.LogError("Could not map the selected texture format to a Texture Importer Format. Safest options are ARGB32, or RGB24.");
				coroutineResult.isFinished = true;
				yield break;
			}
		}
		for (int j = 0; j < resultMaterialsTexArray.Length; j++)
		{
			MB_MultiMaterialTexArray mB_MultiMaterialTexArray = resultMaterialsTexArray[j];
			if (mB_MultiMaterialTexArray.combinedMaterial == null)
			{
				Debug.LogError("Material is null for Texture Array Slice Configuration: " + j + ".");
				coroutineResult.isFinished = true;
				yield break;
			}
			List<MB_TexArraySlice> slices = mB_MultiMaterialTexArray.slices;
			for (int k = 0; k < slices.Count; k++)
			{
				for (int l = 0; l < slices[k].sourceMaterials.Count; l++)
				{
					MB_TexArraySliceRendererMatPair mB_TexArraySliceRendererMatPair = slices[k].sourceMaterials[l];
					if (mB_TexArraySliceRendererMatPair.sourceMaterial == null)
					{
						Debug.LogError("Source material is null for Texture Array Slice Configuration: " + j + " slice: " + k);
						coroutineResult.isFinished = true;
						yield break;
					}
					if (slices[k].considerMeshUVs && mB_TexArraySliceRendererMatPair.renderer == null)
					{
						Debug.LogError("Renderer is null for Texture Array Slice Configuration: " + j + " slice: " + k + ". If considerUVs is enabled then a renderer must be supplied for each source material. The same source material can be used multiple times.");
						coroutineResult.isFinished = true;
						yield break;
					}
				}
			}
		}
		int num = resultMaterialsTexArray.Length;
		MB_TextureArrayResultMaterial[] bakedMatsAndSlices = new MB_TextureArrayResultMaterial[num];
		for (int m = 0; m < bakedMatsAndSlices.Length; m++)
		{
			bakedMatsAndSlices[m] = new MB_TextureArrayResultMaterial();
			int count = resultMaterialsTexArray[m].slices.Count;
			MB_AtlasesAndRects[] array = (bakedMatsAndSlices[m].slices = new MB_AtlasesAndRects[count]);
			for (int n = 0; n < count; n++)
			{
				array[n] = new MB_AtlasesAndRects();
			}
		}
		for (int resMatIdx = 0; resMatIdx < bakedMatsAndSlices.Length; resMatIdx++)
		{
			yield return MB_TextureArrays._CreateAtlasesCoroutineSingleResultMaterial(resMatIdx, bakedMatsAndSlices[resMatIdx], resultMaterialsTexArray[resMatIdx], objsToMesh, combiner, textureArrayOutputFormats, resultMaterialsTexArray, customShaderProperties, texturePropNamesToIgnore, progressInfo, coroutineResult, saveAtlasesAsAssets, editorMethods, maxTimePerFrame);
			if (!coroutineResult.success)
			{
				yield break;
			}
		}
		if (coroutineResult.success)
		{
			unpackMat2RectMap(bakedMatsAndSlices);
			editorMethods?.GetMaterialPrimaryKeysIfAddressables(textureBakeResults);
			textureBakeResults.resultType = MB2_TextureBakeResults.ResultType.textureArray;
			textureBakeResults.resultMaterials = new MB_MultiMaterial[0];
			textureBakeResults.resultMaterialsTexArray = resultMaterialsTexArray;
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				Debug.Log("Created Texture2DArrays");
			}
		}
		else if (LOG_LEVEL >= MB2_LogLevel.info)
		{
			Debug.Log("Failed to create Texture2DArrays");
		}
	}

	private IEnumerator _CreateAtlasesCoroutine(ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		new MBVersionConcrete();
		OnCombinedTexturesCoroutineAtlasesAndRects = null;
		if (maxTimePerFrame <= 0f)
		{
			Debug.LogError("maxTimePerFrame must be a value greater than zero");
			coroutineResult.isFinished = true;
			yield break;
		}
		MB2_ValidationLevel validationLevel = (Application.isPlaying ? MB2_ValidationLevel.quick : MB2_ValidationLevel.robust);
		if (!MB3_MeshBakerRoot.DoCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, null, validationLevel))
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (_doMultiMaterial && !_ValidateResultMaterials())
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (resultType != MB2_TextureBakeResults.ResultType.textureArray && !_doMultiMaterial)
		{
			if (_resultMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				coroutineResult.isFinished = true;
				yield break;
			}
			Shader shader = _resultMaterial.shader;
			for (int i = 0; i < objsToMesh.Count; i++)
			{
				Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
				foreach (Material material in gOMaterials)
				{
					if (material != null && material.shader != shader)
					{
						Debug.LogWarning("Game object " + objsToMesh[i]?.ToString() + " does not use shader " + shader?.ToString() + " it may not have the required textures. If not small solid color textures will be generated.");
					}
				}
			}
		}
		MB3_TextureCombiner mB3_TextureCombiner = CreateAndConfigureTextureCombiner();
		mB3_TextureCombiner.saveAtlasesAsAssets = saveAtlasesAsAssets;
		OnCombinedTexturesCoroutineAtlasesAndRects = null;
		if (resultType == MB2_TextureBakeResults.ResultType.textureArray)
		{
			yield return _CreateAtlasesCoroutineTextureArray(mB3_TextureCombiner, progressInfo, coroutineResult, saveAtlasesAsAssets, editorMethods, maxTimePerFrame);
			if (!coroutineResult.success)
			{
				yield break;
			}
		}
		else
		{
			yield return _CreateAtlasesCoroutineAtlases(mB3_TextureCombiner, progressInfo, coroutineResult, saveAtlasesAsAssets, editorMethods, maxTimePerFrame);
			if (!coroutineResult.success)
			{
				yield break;
			}
		}
		MB3_MeshBakerCommon[] componentsInChildren = GetComponentsInChildren<MB3_MeshBakerCommon>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].textureBakeResults = textureBakeResults;
		}
		coroutineResult.isFinished = true;
	}

	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null)
	{
		MB_AtlasesAndRects[] array = null;
		try
		{
			_coroutineResult = new MB3_TextureCombiner.CreateAtlasesCoroutineResult();
			MB3_TextureCombiner.RunCorutineWithoutPause(CreateAtlasesCoroutine(progressInfo, _coroutineResult, saveAtlasesAsAssets, editorMethods, 1000f), 0);
			if (_coroutineResult.success && textureBakeResults != null)
			{
				array = OnCombinedTexturesCoroutineAtlasesAndRects;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + "\n" + ex.StackTrace.ToString());
		}
		finally
		{
			if (saveAtlasesAsAssets && array != null)
			{
				foreach (MB_AtlasesAndRects mB_AtlasesAndRects in array)
				{
					if (mB_AtlasesAndRects == null || mB_AtlasesAndRects.atlases == null)
					{
						continue;
					}
					for (int j = 0; j < mB_AtlasesAndRects.atlases.Length; j++)
					{
						if (mB_AtlasesAndRects.atlases[j] != null)
						{
							if (editorMethods != null)
							{
								editorMethods.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
							else
							{
								MB_Utility.Destroy(mB_AtlasesAndRects.atlases[j]);
							}
						}
					}
				}
			}
		}
		return array;
	}

	private void unpackMat2RectMap(MB_AtlasesAndRects[] rawResults)
	{
		List<MB_MaterialAndUVRect> list = new List<MB_MaterialAndUVRect>();
		for (int i = 0; i < rawResults.Length; i++)
		{
			List<MB_MaterialAndUVRect> mat2rect_map = rawResults[i].mat2rect_map;
			if (mat2rect_map != null)
			{
				for (int j = 0; j < mat2rect_map.Count; j++)
				{
					mat2rect_map[j].textureArraySliceIdx = -1;
					list.Add(mat2rect_map[j]);
				}
			}
		}
		textureBakeResults.version = MB2_TextureBakeResults.VERSION;
		textureBakeResults.materialsAndUVRects = list.ToArray();
	}

	internal void unpackMat2RectMap(MB_TextureArrayResultMaterial[] rawResults)
	{
		List<MB_MaterialAndUVRect> list = new List<MB_MaterialAndUVRect>();
		for (int i = 0; i < rawResults.Length; i++)
		{
			MB_AtlasesAndRects[] slices = rawResults[i].slices;
			for (int j = 0; j < slices.Length; j++)
			{
				List<MB_MaterialAndUVRect> mat2rect_map = slices[j].mat2rect_map;
				if (mat2rect_map != null)
				{
					for (int k = 0; k < mat2rect_map.Count; k++)
					{
						mat2rect_map[k].textureArraySliceIdx = j;
						list.Add(mat2rect_map[k]);
					}
				}
			}
		}
		textureBakeResults.version = MB2_TextureBakeResults.VERSION;
		textureBakeResults.materialsAndUVRects = list.ToArray();
	}

	public MB3_TextureCombiner CreateAndConfigureTextureCombiner()
	{
		return new MB3_TextureCombiner
		{
			LOG_LEVEL = LOG_LEVEL,
			atlasPadding = _atlasPadding,
			maxAtlasSize = _maxAtlasSize,
			maxAtlasHeightOverride = _maxAtlasHeightOverride,
			maxAtlasWidthOverride = _maxAtlasWidthOverride,
			useMaxAtlasHeightOverride = _useMaxAtlasHeightOverride,
			useMaxAtlasWidthOverride = _useMaxAtlasWidthOverride,
			customShaderPropNames = _customShaderProperties,
			fixOutOfBoundsUVs = _fixOutOfBoundsUVs,
			maxTilingBakeSize = _maxTilingBakeSize,
			packingAlgorithm = _packingAlgorithm,
			layerTexturePackerFastMesh = _layerTexturePackerFastMesh,
			resultType = _resultType,
			meshBakerTexturePackerForcePowerOfTwo = _meshBakerTexturePackerForcePowerOfTwo,
			resizePowerOfTwoTextures = _resizePowerOfTwoTextures,
			considerNonTextureProperties = _considerNonTextureProperties
		};
	}

	public static void ConfigureNewMaterialToMatchOld(Material newMat, Material original)
	{
		if (original == null)
		{
			Debug.LogWarning("Original material is null, could not copy properties to " + newMat?.ToString() + ". Setting shader to " + newMat.shader);
			return;
		}
		newMat.shader = original.shader;
		newMat.CopyPropertiesFromMaterial(original);
		ShaderTextureProperty[] shaderTexPropertyNames = MB3_TextureCombinerPipeline.shaderTexPropertyNames;
		for (int i = 0; i < shaderTexPropertyNames.Length; i++)
		{
			Vector2 one = Vector2.one;
			Vector2 zero = Vector2.zero;
			if (newMat.HasProperty(shaderTexPropertyNames[i].name))
			{
				newMat.SetTextureOffset(shaderTexPropertyNames[i].name, zero);
				newMat.SetTextureScale(shaderTexPropertyNames[i].name, one);
			}
		}
	}

	private string PrintSet(HashSet<Material> s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Material item in s)
		{
			stringBuilder.Append(item?.ToString() + ",");
		}
		return stringBuilder.ToString();
	}

	private bool _ValidateResultMaterials()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			if (!(objsToMesh[i] != null))
			{
				continue;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(objsToMesh[i]);
			for (int j = 0; j < gOMaterials.Length; j++)
			{
				if (gOMaterials[j] != null)
				{
					hashSet.Add(gOMaterials[j]);
				}
			}
		}
		if (resultMaterials.Length < 1)
		{
			Debug.LogError("Using multiple materials but there are no 'Source Material To Combined Mappings'. You need at least one.");
		}
		HashSet<Material> hashSet2 = new HashSet<Material>();
		for (int k = 0; k < resultMaterials.Length; k++)
		{
			for (int l = k + 1; l < resultMaterials.Length; l++)
			{
				if (resultMaterials[k].combinedMaterial == resultMaterials[l].combinedMaterial)
				{
					Debug.LogError($"Source To Combined Mapping: Submesh {k} and Submesh {l} use the same combined material. These should be different");
					return false;
				}
			}
			MB_MultiMaterial mB_MultiMaterial = resultMaterials[k];
			if (mB_MultiMaterial.combinedMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return false;
			}
			Shader shader = mB_MultiMaterial.combinedMaterial.shader;
			for (int m = 0; m < mB_MultiMaterial.sourceMaterials.Count; m++)
			{
				if (mB_MultiMaterial.sourceMaterials[m] == null)
				{
					Debug.LogError("There are null entries in the list of Source Materials");
					return false;
				}
				if (shader != mB_MultiMaterial.sourceMaterials[m].shader)
				{
					Debug.LogWarning("Source material " + mB_MultiMaterial.sourceMaterials[m]?.ToString() + " does not use shader " + shader?.ToString() + " it may not have the required textures. If not empty textures will be generated.");
				}
				if (hashSet2.Contains(mB_MultiMaterial.sourceMaterials[m]))
				{
					Debug.LogError("A Material " + mB_MultiMaterial.sourceMaterials[m]?.ToString() + " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique.");
					return false;
				}
				hashSet2.Add(mB_MultiMaterial.sourceMaterials[m]);
			}
		}
		if (hashSet.IsProperSubsetOf(hashSet2))
		{
			hashSet2.ExceptWith(hashSet);
			Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + PrintSet(hashSet2));
		}
		if (resultMaterials != null && resultMaterials.Length != 0 && hashSet2.IsProperSubsetOf(hashSet))
		{
			hashSet.ExceptWith(hashSet2);
			Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + PrintSet(hashSet));
			return false;
		}
		return true;
	}
}
