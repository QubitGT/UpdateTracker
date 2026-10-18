using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class TweakManagerForAddon : SubManagerForAddon
{
	public override string TelemetryAnnotation => "Tweaks";

	protected override bool RegisterSpecialisedWidget(IMember member, MemberInfo memberInfo, DebugMember memberAttribute, InstanceHandle handle)
	{
		if (!memberAttribute.Tweakable || !TweakUtils.IsMemberValidForTweak(memberInfo))
		{
			return false;
		}
		Tweak tweak = member.GetTweak();
		if (tweak == null || !tweak.Matches(memberInfo, handle))
		{
			if (memberInfo.IsBaseTypeEqual(typeof(Enum)))
			{
				member.RegisterEnum(TweakUtils.Create(memberInfo, memberAttribute, handle, memberInfo.GetDataType()));
			}
			else
			{
				TweakUtils.ProcessMinMaxRange(memberInfo, memberAttribute, handle);
				member.RegisterTweak(TweakUtils.Create(memberInfo, memberAttribute, handle));
			}
		}
		return true;
	}
}
