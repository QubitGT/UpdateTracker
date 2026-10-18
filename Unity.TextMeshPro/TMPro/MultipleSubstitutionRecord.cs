using System;
using UnityEngine;

namespace TMPro;

[Serializable]
public struct MultipleSubstitutionRecord
{
	[SerializeField]
	private uint m_TargetGlyphID;

	[SerializeField]
	private uint[] m_SubstituteGlyphIDs;

	public uint targetGlyphID
	{
		get
		{
			return m_TargetGlyphID;
		}
		set
		{
			m_TargetGlyphID = value;
		}
	}

	public uint[] substituteGlyphIDs
	{
		get
		{
			return m_SubstituteGlyphIDs;
		}
		set
		{
			m_SubstituteGlyphIDs = value;
		}
	}
}
