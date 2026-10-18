using System;
using UnityEngine;

namespace Liv.Lck.Cosmetics;

[Serializable]
public class RootCosmetic
{
	[Tooltip("The asset (Prefab, Material, Texture, etc.) to be included as a root in the bundle.")]
	public UnityEngine.Object Asset;
}
