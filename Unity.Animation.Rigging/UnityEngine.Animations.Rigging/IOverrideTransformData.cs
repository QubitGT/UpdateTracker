namespace UnityEngine.Animations.Rigging;

public interface IOverrideTransformData
{
	Transform constrainedObject { get; }

	Transform sourceObject { get; }

	int space { get; }

	string positionWeightFloatProperty { get; }

	string rotationWeightFloatProperty { get; }

	string positionVector3Property { get; }

	string rotationVector3Property { get; }
}
