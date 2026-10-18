using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class DictionaryFormatter<TKey, TValue> : IYamlFormatter<Dictionary<TKey, TValue>?>, IYamlFormatter where TKey : notnull
{
	public void Serialize(ref Utf8YamlEmitter emitter, Dictionary<TKey, TValue>? value, YamlSerializationContext context)
	{
		if (value == null)
		{
			emitter.WriteNull();
			return;
		}
		IYamlFormatter<TKey> formatterWithVerify = context.Resolver.GetFormatterWithVerify<TKey>();
		IYamlFormatter<TValue> formatterWithVerify2 = context.Resolver.GetFormatterWithVerify<TValue>();
		emitter.BeginMapping();
		foreach (KeyValuePair<TKey, TValue> item in value)
		{
			formatterWithVerify.Serialize(ref emitter, item.Key, context);
			formatterWithVerify2.Serialize(ref emitter, item.Value, context);
		}
		emitter.EndMapping();
	}

	public Dictionary<TKey, TValue>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		parser.ReadWithVerify(ParseEventType.MappingStart);
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
		IYamlFormatter<TKey> formatterWithVerify = context.Resolver.GetFormatterWithVerify<TKey>();
		IYamlFormatter<TValue> formatterWithVerify2 = context.Resolver.GetFormatterWithVerify<TValue>();
		while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
		{
			TKey key = context.DeserializeWithAlias(formatterWithVerify, ref parser);
			TValue value = context.DeserializeWithAlias(formatterWithVerify2, ref parser);
			dictionary.Add(key, value);
		}
		parser.ReadWithVerify(ParseEventType.MappingEnd);
		return dictionary;
	}
}
