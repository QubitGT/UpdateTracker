using System;
using Oculus.Platform.Models;

namespace Oculus.Platform;

public class MessageWithAchievementProgressList : Message<AchievementProgressList>
{
	public MessageWithAchievementProgressList(IntPtr c_message)
		: base(c_message)
	{
	}

	public override AchievementProgressList GetAchievementProgressList()
	{
		return base.Data;
	}

	protected override AchievementProgressList GetDataFromMessage(IntPtr c_message)
	{
		return new AchievementProgressList(CAPI.ovr_Message_GetAchievementProgressArray(CAPI.ovr_Message_GetNativeMessage(c_message)));
	}
}
