using System;
using System.Linq;
using Meta.XR.Acoustics;
using UnityEngine;

public sealed class MetaXRAcousticMaterial : MonoBehaviour, IMaterialDataProvider
{
	[SerializeField]
	private MetaXRAcousticMaterialProperties properties;

	[SerializeField]
	private bool hasCustomData;

	[SerializeField]
	internal MaterialData customData;

	[NonSerialized]
	private IntPtr materialHandle = IntPtr.Zero;

	internal MetaXRAcousticMaterialProperties Properties
	{
		get
		{
			return properties;
		}
		set
		{
			properties = value;
		}
	}

	public MaterialData Data
	{
		get
		{
			if (!hasCustomData)
			{
				return properties?.Data;
			}
			return customData;
		}
	}

	internal Color Color
	{
		get
		{
			if (Data == null)
			{
				return Color.magenta;
			}
			return Data.color;
		}
	}

	internal void CopyPresetToCustomData(MetaXRAcousticMaterialProperties.BuiltinPreset preset)
	{
		if (!hasCustomData)
		{
			Debug.LogError("Material doesn't have custom data", base.gameObject);
		}
		else
		{
			MetaXRAcousticMaterialProperties.SetPreset(preset, ref customData);
		}
	}

	private void Start()
	{
		if (!base.gameObject.isStatic)
		{
			StartInternal();
		}
	}

	internal bool StartInternal()
	{
		if (materialHandle != IntPtr.Zero)
		{
			return true;
		}
		materialHandle = CreateMaterialNativeHandle(Data);
		return true;
	}

	private void OnDestroy()
	{
		DestroyInternal();
	}

	internal void DestroyInternal()
	{
		if (materialHandle != IntPtr.Zero)
		{
			DestroyMaterialNativeHandle(materialHandle);
			materialHandle = IntPtr.Zero;
		}
	}

	internal bool ApplyMaterialProperties()
	{
		return ApplyPropertiesToNative(materialHandle, Data);
	}

	internal static IntPtr CreateMaterialNativeHandle(MaterialData data = null)
	{
		IntPtr material = IntPtr.Zero;
		if (MetaXRAcousticNativeInterface.Interface.CreateAudioMaterial(out material) != 0)
		{
			Debug.LogError("Unable to create internal audio material");
			return material;
		}
		if (data != null)
		{
			ApplyPropertiesToNative(material, data);
		}
		return material;
	}

	internal static void DestroyMaterialNativeHandle(IntPtr handle)
	{
		MetaXRAcousticNativeInterface.Interface.DestroyAudioMaterial(handle);
	}

	private static bool ApplyPropertiesToNative(IntPtr handle, MaterialData data)
	{
		return ApplyPropertiesToNative(handle, data, null);
	}

	private static bool ApplyPropertiesToNative(IntPtr handle, MaterialData data, GameObject gameObject)
	{
		if (handle == IntPtr.Zero || data == null)
		{
			if (gameObject != null)
			{
				_ = gameObject.scene;
				string text = gameObject.scene.name + ":" + string.Join("/", (from t in gameObject.GetComponentsInParent<Transform>()
					select t.name).Reverse().ToArray());
				Debug.LogWarning("Acoustic Material configured with empty properties: " + text, gameObject);
			}
			return false;
		}
		MetaXRAcousticNativeInterface.Interface.AudioMaterialReset(handle, MaterialProperty.ABSORPTION);
		foreach (Spectrum.Point point in data.absorption.points)
		{
			MetaXRAcousticNativeInterface.Interface.AudioMaterialSetFrequency(handle, MaterialProperty.ABSORPTION, point.frequency, point.data);
		}
		MetaXRAcousticNativeInterface.Interface.AudioMaterialReset(handle, MaterialProperty.TRANSMISSION);
		foreach (Spectrum.Point point2 in data.transmission.points)
		{
			MetaXRAcousticNativeInterface.Interface.AudioMaterialSetFrequency(handle, MaterialProperty.TRANSMISSION, point2.frequency, point2.data);
		}
		MetaXRAcousticNativeInterface.Interface.AudioMaterialReset(handle, MaterialProperty.SCATTERING);
		foreach (Spectrum.Point point3 in data.scattering.points)
		{
			MetaXRAcousticNativeInterface.Interface.AudioMaterialSetFrequency(handle, MaterialProperty.SCATTERING, point3.frequency, point3.data);
		}
		return true;
	}

	string IMaterialDataProvider.get_name()
	{
		return base.name;
	}
}
