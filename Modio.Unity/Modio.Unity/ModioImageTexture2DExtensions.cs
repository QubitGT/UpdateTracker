using System;
using System.Threading.Tasks;
using Modio.Images;
using UnityEngine;

namespace Modio.Unity;

public static class ModioImageTexture2DExtensions
{
	public static Task<(Error error, Texture2D texture)> DownloadAsTexture2D(this ImageReference imageReference)
	{
		return ImageCacheTexture2D.Instance.DownloadImage(imageReference);
	}

	public static Task<(Error error, Texture2D texture)> DownloadAsTexture2D<TResolution>(this ModioImageSource<TResolution> imageSource, TResolution resolution) where TResolution : Enum
	{
		return ImageCacheTexture2D.Instance.DownloadImage(imageSource.GetUri(resolution));
	}
}
