namespace UnityEngine.Animations.Rigging;

public interface IMultiReferentialConstraintData
{
	int driverValue { get; }

	string driverIntProperty { get; }

	Transform[] sourceObjects { get; }
}
