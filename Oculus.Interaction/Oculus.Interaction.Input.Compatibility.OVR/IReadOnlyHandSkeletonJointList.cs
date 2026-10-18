namespace Oculus.Interaction.Input.Compatibility.OVR;

public interface IReadOnlyHandSkeletonJointList
{
	ref readonly HandSkeletonJoint this[int jointId] { get; }
}
