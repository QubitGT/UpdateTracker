using System;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class SphereGrabSurfaceData : ICloneable
{
	public Vector3 centre = Vector3.zero;

	public object Clone()
	{
		return new SphereGrabSurfaceData
		{
			centre = centre
		};
	}

	public SphereGrabSurfaceData Mirror()
	{
		return Clone() as SphereGrabSurfaceData;
	}
}
