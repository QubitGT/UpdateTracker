using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Gaze;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXROverridesGazeAutoSelect
{
	bool overrideGazeTimeToSelect { get; }

	float gazeTimeToSelect { get; }

	bool overrideTimeToAutoDeselectGaze { get; }

	float timeToAutoDeselectGaze { get; }
}
