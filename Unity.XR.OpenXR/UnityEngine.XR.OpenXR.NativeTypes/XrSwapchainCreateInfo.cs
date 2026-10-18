namespace UnityEngine.XR.OpenXR.NativeTypes;

public struct XrSwapchainCreateInfo
{
	public uint Type;

	public unsafe void* Next;

	public ulong CreateFlags;

	public ulong UsageFlags;

	public long Format;

	public uint SampleCount;

	public uint Width;

	public uint Height;

	public uint FaceCount;

	public uint ArraySize;

	public uint MipCount;
}
