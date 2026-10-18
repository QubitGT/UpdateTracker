using UnityEngine;

namespace Oculus.Interaction;

public interface ISnapPoseDelegate
{
	void TrackElement(int id, Pose p);

	void UntrackElement(int id);

	void SnapElement(int id, Pose pose);

	void UnsnapElement(int id);

	void MoveTrackedElement(int id, Pose p);

	bool SnapPoseForElement(int id, Pose pose, out Pose result);
}
