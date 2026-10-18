using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public struct MarkToMarkAdjustmentRecord
{
	[SerializeField]
	private uint m_BaseMarkGlyphID;

	[SerializeField]
	private GlyphAnchorPoint m_BaseMarkGlyphAnchorPoint;

	[SerializeField]
	private uint m_CombiningMarkGlyphID;

	[SerializeField]
	private MarkPositionAdjustment m_CombiningMarkPositionAdjustment;

	public uint baseMarkGlyphID
	{
		get
		{
			return m_BaseMarkGlyphID;
		}
		set
		{
			m_BaseMarkGlyphID = value;
		}
	}

	public GlyphAnchorPoint baseMarkGlyphAnchorPoint
	{
		get
		{
			return m_BaseMarkGlyphAnchorPoint;
		}
		set
		{
			m_BaseMarkGlyphAnchorPoint = value;
		}
	}

	public uint combiningMarkGlyphID
	{
		get
		{
			return m_CombiningMarkGlyphID;
		}
		set
		{
			m_CombiningMarkGlyphID = value;
		}
	}

	public MarkPositionAdjustment combiningMarkPositionAdjustment
	{
		get
		{
			return m_CombiningMarkPositionAdjustment;
		}
		set
		{
			m_CombiningMarkPositionAdjustment = value;
		}
	}
}
