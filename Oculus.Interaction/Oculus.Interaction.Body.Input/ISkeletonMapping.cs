using Oculus.Interaction.Collections;

namespace Oculus.Interaction.Body.Input;

public interface ISkeletonMapping
{
	IEnumerableHashSet<BodyJointId> Joints { get; }

	bool TryGetParentJointId(BodyJointId jointId, out BodyJointId parent);
}
