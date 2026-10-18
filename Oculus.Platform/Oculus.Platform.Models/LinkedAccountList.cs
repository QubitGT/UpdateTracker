using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class LinkedAccountList : DeserializableList<LinkedAccount>
{
	public LinkedAccountList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_LinkedAccountArray_GetSize(a);
		_Data = new List<LinkedAccount>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new LinkedAccount(CAPI.ovr_LinkedAccountArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
