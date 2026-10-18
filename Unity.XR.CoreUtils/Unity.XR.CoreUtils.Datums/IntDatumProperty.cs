using System;

namespace Unity.XR.CoreUtils.Datums;

[Serializable]
public class IntDatumProperty : DatumProperty<int, IntDatum>
{
	public IntDatumProperty(int value)
		: base(value)
	{
	}

	public IntDatumProperty(IntDatum datum)
		: base(datum)
	{
	}
}
