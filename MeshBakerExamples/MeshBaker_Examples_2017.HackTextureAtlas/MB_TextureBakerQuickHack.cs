using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

namespace MeshBaker_Examples_2017.HackTextureAtlas;

public class MB_TextureBakerQuickHack : MonoBehaviour
{
	[Header("Hack Atlas Generation")]
	public string colorTintPropertyName;

	public string albedoTexturePropertyName;

	public string shaderName;

	public Material[] sourceMaterials;

	[Space(20f)]
	[Header("Generated Output")]
	public MB2_TextureBakeResults materialBakeResult;

	public Material atlasMaterial;

	public Texture2D atlasTexture;

	[ContextMenu("Generate Material Bake Result")]
	public void CreateAtlas(Material[] passedInSourceMaterials)
	{
		Debug.Log("Validating source materials");
		bool flag = true;
		if (new HashSet<Material>(passedInSourceMaterials).Count != passedInSourceMaterials.Length)
		{
			Debug.LogError("Source materials are not unique");
			flag = false;
		}
		if (passedInSourceMaterials.Length == 0)
		{
			Debug.LogError("No source materials were passed in");
			flag = false;
		}
		if (string.IsNullOrEmpty(colorTintPropertyName))
		{
			Debug.LogError("ColorTintProperty is not set");
			flag = false;
		}
		sourceMaterials = new Material[passedInSourceMaterials.Length];
		for (int i = 0; i < passedInSourceMaterials.Length; i++)
		{
			if (passedInSourceMaterials[i] == null)
			{
				Debug.LogError("Source material " + i + " is null");
				flag = false;
			}
			if (!passedInSourceMaterials[i].HasProperty(colorTintPropertyName))
			{
				Debug.LogError("Source material " + i + " does not have the colorTint property");
				flag = false;
			}
			sourceMaterials[i] = passedInSourceMaterials[i];
			sourceMaterials[i].shader = Shader.Find(shaderName);
		}
		if (!flag)
		{
			Debug.LogError("Some validation of the source materials failed and the atlas was not generated.");
			return;
		}
		int num = 2;
		int num2 = 8;
		bool linear = MBVersion.GetProjectColorSpace() == ColorSpace.Linear;
		Texture2D[] array = new Texture2D[sourceMaterials.Length];
		StringBuilder stringBuilder = new StringBuilder("Collecting color tints from source materials: \n");
		Color[] array2 = new Color[num2 * num2];
		for (int j = 0; j < sourceMaterials.Length; j++)
		{
			Material material = sourceMaterials[j];
			Color color = material.GetColor(colorTintPropertyName);
			string[] obj = new string[5] { "Material: ", material.name, " - colorTint: ", null, null };
			Color color2 = color;
			obj[3] = color2.ToString();
			obj[4] = "\n";
			stringBuilder.Append(string.Concat(obj));
			Texture2D texture2D = (array[j] = new Texture2D(num2, num2, TextureFormat.ARGB32, mipChain: false, linear));
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = color;
			}
			texture2D.SetPixels(array2);
			texture2D.Apply();
		}
		Debug.Log(stringBuilder);
		Debug.Log("Calculating the atlas dimensions");
		int num3 = (int)Mathf.Ceil(Mathf.Sqrt(sourceMaterials.Length)) * num2;
		Debug.Log("Creating atlas for " + sourceMaterials.Length + " textures");
		atlasTexture = new Texture2D(num3, num3, TextureFormat.ARGB32, mipChain: false, linear);
		Rect[] array3 = atlasTexture.PackTextures(array, 0, num3);
		Debug.Log("Atlas size: w:" + atlasTexture.width + "  h:" + atlasTexture.height + "  numTex: " + array.Length + " (" + num2 + "x" + num2 + " each)");
		atlasTexture.filterMode = FilterMode.Point;
		atlasMaterial = new Material(Shader.Find(shaderName));
		atlasMaterial.SetTexture(albedoTexturePropertyName, atlasTexture);
		atlasMaterial.SetColor(colorTintPropertyName, Color.white);
		StringBuilder stringBuilder2 = new StringBuilder("Creating MB2_TextureBakeResult for storing atlas rectangle information: \n");
		for (int l = 0; l < array.Length; l++)
		{
			string[] obj2 = new string[5]
			{
				"Material: ",
				sourceMaterials[l].name,
				" will use rectangle: ",
				null,
				null
			};
			Rect rect = array3[l];
			obj2[3] = rect.ToString();
			obj2[4] = "\n";
			stringBuilder2.Append(string.Concat(obj2));
		}
		Debug.Log(stringBuilder2);
		Debug.Log("Creating and setting up MB2_TextureBakeResults");
		materialBakeResult = ScriptableObject.CreateInstance<MB2_TextureBakeResults>();
		materialBakeResult.resultType = MB2_TextureBakeResults.ResultType.atlas;
		materialBakeResult.materialsAndUVRects = new MB_MaterialAndUVRect[array.Length];
		float num4 = (float)num / (float)atlasTexture.width;
		float num5 = (float)num / (float)atlasTexture.height;
		for (int m = 0; m < array.Length; m++)
		{
			Rect destRect = array3[m];
			destRect.x += num4;
			destRect.y += num5;
			destRect.width -= 2f * num4;
			destRect.height -= 2f * num5;
			bool allPropsUseSameTiling = true;
			materialBakeResult.materialsAndUVRects[m] = new MB_MaterialAndUVRect(sourceMaterials[m], destRect, allPropsUseSameTiling, new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, 0f, 0f), MB_TextureTilingTreatment.none, sourceMaterials[m].name);
		}
		materialBakeResult.resultMaterials = new MB_MultiMaterial[1];
		materialBakeResult.resultMaterials[0] = new MB_MultiMaterial();
		materialBakeResult.resultMaterials[0].combinedMaterial = atlasMaterial;
		materialBakeResult.resultMaterials[0].considerMeshUVs = false;
		List<Material> list = new List<Material>();
		list.AddRange(sourceMaterials);
		materialBakeResult.resultMaterials[0].sourceMaterials = list;
	}
}
