using System;

namespace Modio.Images;

public class LazyImage<TImage> where TImage : class
{
	private ImageReference _currentImageReference;

	private BaseImageCache<TImage> _imageCache;

	private bool _failedToLoad;

	public event Action<TImage> OnNewImageAvailable;

	public event Action<bool> OnLoadingActive;

	public LazyImage(BaseImageCache<TImage> imageCache, Action<TImage> onImageAvailable = null, Action<bool> onLoadingActive = null)
	{
		_imageCache = imageCache;
		this.OnNewImageAvailable = onImageAvailable;
		this.OnLoadingActive = onLoadingActive;
	}

	public async void SetImage<T>(ModioImageSource<T> source, T resolution) where T : Enum
	{
		ImageReference uri = source.GetUri(resolution);
		if (uri != _currentImageReference)
		{
			_failedToLoad = false;
		}
		else if (_failedToLoad)
		{
			return;
		}
		_currentImageReference = uri;
		TImage cachedImage = _imageCache.GetCachedImage(_currentImageReference);
		if (cachedImage != null)
		{
			ApplyImage(cachedImage);
			return;
		}
		TImage firstCachedImage = _imageCache.GetFirstCachedImage(source.GetAllReferences());
		if (firstCachedImage != null)
		{
			ApplyImage(firstCachedImage);
		}
		this.OnLoadingActive?.Invoke(obj: true);
		ImageReference currentlyDownloading = _currentImageReference;
		var (error, cachedImage2) = await _imageCache.DownloadImage(_currentImageReference);
		if (_currentImageReference == currentlyDownloading)
		{
			if ((bool)error)
			{
				_failedToLoad = true;
			}
			ApplyImage(cachedImage2);
		}
		this.OnLoadingActive?.Invoke(obj: false);
	}

	private void ApplyImage(TImage cachedImage)
	{
		this.OnNewImageAvailable?.Invoke(cachedImage);
	}
}
