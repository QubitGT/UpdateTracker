using System;

namespace Liv.Lck;

[Serializable]
public struct CameraResolutionDescriptor
{
	public uint Width;

	public uint Height;

	public CameraResolutionDescriptor(uint width = 512u, uint height = 512u)
	{
		Width = width;
		Height = height;
	}

	public bool IsValid()
	{
		if (Width != 0)
		{
			return Height != 0;
		}
		return false;
	}

	public CameraResolutionDescriptor GetResolutionInOrientation(LckCameraOrientation orientation)
	{
		return orientation switch
		{
			LckCameraOrientation.Landscape => new CameraResolutionDescriptor(Math.Max(Width, Height), Math.Min(Width, Height)), 
			LckCameraOrientation.Portrait => new CameraResolutionDescriptor(Math.Min(Width, Height), Math.Max(Width, Height)), 
			_ => this, 
		};
	}
}
