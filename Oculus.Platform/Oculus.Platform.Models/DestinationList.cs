using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class DestinationList : DeserializableList<Destination>
{
	public DestinationList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_DestinationArray_GetSize(a);
		_Data = new List<Destination>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new Destination(CAPI.ovr_DestinationArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
		_NextUrl = CAPI.ovr_DestinationArray_GetNextUrl(a);
	}
}
