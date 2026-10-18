using UnityEngine;

namespace Oculus.Interaction;

public struct PointerEvent : IEvent
{
	private static ulong _nextEventId;

	public int Identifier { get; }

	public ulong EventId { get; }

	public PointerEventType Type { get; }

	public Pose Pose { get; }

	public object Data { get; }

	public PointerEvent(int identifier, PointerEventType type, Pose pose, object data = null)
	{
		Identifier = identifier;
		EventId = ++_nextEventId;
		Type = type;
		Pose = pose;
		Data = data;
	}
}
