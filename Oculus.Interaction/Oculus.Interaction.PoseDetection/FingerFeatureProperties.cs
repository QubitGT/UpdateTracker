using System.Collections.Generic;

namespace Oculus.Interaction.PoseDetection;

public static class FingerFeatureProperties
{
	public static readonly FeatureStateDescription[] CurlFeatureStates = new FeatureStateDescription[3]
	{
		new FeatureStateDescription("0", "Open"),
		new FeatureStateDescription("1", "Neutral"),
		new FeatureStateDescription("2", "Closed")
	};

	public static readonly FeatureStateDescription[] FlexionFeatureStates = new FeatureStateDescription[3]
	{
		new FeatureStateDescription("3", "Open"),
		new FeatureStateDescription("4", "Neutral"),
		new FeatureStateDescription("5", "Closed")
	};

	public static readonly FeatureStateDescription[] AbductionFeatureStates = new FeatureStateDescription[3]
	{
		new FeatureStateDescription("6", "None"),
		new FeatureStateDescription("7", "Closed"),
		new FeatureStateDescription("8", "Open")
	};

	public static readonly FeatureStateDescription[] OppositionFeatureStates = new FeatureStateDescription[3]
	{
		new FeatureStateDescription("9", "Touching"),
		new FeatureStateDescription("10", "Near"),
		new FeatureStateDescription("11", "None")
	};

	public const string FeatureCurlShortHelpText = "Convex angle (in degrees) representing the top 2 joints of the fingers. Angle increases as finger curl becomes closed.";

	public const string FeatureCurlDetailHelpText = "Calculated from the average of the convex angles formed by the 2 bones connected to Joint 2, and 2 bones connected to Joint 3.\nValues above 180 Positive show a curled state, while values below 180 represent hyper-extension.";

	public const string FeatureFlexionShortHelpText = "Convex angle (in degrees) of joint 1 of the finger. Angle increases as finger flexion becomes closed.";

	public const string FeatureFlexionDetailHelpText = "Calculated from the angle between the bones connected to finger Joint 1 around the Z axis of the joint.\nFor fingers, joint 1 is commonly known as the 'Knuckle'; but for the thumb it is alongside the wrist.\nValues above 180 Positive show a curled state, while values below 180 represent hyper-extension.upwards from the palm.";

	public const string FeatureAbductionShortHelpText = "Angle (in degrees) between the given finger, and the next finger towards the pinkie.";

	public const string FeatureAbductionDetailHelpText = "Zero value implies that the two fingers are parallel.\nPositive angles indicate that the fingertips are spread apart.\nSmall negative angles are possible, and indicate that the finger is pressed up against the next finger.";

	public const string FeatureOppositionShortHelpText = "Distance between the tip of the given finger and the tip of the thumb.\nCalculated tracking space, with a 1.0 hand scale.";

	public const string FeatureOppositionDetailHelpText = "Positive values indicate that the fingertips are spread apart.\nNegative values are not possible.";

	public const string FeatureStateThresholdMidpointHelpText = "The angle at which a state will transition from A > B (or B > A)";

	public const string FeatureStateThresholdWidthHelpText = "How far the angle must exceed the midpoint until the transition can occur. This is to prevent rapid flickering at transition edges.";

	public static IReadOnlyDictionary<FingerFeature, FeatureDescription> FeatureDescriptions { get; } = new Dictionary<FingerFeature, FeatureDescription>
	{
		[FingerFeature.Curl] = new FeatureDescription("Convex angle (in degrees) representing the top 2 joints of the fingers. Angle increases as finger curl becomes closed.", "Calculated from the average of the convex angles formed by the 2 bones connected to Joint 2, and 2 bones connected to Joint 3.\nValues above 180 Positive show a curled state, while values below 180 represent hyper-extension.", 180f, 260f, CurlFeatureStates),
		[FingerFeature.Flexion] = new FeatureDescription("Convex angle (in degrees) of joint 1 of the finger. Angle increases as finger flexion becomes closed.", "Calculated from the angle between the bones connected to finger Joint 1 around the Z axis of the joint.\nFor fingers, joint 1 is commonly known as the 'Knuckle'; but for the thumb it is alongside the wrist.\nValues above 180 Positive show a curled state, while values below 180 represent hyper-extension.upwards from the palm.", 180f, 260f, FlexionFeatureStates),
		[FingerFeature.Abduction] = new FeatureDescription("Angle (in degrees) between the given finger, and the next finger towards the pinkie.", "Zero value implies that the two fingers are parallel.\nPositive angles indicate that the fingertips are spread apart.\nSmall negative angles are possible, and indicate that the finger is pressed up against the next finger.", 8f, 90f, AbductionFeatureStates),
		[FingerFeature.Opposition] = new FeatureDescription("Distance between the tip of the given finger and the tip of the thumb.\nCalculated tracking space, with a 1.0 hand scale.", "Positive values indicate that the fingertips are spread apart.\nNegative values are not possible.", 0f, 0.2f, OppositionFeatureStates)
	};

}
