#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;

namespace ExitGames.Client.Photon;

public class Protocol18 : IProtocol
{
	public enum GpType : byte
	{
		Unknown = 0,
		Boolean = 2,
		Byte = 3,
		Short = 4,
		Float = 5,
		Double = 6,
		String = 7,
		Null = 8,
		CompressedInt = 9,
		CompressedLong = 10,
		Int1 = 11,
		Int1_ = 12,
		Int2 = 13,
		Int2_ = 14,
		L1 = 15,
		L1_ = 16,
		L2 = 17,
		L2_ = 18,
		Custom = 19,
		CustomTypeSlim = 128,
		Dictionary = 20,
		Hashtable = 21,
		ObjectArray = 23,
		OperationRequest = 24,
		OperationResponse = 25,
		EventData = 26,
		BooleanFalse = 27,
		BooleanTrue = 28,
		ShortZero = 29,
		IntZero = 30,
		LongZero = 31,
		FloatZero = 32,
		DoubleZero = 33,
		ByteZero = 34,
		Array = 64,
		BooleanArray = 66,
		ByteArray = 67,
		ShortArray = 68,
		DoubleArray = 70,
		FloatArray = 69,
		StringArray = 71,
		HashtableArray = 85,
		DictionaryArray = 84,
		CustomTypeArray = 83,
		CompressedIntArray = 73,
		CompressedLongArray = 74
	}

	private readonly byte[] versionBytes = new byte[2] { 1, 8 };

	private static readonly byte[] boolMasks = new byte[8] { 1, 2, 4, 8, 16, 32, 64, 128 };

	private readonly double[] memDoubleBlock = new double[1];

	private readonly float[] memFloatBlock = new float[1];

	private readonly byte[] memCustomTypeBodyLengthSerialized = new byte[5];

	private readonly byte[] memCompressedUInt32 = new byte[5];

	private byte[] memCompressedUInt64 = new byte[10];

	public override string ProtocolType => "GpBinaryV18";

	public override byte[] VersionBytes => versionBytes;

	public override void Serialize(StreamBuffer dout, object serObject, bool setType)
	{
		Write(dout, serObject, setType);
	}

	public override void SerializeShort(StreamBuffer dout, short serObject, bool setType)
	{
		WriteInt16(dout, serObject, setType);
	}

	public override void SerializeString(StreamBuffer dout, string serObject, bool setType)
	{
		WriteString(dout, serObject, setType);
	}

	public override object Deserialize(StreamBuffer din, byte type, DeserializationFlags flags = DeserializationFlags.None)
	{
		return Read(din, type);
	}

	public override short DeserializeShort(StreamBuffer din)
	{
		return ReadInt16(din);
	}

	public override byte DeserializeByte(StreamBuffer din)
	{
		return ReadByte(din);
	}

	private static Type GetAllowedDictionaryKeyTypes(GpType gpType)
	{
		switch (gpType)
		{
		case GpType.Byte:
		case GpType.ByteZero:
			return typeof(byte);
		case GpType.Short:
		case GpType.ShortZero:
			return typeof(short);
		case GpType.Float:
		case GpType.FloatZero:
			return typeof(float);
		case GpType.Double:
		case GpType.DoubleZero:
			return typeof(double);
		case GpType.String:
			return typeof(string);
		case GpType.CompressedInt:
		case GpType.Int1:
		case GpType.Int1_:
		case GpType.Int2:
		case GpType.Int2_:
		case GpType.IntZero:
			return typeof(int);
		case GpType.CompressedLong:
		case GpType.L1:
		case GpType.L1_:
		case GpType.L2:
		case GpType.L2_:
		case GpType.LongZero:
			return typeof(long);
		default:
			throw new Exception($"{gpType} is not a valid Type as Dictionary key.");
		}
	}

	private static Type GetClrArrayType(GpType gpType)
	{
		switch (gpType)
		{
		case GpType.Boolean:
		case GpType.BooleanFalse:
		case GpType.BooleanTrue:
			return typeof(bool);
		case GpType.Byte:
		case GpType.ByteZero:
			return typeof(byte);
		case GpType.Short:
		case GpType.ShortZero:
			return typeof(short);
		case GpType.Float:
		case GpType.FloatZero:
			return typeof(float);
		case GpType.Double:
		case GpType.DoubleZero:
			return typeof(double);
		case GpType.String:
			return typeof(string);
		case GpType.CompressedInt:
		case GpType.Int1:
		case GpType.Int1_:
		case GpType.Int2:
		case GpType.Int2_:
		case GpType.IntZero:
			return typeof(int);
		case GpType.CompressedLong:
		case GpType.L1:
		case GpType.L1_:
		case GpType.L2:
		case GpType.L2_:
		case GpType.LongZero:
			return typeof(long);
		case GpType.Hashtable:
			return typeof(Hashtable);
		case GpType.OperationRequest:
			return typeof(OperationRequest);
		case GpType.OperationResponse:
			return typeof(OperationResponse);
		case GpType.EventData:
			return typeof(EventData);
		case GpType.BooleanArray:
			return typeof(bool[]);
		case GpType.ByteArray:
			return typeof(byte[]);
		case GpType.ShortArray:
			return typeof(short[]);
		case GpType.DoubleArray:
			return typeof(double[]);
		case GpType.FloatArray:
			return typeof(float[]);
		case GpType.StringArray:
			return typeof(string[]);
		case GpType.HashtableArray:
			return typeof(Hashtable[]);
		case GpType.CompressedIntArray:
			return typeof(int[]);
		case GpType.CompressedLongArray:
			return typeof(long[]);
		default:
			return null;
		}
	}

	private GpType GetCodeOfType(Type type)
	{
		if (type == null)
		{
			return GpType.Null;
		}
		if (type == typeof(StructWrapper<>))
		{
			return GpType.Unknown;
		}
		if (type.IsPrimitive || type.IsEnum)
		{
			TypeCode typeCode = Type.GetTypeCode(type);
			return GetCodeOfTypeCode(typeCode);
		}
		if (type == typeof(string))
		{
			return GpType.String;
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			if (elementType == null)
			{
				throw new InvalidDataException($"Arrays of type {type} are not supported");
			}
			if (elementType.IsPrimitive)
			{
				switch (Type.GetTypeCode(elementType))
				{
				case TypeCode.Byte:
					return GpType.ByteArray;
				case TypeCode.Int16:
					return GpType.ShortArray;
				case TypeCode.Int32:
					return GpType.CompressedIntArray;
				case TypeCode.Int64:
					return GpType.CompressedLongArray;
				case TypeCode.Boolean:
					return GpType.BooleanArray;
				case TypeCode.Single:
					return GpType.FloatArray;
				case TypeCode.Double:
					return GpType.DoubleArray;
				}
			}
			if (elementType.IsArray)
			{
				return GpType.Array;
			}
			if (elementType == typeof(string))
			{
				return GpType.StringArray;
			}
			if (elementType == typeof(object) || elementType == typeof(StructWrapper))
			{
				return GpType.ObjectArray;
			}
			if (elementType == typeof(Hashtable))
			{
				return GpType.HashtableArray;
			}
			if (elementType.IsGenericType && typeof(Dictionary<, >) == elementType.GetGenericTypeDefinition())
			{
				return GpType.DictionaryArray;
			}
			return GpType.CustomTypeArray;
		}
		if (type == typeof(Hashtable))
		{
			return GpType.Hashtable;
		}
		if (type == typeof(List<object>))
		{
			return GpType.ObjectArray;
		}
		if (type.IsGenericType && typeof(Dictionary<, >) == type.GetGenericTypeDefinition())
		{
			return GpType.Dictionary;
		}
		if (type == typeof(EventData))
		{
			return GpType.EventData;
		}
		if (type == typeof(OperationRequest))
		{
			return GpType.OperationRequest;
		}
		if (type == typeof(OperationResponse))
		{
			return GpType.OperationResponse;
		}
		return GpType.Unknown;
	}

	private GpType GetCodeOfTypeCode(TypeCode type)
	{
		return type switch
		{
			TypeCode.Byte => GpType.Byte, 
			TypeCode.String => GpType.String, 
			TypeCode.Boolean => GpType.Boolean, 
			TypeCode.Int16 => GpType.Short, 
			TypeCode.Int32 => GpType.CompressedInt, 
			TypeCode.Int64 => GpType.CompressedLong, 
			TypeCode.Single => GpType.Float, 
			TypeCode.Double => GpType.Double, 
			_ => GpType.Unknown, 
		};
	}

	private object Read(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		return Read(stream, ReadByte(stream), flags, parameters);
	}

	private object Read(StreamBuffer stream, byte gpType, DeserializationFlags flags = DeserializationFlags.None, ParameterDictionary parameters = null)
	{
		int num = ((gpType >= 128) ? (gpType - 128) : gpType);
		num = ((num >= 64) ? (num - 64) : num);
		bool flag = (flags & DeserializationFlags.WrapIncomingStructs) == DeserializationFlags.WrapIncomingStructs;
		if (gpType >= 128 && gpType <= 228)
		{
			return ReadCustomType(stream, gpType);
		}
		switch ((GpType)gpType)
		{
		case GpType.Boolean:
		{
			bool flag3 = ReadBoolean(stream);
			return flag ? parameters.wrapperPools.Acquire(flag3) : ((object)flag3);
		}
		case GpType.BooleanTrue:
		{
			bool flag4 = true;
			return flag ? parameters.wrapperPools.Acquire(flag4) : ((object)flag4);
		}
		case GpType.BooleanFalse:
		{
			bool flag2 = false;
			return flag ? parameters.wrapperPools.Acquire(flag2) : ((object)flag2);
		}
		case GpType.Byte:
		{
			byte b = ReadByte(stream);
			return flag ? parameters.wrapperPools.Acquire(b) : ((object)b);
		}
		case GpType.ByteZero:
		{
			byte b2 = 0;
			return flag ? parameters.wrapperPools.Acquire(b2) : ((object)b2);
		}
		case GpType.Short:
		{
			short num3 = ReadInt16(stream);
			return flag ? parameters.wrapperPools.Acquire(num3) : ((object)num3);
		}
		case GpType.ShortZero:
		{
			short num19 = 0;
			return flag ? parameters.wrapperPools.Acquire(num19) : ((object)num19);
		}
		case GpType.Float:
		{
			float num12 = ReadSingle(stream);
			return flag ? parameters.wrapperPools.Acquire(num12) : ((object)num12);
		}
		case GpType.FloatZero:
		{
			float num10 = 0f;
			return flag ? parameters.wrapperPools.Acquire(num10) : ((object)num10);
		}
		case GpType.Double:
		{
			double num15 = ReadDouble(stream);
			return flag ? parameters.wrapperPools.Acquire(num15) : ((object)num15);
		}
		case GpType.DoubleZero:
		{
			double num13 = 0.0;
			return flag ? parameters.wrapperPools.Acquire(num13) : ((object)num13);
		}
		case GpType.String:
			return ReadString(stream);
		case GpType.Int1:
		{
			int num4 = ReadInt1(stream, signNegative: false);
			return flag ? parameters.wrapperPools.Acquire(num4) : ((object)num4);
		}
		case GpType.Int2:
		{
			int num18 = ReadInt2(stream, signNegative: false);
			return flag ? parameters.wrapperPools.Acquire(num18) : ((object)num18);
		}
		case GpType.Int1_:
		{
			int num16 = ReadInt1(stream, signNegative: true);
			return flag ? parameters.wrapperPools.Acquire(num16) : ((object)num16);
		}
		case GpType.Int2_:
		{
			int num9 = ReadInt2(stream, signNegative: true);
			return flag ? parameters.wrapperPools.Acquire(num9) : ((object)num9);
		}
		case GpType.CompressedInt:
		{
			int num7 = ReadCompressedInt32(stream);
			return flag ? parameters.wrapperPools.Acquire(num7) : ((object)num7);
		}
		case GpType.IntZero:
		{
			int num5 = 0;
			return flag ? parameters.wrapperPools.Acquire(num5) : ((object)num5);
		}
		case GpType.L1:
		{
			long num17 = ReadInt1(stream, signNegative: false);
			return flag ? parameters.wrapperPools.Acquire(num17) : ((object)num17);
		}
		case GpType.L2:
		{
			long num14 = ReadInt2(stream, signNegative: false);
			return flag ? parameters.wrapperPools.Acquire(num14) : ((object)num14);
		}
		case GpType.L1_:
		{
			long num11 = ReadInt1(stream, signNegative: true);
			return flag ? parameters.wrapperPools.Acquire(num11) : ((object)num11);
		}
		case GpType.L2_:
		{
			long num8 = ReadInt2(stream, signNegative: true);
			return flag ? parameters.wrapperPools.Acquire(num8) : ((object)num8);
		}
		case GpType.CompressedLong:
		{
			long num6 = ReadCompressedInt64(stream);
			return flag ? parameters.wrapperPools.Acquire(num6) : ((object)num6);
		}
		case GpType.LongZero:
		{
			long num2 = 0L;
			return flag ? parameters.wrapperPools.Acquire(num2) : ((object)num2);
		}
		case GpType.Hashtable:
			return ReadHashtable(stream, flags, parameters);
		case GpType.Dictionary:
			return ReadDictionary(stream, flags, parameters);
		case GpType.Custom:
			return ReadCustomType(stream, 0);
		case GpType.OperationRequest:
			return DeserializeOperationRequest(stream);
		case GpType.OperationResponse:
			return DeserializeOperationResponse(stream, flags);
		case GpType.EventData:
			return DeserializeEventData(stream);
		case GpType.ObjectArray:
			return ReadObjectArray(stream, flags, parameters);
		case GpType.BooleanArray:
			return ReadBooleanArray(stream);
		case GpType.ByteArray:
			return ReadByteArray(stream);
		case GpType.ShortArray:
			return ReadInt16Array(stream);
		case GpType.DoubleArray:
			return ReadDoubleArray(stream);
		case GpType.FloatArray:
			return ReadSingleArray(stream);
		case GpType.StringArray:
			return ReadStringArray(stream);
		case GpType.HashtableArray:
			return ReadHashtableArray(stream, flags, parameters);
		case GpType.DictionaryArray:
			return ReadDictionaryArray(stream, flags, parameters);
		case GpType.CustomTypeArray:
			return ReadCustomTypeArray(stream);
		case GpType.CompressedIntArray:
			return ReadCompressedInt32Array(stream);
		case GpType.CompressedLongArray:
			return ReadCompressedInt64Array(stream);
		case GpType.Array:
			return ReadArrayInArray(stream, flags, parameters);
		case GpType.Null:
			return null;
		default:
			throw new InvalidDataException(string.Format("GpTypeCode not found: {0}(0x{0:X}). Is not a CustomType either. Pos: {1} Available: {2}", gpType, stream.Position, stream.Available));
		}
	}

	internal bool ReadBoolean(StreamBuffer stream)
	{
		return stream.ReadByte() > 0;
	}

	internal byte ReadByte(StreamBuffer stream)
	{
		return stream.ReadByte();
	}

	internal short ReadInt16(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(2, out offset);
		return (short)(bufferAndAdvance[offset++] | (bufferAndAdvance[offset] << 8));
	}

	internal ushort ReadUShort(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(2, out offset);
		return (ushort)(bufferAndAdvance[offset++] | (bufferAndAdvance[offset] << 8));
	}

	internal int ReadInt32(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(4, out offset);
		return (bufferAndAdvance[offset++] << 24) | (bufferAndAdvance[offset++] << 16) | (bufferAndAdvance[offset++] << 8) | bufferAndAdvance[offset];
	}

	internal long ReadInt64(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(4, out offset);
		return (long)(((ulong)bufferAndAdvance[offset++] << 56) | ((ulong)bufferAndAdvance[offset++] << 48) | ((ulong)bufferAndAdvance[offset++] << 40) | ((ulong)bufferAndAdvance[offset++] << 32) | ((ulong)bufferAndAdvance[offset++] << 24) | ((ulong)bufferAndAdvance[offset++] << 16) | ((ulong)bufferAndAdvance[offset++] << 8) | bufferAndAdvance[offset]);
	}

	internal float ReadSingle(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(4, out offset);
		return BitConverter.ToSingle(bufferAndAdvance, offset);
	}

	internal double ReadDouble(StreamBuffer stream)
	{
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(8, out offset);
		return BitConverter.ToDouble(bufferAndAdvance, offset);
	}

	internal ByteArraySlice ReadNonAllocByteArray(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		ByteArraySlice byteArraySlice = ByteArraySlicePool.Acquire((int)num);
		stream.Read(byteArraySlice.Buffer, 0, (int)num);
		byteArraySlice.Count = (int)num;
		return byteArraySlice;
	}

	internal byte[] ReadByteArray(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		byte[] array = new byte[num];
		stream.Read(array, 0, (int)num);
		return array;
	}

	public object ReadCustomType(StreamBuffer stream, byte gpType = 0)
	{
		byte b = 0;
		b = ((gpType != 0) ? ((byte)(gpType - 128)) : stream.ReadByte());
		int num = (int)ReadCompressedUInt32(stream);
		if (num < 0)
		{
			throw new InvalidDataException("ReadCustomType read negative size value: " + num + " before position: " + stream.Position);
		}
		bool flag = num <= stream.Available;
		if (!flag || num > 32767 || !Protocol.CodeDict.TryGetValue(b, out var value))
		{
			UnknownType unknownType = new UnknownType
			{
				TypeCode = b,
				Size = num
			};
			int num2 = (flag ? num : stream.Available);
			if (num2 > 0)
			{
				byte[] array = new byte[num2];
				stream.Read(array, 0, num2);
				unknownType.Data = array;
			}
			return unknownType;
		}
		if (value.DeserializeStreamFunction == null)
		{
			byte[] array2 = new byte[num];
			stream.Read(array2, 0, num);
			return value.DeserializeFunction(array2);
		}
		int position = stream.Position;
		object result = value.DeserializeStreamFunction(stream, (short)num);
		int num3 = stream.Position - position;
		if (num3 != num)
		{
			stream.Position = position + num;
		}
		return result;
	}

	public override EventData DeserializeEventData(StreamBuffer din, EventData target = null, DeserializationFlags flags = DeserializationFlags.None)
	{
		EventData eventData;
		if (target != null)
		{
			target.Reset();
			eventData = target;
		}
		else
		{
			eventData = new EventData();
		}
		eventData.Code = ReadByte(din);
		short num = ReadByte(din);
		bool flag = (flags & DeserializationFlags.AllowPooledByteArray) == DeserializationFlags.AllowPooledByteArray;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			byte b = din.ReadByte();
			byte b2 = din.ReadByte();
			object value;
			if (!flag)
			{
				value = Read(din, b2, flags, eventData.Parameters);
			}
			else if (b2 == 67)
			{
				value = ReadNonAllocByteArray(din);
			}
			else
			{
				if (b == eventData.SenderKey)
				{
					switch ((GpType)b2)
					{
					case GpType.Int1:
						eventData.Sender = ReadInt1(din, signNegative: false);
						break;
					case GpType.Int2:
						eventData.Sender = ReadInt2(din, signNegative: false);
						break;
					case GpType.Int1_:
						eventData.Sender = ReadInt1(din, signNegative: true);
						break;
					case GpType.Int2_:
						eventData.Sender = ReadInt2(din, signNegative: true);
						break;
					case GpType.CompressedInt:
						eventData.Sender = ReadCompressedInt32(din);
						break;
					case GpType.IntZero:
						eventData.Sender = 0;
						break;
					}
					continue;
				}
				value = Read(din, b2, flags, eventData.Parameters);
			}
			eventData.Parameters.Add(b, value);
		}
		return eventData;
	}

	[Obsolete("Use ParameterDictionary instead.")]
	private Dictionary<byte, object> ReadParameterTable(StreamBuffer stream, Dictionary<byte, object> target = null, DeserializationFlags flags = DeserializationFlags.None)
	{
		short num = ReadByte(stream);
		Dictionary<byte, object> dictionary = ((target != null) ? target : new Dictionary<byte, object>(num));
		for (uint num2 = 0u; num2 < num; num2++)
		{
			byte key = stream.ReadByte();
			byte b = stream.ReadByte();
			object value = ((b != 67 || (flags & DeserializationFlags.AllowPooledByteArray) != DeserializationFlags.AllowPooledByteArray) ? Read(stream, b, flags) : ReadNonAllocByteArray(stream));
			dictionary[key] = value;
		}
		return dictionary;
	}

	private ParameterDictionary ReadParameterDictionary(StreamBuffer stream, ParameterDictionary target = null, DeserializationFlags flags = DeserializationFlags.None)
	{
		short num = ReadByte(stream);
		ParameterDictionary parameterDictionary = ((target != null) ? target : new ParameterDictionary(num));
		bool flag = (flags & DeserializationFlags.AllowPooledByteArray) == DeserializationFlags.AllowPooledByteArray;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			byte code = stream.ReadByte();
			byte b = stream.ReadByte();
			object value = ((!flag || b != 67) ? Read(stream, b, flags, parameterDictionary) : ReadNonAllocByteArray(stream));
			parameterDictionary.Add(code, value);
		}
		return parameterDictionary;
	}

	public Hashtable ReadHashtable(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		int num = (int)ReadCompressedUInt32(stream);
		Hashtable hashtable = new Hashtable(num);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			object obj = Read(stream, flags, parameters);
			object value = Read(stream, flags, parameters);
			if (obj != null)
			{
				if (!(obj is StructWrapper<byte> obj2))
				{
					hashtable[obj] = value;
				}
				else
				{
					hashtable[obj2.Unwrap<byte>()] = value;
				}
			}
		}
		return hashtable;
	}

	public int[] ReadIntArray(StreamBuffer stream)
	{
		int num = ReadInt32(stream);
		int[] array = new int[num];
		for (uint num2 = 0u; num2 < num; num2++)
		{
			array[num2] = ReadInt32(stream);
		}
		return array;
	}

	public override OperationRequest DeserializeOperationRequest(StreamBuffer din, DeserializationFlags flags = DeserializationFlags.None)
	{
		OperationRequest operationRequest = new OperationRequest();
		operationRequest.OperationCode = ReadByte(din);
		operationRequest.Parameters = ReadParameterDictionary(din, operationRequest.Parameters, flags);
		return operationRequest;
	}

	public override OperationResponse DeserializeOperationResponse(StreamBuffer stream, DeserializationFlags flags = DeserializationFlags.None)
	{
		OperationResponse operationResponse = new OperationResponse();
		operationResponse.OperationCode = ReadByte(stream);
		operationResponse.ReturnCode = ReadInt16(stream);
		operationResponse.DebugMessage = Read(stream, ReadByte(stream), flags, operationResponse.Parameters) as string;
		operationResponse.Parameters = ReadParameterDictionary(stream, operationResponse.Parameters, flags);
		return operationResponse;
	}

	public override DisconnectMessage DeserializeDisconnectMessage(StreamBuffer stream)
	{
		DisconnectMessage disconnectMessage = new DisconnectMessage();
		disconnectMessage.Code = ReadInt16(stream);
		disconnectMessage.DebugMessage = Read(stream, ReadByte(stream)) as string;
		disconnectMessage.Parameters = ReadParameterTable(stream);
		return disconnectMessage;
	}

	internal string ReadString(StreamBuffer stream)
	{
		int num = (int)ReadCompressedUInt32(stream);
		if (num == 0)
		{
			return string.Empty;
		}
		int offset = 0;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(num, out offset);
		return Encoding.UTF8.GetString(bufferAndAdvance, offset, num);
	}

	private object ReadCustomTypeArray(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		byte b = stream.ReadByte();
		if (!Protocol.CodeDict.TryGetValue(b, out var value))
		{
			int position = stream.Position;
			for (uint num2 = 0u; num2 < num; num2++)
			{
				int num3 = (int)ReadCompressedUInt32(stream);
				int available = stream.Available;
				int num4 = ((num3 > available) ? available : num3);
				stream.Position += num4;
			}
			return new UnknownType[1]
			{
				new UnknownType
				{
					TypeCode = b,
					Size = stream.Position - position
				}
			};
		}
		Array array = Array.CreateInstance(value.Type, (int)num);
		for (uint num5 = 0u; num5 < num; num5++)
		{
			int num6 = (int)ReadCompressedUInt32(stream);
			if (num6 < 0)
			{
				throw new InvalidDataException("ReadCustomTypeArray read negative size value: " + num6 + " before position: " + stream.Position);
			}
			if (num6 > stream.Available || num6 > 32767)
			{
				stream.Position = stream.Length;
				throw new InvalidDataException("ReadCustomTypeArray read size value: " + num6 + " larger than short.MaxValue or available data: " + stream.Available);
			}
			object obj;
			if (value.DeserializeStreamFunction == null)
			{
				byte[] array2 = new byte[num6];
				stream.Read(array2, 0, num6);
				obj = value.DeserializeFunction(array2);
			}
			else
			{
				int position2 = stream.Position;
				obj = value.DeserializeStreamFunction(stream, (short)num6);
				int num7 = stream.Position - position2;
				if (num7 != num6)
				{
					stream.Position = position2 + num6;
				}
			}
			if (obj != null && value.Type.IsAssignableFrom(obj.GetType()))
			{
				array.SetValue(obj, (int)num5);
			}
		}
		return array;
	}

	private Type ReadDictionaryType(StreamBuffer stream, out GpType keyReadType, out GpType valueReadType)
	{
		keyReadType = (GpType)stream.ReadByte();
		GpType gpType = (valueReadType = (GpType)stream.ReadByte());
		Type type = ((keyReadType != 0) ? GetAllowedDictionaryKeyTypes(keyReadType) : typeof(object));
		Type type2;
		switch (gpType)
		{
		case GpType.Unknown:
			type2 = typeof(object);
			break;
		case GpType.Dictionary:
			type2 = ReadDictionaryType(stream);
			break;
		case GpType.Array:
			type2 = GetDictArrayType(stream);
			valueReadType = GpType.Unknown;
			break;
		case GpType.ObjectArray:
			type2 = typeof(object[]);
			break;
		case GpType.HashtableArray:
			type2 = typeof(Hashtable[]);
			break;
		default:
			type2 = GetClrArrayType(gpType);
			break;
		}
		return typeof(Dictionary<, >).MakeGenericType(type, type2);
	}

	private Type ReadDictionaryType(StreamBuffer stream)
	{
		GpType gpType = (GpType)stream.ReadByte();
		GpType gpType2 = (GpType)stream.ReadByte();
		Type type = ((gpType != 0) ? GetAllowedDictionaryKeyTypes(gpType) : typeof(object));
		Type type2 = gpType2 switch
		{
			GpType.Unknown => typeof(object), 
			GpType.Dictionary => ReadDictionaryType(stream), 
			GpType.Array => GetDictArrayType(stream), 
			_ => GetClrArrayType(gpType2), 
		};
		return typeof(Dictionary<, >).MakeGenericType(type, type2);
	}

	private Type GetDictArrayType(StreamBuffer stream)
	{
		GpType gpType = (GpType)stream.ReadByte();
		int num = 0;
		while (gpType == GpType.Array)
		{
			num++;
			gpType = (GpType)stream.ReadByte();
		}
		Type clrArrayType = GetClrArrayType(gpType);
		Type type = clrArrayType.MakeArrayType();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			type = type.MakeArrayType();
		}
		return type;
	}

	private IDictionary ReadDictionary(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		GpType keyReadType;
		GpType valueReadType;
		Type type = ReadDictionaryType(stream, out keyReadType, out valueReadType);
		if (type == null)
		{
			return null;
		}
		if (!(Activator.CreateInstance(type) is IDictionary dictionary))
		{
			return null;
		}
		ReadDictionaryElements(stream, keyReadType, valueReadType, dictionary, flags, parameters);
		return dictionary;
	}

	private bool ReadDictionaryElements(StreamBuffer stream, GpType keyReadType, GpType valueReadType, IDictionary dictionary, DeserializationFlags flags, ParameterDictionary parameters)
	{
		uint num = ReadCompressedUInt32(stream);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			object obj = ((keyReadType == GpType.Unknown) ? Read(stream, flags, parameters) : Read(stream, (byte)keyReadType));
			object value = ((valueReadType == GpType.Unknown) ? Read(stream, flags, parameters) : Read(stream, (byte)valueReadType));
			if (obj != null)
			{
				dictionary.Add(obj, value);
			}
		}
		return true;
	}

	private object[] ReadObjectArray(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		uint num = ReadCompressedUInt32(stream);
		object[] array = new object[num];
		for (uint num2 = 0u; num2 < num; num2++)
		{
			object obj = Read(stream, flags, parameters);
			array[num2] = obj;
		}
		return array;
	}

	private StructWrapper[] ReadWrapperArray(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		uint num = ReadCompressedUInt32(stream);
		StructWrapper[] array = new StructWrapper[num];
		for (uint num2 = 0u; num2 < num; num2++)
		{
			object obj = Read(stream, flags, parameters);
			array[num2] = obj as StructWrapper;
			if (obj == null)
			{
				Debug.WriteLine("Error: ReadWrapperArray hit null");
			}
			if (array[num2] == null)
			{
				Debug.WriteLine("Error: ReadWrapperArray null wrapper");
			}
		}
		return array;
	}

	private bool[] ReadBooleanArray(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		bool[] array = new bool[num];
		int num2 = (int)num / 8;
		int num3 = 0;
		while (num2 > 0)
		{
			byte b = stream.ReadByte();
			array[num3++] = (b & 1) == 1;
			array[num3++] = (b & 2) == 2;
			array[num3++] = (b & 4) == 4;
			array[num3++] = (b & 8) == 8;
			array[num3++] = (b & 0x10) == 16;
			array[num3++] = (b & 0x20) == 32;
			array[num3++] = (b & 0x40) == 64;
			array[num3++] = (b & 0x80) == 128;
			num2--;
		}
		if (num3 < num)
		{
			byte b2 = stream.ReadByte();
			int num4 = 0;
			while (num3 < num)
			{
				array[num3++] = (b2 & boolMasks[num4]) == boolMasks[num4];
				num4++;
			}
		}
		return array;
	}

	internal short[] ReadInt16Array(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		short[] array = new short[num];
		for (uint num2 = 0u; num2 < array.Length; num2++)
		{
			array[num2] = ReadInt16(stream);
		}
		return array;
	}

	private float[] ReadSingleArray(StreamBuffer stream)
	{
		int num = (int)ReadCompressedUInt32(stream);
		int num2 = num * 4;
		float[] array = new float[num];
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(num2, out offset);
		Buffer.BlockCopy(bufferAndAdvance, offset, array, 0, num2);
		return array;
	}

	private double[] ReadDoubleArray(StreamBuffer stream)
	{
		int num = (int)ReadCompressedUInt32(stream);
		int num2 = num * 8;
		double[] array = new double[num];
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(num2, out offset);
		Buffer.BlockCopy(bufferAndAdvance, offset, array, 0, num2);
		return array;
	}

	internal string[] ReadStringArray(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		string[] array = new string[num];
		for (uint num2 = 0u; num2 < array.Length; num2++)
		{
			array[num2] = ReadString(stream);
		}
		return array;
	}

	private Hashtable[] ReadHashtableArray(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		uint num = ReadCompressedUInt32(stream);
		Hashtable[] array = new Hashtable[num];
		for (uint num2 = 0u; num2 < num; num2++)
		{
			array[num2] = ReadHashtable(stream, flags, parameters);
		}
		return array;
	}

	private IDictionary[] ReadDictionaryArray(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		GpType keyReadType;
		GpType valueReadType;
		Type type = ReadDictionaryType(stream, out keyReadType, out valueReadType);
		uint num = ReadCompressedUInt32(stream);
		IDictionary[] array = (IDictionary[])Array.CreateInstance(type, (int)num);
		for (uint num2 = 0u; num2 < num; num2++)
		{
			array[num2] = (IDictionary)Activator.CreateInstance(type);
			ReadDictionaryElements(stream, keyReadType, valueReadType, array[num2], flags, parameters);
		}
		return array;
	}

	private Array ReadArrayInArray(StreamBuffer stream, DeserializationFlags flags, ParameterDictionary parameters)
	{
		uint num = ReadCompressedUInt32(stream);
		Array array = null;
		Type type = null;
		for (uint num2 = 0u; num2 < num; num2++)
		{
			object obj = Read(stream, flags, parameters);
			if (obj is Array array2)
			{
				if (array == null)
				{
					type = array2.GetType();
					array = Array.CreateInstance(type, (int)num);
				}
				if (type.IsAssignableFrom(array2.GetType()))
				{
					array.SetValue(array2, (int)num2);
				}
			}
		}
		return array;
	}

	internal int ReadInt1(StreamBuffer stream, bool signNegative)
	{
		if (signNegative)
		{
			return -stream.ReadByte();
		}
		return stream.ReadByte();
	}

	internal int ReadInt2(StreamBuffer stream, bool signNegative)
	{
		if (signNegative)
		{
			return -ReadUShort(stream);
		}
		return ReadUShort(stream);
	}

	internal int ReadCompressedInt32(StreamBuffer stream)
	{
		uint value = ReadCompressedUInt32(stream);
		return DecodeZigZag32(value);
	}

	private uint ReadCompressedUInt32(StreamBuffer stream)
	{
		uint num = 0u;
		int num2 = 0;
		byte[] buffer = stream.GetBuffer();
		int num3 = stream.Position;
		while (num2 != 35)
		{
			if (num3 >= stream.Length)
			{
				stream.Position = stream.Length;
				throw new EndOfStreamException("Failed to read full uint. offset: " + num3 + " stream.Length: " + stream.Length + " data.Length: " + buffer.Length + " stream.Available: " + stream.Available);
			}
			byte b = buffer[num3];
			num3++;
			num |= (uint)((b & 0x7F) << num2);
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				break;
			}
		}
		stream.Position = num3;
		return num;
	}

	internal long ReadCompressedInt64(StreamBuffer stream)
	{
		ulong value = ReadCompressedUInt64(stream);
		return DecodeZigZag64(value);
	}

	private ulong ReadCompressedUInt64(StreamBuffer stream)
	{
		ulong num = 0uL;
		int num2 = 0;
		byte[] buffer = stream.GetBuffer();
		int num3 = stream.Position;
		while (num2 != 70)
		{
			if (num3 >= buffer.Length)
			{
				throw new EndOfStreamException("Failed to read full ulong.");
			}
			byte b = buffer[num3];
			num3++;
			num |= (ulong)((long)(b & 0x7F) << num2);
			num2 += 7;
			if ((b & 0x80) == 0)
			{
				break;
			}
		}
		stream.Position = num3;
		return num;
	}

	internal int[] ReadCompressedInt32Array(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		int[] array = new int[num];
		for (uint num2 = 0u; num2 < array.Length; num2++)
		{
			array[num2] = ReadCompressedInt32(stream);
		}
		return array;
	}

	internal long[] ReadCompressedInt64Array(StreamBuffer stream)
	{
		uint num = ReadCompressedUInt32(stream);
		long[] array = new long[num];
		for (uint num2 = 0u; num2 < array.Length; num2++)
		{
			array[num2] = ReadCompressedInt64(stream);
		}
		return array;
	}

	private int DecodeZigZag32(uint value)
	{
		return (int)((value >> 1) ^ (0L - (long)(value & 1)));
	}

	private long DecodeZigZag64(ulong value)
	{
		return (long)((value >> 1) ^ (0L - (value & 1)));
	}

	internal void Write(StreamBuffer stream, object value, bool writeType)
	{
		if (value == null)
		{
			Write(stream, value, GpType.Null, writeType);
		}
		else
		{
			Write(stream, value, GetCodeOfType(value.GetType()), writeType);
		}
	}

	private void Write(StreamBuffer stream, object value, GpType gpType, bool writeType)
	{
		switch (gpType)
		{
		case GpType.Unknown:
			if (value is ByteArraySlice)
			{
				ByteArraySlice buffer = (ByteArraySlice)value;
				WriteByteArraySlice(stream, buffer, writeType);
				break;
			}
			if (value is ArraySegment<byte> seg)
			{
				WriteArraySegmentByte(stream, seg, writeType);
				break;
			}
			if (value is StructWrapper structWrapper)
			{
				switch (structWrapper.wrappedType)
				{
				case WrappedType.Bool:
					WriteBoolean(stream, value.Get<bool>(), writeType);
					break;
				case WrappedType.Byte:
					WriteByte(stream, value.Get<byte>(), writeType);
					break;
				case WrappedType.Int16:
					WriteInt16(stream, value.Get<short>(), writeType);
					break;
				case WrappedType.Int32:
					WriteCompressedInt32(stream, value.Get<int>(), writeType);
					break;
				case WrappedType.Int64:
					WriteCompressedInt64(stream, value.Get<long>(), writeType);
					break;
				case WrappedType.Single:
					WriteSingle(stream, value.Get<float>(), writeType);
					break;
				case WrappedType.Double:
					WriteDouble(stream, value.Get<double>(), writeType);
					break;
				default:
					WriteCustomType(stream, value, writeType);
					break;
				}
				break;
			}
			goto case GpType.Custom;
		case GpType.Custom:
			WriteCustomType(stream, value, writeType);
			break;
		case GpType.CustomTypeArray:
			WriteCustomTypeArray(stream, value, writeType);
			break;
		case GpType.Array:
			WriteArrayInArray(stream, value, writeType);
			break;
		case GpType.CompressedInt:
			WriteCompressedInt32(stream, (int)value, writeType);
			break;
		case GpType.CompressedLong:
			WriteCompressedInt64(stream, (long)value, writeType);
			break;
		case GpType.Dictionary:
			WriteDictionary(stream, (IDictionary)value, writeType);
			break;
		case GpType.Byte:
			WriteByte(stream, (byte)value, writeType);
			break;
		case GpType.Double:
			WriteDouble(stream, (double)value, writeType);
			break;
		case GpType.EventData:
			SerializeEventData(stream, (EventData)value, writeType);
			break;
		case GpType.Float:
			WriteSingle(stream, (float)value, writeType);
			break;
		case GpType.Hashtable:
			WriteHashtable(stream, (Hashtable)value, writeType);
			break;
		case GpType.Short:
			WriteInt16(stream, (short)value, writeType);
			break;
		case GpType.CompressedIntArray:
			WriteInt32ArrayCompressed(stream, (int[])value, writeType);
			break;
		case GpType.CompressedLongArray:
			WriteInt64ArrayCompressed(stream, (long[])value, writeType);
			break;
		case GpType.Boolean:
			WriteBoolean(stream, (bool)value, writeType);
			break;
		case GpType.OperationResponse:
			SerializeOperationResponse(stream, (OperationResponse)value, writeType);
			break;
		case GpType.OperationRequest:
			SerializeOperationRequest(stream, (OperationRequest)value, writeType);
			break;
		case GpType.String:
			WriteString(stream, (string)value, writeType);
			break;
		case GpType.ByteArray:
			WriteByteArray(stream, (byte[])value, writeType);
			break;
		case GpType.ObjectArray:
			WriteObjectArray(stream, (IList)value, writeType);
			break;
		case GpType.DictionaryArray:
			WriteDictionaryArray(stream, (IDictionary[])value, writeType);
			break;
		case GpType.DoubleArray:
			WriteDoubleArray(stream, (double[])value, writeType);
			break;
		case GpType.FloatArray:
			WriteSingleArray(stream, (float[])value, writeType);
			break;
		case GpType.HashtableArray:
			WriteHashtableArray(stream, value, writeType);
			break;
		case GpType.ShortArray:
			WriteInt16Array(stream, (short[])value, writeType);
			break;
		case GpType.BooleanArray:
			WriteBoolArray(stream, (bool[])value, writeType);
			break;
		case GpType.StringArray:
			WriteStringArray(stream, value, writeType);
			break;
		case GpType.Null:
			if (writeType)
			{
				stream.WriteByte(8);
			}
			break;
		}
	}

	public override void SerializeEventData(StreamBuffer stream, EventData serObject, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(26);
		}
		stream.WriteByte(serObject.Code);
		WriteParameterTable(stream, serObject.Parameters);
	}

	private void WriteParameterTable(StreamBuffer stream, Dictionary<byte, object> parameters)
	{
		if (parameters == null || parameters.Count == 0)
		{
			WriteByte(stream, 0, writeType: false);
			return;
		}
		WriteByte(stream, (byte)parameters.Count, writeType: false);
		foreach (KeyValuePair<byte, object> parameter in parameters)
		{
			stream.WriteByte(parameter.Key);
			Write(stream, parameter.Value, writeType: true);
		}
	}

	private void WriteParameterTable(StreamBuffer stream, ParameterDictionary parameters)
	{
		if (parameters == null || parameters.Count == 0)
		{
			WriteByte(stream, 0, writeType: false);
			return;
		}
		WriteByte(stream, (byte)parameters.Count, writeType: false);
		foreach (KeyValuePair<byte, object> parameter in parameters)
		{
			stream.WriteByte(parameter.Key);
			Write(stream, parameter.Value, writeType: true);
		}
	}

	private void SerializeOperationRequest(StreamBuffer stream, OperationRequest operation, bool setType)
	{
		SerializeOperationRequest(stream, operation.OperationCode, operation.Parameters, setType);
	}

	[Obsolete("Use ParameterDictionary instead.")]
	public override void SerializeOperationRequest(StreamBuffer stream, byte operationCode, Dictionary<byte, object> parameters, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(24);
		}
		stream.WriteByte(operationCode);
		WriteParameterTable(stream, parameters);
	}

	public override void SerializeOperationRequest(StreamBuffer stream, byte operationCode, ParameterDictionary parameters, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(24);
		}
		stream.WriteByte(operationCode);
		WriteParameterTable(stream, parameters);
	}

	public override void SerializeOperationResponse(StreamBuffer stream, OperationResponse serObject, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(25);
		}
		stream.WriteByte(serObject.OperationCode);
		WriteInt16(stream, serObject.ReturnCode, writeType: false);
		if (string.IsNullOrEmpty(serObject.DebugMessage))
		{
			stream.WriteByte(8);
		}
		else
		{
			stream.WriteByte(7);
			WriteString(stream, serObject.DebugMessage, writeType: false);
		}
		WriteParameterTable(stream, serObject.Parameters);
	}

	internal void WriteByte(StreamBuffer stream, byte value, bool writeType)
	{
		if (writeType)
		{
			if (value == 0)
			{
				stream.WriteByte(34);
				return;
			}
			stream.WriteByte(3);
		}
		stream.WriteByte(value);
	}

	internal void WriteBoolean(StreamBuffer stream, bool value, bool writeType)
	{
		if (writeType)
		{
			if (value)
			{
				stream.WriteByte(28);
			}
			else
			{
				stream.WriteByte(27);
			}
		}
		else
		{
			stream.WriteByte((byte)(value ? 1 : 0));
		}
	}

	internal void WriteUShort(StreamBuffer stream, ushort value)
	{
		stream.WriteBytes((byte)value, (byte)(value >> 8));
	}

	internal void WriteInt16(StreamBuffer stream, short value, bool writeType)
	{
		if (writeType)
		{
			if (value == 0)
			{
				stream.WriteByte(29);
				return;
			}
			stream.WriteByte(4);
		}
		stream.WriteBytes((byte)value, (byte)(value >> 8));
	}

	internal void WriteDouble(StreamBuffer stream, double value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(6);
		}
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(8, out offset);
		lock (memDoubleBlock)
		{
			memDoubleBlock[0] = value;
			Buffer.BlockCopy(memDoubleBlock, 0, bufferAndAdvance, offset, 8);
		}
	}

	internal void WriteSingle(StreamBuffer stream, float value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(5);
		}
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(4, out offset);
		lock (memFloatBlock)
		{
			memFloatBlock[0] = value;
			Buffer.BlockCopy(memFloatBlock, 0, bufferAndAdvance, offset, 4);
		}
	}

	internal void WriteString(StreamBuffer stream, string value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(7);
		}
		int byteCount = Encoding.UTF8.GetByteCount(value);
		if (byteCount > 32767)
		{
			throw new NotSupportedException("Strings that exceed a UTF8-encoded byte-length of 32767 (short.MaxValue) are not supported. Yours is: " + byteCount);
		}
		WriteIntLength(stream, byteCount);
		int offset = 0;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(byteCount, out offset);
		Encoding.UTF8.GetBytes(value, 0, value.Length, bufferAndAdvance, offset);
	}

	private void WriteHashtable(StreamBuffer stream, object value, bool writeType)
	{
		Hashtable hashtable = (Hashtable)value;
		if (writeType)
		{
			stream.WriteByte(21);
		}
		WriteIntLength(stream, hashtable.Count);
		Dictionary<object, object>.KeyCollection keys = hashtable.Keys;
		foreach (object item in keys)
		{
			Write(stream, item, writeType: true);
			Write(stream, hashtable[item], writeType: true);
		}
	}

	internal void WriteByteArray(StreamBuffer stream, byte[] value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(67);
		}
		WriteIntLength(stream, value.Length);
		stream.Write(value, 0, value.Length);
	}

	private void WriteArraySegmentByte(StreamBuffer stream, ArraySegment<byte> seg, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(67);
		}
		int count = seg.Count;
		WriteIntLength(stream, count);
		if (count > 0)
		{
			stream.Write(seg.Array, seg.Offset, count);
		}
	}

	private void WriteByteArraySlice(StreamBuffer stream, ByteArraySlice buffer, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(67);
		}
		int count = buffer.Count;
		WriteIntLength(stream, count);
		stream.Write(buffer.Buffer, buffer.Offset, count);
		buffer.Release();
	}

	internal void WriteInt32ArrayCompressed(StreamBuffer stream, int[] value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(73);
		}
		WriteIntLength(stream, value.Length);
		for (int i = 0; i < value.Length; i++)
		{
			WriteCompressedInt32(stream, value[i], writeType: false);
		}
	}

	private void WriteInt64ArrayCompressed(StreamBuffer stream, long[] values, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(74);
		}
		WriteIntLength(stream, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			WriteCompressedInt64(stream, values[i], writeType: false);
		}
	}

	internal void WriteBoolArray(StreamBuffer stream, bool[] value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(66);
		}
		WriteIntLength(stream, value.Length);
		int num = value.Length >> 3;
		uint num2 = (uint)(num + 1);
		byte[] array = new byte[num2];
		int num3 = 0;
		int i = 0;
		while (num > 0)
		{
			byte b = 0;
			if (value[i++])
			{
				b = (byte)(b | 1u);
			}
			if (value[i++])
			{
				b = (byte)(b | 2u);
			}
			if (value[i++])
			{
				b = (byte)(b | 4u);
			}
			if (value[i++])
			{
				b = (byte)(b | 8u);
			}
			if (value[i++])
			{
				b = (byte)(b | 0x10u);
			}
			if (value[i++])
			{
				b = (byte)(b | 0x20u);
			}
			if (value[i++])
			{
				b = (byte)(b | 0x40u);
			}
			if (value[i++])
			{
				b = (byte)(b | 0x80u);
			}
			array[num3] = b;
			num--;
			num3++;
		}
		if (i < value.Length)
		{
			byte b2 = 0;
			int num4 = 0;
			for (; i < value.Length; i++)
			{
				if (value[i])
				{
					b2 |= (byte)(1 << num4);
				}
				num4++;
			}
			array[num3] = b2;
			num3++;
		}
		stream.Write(array, 0, num3);
	}

	internal void WriteInt16Array(StreamBuffer stream, short[] value, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(68);
		}
		WriteIntLength(stream, value.Length);
		for (int i = 0; i < value.Length; i++)
		{
			WriteInt16(stream, value[i], writeType: false);
		}
	}

	internal void WriteSingleArray(StreamBuffer stream, float[] values, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(69);
		}
		WriteIntLength(stream, values.Length);
		int num = values.Length * 4;
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(num, out offset);
		Buffer.BlockCopy(values, 0, bufferAndAdvance, offset, num);
	}

	internal void WriteDoubleArray(StreamBuffer stream, double[] values, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(70);
		}
		WriteIntLength(stream, values.Length);
		int num = values.Length * 8;
		int offset;
		byte[] bufferAndAdvance = stream.GetBufferAndAdvance(num, out offset);
		Buffer.BlockCopy(values, 0, bufferAndAdvance, offset, num);
	}

	internal void WriteStringArray(StreamBuffer stream, object value0, bool writeType)
	{
		string[] array = (string[])value0;
		if (writeType)
		{
			stream.WriteByte(71);
		}
		WriteIntLength(stream, array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null)
			{
				throw new InvalidDataException("Unexpected - cannot serialize string array with null element " + i);
			}
			WriteString(stream, array[i], writeType: false);
		}
	}

	private void WriteObjectArray(StreamBuffer stream, object array, bool writeType)
	{
		WriteObjectArray(stream, (IList)array, writeType);
	}

	private void WriteObjectArray(StreamBuffer stream, IList array, bool writeType)
	{
		if (writeType)
		{
			stream.WriteByte(23);
		}
		WriteIntLength(stream, array.Count);
		for (int i = 0; i < array.Count; i++)
		{
			object value = array[i];
			Write(stream, value, writeType: true);
		}
	}

	private void WriteArrayInArray(StreamBuffer stream, object value, bool writeType)
	{
		object[] array = (object[])value;
		stream.WriteByte(64);
		WriteIntLength(stream, array.Length);
		object[] array2 = array;
		foreach (object value2 in array2)
		{
			Write(stream, value2, writeType: true);
		}
	}

	private void WriteCustomTypeBody(CustomType customType, StreamBuffer stream, object value)
	{
		if (customType.SerializeStreamFunction == null)
		{
			byte[] array = customType.SerializeFunction(value);
			WriteIntLength(stream, array.Length);
			stream.Write(array, 0, array.Length);
			return;
		}
		int position = stream.Position;
		stream.Position++;
		uint num = (uint)customType.SerializeStreamFunction(stream, value);
		int num2 = stream.Position - position - 1;
		if (num2 != num)
		{
			Debug.WriteLine("Serialization for Custom Type '" + value.GetType()?.ToString() + "' returns size " + num + " bytes but wrote " + num2 + " bytes. Sending the latter as size.");
		}
		int num3 = WriteCompressedUInt32(memCustomTypeBodyLengthSerialized, (uint)num2);
		if (num3 == 1)
		{
			stream.GetBuffer()[position] = memCustomTypeBodyLengthSerialized[0];
			return;
		}
		for (int i = 0; i < num3 - 1; i++)
		{
			stream.WriteByte(0);
		}
		Buffer.BlockCopy(stream.GetBuffer(), position + 1, stream.GetBuffer(), position + num3, num2);
		Buffer.BlockCopy(memCustomTypeBodyLengthSerialized, 0, stream.GetBuffer(), position, num3);
		stream.Position = position + num3 + num2;
	}

	private void WriteCustomType(StreamBuffer stream, object value, bool writeType)
	{
		Type type = ((!(value is StructWrapper structWrapper)) ? value.GetType() : structWrapper.ttype);
		if (Protocol.TypeDict.TryGetValue(type, out var value2))
		{
			if (writeType)
			{
				if (value2.Code < 100)
				{
					stream.WriteByte((byte)(128 + value2.Code));
				}
				else
				{
					stream.WriteByte(19);
					stream.WriteByte(value2.Code);
				}
			}
			else
			{
				stream.WriteByte(value2.Code);
			}
			WriteCustomTypeBody(value2, stream, value);
			return;
		}
		throw new Exception("Write failed. Custom type not found: " + type);
	}

	private void WriteCustomTypeArray(StreamBuffer stream, object value, bool writeType)
	{
		IList list = (IList)value;
		Type elementType = value.GetType().GetElementType();
		if (Protocol.TypeDict.TryGetValue(elementType, out var value2))
		{
			if (writeType)
			{
				stream.WriteByte(83);
			}
			WriteIntLength(stream, list.Count);
			stream.WriteByte(value2.Code);
			{
				foreach (object item in list)
				{
					WriteCustomTypeBody(value2, stream, item);
				}
				return;
			}
		}
		throw new Exception("Write failed. Custom type of element not found: " + elementType);
	}

	private bool WriteArrayHeader(StreamBuffer stream, Type type)
	{
		Type elementType = type.GetElementType();
		while (elementType.IsArray)
		{
			stream.WriteByte(64);
			elementType = elementType.GetElementType();
		}
		GpType codeOfType = GetCodeOfType(elementType);
		if (codeOfType == GpType.Unknown)
		{
			return false;
		}
		stream.WriteByte((byte)(codeOfType | GpType.CustomTypeSlim));
		return true;
	}

	private void WriteDictionaryElements(StreamBuffer stream, IDictionary dictionary, GpType keyWriteType, GpType valueWriteType)
	{
		WriteIntLength(stream, dictionary.Count);
		foreach (DictionaryEntry item in dictionary)
		{
			Write(stream, item.Key, keyWriteType == GpType.Unknown);
			Write(stream, item.Value, valueWriteType == GpType.Unknown);
		}
	}

	private void WriteDictionary(StreamBuffer stream, object dict, bool setType)
	{
		if (setType)
		{
			stream.WriteByte(20);
		}
		WriteDictionaryHeader(stream, dict.GetType(), out var keyWriteType, out var valueWriteType);
		IDictionary dictionary = (IDictionary)dict;
		WriteDictionaryElements(stream, dictionary, keyWriteType, valueWriteType);
	}

	private void WriteDictionaryHeader(StreamBuffer stream, Type type, out GpType keyWriteType, out GpType valueWriteType)
	{
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments[0] == typeof(object))
		{
			stream.WriteByte(0);
			keyWriteType = GpType.Unknown;
		}
		else
		{
			if (!genericArguments[0].IsPrimitive && genericArguments[0] != typeof(string))
			{
				throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
			}
			keyWriteType = GetCodeOfType(genericArguments[0]);
			if (keyWriteType == GpType.Unknown)
			{
				throw new InvalidDataException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
			}
			stream.WriteByte((byte)keyWriteType);
		}
		if (genericArguments[1] == typeof(object))
		{
			stream.WriteByte(0);
			valueWriteType = GpType.Unknown;
			return;
		}
		if (genericArguments[1].IsArray)
		{
			if (WriteArrayType(stream, genericArguments[1], out valueWriteType))
			{
				return;
			}
			throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
		}
		valueWriteType = GetCodeOfType(genericArguments[1]);
		if (valueWriteType == GpType.Unknown)
		{
			throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
		}
		if (valueWriteType == GpType.Array)
		{
			if (!WriteArrayHeader(stream, genericArguments[1]))
			{
				throw new InvalidDataException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[1]);
			}
		}
		else if (valueWriteType == GpType.Dictionary)
		{
			stream.WriteByte((byte)valueWriteType);
			WriteDictionaryHeader(stream, genericArguments[1], out var _, out var _);
		}
		else
		{
			stream.WriteByte((byte)valueWriteType);
		}
	}

	private bool WriteArrayType(StreamBuffer stream, Type type, out GpType writeType)
	{
		Type elementType = type.GetElementType();
		if (elementType == null)
		{
			throw new InvalidDataException("Unexpected - cannot serialize array with type: " + type);
		}
		if (elementType.IsArray)
		{
			while (elementType != null && elementType.IsArray)
			{
				stream.WriteByte(64);
				elementType = elementType.GetElementType();
			}
			byte value = (byte)(GetCodeOfType(elementType) | GpType.Array);
			stream.WriteByte(value);
			writeType = GpType.Array;
			return true;
		}
		if (elementType.IsPrimitive)
		{
			byte b = (byte)(GetCodeOfType(elementType) | GpType.Array);
			if (b == 226)
			{
				b = 67;
			}
			stream.WriteByte(b);
			if (Enum.IsDefined(typeof(GpType), b))
			{
				writeType = (GpType)b;
				return true;
			}
			writeType = GpType.Unknown;
			return false;
		}
		if (elementType == typeof(string))
		{
			stream.WriteByte(71);
			writeType = GpType.StringArray;
			return true;
		}
		if (elementType == typeof(object))
		{
			stream.WriteByte(23);
			writeType = GpType.ObjectArray;
			return true;
		}
		if (elementType == typeof(Hashtable))
		{
			stream.WriteByte(85);
			writeType = GpType.HashtableArray;
			return true;
		}
		writeType = GpType.Unknown;
		return false;
	}

	private void WriteHashtableArray(StreamBuffer stream, object value, bool writeType)
	{
		Hashtable[] array = (Hashtable[])value;
		if (writeType)
		{
			stream.WriteByte(85);
		}
		WriteIntLength(stream, array.Length);
		Hashtable[] array2 = array;
		foreach (Hashtable value2 in array2)
		{
			WriteHashtable(stream, value2, writeType: false);
		}
	}

	private void WriteDictionaryArray(StreamBuffer stream, IDictionary[] dictArray, bool writeType)
	{
		stream.WriteByte(84);
		WriteDictionaryHeader(stream, dictArray.GetType().GetElementType(), out var keyWriteType, out var valueWriteType);
		WriteIntLength(stream, dictArray.Length);
		foreach (IDictionary dictionary in dictArray)
		{
			WriteDictionaryElements(stream, dictionary, keyWriteType, valueWriteType);
		}
	}

	private void WriteIntLength(StreamBuffer stream, int value)
	{
		WriteCompressedUInt32(stream, (uint)value);
	}

	private void WriteVarInt32(StreamBuffer stream, int value, bool writeType)
	{
		WriteCompressedInt32(stream, value, writeType);
	}

	private void WriteCompressedInt32(StreamBuffer stream, int value, bool writeType)
	{
		if (writeType)
		{
			if (value == 0)
			{
				stream.WriteByte(30);
				return;
			}
			if (value > 0)
			{
				if (value <= 255)
				{
					stream.WriteByte(11);
					stream.WriteByte((byte)value);
					return;
				}
				if (value <= 65535)
				{
					stream.WriteByte(13);
					WriteUShort(stream, (ushort)value);
					return;
				}
			}
			else if (value >= -65535)
			{
				if (value >= -255)
				{
					stream.WriteByte(12);
					stream.WriteByte((byte)(-value));
					return;
				}
				if (value >= -65535)
				{
					stream.WriteByte(14);
					WriteUShort(stream, (ushort)(-value));
					return;
				}
			}
		}
		if (writeType)
		{
			stream.WriteByte(9);
		}
		uint value2 = EncodeZigZag32(value);
		WriteCompressedUInt32(stream, value2);
	}

	private void WriteCompressedInt64(StreamBuffer stream, long value, bool writeType)
	{
		if (writeType)
		{
			if (value == 0)
			{
				stream.WriteByte(31);
				return;
			}
			if (value > 0)
			{
				if (value <= 255)
				{
					stream.WriteByte(15);
					stream.WriteByte((byte)value);
					return;
				}
				if (value <= 65535)
				{
					stream.WriteByte(17);
					WriteUShort(stream, (ushort)value);
					return;
				}
			}
			else if (value >= -65535)
			{
				if (value >= -255)
				{
					stream.WriteByte(16);
					stream.WriteByte((byte)(-value));
					return;
				}
				if (value >= -65535)
				{
					stream.WriteByte(18);
					WriteUShort(stream, (ushort)(-value));
					return;
				}
			}
		}
		if (writeType)
		{
			stream.WriteByte(10);
		}
		ulong value2 = EncodeZigZag64(value);
		WriteCompressedUInt64(stream, value2);
	}

	private void WriteCompressedUInt32(StreamBuffer stream, uint value)
	{
		lock (memCompressedUInt32)
		{
			stream.Write(memCompressedUInt32, 0, WriteCompressedUInt32(memCompressedUInt32, value));
		}
	}

	private int WriteCompressedUInt32(byte[] buffer, uint value)
	{
		int num = 0;
		buffer[num] = (byte)(value & 0x7Fu);
		for (value >>= 7; value != 0; value >>= 7)
		{
			buffer[num] |= 128;
			buffer[++num] = (byte)(value & 0x7Fu);
		}
		return num + 1;
	}

	private void WriteCompressedUInt64(StreamBuffer stream, ulong value)
	{
		int num = 0;
		lock (memCompressedUInt64)
		{
			memCompressedUInt64[num] = (byte)(value & 0x7F);
			for (value >>= 7; value != 0; value >>= 7)
			{
				memCompressedUInt64[num] |= 128;
				memCompressedUInt64[++num] = (byte)(value & 0x7F);
			}
			num++;
			stream.Write(memCompressedUInt64, 0, num);
		}
	}

	private uint EncodeZigZag32(int value)
	{
		return (uint)((value << 1) ^ (value >> 31));
	}

	private ulong EncodeZigZag64(long value)
	{
		return (ulong)((value << 1) ^ (value >> 63));
	}
}
