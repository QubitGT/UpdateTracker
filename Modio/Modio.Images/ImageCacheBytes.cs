namespace Modio.Images;

public class ImageCacheBytes : BaseImageCache<byte[]>
{
	public static readonly ImageCacheBytes Instance = new ImageCacheBytes();

	protected override byte[] Convert(byte[] rawBytes)
	{
		return rawBytes;
	}

	protected override byte[] ConvertToBytes(byte[] image)
	{
		return image;
	}
}
