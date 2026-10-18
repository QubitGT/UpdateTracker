using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Meta.WitAi;
using Meta.WitAi.Json;

namespace Meta.Conduit;

internal class ParameterProvider : IParameterProvider
{
	public const string WitResponseNodeReservedName = "@WitResponseNode";

	public const string VoiceSessionReservedName = "@VoiceSession";

	protected readonly Dictionary<string, object> ActualParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string> _parameterToRoleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<Type, List<string>> _parametersOfType = new Dictionary<Type, List<string>>();

	private readonly Dictionary<Type, string> _specializedParameters = new Dictionary<Type, string>();

	private static readonly Dictionary<string, List<Type>> BuiltInTypes = new Dictionary<string, List<Type>>
	{
		{
			"wit$age_of_person",
			new List<Type>
			{
				typeof(int),
				typeof(short),
				typeof(long),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			"wit$amount_of_money",
			new List<Type>
			{
				typeof(decimal),
				typeof(float),
				typeof(double),
				typeof(int)
			}
		},
		{
			"wit$datetime",
			new List<Type> { typeof(DateTime) }
		},
		{
			"wit$distance",
			new List<Type>
			{
				typeof(decimal),
				typeof(float),
				typeof(double),
				typeof(int)
			}
		},
		{
			"wit$duration",
			new List<Type>
			{
				typeof(TimeSpan),
				typeof(float),
				typeof(double),
				typeof(int),
				typeof(decimal)
			}
		},
		{
			"wit$number",
			new List<Type>
			{
				typeof(int),
				typeof(long),
				typeof(short),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			"wit$ordinal",
			new List<Type>
			{
				typeof(int),
				typeof(long),
				typeof(short)
			}
		},
		{
			"wit$quantity",
			new List<Type>
			{
				typeof(int),
				typeof(long),
				typeof(short),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		},
		{
			"wit$temperature",
			new List<Type>
			{
				typeof(decimal),
				typeof(float),
				typeof(double),
				typeof(int),
				typeof(short),
				typeof(long)
			}
		},
		{
			"wit$volume",
			new List<Type>
			{
				typeof(int),
				typeof(long),
				typeof(short),
				typeof(float),
				typeof(double),
				typeof(decimal)
			}
		}
	};

	private readonly Dictionary<string, Type> _customTypes = new Dictionary<string, Type>();

	public List<string> AllParameterNames => ActualParameters.Keys.ToList();

	public void AddCustomType(string name, Type type)
	{
		_customTypes[name] = type;
	}

	public void AddParameter(string parameterName, object value)
	{
		ActualParameters[parameterName] = value;
	}

	public void PopulateParametersFromNode(WitResponseNode responseNode)
	{
		_parametersOfType.Clear();
		Dictionary<string, ConduitParameterValue> dictionary = new Dictionary<string, ConduitParameterValue>();
		foreach (WitResponseNode child in responseNode.AsObject["entities"].Childs)
		{
			string value = child[0]["role"].Value;
			string value2 = child[0]["value"].Value;
			List<Type> list = GetParameterTypes(child[0]["name"].Value, value2).ToList();
			foreach (Type item in list)
			{
				if (!_parametersOfType.ContainsKey(item))
				{
					_parametersOfType.Add(item, new List<string>());
				}
				_parametersOfType[item].Add(value);
			}
			dictionary.Add(value, new ConduitParameterValue(value2, list.First()));
		}
		dictionary.Add("@WitResponseNode", new ConduitParameterValue(responseNode, typeof(WitResponseNode)));
		PopulateParameters(dictionary);
	}

	public void SetSpecializedParameter(string reservedParameterName, Type parameterType)
	{
		_specializedParameters[parameterType] = reservedParameterName.ToLower();
	}

	public void PopulateParameters(Dictionary<string, ConduitParameterValue> actualParameters)
	{
		ActualParameters.Clear();
		foreach (KeyValuePair<string, ConduitParameterValue> actualParameter in actualParameters)
		{
			ActualParameters[actualParameter.Key] = actualParameter.Value.Value;
		}
	}

	public void PopulateRoles(Dictionary<string, string> parameterToRoleMap)
	{
		_parameterToRoleMap.Clear();
		foreach (KeyValuePair<string, string> item in parameterToRoleMap)
		{
			_parameterToRoleMap[item.Key.ToLower()] = item.Value;
		}
	}

	public bool ContainsParameter(ParameterInfo parameter, StringBuilder log)
	{
		if (SupportedSpecializedParameter(parameter))
		{
			return true;
		}
		if (!ActualParameters.ContainsKey(parameter.Name))
		{
			log.AppendLine("\tParameter '" + parameter.Name + "' not sent in invoke");
			return false;
		}
		if (!_parameterToRoleMap.ContainsKey(parameter.Name))
		{
			log.AppendLine("\tParameter '" + parameter.Name + "' not found in role map");
			return false;
		}
		return true;
	}

	public object GetRawParameterValue(string parameterName)
	{
		if (!ActualParameters.TryGetValue(parameterName, out var value) || value == null)
		{
			return null;
		}
		return value;
	}

	public object GetParameterValue(ParameterInfo formalParameter, Dictionary<string, string> parameterMap = null, bool relaxed = false)
	{
		if (parameterMap == null)
		{
			parameterMap = new Dictionary<string, string>();
		}
		if (SupportedSpecializedParameter(formalParameter))
		{
			return GetSpecializedParameter(formalParameter);
		}
		string actualParameterName = GetActualParameterName(formalParameter, parameterMap, relaxed);
		if (string.IsNullOrEmpty(actualParameterName))
		{
			return null;
		}
		if (!ActualParameters.TryGetValue(actualParameterName, out var value) || value == null)
		{
			return null;
		}
		return ConduitUtilities.GetTypedParameterValue(formalParameter, value);
	}

	public T GetParameterValue<T>(string parameterName, Dictionary<string, string> parameterMap = null, bool relaxed = false)
	{
		if (!ActualParameters.TryGetValue(parameterName, out var value) || value == null)
		{
			return default(T);
		}
		return (T)ConduitUtilities.GetTypedParameterValue(typeof(T), value);
	}

	private static object ToNullable(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		Type type = obj.GetType();
		if (type.IsNullableType())
		{
			return obj;
		}
		Type conversionType = typeof(Nullable<>).MakeGenericType(type);
		return Convert.ChangeType(obj, conversionType);
	}

	public List<string> GetParameterNamesOfType(Type targetType)
	{
		if (_parametersOfType.ContainsKey(targetType))
		{
			return _parametersOfType[targetType];
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, object> actualParameter in ActualParameters)
		{
			Type type = actualParameter.Value.GetType();
			if (type == targetType)
			{
				list.Add(actualParameter.Key);
			}
			else if (targetType.IsNullableType())
			{
				Type underlyingType = Nullable.GetUnderlyingType(targetType);
				object result;
				if (underlyingType == null)
				{
					VLog.E($"Got a null underlying type from nullable type {targetType}");
				}
				else if (type == underlyingType)
				{
					list.Add(actualParameter.Key);
				}
				else if (underlyingType.IsEnum && actualParameter.Value is string value && Enum.TryParse(underlyingType, value, out result))
				{
					list.Add(actualParameter.Key);
				}
			}
		}
		return _parametersOfType[targetType] = list;
	}

	protected virtual bool SupportedSpecializedParameter(ParameterInfo formalParameter)
	{
		return _specializedParameters.ContainsKey(formalParameter.ParameterType);
	}

	protected virtual object GetSpecializedParameter(ParameterInfo formalParameter)
	{
		if (_specializedParameters.ContainsKey(formalParameter.ParameterType))
		{
			string key = _specializedParameters[formalParameter.ParameterType];
			if (ActualParameters.ContainsKey(key))
			{
				return ActualParameters[key];
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Specialized parameter not found");
		stringBuilder.AppendLine($"Parameter Type: {formalParameter.ParameterType}");
		stringBuilder.AppendLine("Parameter Name: " + formalParameter.Name);
		stringBuilder.AppendLine($"Actual Parameters: {ActualParameters.Keys.Count}");
		foreach (string key2 in ActualParameters.Keys)
		{
			string text = ((ActualParameters[key2] == null) ? "NULL" : ActualParameters[key2].GetType().ToString());
			stringBuilder.AppendLine("\t" + key2 + ": " + text);
		}
		VLog.W(stringBuilder.ToString());
		return null;
	}

	private IEnumerable<Type> GetParameterTypes(string typeString, string value)
	{
		if (_customTypes.ContainsKey(typeString))
		{
			return new List<Type> { _customTypes[typeString] };
		}
		if (!BuiltInTypes.ContainsKey(typeString) || BuiltInTypes[typeString].Count == 0)
		{
			return new List<Type> { typeof(string) };
		}
		return BuiltInTypes[typeString].Where((Type type) => PerfectTypeMatch(type, value)).ToList();
	}

	private bool PerfectTypeMatch(Type targetType, string value)
	{
		try
		{
			object obj = Convert.ChangeType(value, targetType);
			if (obj == null)
			{
				return false;
			}
			if (!targetType.IsPrimitive)
			{
				return true;
			}
			return value.Equals(obj.ToString());
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string GetActualParameterName(ParameterInfo formalParameter, Dictionary<string, string> parameterMap, bool relaxed)
	{
		string name = formalParameter.Name;
		string text = ((!parameterMap.ContainsKey(name)) ? name : parameterMap[name]);
		if (ActualParameters.ContainsKey(text))
		{
			return text;
		}
		if (_parameterToRoleMap.ContainsKey(text))
		{
			string text2 = _parameterToRoleMap[text];
			if (!string.IsNullOrEmpty(text2) && ActualParameters.ContainsKey(text2))
			{
				return text2;
			}
		}
		if (!relaxed)
		{
			if (formalParameter.ParameterType.IsNullableType())
			{
				return null;
			}
			VLog.E("Parameter '" + name + "' is missing");
			return null;
		}
		Type parameterType = formalParameter.ParameterType;
		List<string> parameterNamesOfType = GetParameterNamesOfType(parameterType);
		if (parameterNamesOfType.Count > 1)
		{
			VLog.E($"Got multiple parameters of type {parameterType} but none with the correct name");
			return null;
		}
		if (parameterNamesOfType.Count == 0)
		{
			if (!formalParameter.ParameterType.IsNullableType())
			{
				VLog.E($"Got zero parameters of type {parameterType}.");
			}
			return null;
		}
		return parameterNamesOfType[0];
	}

	public override string ToString()
	{
		return string.Join("',", AllParameterNames);
	}
}
