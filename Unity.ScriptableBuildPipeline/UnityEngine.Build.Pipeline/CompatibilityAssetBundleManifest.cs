using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Build.Pipeline;

[Serializable]
public class CompatibilityAssetBundleManifest : ScriptableObject, ISerializationCallbackReceiver
{
	private Dictionary<string, BundleDetails> m_Details;

	[SerializeField]
	private List<string> m_Keys;

	[SerializeField]
	private List<BundleDetails> m_Values;

	public void SetResults(Dictionary<string, BundleDetails> results)
	{
		m_Details = new Dictionary<string, BundleDetails>(results);
	}

	public string[] GetAllAssetBundles()
	{
		string[] array = m_Details.Keys.ToArray();
		Array.Sort(array);
		return array;
	}

	public string[] GetAllAssetBundlesWithVariant()
	{
		return new string[0];
	}

	public Hash128 GetAssetBundleHash(string assetBundleName)
	{
		if (m_Details.TryGetValue(assetBundleName, out var value))
		{
			return value.Hash;
		}
		return default(Hash128);
	}

	public uint GetAssetBundleCrc(string assetBundleName)
	{
		if (m_Details.TryGetValue(assetBundleName, out var value))
		{
			return value.Crc;
		}
		return 0u;
	}

	public string[] GetDirectDependencies(string assetBundleName)
	{
		return GetAllDependencies(assetBundleName);
	}

	public string[] GetAllDependencies(string assetBundleName)
	{
		if (m_Details.TryGetValue(assetBundleName, out var value))
		{
			return value.Dependencies.ToArray();
		}
		return new string[0];
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ManifestFileVersion: 1\n");
		stringBuilder.Append("CompatibilityAssetBundleManifest:\n");
		if (m_Details != null && m_Details.Count > 0)
		{
			stringBuilder.Append("  AssetBundleInfos:\n");
			int num = 0;
			foreach (KeyValuePair<string, BundleDetails> detail in m_Details)
			{
				stringBuilder.AppendFormat("    Info_{0}:\n", num++);
				stringBuilder.AppendFormat("      Name: {0}\n", detail.Key);
				stringBuilder.AppendFormat("      Hash: {0}\n", detail.Value.Hash);
				stringBuilder.AppendFormat("      CRC: {0}\n", detail.Value.Crc);
				int num2 = 0;
				if (detail.Value.Dependencies != null && detail.Value.Dependencies.Length != 0)
				{
					stringBuilder.Append("      Dependencies: {}\n");
					string[] dependencies = detail.Value.Dependencies;
					foreach (string arg in dependencies)
					{
						stringBuilder.AppendFormat("        Dependency_{0}: {1}\n", num2++, arg);
					}
				}
				else
				{
					stringBuilder.Append("      Dependencies: {}\n");
				}
			}
		}
		else
		{
			stringBuilder.Append("  AssetBundleInfos: {}\n");
		}
		return stringBuilder.ToString();
	}

	public void OnBeforeSerialize()
	{
		m_Keys = new List<string>();
		m_Values = new List<BundleDetails>();
		foreach (KeyValuePair<string, BundleDetails> detail in m_Details)
		{
			m_Keys.Add(detail.Key);
			m_Values.Add(detail.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		m_Details = new Dictionary<string, BundleDetails>();
		for (int i = 0; i != Math.Min(m_Keys.Count, m_Values.Count); i++)
		{
			m_Details.Add(m_Keys[i], m_Values[i]);
		}
	}
}
