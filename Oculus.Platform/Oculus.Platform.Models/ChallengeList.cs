using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class ChallengeList : DeserializableList<Challenge>
{
	public readonly ulong TotalCount;

	public ChallengeList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_ChallengeArray_GetSize(a);
		_Data = new List<Challenge>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new Challenge(CAPI.ovr_ChallengeArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
		TotalCount = CAPI.ovr_ChallengeArray_GetTotalCount(a);
		_PreviousUrl = CAPI.ovr_ChallengeArray_GetPreviousUrl(a);
		_NextUrl = CAPI.ovr_ChallengeArray_GetNextUrl(a);
	}
}
