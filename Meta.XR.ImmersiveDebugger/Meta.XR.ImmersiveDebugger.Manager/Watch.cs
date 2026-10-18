using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal abstract class Watch : Hook
{
	public abstract string Value { get; }

	public abstract string[] Values { get; }

	public abstract int NumberOfValues { get; }

	protected Watch(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
	}
}
internal class Watch<T> : Watch
{
	public delegate void ToDisplayStringSignature(T value, ref string[] valuesContainer);

	private static string[] _buffer = new string[1];

	private readonly Func<T> _getter;

	public static ToDisplayStringSignature ToDisplayStringsDelegate { get; private set; } = null;


	public static int NumberOfDisplayStrings { get; private set; } = 1;


	public override int NumberOfValues => NumberOfDisplayStrings;

	public override string[] Values => ToDisplayStrings(_getter());

	public override string Value => Values[0];

	internal static void ResetBuffer()
	{
		_buffer = new string[NumberOfDisplayStrings];
	}

	public static void Setup(ToDisplayStringSignature del, int numberOfValues)
	{
		ToDisplayStringsDelegate = del;
		NumberOfDisplayStrings = numberOfValues;
		ResetBuffer();
	}

	public static string[] ToDisplayStrings(T value)
	{
		if (ToDisplayStringsDelegate != null)
		{
			ToDisplayStringsDelegate(value, ref _buffer);
		}
		else
		{
			_buffer[0] = ((value != null) ? value.ToString() : "");
		}
		return _buffer;
	}

	public Watch(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
		Watch<T> watch = this;
		_getter = () => (T)memberInfo.GetValue(watch._instance);
	}
}
