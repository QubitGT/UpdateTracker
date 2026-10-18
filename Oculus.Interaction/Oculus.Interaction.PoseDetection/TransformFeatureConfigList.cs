using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public class TransformFeatureConfigList
{
	[SerializeField]
	private List<TransformFeatureConfig> _values;

	public List<TransformFeatureConfig> Values => _values;

	public static TransformFeatureConfigList Create(List<TransformFeatureConfig> values)
	{
		return new TransformFeatureConfigList
		{
			_values = values
		};
	}
}
