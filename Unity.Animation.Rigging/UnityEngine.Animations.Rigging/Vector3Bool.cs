using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct Vector3Bool
{
	public bool x;

	public bool y;

	public bool z;

	public Vector3Bool(bool val)
	{
		x = (y = (z = val));
	}

	public Vector3Bool(bool x, bool y, bool z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}
