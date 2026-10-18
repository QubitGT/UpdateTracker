using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public struct SurfaceHit
{
	public Vector3 Point { get; set; }

	public Vector3 Normal { get; set; }

	public float Distance { get; set; }
}
