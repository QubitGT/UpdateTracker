using System;

namespace UnityEngine.Rendering;

[Serializable]
public sealed class LensFlareDataSRP : ScriptableObject
{
	public LensFlareDataElementSRP[] elements;

	public LensFlareDataSRP()
	{
		elements = null;
	}

	public bool HasAModulateByLightColorElement()
	{
		if (elements != null)
		{
			LensFlareDataElementSRP[] array = elements;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].modulateByLightColor)
				{
					return true;
				}
			}
		}
		return false;
	}
}
