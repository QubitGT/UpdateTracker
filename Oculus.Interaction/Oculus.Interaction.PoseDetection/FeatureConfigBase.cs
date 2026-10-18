using System;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[Serializable]
public abstract class FeatureConfigBase<TFeature>
{
	[SerializeField]
	private FeatureStateActiveMode _mode;

	[SerializeField]
	private TFeature _feature;

	[SerializeField]
	private string _state;

	public FeatureStateActiveMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public TFeature Feature
	{
		get
		{
			return _feature;
		}
		set
		{
			_feature = value;
		}
	}

	public string State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}
}
