using UnityEngine;

namespace Oculus.Interaction;

public interface IGameObjectFilter
{
	bool Filter(GameObject gameObject);
}
