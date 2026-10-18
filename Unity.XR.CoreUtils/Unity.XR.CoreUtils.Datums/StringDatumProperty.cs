using System;

namespace Unity.XR.CoreUtils.Datums;

[Serializable]
public class StringDatumProperty : DatumProperty<string, StringDatum>
{
	public StringDatumProperty(string value)
		: base(value)
	{
	}

	public StringDatumProperty(StringDatum datum)
		: base(datum)
	{
	}
}
