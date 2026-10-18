using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public interface IGrabbable
{
	List<Pose> GrabPoints { get; }

	Transform Transform { get; }
}
