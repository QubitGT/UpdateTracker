using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class CylinderSurfaceData : ICloneable
{
	public Vector3 startPoint = new Vector3(0f, 0.1f, 0f);

	public Vector3 endPoint = new Vector3(0f, -0.1f, 0f);

	[Range(0f, 360f)]
	public float arcOffset;

	[Range(0f, 360f)]
	[FormerlySerializedAs("angle")]
	public float arcLength = 360f;

	public object Clone()
	{
		return new CylinderSurfaceData
		{
			startPoint = startPoint,
			endPoint = endPoint,
			arcOffset = arcOffset,
			arcLength = arcLength
		};
	}

	public CylinderSurfaceData Mirror()
	{
		return Clone() as CylinderSurfaceData;
	}
}
