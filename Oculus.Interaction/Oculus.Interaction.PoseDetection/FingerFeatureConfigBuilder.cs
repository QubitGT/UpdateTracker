namespace Oculus.Interaction.PoseDetection;

public class FingerFeatureConfigBuilder : FeatureConfigBuilder
{
	public class OpenCloseStateBuilder
	{
		private readonly FeatureStateActiveMode _mode;

		private readonly FingerFeature _fingerFeature;

		private readonly FeatureStateDescription[] _states;

		public ShapeRecognizer.FingerFeatureConfig Open => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = _fingerFeature,
			Mode = _mode,
			State = _states[0].Id
		};

		public ShapeRecognizer.FingerFeatureConfig Neutral => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = _fingerFeature,
			Mode = _mode,
			State = _states[1].Id
		};

		public ShapeRecognizer.FingerFeatureConfig Closed => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = _fingerFeature,
			Mode = _mode,
			State = _states[2].Id
		};

		public OpenCloseStateBuilder(FeatureStateActiveMode featureStateActiveMode, FingerFeature fingerFeature)
		{
			_mode = featureStateActiveMode;
			_fingerFeature = fingerFeature;
			_states = FingerFeatureProperties.FeatureDescriptions[_fingerFeature].FeatureStates;
		}
	}

	public class AbductionStateBuilder
	{
		private readonly FeatureStateActiveMode _mode;

		public ShapeRecognizer.FingerFeatureConfig None => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Abduction,
			Mode = _mode,
			State = FingerFeatureProperties.AbductionFeatureStates[0].Id
		};

		public ShapeRecognizer.FingerFeatureConfig Closed => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Abduction,
			Mode = _mode,
			State = FingerFeatureProperties.AbductionFeatureStates[1].Id
		};

		public ShapeRecognizer.FingerFeatureConfig Open => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Abduction,
			Mode = _mode,
			State = FingerFeatureProperties.AbductionFeatureStates[2].Id
		};

		public AbductionStateBuilder(FeatureStateActiveMode mode)
		{
			_mode = mode;
		}
	}

	public class OppositionStateBuilder
	{
		private readonly FeatureStateActiveMode _mode;

		public ShapeRecognizer.FingerFeatureConfig Touching => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Opposition,
			Mode = _mode,
			State = FingerFeatureProperties.OppositionFeatureStates[0].Id
		};

		public ShapeRecognizer.FingerFeatureConfig Near => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Opposition,
			Mode = _mode,
			State = FingerFeatureProperties.OppositionFeatureStates[1].Id
		};

		public ShapeRecognizer.FingerFeatureConfig None => new ShapeRecognizer.FingerFeatureConfig
		{
			Feature = FingerFeature.Opposition,
			Mode = _mode,
			State = FingerFeatureProperties.OppositionFeatureStates[2].Id
		};

		public OppositionStateBuilder(FeatureStateActiveMode mode)
		{
			_mode = mode;
		}
	}

	public static BuildCondition<OpenCloseStateBuilder> Curl { get; } = new BuildCondition<OpenCloseStateBuilder>((FeatureStateActiveMode mode) => new OpenCloseStateBuilder(mode, FingerFeature.Curl));


	public static BuildCondition<OpenCloseStateBuilder> Flexion { get; } = new BuildCondition<OpenCloseStateBuilder>((FeatureStateActiveMode mode) => new OpenCloseStateBuilder(mode, FingerFeature.Flexion));


	public static BuildCondition<AbductionStateBuilder> Abduction { get; } = new BuildCondition<AbductionStateBuilder>((FeatureStateActiveMode mode) => new AbductionStateBuilder(mode));


	public static BuildCondition<OppositionStateBuilder> Opposition { get; } = new BuildCondition<OppositionStateBuilder>((FeatureStateActiveMode mode) => new OppositionStateBuilder(mode));

}
