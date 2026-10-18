using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Detection/Shape")]
public class ShapeRecognizer : ScriptableObject
{
	[Serializable]
	public class FingerFeatureConfigList
	{
		[SerializeField]
		private List<FingerFeatureConfig> _value;

		public IReadOnlyList<FingerFeatureConfig> Value => _value;

		public FingerFeatureConfigList()
		{
		}

		public FingerFeatureConfigList(List<FingerFeatureConfig> value)
		{
			_value = value;
		}
	}

	[Serializable]
	public class FingerFeatureConfig : FeatureConfigBase<FingerFeature>
	{
	}

	[SerializeField]
	private string _shapeName;

	[SerializeField]
	private FingerFeatureConfigList _thumbFeatureConfigs = new FingerFeatureConfigList();

	[SerializeField]
	private FingerFeatureConfigList _indexFeatureConfigs = new FingerFeatureConfigList();

	[SerializeField]
	private FingerFeatureConfigList _middleFeatureConfigs = new FingerFeatureConfigList();

	[SerializeField]
	private FingerFeatureConfigList _ringFeatureConfigs = new FingerFeatureConfigList();

	[SerializeField]
	private FingerFeatureConfigList _pinkyFeatureConfigs = new FingerFeatureConfigList();

	public IReadOnlyList<FingerFeatureConfig> ThumbFeatureConfigs => _thumbFeatureConfigs.Value;

	public IReadOnlyList<FingerFeatureConfig> IndexFeatureConfigs => _indexFeatureConfigs.Value;

	public IReadOnlyList<FingerFeatureConfig> MiddleFeatureConfigs => _middleFeatureConfigs.Value;

	public IReadOnlyList<FingerFeatureConfig> RingFeatureConfigs => _ringFeatureConfigs.Value;

	public IReadOnlyList<FingerFeatureConfig> PinkyFeatureConfigs => _pinkyFeatureConfigs.Value;

	public string ShapeName => _shapeName;

	public IReadOnlyList<FingerFeatureConfig> GetFingerFeatureConfigs(HandFinger finger)
	{
		return finger switch
		{
			HandFinger.Thumb => ThumbFeatureConfigs, 
			HandFinger.Index => IndexFeatureConfigs, 
			HandFinger.Middle => MiddleFeatureConfigs, 
			HandFinger.Ring => RingFeatureConfigs, 
			HandFinger.Pinky => PinkyFeatureConfigs, 
			_ => throw new ArgumentException("must be a HandFinger enum value", "finger"), 
		};
	}

	public IEnumerable<(HandFinger, IReadOnlyList<FingerFeatureConfig>)> GetFingerFeatureConfigs()
	{
		int fingerIdx = 0;
		while (fingerIdx < 5)
		{
			HandFinger handFinger = (HandFinger)fingerIdx;
			IReadOnlyList<FingerFeatureConfig> fingerFeatureConfigs = GetFingerFeatureConfigs(handFinger);
			if (fingerFeatureConfigs.Count != 0)
			{
				yield return (handFinger, fingerFeatureConfigs);
			}
			int num = fingerIdx + 1;
			fingerIdx = num;
		}
	}

	public void InjectAllShapeRecognizer(IDictionary<HandFinger, FingerFeatureConfig[]> fingerFeatureConfigs)
	{
		_thumbFeatureConfigs = ReadFeatureConfigs(HandFinger.Thumb);
		_indexFeatureConfigs = ReadFeatureConfigs(HandFinger.Index);
		_middleFeatureConfigs = ReadFeatureConfigs(HandFinger.Middle);
		_ringFeatureConfigs = ReadFeatureConfigs(HandFinger.Ring);
		_pinkyFeatureConfigs = ReadFeatureConfigs(HandFinger.Pinky);
		FingerFeatureConfigList ReadFeatureConfigs(HandFinger finger)
		{
			if (!fingerFeatureConfigs.TryGetValue(finger, out var value))
			{
				value = Array.Empty<FingerFeatureConfig>();
			}
			return new FingerFeatureConfigList(new List<FingerFeatureConfig>(value));
		}
	}

	public void InjectThumbFeatureConfigs(FingerFeatureConfig[] configs)
	{
		_thumbFeatureConfigs = new FingerFeatureConfigList(new List<FingerFeatureConfig>(configs));
	}

	public void InjectIndexFeatureConfigs(FingerFeatureConfig[] configs)
	{
		_indexFeatureConfigs = new FingerFeatureConfigList(new List<FingerFeatureConfig>(configs));
	}

	public void InjectMiddleFeatureConfigs(FingerFeatureConfig[] configs)
	{
		_middleFeatureConfigs = new FingerFeatureConfigList(new List<FingerFeatureConfig>(configs));
	}

	public void InjectRingFeatureConfigs(FingerFeatureConfig[] configs)
	{
		_ringFeatureConfigs = new FingerFeatureConfigList(new List<FingerFeatureConfig>(configs));
	}

	public void InjectPinkyFeatureConfigs(FingerFeatureConfig[] configs)
	{
		_pinkyFeatureConfigs = new FingerFeatureConfigList(new List<FingerFeatureConfig>(configs));
	}

	public void InjectShapeName(string shapeName)
	{
		_shapeName = shapeName;
	}
}
