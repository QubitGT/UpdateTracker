using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class AssetDetailsList : DeserializableList<AssetDetails>
{
	public AssetDetailsList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_AssetDetailsArray_GetSize(a);
		_Data = new List<AssetDetails>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new AssetDetails(CAPI.ovr_AssetDetailsArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
