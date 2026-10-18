using System;

namespace Oculus.Platform;

public class AvatarEditorOptions
{
	private IntPtr Handle;

	public AvatarEditorOptions()
	{
		Handle = CAPI.ovr_AvatarEditorOptions_Create();
	}

	public void SetSourceOverride(string value)
	{
		CAPI.ovr_AvatarEditorOptions_SetSourceOverride(Handle, value);
	}

	public static explicit operator IntPtr(AvatarEditorOptions options)
	{
		return options?.Handle ?? IntPtr.Zero;
	}

	~AvatarEditorOptions()
	{
		CAPI.ovr_AvatarEditorOptions_Destroy(Handle);
	}
}
