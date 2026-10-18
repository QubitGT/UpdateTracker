using System;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public readonly struct AffordanceStateData : IEquatable<AffordanceStateData>
{
	public const byte totalStateTransitionIncrements = byte.MaxValue;

	public byte stateIndex { get; }

	public byte stateTransitionIncrement { get; }

	public float stateTransitionAmountFloat => (float)(int)stateTransitionIncrement / 255f;

	public AffordanceStateData(byte stateIndex, float transitionAmount)
		: this(stateIndex, (byte)(Mathf.Clamp01(transitionAmount) * 255f))
	{
	}

	public AffordanceStateData(byte stateIndex, byte transitionIncrement)
	{
		this.stateIndex = stateIndex;
		stateTransitionIncrement = transitionIncrement;
	}

	public bool Equals(AffordanceStateData other)
	{
		if (stateIndex == other.stateIndex)
		{
			return stateTransitionIncrement == other.stateTransitionIncrement;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AffordanceStateData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 31 + stateIndex.GetHashCode()) * 31 + stateTransitionIncrement.GetHashCode();
	}
}
