using System;
using Meta.Voice.Logging;
using Meta.WitAi.Attributes;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

public abstract class TTSEventAnimator<TEvent, TData> : MonoBehaviour where TEvent : TTSEvent<TData>
{
	[SerializeField]
	[ObjectType(typeof(ITTSEventPlayer), new Type[] { })]
	private UnityEngine.Object _player;

	public bool easeIgnored;

	public AnimationCurve easeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool sendMinEvent = true;

	public bool sendMaxEvent = true;

	private int _sample = -1;

	private int _prevEventIndex;

	private TEvent _prevEvent;

	private TEvent _nextEvent;

	private TEvent _minEvent;

	private TEvent _maxEvent;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.TextToSpeech);


	public ITTSEventPlayer Player
	{
		get
		{
			return _player as ITTSEventPlayer;
		}
		set
		{
			if (value is UnityEngine.Object player)
			{
				_player = player;
				return;
			}
			if (value != null)
			{
				Logger.Error("Invalid ITTSEventPlayer type: {0}", value.GetType().Name);
			}
			_player = null;
		}
	}

	public TTSEventContainer EventContainer { get; private set; }

	protected virtual void Awake()
	{
		_minEvent = Activator.CreateInstance<TEvent>();
		_maxEvent = Activator.CreateInstance<TEvent>();
	}

	protected virtual void OnEnable()
	{
		if (Player == null)
		{
			_player = base.gameObject.GetComponentInChildren(typeof(ITTSEventPlayer));
			if (Player == null)
			{
				VLog.E("No ITTSEventPlayer found for " + GetType().Name + " on " + base.name);
			}
		}
		RefreshSample(force: true);
	}

	protected virtual void Update()
	{
		RefreshSample(force: false);
	}

	protected virtual void RefreshSample(bool force)
	{
		int num = ((Player != null) ? Player.ElapsedSamples : 0);
		if (!force && num == _sample)
		{
			return;
		}
		_sample = num;
		TTSEventContainer tTSEventContainer = Player?.CurrentEvents;
		if (num < 0 || tTSEventContainer?.Events == null)
		{
			if (sendMinEvent)
			{
				LerpEvent(_minEvent, _minEvent, 0f);
			}
			return;
		}
		tTSEventContainer.GetClosestEvents(_sample, ref _prevEventIndex, ref _prevEvent, ref _nextEvent);
		TEvent val = _prevEvent ?? _minEvent;
		TEvent val2 = _nextEvent ?? _maxEvent;
		if (Player != null)
		{
			_maxEvent.offset = Player.TotalSamples;
		}
		float sampleEventProgress = GetSampleEventProgress(num, val.SampleOffset, val2.SampleOffset);
		if ((val2 != _minEvent || sendMinEvent) && (val2 != _maxEvent || sendMaxEvent))
		{
			LerpEvent(val, val2, sampleEventProgress);
		}
	}

	private float GetSampleEventProgress(int sample, int previousEventSample, int nextEventSample)
	{
		float num = 0f;
		if (previousEventSample != nextEventSample)
		{
			num = Mathf.Clamp01((float)(sample - previousEventSample) / (float)(nextEventSample - previousEventSample));
		}
		if (easeIgnored)
		{
			return (num >= 1f) ? 1f : 0f;
		}
		return easeCurve.Evaluate(num);
	}

	protected abstract void LerpEvent(TEvent fromEvent, TEvent toEvent, float percentage);
}
