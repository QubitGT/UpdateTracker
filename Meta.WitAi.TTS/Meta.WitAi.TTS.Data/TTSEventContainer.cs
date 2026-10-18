using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Json;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSEventContainer
{
	private ConcurrentQueue<ITTSEvent> _events = new ConcurrentQueue<ITTSEvent>();

	internal const string EVENT_TYPE_KEY = "type";

	internal const string EVENT_WORD_TYPE_KEY = "WORD";

	internal const string EVENT_VISEME_TYPE_KEY = "VISEME";

	internal const string EVENT_PHONEME_TYPE_KEY = "PHONE";

	internal const string EVENT_EMOTE_TYPE_KEY = "EMOTE";

	internal const string EVENT_ACTION_TYPE_KEY = "ACTION";

	public IEnumerable<ITTSEvent> Events => _events;

	public IEnumerable<TTSWordEvent> WordEvents => GetEvents<TTSWordEvent>();

	public IEnumerable<TTSVisemeEvent> VisemeEvents => GetEvents<TTSVisemeEvent>();

	public event Action<WitResponseNode> OnEventJsonAdded;

	public event Action<ITTSEvent> OnEventAdded;

	public IEnumerable<TEvent> GetEvents<TEvent>(string eventTypeKey = null) where TEvent : ITTSEvent
	{
		return from e in _events
			where e is TEvent && (string.IsNullOrEmpty(eventTypeKey) || eventTypeKey.Equals(e.EventType))
			select (TEvent)e;
	}

	public void AddEvents(IEnumerable<WitResponseNode> events)
	{
		if (events == null)
		{
			return;
		}
		foreach (WitResponseNode @event in events)
		{
			AddEvent(@event);
		}
	}

	public bool AddEvent(WitResponseNode eventNode)
	{
		ITTSEvent iTTSEvent = DecodeEvent(eventNode);
		if (iTTSEvent == null)
		{
			return false;
		}
		_events.Enqueue(iTTSEvent);
		this.OnEventJsonAdded?.Invoke(eventNode);
		this.OnEventAdded?.Invoke(iTTSEvent);
		return true;
	}

	private ITTSEvent DecodeEvent(WitResponseNode eventNode)
	{
		try
		{
			return eventNode["type"].Value switch
			{
				"WORD" => JsonConvert.DeserializeObject<TTSWordEvent>(eventNode), 
				"VISEME" => JsonConvert.DeserializeObject<TTSVisemeEvent>(eventNode), 
				"PHONE" => JsonConvert.DeserializeObject<TTSPhonemeEvent>(eventNode), 
				"EMOTE" => JsonConvert.DeserializeObject<TTSEmoteEvent>(eventNode), 
				"ACTION" => JsonConvert.DeserializeObject<TTSActionEvent>(eventNode), 
				_ => JsonConvert.DeserializeObject<TTSStringEvent>(eventNode), 
			};
		}
		catch (Exception)
		{
			return null;
		}
	}

	public void GetClosestEvents<TEvent>(int sample, ref int previousEventIndex, ref TEvent previousEvent, ref TEvent nextEvent) where TEvent : ITTSEvent
	{
		if (previousEvent == null || sample < previousEvent.SampleOffset)
		{
			previousEventIndex = 0;
		}
		nextEvent = default(TEvent);
		int num = 0;
		foreach (ITTSEvent @event in _events)
		{
			if (num >= previousEventIndex && @event is TEvent val)
			{
				if (sample < val.SampleOffset)
				{
					nextEvent = val;
					break;
				}
				previousEventIndex = num;
				previousEvent = val;
			}
			num++;
		}
	}
}
