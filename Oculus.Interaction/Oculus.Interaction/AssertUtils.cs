using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oculus.Interaction;

public static class AssertUtils
{
	public const string HiglightColor = "#3366ff";

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertIsTrue(this Component component, bool value, string whyItFailed = null, string whereItFailed = null, string howToFix = null)
	{
		_ = component.name;
		_ = component.GetType().Name;
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertAspect<TValue>(this Component component, TValue aspect, string aspectLocation, string whyItFailed = null, string whereFailed = null, string howToFix = null) where TValue : class
	{
		_ = component.name;
		_ = component.GetType().Name;
		Nicify(aspectLocation);
		_ = typeof(TValue).Name;
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertField<TValue>(this Component component, TValue value, string variableName, string whyItFailed = null, string whereItFailed = null, string howToFix = null) where TValue : class
	{
		_ = component.name;
		_ = component.GetType().Name;
		Nicify(variableName);
		_ = typeof(TValue).Name;
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertCollectionField<TValue>(this Component component, IEnumerable<TValue> value, string variableName, string whyItFailed = null, string whereFailed = null, string howToFix = null)
	{
		_ = component.name;
		_ = component.GetType().Name;
		Nicify(variableName);
		_ = typeof(TValue).Name;
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void AssertCollectionItems<TValue>(this Component component, IEnumerable<TValue> value, string variableName, string whyItFailed = null, string whereItFailed = null, string howToFix = null)
	{
		_ = component.name;
		_ = component.GetType().Name;
		Nicify(variableName);
		_ = typeof(TValue).Name;
		int num = 0;
		foreach (TValue item in value)
		{
			_ = item;
			num++;
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void WarnInspectorCollectionItems(this Component component, IEnumerable<Object> value, string variableName, string whyItFailed = null, string whereItFailed = null, string howToFix = null)
	{
		string name = component.name;
		string name2 = component.GetType().Name;
		string text = Nicify(variableName);
		string name3 = typeof(Object).Name;
		int num = 0;
		foreach (Object item in value)
		{
			string message = (whereItFailed ?? ("At GameObject <color=#3366ff><b>" + name + "</b></color>, component <b>" + name2 + "</b>. ")) + (whyItFailed ?? $"Invalid item in the collection <b>{text}</b> at index <b>{num}</b>. ") + (howToFix ?? $"Assign a <b>{name3}</b> to the collection <b>{text}</b> at index <b>{num}</b> or remove the entry. ");
			if (item == null)
			{
				UnityEngine.Debug.LogWarning(message, component);
			}
			num++;
		}
	}

	[Conditional("UNITY_ASSERTIONS")]
	public static void LogWarning(this Component component, string whyItFailed = null, string whereItFailed = null, string howToFix = null)
	{
		string name = component.name;
		string name2 = component.GetType().Name;
		UnityEngine.Debug.LogWarning((whereItFailed ?? ("At GameObject <color=#3366ff><b>" + name + "</b></color>, component <b>" + name2 + "</b>. ")) + (whyItFailed ?? string.Empty) + (howToFix ?? string.Empty), component);
	}

	public static string Nicify(string variableName)
	{
		variableName = Regex.Replace(variableName, "_([a-z])", (Match match) => match.Value.ToUpper(), RegexOptions.Compiled);
		variableName = Regex.Replace(variableName, "m_|_", " ", RegexOptions.Compiled);
		variableName = Regex.Replace(variableName, "k([A-Z])", "$1", RegexOptions.Compiled);
		variableName = Regex.Replace(variableName, "([A-Z])", " $1", RegexOptions.Compiled);
		variableName = variableName.Trim();
		return variableName;
	}
}
