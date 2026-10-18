using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckStorageWatcher : ILckStorageWatcher, IDisposable
{
	private readonly ILckEventBus _eventBus;

	private const long DefaultStorageThreshold = 524288000L;

	private const long SafetyBufferBytes = 52428800L;

	private const float PollIntervalInSeconds = 5f;

	private long _freeSpace = long.MaxValue;

	private bool _isRecordingActive;

	private CameraTrackDescriptor _recordingDescriptor;

	private Func<float> _getDurationSeconds;

	[Preserve]
	public LckStorageWatcher(ILckEventBus eventBus)
	{
		_eventBus = eventBus;
		LckMonoBehaviourMediator.StartCoroutine("LckStorageWatcher:Update", Update());
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

	private IEnumerator Update()
	{
		while (true)
		{
			yield return new WaitForSeconds(5f);
			CheckStorageSpace();
		}
	}

	private void CheckStorageSpace()
	{
		_freeSpace = GetAvailableStorageSpace();
		long currentStorageThreshold = GetCurrentStorageThreshold();
		if (_freeSpace < currentStorageThreshold)
		{
			_eventBus.Trigger(new LckEvents.LowStorageSpaceDetectedEvent(LckResult.NewSuccess()));
		}
	}

	private long GetCurrentStorageThreshold()
	{
		if (!_isRecordingActive)
		{
			return 524288000L;
		}
		return CalculateEstimatedRecordingSize() + 52428800;
	}

	private long CalculateEstimatedRecordingSize()
	{
		float num = _getDurationSeconds?.Invoke() ?? 0f;
		if (num <= 0f)
		{
			return 0L;
		}
		uint bitrate = _recordingDescriptor.Bitrate;
		uint audioBitrate = _recordingDescriptor.AudioBitrate;
		return (long)((float)(bitrate + audioBitrate) * num / 8f);
	}

	public void SetRecordingContext(CameraTrackDescriptor descriptor, Func<float> getDurationSeconds)
	{
		_recordingDescriptor = descriptor;
		_getDurationSeconds = getDurationSeconds;
		_isRecordingActive = true;
	}

	public void ClearRecordingContext()
	{
		_isRecordingActive = false;
	}

	private long GetAvailableStorageSpace()
	{
		return GetWindowsAvailableStorageSpace();
	}

	public long GetWindowsAvailableStorageSpace()
	{
		try
		{
			if (GetDiskFreeSpaceEx(Path.GetPathRoot(Application.temporaryCachePath), out var lpFreeBytesAvailable, out var _, out var _))
			{
				return (long)lpFreeBytesAvailable;
			}
			LckLog.LogError("Failed to get Windows storage space: " + Marshal.GetLastWin32Error(), "GetWindowsAvailableStorageSpace", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckStorageWatcher.cs", 146);
			return -1L;
		}
		catch (Exception ex)
		{
			LckLog.LogError("Failed to get Windows storage space: " + ex.Message, "GetWindowsAvailableStorageSpace", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckStorageWatcher.cs", 152);
			return -1L;
		}
	}

	public bool HasEnoughFreeStorage()
	{
		return _freeSpace > GetCurrentStorageThreshold();
	}

	public void Dispose()
	{
		LckMonoBehaviourMediator.StopCoroutineByName("LckStorageWatcher:Update");
	}
}
