using Modio.Images;
using UnityEngine;

namespace Modio.Unity;

public class ImageCacheTexture2D : BaseImageCache<Texture2D>
{
	public static readonly ImageCacheTexture2D Instance = new ImageCacheTexture2D();

	protected override Texture2D Convert(byte[] rawBytes)
	{
		if (rawBytes == null || rawBytes.Length == 0)
		{
			ModioLog.Verbose?.Log(":INTERNAL: Attempted to parse image from NULL/0-length buffer.");
			return null;
		}
		Texture2D texture2D = new Texture2D(0, 0);
		if (texture2D.LoadImage(rawBytes, markNonReadable: false))
		{
			return texture2D;
		}
		ModioLog.Verbose?.Log(":INTERNAL: Failed to parse image data.");
		return null;
	}

	protected override byte[] ConvertToBytes(Texture2D image)
	{
		if (!(image != null))
		{
			return null;
		}
		return image.EncodeToPNG();
	}
}
