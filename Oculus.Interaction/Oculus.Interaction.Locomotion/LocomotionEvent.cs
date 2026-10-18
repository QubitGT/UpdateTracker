using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public struct LocomotionEvent
{
	public enum TranslationType
	{
		None,
		Velocity,
		Absolute,
		AbsoluteEyeLevel,
		Relative
	}

	public enum RotationType
	{
		None,
		Velocity,
		Absolute,
		Relative
	}

	private static ulong _nextEventId;

	public int Identifier { get; }

	public Pose Pose { get; }

	public TranslationType Translation { get; }

	public RotationType Rotation { get; }

	public ulong EventId { get; }

	public LocomotionEvent(int identifier, Pose pose, TranslationType translationType, RotationType rotationType)
	{
		Identifier = identifier;
		EventId = ++_nextEventId;
		Pose = pose;
		Translation = translationType;
		Rotation = rotationType;
	}

	public LocomotionEvent(int identifier, Vector3 position, TranslationType translationType)
		: this(identifier, new Pose(position, Quaternion.identity), translationType, RotationType.None)
	{
	}

	public LocomotionEvent(int identifier, Quaternion rotation, RotationType rotationType)
		: this(identifier, new Pose(Vector3.zero, rotation), TranslationType.None, rotationType)
	{
	}
}
