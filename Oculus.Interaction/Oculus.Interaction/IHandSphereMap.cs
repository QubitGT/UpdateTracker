using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public interface IHandSphereMap
{
	void GetSpheres(Handedness handedness, HandJointId joint, Pose pose, float scale, List<HandSphere> spheres);
}
