using System;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class ColorAffordanceTheme : BaseAffordanceTheme<Color>
{
	[Header("Color Blend Configuration")]
	[SerializeField]
	[Tooltip("- Solid: Replaces the target value directly.\n- Add: Adds initial color to target color.\n- Mix: Blends initial and target color.")]
	private ColorBlendMode m_ColorBlendMode;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Value between 0 and 1 used to compute color blend modes.")]
	private float m_BlendAmount = 1f;

	public ColorBlendMode colorBlendMode
	{
		get
		{
			return m_ColorBlendMode;
		}
		set
		{
			m_ColorBlendMode = value;
		}
	}

	public float blendAmount
	{
		get
		{
			return m_BlendAmount;
		}
		set
		{
			m_BlendAmount = value;
		}
	}

	public override void CopyFrom(BaseAffordanceTheme<Color> other)
	{
		base.CopyFrom(other);
		ColorAffordanceTheme colorAffordanceTheme = (ColorAffordanceTheme)other;
		colorBlendMode = colorAffordanceTheme.colorBlendMode;
		blendAmount = colorAffordanceTheme.blendAmount;
	}
}
