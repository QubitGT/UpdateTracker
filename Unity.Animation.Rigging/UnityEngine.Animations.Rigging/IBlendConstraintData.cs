namespace UnityEngine.Animations.Rigging;

public interface IBlendConstraintData
{
	Transform constrainedObject { get; }

	Transform sourceObjectA { get; }

	Transform sourceObjectB { get; }

	bool maintainPositionOffsets { get; }

	bool maintainRotationOffsets { get; }

	string blendPositionBoolProperty { get; }

	string blendRotationBoolProperty { get; }

	string positionWeightFloatProperty { get; }

	string rotationWeightFloatProperty { get; }
}
