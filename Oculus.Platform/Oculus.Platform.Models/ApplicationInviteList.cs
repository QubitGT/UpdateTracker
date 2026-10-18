using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class ApplicationInviteList : DeserializableList<ApplicationInvite>
{
	public ApplicationInviteList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_ApplicationInviteArray_GetSize(a);
		_Data = new List<ApplicationInvite>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new ApplicationInvite(CAPI.ovr_ApplicationInviteArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
		_NextUrl = CAPI.ovr_ApplicationInviteArray_GetNextUrl(a);
	}
}
