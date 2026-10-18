using System;
using Modio.Mods;

namespace Modio.FileIO;

public class ModInstallProgressTracker
{
	private readonly Mod _mod;

	private readonly long _totalSize;

	private Func<long> _currentBytesGetter;

	private DateTime _lastCalculatedAt;

	private long _bytesPerSecond;

	private long _lastCalculatedSpeedAtBytes;

	public Func<long> CurrentBytesGetter
	{
		get
		{
			return _currentBytesGetter;
		}
		set
		{
			_currentBytesGetter = value;
		}
	}

	public ModInstallProgressTracker(Mod mod, long totalSize, Func<long> currentBytesGetter = null)
	{
		_totalSize = totalSize;
		_currentBytesGetter = currentBytesGetter;
		_mod = mod;
	}

	public void Update()
	{
		if (_currentBytesGetter != null)
		{
			SetBytesRead(_currentBytesGetter());
		}
	}

	public void SetBytesRead(long currentBytes)
	{
		DateTime now = DateTime.Now;
		float num = (float)(now - _lastCalculatedAt).TotalMilliseconds / 1000f;
		if (num > 1f || _lastCalculatedSpeedAtBytes == 0L)
		{
			_bytesPerSecond = (long)((float)(currentBytes - _lastCalculatedSpeedAtBytes) / num);
			if (num > 1f)
			{
				_lastCalculatedAt = now;
				_lastCalculatedSpeedAtBytes = currentBytes;
			}
		}
		float fileStateProgress = 0.99f * (float)currentBytes / (float)_totalSize;
		_mod.File.FileStateProgress = fileStateProgress;
		_mod.File.DownloadingBytesPerSecond = _bytesPerSecond;
		_mod.InvokeModUpdated(ModChangeType.DownloadProgress);
	}
}
