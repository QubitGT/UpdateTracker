namespace Oculus.Interaction.Input;

public interface IReadOnlyHandSkeletonJointList
{
	ref readonly HandSkeletonJoint this[int jointId] { get; }
}
