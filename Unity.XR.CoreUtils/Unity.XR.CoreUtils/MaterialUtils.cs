using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.XR.CoreUtils;

public static class MaterialUtils
{
	public static Material GetMaterialClone(Renderer renderer)
	{
		return renderer.material = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
	}

	public static Material GetMaterialClone(Graphic graphic)
	{
		return graphic.material = UnityEngine.Object.Instantiate(graphic.material);
	}

	public static Material[] CloneMaterials(Renderer renderer)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			sharedMaterials[i] = UnityEngine.Object.Instantiate(sharedMaterials[i]);
		}
		renderer.sharedMaterials = sharedMaterials;
		return sharedMaterials;
	}

	public static Color HexToColor(string hex)
	{
		hex = hex.Replace("0x", "").Replace("#", "");
		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		byte a = ((hex.Length == 8) ? byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) : byte.MaxValue);
		return new Color32(r, g, b, a);
	}

	public static Color HueShift(Color color, float shift)
	{
		Vector3 vector = default(Vector3);
		Color.RGBToHSV(color, out vector.x, out vector.y, out vector.z);
		vector.x = Mathf.Repeat(vector.x + shift, 1f);
		return Color.HSVToRGB(vector.x, vector.y, vector.z);
	}

	public static void AddMaterial(this Renderer renderer, Material material)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		int num = sharedMaterials.Length;
		Material[] array = new Material[num + 1];
		Array.Copy(sharedMaterials, array, num);
		array[num] = material;
		renderer.sharedMaterials = array;
	}
}
