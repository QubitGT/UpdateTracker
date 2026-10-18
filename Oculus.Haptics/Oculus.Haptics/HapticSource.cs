using System.ComponentModel;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Haptics;

[Feature(Feature.Haptics)]
public class HapticSource : MonoBehaviour, ISerializationCallbackReceiver
{
	private HapticClipPlayer _player;

	[SerializeField]
	private HapticClip _clip;

	[SerializeField]
	private Controller _controller = Controller.Both;

	[SerializeField]
	private bool _loop;

	[SerializeField]
	[Range(0f, float.MaxValue)]
	private float _amplitude = 1f;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _frequencyShift;

	[SerializeField]
	[Range(0f, 255f)]
	private uint _priority = 128u;

	public HapticClip clip
	{
		set
		{
			_clip = value;
			if (_player != null)
			{
				_player.clip = _clip;
			}
		}
	}

	public float clipDuration => _player.clipDuration;

	public Controller controller
	{
		set
		{
			_controller = value;
		}
	}

	[DefaultValue(false)]
	public bool loop
	{
		get
		{
			return _loop;
		}
		set
		{
			_loop = value;
			_player.isLooping = _loop;
		}
	}

	[DefaultValue(1.0)]
	public float amplitude
	{
		get
		{
			return _amplitude;
		}
		set
		{
			_amplitude = value;
			_player.amplitude = _amplitude;
		}
	}

	[DefaultValue(0.0)]
	public float frequencyShift
	{
		get
		{
			return _frequencyShift;
		}
		set
		{
			_frequencyShift = value;
			_player.frequencyShift = _frequencyShift;
		}
	}

	[DefaultValue(128)]
	public uint priority
	{
		get
		{
			return _priority;
		}
		set
		{
			_priority = value;
			_player.priority = _priority;
		}
	}

	private void Awake()
	{
		_player = new HapticClipPlayer();
		_player.clip = _clip;
		SyncSerializedFieldsToPlayer();
	}

	public void Play()
	{
		_player.Play(_controller);
	}

	public void Play(Controller controller)
	{
		this.controller = controller;
		_player.Play(_controller);
	}

	public void Pause()
	{
		_player.Pause();
	}

	public void Resume()
	{
		_player.Resume();
	}

	public void Stop()
	{
		_player.Stop();
	}

	public void Seek(float time)
	{
		_player.Seek(time);
	}

	private void SyncSerializedFieldsToPlayer()
	{
		if (_player != null)
		{
			_player.isLooping = _loop;
			_player.amplitude = _amplitude;
			_player.frequencyShift = _frequencyShift;
			_player.priority = _priority;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (_player != null)
		{
			SyncSerializedFieldsToPlayer();
		}
	}

	protected virtual void OnDestroy()
	{
		_player.Dispose();
	}
}
