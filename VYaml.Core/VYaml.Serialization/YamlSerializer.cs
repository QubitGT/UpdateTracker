using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VYaml.Emitter;
using VYaml.Internal;
using VYaml.Parser;

namespace VYaml.Serialization;

public static class YamlSerializer
{
	[ThreadStatic]
	private static YamlDeserializationContext? deserializationContext;

	[ThreadStatic]
	private static YamlSerializationContext? serializationContext;

	private static YamlSerializerOptions? defaultOptions;

	public static YamlSerializerOptions DefaultOptions
	{
		get
		{
			return defaultOptions ?? (defaultOptions = YamlSerializerOptions.Standard);
		}
		set
		{
			defaultOptions = value;
		}
	}

	private static YamlDeserializationContext GetThreadLocalDeserializationContext(YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlDeserializationContext? obj = deserializationContext ?? (deserializationContext = new YamlDeserializationContext(options));
		obj.Resolver = options.Resolver;
		return obj;
	}

	private static YamlSerializationContext GetThreadLocalSerializationContext(YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlSerializationContext? obj = serializationContext ?? (serializationContext = new YamlSerializationContext(options));
		obj.Resolver = options.Resolver;
		obj.EmitOptions = options.EmitOptions;
		return obj;
	}

	public static ReadOnlyMemory<byte> Serialize<T>(T value, YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlSerializationContext threadLocalSerializationContext = GetThreadLocalSerializationContext(options);
		ArrayBufferWriter<byte> arrayBufferWriter = threadLocalSerializationContext.GetArrayBufferWriter();
		Utf8YamlEmitter emitter = new Utf8YamlEmitter(arrayBufferWriter);
		threadLocalSerializationContext.Reset();
		threadLocalSerializationContext.Resolver.GetFormatterWithVerify<T>().Serialize(ref emitter, value, threadLocalSerializationContext);
		return arrayBufferWriter.WrittenMemory;
	}

	public static void Serialize<T>(IBufferWriter<byte> writer, T value, YamlSerializerOptions? options = null)
	{
		Utf8YamlEmitter emitter = new Utf8YamlEmitter(writer);
		Serialize(ref emitter, value, options);
	}

	public static void Serialize<T>(ref Utf8YamlEmitter emitter, T value, YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlSerializationContext threadLocalSerializationContext = GetThreadLocalSerializationContext(options);
		threadLocalSerializationContext.Reset();
		threadLocalSerializationContext.Resolver.GetFormatterWithVerify<T>().Serialize(ref emitter, value, threadLocalSerializationContext);
	}

	public static string SerializeToString<T>(T value, YamlSerializerOptions? options = null)
	{
		ReadOnlyMemory<byte> readOnlyMemory = Serialize(value, options);
		return StringEncoding.Utf8.GetString(readOnlyMemory.Span);
	}

	public static T Deserialize<T>(ReadOnlyMemory<byte> memory, YamlSerializerOptions? options = null)
	{
		ReadOnlySequence<byte> sequence = new ReadOnlySequence<byte>(memory);
		YamlParser parser = YamlParser.FromSequence(in sequence);
		return Deserialize<T>(ref parser, options);
	}

	public static T Deserialize<T>(in ReadOnlySequence<byte> sequence, YamlSerializerOptions? options = null)
	{
		YamlParser parser = YamlParser.FromSequence(in sequence);
		return Deserialize<T>(ref parser, options);
	}

	public static async ValueTask<T> DeserializeAsync<T>(Stream stream, YamlSerializerOptions? options = null)
	{
		ReusableByteSequenceBuilder reusableByteSequenceBuilder = await StreamHelper.ReadAsSequenceAsync(stream);
		try
		{
			ReadOnlySequence<byte> sequence = reusableByteSequenceBuilder.Build();
			return Deserialize<T>(in sequence, options);
		}
		finally
		{
			ReusableByteSequenceBuilderPool.Return(reusableByteSequenceBuilder);
		}
	}

	public static T Deserialize<T>(ref YamlParser parser, YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlDeserializationContext threadLocalDeserializationContext = GetThreadLocalDeserializationContext(options);
		threadLocalDeserializationContext.Reset();
		parser.SkipAfter(ParseEventType.DocumentStart);
		IYamlFormatter<T> formatterWithVerify = options.Resolver.GetFormatterWithVerify<T>();
		return threadLocalDeserializationContext.DeserializeWithAlias(formatterWithVerify, ref parser);
	}

	public static async ValueTask<IEnumerable<T>> DeserializeMultipleDocumentsAsync<T>(Stream stream, YamlSerializerOptions? options = null)
	{
		ReusableByteSequenceBuilder reusableByteSequenceBuilder = await StreamHelper.ReadAsSequenceAsync(stream);
		try
		{
			ReadOnlySequence<byte> sequence = reusableByteSequenceBuilder.Build();
			return DeserializeMultipleDocuments<T>(in sequence, options);
		}
		finally
		{
			ReusableByteSequenceBuilderPool.Return(reusableByteSequenceBuilder);
		}
	}

	public static IEnumerable<T> DeserializeMultipleDocuments<T>(ReadOnlyMemory<byte> memory, YamlSerializerOptions? options = null)
	{
		ReadOnlySequence<byte> sequence = new ReadOnlySequence<byte>(memory);
		YamlParser parser = YamlParser.FromSequence(in sequence);
		return DeserializeMultipleDocuments<T>(ref parser, options);
	}

	public static IEnumerable<T> DeserializeMultipleDocuments<T>(in ReadOnlySequence<byte> sequence, YamlSerializerOptions? options = null)
	{
		YamlParser parser = YamlParser.FromSequence(in sequence);
		return DeserializeMultipleDocuments<T>(ref parser, options);
	}

	public static IEnumerable<T> DeserializeMultipleDocuments<T>(ref YamlParser parser, YamlSerializerOptions? options = null)
	{
		if (options == null)
		{
			options = DefaultOptions;
		}
		YamlDeserializationContext threadLocalDeserializationContext = GetThreadLocalDeserializationContext(options);
		IYamlFormatter<T> formatterWithVerify = options.Resolver.GetFormatterWithVerify<T>();
		List<T> list = new List<T>();
		while (true)
		{
			parser.SkipAfter(ParseEventType.DocumentStart);
			if (parser.End)
			{
				break;
			}
			threadLocalDeserializationContext.Reset();
			T item = threadLocalDeserializationContext.DeserializeWithAlias(formatterWithVerify, ref parser);
			list.Add(item);
		}
		return list;
	}
}
