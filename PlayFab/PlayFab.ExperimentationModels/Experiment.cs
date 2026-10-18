using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.ExperimentationModels;

[Serializable]
public class Experiment : PlayFabBaseModel
{
	public string Description;

	public uint Duration;

	public ExperimentType? ExperimentType;

	public string Id;

	public string Name;

	public string SegmentId;

	public DateTime StartDate;

	public ExperimentState? State;

	public List<string> TitlePlayerAccountTestIds;

	public List<Variant> Variants;
}
