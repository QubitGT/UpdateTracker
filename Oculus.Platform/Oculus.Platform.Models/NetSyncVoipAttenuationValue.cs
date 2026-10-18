using System;

namespace Oculus.Platform.Models;

public class NetSyncVoipAttenuationValue
{
	public readonly float Decibels;

	public readonly float Distance;

	public NetSyncVoipAttenuationValue(IntPtr o)
	{
		Decibels = CAPI.ovr_NetSyncVoipAttenuationValue_GetDecibels(o);
		Distance = CAPI.ovr_NetSyncVoipAttenuationValue_GetDistance(o);
	}
}
