using System;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportVolumeDestinationSettingsDatumProperty : DatumProperty<TeleportVolumeDestinationSettings, TeleportVolumeDestinationSettingsDatum>
{
	public TeleportVolumeDestinationSettingsDatumProperty(TeleportVolumeDestinationSettings value)
		: base(value)
	{
	}

	public TeleportVolumeDestinationSettingsDatumProperty(TeleportVolumeDestinationSettingsDatum datum)
		: base(datum)
	{
	}
}
