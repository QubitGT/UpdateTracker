using Meta.XR.Acoustics;
using UnityEngine;

[CreateAssetMenu(menuName = "MetaXRAudio/Acoustic Material Properties")]
internal class MetaXRAcousticMaterialProperties : ScriptableObject, IMaterialDataProvider
{
	internal enum BuiltinPreset
	{
		Custom,
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

	[SerializeField]
	private MaterialData data = new MaterialData();

	[SerializeField]
	private BuiltinPreset preset;

	public MaterialData Data => data;

	internal BuiltinPreset Preset
	{
		get
		{
			return preset;
		}
		set
		{
			if (value != 0)
			{
				SetPreset(value, ref data);
			}
			preset = value;
		}
	}

	internal static void SetPreset(BuiltinPreset builtinPreset, ref MaterialData data)
	{
		switch (builtinPreset)
		{
		case BuiltinPreset.AcousticTile:
			AcousticTile(ref data);
			break;
		case BuiltinPreset.Brick:
			Brick(ref data);
			break;
		case BuiltinPreset.BrickPainted:
			BrickPainted(ref data);
			break;
		case BuiltinPreset.Cardboard:
			Cardboard(ref data);
			break;
		case BuiltinPreset.Carpet:
			Carpet(ref data);
			break;
		case BuiltinPreset.CarpetHeavy:
			CarpetHeavy(ref data);
			break;
		case BuiltinPreset.CarpetHeavyPadded:
			CarpetHeavyPadded(ref data);
			break;
		case BuiltinPreset.CeramicTile:
			CeramicTile(ref data);
			break;
		case BuiltinPreset.Concrete:
			Concrete(ref data);
			break;
		case BuiltinPreset.ConcreteRough:
			ConcreteRough(ref data);
			break;
		case BuiltinPreset.ConcreteBlock:
			ConcreteBlock(ref data);
			break;
		case BuiltinPreset.ConcreteBlockPainted:
			ConcreteBlockPainted(ref data);
			break;
		case BuiltinPreset.Curtain:
			Curtain(ref data);
			break;
		case BuiltinPreset.Foliage:
			Foliage(ref data);
			break;
		case BuiltinPreset.Glass:
			Glass(ref data);
			break;
		case BuiltinPreset.GlassHeavy:
			GlassHeavy(ref data);
			break;
		case BuiltinPreset.Grass:
			Grass(ref data);
			break;
		case BuiltinPreset.Gravel:
			Gravel(ref data);
			break;
		case BuiltinPreset.GypsumBoard:
			GypsumBoard(ref data);
			break;
		case BuiltinPreset.Marble:
			Marble(ref data);
			break;
		case BuiltinPreset.Mud:
			Mud(ref data);
			break;
		case BuiltinPreset.PlasterOnBrick:
			PlasterOnBrick(ref data);
			break;
		case BuiltinPreset.PlasterOnConcreteBlock:
			PlasterOnConcreteBlock(ref data);
			break;
		case BuiltinPreset.Rubber:
			Rubber(ref data);
			break;
		case BuiltinPreset.Soil:
			Soil(ref data);
			break;
		case BuiltinPreset.SoundProof:
			SoundProof(ref data);
			break;
		case BuiltinPreset.Snow:
			Snow(ref data);
			break;
		case BuiltinPreset.Steel:
			Steel(ref data);
			break;
		case BuiltinPreset.Stone:
			Stone(ref data);
			break;
		case BuiltinPreset.Vent:
			Vent(ref data);
			break;
		case BuiltinPreset.Water:
			Water(ref data);
			break;
		case BuiltinPreset.WoodThin:
			WoodThin(ref data);
			break;
		case BuiltinPreset.WoodThick:
			WoodThick(ref data);
			break;
		case BuiltinPreset.WoodFloor:
			WoodFloor(ref data);
			break;
		case BuiltinPreset.WoodOnConcrete:
			WoodOnConcrete(ref data);
			break;
		case BuiltinPreset.MetaDefault:
			MetaDefault(ref data);
			break;
		default:
			Debug.LogError("no preset specified");
			break;
		}
	}

	private static void AcousticTile(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.5f },
			{ 250f, 0.7f },
			{ 500f, 0.6f },
			{ 1000f, 0.7f },
			{ 2000f, 0.7f },
			{ 4000f, 0.5f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.15f },
			{ 500f, 0.2f },
			{ 1000f, 0.2f },
			{ 2000f, 0.25f },
			{ 4000f, 0.3f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.05f },
			{ 250f, 0.04f },
			{ 500f, 0.03f },
			{ 1000f, 0.02f },
			{ 2000f, 0.005f },
			{ 4000f, 0.002f }
		};
	}

	private static void Brick(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.02f },
			{ 250f, 0.02f },
			{ 500f, 0.03f },
			{ 1000f, 0.04f },
			{ 2000f, 0.05f },
			{ 4000f, 0.07f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.25f },
			{ 500f, 0.3f },
			{ 1000f, 0.35f },
			{ 2000f, 0.4f },
			{ 4000f, 0.45f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.025f },
			{ 250f, 0.019f },
			{ 500f, 0.01f },
			{ 1000f, 0.0045f },
			{ 2000f, 0.0018f },
			{ 4000f, 0.00089f }
		};
	}

	private static void BrickPainted(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.02f },
			{ 1000f, 0.02f },
			{ 2000f, 0.02f },
			{ 4000f, 0.03f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.15f },
			{ 250f, 0.15f },
			{ 500f, 0.2f },
			{ 1000f, 0.2f },
			{ 2000f, 0.2f },
			{ 4000f, 0.25f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.025f },
			{ 250f, 0.019f },
			{ 500f, 0.01f },
			{ 1000f, 0.0045f },
			{ 2000f, 0.0018f },
			{ 4000f, 0.00089f }
		};
	}

	private static void Cardboard(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 400f, 0.41f },
			{ 500f, 0.607f },
			{ 630f, 0.773f },
			{ 800f, 0.669f },
			{ 1000f, 0.685f },
			{ 1250f, 0.806f },
			{ 1600f, 0.579f },
			{ 2000f, 0.792f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.12f },
			{ 500f, 0.14f },
			{ 1000f, 0.16f },
			{ 2000f, 0.18f },
			{ 4000f, 0.2f }
		};
		data.transmission = new Spectrum
		{
			{ 400f, 0.082f },
			{ 500f, 0.121f },
			{ 630f, 0.155f },
			{ 800f, 0.134f },
			{ 1000f, 0.137f },
			{ 1250f, 0.161f },
			{ 1600f, 0.116f },
			{ 2000f, 0.158f }
		};
	}

	private static void Carpet(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.05f },
			{ 500f, 0.1f },
			{ 1000f, 0.2f },
			{ 2000f, 0.45f },
			{ 4000f, 0.65f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.15f },
			{ 1000f, 0.2f },
			{ 2000f, 0.3f },
			{ 4000f, 0.45f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void CarpetHeavy(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.02f },
			{ 250f, 0.06f },
			{ 500f, 0.14f },
			{ 1000f, 0.37f },
			{ 2000f, 0.48f },
			{ 4000f, 0.63f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.15f },
			{ 500f, 0.2f },
			{ 1000f, 0.25f },
			{ 2000f, 0.35f },
			{ 4000f, 0.5f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void CarpetHeavyPadded(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.08f },
			{ 250f, 0.24f },
			{ 500f, 0.57f },
			{ 1000f, 0.69f },
			{ 2000f, 0.71f },
			{ 4000f, 0.73f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.15f },
			{ 500f, 0.2f },
			{ 1000f, 0.25f },
			{ 2000f, 0.35f },
			{ 4000f, 0.5f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void CeramicTile(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.01f },
			{ 1000f, 0.01f },
			{ 2000f, 0.02f },
			{ 4000f, 0.02f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.12f },
			{ 500f, 0.14f },
			{ 1000f, 0.16f },
			{ 2000f, 0.18f },
			{ 4000f, 0.2f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void Concrete(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.02f },
			{ 1000f, 0.02f },
			{ 2000f, 0.02f },
			{ 4000f, 0.02f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.11f },
			{ 500f, 0.12f },
			{ 1000f, 0.13f },
			{ 2000f, 0.14f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void ConcreteRough(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.02f },
			{ 500f, 0.04f },
			{ 1000f, 0.06f },
			{ 2000f, 0.08f },
			{ 4000f, 0.1f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.12f },
			{ 500f, 0.15f },
			{ 1000f, 0.2f },
			{ 2000f, 0.25f },
			{ 4000f, 0.3f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void ConcreteBlock(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.36f },
			{ 250f, 0.44f },
			{ 500f, 0.31f },
			{ 1000f, 0.29f },
			{ 2000f, 0.39f },
			{ 4000f, 0.21f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.12f },
			{ 500f, 0.15f },
			{ 1000f, 0.2f },
			{ 2000f, 0.3f },
			{ 4000f, 0.4f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.02f },
			{ 250f, 0.01f },
			{ 500f, 0.0063f },
			{ 1000f, 0.0035f },
			{ 2000f, 0.00011f },
			{ 4000f, 0.00063f }
		};
	}

	private static void ConcreteBlockPainted(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.05f },
			{ 500f, 0.06f },
			{ 1000f, 0.07f },
			{ 2000f, 0.09f },
			{ 4000f, 0.08f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.11f },
			{ 500f, 0.13f },
			{ 1000f, 0.15f },
			{ 2000f, 0.16f },
			{ 4000f, 0.2f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.02f },
			{ 250f, 0.01f },
			{ 500f, 0.0063f },
			{ 1000f, 0.0035f },
			{ 2000f, 0.00011f },
			{ 4000f, 0.00063f }
		};
	}

	private static void Curtain(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.07f },
			{ 250f, 0.31f },
			{ 500f, 0.49f },
			{ 1000f, 0.75f },
			{ 2000f, 0.7f },
			{ 4000f, 0.6f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.15f },
			{ 500f, 0.2f },
			{ 1000f, 0.3f },
			{ 2000f, 0.4f },
			{ 4000f, 0.5f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.42f },
			{ 250f, 0.39f },
			{ 500f, 0.21f },
			{ 1000f, 0.14f },
			{ 2000f, 0.079f },
			{ 4000f, 0.045f }
		};
	}

	private static void Foliage(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.03f },
			{ 250f, 0.06f },
			{ 500f, 0.11f },
			{ 1000f, 0.17f },
			{ 2000f, 0.27f },
			{ 4000f, 0.31f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.3f },
			{ 500f, 0.4f },
			{ 1000f, 0.5f },
			{ 2000f, 0.7f },
			{ 4000f, 0.8f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.9f },
			{ 250f, 0.9f },
			{ 500f, 0.9f },
			{ 1000f, 0.8f },
			{ 2000f, 0.5f },
			{ 4000f, 0.3f }
		};
	}

	private static void Glass(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.35f },
			{ 250f, 0.25f },
			{ 500f, 0.18f },
			{ 1000f, 0.12f },
			{ 2000f, 0.07f },
			{ 4000f, 0.05f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.05f },
			{ 250f, 0.05f },
			{ 500f, 0.05f },
			{ 1000f, 0.05f },
			{ 2000f, 0.05f },
			{ 4000f, 0.05f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.125f },
			{ 250f, 0.089f },
			{ 500f, 0.05f },
			{ 1000f, 0.028f },
			{ 2000f, 0.022f },
			{ 4000f, 0.079f }
		};
	}

	private static void GlassHeavy(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.18f },
			{ 250f, 0.06f },
			{ 500f, 0.04f },
			{ 1000f, 0.03f },
			{ 2000f, 0.02f },
			{ 4000f, 0.02f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.05f },
			{ 250f, 0.05f },
			{ 500f, 0.05f },
			{ 1000f, 0.05f },
			{ 2000f, 0.05f },
			{ 4000f, 0.05f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.056f },
			{ 250f, 0.039f },
			{ 500f, 0.028f },
			{ 1000f, 0.02f },
			{ 2000f, 0.032f },
			{ 4000f, 0.014f }
		};
	}

	private static void Grass(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.11f },
			{ 250f, 0.26f },
			{ 500f, 0.6f },
			{ 1000f, 0.69f },
			{ 2000f, 0.92f },
			{ 4000f, 0.99f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.3f },
			{ 250f, 0.3f },
			{ 500f, 0.4f },
			{ 1000f, 0.5f },
			{ 2000f, 0.6f },
			{ 4000f, 0.7f }
		};
		data.transmission = new Spectrum();
	}

	private static void Gravel(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.25f },
			{ 250f, 0.6f },
			{ 500f, 0.65f },
			{ 1000f, 0.7f },
			{ 2000f, 0.75f },
			{ 4000f, 0.8f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.3f },
			{ 500f, 0.4f },
			{ 1000f, 0.5f },
			{ 2000f, 0.6f },
			{ 4000f, 0.7f }
		};
		data.transmission = new Spectrum();
	}

	private static void GypsumBoard(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.29f },
			{ 250f, 0.1f },
			{ 500f, 0.05f },
			{ 1000f, 0.04f },
			{ 2000f, 0.07f },
			{ 4000f, 0.09f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.11f },
			{ 500f, 0.12f },
			{ 1000f, 0.13f },
			{ 2000f, 0.14f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.035f },
			{ 250f, 0.0125f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0025f },
			{ 2000f, 0.0013f },
			{ 4000f, 0.0032f }
		};
	}

	private static void Marble(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.01f },
			{ 1000f, 0.01f },
			{ 2000f, 0.02f },
			{ 4000f, 0.02f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.1f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void Mud(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.15f },
			{ 250f, 0.25f },
			{ 500f, 0.3f },
			{ 1000f, 0.25f },
			{ 2000f, 0.2f },
			{ 4000f, 0.15f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.2f },
			{ 500f, 0.25f },
			{ 1000f, 0.4f },
			{ 2000f, 0.55f },
			{ 4000f, 0.7f }
		};
		data.transmission = new Spectrum();
	}

	private static void PlasterOnBrick(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.02f },
			{ 500f, 0.02f },
			{ 1000f, 0.03f },
			{ 2000f, 0.04f },
			{ 4000f, 0.05f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.25f },
			{ 500f, 0.3f },
			{ 1000f, 0.35f },
			{ 2000f, 0.4f },
			{ 4000f, 0.45f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.025f },
			{ 250f, 0.019f },
			{ 500f, 0.01f },
			{ 1000f, 0.0045f },
			{ 2000f, 0.0018f },
			{ 4000f, 0.00089f }
		};
	}

	private static void PlasterOnConcreteBlock(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.12f },
			{ 250f, 0.09f },
			{ 500f, 0.07f },
			{ 1000f, 0.05f },
			{ 2000f, 0.05f },
			{ 4000f, 0.04f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.25f },
			{ 500f, 0.3f },
			{ 1000f, 0.35f },
			{ 2000f, 0.4f },
			{ 4000f, 0.45f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.02f },
			{ 250f, 0.01f },
			{ 500f, 0.0063f },
			{ 1000f, 0.0035f },
			{ 2000f, 0.00011f },
			{ 4000f, 0.00063f }
		};
	}

	private static void Rubber(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.05f },
			{ 250f, 0.05f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.05f },
			{ 4000f, 0.05f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.15f },
			{ 4000f, 0.2f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.02f },
			{ 1000f, 0.02f },
			{ 2000f, 0.01f },
			{ 4000f, 0.01f }
		};
	}

	private static void Soil(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.15f },
			{ 250f, 0.25f },
			{ 500f, 0.4f },
			{ 1000f, 0.55f },
			{ 2000f, 0.6f },
			{ 4000f, 0.6f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.2f },
			{ 500f, 0.25f },
			{ 1000f, 0.4f },
			{ 2000f, 0.55f },
			{ 4000f, 0.7f }
		};
		data.transmission = new Spectrum();
	}

	private static void SoundProof(ref MaterialData data)
	{
		data.absorption = new Spectrum { { 1000f, 1f } };
		data.scattering = new Spectrum { { 1000f, 0f } };
		data.transmission = new Spectrum();
	}

	private static void Snow(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.45f },
			{ 250f, 0.75f },
			{ 500f, 0.9f },
			{ 1000f, 0.95f },
			{ 2000f, 0.95f },
			{ 4000f, 0.95f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.3f },
			{ 500f, 0.4f },
			{ 1000f, 0.5f },
			{ 2000f, 0.6f },
			{ 4000f, 0.75f }
		};
		data.transmission = new Spectrum();
	}

	private static void Steel(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.05f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.07f },
			{ 4000f, 0.02f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.1f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.25f },
			{ 250f, 0.2f },
			{ 500f, 0.17f },
			{ 1000f, 0.089f },
			{ 2000f, 0.089f },
			{ 4000f, 0.0056f }
		};
	}

	private static void Stone(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.02f },
			{ 500f, 0.02f },
			{ 2000f, 0.05f },
			{ 4000f, 0.05f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.15f },
			{ 1000f, 0.2f },
			{ 2000f, 0.25f },
			{ 4000f, 0.3f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.00016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void Vent(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 63.5f, 0.15f },
			{ 125f, 0.15f },
			{ 250f, 0.2f },
			{ 500f, 0.5f },
			{ 1000f, 0.35f },
			{ 2000f, 0.3f },
			{ 4000f, 0.2f },
			{ 8000f, 0.2f }
		};
		data.scattering = new Spectrum
		{
			{ 63.5f, 0.1f },
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.15f },
			{ 1000f, 0.3f },
			{ 2000f, 0.4f },
			{ 4000f, 0.5f },
			{ 8000f, 0.5f }
		};
		data.transmission = new Spectrum
		{
			{ 63.5f, 0.135f },
			{ 125f, 0.135f },
			{ 250f, 0.18f },
			{ 500f, 0.45f },
			{ 1000f, 0.315f },
			{ 2000f, 0.27f },
			{ 4000f, 0.18f },
			{ 8000f, 0.18f }
		};
	}

	private static void Water(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.01f },
			{ 250f, 0.01f },
			{ 500f, 0.01f },
			{ 1000f, 0.02f },
			{ 2000f, 0.02f },
			{ 4000f, 0.03f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.07f },
			{ 2000f, 0.05f },
			{ 4000f, 0.05f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.03f },
			{ 250f, 0.03f },
			{ 500f, 0.03f },
			{ 1000f, 0.02f },
			{ 2000f, 0.015f },
			{ 4000f, 0.01f }
		};
	}

	private static void WoodThin(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.42f },
			{ 250f, 0.21f },
			{ 500f, 0.1f },
			{ 1000f, 0.08f },
			{ 2000f, 0.06f },
			{ 4000f, 0.06f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.2f },
			{ 250f, 0.125f },
			{ 500f, 0.079f },
			{ 1000f, 0.1f },
			{ 2000f, 0.089f },
			{ 4000f, 0.05f }
		};
	}

	private static void WoodThick(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.19f },
			{ 250f, 0.14f },
			{ 500f, 0.09f },
			{ 1000f, 0.06f },
			{ 2000f, 0.06f },
			{ 4000f, 0.05f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.035f },
			{ 250f, 0.028f },
			{ 500f, 0.028f },
			{ 1000f, 0.028f },
			{ 2000f, 0.011f },
			{ 4000f, 0.0071f }
		};
	}

	private static void WoodFloor(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.15f },
			{ 250f, 0.11f },
			{ 500f, 0.1f },
			{ 1000f, 0.07f },
			{ 2000f, 0.06f },
			{ 4000f, 0.07f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.071f },
			{ 250f, 0.025f },
			{ 500f, 0.0158f },
			{ 1000f, 0.0056f },
			{ 2000f, 0.0035f },
			{ 4000f, 0.0016f }
		};
	}

	private static void WoodOnConcrete(ref MaterialData data)
	{
		data.absorption = new Spectrum
		{
			{ 125f, 0.04f },
			{ 250f, 0.04f },
			{ 500f, 0.07f },
			{ 1000f, 0.06f },
			{ 2000f, 0.06f },
			{ 4000f, 0.07f }
		};
		data.scattering = new Spectrum
		{
			{ 125f, 0.1f },
			{ 250f, 0.1f },
			{ 500f, 0.1f },
			{ 1000f, 0.1f },
			{ 2000f, 0.1f },
			{ 4000f, 0.15f }
		};
		data.transmission = new Spectrum
		{
			{ 125f, 0.004f },
			{ 250f, 0.0079f },
			{ 500f, 0.0056f },
			{ 1000f, 0.0016f },
			{ 2000f, 0.0014f },
			{ 4000f, 0.0005f }
		};
	}

	private static void MetaDefault(ref MaterialData data)
	{
		data.absorption = new Spectrum { { 1000f, 0.1f } };
		data.scattering = new Spectrum { { 1000f, 0.5f } };
		data.transmission = new Spectrum { { 1000f, 0f } };
	}

	string IMaterialDataProvider.get_name()
	{
		return base.name;
	}
}
