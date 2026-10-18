using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_TextureCombinerPipeline
{
	public struct CreateAtlasForProperty
	{
		public bool allTexturesAreNull;

		public bool allTexturesAreSame;

		public bool allNonTexturePropsAreSame;

		public bool allSrcMatsOmittedTextureProperty;

		public override string ToString()
		{
			return $"AllTexturesNull={allTexturesAreNull} areSame={allTexturesAreSame} nonTexPropsAreSame={allNonTexturePropsAreSame} allSrcMatsOmittedTextureProperty={allSrcMatsOmittedTextureProperty}";
		}
	}

	internal class TexturePipelineData
	{
		internal MB2_TextureBakeResults _textureBakeResults;

		internal int _atlasPadding_pix = 1;

		internal int _maxAtlasWidth = 1;

		internal int _maxAtlasHeight = 1;

		internal bool _useMaxAtlasHeightOverride;

		internal bool _useMaxAtlasWidthOverride;

		internal bool _resizePowerOfTwoTextures;

		internal bool _fixOutOfBoundsUVs;

		internal int _maxTilingBakeSize = 1024;

		internal bool _saveAtlasesAsAssets;

		internal MB2_PackingAlgorithmEnum _packingAlgorithm;

		internal int _layerTexturePackerFastV2 = -1;

		internal bool _meshBakerTexturePackerForcePowerOfTwo = true;

		internal List<ShaderTextureProperty> _customShaderPropNames = new List<ShaderTextureProperty>();

		internal bool _normalizeTexelDensity;

		internal bool _considerNonTextureProperties;

		internal bool doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize;

		internal ColorSpace colorSpace;

		internal MB3_TextureCombinerNonTextureProperties nonTexturePropertyBlender;

		internal List<MB_TexSet> distinctMaterialTextures;

		internal List<GameObject> allObjsToMesh;

		internal List<Material> allowedMaterialsFilter;

		internal List<ShaderTextureProperty> texPropertyNames;

		internal List<string> texPropNamesToIgnore;

		internal CreateAtlasForProperty[] allTexturesAreNullAndSameColor;

		internal MB2_TextureBakeResults.ResultType resultType;

		internal Material resultMaterial;

		internal int numAtlases
		{
			get
			{
				if (texPropertyNames != null)
				{
					return texPropertyNames.Count;
				}
				return 0;
			}
		}

		internal bool OnlyOneTextureInAtlasReuseTextures()
		{
			if (distinctMaterialTextures != null && distinctMaterialTextures.Count == 1 && distinctMaterialTextures[0].thisIsOnlyTexSetInAtlas && !_fixOutOfBoundsUVs && !_considerNonTextureProperties)
			{
				return true;
			}
			return false;
		}
	}

	public static bool USE_EXPERIMENTAL_HOIZONTALVERTICAL = true;

	public static ShaderTextureProperty[] shaderTexPropertyNames = new ShaderTextureProperty[34]
	{
		new ShaderTextureProperty("_MainTex", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_BaseMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_BaseColorMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_BumpMap", norm: true, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_Normal", norm: true, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_NormalMap", norm: true, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_BumpSpecMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_DecalTex", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_MaskMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_BentNormalMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_TangentMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_AnisotropyMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_SubsurfaceMaskMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_ThicknessMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_IridescenceThicknessMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_IridescenceMaskMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_SpecularColorMap", norm: false, isGamma: true, isNormalDontKnow: true),
		new ShaderTextureProperty("_EmissiveColorMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_DistortionVectorMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_TransmittanceColorMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_Detail", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_GlossMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_Illum", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_LightTextureB0", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_ParallaxMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_ShadowOffset", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_TranslucencyMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_SpecMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_SpecGlossMap", norm: false, isGamma: false, isNormalDontKnow: false),
		new ShaderTextureProperty("_TranspMap", norm: false, isGamma: false, isNormalDontKnow: true),
		new ShaderTextureProperty("_MetallicGlossMap", norm: false, isGamma: false, isNormalDontKnow: true),
		new ShaderTextureProperty("_OcclusionMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_EmissionMap", norm: false, isGamma: true, isNormalDontKnow: false),
		new ShaderTextureProperty("_DetailMask", norm: false, isGamma: false, isNormalDontKnow: false)
	};

	internal static bool _ShouldWeCreateAtlasForThisProperty(int propertyIndex, bool considerNonTextureProperties, CreateAtlasForProperty[] allTexturesAreNullAndSameColor)
	{
		CreateAtlasForProperty createAtlasForProperty = allTexturesAreNullAndSameColor[propertyIndex];
		if (considerNonTextureProperties)
		{
			if (!createAtlasForProperty.allNonTexturePropsAreSame || !createAtlasForProperty.allTexturesAreNull)
			{
				return true;
			}
			return false;
		}
		if (!createAtlasForProperty.allTexturesAreNull)
		{
			return true;
		}
		return false;
	}

	internal static bool _DoAnySrcMatsHaveProperty(int propertyIndex, CreateAtlasForProperty[] allTexturesAreNullAndSameColor)
	{
		return !allTexturesAreNullAndSameColor[propertyIndex].allSrcMatsOmittedTextureProperty;
	}

	internal static bool _CollectPropertyNames(TexturePipelineData data, MB2_LogLevel LOG_LEVEL)
	{
		return _CollectPropertyNames(data.texPropertyNames, data._customShaderPropNames, data.texPropNamesToIgnore, data.resultMaterial, LOG_LEVEL);
	}

	internal static bool _CollectPropertyNames(List<ShaderTextureProperty> texPropertyNames, List<ShaderTextureProperty> _customShaderPropNames, List<string> texPropsToIgnore, Material resultMaterial, MB2_LogLevel LOG_LEVEL)
	{
		int i;
		for (i = 0; i < texPropertyNames.Count; i++)
		{
			ShaderTextureProperty shaderTextureProperty = _customShaderPropNames.Find((ShaderTextureProperty x) => x.name.Equals(texPropertyNames[i].name));
			if (shaderTextureProperty != null)
			{
				_customShaderPropNames.Remove(shaderTextureProperty);
			}
		}
		if (resultMaterial == null)
		{
			UnityEngine.Debug.LogError("Please assign a result material. The combined mesh will use this material.");
			return false;
		}
		MBVersion.CollectPropertyNames(texPropertyNames, shaderTexPropertyNames, _customShaderPropNames, resultMaterial, LOG_LEVEL);
		for (int num = texPropertyNames.Count - 1; num >= 0; num--)
		{
			for (int j = 0; j < texPropsToIgnore.Count; j++)
			{
				if (texPropsToIgnore[j].Equals(texPropertyNames[num].name))
				{
					texPropertyNames.RemoveAt(num);
				}
			}
		}
		return true;
	}

	public static Texture GetTextureConsideringStandardShaderKeywords(string shaderName, Material mat, string propertyName)
	{
		if ((shaderName.Equals("Standard") || shaderName.Equals("Standard (Specular setup)") || shaderName.Equals("Standard (Roughness setup")) && propertyName.Equals("_EmissionMap"))
		{
			if (mat.IsKeywordEnabled("_EMISSION"))
			{
				return mat.GetTexture(propertyName);
			}
			return null;
		}
		return mat.GetTexture(propertyName);
	}

	internal virtual IEnumerator __Step1_CollectDistinctMatTexturesAndUsedObjects(ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult result, TexturePipelineData data, MB3_TextureCombiner combiner, MB2_EditorMethodsInterface textureEditorMethods, List<GameObject> usedObjsToMesh, MB2_LogLevel LOG_LEVEL)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		bool flag = false;
		Dictionary<int, MB_Utility.MeshAnalysisResult[]> dictionary = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
		for (int i = 0; i < data.allObjsToMesh.Count; i++)
		{
			GameObject gameObject = data.allObjsToMesh[i];
			progressInfo?.Invoke("Collecting textures for " + gameObject, (float)i / (float)data.allObjsToMesh.Count / 2f);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Collecting textures for object " + gameObject);
			}
			if (gameObject == null)
			{
				UnityEngine.Debug.LogError("The list of objects to mesh contained nulls.");
				result.success = false;
				yield break;
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			if (mesh == null)
			{
				UnityEngine.Debug.LogError("Object " + gameObject.name + " in the list of objects to mesh has no mesh.");
				result.success = false;
				yield break;
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(gameObject);
			if (gOMaterials.Length == 0)
			{
				UnityEngine.Debug.LogError("Object " + gameObject.name + " in the list of objects has no materials.");
				result.success = false;
				yield break;
			}
			if (!dictionary.TryGetValue(mesh.GetInstanceID(), out var value))
			{
				value = new MB_Utility.MeshAnalysisResult[mesh.subMeshCount];
				for (int j = 0; j < mesh.subMeshCount; j++)
				{
					MB_Utility.hasOutOfBoundsUVs(mesh, ref value[j], j);
					if (data._normalizeTexelDensity)
					{
						value[j].submeshArea = GetSubmeshArea(mesh, j);
					}
					if (data._fixOutOfBoundsUVs && !value[j].hasUVs)
					{
						value[j].uvRect = new Rect(0f, 0f, 1f, 1f);
						UnityEngine.Debug.LogWarning("Mesh for object " + gameObject?.ToString() + " has no UV channel but 'consider UVs' is enabled. Assuming UVs will be generated filling 0,0,1,1 rectangle.");
					}
				}
				dictionary.Add(mesh.GetInstanceID(), value);
			}
			if (data._fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.trace)
			{
				string[] obj = new string[8]
				{
					"Mesh Analysis for object ",
					gameObject?.ToString(),
					" numSubmesh=",
					value.Length.ToString(),
					" HasOBUV=",
					value[0].hasOutOfBoundsUVs.ToString(),
					" UVrectSubmesh0=",
					null
				};
				Rect uvRect = value[0].uvRect;
				obj[7] = uvRect.ToString();
				UnityEngine.Debug.Log(string.Concat(obj));
			}
			for (int k = 0; k < gOMaterials.Length; k++)
			{
				progressInfo?.Invoke($"Collecting textures for {gameObject} submesh {k}", (float)i / (float)data.allObjsToMesh.Count / 2f);
				Material material = gOMaterials[k];
				if (data.allowedMaterialsFilter != null && !data.allowedMaterialsFilter.Contains(material))
				{
					continue;
				}
				flag = flag || value[k].hasOutOfBoundsUVs;
				if (material.name.Contains("(Instance)"))
				{
					UnityEngine.Debug.LogWarning("The sharedMaterial on object " + gameObject.name + " has been 'Instanced'. This was probably caused by a script accessing the meshRender.material property in the editor.  The material to UV Rectangle mapping may be incorrect. To fix this recreate the object from its prefab or re-assign its material from the correct asset.");
				}
				if (data._fixOutOfBoundsUVs && !MB_Utility.AreAllSharedMaterialsDistinct(gOMaterials) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Object " + gameObject.name + " uses the same material on multiple submeshes. This may generate strange resultAtlasesAndRects especially when used with Consider Mesh UVs. Try duplicating the material.");
				}
				MeshBakerMaterialTexture[] array = new MeshBakerMaterialTexture[data.texPropertyNames.Count];
				for (int l = 0; l < data.texPropertyNames.Count; l++)
				{
					Texture texture = null;
					Vector2 scale = Vector2.one;
					Vector2 offset = Vector2.zero;
					float texelDens = 0f;
					int isImportedAsNormalMap = 0;
					if (material.HasProperty(data.texPropertyNames[l].name))
					{
						Texture textureConsideringStandardShaderKeywords = GetTextureConsideringStandardShaderKeywords(data.resultMaterial.shader.name, material, data.texPropertyNames[l].name);
						if (textureConsideringStandardShaderKeywords != null)
						{
							if (!(textureConsideringStandardShaderKeywords is Texture2D))
							{
								UnityEngine.Debug.LogError("Object '" + gameObject.name + "' in the list of objects to mesh uses a Texture that is not a Texture2D. Cannot build atlases with this object.");
								result.success = false;
								yield break;
							}
							texture = textureConsideringStandardShaderKeywords;
							TextureFormat format = ((Texture2D)texture).format;
							bool flag2 = false;
							if (!Application.isPlaying && textureEditorMethods != null)
							{
								flag2 = textureEditorMethods.IsNormalMap((Texture2D)texture);
								isImportedAsNormalMap = ((!flag2) ? 1 : (-1));
							}
							if ((format != TextureFormat.ARGB32 && format != TextureFormat.RGBA32 && format != TextureFormat.BGRA32 && format != TextureFormat.RGB24 && format != TextureFormat.Alpha8) || flag2)
							{
								if (Application.isPlaying && data.resultType == MB2_TextureBakeResults.ResultType.atlas && data._packingAlgorithm != MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast && data._packingAlgorithm != MB2_PackingAlgorithmEnum.MeshBakerTexturePaker_Fast_V2_Beta)
								{
									UnityEngine.Debug.LogError("Object " + gameObject.name + " in the list of objects to mesh uses Texture " + texture.name + " uses format " + format.ToString() + " that is not in: ARGB32, RGBA32, BGRA32, RGB24, Alpha8 or DXT. These textures cannot be resized at runtime. Try changing texture format. If format says 'compressed' try changing it to 'truecolor'");
									result.success = false;
									yield break;
								}
								texture = (Texture2D)material.GetTexture(data.texPropertyNames[l].name);
							}
						}
						if (texture != null && data._normalizeTexelDensity)
						{
							texelDens = ((value[l].submeshArea != 0f) ? ((float)(texture.width * texture.height) / value[l].submeshArea) : 0f);
						}
						GetMaterialScaleAndOffset(material, data.texPropertyNames[l].name, out offset, out scale);
					}
					array[l] = new MeshBakerMaterialTexture(texture, offset, scale, texelDens, isImportedAsNormalMap);
				}
				data.nonTexturePropertyBlender.CollectAverageValuesOfNonTextureProperties(data.resultMaterial, material);
				Vector2 vector = new Vector2(value[k].uvRect.width, value[k].uvRect.height);
				Vector2 vector2 = new Vector2(value[k].uvRect.x, value[k].uvRect.y);
				MB_TextureTilingTreatment treatment = MB_TextureTilingTreatment.none;
				if (data._fixOutOfBoundsUVs)
				{
					treatment = MB_TextureTilingTreatment.considerUVs;
				}
				MB_TexSet setOfTexs = new MB_TexSet(array, vector2, vector, treatment);
				MatAndTransformToMerged item = new MatAndTransformToMerged(new DRect(vector2, vector), data._fixOutOfBoundsUVs, material);
				setOfTexs.matsAndGOs.mats.Add(item);
				MB_TexSet mB_TexSet = data.distinctMaterialTextures.Find((MB_TexSet x) => x.IsEqual(setOfTexs, data._fixOutOfBoundsUVs, data.nonTexturePropertyBlender));
				if (mB_TexSet != null)
				{
					setOfTexs = mB_TexSet;
				}
				else
				{
					data.distinctMaterialTextures.Add(setOfTexs);
				}
				if (!setOfTexs.matsAndGOs.mats.Contains(item))
				{
					setOfTexs.matsAndGOs.mats.Add(item);
				}
				if (!setOfTexs.matsAndGOs.gos.Contains(gameObject))
				{
					setOfTexs.matsAndGOs.gos.Add(gameObject);
					if (!usedObjsToMesh.Contains(gameObject))
					{
						usedObjsToMesh.Add(gameObject);
					}
				}
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log($"Step1_CollectDistinctTextures collected {data.distinctMaterialTextures.Count} sets of textures fixOutOfBoundsUV={data._fixOutOfBoundsUVs} considerNonTextureProperties={data._considerNonTextureProperties}");
		}
		if (data.distinctMaterialTextures.Count == 0)
		{
			string[] array2 = new string[data.allowedMaterialsFilter.Count];
			for (int m = 0; m < array2.Length; m++)
			{
				array2[m] = data.allowedMaterialsFilter[m].name;
			}
			string text = string.Join(", ", array2);
			UnityEngine.Debug.LogError("None of the materials on the objects to combine matched any of the allowed materials for submesh with result material: " + data.resultMaterial?.ToString() + " allowedMaterials: " + text + ". Do any of the source objects use the allowed materials?");
			result.success = false;
			yield break;
		}
		MB3_TextureCombinerMerging mB3_TextureCombinerMerging = new MB3_TextureCombinerMerging(data._considerNonTextureProperties, data.nonTexturePropertyBlender, data._fixOutOfBoundsUVs, LOG_LEVEL);
		mB3_TextureCombinerMerging.MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(data.distinctMaterialTextures);
		if (data.doMergeDistinctMaterialTexturesThatWouldExceedAtlasSize)
		{
			mB3_TextureCombinerMerging.MergeDistinctMaterialTexturesThatWouldExceedMaxAtlasSizeAndCalcMaterialSubrects(data.distinctMaterialTextures, Mathf.Max(data._maxAtlasHeight, data._maxAtlasWidth));
		}
		for (int n = 0; n < data.texPropertyNames.Count; n++)
		{
			ShaderTextureProperty shaderTextureProperty = data.texPropertyNames[n];
			if (shaderTextureProperty.isNormalDontKnow)
			{
				int num = 0;
				for (int num2 = 0; num2 < data.distinctMaterialTextures.Count; num2++)
				{
					MeshBakerMaterialTexture meshBakerMaterialTexture = data.distinctMaterialTextures[num2].ts[n];
					num += meshBakerMaterialTexture.isImportedAsNormalMap;
				}
				shaderTextureProperty.isNormalMap = num < 0;
				shaderTextureProperty.isNormalDontKnow = false;
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time Step1_CollectDistinctTextures " + stopwatch.ElapsedMilliseconds.ToString("f5"));
		}
	}

	private static CreateAtlasForProperty[] CalculateAllTexturesAreNullAndSameColor(TexturePipelineData data, MB2_LogLevel LOG_LEVEL)
	{
		CreateAtlasForProperty[] array = new CreateAtlasForProperty[data.texPropertyNames.Count];
		for (int i = 0; i < data.texPropertyNames.Count; i++)
		{
			MeshBakerMaterialTexture meshBakerMaterialTexture = data.distinctMaterialTextures[0].ts[i];
			Color color = Color.black;
			if (data._considerNonTextureProperties)
			{
				color = data.nonTexturePropertyBlender.GetColorAsItWouldAppearInAtlasIfNoTexture(data.distinctMaterialTextures[0].matsAndGOs.mats[0].mat, data.texPropertyNames[i]);
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = true;
			for (int j = 0; j < data.distinctMaterialTextures.Count; j++)
			{
				MB_TexSet mB_TexSet = data.distinctMaterialTextures[j];
				if (!mB_TexSet.ts[i].isNull)
				{
					num++;
				}
				if (meshBakerMaterialTexture.AreTexturesEqual(mB_TexSet.ts[i]))
				{
					num2++;
				}
				if (data._considerNonTextureProperties && data.nonTexturePropertyBlender.GetColorAsItWouldAppearInAtlasIfNoTexture(mB_TexSet.matsAndGOs.mats[0].mat, data.texPropertyNames[i]) == color)
				{
					num3++;
				}
				for (int k = 0; k < mB_TexSet.matsAndGOs.mats.Count; k++)
				{
					flag = !mB_TexSet.matsAndGOs.mats[k].mat.HasProperty(data.texPropertyNames[i].name);
				}
			}
			array[i].allTexturesAreNull = num == 0;
			array[i].allTexturesAreSame = num2 == data.distinctMaterialTextures.Count;
			array[i].allNonTexturePropsAreSame = num3 == data.distinctMaterialTextures.Count;
			array[i].allSrcMatsOmittedTextureProperty |= flag;
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log($"AllTexturesAreNullAndSameColor prop: {data.texPropertyNames[i].name} createAtlas:{_ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, array)}  val:{array[i]}");
			}
		}
		return array;
	}

	internal virtual IEnumerator CalculateIdealSizesForTexturesInAtlasAndPadding(ProgressUpdateDelegate progressInfo, MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult result, TexturePipelineData data, MB3_TextureCombiner combiner, MB2_EditorMethodsInterface textureEditorMethods, MB2_LogLevel LOG_LEVEL)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		MeshBakerMaterialTexture.readyToBuildAtlases = true;
		data.allTexturesAreNullAndSameColor = CalculateAllTexturesAreNullAndSameColor(data, LOG_LEVEL);
		if (MB3_MeshCombiner.EVAL_VERSION)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < data.allTexturesAreNullAndSameColor.Length; i++)
			{
				if (_ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
				{
					if ((data.texPropertyNames[i].name.Equals("_Albedo") || data.texPropertyNames[i].name.Equals("_MainTex") || data.texPropertyNames[i].name.Equals("_BaseMap") || data.texPropertyNames[i].name.Equals("_BaseColorMap")) && list.Count < 2)
					{
						list.Add(i);
					}
					if ((data.texPropertyNames[i].name.Equals("_BumpMap") || data.texPropertyNames[i].name.Equals("_Normal") || data.texPropertyNames[i].name.Equals("_NormalMap") || data.texPropertyNames[i].name.Equals("_BentNormalMap")) && list.Count < 2)
					{
						list.Add(i);
					}
				}
			}
			List<string> list2 = new List<string>();
			List<int> list3 = new List<int>();
			for (int j = 0; j < data.allTexturesAreNullAndSameColor.Length; j++)
			{
				if (_ShouldWeCreateAtlasForThisProperty(j, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor) && list.Count >= 2 && !list.Contains(j))
				{
					list2.Add(data.texPropertyNames[j].name);
					list3.Add(j);
				}
			}
			for (int k = 0; k < list3.Count; k++)
			{
				data.allTexturesAreNullAndSameColor[list3[k]].allTexturesAreNull = true;
				data.allTexturesAreNullAndSameColor[list3[k]].allTexturesAreSame = true;
				data.allTexturesAreNullAndSameColor[list3[k]].allNonTexturePropsAreSame = true;
			}
			if (list2.Count > 0)
			{
				UnityEngine.Debug.LogError("The free version of Mesh Baker will generate a maximum of two atlases per combined material. The source materials had more than two properties with textures. Atlases will not be generated for: " + string.Join(",", list2.ToArray()));
			}
		}
		int num = data._atlasPadding_pix;
		if (data.distinctMaterialTextures.Count == 1 && !data._fixOutOfBoundsUVs && !data._considerNonTextureProperties)
		{
			if (LOG_LEVEL >= MB2_LogLevel.info)
			{
				UnityEngine.Debug.Log("All objects use the same textures in this set of atlases. Original textures will be reused instead of creating atlases.");
			}
			num = 0;
			data.distinctMaterialTextures[0].SetThisIsOnlyTexSetInAtlasTrue();
			data.distinctMaterialTextures[0].SetTilingTreatmentAndAdjustEncapsulatingSamplingRect(MB_TextureTilingTreatment.edgeToEdgeXY);
		}
		for (int l = 0; l < data.distinctMaterialTextures.Count; l++)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Calculating ideal sizes for texSet TexSet " + l + " of " + data.distinctMaterialTextures.Count);
			}
			MB_TexSet mB_TexSet = data.distinctMaterialTextures[l];
			mB_TexSet.idealWidth_pix = 1;
			mB_TexSet.idealHeight_pix = 1;
			int num2 = 1;
			int num3 = 1;
			for (int m = 0; m < data.texPropertyNames.Count; m++)
			{
				if (!_ShouldWeCreateAtlasForThisProperty(m, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
				{
					continue;
				}
				MeshBakerMaterialTexture meshBakerMaterialTexture = mB_TexSet.ts[m];
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Calculating ideal size for texSet {l} property {data.texPropertyNames[m].name}");
				}
				if (!meshBakerMaterialTexture.matTilingRect.size.Equals(Vector2.one) && data.distinctMaterialTextures.Count > 1 && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Texture " + meshBakerMaterialTexture.GetTexName() + "is tiled by " + meshBakerMaterialTexture.matTilingRect.size.ToString() + " tiling will be baked into a texture with maxSize:" + data._maxTilingBakeSize);
				}
				if (!mB_TexSet.obUVscale.Equals(Vector2.one) && data.distinctMaterialTextures.Count > 1 && data._fixOutOfBoundsUVs && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Texture " + meshBakerMaterialTexture.GetTexName() + " has out of bounds UVs that effectively tile by " + mB_TexSet.obUVscale.ToString() + " tiling will be baked into a texture with maxSize:" + data._maxTilingBakeSize);
				}
				if (meshBakerMaterialTexture.isNull)
				{
					Vector2 adjustedForScaleAndOffset2Dimensions = GetAdjustedForScaleAndOffset2Dimensions(meshBakerMaterialTexture, mB_TexSet.obUVoffset, mB_TexSet.obUVscale, data, LOG_LEVEL);
					if ((int)(adjustedForScaleAndOffset2Dimensions.x * adjustedForScaleAndOffset2Dimensions.y) > num2 * num3)
					{
						if (LOG_LEVEL >= MB2_LogLevel.trace)
						{
							string[] obj = new string[8]
							{
								"    matTex ",
								meshBakerMaterialTexture.GetTexName(),
								" ",
								null,
								null,
								null,
								null,
								null
							};
							Vector2 vector = adjustedForScaleAndOffset2Dimensions;
							obj[3] = vector.ToString();
							obj[4] = " has a bigger size than ";
							obj[5] = num2.ToString();
							obj[6] = " ";
							obj[7] = num3.ToString();
							UnityEngine.Debug.Log(string.Concat(obj));
						}
						num2 = (int)adjustedForScaleAndOffset2Dimensions.x;
						num3 = (int)adjustedForScaleAndOffset2Dimensions.y;
					}
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						UnityEngine.Debug.Log($"No source texture creating a 16x16 texture for {data.texPropertyNames[m].name} texSet {l} srcMat {mB_TexSet.matsAndGOs.mats[0].GetMaterialName()}");
					}
				}
				if (meshBakerMaterialTexture.isNull)
				{
					continue;
				}
				Vector2 adjustedForScaleAndOffset2Dimensions2 = GetAdjustedForScaleAndOffset2Dimensions(meshBakerMaterialTexture, mB_TexSet.obUVoffset, mB_TexSet.obUVscale, data, LOG_LEVEL);
				if ((int)(adjustedForScaleAndOffset2Dimensions2.x * adjustedForScaleAndOffset2Dimensions2.y) > num2 * num3)
				{
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						string[] obj2 = new string[8]
						{
							"    matTex ",
							meshBakerMaterialTexture.GetTexName(),
							" ",
							null,
							null,
							null,
							null,
							null
						};
						Vector2 vector = adjustedForScaleAndOffset2Dimensions2;
						obj2[3] = vector.ToString();
						obj2[4] = " has a bigger size than ";
						obj2[5] = num2.ToString();
						obj2[6] = " ";
						obj2[7] = num3.ToString();
						UnityEngine.Debug.Log(string.Concat(obj2));
					}
					num2 = (int)adjustedForScaleAndOffset2Dimensions2.x;
					num3 = (int)adjustedForScaleAndOffset2Dimensions2.y;
				}
			}
			if (data._resizePowerOfTwoTextures)
			{
				if (num2 <= num * 5)
				{
					UnityEngine.Debug.LogWarning(string.Format("Some of the textures have widths close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with widths this small.", mB_TexSet.ToString()));
				}
				if (num3 <= num * 5)
				{
					UnityEngine.Debug.LogWarning(string.Format("Some of the textures have heights close to the size of the padding. It is not recommended to use _resizePowerOfTwoTextures with heights this small.", mB_TexSet.ToString()));
				}
				if (IsPowerOfTwo(num2))
				{
					num2 -= num * 2;
				}
				if (IsPowerOfTwo(num3))
				{
					num3 -= num * 2;
				}
				if (num2 < 1)
				{
					num2 = 1;
				}
				if (num3 < 1)
				{
					num3 = 1;
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("    Ideal size is " + num2 + " " + num3);
			}
			mB_TexSet.idealWidth_pix = num2;
			mB_TexSet.idealHeight_pix = num3;
		}
		data._atlasPadding_pix = num;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time Step2 Calculate Ideal Sizes part1: " + stopwatch.Elapsed);
		}
		yield break;
	}

	internal virtual AtlasPackingResult[] RunTexturePackerOnly(TexturePipelineData data, bool doSplitIntoMultiAtlasIfTooBig, MB_AtlasesAndRects resultAtlasesAndRects, MB_ITextureCombinerPacker texturePacker, MB2_LogLevel LOG_LEVEL)
	{
		AtlasPackingResult[] array = texturePacker.CalculateAtlasRectangles(data, doSplitIntoMultiAtlasIfTooBig, LOG_LEVEL);
		FillAtlasPackingResultAuxillaryData(data, array);
		Texture2D[] atlases = new Texture2D[data.texPropertyNames.Count];
		if (!doSplitIntoMultiAtlasIfTooBig)
		{
			FillResultAtlasesAndRects(data, array[0], resultAtlasesAndRects, atlases);
		}
		return array;
	}

	internal virtual MB_ITextureCombinerPacker CreatePacker(bool onlyOneTextureInAtlasReuseTextures, MB2_PackingAlgorithmEnum packingAlgorithm)
	{
		if (onlyOneTextureInAtlasReuseTextures)
		{
			return new MB3_TextureCombinerPackerOneTextureInAtlas();
		}
		switch (packingAlgorithm)
		{
		case MB2_PackingAlgorithmEnum.UnitysPackTextures:
			return new MB3_TextureCombinerPackerUnity();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal:
			if (USE_EXPERIMENTAL_HOIZONTALVERTICAL)
			{
				return new MB3_TextureCombinerPackerMeshBakerHorizontalVertical(MB3_TextureCombinerPackerMeshBakerHorizontalVertical.AtlasDirection.horizontal);
			}
			return new MB3_TextureCombinerPackerMeshBaker();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical:
			if (USE_EXPERIMENTAL_HOIZONTALVERTICAL)
			{
				return new MB3_TextureCombinerPackerMeshBakerHorizontalVertical(MB3_TextureCombinerPackerMeshBakerHorizontalVertical.AtlasDirection.vertical);
			}
			return new MB3_TextureCombinerPackerMeshBaker();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker:
			return new MB3_TextureCombinerPackerMeshBaker();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePaker_Fast_V2_Beta:
			return new MB3_TextureCombinerPackerMeshBakerFastV2();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast:
			return new MB3_TextureCombinerPackerMeshBakerFast();
		default:
			UnityEngine.Debug.LogError("Unknown texture packer type. " + packingAlgorithm.ToString() + " This should never happen.");
			return null;
		}
	}

	internal virtual IEnumerator __Step3_BuildAndSaveAtlasesAndStoreResults(MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult result, ProgressUpdateDelegate progressInfo, TexturePipelineData data, MB3_TextureCombiner combiner, MB_ITextureCombinerPacker packer, AtlasPackingResult atlasPackingResult, MB2_EditorMethodsInterface textureEditorMethods, MB_AtlasesAndRects resultAtlasesAndRects, StringBuilder report, MB2_LogLevel LOG_LEVEL)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();
		GC.Collect();
		Texture2D[] atlases = new Texture2D[data.numAtlases];
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("time Step 3 Create And Save Atlases part 1 " + sw.Elapsed);
		}
		MB_TextureCombinerSRPCustom.ConfigureMaterialKeywordsIfNecessary(data);
		yield return packer.CreateAtlases(progressInfo, data, combiner, atlasPackingResult, atlases, textureEditorMethods, LOG_LEVEL);
		float num = sw.ElapsedMilliseconds;
		data.nonTexturePropertyBlender.AdjustNonTextureProperties(data.resultMaterial, data.texPropertyNames, textureEditorMethods);
		if (data.distinctMaterialTextures.Count > 0)
		{
			data.distinctMaterialTextures[0].AdjustResultMaterialNonTextureProperties(data.resultMaterial, data.texPropertyNames);
		}
		progressInfo?.Invoke("Building Report", 0.7f);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("---- Atlases ------");
		for (int i = 0; i < data.numAtlases; i++)
		{
			if (atlases[i] != null)
			{
				stringBuilder.AppendLine("Created Atlas For: " + data.texPropertyNames[i].name + " h=" + atlases[i].height + " w=" + atlases[i].width);
			}
			else if (!_ShouldWeCreateAtlasForThisProperty(i, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
			{
				stringBuilder.AppendLine("Did not create atlas for " + data.texPropertyNames[i].name + " because all source textures were null.");
			}
		}
		report.Append(stringBuilder.ToString());
		FillResultAtlasesAndRects(data, atlasPackingResult, resultAtlasesAndRects, atlases);
		progressInfo?.Invoke("Restoring Texture Formats & Read Flags", 0.8f);
		combiner._destroyAllTemporaryTextures();
		textureEditorMethods?.RestoreReadFlagsAndFormats(progressInfo);
		if (report != null && LOG_LEVEL >= MB2_LogLevel.info)
		{
			UnityEngine.Debug.Log(report.ToString());
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Time Step 3 Create And Save Atlases part 3 " + ((float)sw.ElapsedMilliseconds - num).ToString("f5"));
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("Total time Step 3 Create And Save Atlases " + sw.Elapsed);
		}
	}

	private void FillAtlasPackingResultAuxillaryData(TexturePipelineData data, AtlasPackingResult[] atlasPackingResults)
	{
		foreach (AtlasPackingResult atlasPackingResult in atlasPackingResults)
		{
			List<MB_MaterialAndUVRect> list = new List<MB_MaterialAndUVRect>();
			for (int j = 0; j < atlasPackingResult.srcImgIdxs.Length; j++)
			{
				int index = atlasPackingResult.srcImgIdxs[j];
				MB_TexSet mB_TexSet = data.distinctMaterialTextures[index];
				List<MatAndTransformToMerged> mats = mB_TexSet.matsAndGOs.mats;
				mB_TexSet.GetRectsForTextureBakeResults(out var allPropsUseSameTiling_encapsulatingSamplingRect, out var propsUseDifferntTiling_obUVRect);
				for (int k = 0; k < mats.Count; k++)
				{
					Rect materialTilingRectForTextureBakerResults = mB_TexSet.GetMaterialTilingRectForTextureBakerResults(k);
					MB_MaterialAndUVRect mB_MaterialAndUVRect = new MB_MaterialAndUVRect(mats[k].mat, atlasPackingResult.rects[j], mB_TexSet.allTexturesUseSameMatTiling, materialTilingRectForTextureBakerResults, allPropsUseSameTiling_encapsulatingSamplingRect, propsUseDifferntTiling_obUVRect, mB_TexSet.tilingTreatment, mats[k].objName);
					mB_MaterialAndUVRect.objectsThatUse = new List<GameObject>(mB_TexSet.matsAndGOs.gos);
					list.Add(mB_MaterialAndUVRect);
				}
			}
			atlasPackingResult.data = list;
		}
	}

	private void FillResultAtlasesAndRects(TexturePipelineData data, AtlasPackingResult atlasPackingResult, MB_AtlasesAndRects resultAtlasesAndRects, Texture2D[] atlases)
	{
		List<MB_MaterialAndUVRect> list = new List<MB_MaterialAndUVRect>();
		for (int i = 0; i < data.distinctMaterialTextures.Count; i++)
		{
			MB_TexSet mB_TexSet = data.distinctMaterialTextures[i];
			List<MatAndTransformToMerged> mats = mB_TexSet.matsAndGOs.mats;
			mB_TexSet.GetRectsForTextureBakeResults(out var allPropsUseSameTiling_encapsulatingSamplingRect, out var propsUseDifferntTiling_obUVRect);
			for (int j = 0; j < mats.Count; j++)
			{
				Rect materialTilingRectForTextureBakerResults = mB_TexSet.GetMaterialTilingRectForTextureBakerResults(j);
				MB_MaterialAndUVRect item = new MB_MaterialAndUVRect(mats[j].mat, atlasPackingResult.rects[i], mB_TexSet.allTexturesUseSameMatTiling, materialTilingRectForTextureBakerResults, allPropsUseSameTiling_encapsulatingSamplingRect, propsUseDifferntTiling_obUVRect, mB_TexSet.tilingTreatment, mats[j].objName);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		resultAtlasesAndRects.atlases = atlases;
		resultAtlasesAndRects.texPropertyNames = ShaderTextureProperty.GetNames(data.texPropertyNames);
		resultAtlasesAndRects.mat2rect_map = list;
	}

	internal virtual StringBuilder GenerateReport(TexturePipelineData data)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (data.numAtlases > 0)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Report");
			if (data.texPropNamesToIgnore.Count > 0)
			{
				stringBuilder.Append("Ignoring texture properties: ");
				for (int i = 0; i < data.texPropNamesToIgnore.Count; i++)
				{
					stringBuilder.Append(data.texPropNamesToIgnore[i]);
					stringBuilder.Append(", ");
				}
				stringBuilder.AppendLine();
			}
			for (int j = 0; j < data.distinctMaterialTextures.Count; j++)
			{
				MB_TexSet mB_TexSet = data.distinctMaterialTextures[j];
				stringBuilder.AppendLine("----------");
				stringBuilder.Append("This set of textures will be a rectangle in the atlas. It will be resized to:" + mB_TexSet.idealWidth_pix + "x" + mB_TexSet.idealHeight_pix + "\n");
				for (int k = 0; k < mB_TexSet.ts.Length; k++)
				{
					if (!mB_TexSet.ts[k].isNull)
					{
						stringBuilder.Append("   [" + data.texPropertyNames[k].name + " " + mB_TexSet.ts[k].GetTexName() + " " + mB_TexSet.ts[k].width + "x" + mB_TexSet.ts[k].height + "]");
						if (mB_TexSet.ts[k].matTilingRect.size != Vector2.one || mB_TexSet.ts[k].matTilingRect.min != Vector2.zero)
						{
							stringBuilder.AppendFormat(" material scale {0} offset{1} ", mB_TexSet.ts[k].matTilingRect.size.ToString("G4"), mB_TexSet.ts[k].matTilingRect.min.ToString("G4"));
						}
						if (mB_TexSet.obUVscale != Vector2.one || mB_TexSet.obUVoffset != Vector2.zero)
						{
							stringBuilder.AppendFormat(" obUV scale {0} offset{1} ", mB_TexSet.obUVscale.ToString("G4"), mB_TexSet.obUVoffset.ToString("G4"));
						}
						stringBuilder.AppendLine("");
					}
					else
					{
						stringBuilder.Append("   [" + data.texPropertyNames[k].name + " null ");
						if (!_ShouldWeCreateAtlasForThisProperty(k, data._considerNonTextureProperties, data.allTexturesAreNullAndSameColor))
						{
							stringBuilder.Append("no atlas will be created all textures null]\n");
						}
						else
						{
							stringBuilder.AppendFormat("a 16x16 texture will be created]\n");
						}
					}
				}
				stringBuilder.AppendLine("");
				stringBuilder.Append("Materials using this rectangle:");
				for (int l = 0; l < mB_TexSet.matsAndGOs.mats.Count; l++)
				{
					stringBuilder.Append(mB_TexSet.matsAndGOs.mats[l].mat.name + ", ");
				}
				stringBuilder.AppendLine("");
			}
		}
		return stringBuilder;
	}

	internal static MB2_TexturePacker CreateTexturePacker(MB2_PackingAlgorithmEnum _packingAlgorithm)
	{
		switch (_packingAlgorithm)
		{
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker:
			return new MB2_TexturePackerRegular();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Fast:
			return new MB2_TexturePackerRegular();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePaker_Fast_V2_Beta:
			return new MB2_TexturePackerRegular();
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Horizontal:
			return new MB2_TexturePackerHorizontalVert
			{
				packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.horizontal
			};
		case MB2_PackingAlgorithmEnum.MeshBakerTexturePacker_Vertical:
			return new MB2_TexturePackerHorizontalVert
			{
				packingOrientation = MB2_TexturePackerHorizontalVert.TexturePackingOrientation.vertical
			};
		default:
			UnityEngine.Debug.LogError("packing algorithm must be one of the MeshBaker options to create a Texture Packer");
			return null;
		}
	}

	internal static Vector2 GetAdjustedForScaleAndOffset2Dimensions(MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, TexturePipelineData data, MB2_LogLevel LOG_LEVEL)
	{
		if (source.matTilingRect.x == 0.0 && source.matTilingRect.y == 0.0 && source.matTilingRect.width == 1.0 && source.matTilingRect.height == 1.0)
		{
			if (!data._fixOutOfBoundsUVs)
			{
				return new Vector2(source.width, source.height);
			}
			if (obUVoffset.x == 0f && obUVoffset.y == 0f && obUVscale.x == 1f && obUVscale.y == 1f)
			{
				return new Vector2(source.width, source.height);
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			string[] obj = new string[6]
			{
				"GetAdjustedForScaleAndOffset2Dimensions: ",
				source.GetTexName(),
				" ",
				null,
				null,
				null
			};
			Vector2 vector = obUVoffset;
			obj[3] = vector.ToString();
			obj[4] = " ";
			vector = obUVscale;
			obj[5] = vector.ToString();
			UnityEngine.Debug.Log(string.Concat(obj));
		}
		Rect rect = source.GetEncapsulatingSamplingRect().GetRect();
		float num = rect.width * (float)source.width;
		float num2 = rect.height * (float)source.height;
		if (num > (float)data._maxTilingBakeSize)
		{
			num = data._maxTilingBakeSize;
		}
		if (num2 > (float)data._maxTilingBakeSize)
		{
			num2 = data._maxTilingBakeSize;
		}
		if (num < 1f)
		{
			num = 1f;
		}
		if (num2 < 1f)
		{
			num2 = 1f;
		}
		return new Vector2(num, num2);
	}

	internal static Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (float)(int)c.a * 2f - 1f;
		zero.y = (float)(int)c.g * 2f - 1f;
		zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
		Color32 result = default(Color32);
		result.a = 1;
		result.r = (byte)((zero.x + 1f) * 0.5f);
		result.g = (byte)((zero.y + 1f) * 0.5f);
		result.b = (byte)((zero.z + 1f) * 0.5f);
		return result;
	}

	internal static void GetMaterialScaleAndOffset(Material mat, string propertyName, out Vector2 offset, out Vector2 scale)
	{
		if (mat == null)
		{
			UnityEngine.Debug.LogError("Material was null. Should never happen.");
			offset = Vector2.zero;
			scale = Vector2.one;
		}
		else
		{
			MB3_ShadersThatShareTiling.GetScaleAndOffsetForTextureProp(mat, propertyName, out offset, out scale);
		}
	}

	internal static float GetSubmeshArea(Mesh m, int submeshIdx)
	{
		if (submeshIdx >= m.subMeshCount || submeshIdx < 0)
		{
			return 0f;
		}
		Vector3[] vertices = m.vertices;
		int[] indices = m.GetIndices(submeshIdx);
		float num = 0f;
		for (int i = 0; i < indices.Length; i += 3)
		{
			Vector3 vector = vertices[indices[i]];
			Vector3 vector2 = vertices[indices[i + 1]];
			Vector3 vector3 = vertices[indices[i + 2]];
			num += Vector3.Cross(vector2 - vector, vector3 - vector).magnitude / 2f;
		}
		return num;
	}

	internal static bool IsPowerOfTwo(int x)
	{
		return (x & (x - 1)) == 0;
	}
}
