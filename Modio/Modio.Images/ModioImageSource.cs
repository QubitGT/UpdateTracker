using System;
using System.Collections.Generic;

namespace Modio.Images;

public class ModioImageSource<TResolution> where TResolution : Enum
{
	private readonly ImageReference[] _resolutions;

	private bool _isCachingLowestResolution;

	public string FileName { get; private set; }

	internal ModioImageSource(string fileName, params string[] links)
	{
		FileName = fileName;
		_resolutions = new ImageReference[links.Length];
		for (int i = 0; i < _resolutions.Length; i++)
		{
			_resolutions[i] = new ImageReference(links[i]);
		}
	}

	public ImageReference GetUri(TResolution resolution)
	{
		int val = (int)(object)resolution;
		val = Math.Min(_resolutions.Length - 1, val);
		return _resolutions[val];
	}

	public IEnumerable<ImageReference> GetAllReferences()
	{
		return _resolutions;
	}

	public void CacheLowestResolutionOnDisk(bool shouldCache)
	{
		if (_isCachingLowestResolution != shouldCache)
		{
			_isCachingLowestResolution = shouldCache;
			if (_resolutions.Length != 0)
			{
				BaseImageCache.CacheToDisk(_resolutions[0], shouldCache);
			}
		}
	}
}
