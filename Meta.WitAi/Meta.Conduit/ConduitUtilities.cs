using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Meta.WitAi;

namespace Meta.Conduit;

internal static class ConduitUtilities
{
	public delegate void ProgressDelegate(string status, float progress);

	private static readonly Regex UnderscoreSplitter = new Regex("(\\B[A-Z])", RegexOptions.Compiled);

	public static string DelimitWithUnderscores(string input)
	{
		return UnderscoreSplitter.Replace(input, "_$1");
	}

	public static bool IsNullableType(this Type type)
	{
		if (type.GetTypeInfo().IsGenericType)
		{
			return (object)type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	public static bool ContainsIgnoringWhitespace(string stringToSearch, string value)
	{
		stringToSearch = StripWhiteSpace(stringToSearch);
		value = StripWhiteSpace(value);
		return stringToSearch.Contains(value);
	}

	internal static object GetTypedParameterValue(ParameterInfo formalParameter, object parameterValue)
	{
		return GetTypedParameterValue(formalParameter.ParameterType, parameterValue);
	}

	internal static object GetTypedParameterValue(Type parameterType, object parameterValue)
	{
		if (parameterValue == null)
		{
			return null;
		}
		Type type = parameterType;
		if (type.IsNullableType())
		{
			type = Nullable.GetUnderlyingType(type);
			if (type == null)
			{
				VLog.E($"Got null underlying type for nullable parameter of type {parameterType}");
				return null;
			}
		}
		if (type == typeof(string))
		{
			return parameterValue.ToString();
		}
		if (type.IsEnum)
		{
			try
			{
				return Enum.Parse(type, SanitizeString(parameterValue.ToString()), ignoreCase: true);
			}
			catch (Exception arg)
			{
				VLog.E($"Parameter value '{parameterValue}' could not be cast to enum\nEnum Type: {type.FullName}\n{arg}");
				throw;
			}
		}
		try
		{
			return Convert.ChangeType(parameterValue, type);
		}
		catch (Exception arg2)
		{
			VLog.E($"Nullable parameter value '{parameterValue}' could not be cast to {type.FullName}\n{arg2}");
			return null;
		}
	}

	public static string GetEntityEnumName(string entityRole)
	{
		return SanitizeName(entityRole);
	}

	public static string GetEntityEnumValue(string entityValue)
	{
		return SanitizeString(entityValue);
	}

	public static string SanitizeName(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}
		string text = SanitizeString(input);
		return text[0].ToString().ToUpper() + text.Substring(1);
	}

	public static string SanitizeString(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}
		string text = Regex.Replace(input, "[^\\w_-]", "");
		if (Regex.IsMatch(text[0].ToString(), "^\\d$"))
		{
			text = "N" + text;
		}
		return text;
	}

	private static string StripWhiteSpace(string input)
	{
		if (!string.IsNullOrEmpty(input))
		{
			return input.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
		}
		return string.Empty;
	}
}
