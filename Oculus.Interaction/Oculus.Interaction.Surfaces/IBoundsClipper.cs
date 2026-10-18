using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public interface IBoundsClipper
{
	bool GetLocalBounds(Transform localTo, out Bounds bounds);
}
