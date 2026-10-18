using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class TweakEnum : Tweak
{
	private readonly Type _enumType;

	public MemberInfo Member => _memberInfo;

	public string Value
	{
		get
		{
			return _memberInfo.GetValue(_instance).ToString();
		}
		set
		{
			object value2 = Enum.Parse(_enumType, value);
			_memberInfo.SetValue(_instance, value2);
		}
	}

	public override float Tween { get; set; }

	public TweakEnum(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute, Type enumType)
		: base(memberInfo, instanceHandle, attribute)
	{
		_enumType = enumType;
	}
}
