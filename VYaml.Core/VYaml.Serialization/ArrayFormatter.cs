using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class ArrayFormatter<T> : IYamlFormatter<T[]?>, IYamlFormatter
{
	public void Serialize(ref Utf8YamlEmitter emitter, T[]? value, YamlSerializationContext context)
	{
		if (value == null)
		{
			emitter.WriteNull();
			return;
		}
		IYamlFormatter<T> formatterWithVerify = context.Resolver.GetFormatterWithVerify<T>();
		emitter.BeginSequence();
		foreach (T value2 in value)
		{
			formatterWithVerify.Serialize(ref emitter, value2, context);
		}
		emitter.EndSequence();
	}

	public T[]? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			parser.Read();
			return null;
		}
		parser.ReadWithVerify(ParseEventType.SequenceStart);
		List<T> list = new List<T>();
		IYamlFormatter<T> formatterWithVerify = context.Resolver.GetFormatterWithVerify<T>();
		while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
		{
			T item = context.DeserializeWithAlias(formatterWithVerify, ref parser);
			list.Add(item);
		}
		parser.ReadWithVerify(ParseEventType.SequenceEnd);
		return list.ToArray();
	}
}
