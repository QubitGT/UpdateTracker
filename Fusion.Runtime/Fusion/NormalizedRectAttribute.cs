using UnityEngine;

namespace Fusion;

public class NormalizedRectAttribute : UnityEngine.PropertyAttribute
{
	public bool InvertY;

	public float AspectRatio;

	public NormalizedRectAttribute(bool invertY = true, float aspectRatio = 0f)
	{
		InvertY = invertY;
		AspectRatio = aspectRatio;
	}
}
