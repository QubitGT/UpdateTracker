using System;
using UnityEngine;

namespace Meta.XR.Acoustics;

[Serializable]
public class MaterialData
{
	[SerializeField]
	internal Spectrum absorption = new Spectrum();

	[SerializeField]
	internal Spectrum transmission = new Spectrum();

	[SerializeField]
	internal Spectrum scattering = new Spectrum();

	[SerializeField]
	internal Color color = Color.yellow;

	internal bool IsEmpty
	{
		get
		{
			if (absorption.points.Count == 0 && transmission.points.Count == 0)
			{
				return scattering.points.Count == 0;
			}
			return false;
		}
	}

	internal void Clone(MaterialData other)
	{
		color = other.color;
		absorption.Clone(other.absorption);
		transmission.Clone(other.transmission);
		scattering.Clone(other.scattering);
	}
}
