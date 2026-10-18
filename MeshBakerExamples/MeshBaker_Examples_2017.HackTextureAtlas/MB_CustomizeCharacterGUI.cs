using DigitalOpus.MB.Core;
using UnityEngine;

namespace MeshBaker_Examples_2017.HackTextureAtlas;

public class MB_CustomizeCharacterGUI : MonoBehaviour
{
	public Material[] sourceMaterials;

	public GameObject[] objectsToBeCombined;

	[Header("Mesh Baker Config")]
	public MB3_MeshBaker targetMeshBaker;

	private MB_TextureBakerQuickHack textureBakerQuickHack;

	private string colorTintPropertyName;

	private string albedoTexturePropertyName;

	private string shaderName;

	private void Start()
	{
		switch (MBVersion.DetectPipeline())
		{
		case MBVersion.PipelineType.Default:
			colorTintPropertyName = "_Color";
			albedoTexturePropertyName = "_MainTex";
			shaderName = "Standard";
			break;
		case MBVersion.PipelineType.URP:
			colorTintPropertyName = "_BaseColor";
			albedoTexturePropertyName = "_BaseMap";
			shaderName = "Universal Render Pipeline/Lit";
			break;
		case MBVersion.PipelineType.HDRP:
			colorTintPropertyName = "_BaseColor";
			albedoTexturePropertyName = "_BaseColorMap";
			shaderName = "HDRP/Lit";
			break;
		default:
			Debug.LogError("Unknown pipeline type");
			break;
		}
		textureBakerQuickHack = GetComponent<MB_TextureBakerQuickHack>();
		Debug.Log("Creating atlas using TextureBakerQuickHack method");
		textureBakerQuickHack.colorTintPropertyName = colorTintPropertyName;
		textureBakerQuickHack.albedoTexturePropertyName = albedoTexturePropertyName;
		textureBakerQuickHack.shaderName = shaderName;
		textureBakerQuickHack.CreateAtlas(sourceMaterials);
		Debug.Log("Baking MeshBaker using TextureBakerQuickHack output");
		BakeMeshBaker();
	}

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("This example demonstrates how to create\r\nsolid-color-rectangle texture atlases at\r\nruntime for character customization. This\r\nis MUCH faster and more flexible than using\r\nthe full TextureBaker. These atlases can be\r\nused at runtime with a Mesh Baker.\r\n".ToString());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Hoof Color");
		if (GUILayout.Button("Red"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[0], Color.red);
		}
		if (GUILayout.Button("Green"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[0], Color.green);
		}
		if (GUILayout.Button("Blue"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[0], Color.blue);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Body Color");
		if (GUILayout.Button("Red"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[1], Color.red);
		}
		if (GUILayout.Button("Green"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[1], Color.green);
		}
		if (GUILayout.Button("Blue"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[1], Color.blue);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Horns Color");
		if (GUILayout.Button("Red"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[2], Color.red);
		}
		if (GUILayout.Button("Green"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[2], Color.green);
		}
		if (GUILayout.Button("Blue"))
		{
			SetColorInMaterialBakeResultAndBakeMeshBaker(sourceMaterials[2], Color.blue);
		}
		GUILayout.EndHorizontal();
	}

	private void SetColorInMaterialBakeResultAndBakeMeshBaker(Material bodyPartMaterial, Color color)
	{
		Debug.Log("Changing color of material " + bodyPartMaterial?.ToString() + " used in atlas generation");
		bodyPartMaterial.SetColor(colorTintPropertyName, color);
		Debug.Log("Creating atlas using TextureBakerQuickHack method");
		textureBakerQuickHack.CreateAtlas(sourceMaterials);
		Debug.Log("Baking MeshBaker using TextureBakerQuickHack output");
		BakeMeshBaker();
	}

	[ContextMenu("Bake Mesh Baker")]
	private void BakeMeshBaker()
	{
		targetMeshBaker.textureBakeResults = textureBakerQuickHack.materialBakeResult;
		targetMeshBaker.ClearMesh();
		if (targetMeshBaker.AddDeleteGameObjects(objectsToBeCombined, null, disableRendererInSource: true))
		{
			targetMeshBaker.Apply();
		}
	}
}
