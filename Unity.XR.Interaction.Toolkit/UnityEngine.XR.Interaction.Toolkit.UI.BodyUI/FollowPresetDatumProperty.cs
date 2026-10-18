using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;

[Serializable]
public class FollowPresetDatumProperty : DatumProperty<FollowPreset, FollowPresetDatum>
{
	public FollowPresetDatumProperty(FollowPreset value)
		: base(value)
	{
	}

	public FollowPresetDatumProperty(FollowPresetDatum datum)
		: base(datum)
	{
	}
}
