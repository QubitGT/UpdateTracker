namespace Oculus.Interaction.PoseDetection;

public class FeatureConfigBuilder
{
	public class BuildCondition<TBuildState>
	{
		public delegate TBuildState BuildStateDelegate(FeatureStateActiveMode mode);

		private readonly BuildStateDelegate _buildStateFn;

		public TBuildState Is => _buildStateFn(FeatureStateActiveMode.Is);

		public TBuildState IsNot => _buildStateFn(FeatureStateActiveMode.IsNot);

		public BuildCondition(BuildStateDelegate buildStateFn)
		{
			_buildStateFn = buildStateFn;
		}
	}
}
