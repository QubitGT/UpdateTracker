namespace Oculus.Interaction.PoseDetection;

public class TransformFeatureConfigBuilder : FeatureConfigBuilder
{
	public class TrueFalseStateBuilder
	{
		private readonly FeatureStateActiveMode _mode;

		private readonly TransformFeature _transformFeature;

		private readonly FeatureStateDescription[] _states;

		public TransformFeatureConfig Open => new TransformFeatureConfig
		{
			Feature = _transformFeature,
			Mode = _mode,
			State = _states[0].Id
		};

		public TransformFeatureConfig Closed => new TransformFeatureConfig
		{
			Feature = _transformFeature,
			Mode = _mode,
			State = _states[1].Id
		};

		public TrueFalseStateBuilder(FeatureStateActiveMode featureStateActiveMode, TransformFeature transformFeature)
		{
			_mode = featureStateActiveMode;
			_transformFeature = transformFeature;
			_states = TransformFeatureProperties.FeatureDescriptions[_transformFeature].FeatureStates;
		}
	}

	public static BuildCondition<TrueFalseStateBuilder> WristUp { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.WristUp));


	public static BuildCondition<TrueFalseStateBuilder> WristDown { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.WristDown));


	public static BuildCondition<TrueFalseStateBuilder> PalmDown { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.PalmDown));


	public static BuildCondition<TrueFalseStateBuilder> PalmUp { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.PalmUp));


	public static BuildCondition<TrueFalseStateBuilder> PalmTowardsFace { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.PalmTowardsFace));


	public static BuildCondition<TrueFalseStateBuilder> PalmAwayFromFace { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.PalmAwayFromFace));


	public static BuildCondition<TrueFalseStateBuilder> FingersUp { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.FingersUp));


	public static BuildCondition<TrueFalseStateBuilder> FingersDown { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.FingersDown));


	public static BuildCondition<TrueFalseStateBuilder> PinchClear { get; } = new BuildCondition<TrueFalseStateBuilder>((FeatureStateActiveMode mode) => new TrueFalseStateBuilder(mode, TransformFeature.PinchClear));

}
