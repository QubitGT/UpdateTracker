using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public struct GlyphAnchorPoint
{
	[SerializeField]
	private float m_XCoordinate;

	[SerializeField]
	private float m_YCoordinate;

	public float xCoordinate
	{
		get
		{
			return m_XCoordinate;
		}
		set
		{
			m_XCoordinate = value;
		}
	}

	public float yCoordinate
	{
		get
		{
			return m_YCoordinate;
		}
		set
		{
			m_YCoordinate = value;
		}
	}
}
