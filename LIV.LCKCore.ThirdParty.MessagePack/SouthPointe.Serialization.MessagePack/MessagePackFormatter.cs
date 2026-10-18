using System;
using System.IO;

namespace SouthPointe.Serialization.MessagePack;

public class MessagePackFormatter
{
	public SerializationContext Context { get; set; }

	public MessagePackFormatter(SerializationContext context = null)
	{
		Context = context ?? SerializationContext.Default;
	}

	public T Deserialize<T>(byte[] bytes)
	{
		return (T)Deserialize(typeof(T), bytes);
	}

	public T Deserialize<T>(Stream stream)
	{
		return (T)Deserialize(typeof(T), stream);
	}

	public object Deserialize(Type type, byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		return Deserialize(type, new MemoryStream(bytes));
	}

	public object Deserialize(Type type, Stream stream)
	{
		try
		{
			FormatReader formatReader = new FormatReader(stream);
			return Context.TypeHandlers.Get(type).Read(formatReader.ReadFormat(), formatReader);
		}
		catch (FormatException ex)
		{
			if (stream.CanSeek)
			{
				MemoryStream memoryStream = new MemoryStream((int)stream.Position);
				byte[] array = new byte[16384];
				stream.Position = 0L;
				int num = memoryStream.Capacity;
				while (num > 0)
				{
					int num2 = stream.Read(array, 0, array.Length);
					memoryStream.Write(array, 0, num2);
					num -= num2;
				}
				memoryStream.Position = 0L;
				ex.Source = JsonConverter.Encode(memoryStream);
				memoryStream.Close();
			}
			throw;
		}
	}

	public byte[] Serialize<T>(T obj)
	{
		return Serialize(typeof(T), obj);
	}

	public byte[] Serialize(Type type, object obj)
	{
		MemoryStream memoryStream = new MemoryStream();
		Serialize(memoryStream, type, obj);
		return memoryStream.ToArray();
	}

	public void Serialize<T>(Stream stream, T obj)
	{
		Type type = ((obj != null) ? obj.GetType() : typeof(T));
		Serialize(stream, type, obj);
	}

	public void Serialize(Stream stream, Type type, object obj)
	{
		Context.TypeHandlers.Get(type).Write(obj, new FormatWriter(stream));
	}

	public string AsJson(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return null;
		}
		return AsJson(new MemoryStream(data));
	}

	public string AsJson(Stream stream)
	{
		return JsonConverter.Encode(stream, Context);
	}
}
