namespace Oculus.Interaction.PoseDetection;

public class FeatureDescription
{
	public string ShortDescription { get; }

	public string Description { get; }

	public float MinValueHint { get; }

	public float MaxValueHint { get; }

	public FeatureStateDescription[] FeatureStates { get; }

	public FeatureDescription(string shortDescription, string description, float minValueHint, float maxValueHint, FeatureStateDescription[] featureStates)
	{
		ShortDescription = shortDescription;
		Description = description;
		MinValueHint = minValueHint;
		MaxValueHint = maxValueHint;
		FeatureStates = featureStates;
	}
}
