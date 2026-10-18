using System;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[Serializable]
public struct AutoUnwrapSettings
{
	public enum Anchor
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
		None
	}

	public enum Fill
	{
		Fit,
		Tile,
		Stretch
	}

	[SerializeField]
	[FormerlySerializedAs("useWorldSpace")]
	private bool m_UseWorldSpace;

	[SerializeField]
	[FormerlySerializedAs("flipU")]
	private bool m_FlipU;

	[SerializeField]
	[FormerlySerializedAs("flipV")]
	private bool m_FlipV;

	[SerializeField]
	[FormerlySerializedAs("swapUV")]
	private bool m_SwapUV;

	[SerializeField]
	[FormerlySerializedAs("fill")]
	private Fill m_Fill;

	[SerializeField]
	[FormerlySerializedAs("scale")]
	private Vector2 m_Scale;

	[SerializeField]
	[FormerlySerializedAs("offset")]
	private Vector2 m_Offset;

	[SerializeField]
	[FormerlySerializedAs("rotation")]
	private float m_Rotation;

	[SerializeField]
	[FormerlySerializedAs("anchor")]
	private Anchor m_Anchor;

	public static AutoUnwrapSettings defaultAutoUnwrapSettings
	{
		get
		{
			AutoUnwrapSettings result = default(AutoUnwrapSettings);
			result.Reset();
			return result;
		}
	}

	public bool useWorldSpace
	{
		get
		{
			return m_UseWorldSpace;
		}
		set
		{
			m_UseWorldSpace = value;
		}
	}

	public bool flipU
	{
		get
		{
			return m_FlipU;
		}
		set
		{
			m_FlipU = value;
		}
	}

	public bool flipV
	{
		get
		{
			return m_FlipV;
		}
		set
		{
			m_FlipV = value;
		}
	}

	public bool swapUV
	{
		get
		{
			return m_SwapUV;
		}
		set
		{
			m_SwapUV = value;
		}
	}

	public Fill fill
	{
		get
		{
			return m_Fill;
		}
		set
		{
			m_Fill = value;
		}
	}

	public Vector2 scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			m_Scale = value;
		}
	}

	public Vector2 offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
		}
	}

	public float rotation
	{
		get
		{
			return m_Rotation;
		}
		set
		{
			m_Rotation = value;
		}
	}

	public Anchor anchor
	{
		get
		{
			return m_Anchor;
		}
		set
		{
			m_Anchor = value;
		}
	}

	public static AutoUnwrapSettings tile
	{
		get
		{
			AutoUnwrapSettings result = default(AutoUnwrapSettings);
			result.Reset();
			return result;
		}
	}

	public static AutoUnwrapSettings fit
	{
		get
		{
			AutoUnwrapSettings result = default(AutoUnwrapSettings);
			result.Reset();
			result.fill = Fill.Fit;
			return result;
		}
	}

	public static AutoUnwrapSettings stretch
	{
		get
		{
			AutoUnwrapSettings result = default(AutoUnwrapSettings);
			result.Reset();
			result.fill = Fill.Stretch;
			return result;
		}
	}

	public AutoUnwrapSettings(AutoUnwrapSettings unwrapSettings)
	{
		m_UseWorldSpace = unwrapSettings.m_UseWorldSpace;
		m_FlipU = unwrapSettings.m_FlipU;
		m_FlipV = unwrapSettings.m_FlipV;
		m_SwapUV = unwrapSettings.m_SwapUV;
		m_Fill = unwrapSettings.m_Fill;
		m_Scale = unwrapSettings.m_Scale;
		m_Offset = unwrapSettings.m_Offset;
		m_Rotation = unwrapSettings.m_Rotation;
		m_Anchor = unwrapSettings.m_Anchor;
	}

	public void Reset()
	{
		m_UseWorldSpace = false;
		m_FlipU = false;
		m_FlipV = false;
		m_SwapUV = false;
		m_Fill = Fill.Tile;
		m_Scale = new Vector2(1f, 1f);
		m_Offset = new Vector2(0f, 0f);
		m_Rotation = 0f;
		m_Anchor = Anchor.None;
	}

	public override string ToString()
	{
		return "Use World Space: " + useWorldSpace + "\nFlip U: " + flipU + "\nFlip V: " + flipV + "\nSwap UV: " + swapUV + "\nFill Mode: " + fill.ToString() + "\nAnchor: " + anchor.ToString() + "\nScale: " + scale.ToString() + "\nOffset: " + offset.ToString() + "\nRotation: " + rotation;
	}
}
