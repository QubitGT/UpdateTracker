using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

public static class TransformFeatureProperties
{
	public const string FeatureStateThresholdMidpointHelpText = "The value at which a state will transition from A > B (or B > A)";

	public const string FeatureStateThresholdWidthHelpText = "How far the transform value must exceed the midpoint until the transition can occur. This is to prevent rapid flickering at transition edges.";

	public static IReadOnlyDictionary<TransformFeature, FeatureDescription> FeatureDescriptions { get; } = CreateFeatureDescriptions();


	private static IReadOnlyDictionary<TransformFeature, FeatureDescription> CreateFeatureDescriptions()
	{
		int startIndex = 0;
		return new Dictionary<TransformFeature, FeatureDescription>
		{
			[TransformFeature.WristUp] = CreateDesc(ref startIndex),
			[TransformFeature.WristDown] = CreateDesc(ref startIndex),
			[TransformFeature.PalmDown] = CreateDesc(ref startIndex),
			[TransformFeature.PalmUp] = CreateDesc(ref startIndex),
			[TransformFeature.PalmTowardsFace] = CreateDesc(ref startIndex),
			[TransformFeature.PalmAwayFromFace] = CreateDesc(ref startIndex),
			[TransformFeature.FingersUp] = CreateDesc(ref startIndex),
			[TransformFeature.FingersDown] = CreateDesc(ref startIndex),
			[TransformFeature.PinchClear] = CreateDesc(ref startIndex)
		};
	}

	private static FeatureDescription CreateDesc(ref int startIndex)
	{
		FeatureDescription result = new FeatureDescription("", "", 0f, 180f, new FeatureStateDescription[2]
		{
			new FeatureStateDescription(startIndex.ToString(), "True"),
			new FeatureStateDescription((startIndex + 2).ToString(), "False")
		});
		startIndex += 3;
		return result;
	}
}
