namespace UnityEngine.Animations.Rigging;

public interface IDampedTransformData
{
	Transform constrainedObject { get; }

	Transform sourceObject { get; }

	bool maintainAim { get; }

	string dampPositionFloatProperty { get; }

	string dampRotationFloatProperty { get; }
}
