using UnityEngine;

public sealed class MetaXRAudioRoomAcousticProperties : MonoBehaviour
{
	public enum MaterialPreset
	{
		AcousticTile,
		Brick,
		BrickPainted,
		Cardboard,
		Carpet,
		CarpetHeavy,
		CarpetHeavyPadded,
		CeramicTile,
		Concrete,
		ConcreteRough,
		ConcreteBlock,
		ConcreteBlockPainted,
		Curtain,
		Foliage,
		Glass,
		GlassHeavy,
		Grass,
		Gravel,
		GypsumBoard,
		Marble,
		Mud,
		PlasterOnBrick,
		PlasterOnConcreteBlock,
		Rubber,
		Soil,
		SoundProof,
		Snow,
		Steel,
		Stone,
		Vent,
		Water,
		WoodThin,
		WoodThick,
		WoodFloor,
		WoodOnConcrete,
		MetaDefault
	}

	[Tooltip("Center the room model on the listener. When disabled, center the room model on the GameObject this script is attached to.")]
	public bool lockPositionToListener = true;

	[Tooltip("Width of the room model in meters")]
	public float width = 8f;

	[Tooltip("Height of the room model in meters")]
	public float height = 3f;

	[Tooltip("Depth of the room model in meters")]
	public float depth = 5f;

	[Tooltip("Material of the left wall of the room model")]
	public MaterialPreset leftMaterial = MaterialPreset.GypsumBoard;

	[Tooltip("Material of the right wall of the room model")]
	public MaterialPreset rightMaterial = MaterialPreset.GypsumBoard;

	[Tooltip("Material of the ceiling of the room model")]
	public MaterialPreset ceilingMaterial;

	[Tooltip("Material of the floor of the room model")]
	public MaterialPreset floorMaterial = MaterialPreset.Carpet;

	[Tooltip("Material of the front wall of the room model")]
	public MaterialPreset frontMaterial = MaterialPreset.GypsumBoard;

	[Tooltip("Material of the back wall of the room model")]
	public MaterialPreset backMaterial = MaterialPreset.GypsumBoard;

	[Tooltip("Diffuses the reflections and reverberation to simulate objects inside the room. Zero represents a completely empty room.")]
	[Range(0f, 1f)]
	public float clutterFactor = 0.5f;

	private const int kAudioBandCount = 4;

	private float[] clutterFactorBands = new float[4];

	private float[] wallMaterials = new float[24];

	[RuntimeInitializeOnLoadMethod]
	private static void CheckSceneHasRoom()
	{
		MetaXRAudioRoomAcousticProperties[] array = Object.FindObjectsOfType<MetaXRAudioRoomAcousticProperties>();
		if (array.Length == 0)
		{
			Debug.Log("No Meta XR Audio Room found, setting default room");
			GameObject obj = new GameObject("Temporary Room");
			obj.AddComponent<MetaXRAudioRoomAcousticProperties>().Update();
			Object.DestroyImmediate(obj);
		}
		if (array.Length > 1)
		{
			Debug.LogError("Multiple Meta XR Audio Rooms found, only one is allowed!");
		}
	}

	private void Update()
	{
		SetWallMaterialPreset(0, rightMaterial);
		SetWallMaterialPreset(1, leftMaterial);
		SetWallMaterialPreset(2, ceilingMaterial);
		SetWallMaterialPreset(3, floorMaterial);
		SetWallMaterialPreset(4, frontMaterial);
		SetWallMaterialPreset(5, backMaterial);
		MetaXRAudioNativeInterface.Interface.SetAdvancedBoxRoomParameters(width, height, depth, lockPositionToListener, base.transform.position, wallMaterials);
		float num = clutterFactor;
		for (int num2 = 3; num2 >= 0; num2--)
		{
			clutterFactorBands[num2] = num;
			num *= 0.5f;
		}
		MetaXRAudioNativeInterface.Interface.SetRoomClutterFactor(clutterFactorBands);
	}

	private void SetWallMaterialPreset(int wallIndex, MaterialPreset materialPreset)
	{
		switch (materialPreset)
		{
		case MaterialPreset.AcousticTile:
			SetWallMaterialProperties(wallIndex, 0.48816842f, 0.36147523f, 0.33959538f, 0.49894625f);
			break;
		case MaterialPreset.Brick:
			SetWallMaterialProperties(wallIndex, 0.9754688f, 0.9720645f, 0.9491802f, 0.9301054f);
			break;
		case MaterialPreset.BrickPainted:
			SetWallMaterialProperties(wallIndex, 0.9757106f, 0.98332417f, 0.9781167f, 0.9700527f);
			break;
		case MaterialPreset.Cardboard:
			SetWallMaterialProperties(wallIndex, 0.59f, 0.435728f, 0.25165f, 0.208f);
			break;
		case MaterialPreset.Carpet:
			SetWallMaterialProperties(wallIndex, 0.9876337f, 0.90548664f, 0.5831106f, 0.35105383f);
			break;
		case MaterialPreset.CarpetHeavy:
			SetWallMaterialProperties(wallIndex, 0.9776337f, 0.8590829f, 0.5264796f, 0.37079042f);
			break;
		case MaterialPreset.CarpetHeavyPadded:
			SetWallMaterialProperties(wallIndex, 0.91053474f, 0.5304332f, 0.29405582f, 0.27010542f);
			break;
		case MaterialPreset.CeramicTile:
			SetWallMaterialProperties(wallIndex, 0.99f, 0.99f, 0.98275393f, 0.98f);
			break;
		case MaterialPreset.Concrete:
			SetWallMaterialProperties(wallIndex, 0.99f, 0.98332417f, 0.98f, 0.98f);
			break;
		case MaterialPreset.ConcreteRough:
			SetWallMaterialProperties(wallIndex, 0.98940843f, 0.96449465f, 0.922127f, 0.90010536f);
			break;
		case MaterialPreset.ConcreteBlock:
			SetWallMaterialProperties(wallIndex, 0.6352674f, 0.6522307f, 0.67105347f, 0.7890516f);
			break;
		case MaterialPreset.ConcreteBlockPainted:
			SetWallMaterialProperties(wallIndex, 0.9029579f, 0.9402359f, 0.91758406f, 0.9199473f);
			break;
		case MaterialPreset.Curtain:
			SetWallMaterialProperties(wallIndex, 0.68649423f, 0.54586f, 0.31007856f, 0.39947313f);
			break;
		case MaterialPreset.Foliage:
			SetWallMaterialProperties(wallIndex, 0.51825935f, 0.5035683f, 0.5786888f, 0.6902108f);
			break;
		case MaterialPreset.Glass:
			SetWallMaterialProperties(wallIndex, 0.6559158f, 0.8006318f, 0.9188397f, 0.92348814f);
			break;
		case MaterialPreset.GlassHeavy:
			SetWallMaterialProperties(wallIndex, 0.82709897f, 0.95022273f, 0.9746041f, 0.98f);
			break;
		case MaterialPreset.Grass:
			SetWallMaterialProperties(wallIndex, 0.8811263f, 0.5071708f, 0.1318931f, 0.010368884f);
			break;
		case MaterialPreset.Gravel:
			SetWallMaterialProperties(wallIndex, 0.7292947f, 0.37312245f, 0.25531745f, 0.20026344f);
			break;
		case MaterialPreset.GypsumBoard:
			SetWallMaterialProperties(wallIndex, 0.72124004f, 0.92769015f, 0.9343023f, 0.9101054f);
			break;
		case MaterialPreset.Marble:
			SetWallMaterialProperties(wallIndex, 0.99f, 0.99f, 0.982754f, 0.98f);
			break;
		case MaterialPreset.Mud:
			SetWallMaterialProperties(wallIndex, 0.844084f, 0.726577f, 0.794683f, 0.849737f);
			break;
		case MaterialPreset.PlasterOnBrick:
			SetWallMaterialProperties(wallIndex, 0.9756965f, 0.979106f, 0.9610635f, 0.9500527f);
			break;
		case MaterialPreset.PlasterOnConcreteBlock:
			SetWallMaterialProperties(wallIndex, 0.8817747f, 0.92477393f, 0.95149755f, 0.9599473f);
			break;
		case MaterialPreset.Rubber:
			SetWallMaterialProperties(wallIndex, 0.95f, 0.916621f, 0.93623f, 0.95f);
			break;
		case MaterialPreset.Soil:
			SetWallMaterialProperties(wallIndex, 0.8440842f, 0.63462424f, 0.41666287f, 0.40000004f);
			break;
		case MaterialPreset.SoundProof:
			SetWallMaterialProperties(wallIndex, 0f, 0f, 0f, 0f);
			break;
		case MaterialPreset.Snow:
			SetWallMaterialProperties(wallIndex, 0.53225267f, 0.15453577f, 0.050964415f, 0.050000012f);
			break;
		case MaterialPreset.Steel:
			SetWallMaterialProperties(wallIndex, 0.7931117f, 0.8401404f, 0.92559177f, 0.97973657f);
			break;
		case MaterialPreset.Stone:
			SetWallMaterialProperties(wallIndex, 0.98f, 0.97874f, 0.955701f, 0.95f);
			break;
		case MaterialPreset.Vent:
			SetWallMaterialProperties(wallIndex, 0.847042f, 0.62045f, 0.70217f, 0.799473f);
			break;
		case MaterialPreset.Water:
			SetWallMaterialProperties(wallIndex, 0.97058827f, 0.9717535f, 0.9783096f, 0.9700527f);
			break;
		case MaterialPreset.WoodThin:
			SetWallMaterialProperties(wallIndex, 0.59242314f, 0.8582733f, 0.9172423f, 0.94f);
			break;
		case MaterialPreset.WoodThick:
			SetWallMaterialProperties(wallIndex, 0.8129579f, 0.8953296f, 0.9413047f, 0.9499473f);
			break;
		case MaterialPreset.WoodFloor:
			SetWallMaterialProperties(wallIndex, 0.8523663f, 0.8989921f, 0.9347841f, 0.9300527f);
			break;
		case MaterialPreset.WoodOnConcrete:
			SetWallMaterialProperties(wallIndex, 0.96f, 0.94123226f, 0.9379238f, 0.9300527f);
			break;
		case MaterialPreset.MetaDefault:
			SetWallMaterialProperties(wallIndex, 0.9f, 0.9f, 0.9f, 0.9f);
			break;
		}
	}

	private void SetWallMaterialProperties(int wallIndex, float band0, float band1, float band2, float band3)
	{
		wallMaterials[wallIndex * 4] = band0;
		wallMaterials[wallIndex * 4 + 1] = band1;
		wallMaterials[wallIndex * 4 + 2] = band2;
		wallMaterials[wallIndex * 4 + 3] = band3;
	}
}
