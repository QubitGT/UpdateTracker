using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX;

internal class VisualEffectControlTrackController
{
	private struct Event
	{
		public enum ClipType
		{
			None,
			Enter,
			Exit
		}

		public int nameId;

		public VFXEventAttribute attribute;

		public double time;

		public int clipIndex;

		public ClipType clipType;
	}

	private struct Clip
	{
		public int enter;

		public int exit;
	}

	private struct Chunk
	{
		public bool scrubbing;

		public bool reinitEnter;

		public bool reinitExit;

		public uint startSeed;

		public double begin;

		public double end;

		public uint prewarmCount;

		public float prewarmDeltaTime;

		public double prewarmOffset;

		public int prewarmEvent;

		public Event[] events;

		public Clip[] clips;
	}

	private class VisualEffectControlPlayableBehaviourComparer : IComparer<VisualEffectControlPlayableBehaviour>
	{
		public int Compare(VisualEffectControlPlayableBehaviour x, VisualEffectControlPlayableBehaviour y)
		{
			return x.clipStart.CompareTo(y.clipStart);
		}
	}

	private const int kErrorIndex = int.MinValue;

	private int m_LastChunk = int.MinValue;

	private int m_LastEvent = int.MinValue;

	private double m_LastPlayableTime = double.MinValue;

	private List<int> m_EventListIndexCache = new List<int>();

	private VisualEffect m_Target;

	private bool m_BackupReseedOnPlay;

	private uint m_BackupStartSeed;

	private Chunk[] m_Chunks;

	private static readonly double kEpsilonEvent = 1E-12;

	private void OnEnterChunk(int currentChunk)
	{
		Chunk chunk = m_Chunks[currentChunk];
		if (chunk.reinitEnter)
		{
			m_Target.resetSeedOnPlay = false;
			m_Target.startSeed = chunk.startSeed;
			m_Target.Reinit(sendInitialEventAndPrewarm: false);
			if (chunk.prewarmCount != 0)
			{
				m_Target.SendEvent(chunk.prewarmEvent);
				m_Target.Simulate(chunk.prewarmDeltaTime, chunk.prewarmCount);
			}
		}
	}

	private void OnLeaveChunk(int previousChunkIndex, bool leavingGoingBeforeClip)
	{
		Chunk chunk = m_Chunks[previousChunkIndex];
		if (chunk.reinitExit)
		{
			m_Target.Reinit(sendInitialEventAndPrewarm: false);
		}
		else
		{
			ProcessNoScrubbingEvents(chunk, m_LastPlayableTime, leavingGoingBeforeClip ? double.NegativeInfinity : double.PositiveInfinity);
		}
		RestoreVFXState(chunk.scrubbing, chunk.reinitEnter);
	}

	private bool IsTimeInChunk(double time, int index)
	{
		Chunk chunk = m_Chunks[index];
		if (chunk.begin <= time)
		{
			return time < chunk.end;
		}
		return false;
	}

	public void Update(double playableTime, float deltaTime)
	{
		double num = playableTime + kEpsilonEvent;
		bool flag = (double)deltaTime == 0.0;
		int num2 = int.MinValue;
		if (m_LastChunk != num2 && IsTimeInChunk(playableTime, m_LastChunk))
		{
			num2 = m_LastChunk;
		}
		if (num2 == int.MinValue)
		{
			uint num3 = ((m_LastChunk != int.MinValue) ? ((uint)m_LastEvent) : 0u);
			for (uint num4 = num3; num4 < num3 + m_Chunks.Length; num4++)
			{
				int num5 = (int)(num4 % m_Chunks.Length);
				if (IsTimeInChunk(playableTime, num5))
				{
					num2 = num5;
					break;
				}
			}
		}
		bool flag2 = false;
		if (m_LastChunk != num2)
		{
			if (m_LastChunk != int.MinValue)
			{
				bool leavingGoingBeforeClip = playableTime < m_Chunks[m_LastChunk].begin;
				OnLeaveChunk(m_LastChunk, leavingGoingBeforeClip);
			}
			if (num2 != int.MinValue)
			{
				OnEnterChunk(num2);
				flag2 = true;
			}
			m_LastChunk = num2;
			m_LastEvent = int.MinValue;
		}
		if (num2 != int.MinValue)
		{
			Chunk chunk = m_Chunks[num2];
			if (chunk.scrubbing)
			{
				m_Target.pause = flag;
				double num6 = chunk.begin + (double)m_Target.time;
				if (!flag2)
				{
					num6 -= chunk.prewarmOffset;
				}
				if (!(playableTime < m_LastPlayableTime))
				{
					if (Math.Abs(m_LastPlayableTime - num6) < (double)VFXManager.maxDeltaTime)
					{
						num6 = m_LastPlayableTime;
					}
				}
				else
				{
					num6 = chunk.begin;
					m_LastEvent = int.MinValue;
					OnEnterChunk(m_LastChunk);
				}
				double num7 = ((!flag) ? (playableTime - (double)VFXManager.fixedTimeStep) : playableTime);
				if (m_LastPlayableTime < num6)
				{
					List<int> eventListIndexCache = m_EventListIndexCache;
					GetEventsIndex(chunk, m_LastPlayableTime, num6, m_LastEvent, eventListIndexCache);
					foreach (int item in eventListIndexCache)
					{
						ProcessEvent(item, chunk);
					}
				}
				if (num6 < num7)
				{
					List<int> eventListIndexCache2 = m_EventListIndexCache;
					GetEventsIndex(chunk, num6, num7, m_LastEvent, eventListIndexCache2);
					int count = eventListIndexCache2.Count;
					int num8 = 0;
					float maxScrubTime = VFXManager.maxScrubTime;
					float num9 = VFXManager.maxDeltaTime;
					if (num7 - num6 > (double)maxScrubTime)
					{
						num9 = (float)((num7 - num6) * (double)VFXManager.maxDeltaTime / (double)maxScrubTime);
					}
					while (num6 < num7)
					{
						int num10 = int.MinValue;
						uint num11;
						if (num8 < count)
						{
							num10 = eventListIndexCache2[num8++];
							num11 = (uint)((chunk.events[num10].time - num6) / (double)num9);
						}
						else
						{
							num11 = (uint)((num7 - num6) / (double)num9);
							if (num11 == 0)
							{
								break;
							}
						}
						if (num11 != 0)
						{
							m_Target.Simulate(num9, num11);
							num6 += (double)(num9 * (float)num11);
						}
						ProcessEvent(num10, chunk);
					}
				}
				if (num6 < num)
				{
					List<int> eventListIndexCache3 = m_EventListIndexCache;
					GetEventsIndex(chunk, num6, num, m_LastEvent, eventListIndexCache3);
					foreach (int item2 in eventListIndexCache3)
					{
						ProcessEvent(item2, chunk);
					}
				}
			}
			else
			{
				m_Target.pause = false;
				ProcessNoScrubbingEvents(chunk, m_LastPlayableTime, num);
			}
		}
		m_LastPlayableTime = playableTime;
	}

	private void ProcessNoScrubbingEvents(Chunk chunk, double oldTime, double newTime)
	{
		if (newTime < oldTime)
		{
			List<int> eventListIndexCache = m_EventListIndexCache;
			GetEventsIndex(chunk, newTime, oldTime, int.MinValue, eventListIndexCache);
			if (eventListIndexCache.Count <= 0)
			{
				return;
			}
			for (int num = eventListIndexCache.Count - 1; num >= 0; num--)
			{
				int num2 = eventListIndexCache[num];
				Event @event = chunk.events[num2];
				if (@event.clipType == Event.ClipType.Enter)
				{
					ProcessEvent(chunk.clips[@event.clipIndex].exit, chunk);
				}
				else if (@event.clipType == Event.ClipType.Exit)
				{
					ProcessEvent(chunk.clips[@event.clipIndex].enter, chunk);
				}
			}
			m_LastEvent = int.MinValue;
			return;
		}
		List<int> eventListIndexCache2 = m_EventListIndexCache;
		GetEventsIndex(chunk, oldTime, newTime, m_LastEvent, eventListIndexCache2);
		foreach (int item in eventListIndexCache2)
		{
			ProcessEvent(item, chunk);
		}
	}

	private void ProcessEvent(int eventIndex, Chunk currentChunk)
	{
		if (eventIndex != int.MinValue)
		{
			m_LastEvent = eventIndex;
			Event @event = currentChunk.events[eventIndex];
			m_Target.SendEvent(@event.nameId, @event.attribute);
		}
	}

	private static void GetEventsIndex(Chunk chunk, double minTime, double maxTime, int lastIndex, List<int> eventListIndex)
	{
		eventListIndex.Clear();
		for (int i = ((lastIndex != int.MinValue) ? (lastIndex + 1) : 0); i < chunk.events.Length; i++)
		{
			Event @event = chunk.events[i];
			if (!(@event.time >= maxTime))
			{
				if (minTime <= @event.time)
				{
					eventListIndex.Add(i);
				}
				continue;
			}
			break;
		}
	}

	private static VFXEventAttribute ComputeAttribute(VisualEffect vfx, EventAttributes attributes)
	{
		if (attributes.content == null)
		{
			return null;
		}
		VFXEventAttribute vFXEventAttribute = vfx.CreateVFXEventAttribute();
		bool flag = false;
		EventAttribute[] content = attributes.content;
		foreach (EventAttribute obj in content)
		{
			if (obj != null && obj.ApplyToVFX(vFXEventAttribute))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return null;
		}
		return vFXEventAttribute;
	}

	private static IEnumerable<Event> ComputeRuntimeEvent(VisualEffectControlPlayableBehaviour behavior, VisualEffect vfx)
	{
		IEnumerable<VisualEffectPlayableSerializedEvent> eventNormalizedSpace = VFXTimeSpaceHelper.GetEventNormalizedSpace(PlayableTimeSpace.Absolute, behavior);
		foreach (VisualEffectPlayableSerializedEvent item in eventNormalizedSpace)
		{
			double time = Math.Max(behavior.clipStart, Math.Min(behavior.clipEnd, item.time));
			yield return new Event
			{
				attribute = ComputeAttribute(vfx, item.eventAttributes),
				nameId = item.name,
				time = time,
				clipIndex = -1,
				clipType = Event.ClipType.None
			};
		}
	}

	public void RestoreVFXState(bool restorePause = true, bool restoreSeedState = true)
	{
		if (!(m_Target == null))
		{
			if (restorePause)
			{
				m_Target.pause = false;
			}
			if (restoreSeedState)
			{
				m_Target.startSeed = m_BackupStartSeed;
				m_Target.resetSeedOnPlay = m_BackupReseedOnPlay;
			}
		}
	}

	public void Init(Playable playable, VisualEffect vfx, VisualEffectControlTrack parentTrack)
	{
		m_Target = vfx;
		m_BackupStartSeed = m_Target.startSeed;
		m_BackupReseedOnPlay = m_Target.resetSeedOnPlay;
		Stack<(Chunk, List<Event>, List<Clip>)> stack = new Stack<(Chunk, List<Event>, List<Clip>)>();
		int inputCount = playable.GetInputCount();
		List<VisualEffectControlPlayableBehaviour> list = new List<VisualEffectControlPlayableBehaviour>();
		for (int i = 0; i < inputCount; i++)
		{
			Playable input = playable.GetInput(i);
			if (!(input.GetPlayableType() != typeof(VisualEffectControlPlayableBehaviour)))
			{
				VisualEffectControlPlayableBehaviour behaviour = ((ScriptPlayable<VisualEffectControlPlayableBehaviour>)input).GetBehaviour();
				if (behaviour != null)
				{
					list.Add(behaviour);
				}
			}
		}
		list.Sort(new VisualEffectControlPlayableBehaviourComparer());
		foreach (VisualEffectControlPlayableBehaviour item6 in list)
		{
			if (stack.Count == 0 || item6.clipStart > stack.Peek().Item1.end || item6.scrubbing != stack.Peek().Item1.scrubbing || (!item6.scrubbing && (item6.reinitEnter || stack.Peek().Item1.reinitExit)) || item6.startSeed != stack.Peek().Item1.startSeed || item6.prewarmStepCount != 0)
			{
				(Chunk, List<Event>, List<Clip>) item = default((Chunk, List<Event>, List<Clip>));
				Chunk item2 = new Chunk
				{
					begin = item6.clipStart,
					scrubbing = item6.scrubbing,
					startSeed = item6.startSeed,
					reinitEnter = item6.reinitEnter,
					reinitExit = item6.reinitExit,
					prewarmCount = item6.prewarmStepCount,
					prewarmDeltaTime = item6.prewarmDeltaTime
				};
				ExposedProperty prewarmEvent = item6.prewarmEvent;
				item2.prewarmEvent = ((prewarmEvent != null) ? ((int)prewarmEvent) : 0);
				item2.prewarmOffset = (double)item6.prewarmStepCount * (double)item6.prewarmDeltaTime;
				item.Item1 = item2;
				item.Item2 = new List<Event>();
				item.Item3 = new List<Clip>();
				stack.Push(item);
			}
			(Chunk, List<Event>, List<Clip>) item3 = stack.Pop();
			item3.Item1.end = item6.clipEnd;
			List<Event> list2 = new List<Event>(ComputeRuntimeEvent(item6, vfx));
			if (!item3.Item1.scrubbing)
			{
				List<(Event, int)> list3 = new List<(Event, int)>();
				for (int j = 0; j < list2.Count; j++)
				{
					list3.Add((list2[j], j));
				}
				list3.Sort(((Event evt, int sourceIndex) x, (Event evt, int sourceIndex) y) => x.evt.time.CompareTo(y.evt.time));
				Clip[] array = new Clip[item6.clipEventsCount];
				List<Event> list4 = new List<Event>();
				for (int k = 0; k < list3.Count; k++)
				{
					Event item4 = list3[k].Item1;
					int item5 = list3[k].Item2;
					if (item5 < item6.clipEventsCount * 2)
					{
						int num = item3.Item2.Count + k;
						int num2 = item5 / 2;
						item4.clipIndex = num2 + item3.Item3.Count;
						if (item5 % 2 == 0)
						{
							item4.clipType = Event.ClipType.Enter;
							array[num2].enter = num;
						}
						else
						{
							item4.clipType = Event.ClipType.Exit;
							array[num2].exit = num;
						}
						list4.Add(item4);
					}
					else
					{
						list4.Add(item4);
					}
				}
				item3.Item3.AddRange(array);
				item3.Item2.AddRange(list4);
			}
			else
			{
				list2.Sort((Event x, Event y) => x.time.CompareTo(y.time));
				item3.Item2.AddRange(list2);
			}
			stack.Push(item3);
		}
		m_Chunks = new Chunk[stack.Count];
		for (int l = 0; l < m_Chunks.Length; l++)
		{
			(Chunk, List<Event>, List<Clip>) tuple = stack.Pop();
			m_Chunks[l] = tuple.Item1;
			m_Chunks[l].clips = tuple.Item3.ToArray();
			m_Chunks[l].events = tuple.Item2.ToArray();
		}
	}

	public void Release()
	{
		RestoreVFXState();
	}
}
