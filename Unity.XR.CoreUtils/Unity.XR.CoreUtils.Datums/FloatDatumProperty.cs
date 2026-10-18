using System;

namespace Unity.XR.CoreUtils.Datums;

[Serializable]
public class FloatDatumProperty : DatumProperty<float, FloatDatum>
{
	public FloatDatumProperty(float value)
		: base(value)
	{
	}

	public FloatDatumProperty(FloatDatum datum)
		: base(datum)
	{
	}
}
