using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Meta.WitAi.Json;

public static class JsonConvert
{
	private static JsonConverter[] _defaultConverters = new JsonConverter[3]
	{
		new ColorConverter(),
		new DateTimeConverter(),
		new HashSetConverter<string>()
	};

	private const BindingFlags BIND_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static MethodInfo _enumParseMethod;

	public static JsonConverter[] DefaultConverters => _defaultConverters;

	private static object EnsureExists(Type objType, object obj)
	{
		if (obj == null && objType != null)
		{
			if (objType == typeof(string))
			{
				return string.Empty;
			}
			if (objType.IsArray)
			{
				return Activator.CreateInstance(objType, 0);
			}
			return Activator.CreateInstance(objType);
		}
		return obj;
	}

	public static WitResponseNode DeserializeToken(string jsonString)
	{
		if (string.IsNullOrEmpty(jsonString))
		{
			VLog.W("Parse Failed\nNo content provided");
			return null;
		}
		try
		{
			return WitResponseNode.Parse(jsonString);
		}
		catch (Exception e)
		{
			VLog.W((object)("Parse Failed\n\n" + jsonString), e);
			return null;
		}
	}

	public static async Task<WitResponseNode> DeserializeTokenAsync(string jsonString)
	{
		WitResponseNode result = null;
		await Task.Run(() => result = DeserializeToken(jsonString));
		return result;
	}

	public static IN_TYPE DeserializeObject<IN_TYPE>(string jsonString, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		return DeserializeIntoObject((IN_TYPE)EnsureExists(typeof(IN_TYPE), null), jsonString, customConverters, suppressWarnings);
	}

	public static async Task<IN_TYPE> DeserializeObjectAsync<IN_TYPE>(string jsonString, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		IN_TYPE result = default(IN_TYPE);
		await Task.Run(() => result = DeserializeObject<IN_TYPE>(jsonString, customConverters, suppressWarnings));
		return result;
	}

	public static IN_TYPE DeserializeObject<IN_TYPE>(WitResponseNode jsonToken, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		return DeserializeIntoObject((IN_TYPE)EnsureExists(typeof(IN_TYPE), null), jsonToken, customConverters, suppressWarnings);
	}

	public static async Task<IN_TYPE> DeserializeObjectAsync<IN_TYPE>(WitResponseNode jsonToken, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		IN_TYPE result = default(IN_TYPE);
		await Task.Run(() => result = DeserializeObject<IN_TYPE>(jsonToken, customConverters, suppressWarnings));
		return result;
	}

	public static IN_TYPE DeserializeIntoObject<IN_TYPE>(IN_TYPE instance, string jsonString, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		return DeserializeIntoObject(instance, DeserializeToken(jsonString), customConverters, suppressWarnings);
	}

	public static async Task<IN_TYPE> DeserializeIntoObjectAsync<IN_TYPE>(IN_TYPE instance, string jsonString, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		IN_TYPE result = default(IN_TYPE);
		await Task.Run(() => result = DeserializeIntoObject(instance, jsonString, customConverters, suppressWarnings));
		return result;
	}

	public static IN_TYPE DeserializeIntoObject<IN_TYPE>(IN_TYPE instance, WitResponseNode jsonToken, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		try
		{
			if (jsonToken == null)
			{
				return instance;
			}
			if (customConverters == null)
			{
				customConverters = DefaultConverters;
			}
			Type typeFromHandle = typeof(IN_TYPE);
			if (typeFromHandle == typeof(WitResponseNode))
			{
				return (IN_TYPE)(object)jsonToken;
			}
			if (typeFromHandle == typeof(WitResponseClass))
			{
				return (IN_TYPE)(object)jsonToken.AsObject;
			}
			if (typeFromHandle == typeof(WitResponseArray))
			{
				return (IN_TYPE)(object)jsonToken.AsArray;
			}
			StringBuilder stringBuilder = new StringBuilder();
			IN_TYPE result = (IN_TYPE)DeserializeToken(typeFromHandle, instance, jsonToken, stringBuilder, customConverters);
			if (stringBuilder.Length > 0 && !suppressWarnings)
			{
				VLog.D($"Deserialize Warnings\n{stringBuilder}");
			}
			return result;
		}
		catch (Exception e)
		{
			VLog.E((object)$"Deserialize Failed\nTo: {typeof(IN_TYPE)}", e);
			return instance;
		}
	}

	public static async Task<IN_TYPE> DeserializeIntoObjectAsync<IN_TYPE>(IN_TYPE instance, WitResponseNode jsonToken, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		IN_TYPE result = default(IN_TYPE);
		await Task.Run(() => result = DeserializeIntoObject(instance, jsonToken, customConverters, suppressWarnings));
		return result;
	}

	private static object DeserializeToken(Type toType, object oldValue, WitResponseNode jsonToken, StringBuilder log, JsonConverter[] customConverters)
	{
		if (customConverters != null)
		{
			foreach (JsonConverter jsonConverter in customConverters)
			{
				if (jsonConverter.CanRead && jsonConverter.CanConvert(toType))
				{
					return jsonConverter.ReadJson(jsonToken, toType, oldValue);
				}
			}
		}
		if (toType == typeof(string))
		{
			return jsonToken.Value;
		}
		if (toType.IsEnum)
		{
			string enumString = jsonToken.Value;
			foreach (object value in Enum.GetValues(toType))
			{
				object[] customAttributes = toType.GetMember(value.ToString())[0].GetCustomAttributes(typeof(JsonPropertyAttribute), inherit: false);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					JsonPropertyAttribute jsonPropertyAttribute = (JsonPropertyAttribute)customAttributes[i];
					if (!string.IsNullOrEmpty(jsonPropertyAttribute.PropertyName) && string.Equals(jsonToken.Value, jsonPropertyAttribute.PropertyName, StringComparison.CurrentCultureIgnoreCase))
					{
						enumString = value.ToString();
						break;
					}
				}
			}
			return DeserializeEnum(toType, EnsureExists(toType, oldValue), enumString, log);
		}
		if (toType.GetInterfaces().Contains(typeof(IDictionary)))
		{
			return DeserializeDictionary(toType, EnsureExists(toType, oldValue), jsonToken.AsObject, log, customConverters);
		}
		if (toType.GetInterfaces().Contains(typeof(IEnumerable)))
		{
			Type type = toType.GetElementType();
			if (type == null)
			{
				Type[] genericArguments = toType.GetGenericArguments();
				if (genericArguments != null && genericArguments.Length != 0)
				{
					type = genericArguments[0];
				}
			}
			if (type != null)
			{
				object obj = (obj = typeof(JsonConvert).GetMethod("DeserializeArray", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, new object[4] { oldValue, jsonToken, log, customConverters }));
				if (toType.IsArray)
				{
					return obj;
				}
				if (toType.GetInterfaces().Contains(typeof(IList)))
				{
					return Activator.CreateInstance(toType, obj);
				}
			}
		}
		if (toType.IsClass)
		{
			return DeserializeClass(toType, oldValue, jsonToken.AsObject, log, customConverters);
		}
		if (toType.IsValueType && !toType.IsPrimitive)
		{
			object oldObject = Activator.CreateInstance(toType);
			return DeserializeClass(toType, oldObject, jsonToken.AsObject, log, customConverters);
		}
		try
		{
			return Convert.ChangeType(jsonToken.Value, toType);
		}
		catch (Exception arg)
		{
			log.AppendLine($"\nJson Deserializer failed to cast '{jsonToken.Value}' to type '{toType}'\n{arg}");
			return oldValue;
		}
	}

	private static object DeserializeEnum(Type toType, object oldValue, string enumString, StringBuilder log)
	{
		if (_enumParseMethod == null)
		{
			_enumParseMethod = typeof(Enum).GetMethods().ToList().Find((MethodInfo method) => method.IsGenericMethod && method.GetParameters().Length == 3 && string.Equals(method.Name, "TryParse"));
		}
		MethodInfo methodInfo = _enumParseMethod.MakeGenericMethod(toType);
		object[] array = new object[3]
		{
			enumString,
			false,
			Activator.CreateInstance(toType)
		};
		if ((bool)methodInfo.Invoke(null, array))
		{
			return array[2];
		}
		log.AppendLine($"\nJson Deserializer Failed to cast '{enumString}' to enum type '{toType}'");
		return oldValue;
	}

	[Preserve]
	public static ITEM_TYPE[] DeserializeArray<ITEM_TYPE>(object oldArray, WitResponseNode jsonToken, StringBuilder log, JsonConverter[] customConverters)
	{
		if (jsonToken == null)
		{
			return (ITEM_TYPE[])oldArray;
		}
		WitResponseArray asArray = jsonToken.AsArray;
		ITEM_TYPE[] array = new ITEM_TYPE[asArray.Count];
		Type typeFromHandle = typeof(ITEM_TYPE);
		for (int i = 0; i < asArray.Count; i++)
		{
			object oldValue = EnsureExists(typeFromHandle, null);
			ITEM_TYPE val = (ITEM_TYPE)DeserializeToken(typeFromHandle, oldValue, asArray[i], log, customConverters);
			array[i] = val;
		}
		return array;
	}

	private static object DeserializeClass(Type toType, object oldObject, WitResponseClass jsonClass, StringBuilder log, JsonConverter[] customConverters)
	{
		if (jsonClass == null)
		{
			return oldObject;
		}
		object obj = oldObject;
		if (obj == null)
		{
			obj = Activator.CreateInstance(toType);
		}
		Dictionary<string, IJsonVariableInfo> varDictionary = GetVarDictionary(toType, log);
		string[] childNodeNames = jsonClass.ChildNodeNames;
		foreach (string text in childNodeNames)
		{
			if (!varDictionary.ContainsKey(text))
			{
				log.AppendLine("\t" + toType.FullName + " does not have a matching '" + text + "' field or property.");
				continue;
			}
			IJsonVariableInfo jsonVariableInfo = varDictionary[text];
			if (!jsonVariableInfo.GetShouldDeserialize())
			{
				log.AppendLine("\t" + toType.FullName + " cannot deserialize '" + text + "' to the matching " + ((jsonVariableInfo is JsonPropertyInfo) ? "property" : "field") + ".");
			}
			else
			{
				object oldValue = (jsonVariableInfo.GetShouldSerialize() ? jsonVariableInfo.GetValue(obj) : null);
				object newValue = DeserializeToken(jsonVariableInfo.GetVariableType(), oldValue, jsonClass[text], log, customConverters);
				jsonVariableInfo.SetValue(obj, newValue);
			}
		}
		if (toType.GetInterfaces().Contains(typeof(IJsonDeserializer)) && !(obj as IJsonDeserializer).DeserializeObject(jsonClass))
		{
			log.AppendLine($"\tIJsonDeserializer '{toType}' failed");
		}
		return obj;
	}

	private static object DeserializeDictionary(Type toType, object oldObject, WitResponseClass jsonClass, StringBuilder log, JsonConverter[] customConverters)
	{
		Type[] genericArguments = toType.GetGenericArguments();
		if (genericArguments == null || genericArguments.Length != 2)
		{
			return oldObject;
		}
		IDictionary dictionary = oldObject as IDictionary;
		Type conversionType = genericArguments[0];
		Type toType2 = genericArguments[1];
		string[] childNodeNames = jsonClass.ChildNodeNames;
		foreach (string text in childNodeNames)
		{
			object key = Convert.ChangeType(text, conversionType);
			object value = DeserializeToken(toType2, null, jsonClass[text], log, customConverters);
			dictionary[key] = value;
		}
		return dictionary;
	}

	public static string SerializeObject<TFromType>(TFromType inObject, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		WitResponseNode witResponseNode = SerializeToken(inObject, customConverters, suppressWarnings);
		if (witResponseNode != null)
		{
			try
			{
				return witResponseNode.ToString();
			}
			catch (Exception e)
			{
				VLog.E((object)"Serialize Object Failed", e);
			}
		}
		return "{}";
	}

	public static async Task<string> SerializeObjectAsync<TFromType>(TFromType inObject, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		string results = null;
		await Task.Run(() => results = SerializeObject(inObject, customConverters, suppressWarnings));
		return results;
	}

	public static WitResponseNode SerializeToken<TFromType>(TFromType inObject, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		if (inObject is WitResponseNode result)
		{
			return result;
		}
		if (customConverters == null)
		{
			customConverters = DefaultConverters;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			WitResponseNode result2 = SerializeToken(typeof(TFromType), inObject, stringBuilder, customConverters);
			if (stringBuilder.Length > 0 && !suppressWarnings)
			{
				VLog.W($"Serialize Token Warnings\n{stringBuilder}");
			}
			return result2;
		}
		catch (Exception e)
		{
			VLog.E((object)$"Serialize Token Failed for {inObject.GetType().Name}\n{inObject}", e);
		}
		return null;
	}

	public static async Task<WitResponseNode> SerializeTokenAsync<TFromType>(TFromType inObject, JsonConverter[] customConverters = null, bool suppressWarnings = false)
	{
		WitResponseNode results = null;
		await Task.Run(() => results = SerializeToken(inObject, customConverters, suppressWarnings));
		return results;
	}

	private static WitResponseNode SerializeToken(Type inType, object inObject, StringBuilder log, JsonConverter[] customConverters)
	{
		if (inObject != null && inType == typeof(object))
		{
			inType = inObject.GetType();
		}
		if (inObject is WitResponseNode result)
		{
			return result;
		}
		if (customConverters != null)
		{
			foreach (JsonConverter jsonConverter in customConverters)
			{
				if (jsonConverter.CanWrite && jsonConverter.CanConvert(inType))
				{
					return jsonConverter.WriteJson(inObject);
				}
			}
		}
		if (inObject == null)
		{
			return null;
		}
		if (inType == null)
		{
			throw new ArgumentException("In Type cannot be null");
		}
		if (inType == typeof(string))
		{
			return new WitResponseData((string)inObject);
		}
		if (inType == typeof(bool))
		{
			return new WitResponseData((bool)inObject);
		}
		if (inType == typeof(int))
		{
			return new WitResponseData((int)inObject);
		}
		if (inType == typeof(float))
		{
			return new WitResponseData((float)inObject);
		}
		if (inType == typeof(double))
		{
			return new WitResponseData((double)inObject);
		}
		if (inType == typeof(short))
		{
			return new WitResponseData((short)inObject);
		}
		if (inType == typeof(long))
		{
			return new WitResponseData((long)inObject);
		}
		if (inType.IsEnum)
		{
			return new WitResponseData(inObject.ToString());
		}
		if (inType.GetInterfaces().Contains(typeof(IDictionary)))
		{
			IDictionary dictionary = (IDictionary)inObject;
			WitResponseClass witResponseClass = new WitResponseClass();
			Type type = inType.GetGenericArguments()[1];
			{
				foreach (object key in dictionary.Keys)
				{
					object obj = dictionary[key];
					if (obj == null)
					{
						obj = ((!(type == typeof(string))) ? Activator.CreateInstance(type) : string.Empty);
					}
					witResponseClass.Add(key.ToString(), SerializeToken(type, obj, log, customConverters));
				}
				return witResponseClass;
			}
		}
		if (inType.GetInterfaces().Contains(typeof(IEnumerable)))
		{
			WitResponseArray witResponseArray = new WitResponseArray();
			IEnumerator enumerator2 = ((IEnumerable)inObject).GetEnumerator();
			Type type2 = inType.GetElementType();
			if (type2 == null)
			{
				Type[] genericArguments = inType.GetGenericArguments();
				if (genericArguments != null && genericArguments.Length != 0)
				{
					type2 = genericArguments[0];
				}
			}
			while (enumerator2.MoveNext())
			{
				object inObject2 = EnsureExists(type2, enumerator2.Current);
				witResponseArray.Add(string.Empty, SerializeToken(type2, inObject2, log, customConverters));
			}
			return witResponseArray;
		}
		if (inType.IsClass || (inType.IsValueType && !inType.IsPrimitive))
		{
			return SerializeClass(inType, inObject, log, customConverters);
		}
		log.AppendLine($"\tJson Serializer cannot serialize: {inType}");
		if (inObject != null)
		{
			return new WitResponseData(inObject.ToString());
		}
		return null;
	}

	private static WitResponseClass SerializeClass(Type inType, object inObject, StringBuilder log, JsonConverter[] customConverters)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		foreach (IJsonVariableInfo varInfo in GetVarInfos(inType))
		{
			if (!varInfo.GetShouldSerialize())
			{
				continue;
			}
			string[] serializeNames = varInfo.GetSerializeNames();
			foreach (string text in serializeNames)
			{
				try
				{
					object value = varInfo.GetValue(inObject);
					if (value != null)
					{
						witResponseClass.Add(text, SerializeToken(varInfo.GetVariableType(), value, log, customConverters));
					}
				}
				catch (Exception ex)
				{
					throw new ArgumentException("Cannot encode '" + inType.Name + "." + text + "': " + ex.Message, ex);
				}
			}
		}
		return witResponseClass;
	}

	private static List<IJsonVariableInfo> GetVarInfos(Type forType)
	{
		List<IJsonVariableInfo> list = new List<IJsonVariableInfo>();
		FieldInfo[] fields = forType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < fields.Length; i++)
		{
			JsonFieldInfo jsonFieldInfo = new JsonFieldInfo(fields[i]);
			if (jsonFieldInfo.GetShouldSerialize() || jsonFieldInfo.GetShouldDeserialize())
			{
				list.Add(jsonFieldInfo);
			}
		}
		PropertyInfo[] properties = forType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < properties.Length; i++)
		{
			JsonPropertyInfo jsonPropertyInfo = new JsonPropertyInfo(properties[i]);
			if (jsonPropertyInfo.GetShouldSerialize() || jsonPropertyInfo.GetShouldDeserialize())
			{
				list.Add(jsonPropertyInfo);
			}
		}
		return list;
	}

	private static Dictionary<string, IJsonVariableInfo> GetVarDictionary(Type forType, StringBuilder log)
	{
		Dictionary<string, IJsonVariableInfo> dictionary = new Dictionary<string, IJsonVariableInfo>();
		foreach (IJsonVariableInfo varInfo in GetVarInfos(forType))
		{
			string[] serializeNames = varInfo.GetSerializeNames();
			foreach (string text in serializeNames)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (dictionary.ContainsKey(text))
					{
						log.AppendLine("\t" + forType.FullName + " has two fields/properties with the same name '" + text + "' exposed to JsonConvert.");
					}
					else
					{
						dictionary[text] = varInfo;
					}
				}
			}
		}
		return dictionary;
	}
}
