using System;
using System.Collections.Generic;
using Meta.Voice.Logging;
using Meta.WitAi.Attributes;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

public abstract class TTSEventTrigger<TEvent, TData> : MonoBehaviour where TEvent : TTSEvent<TData>
{
	private int _sample = -1;

	private Queue<ITTSEvent> queuedEvents = new Queue<ITTSEvent>();

	[SerializeField]
	[ObjectType(typeof(ITTSEventPlayer), new Type[] { })]
	private UnityEngine.Object _player;

	private TTSEventContainer _currentEvents;

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

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
		ClearCurrentEvents();
	}

	private void ClearCurrentEvents()
	{
		if (_currentEvents != null)
		{
			_currentEvents.OnEventAdded -= OnEventAdded;
			_currentEvents = null;
		}
	}

	private void OnEventAdded(ITTSEvent ev)
	{
		if (ev is TEvent)
		{
			queuedEvents.Enqueue(ev);
		}
	}

	protected virtual void Update()
	{
		if (Player == null || Player.CurrentEvents == null)
		{
			return;
		}
		if (_currentEvents != Player.CurrentEvents)
		{
			ClearCurrentEvents();
			_currentEvents = Player.CurrentEvents;
			if (_currentEvents != null)
			{
				_currentEvents.OnEventAdded += OnEventAdded;
				foreach (ITTSEvent @event in Player.CurrentEvents.Events)
				{
					if (@event is TEvent)
					{
						queuedEvents.Enqueue(@event);
					}
				}
			}
		}
		RefreshSample(force: false);
	}

	protected virtual void RefreshSample(bool force)
	{
		int num = ((Player != null) ? Player.ElapsedSamples : 0);
		if (force || num != _sample)
		{
			_sample = num;
			while (queuedEvents.Count > 0 && _sample > queuedEvents.Peek().SampleOffset)
			{
				OnEventTriggered((TEvent)queuedEvents.Dequeue());
			}
		}
	}

	protected abstract void OnEventTriggered(TEvent queuedEvent);
}
