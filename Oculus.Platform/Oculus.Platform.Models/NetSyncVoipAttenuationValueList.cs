using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class NetSyncVoipAttenuationValueList : DeserializableList<NetSyncVoipAttenuationValue>
{
	public NetSyncVoipAttenuationValueList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_NetSyncVoipAttenuationValueArray_GetSize(a);
		_Data = new List<NetSyncVoipAttenuationValue>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new NetSyncVoipAttenuationValue(CAPI.ovr_NetSyncVoipAttenuationValueArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
