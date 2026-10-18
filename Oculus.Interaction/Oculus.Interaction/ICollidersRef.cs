using UnityEngine;

namespace Oculus.Interaction;

public interface ICollidersRef
{
	Collider[] Colliders { get; }
}
