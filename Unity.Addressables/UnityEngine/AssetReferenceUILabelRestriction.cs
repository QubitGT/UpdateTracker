using System;
using System.Text;

namespace UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class AssetReferenceUILabelRestriction : AssetReferenceUIRestriction
{
	public string[] m_AllowedLabels;

	public string m_CachedToString;

	public AssetReferenceUILabelRestriction(params string[] allowedLabels)
	{
		m_AllowedLabels = allowedLabels;
	}

	public override bool ValidateAsset(Object obj)
	{
		return true;
	}

	public override bool ValidateAsset(string path)
	{
		return true;
	}

	public override string ToString()
	{
		if (m_CachedToString == null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			string[] allowedLabels = m_AllowedLabels;
			foreach (string value in allowedLabels)
			{
				if (!flag)
				{
					stringBuilder.Append(',');
				}
				flag = false;
				stringBuilder.Append(value);
			}
			m_CachedToString = stringBuilder.ToString();
		}
		return m_CachedToString;
	}
}
