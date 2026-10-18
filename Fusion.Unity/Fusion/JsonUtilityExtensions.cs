using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Fusion;

public static class JsonUtilityExtensions
{
	public delegate Type TypeResolverDelegate(string typeName);

	public delegate string TypeSerializerDelegate(Type type);

	public delegate string InstanceIDHandlerDelegate(object context, int value);

	[Serializable]
	private class TypeNameWrapper
	{
		public string __TypeName;
	}

	private const string TypePropertyName = "$type";

	public static string EnquoteIntegers(string json, int minDigits = 8)
	{
		return Regex.Replace(json, $"(?<=\":\\s*)(-?[0-9]{{{minDigits},}})(?=[,}}\\n\\r\\s])", "\"$1\"", RegexOptions.Compiled);
	}

	public static string ToJsonWithTypeAnnotation(object obj, InstanceIDHandlerDelegate instanceIDHandler = null)
	{
		StringBuilder stringBuilder = new StringBuilder(1000);
		using (StringWriter writer = new StringWriter(stringBuilder))
		{
			ToJsonWithTypeAnnotation(obj, writer, null, null, instanceIDHandler);
		}
		return stringBuilder.ToString();
	}

	public static void ToJsonWithTypeAnnotation(object obj, TextWriter writer, int? integerEnquoteMinDigits = null, TypeSerializerDelegate typeSerializer = null, InstanceIDHandlerDelegate instanceIDHandler = null)
	{
		if (obj == null)
		{
			writer.Write("null");
		}
		else if (obj is IList list)
		{
			writer.Write("[");
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					writer.Write(",");
				}
				ToJsonInternal(list[i], writer, integerEnquoteMinDigits, typeSerializer, instanceIDHandler);
			}
			writer.Write("]");
		}
		else
		{
			ToJsonInternal(obj, writer, integerEnquoteMinDigits, typeSerializer, instanceIDHandler);
		}
	}

	public static T FromJsonWithTypeAnnotation<T>(string json, TypeResolverDelegate typeResolver = null)
	{
		if (typeof(T).IsArray)
		{
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(typeof(T).GetElementType()));
			FromJsonWithTypeAnnotationInternal(json, typeResolver, list);
			Array array = Array.CreateInstance(typeof(T).GetElementType(), list.Count);
			list.CopyTo(array, 0);
			return (T)(object)array;
		}
		if (typeof(T).GetInterface(typeof(IList).FullName) != null)
		{
			IList list2 = (IList)Activator.CreateInstance(typeof(T));
			FromJsonWithTypeAnnotationInternal(json, typeResolver, list2);
			return (T)list2;
		}
		return (T)FromJsonWithTypeAnnotationInternal(json, typeResolver);
	}

	public static object FromJsonWithTypeAnnotation(string json, TypeResolverDelegate typeResolver = null)
	{
		int i2 = SkipWhiteOrThrow(0);
		if (json[i2] == '[')
		{
			List<object> list = new List<object>();
			i2++;
			bool flag = false;
			while (true)
			{
				i2 = SkipWhiteOrThrow(i2);
				if (json[i2] == ']')
				{
					break;
				}
				if (flag)
				{
					if (json[i2] != ',')
					{
						throw new InvalidOperationException($"Malformed at {i2}: expected ,");
					}
					i2 = SkipWhiteOrThrow(i2 + 1);
				}
				object item = FromJsonWithTypeAnnotationToObject(ref i2, json, typeResolver);
				list.Add(item);
				flag = true;
			}
			return list.ToArray();
		}
		return FromJsonWithTypeAnnotationToObject(ref i2, json, typeResolver);
		int SkipWhiteOrThrow(int i)
		{
			while (i < json.Length && char.IsWhiteSpace(json[i]))
			{
				i++;
			}
			if (i == json.Length)
			{
				throw new InvalidOperationException($"Malformed at {i}: expected more");
			}
			return i;
		}
	}

	private static object FromJsonWithTypeAnnotationInternal(string json, TypeResolverDelegate typeResolver = null, IList targetList = null)
	{
		int i2 = SkipWhiteOrThrow(0);
		if (json[i2] == '[')
		{
			IList list = targetList ?? new List<object>();
			i2++;
			bool flag = false;
			while (true)
			{
				i2 = SkipWhiteOrThrow(i2);
				if (json[i2] == ']')
				{
					break;
				}
				if (flag)
				{
					if (json[i2] != ',')
					{
						throw new InvalidOperationException($"Malformed at {i2}: expected ,");
					}
					i2 = SkipWhiteOrThrow(i2 + 1);
				}
				object value = FromJsonWithTypeAnnotationToObject(ref i2, json, typeResolver);
				list.Add(value);
				flag = true;
			}
			return targetList ?? ((List<object>)list).ToArray();
		}
		if (targetList != null)
		{
			throw new InvalidOperationException($"Expected list, got {json[i2]}");
		}
		return FromJsonWithTypeAnnotationToObject(ref i2, json, typeResolver);
		int SkipWhiteOrThrow(int i)
		{
			while (i < json.Length && char.IsWhiteSpace(json[i]))
			{
				i++;
			}
			if (i == json.Length)
			{
				throw new InvalidOperationException($"Malformed at {i}: expected more");
			}
			return i;
		}
	}

	private static void ToJsonInternal(object obj, TextWriter writer, int? integerEnquoteMinDigits = null, TypeSerializerDelegate typeResolver = null, InstanceIDHandlerDelegate instanceIDHandler = null)
	{
		string text = JsonUtility.ToJson(obj);
		if (integerEnquoteMinDigits.HasValue)
		{
			text = EnquoteIntegers(text, integerEnquoteMinDigits.Value);
		}
		Type type = obj.GetType();
		writer.Write("{\"");
		writer.Write("$type");
		writer.Write("\":\"");
		writer.Write(typeResolver?.Invoke(type) ?? SerializableType.GetShortAssemblyQualifiedName(type));
		writer.Write('"');
		if (text == "{}")
		{
			writer.Write("}");
			return;
		}
		writer.Write(',');
		if (instanceIDHandler != null)
		{
			int num = 1;
			while (true)
			{
				int num2 = text.IndexOf("{\"instanceID\":", num, StringComparison.Ordinal);
				if (num2 < 0)
				{
					break;
				}
				int num3 = num2 + "{\"instanceID\":".Length;
				int num4 = text.IndexOf('}', num3);
				int value = int.Parse(text.AsSpan(num3, num4 - num3));
				writer.Write(text.AsSpan(num, num2 - num));
				writer.Write(instanceIDHandler(obj, value));
				num = num4 + 1;
			}
			writer.Write(text.AsSpan(num, text.Length - num));
		}
		else
		{
			writer.Write(text.AsSpan(1, text.Length - 1));
		}
	}

	private static object FromJsonWithTypeAnnotationToObject(ref int i, string json, TypeResolverDelegate typeResolver)
	{
		if (json[i] == '{')
		{
			int num = FindScopeEnd(json, i);
			if (num < 0)
			{
				throw new InvalidOperationException($"Unable to find end of object's end (starting at {i})");
			}
			string text = json.Substring(i, num - i + 1);
			i = num + 1;
			TypeNameWrapper typeNameWrapper = JsonUtility.FromJson<TypeNameWrapper>(text.Replace("$type", "__TypeName", StringComparison.Ordinal));
			Type type;
			if (typeResolver != null)
			{
				type = typeResolver(typeNameWrapper.__TypeName);
				if (type == null)
				{
					return null;
				}
			}
			else
			{
				type = Type.GetType(typeNameWrapper.__TypeName, throwOnError: true);
			}
			if (type.IsSubclassOf(typeof(ScriptableObject)))
			{
				ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);
				JsonUtility.FromJsonOverwrite(text, scriptableObject);
				return scriptableObject;
			}
			return JsonUtility.FromJson(text, type);
		}
		if (i + 4 < json.Length && json.AsSpan(i, 4).SequenceEqual("null"))
		{
			i += 4;
			return null;
		}
		throw new InvalidOperationException($"Malformed at {i}: expected {{ or null");
	}

	internal static int FindObjectEnd(string json, int start = 0)
	{
		return FindScopeEnd(json, start);
	}

	private static int FindScopeEnd(string json, int start, char cstart = '{', char cend = '}')
	{
		int num = 0;
		if (json[start] != cstart)
		{
			return -1;
		}
		for (int i = start; i < json.Length; i++)
		{
			if (json[i] == '"')
			{
				while (i < json.Length && (json[++i] != '"' || json[i - 1] == '\\'))
				{
				}
			}
			else if (json[i] == cstart)
			{
				num++;
			}
			else if (json[i] == cend)
			{
				num--;
				if (num == 0)
				{
					return i;
				}
			}
		}
		return -1;
	}
}
