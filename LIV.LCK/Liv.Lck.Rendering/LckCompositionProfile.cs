using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Liv.Lck.Rendering;

[CreateAssetMenu(fileName = "Lck Composition Profile", menuName = "LIV/LCK/Composition Profile")]
public class LckCompositionProfile : ScriptableObject
{
	[SerializeReference]
	[Tooltip("The list of layers to be composed. Order matters.")]
	public List<LckCompositionLayer> Layers = new List<LckCompositionLayer>();

	public void SetOrientation(bool isHorizontal)
	{
		Debug.Log("LCK SetOrientation 0");
		if (Layers == null)
		{
			return;
		}
		foreach (LckCompositionLayer layer in Layers)
		{
			Debug.Log("LCK SetOrientation 1");
			if (layer is ILckOrientationAwareLayer lckOrientationAwareLayer)
			{
				Debug.Log("LCK SetOrientation 2");
				lckOrientationAwareLayer.SetOrientation(isHorizontal);
			}
		}
		LckCompositionEngine.Instance?.SetDirty();
	}

	public T GetLayer<T>(string name) where T : LckCompositionLayer
	{
		if (Layers == null)
		{
			return null;
		}
		return Layers.FirstOrDefault((LckCompositionLayer layer) => layer.Name == name) as T;
	}

	public void SetLayerActive(string name, bool isActive)
	{
		LckCompositionLayer layer = GetLayer<LckCompositionLayer>(name);
		if (layer != null)
		{
			layer.IsActive = isActive;
			LckCompositionEngine.Instance?.SetDirty();
		}
	}

	public List<ILckCompositionLayer> GetActiveLayers()
	{
		List<ILckCompositionLayer> list = new List<ILckCompositionLayer>();
		if (Layers != null)
		{
			foreach (LckCompositionLayer layer in Layers)
			{
				if (layer != null && layer.IsActive && layer.CurrentTexture != null)
				{
					list.Add(layer);
				}
			}
		}
		return list;
	}
}
