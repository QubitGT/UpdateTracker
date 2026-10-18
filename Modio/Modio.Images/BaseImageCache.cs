using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Modio.Errors;

namespace Modio.Images;

public abstract class BaseImageCache
{
	protected static readonly List<BaseImageCache> ImageCacheInstances = new List<BaseImageCache>();

	protected static readonly HashSet<ImageReference> PendingDiskSaves = new HashSet<ImageReference>();

	public static void CacheToDisk(ImageReference image, bool shouldCache)
	{
		if (!shouldCache)
		{
			ModioClient.DataStorage.DeleteCachedImage(new Uri(image.Url));
			PendingDiskSaves.Remove(image);
			return;
		}
		bool flag = false;
		foreach (BaseImageCache imageCacheInstance in ImageCacheInstances)
		{
			flag |= imageCacheInstance.CacheToDiskInternal(image);
		}
		if (!flag)
		{
			PendingDiskSaves.Add(image);
		}
	}

	protected abstract bool CacheToDiskInternal(ImageReference imageReference);
}
public abstract class BaseImageCache<T> : BaseImageCache where T : class
{
	private readonly Dictionary<ImageReference, (Error, T)> _cache = new Dictionary<ImageReference, (Error, T)>();

	private readonly Dictionary<ImageReference, Task<(Error, T)>> _ongoingDownloads = new Dictionary<ImageReference, Task<(Error, T)>>();

	protected BaseImageCache()
	{
		BaseImageCache.ImageCacheInstances.Add(this);
	}

	public T GetCachedImage(ImageReference uri)
	{
		_cache.TryGetValue(uri, out var value);
		return value.Item2;
	}

	public Task<(Error errror, T image)> DownloadImage(ImageReference uri)
	{
		if (_cache.TryGetValue(uri, out var value))
		{
			return Task.FromResult(value);
		}
		if (_ongoingDownloads.TryGetValue(uri, out var value2))
		{
			return value2;
		}
		Task<(Error, T)> task = DownloadImageInternal(uri);
		if (!task.IsCompleted)
		{
			_ongoingDownloads[uri] = task;
		}
		return task;
	}

	private async Task<(Error, T)> DownloadImageInternal(ImageReference uri)
	{
		var (error, stream) = await ModioClient.Api.DownloadFile(uri.Url);
		if ((bool)error || stream == null)
		{
			T item = await LoadFromDiskCache(uri);
			if (error.Code != ErrorCode.SHUTTING_DOWN)
			{
				ModioLog.Warning?.Log($"Error downloading file at {uri.Url}: {error}");
			}
			_ongoingDownloads.Remove(uri);
			return (Error.None, item);
		}
		MemoryStream memoryStream = new MemoryStream(1048576);
		await stream.CopyToAsync(memoryStream);
		byte[] array = memoryStream.ToArray();
		T item2 = Convert(array);
		_cache[uri] = (Error.None, item2);
		_ongoingDownloads.Remove(uri);
		if (BaseImageCache.PendingDiskSaves.Contains(uri))
		{
			ModioClient.DataStorage.WriteCachedImage(new Uri(uri.Url), array);
			BaseImageCache.PendingDiskSaves.Remove(uri);
		}
		return (Error.None, item2);
	}

	private async Task<T> LoadFromDiskCache(ImageReference imageReference)
	{
		Uri serverPath = new Uri(imageReference.Url);
		var (error, rawBytes) = await ModioClient.DataStorage.ReadCachedImage(serverPath);
		if ((bool)error)
		{
			return null;
		}
		T val = Convert(rawBytes);
		BaseImageCache.PendingDiskSaves.Remove(imageReference);
		_cache[imageReference] = (Error.None, val);
		return val;
	}

	public T GetFirstCachedImage(IEnumerable<ImageReference> imageReferences)
	{
		foreach (ImageReference imageReference in imageReferences)
		{
			T cachedImage = GetCachedImage(imageReference);
			if (cachedImage != null)
			{
				return cachedImage;
			}
		}
		return null;
	}

	protected override bool CacheToDiskInternal(ImageReference imageReference)
	{
		Uri serverPath = new Uri(imageReference.Url);
		T cachedImage = GetCachedImage(imageReference);
		if (cachedImage == null)
		{
			return false;
		}
		byte[] data = ConvertToBytes(cachedImage);
		ModioClient.DataStorage.WriteCachedImage(serverPath, data);
		return true;
	}

	protected abstract T Convert(byte[] rawBytes);

	protected abstract byte[] ConvertToBytes(T image);
}
