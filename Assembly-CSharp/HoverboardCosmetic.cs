using System;
using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Hoverboard Cosmetic")]
public class HoverboardCosmetic : MonoBehaviour
{
	[Serializable]
	public struct BoardColorRule
	{
		[Tooltip("Material slot index on this skin's mesh renderer to tint with the player's color.")]
		public int materialIndex;

		[Tooltip("Shader color property to set. Defaults to _BaseColor if left blank.")]
		public string colorProperty;

		private static readonly int defaultColorId = Shader.PropertyToID("_BaseColor");

		public void Apply(MeshRenderer renderer, Color color)
		{
			if (!(renderer == null) && materialIndex >= 0 && materialIndex < renderer.sharedMaterials.Length)
			{
				int nameID = (string.IsNullOrEmpty(colorProperty) ? defaultColorId : Shader.PropertyToID(colorProperty));
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(materialPropertyBlock, materialIndex);
				materialPropertyBlock.SetColor(nameID, color);
				renderer.SetPropertyBlock(materialPropertyBlock, materialIndex);
			}
		}
	}

	[Tooltip("Optional: material slots on this skin's mesh that should be tinted to match the player's colour. Leave empty if no part of the skin reacts to player colour.")]
	[SerializeField]
	private BoardColorRule[] colorRules = Array.Empty<BoardColorRule>();

	private MeshRenderer colorRenderer;

	public void Apply(Color boardColor)
	{
		if (colorRules.Length != 0)
		{
			ApplyColor(boardColor);
		}
	}

	public void ApplyColor(Color color)
	{
		if (colorRenderer == null)
		{
			colorRenderer = GetComponentInChildren<MeshRenderer>();
		}
		BoardColorRule[] array = colorRules;
		foreach (BoardColorRule boardColorRule in array)
		{
			boardColorRule.Apply(colorRenderer, color);
		}
	}
}
