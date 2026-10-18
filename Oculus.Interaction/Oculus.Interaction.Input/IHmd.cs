using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public interface IHmd
{
	event Action WhenUpdated;

	bool TryGetRootPose(out Pose pose);
}
