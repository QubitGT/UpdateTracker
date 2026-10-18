using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public struct MarkToBaseAdjustmentRecord
{
	[SerializeField]
	private uint m_BaseGlyphID;

	[SerializeField]
	private GlyphAnchorPoint m_BaseGlyphAnchorPoint;

	[SerializeField]
	private uint m_MarkGlyphID;

	[SerializeField]
	private MarkPositionAdjustment m_MarkPositionAdjustment;

	public uint baseGlyphID
	{
		get
		{
			return m_BaseGlyphID;
		}
		set
		{
			m_BaseGlyphID = value;
		}
	}

	public GlyphAnchorPoint baseGlyphAnchorPoint
	{
		get
		{
			return m_BaseGlyphAnchorPoint;
		}
		set
		{
			m_BaseGlyphAnchorPoint = value;
		}
	}

	public uint markGlyphID
	{
		get
		{
			return m_MarkGlyphID;
		}
		set
		{
			m_MarkGlyphID = value;
		}
	}

	public MarkPositionAdjustment markPositionAdjustment
	{
		get
		{
			return m_MarkPositionAdjustment;
		}
		set
		{
			m_MarkPositionAdjustment = value;
		}
	}
}
