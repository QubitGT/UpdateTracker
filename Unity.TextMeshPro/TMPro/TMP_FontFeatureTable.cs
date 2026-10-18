using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TMPro;

[Serializable]
public class TMP_FontFeatureTable
{
	[SerializeField]
	internal List<MultipleSubstitutionRecord> m_MultipleSubstitutionRecords;

	[SerializeField]
	internal List<LigatureSubstitutionRecord> m_LigatureSubstitutionRecords;

	[SerializeField]
	internal List<GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecords;

	[SerializeField]
	internal List<MarkToBaseAdjustmentRecord> m_MarkToBaseAdjustmentRecords;

	[SerializeField]
	internal List<MarkToMarkAdjustmentRecord> m_MarkToMarkAdjustmentRecords;

	internal Dictionary<uint, List<LigatureSubstitutionRecord>> m_LigatureSubstitutionRecordLookup;

	internal Dictionary<uint, GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecordLookup;

	internal Dictionary<uint, MarkToBaseAdjustmentRecord> m_MarkToBaseAdjustmentRecordLookup;

	internal Dictionary<uint, MarkToMarkAdjustmentRecord> m_MarkToMarkAdjustmentRecordLookup;

	public List<MultipleSubstitutionRecord> multipleSubstitutionRecords
	{
		get
		{
			return m_MultipleSubstitutionRecords;
		}
		set
		{
			m_MultipleSubstitutionRecords = value;
		}
	}

	public List<LigatureSubstitutionRecord> ligatureRecords
	{
		get
		{
			return m_LigatureSubstitutionRecords;
		}
		set
		{
			m_LigatureSubstitutionRecords = value;
		}
	}

	public List<GlyphPairAdjustmentRecord> glyphPairAdjustmentRecords
	{
		get
		{
			return m_GlyphPairAdjustmentRecords;
		}
		set
		{
			m_GlyphPairAdjustmentRecords = value;
		}
	}

	public List<MarkToBaseAdjustmentRecord> MarkToBaseAdjustmentRecords
	{
		get
		{
			return m_MarkToBaseAdjustmentRecords;
		}
		set
		{
			m_MarkToBaseAdjustmentRecords = value;
		}
	}

	public List<MarkToMarkAdjustmentRecord> MarkToMarkAdjustmentRecords
	{
		get
		{
			return m_MarkToMarkAdjustmentRecords;
		}
		set
		{
			m_MarkToMarkAdjustmentRecords = value;
		}
	}

	public TMP_FontFeatureTable()
	{
		m_LigatureSubstitutionRecords = new List<LigatureSubstitutionRecord>();
		m_LigatureSubstitutionRecordLookup = new Dictionary<uint, List<LigatureSubstitutionRecord>>();
		m_GlyphPairAdjustmentRecords = new List<GlyphPairAdjustmentRecord>();
		m_GlyphPairAdjustmentRecordLookup = new Dictionary<uint, GlyphPairAdjustmentRecord>();
		m_MarkToBaseAdjustmentRecords = new List<MarkToBaseAdjustmentRecord>();
		m_MarkToBaseAdjustmentRecordLookup = new Dictionary<uint, MarkToBaseAdjustmentRecord>();
		m_MarkToMarkAdjustmentRecords = new List<MarkToMarkAdjustmentRecord>();
		m_MarkToMarkAdjustmentRecordLookup = new Dictionary<uint, MarkToMarkAdjustmentRecord>();
	}

	public void SortGlyphPairAdjustmentRecords()
	{
		if (m_GlyphPairAdjustmentRecords.Count > 0)
		{
			m_GlyphPairAdjustmentRecords = (from s in m_GlyphPairAdjustmentRecords
				orderby s.firstAdjustmentRecord.glyphIndex, s.secondAdjustmentRecord.glyphIndex
				select s).ToList();
		}
	}

	public void SortMarkToBaseAdjustmentRecords()
	{
		if (m_MarkToBaseAdjustmentRecords.Count > 0)
		{
			m_MarkToBaseAdjustmentRecords = (from s in m_MarkToBaseAdjustmentRecords
				orderby s.baseGlyphID, s.markGlyphID
				select s).ToList();
		}
	}

	public void SortMarkToMarkAdjustmentRecords()
	{
		if (m_MarkToMarkAdjustmentRecords.Count > 0)
		{
			m_MarkToMarkAdjustmentRecords = (from s in m_MarkToMarkAdjustmentRecords
				orderby s.baseMarkGlyphID, s.combiningMarkGlyphID
				select s).ToList();
		}
	}
}
