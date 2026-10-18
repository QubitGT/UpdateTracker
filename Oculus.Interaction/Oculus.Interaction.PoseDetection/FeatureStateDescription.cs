namespace Oculus.Interaction.PoseDetection;

public class FeatureStateDescription
{
	public string Id { get; }

	public string Name { get; }

	public FeatureStateDescription(string id, string name)
	{
		Id = id;
		Name = name;
	}
}
