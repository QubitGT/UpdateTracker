using System;

namespace UnityEngine.Rendering.Universal;

[Obsolete("Deprecated in favor of RTHandle", true)]
public struct RenderTargetHandle
{
	public static readonly RenderTargetHandle CameraTarget = new RenderTargetHandle
	{
		id = -1
	};

	public int id { get; set; }

	private RenderTargetIdentifier rtid { get; set; }

	public RenderTargetHandle(RenderTargetIdentifier renderTargetIdentifier)
	{
		id = -2;
		rtid = renderTargetIdentifier;
	}

	public RenderTargetHandle(RTHandle rtHandle)
	{
		if (rtHandle.nameID == BuiltinRenderTextureType.CameraTarget)
		{
			id = -1;
		}
		else if (rtHandle.name.Length == 0)
		{
			id = -2;
		}
		else
		{
			id = Shader.PropertyToID(rtHandle.name);
		}
		rtid = rtHandle.nameID;
		if (rtHandle.rt != null && id != rtid)
		{
			id = -2;
		}
	}

	internal static RenderTargetHandle GetCameraTarget(ref CameraData cameraData)
	{
		if (cameraData.xr.enabled)
		{
			return new RenderTargetHandle(cameraData.xr.renderTarget);
		}
		return CameraTarget;
	}

	public void Init(string shaderProperty)
	{
		id = Shader.PropertyToID(shaderProperty);
	}

	public void Init(RenderTargetIdentifier renderTargetIdentifier)
	{
		id = -2;
		rtid = renderTargetIdentifier;
	}

	public RenderTargetIdentifier Identifier()
	{
		if (id == -1)
		{
			return BuiltinRenderTextureType.CameraTarget;
		}
		if (id == -2)
		{
			return rtid;
		}
		return new RenderTargetIdentifier(id, 0, CubemapFace.Unknown, -1);
	}

	public bool HasInternalRenderTargetId()
	{
		return id == -2;
	}

	public bool Equals(RenderTargetHandle other)
	{
		if (id == -2 || other.id == -2)
		{
			return Identifier() == other.Identifier();
		}
		return id == other.id;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is RenderTargetHandle)
		{
			return Equals((RenderTargetHandle)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return id;
	}

	public static bool operator ==(RenderTargetHandle c1, RenderTargetHandle c2)
	{
		return c1.Equals(c2);
	}

	public static bool operator !=(RenderTargetHandle c1, RenderTargetHandle c2)
	{
		return !c1.Equals(c2);
	}
}
