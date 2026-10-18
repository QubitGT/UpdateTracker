using System;
using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

public class FeatureStateProvider<TFeature, TFeatureState> where TFeature : unmanaged, Enum where TFeatureState : IEquatable<TFeatureState>
{
	private struct FeatureStateSnapshot
	{
		public bool HasCurrentState;

		public TFeatureState State;

		public TFeatureState DesiredState;

		public int LastUpdatedFrameId;

		public double DesiredStateEntryTime;
	}

	private FeatureStateSnapshot[] _featureToCurrentState;

	private IFeatureStateThresholds<TFeature, TFeatureState>[] _featureToThresholds;

	private readonly Func<TFeature, float?> _valueReader;

	private readonly Func<TFeature, int> _featureToInt;

	private readonly Func<float> _timeProvider;

	private static readonly TFeature[] FeatureEnumValues = (TFeature[])Enum.GetValues(typeof(TFeature));

	private IFeatureThresholds<TFeature, TFeatureState> _featureThresholds;

	public int LastUpdatedFrameId { get; set; }

	private int EnumToInt(TFeature value)
	{
		return _featureToInt(value);
	}

	public FeatureStateProvider(Func<TFeature, float?> valueReader, Func<TFeature, int> featureToInt, Func<float> timeProvider)
	{
		_valueReader = valueReader;
		_featureToInt = featureToInt;
		_timeProvider = timeProvider;
	}

	public void InitializeThresholds(IFeatureThresholds<TFeature, TFeatureState> featureThresholds)
	{
		_featureThresholds = featureThresholds;
		_featureToThresholds = ValidateFeatureThresholds(featureThresholds.FeatureStateThresholds);
		InitializeStates();
	}

	public IFeatureStateThresholds<TFeature, TFeatureState>[] ValidateFeatureThresholds(IReadOnlyList<IFeatureStateThresholds<TFeature, TFeatureState>> featureStateThresholdsList)
	{
		IFeatureStateThresholds<TFeature, TFeatureState>[] array = new IFeatureStateThresholds<TFeature, TFeatureState>[Enum.GetNames(typeof(TFeature)).Length];
		foreach (IFeatureStateThresholds<TFeature, TFeatureState> featureStateThresholds in featureStateThresholdsList)
		{
			int num = EnumToInt(featureStateThresholds.Feature);
			array[num] = featureStateThresholds;
			for (int i = 0; i < featureStateThresholds.Thresholds.Count; i++)
			{
				IFeatureStateThreshold<TFeatureState> featureStateThreshold = featureStateThresholds.Thresholds[i];
				_ = featureStateThreshold.ToFirstWhenBelow;
				_ = featureStateThreshold.ToSecondWhenAbove;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			_ = array[j];
		}
		return array;
	}

	private void InitializeStates()
	{
		_featureToCurrentState = new FeatureStateSnapshot[FeatureEnumValues.Length];
		TFeature[] featureEnumValues = FeatureEnumValues;
		foreach (TFeature value in featureEnumValues)
		{
			int num = EnumToInt(value);
			ref FeatureStateSnapshot reference = ref _featureToCurrentState[num];
			reference.State = default(TFeatureState);
			reference.DesiredState = default(TFeatureState);
			reference.DesiredStateEntryTime = 0.0;
		}
	}

	private ref IFeatureStateThresholds<TFeature, TFeatureState> GetFeatureThresholds(TFeature feature)
	{
		return ref _featureToThresholds[EnumToInt(feature)];
	}

	public TFeatureState GetCurrentFeatureState(TFeature feature)
	{
		ref FeatureStateSnapshot reference = ref _featureToCurrentState[EnumToInt(feature)];
		if (reference.LastUpdatedFrameId == LastUpdatedFrameId)
		{
			return reference.State;
		}
		float? num = _valueReader(feature);
		if (!num.HasValue)
		{
			return reference.State;
		}
		reference.LastUpdatedFrameId = LastUpdatedFrameId;
		IReadOnlyList<IFeatureStateThreshold<TFeatureState>> thresholds = GetFeatureThresholds(feature).Thresholds;
		TFeatureState val = (reference.HasCurrentState ? ReadDesiredState(num.Value, thresholds, reference.State) : ReadDesiredState(num.Value, thresholds));
		if (val.Equals(reference.State))
		{
			return reference.State;
		}
		float num2 = _timeProvider();
		if (!val.Equals(reference.DesiredState))
		{
			reference.DesiredStateEntryTime = num2;
			reference.DesiredState = val;
		}
		if (reference.DesiredStateEntryTime + _featureThresholds.MinTimeInState <= (double)num2)
		{
			reference.HasCurrentState = true;
			reference.State = val;
		}
		return reference.State;
	}

	private TFeatureState ReadDesiredState(float value, IReadOnlyList<IFeatureStateThreshold<TFeatureState>> featureStateThresholds, TFeatureState previousState)
	{
		TFeatureState val = previousState;
		for (int i = 0; i < featureStateThresholds.Count; i++)
		{
			IFeatureStateThreshold<TFeatureState> featureStateThreshold = featureStateThresholds[i];
			if (val.Equals(featureStateThreshold.FirstState) && value > featureStateThreshold.ToSecondWhenAbove)
			{
				return featureStateThreshold.SecondState;
			}
			if (val.Equals(featureStateThreshold.SecondState) && value < featureStateThreshold.ToFirstWhenBelow)
			{
				return featureStateThreshold.FirstState;
			}
		}
		return previousState;
	}

	private TFeatureState ReadDesiredState(float value, IReadOnlyList<IFeatureStateThreshold<TFeatureState>> featureStateThresholds)
	{
		TFeatureState result = default(TFeatureState);
		for (int i = 0; i < featureStateThresholds.Count; i++)
		{
			IFeatureStateThreshold<TFeatureState> featureStateThreshold = featureStateThresholds[i];
			if (value <= featureStateThreshold.ToSecondWhenAbove)
			{
				result = featureStateThreshold.FirstState;
				break;
			}
			result = featureStateThreshold.SecondState;
		}
		return result;
	}

	public void ReadTouchedFeatureStates()
	{
		for (int i = 0; i < _featureToCurrentState.Length; i++)
		{
			if (_featureToCurrentState[i].LastUpdatedFrameId != 0)
			{
				GetCurrentFeatureState(FeatureEnumValues[i]);
			}
		}
	}
}
