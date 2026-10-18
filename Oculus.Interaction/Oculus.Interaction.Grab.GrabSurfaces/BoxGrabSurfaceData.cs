using System;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class BoxGrabSurfaceData : ICloneable
{
	[Range(0f, 1f)]
	public float widthOffset = 0.5f;

	public Vector4 snapOffset;

	public Vector3 size = new Vector3(0.1f, 0f, 0.1f);

	public Vector3 eulerAngles;

	public object Clone()
	{
		return new BoxGrabSurfaceData
		{
			widthOffset = widthOffset,
			snapOffset = snapOffset,
			size = size,
			eulerAngles = eulerAngles
		};
	}

	public BoxGrabSurfaceData Mirror()
	{
		BoxGrabSurfaceData boxGrabSurfaceData = Clone() as BoxGrabSurfaceData;
		boxGrabSurfaceData.snapOffset = new Vector4(0f - boxGrabSurfaceData.snapOffset.y, 0f - boxGrabSurfaceData.snapOffset.x, 0f - boxGrabSurfaceData.snapOffset.w, 0f - boxGrabSurfaceData.snapOffset.z);
		return boxGrabSurfaceData;
	}
}
