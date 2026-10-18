using System;
using System.Collections;
using System.Collections.Generic;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization;

public class PrimitiveObjectFormatter : IYamlFormatter<object?>, IYamlFormatter
{
	public static readonly PrimitiveObjectFormatter Instance = new PrimitiveObjectFormatter();

	private static readonly Dictionary<Type, int> TypeToJumpCode = new Dictionary<Type, int>
	{
		{
			typeof(bool),
			0
		},
		{
			typeof(char),
			1
		},
		{
			typeof(sbyte),
			2
		},
		{
			typeof(byte),
			3
		},
		{
			typeof(short),
			4
		},
		{
			typeof(ushort),
			5
		},
		{
			typeof(int),
			6
		},
		{
			typeof(uint),
			7
		},
		{
			typeof(long),
			8
		},
		{
			typeof(ulong),
			9
		},
		{
			typeof(float),
			10
		},
		{
			typeof(double),
			11
		},
		{
			typeof(DateTime),
			12
		},
		{
			typeof(string),
			13
		},
		{
			typeof(byte[]),
			14
		}
	};

	public void Serialize(ref Utf8YamlEmitter emitter, object? value, YamlSerializationContext context)
	{
		if (value == null)
		{
			emitter.WriteNull();
			return;
		}
		Type type = value.GetType();
		if (TypeToJumpCode.TryGetValue(type, out var value2))
		{
			switch (value2)
			{
			case 0:
				emitter.WriteBool((bool)value);
				return;
			case 1:
				emitter.WriteInt32((char)value);
				return;
			case 2:
				emitter.WriteInt32((sbyte)value);
				return;
			case 3:
				emitter.WriteUInt32((byte)value);
				return;
			case 4:
				emitter.WriteInt32((short)value);
				return;
			case 5:
				emitter.WriteUInt32((ushort)value);
				return;
			case 6:
				emitter.WriteInt32((int)value);
				return;
			case 7:
				emitter.WriteUInt32((uint)value);
				return;
			case 8:
				emitter.WriteInt64((long)value);
				return;
			case 9:
				emitter.WriteUInt64((ulong)value);
				return;
			case 10:
				emitter.WriteFloat((float)value);
				return;
			case 11:
				emitter.WriteDouble((double)value);
				return;
			case 12:
				DateTimeFormatter.Instance.Serialize(ref emitter, (DateTime)value, context);
				return;
			case 13:
				emitter.WriteString((string)value);
				return;
			case 14:
				ByteArrayFormatter.Instance.Serialize(ref emitter, (byte[])value, context);
				return;
			}
		}
		if (type.IsEnum)
		{
			string stringValue = EnumAsStringNonGenericCache.Instance.GetStringValue(type, value);
			emitter.WriteString(stringValue, ScalarStyle.Plain);
			return;
		}
		if (value is IDictionary dictionary)
		{
			emitter.BeginMapping();
			foreach (DictionaryEntry item in dictionary)
			{
				Serialize(ref emitter, item.Key, context);
				Serialize(ref emitter, item.Value, context);
			}
			emitter.EndMapping();
			return;
		}
		if (value is ICollection collection)
		{
			emitter.BeginSequence();
			foreach (object item2 in collection)
			{
				Serialize(ref emitter, item2, context);
			}
			emitter.EndSequence();
			return;
		}
		throw new YamlSerializerException($"Not supported primitive object resolver. type: {type}");
	}

	public object? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
	{
		switch (parser.CurrentEventType)
		{
		case ParseEventType.Scalar:
		{
			if (parser.IsNullScalar())
			{
				parser.Read();
				return null;
			}
			if (parser.TryGetScalarAsBool(out var value2))
			{
				parser.Read();
				return value2;
			}
			if (parser.TryGetScalarAsInt32(out var value3))
			{
				parser.Read();
				return value3;
			}
			if (parser.TryGetScalarAsInt64(out var value4))
			{
				parser.Read();
				return value4;
			}
			if (parser.TryGetScalarAsDouble(out var value5))
			{
				parser.Read();
				return value5;
			}
			string? scalarAsString = parser.GetScalarAsString();
			parser.Read();
			return scalarAsString;
		}
		case ParseEventType.MappingStart:
		{
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			parser.Read();
			while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
			{
				object key = context.DeserializeWithAlias(this, ref parser);
				object value = context.DeserializeWithAlias(this, ref parser);
				dictionary.Add(key, value);
			}
			parser.ReadWithVerify(ParseEventType.MappingEnd);
			return dictionary;
		}
		case ParseEventType.SequenceStart:
		{
			List<object> list = new List<object>();
			parser.Read();
			while (!parser.End && parser.CurrentEventType != ParseEventType.SequenceEnd)
			{
				object item = context.DeserializeWithAlias(this, ref parser);
				list.Add(item);
			}
			parser.ReadWithVerify(ParseEventType.SequenceEnd);
			return list;
		}
		default:
			throw new InvalidOperationException();
		}
	}
}
