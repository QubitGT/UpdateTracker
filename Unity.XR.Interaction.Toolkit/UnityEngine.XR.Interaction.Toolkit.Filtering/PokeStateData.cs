using System;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public struct PokeStateData : IEquatable<PokeStateData>
{
	public bool meetsRequirements { get; set; }

	public Vector3 pokeInteractionPoint { get; set; }

	public Vector3 axisAlignedPokeInteractionPoint { get; set; }

	public float interactionStrength { get; set; }

	public Vector3 axisNormal { get; set; }

	public Transform target { get; set; }

	public bool Equals(PokeStateData other)
	{
		if (meetsRequirements == other.meetsRequirements && pokeInteractionPoint.Equals(other.pokeInteractionPoint) && axisAlignedPokeInteractionPoint.Equals(other.axisAlignedPokeInteractionPoint) && interactionStrength.Equals(other.interactionStrength) && axisNormal.Equals(other.axisNormal))
		{
			return target == other.target;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PokeStateData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((17 * 31 + meetsRequirements.GetHashCode()) * 31 + pokeInteractionPoint.GetHashCode()) * 31 + axisAlignedPokeInteractionPoint.GetHashCode()) * 31 + interactionStrength.GetHashCode()) * 31 + axisNormal.GetHashCode()) * 31 + target.GetHashCode();
	}
}
