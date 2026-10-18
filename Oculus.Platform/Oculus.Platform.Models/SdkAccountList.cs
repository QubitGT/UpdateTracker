using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class SdkAccountList : DeserializableList<SdkAccount>
{
	public SdkAccountList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_SdkAccountArray_GetSize(a);
		_Data = new List<SdkAccount>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new SdkAccount(CAPI.ovr_SdkAccountArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
