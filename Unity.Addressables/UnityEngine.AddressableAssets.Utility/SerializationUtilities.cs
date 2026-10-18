using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace UnityEngine.AddressableAssets.Utility;

internal static class SerializationUtilities
{
	internal enum ObjectType
	{
		AsciiString,
		UnicodeString,
		UInt16,
		UInt32,
		Int32,
		Hash128,
		Type,
		JsonObject
	}

	internal static int ReadInt32FromByteArray(byte[] data, int offset)
	{
		return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
	}

	internal static int WriteInt32ToByteArray(byte[] data, int val, int offset)
	{
		data[offset] = (byte)((uint)val & 0xFFu);
		data[offset + 1] = (byte)((uint)(val >> 8) & 0xFFu);
		data[offset + 2] = (byte)((uint)(val >> 16) & 0xFFu);
		data[offset + 3] = (byte)((uint)(val >> 24) & 0xFFu);
		return offset + 4;
	}

	internal static object ReadObjectFromByteArray(byte[] keyData, int dataIndex)
	{
		try
		{
			ObjectType objectType = (ObjectType)keyData[dataIndex];
			dataIndex++;
			switch (objectType)
			{
			case ObjectType.UnicodeString:
			{
				int count3 = BitConverter.ToInt32(keyData, dataIndex);
				return Encoding.Unicode.GetString(keyData, dataIndex + 4, count3);
			}
			case ObjectType.AsciiString:
			{
				int count2 = BitConverter.ToInt32(keyData, dataIndex);
				return Encoding.ASCII.GetString(keyData, dataIndex + 4, count2);
			}
			case ObjectType.UInt16:
				return BitConverter.ToUInt16(keyData, dataIndex);
			case ObjectType.UInt32:
				return BitConverter.ToUInt32(keyData, dataIndex);
			case ObjectType.Int32:
				return BitConverter.ToInt32(keyData, dataIndex);
			case ObjectType.Hash128:
				return Hash128.Parse(Encoding.ASCII.GetString(keyData, dataIndex + 1, keyData[dataIndex]));
			case ObjectType.Type:
				return Type.GetTypeFromCLSID(new Guid(Encoding.ASCII.GetString(keyData, dataIndex + 1, keyData[dataIndex])));
			case ObjectType.JsonObject:
			{
				int num = keyData[dataIndex];
				dataIndex++;
				string @string = Encoding.ASCII.GetString(keyData, dataIndex, num);
				dataIndex += num;
				int num2 = keyData[dataIndex];
				dataIndex++;
				string string2 = Encoding.ASCII.GetString(keyData, dataIndex, num2);
				dataIndex += num2;
				int count = BitConverter.ToInt32(keyData, dataIndex);
				dataIndex += 4;
				string string3 = Encoding.Unicode.GetString(keyData, dataIndex, count);
				Type type = Assembly.Load(@string).GetType(string2);
				return JsonUtility.FromJson(string3, type);
			}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return null;
	}

	internal static int WriteObjectToByteList(object obj, List<byte> buffer)
	{
		Type type = obj.GetType();
		if (type == typeof(string))
		{
			string text = obj as string;
			if (text == null)
			{
				text = string.Empty;
			}
			byte[] bytes = Encoding.Unicode.GetBytes(text);
			byte[] bytes2 = Encoding.ASCII.GetBytes(text);
			if (Encoding.Unicode.GetString(bytes) == Encoding.ASCII.GetString(bytes2))
			{
				buffer.Add(0);
				buffer.AddRange(BitConverter.GetBytes(bytes2.Length));
				buffer.AddRange(bytes2);
				return bytes2.Length + 5;
			}
			buffer.Add(1);
			buffer.AddRange(BitConverter.GetBytes(bytes.Length));
			buffer.AddRange(bytes);
			return bytes.Length + 5;
		}
		if (type == typeof(uint))
		{
			byte[] bytes3 = BitConverter.GetBytes((uint)obj);
			buffer.Add(3);
			buffer.AddRange(bytes3);
			return bytes3.Length + 1;
		}
		if (type == typeof(ushort))
		{
			byte[] bytes4 = BitConverter.GetBytes((ushort)obj);
			buffer.Add(2);
			buffer.AddRange(bytes4);
			return bytes4.Length + 1;
		}
		if (type == typeof(int))
		{
			byte[] bytes5 = BitConverter.GetBytes((int)obj);
			buffer.Add(4);
			buffer.AddRange(bytes5);
			return bytes5.Length + 1;
		}
		if (type == typeof(Hash128))
		{
			Hash128 hash = (Hash128)obj;
			byte[] bytes6 = Encoding.ASCII.GetBytes(hash.ToString());
			buffer.Add(5);
			buffer.Add((byte)bytes6.Length);
			buffer.AddRange(bytes6);
			return bytes6.Length + 2;
		}
		if (type == typeof(Type))
		{
			byte[] array = type.GUID.ToByteArray();
			buffer.Add(6);
			buffer.Add((byte)array.Length);
			buffer.AddRange(array);
			return array.Length + 2;
		}
		if (type.GetCustomAttributes(typeof(SerializableAttribute), inherit: true).Length == 0)
		{
			return 0;
		}
		buffer.Add(7);
		int num = 0 + 1;
		byte[] bytes7 = Encoding.ASCII.GetBytes(type.Assembly.FullName);
		buffer.Add((byte)bytes7.Length);
		int num2 = num + 1;
		buffer.AddRange(bytes7);
		int num3 = num2 + bytes7.Length;
		string text2 = type.FullName;
		if (text2 == null)
		{
			text2 = string.Empty;
		}
		byte[] bytes8 = Encoding.ASCII.GetBytes(text2);
		buffer.Add((byte)bytes8.Length);
		int num4 = num3 + 1;
		buffer.AddRange(bytes8);
		int num5 = num4 + bytes8.Length;
		byte[] bytes9 = Encoding.Unicode.GetBytes(JsonUtility.ToJson(obj));
		buffer.AddRange(BitConverter.GetBytes(bytes9.Length));
		int num6 = num5 + 4;
		buffer.AddRange(bytes9);
		return num6 + bytes9.Length;
	}
}
