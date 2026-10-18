using System;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetLabelReference : IKeyEvaluator
{
	[FormerlySerializedAs("m_labelString")]
	[SerializeField]
	private string m_LabelString;

	public string labelString
	{
		get
		{
			return m_LabelString;
		}
		set
		{
			m_LabelString = value;
		}
	}

	public object RuntimeKey
	{
		get
		{
			if (labelString == null)
			{
				labelString = string.Empty;
			}
			return labelString;
		}
	}

	public bool RuntimeKeyIsValid()
	{
		return !string.IsNullOrEmpty(RuntimeKey.ToString());
	}

	public override int GetHashCode()
	{
		return labelString.GetHashCode();
	}
}
