using System;
using System.Runtime.InteropServices;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-runtime-controller/#use-apis-in-customized-scripts")]
public class OVRKtxTexture
{
	private const uint KTX_TTF_BC7_RGBA = 6u;

	private const uint KTX_TTF_ASTC_4x4_RGBA = 10u;

	public static bool Load(byte[] data, ref OVRTextureData ktxData)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(data[0]) * data.Length);
		Marshal.Copy(data, 0, intPtr, data.Length);
		IntPtr texture = OVRPlugin.Ktx.LoadKtxFromMemory(intPtr, (uint)data.Length);
		Marshal.FreeHGlobal(intPtr);
		ktxData.width = (int)OVRPlugin.Ktx.GetKtxTextureWidth(texture);
		ktxData.height = (int)OVRPlugin.Ktx.GetKtxTextureHeight(texture);
		bool num = OVRPlugin.Ktx.TranscodeKtxTexture(texture, 6u);
		ktxData.transcodedFormat = TextureFormat.BC7;
		if (!num)
		{
			Debug.LogError("Failed to transcode KTX texture.");
			return false;
		}
		uint ktxTextureSize = OVRPlugin.Ktx.GetKtxTextureSize(texture);
		IntPtr intPtr2 = Marshal.AllocHGlobal((int)ktxTextureSize);
		if (!OVRPlugin.Ktx.GetKtxTextureData(texture, intPtr2, ktxTextureSize))
		{
			Debug.LogError("Failed to get texture data from Ktx texture reference");
			return false;
		}
		byte[] array = new byte[ktxTextureSize];
		Marshal.Copy(intPtr2, array, 0, array.Length);
		Marshal.FreeHGlobal(intPtr2);
		ktxData.data = array;
		OVRPlugin.Ktx.DestroyKtxTexture(texture);
		return true;
	}
}
