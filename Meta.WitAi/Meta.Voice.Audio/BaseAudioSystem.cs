using System;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.WitAi;
using UnityEngine;

namespace Meta.Voice.Audio;

[LogCategory(LogCategory.Audio)]
public abstract class BaseAudioSystem<TAudioClipStream, TAudioPlayer> : MonoBehaviour, IAudioSystem, ILogSource where TAudioClipStream : IAudioClipStream where TAudioPlayer : MonoBehaviour, IAudioPlayer
{
	private AudioClipSettings _clipSettings = new AudioClipSettings
	{
		Channels = 1,
		SampleRate = 24000,
		ReadyDuration = 1.5f,
		MaxDuration = 15f
	};

	private ObjectPool<TAudioClipStream> _pool;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Audio);


	public AudioClipSettings ClipSettings
	{
		get
		{
			return _clipSettings;
		}
		set
		{
			if (!_clipSettings.Equals(value))
			{
				_clipSettings = value;
				if (_pool != null)
				{
					Logger.Warning("Due to a settings change, the pool is being cleared.");
					_pool.Dispose();
					_pool = null;
				}
			}
		}
	}

	protected virtual void GeneratePool()
	{
		if (_pool == null)
		{
			_pool = new ObjectPool<TAudioClipStream>(GenerateClip);
		}
	}

	protected virtual TAudioClipStream GenerateClip()
	{
		if (typeof(TAudioClipStream) == typeof(RawAudioClipStream))
		{
			return (TAudioClipStream)(object)new RawAudioClipStream(ClipSettings.Channels, ClipSettings.SampleRate, ClipSettings.ReadyDuration, ClipSettings.MaxDuration);
		}
		Logger.Warning("{0}.GenerateClip() is missing clip instantiation for {1}", GetType().Name, typeof(TAudioClipStream).Name);
		return default(TAudioClipStream);
	}

	protected virtual void OnDestroy()
	{
		_pool.Dispose();
		_pool = null;
	}

	public virtual void PreloadClipStreams(int total)
	{
		GeneratePool();
		_pool.Preload(total);
	}

	public virtual IAudioClipStream GetAudioClipStream()
	{
		GeneratePool();
		TAudioClipStream val = _pool.Get();
		val.OnStreamUnloaded = (AudioClipStreamDelegate)Delegate.Combine(val.OnStreamUnloaded, new AudioClipStreamDelegate(UnloadAudioClipStream));
		return val;
	}

	protected virtual void UnloadAudioClipStream(IAudioClipStream clipStream)
	{
		if (clipStream is TAudioClipStream item)
		{
			item.OnStreamUnloaded = (AudioClipStreamDelegate)Delegate.Remove(item.OnStreamUnloaded, new AudioClipStreamDelegate(UnloadAudioClipStream));
			_pool.Return(item);
		}
	}

	public virtual IAudioPlayer GetAudioPlayer(GameObject root)
	{
		return root.AddComponent<TAudioPlayer>();
	}
}
