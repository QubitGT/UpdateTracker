using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization;

public class EnumAsStringFormatter<T> : IYamlFormatter<T>, IYamlFormatter where T : Enum
{
	private static readonly Dictionary<string, T> NameValueMapping;

	private static readonly Dictionary<T, string> ValueNameMapping;

	static EnumAsStringFormatter()
	{
		List<string> list = new List<string>();
		List<object> list2 = new List<object>();
		Type type = typeof(T);
		NamingConvention namingConvention = type.GetCustomAttribute<YamlObjectAttribute>()?.NamingConvention ?? NamingConvention.LowerCamelCase;
		foreach (FieldInfo item in from x in type.GetFields()
			where x.FieldType == type
			select x)
		{
			object value = item.GetValue(null);
			list2.Add(value);
			object[] customAttributes = item.GetCustomAttributes(inherit: true);
			EnumMemberAttribute enumMemberAttribute = customAttributes.OfType<EnumMemberAttribute>().FirstOrDefault();
			if (enumMemberAttribute != null)
			{
				string value2 = enumMemberAttribute.Value;
				if (value2 != null)
				{
					list.Add(value2);
					continue;
				}
			}
			DataMemberAttribute dataMemberAttribute = customAttributes.OfType<DataMemberAttribute>().FirstOrDefault();
			if (dataMemberAttribute != null)
			{
				string name = dataMemberAttribute.Name;
				if (name != null)
				{
					list.Add(name);
					continue;
				}
			}
			string name2 = Enum.GetName(type, value);
			list.Add(KeyNameMutator.Mutate(name2, namingConvention));
		}
		NameValueMapping = new Dictionary<string, T>(list.Count);
		ValueNameMapping = new Dictionary<T, string>(list.Count);
		foreach (var (obj, text) in list2.Zip(list, (object v, string n) => (v: v, n: n)))
		{
			NameValueMapping[text] = (T)obj;
			ValueNameMapping[(T)obj] = text;
		}
	}

	public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
	{
		if (ValueNameMapping.TryGetValue(value, out string value2))
		{
			emitter.WriteString(value2, ScalarStyle.Plain);
		}
		else
		{
			YamlSerializerException.ThrowInvalidType(value);
		}
	}

	public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		string text = parser.ReadScalarAsString();
		T value;
		if (text == null)
		{
			YamlSerializerException.ThrowInvalidType<T>();
		}
		else if (NameValueMapping.TryGetValue(text, out value))
		{
			return value;
		}
		YamlSerializerException.ThrowInvalidType<T>();
		return default(T);
	}
}
