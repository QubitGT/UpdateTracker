using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal static class WatchUtils
{
	internal static readonly Dictionary<Type, Type> Types;

	private const int MaxLetterCount = 64;

	static WatchUtils()
	{
		Types = new Dictionary<Type, Type>();
		Types?.Clear();
		Register(delegate(float value, ref string[] valuesContainer)
		{
			valuesContainer[0] = FormatFloat(value);
		}, 1);
		Register(delegate(bool value, ref string[] valuesContainer)
		{
			valuesContainer[0] = (value ? "True" : "False");
		}, 1);
		Register(delegate(Vector3 value, ref string[] valuesContainer)
		{
			valuesContainer[0] = FormatFloat(value.x);
			valuesContainer[1] = FormatFloat(value.y);
			valuesContainer[2] = FormatFloat(value.z);
		}, 3);
		Register(delegate(Vector2 value, ref string[] valuesContainer)
		{
			valuesContainer[0] = FormatFloat(value.x);
			valuesContainer[1] = FormatFloat(value.y);
		}, 2);
		Register(delegate(string value, ref string[] valuesContainer)
		{
			valuesContainer[0] = ((value != null && value.Length > 64) ? (value.Substring(0, 64) + "...") : value);
		}, 1);
		RegisterTexture(typeof(Texture2D));
	}

	public static Watch Create(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
	{
		Type dataType = memberInfo.GetDataType();
		if (!Types.TryGetValue(dataType, out var value))
		{
			value = Register(dataType);
		}
		return Activator.CreateInstance(value, memberInfo, instanceHandle, attribute) as Watch;
	}

	internal static string FormatFloat(float value)
	{
		if (!(value > -10000000f) || !(value < 10000000f))
		{
			return value.ToString("g3", CultureInfo.InvariantCulture);
		}
		return value.ToString("0.00", CultureInfo.InvariantCulture);
	}

	private static Type Register<T>(Watch<T>.ToDisplayStringSignature toDisplayString, int numberOfValues)
	{
		Watch<T>.Setup(toDisplayString, numberOfValues);
		Type typeFromHandle = typeof(Watch<T>);
		Types.Add(typeof(T), typeFromHandle);
		return typeFromHandle;
	}

	private static Type Register(Type type)
	{
		Type type2 = typeof(Watch<>).MakeGenericType(type);
		Types.Add(type, type2);
		return type2;
	}

	private static Type RegisterTexture(Type type)
	{
		Type typeFromHandle = typeof(WatchTexture);
		Types.Add(type, typeFromHandle);
		return typeFromHandle;
	}
}
