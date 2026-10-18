using System.Reflection;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class WatchManagerForAddon : SubManagerForAddon
{
	public override string TelemetryAnnotation => "Watches";

	protected override bool RegisterSpecialisedWidget(IMember member, MemberInfo memberInfo, DebugMember memberAttribute, InstanceHandle handle)
	{
		if (!WatchManager.IsMemberValidForWatch(memberInfo))
		{
			return false;
		}
		Watch watch = member.GetWatch();
		if (watch == null || !watch.Matches(memberInfo, handle))
		{
			if (memberInfo.IsTypeEqual(typeof(Texture2D)))
			{
				member.RegisterTexture(WatchUtils.Create(memberInfo, handle, memberAttribute) as WatchTexture);
			}
			else
			{
				member.RegisterWatch(WatchUtils.Create(memberInfo, handle, memberAttribute));
			}
		}
		return true;
	}
}
