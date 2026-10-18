using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class ChallengeEntryList : DeserializableList<ChallengeEntry>
{
	public readonly ulong TotalCount;

	public ChallengeEntryList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_ChallengeEntryArray_GetSize(a);
		_Data = new List<ChallengeEntry>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new ChallengeEntry(CAPI.ovr_ChallengeEntryArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
		TotalCount = CAPI.ovr_ChallengeEntryArray_GetTotalCount(a);
		_PreviousUrl = CAPI.ovr_ChallengeEntryArray_GetPreviousUrl(a);
		_NextUrl = CAPI.ovr_ChallengeEntryArray_GetNextUrl(a);
	}
}
