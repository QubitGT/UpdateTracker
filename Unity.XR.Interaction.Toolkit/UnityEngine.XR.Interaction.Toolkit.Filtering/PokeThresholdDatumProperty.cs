using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class PokeThresholdDatumProperty : DatumProperty<PokeThresholdData, PokeThresholdDatum>
{
	public PokeThresholdDatumProperty(PokeThresholdData value)
		: base(value)
	{
	}

	public PokeThresholdDatumProperty(PokeThresholdDatum datum)
		: base(datum)
	{
	}
}
