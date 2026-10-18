using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public interface ISurface
{
	Transform Transform { get; }

	bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f);

	bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0f);
}
