using Meta.XR.Acoustics;
using UnityEngine;

public class MetaXRAcousticSettings : ScriptableObject
{
	[Tooltip("This is the path inside your Unity project which will store all baked geometry files.")]
	internal const string AcousticFileRootDir = "StreamingAssets/Acoustics";

	[SerializeField]
	[Tooltip("Select which type of acoustic modeling system is used to generate reverb and reflections.")]
	private AcousticModel acousticModel = AcousticModel.Automatic;

	[SerializeField]
	[Tooltip("When enabled and using geometry, all spatailized AudioSources will diffract (propagate around corners and obstructions)")]
	private bool diffractionEnabled = true;

	[SerializeField]
	[Tooltip("Geometry will exclude children with these tags")]
	private string[] excludeTags = new string[0];

	[SerializeField]
	[Tooltip("When you bake an acoustic map, also bake all the acoustic geometry files")]
	private bool mapBakeWriteGeo = true;

	private static MetaXRAcousticSettings instance;

	public AcousticModel AcousticModel
	{
		get
		{
			return acousticModel;
		}
		set
		{
			if (value != acousticModel)
			{
				acousticModel = value;
				MetaXRAcousticNativeInterface.Interface.SetAcousticModel(value);
			}
		}
	}

	internal bool DiffractionEnabled
	{
		get
		{
			return diffractionEnabled;
		}
		set
		{
			if (value != diffractionEnabled)
			{
				diffractionEnabled = value;
				MetaXRAcousticNativeInterface.Interface.SetEnabled(EnableFlagInternal.DIFFRACTION, value);
			}
		}
	}

	internal string[] ExcludeTags
	{
		get
		{
			return excludeTags;
		}
		set
		{
			excludeTags = value;
		}
	}

	[Tooltip("If enabled, acoustic geometry files will also be written when baking an acoustic map")]
	internal bool MapBakeWriteGeo
	{
		get
		{
			return mapBakeWriteGeo;
		}
		set
		{
			mapBakeWriteGeo = value;
		}
	}

	public static MetaXRAcousticSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<MetaXRAcousticSettings>("MetaXRAcousticSettings");
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<MetaXRAcousticSettings>();
				}
			}
			return instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoadRuntimeMethod()
	{
		Instance.ApplyAllSettings();
	}

	internal void ApplyAllSettings()
	{
		Debug.Log("Applying Acoustic Propagation Settings: " + $"[acoustic model = {AcousticModel}], " + $"[diffraction = {DiffractionEnabled}], ");
		MetaXRAcousticNativeInterface.Interface.SetAcousticModel(AcousticModel);
		MetaXRAcousticNativeInterface.Interface.SetEnabled(EnableFlagInternal.DIFFRACTION, DiffractionEnabled);
	}
}
