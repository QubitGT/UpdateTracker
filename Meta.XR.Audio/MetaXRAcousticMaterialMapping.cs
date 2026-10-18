using System;
using UnityEngine;

internal class MetaXRAcousticMaterialMapping : ScriptableObject
{
	[Serializable]
	internal class Pair
	{
		[SerializeField]
		internal PhysicsMaterial physicMaterial;

		[SerializeField]
		internal MetaXRAcousticMaterialProperties acousticMaterial;
	}

	[HideInInspector]
	[SerializeField]
	internal Pair[] mapping;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Acoustic material to apply when there is no physics material.")]
	internal MetaXRAcousticMaterialProperties fallbackMaterial;

	private static MetaXRAcousticMaterialMapping instance;

	internal static MetaXRAcousticMaterialMapping Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<MetaXRAcousticMaterialMapping>("MetaXRAcousticMaterialMapping");
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<MetaXRAcousticMaterialMapping>();
				}
			}
			return instance;
		}
	}

	internal MetaXRAcousticMaterialProperties findAcousticMaterial(PhysicsMaterial pmat)
	{
		if (pmat == null || mapping == null || mapping.Length == 0)
		{
			return fallbackMaterial;
		}
		return Array.Find(mapping, (Pair pair) => object.Equals(pair.physicMaterial, pmat))?.acousticMaterial;
	}
}
