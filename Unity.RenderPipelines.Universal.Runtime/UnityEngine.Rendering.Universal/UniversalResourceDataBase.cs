using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public abstract class UniversalResourceDataBase : ContextItem
{
	internal enum ActiveID
	{
		Camera,
		BackBuffer
	}

	internal bool isAccessible { get; set; }

	internal void InitFrame()
	{
		isAccessible = true;
	}

	internal void EndFrame()
	{
		isAccessible = false;
	}

	protected void CheckAndSetTextureHandle(ref TextureHandle handle, TextureHandle newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			handle = newHandle;
		}
	}

	protected TextureHandle CheckAndGetTextureHandle(ref TextureHandle handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return TextureHandle.nullHandle;
		}
		return handle;
	}

	protected void CheckAndSetTextureHandle(ref TextureHandle[] handle, TextureHandle[] newHandle)
	{
		if (CheckAndWarnAboutAccessibility())
		{
			if (handle == null || handle.Length != newHandle.Length)
			{
				handle = new TextureHandle[newHandle.Length];
			}
			for (int i = 0; i < newHandle.Length; i++)
			{
				handle[i] = newHandle[i];
			}
		}
	}

	protected TextureHandle[] CheckAndGetTextureHandle(ref TextureHandle[] handle)
	{
		if (!CheckAndWarnAboutAccessibility())
		{
			return new TextureHandle[1] { TextureHandle.nullHandle };
		}
		return handle;
	}

	protected bool CheckAndWarnAboutAccessibility()
	{
		if (!isAccessible)
		{
			Debug.LogError("Trying to access Universal Resources outside of the current frame setup.");
		}
		return isAccessible;
	}
}
