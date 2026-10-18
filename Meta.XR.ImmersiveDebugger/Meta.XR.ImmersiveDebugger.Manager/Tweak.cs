using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal abstract class Tweak : Hook
{
	public abstract float Tween { get; set; }

	protected Tweak(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
	}
}
internal class Tweak<T> : Tweak
{
	public static Func<T, T, T, float> InverseLerp;

	public static Func<T, T, float, T> Lerp;

	public static Func<float, T> FromFloat;

	private readonly Func<T> _getter;

	private readonly Action<T> _setter;

	private readonly T _min;

	private readonly T _max;

	public override float Tween
	{
		get
		{
			return InverseLerp(_min, _max, _getter());
		}
		set
		{
			_setter(Lerp(_min, _max, value));
		}
	}

	public Tweak(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
		Tweak<T> tweak = this;
		_min = FromFloat(attribute.Min);
		_max = FromFloat(attribute.Max);
		_getter = () => (T)memberInfo.GetValue(tweak._instance);
		_setter = delegate(T value)
		{
			memberInfo.SetValue(tweak._instance, value);
		};
	}
}
