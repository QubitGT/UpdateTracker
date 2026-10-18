using System;

namespace Meta.XR.Samples;

public class MetaCodeSampleAttribute : Attribute
{
	public string SampleName { get; private set; }

	public MetaCodeSampleAttribute(string sampleName)
	{
		SampleName = sampleName;
	}
}
