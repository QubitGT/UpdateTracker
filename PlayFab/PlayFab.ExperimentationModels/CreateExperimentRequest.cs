using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class CreateExperimentRequest : PlayFabRequestCommon
{
	public string Description;

	public uint Duration;

	public ExperimentType? ExperimentType;

	public string Name;

	public string SegmentId;

	public DateTime StartDate;

	public List<string> TitlePlayerAccountTestIds;

	public List<Variant> Variants;
}
