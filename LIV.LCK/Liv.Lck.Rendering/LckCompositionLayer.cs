using System;
using UnityEngine;

namespace Liv.Lck.Rendering;

[Serializable]
public class LckCompositionLayer : ILckCompositionLayer
{
	[Tooltip("A descriptive name for this layer. Can be used to find and control it at runtime.")]
	public string Name;

	[Tooltip("The material used to perform the blend.")]
	public Material BlendMaterial;

	public bool IsActive = true;

	public virtual Texture CurrentTexture => null;

	string ILckCompositionLayer.Name
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	Material ILckCompositionLayer.BlendMaterial
	{
		get
		{
			return BlendMaterial;
		}
		set
		{
			BlendMaterial = value;
		}
	}

	bool ILckCompositionLayer.IsActive
	{
		get
		{
			return IsActive;
		}
		set
		{
			IsActive = value;
		}
	}
}
