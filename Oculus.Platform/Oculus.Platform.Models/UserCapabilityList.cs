using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class UserCapabilityList : DeserializableList<UserCapability>
{
	public UserCapabilityList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_UserCapabilityArray_GetSize(a);
		_Data = new List<UserCapability>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new UserCapability(CAPI.ovr_UserCapabilityArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
		_NextUrl = CAPI.ovr_UserCapabilityArray_GetNextUrl(a);
	}
}
