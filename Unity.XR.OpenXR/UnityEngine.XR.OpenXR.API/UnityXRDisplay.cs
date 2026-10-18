using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.API;

public static class UnityXRDisplay
{
	public const uint kUnityXRRenderTextureIdDontCare = 0u;

	private const string k_UnityOpenXRLib = "UnityOpenXR";

	[DllImport("UnityOpenXR", EntryPoint = "Display_CreateTexture")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CreateTexture(UnityXRRenderTextureDesc desc, out uint id);

	[DllImport("UnityOpenXR", EntryPoint = "Display_DestroyTexture")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool DestroyTexture(uint textureId);
}
