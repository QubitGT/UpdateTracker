using System;
using UnityEngine;

namespace Oculus.Haptics;

public class HapticClipPlayer : IDisposable
{
	private int _clipId = -1;

	private int _playerId = -1;

	protected Haptics _haptics;

	public bool isLooping
	{
		get
		{
			return _haptics.IsHapticPlayerLooping(_playerId);
		}
		set
		{
			_haptics.LoopHapticPlayer(_playerId, value);
		}
	}

	public float clipDuration => _haptics.GetClipDuration(_clipId);

	public float amplitude
	{
		get
		{
			return _haptics.GetAmplitudeHapticPlayer(_playerId);
		}
		set
		{
			_haptics.SetAmplitudeHapticPlayer(_playerId, value);
		}
	}

	public float frequencyShift
	{
		get
		{
			return _haptics.GetFrequencyShiftHapticPlayer(_playerId);
		}
		set
		{
			_haptics.SetFrequencyShiftHapticPlayer(_playerId, value);
		}
	}

	public uint priority
	{
		get
		{
			return _haptics.GetPriorityHapticPlayer(_playerId);
		}
		set
		{
			_haptics.SetPriorityHapticPlayer(_playerId, value);
		}
	}

	public HapticClip clip
	{
		set
		{
			int num = _haptics.LoadClip(value.json);
			if (-1 != num)
			{
				_haptics.SetHapticPlayerClip(_playerId, num);
				if (_clipId != -1)
				{
					_haptics.ReleaseClip(_clipId);
				}
				_clipId = num;
			}
		}
	}

	public HapticClipPlayer()
	{
		SetHaptics();
		int num = _haptics.CreateHapticPlayer();
		if (-1 != num)
		{
			_playerId = num;
		}
	}

	public HapticClipPlayer(HapticClip clip)
	{
		SetHaptics();
		int num = _haptics.CreateHapticPlayer();
		if (-1 != num)
		{
			_playerId = num;
			this.clip = clip;
		}
	}

	protected virtual void SetHaptics()
	{
		_haptics = Haptics.Instance;
	}

	public void Play(Controller controller)
	{
		_haptics.PlayHapticPlayer(_playerId, controller);
	}

	public void Pause()
	{
		_haptics.PauseHapticPlayer(_playerId);
	}

	public void Resume()
	{
		_haptics.ResumeHapticPlayer(_playerId);
	}

	public void Stop()
	{
		_haptics.StopHapticPlayer(_playerId);
	}

	public void Seek(float time)
	{
		_haptics.SeekPlaybackPositionHapticPlayer(_playerId, time);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_playerId != -1)
		{
			if (!_haptics.ReleaseClip(_clipId) & _haptics.ReleaseHapticPlayer(_playerId))
			{
				Debug.LogError("Error: HapticClipPlayer or HapticClip could not be released");
			}
			_clipId = -1;
			_playerId = -1;
		}
	}

	~HapticClipPlayer()
	{
		Dispose(disposing: false);
	}
}
