using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class KeyValuePairFormatter<TKey, TValue> : IYamlFormatter<KeyValuePair<TKey, TValue>>, IYamlFormatter
{
	public void Serialize(ref Utf8YamlEmitter emitter, KeyValuePair<TKey, TValue> value, YamlSerializationContext context)
	{
		emitter.BeginSequence();
		context.Serialize(ref emitter, value.Key);
		context.Serialize(ref emitter, value.Value);
		emitter.EndSequence();
	}

	public KeyValuePair<TKey, TValue> Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		if (parser.IsNullScalar())
		{
			return default(KeyValuePair<TKey, TValue>);
		}
		parser.ReadWithVerify(ParseEventType.SequenceStart);
		TKey key = context.DeserializeWithAlias<TKey>(ref parser);
		TValue value = context.DeserializeWithAlias<TValue>(ref parser);
		parser.ReadWithVerify(ParseEventType.SequenceEnd);
		return new KeyValuePair<TKey, TValue>(key, value);
	}
}
