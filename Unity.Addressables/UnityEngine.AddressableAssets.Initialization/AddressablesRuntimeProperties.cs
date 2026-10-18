using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.AddressableAssets.Initialization;

public static class AddressablesRuntimeProperties
{
	private static Stack<string> s_TokenStack = new Stack<string>(32);

	private static Stack<int> s_TokenStartStack = new Stack<int>(32);

	private static bool s_StaticStacksAreInUse = false;

	private static Dictionary<string, string> s_CachedValues = new Dictionary<string, string>();

	private static Assembly[] GetAssemblies()
	{
		return AppDomain.CurrentDomain.GetAssemblies();
	}

	internal static int GetCachedValueCount()
	{
		return s_CachedValues.Count;
	}

	public static void SetPropertyValue(string name, string val)
	{
		s_CachedValues[name] = val;
	}

	public static void ClearCachedPropertyValues()
	{
		s_CachedValues.Clear();
	}

	public static string EvaluateProperty(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		if (s_CachedValues.TryGetValue(name, out var value))
		{
			return value;
		}
		int num = name.LastIndexOf('.');
		if (num < 0)
		{
			return name;
		}
		string name2 = name.Substring(0, num);
		string name3 = name.Substring(num + 1);
		Assembly[] assemblies = GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type type = assemblies[i].GetType(name2, throwOnError: false, ignoreCase: false);
			if (type == null)
			{
				continue;
			}
			try
			{
				PropertyInfo property = type.GetProperty(name3, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (property != null)
				{
					object value2 = property.GetValue(null, null);
					if (value2 != null)
					{
						s_CachedValues.Add(name, value2.ToString());
						return value2.ToString();
					}
				}
				FieldInfo field = type.GetField(name3, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (field != null)
				{
					object value3 = field.GetValue(null);
					if (value3 != null)
					{
						s_CachedValues.Add(name, value3.ToString());
						return value3.ToString();
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return name;
	}

	public static string EvaluateString(string inputString)
	{
		if (string.IsNullOrEmpty(inputString))
		{
			return string.Empty;
		}
		if (!inputString.Contains('{', StringComparison.Ordinal))
		{
			return inputString;
		}
		return EvaluateStringInternal(inputString, '{', '}', EvaluateProperty);
	}

	public static string EvaluateString(string inputString, char startDelimiter, char endDelimiter, Func<string, string> varFunc)
	{
		if (string.IsNullOrEmpty(inputString))
		{
			return string.Empty;
		}
		if (!inputString.Contains(startDelimiter, StringComparison.Ordinal))
		{
			return inputString;
		}
		return EvaluateStringInternal(inputString, startDelimiter, endDelimiter, varFunc);
	}

	private static string EvaluateStringInternal(string inputString, char startDelimiter, char endDelimiter, Func<string, string> varFunc)
	{
		string text = inputString;
		Stack<string> stack;
		Stack<int> stack2;
		if (!s_StaticStacksAreInUse)
		{
			stack = s_TokenStack;
			stack2 = s_TokenStartStack;
			s_StaticStacksAreInUse = true;
		}
		else
		{
			stack = new Stack<string>(32);
			stack2 = new Stack<int>(32);
		}
		stack.Push(inputString);
		int num = inputString.Length;
		char[] anyOf = new char[2] { startDelimiter, endDelimiter };
		bool flag = startDelimiter == endDelimiter;
		int num2 = inputString.IndexOf(startDelimiter);
		int num3 = -2;
		while (num2 >= 0)
		{
			char c = inputString[num2];
			if (c == startDelimiter && (!flag || stack2.Count == 0))
			{
				stack2.Push(num2);
				num2++;
			}
			else if (c == endDelimiter && stack2.Count > 0)
			{
				int num4 = stack2.Peek();
				string text2 = inputString.Substring(num4 + 1, num2 - num4 - 1);
				if (num <= num2)
				{
					stack.Pop();
				}
				string text3;
				if (stack.Contains(text2))
				{
					text3 = "#ERROR-CyclicToken#";
				}
				else
				{
					text3 = ((varFunc == null) ? string.Empty : varFunc(text2));
					stack.Push(text2);
				}
				num2 = stack2.Pop();
				num = num2 + text3.Length + 1;
				if (num2 > 0)
				{
					int num5 = num2 + text2.Length + 2;
					inputString = ((num5 != inputString.Length) ? (inputString.Substring(0, num2) + text3 + inputString.Substring(num5)) : (inputString.Substring(0, num2) + text3));
				}
				else
				{
					inputString = text3 + inputString.Substring(num2 + text2.Length + 2);
				}
			}
			if (num3 == num2)
			{
				return "#ERROR-" + text + " contains unmatched delimiters#";
			}
			num3 = num2;
			num2 = inputString.IndexOfAny(anyOf, num2);
		}
		stack.Clear();
		stack2.Clear();
		if (stack == s_TokenStack)
		{
			s_StaticStacksAreInUse = false;
		}
		return inputString;
	}
}
