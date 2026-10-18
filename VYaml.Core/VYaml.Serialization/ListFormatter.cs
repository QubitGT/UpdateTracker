using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class ListFormatter<T> : IYamlFormatter<List<T>?>, IYamlFormatter
{
	public void Serialize(ref Utf8YamlEmitter emitter, List<T>? value, YamlSerializationContext context)
	{
		if (value == null)
		{
			emitter.WriteNull();
			return;
		}
		emitter.BeginSequence();
		if (value.Count > 0)
		{
			IYamlFormatter<T> formatterWithVerify = context.Resolver.GetFormatterWithVerify<T>();
			foreach (T item in value)
			{
				formatterWithVerify.Serialize(ref emitter, item, context);
			}
		}
		emitter.EndSequence();
	}

	public List<T>? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
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
		return list;
	}
}
