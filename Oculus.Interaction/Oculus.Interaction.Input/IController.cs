using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public interface IController
{
	Handedness Handedness { get; }

	float Scale { get; }

	bool IsConnected { get; }

	bool IsPoseValid { get; }

	ControllerInput ControllerInput { get; }

	event Action WhenUpdated;

	bool TryGetPose(out Pose pose);

	bool TryGetPointerPose(out Pose pose);

	bool IsButtonUsageAnyActive(ControllerButtonUsage buttonUsage);

	bool IsButtonUsageAllActive(ControllerButtonUsage buttonUsage);
}
