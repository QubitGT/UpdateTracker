#define DEBUG
#define TRACE
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Fusion.Sockets.Stun;

internal class StunMessage
{
	public enum StunMessageType
	{
		BindingRequest = 1,
		BindingResponse = 257,
		BindingErrorResponse = 273,
		SharedSecretRequest = 2,
		SharedSecretResponse = 258,
		SharedSecretErrorResponse = 274
	}

	private enum AttributeType
	{
		MappedAddress = 1,
		Username = 6,
		MessageIntegrity = 8,
		ErrorCode = 9,
		UnknownAttribute = 10,
		Realm = 20,
		Nonce = 21,
		XorMappedAddress = 32
	}

	internal static class StunDefines
	{
		public const int STUN_MAGIC_COOKIE = 554869826;

		public const ulong STUN_MAGIC_COOKIE_NETWORK_ORDER = 1118048801uL;

		public const short STUN_MAGIC_COOKIE_PARTIAL = 8466;

		public const int STUN_XOR_FINGERPRINT = 1398035790;

		public const int HEADER_SIZE = 20;

		public const int TRANSACTION_ID_SIZE = 12;
	}

	private enum IPFamily
	{
		IPv4 = 1,
		IPv6
	}

	private Guid _id = Guid.Empty;

	private static HashSet<int> _stunMessageTypeValues;

	private static HashSet<int> StunMessageTypeValues
	{
		get
		{
			if (_stunMessageTypeValues == null)
			{
				_stunMessageTypeValues = new HashSet<int>();
				foreach (object value in Enum.GetValues(typeof(StunMessageType)))
				{
					_stunMessageTypeValues.Add((int)value);
				}
			}
			return _stunMessageTypeValues;
		}
	}

	public StunMessageType Type { get; private set; }

	public Guid ID
	{
		get
		{
			if (_id.Equals(Guid.Empty))
			{
				byte[] array = new byte[16];
				Array.Copy(TransactionID, array, 12);
				_id = new Guid(array);
			}
			return _id;
		}
	}

	private byte[] TransactionID { get; set; }

	public IPEndPoint MappedAddress
	{
		get
		{
			if (Attributes.TryGetValue(AttributeType.MappedAddress, out var value))
			{
				return value as IPEndPoint;
			}
			return null;
		}
		set
		{
			Attributes[AttributeType.MappedAddress] = value;
		}
	}

	public string UserName { get; set; } = null;


	public StunErrorAttribute ErrorCode { get; set; } = null;


	private Dictionary<AttributeType, object> Attributes { get; set; }

	public StunMessage(Guid msgID, StunMessageType messageType = StunMessageType.BindingRequest)
	{
		Type = messageType;
		TransactionID = new byte[12];
		Array.Copy(msgID.Equals(Guid.Empty) ? Guid.NewGuid().ToByteArray() : msgID.ToByteArray(), TransactionID, 12);
		Attributes = new Dictionary<AttributeType, object>();
	}

	public unsafe static bool IsStunMessage(byte* data, int length)
	{
		if (length <= 0 || length < 20)
		{
			InternalLogStreams.LogTraceStun?.Log("Invalid STUN Message Size");
			return false;
		}
		int num = (*data << 8) | data[1];
		int num2 = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7];
		bool flag = num2 == 554869826 && (num & 0xC000) == 0 && StunMessageTypeValues.Contains(num);
		InternalLogStreams.LogTraceStun?.Log($"STUN Message Type: {num}, Magic Cookie: {num2}, Result: {flag}");
		return flag;
	}

	public unsafe static StunMessage TryParse(byte* data, int length)
	{
		if (length <= 0 || length < 20)
		{
			return null;
		}
		int offset = 0;
		int num = (data[offset++] << 8) | data[offset++];
		if ((num & 0xC000) == 0 && Enum.IsDefined(typeof(StunMessageType), num))
		{
			StunMessageType type = (StunMessageType)num;
			int num2 = (data[offset++] << 8) | data[offset++];
			int num3 = (data[offset++] << 24) | (data[offset++] << 16) | (data[offset++] << 8) | data[offset++];
			if (num3 != 554869826)
			{
				return null;
			}
			StunMessage stunMessage = new StunMessage(Guid.Empty);
			stunMessage.Type = type;
			stunMessage.TransactionID = new byte[12];
			StunMessage stunMessage2 = stunMessage;
			for (int i = 0; i < 12; i++)
			{
				stunMessage2.TransactionID[i] = data[offset++];
			}
			while (offset - 20 < num2)
			{
				stunMessage2.ReadAttribute(data, ref offset);
			}
			return stunMessage2;
		}
		return null;
	}

	public byte[] Serialize()
	{
		byte[] array = new byte[512];
		int num = 0;
		array[num++] = (byte)((int)Type >> 8);
		array[num++] = (byte)(Type & (StunMessageType)255);
		int num2 = num;
		array[num++] = 0;
		array[num++] = 0;
		array[num++] = 33;
		array[num++] = 18;
		array[num++] = 164;
		array[num++] = 66;
		Array.Copy(TransactionID, 0, array, num, 12);
		num += 12;
		WriteAttributes(array, ref num);
		int num3 = num - 20;
		array[num2++] = (byte)(num3 >> 8);
		array[num2++] = (byte)((uint)num3 & 0xFFu);
		byte[] array2 = new byte[num];
		Array.Copy(array, array2, array2.Length);
		return array2;
	}

	private void WriteAttributes(byte[] msg, ref int offset)
	{
		foreach (KeyValuePair<AttributeType, object> attribute in Attributes)
		{
			switch (attribute.Key)
			{
			case AttributeType.MappedAddress:
				StoreEndPoint(AttributeType.MappedAddress, (IPEndPoint)attribute.Value, msg, ref offset);
				break;
			case AttributeType.Username:
			{
				byte[] bytes2 = Encoding.ASCII.GetBytes((string)attribute.Value);
				msg[offset++] = 0;
				msg[offset++] = 6;
				msg[offset++] = (byte)(bytes2.Length >> 8);
				msg[offset++] = (byte)((uint)bytes2.Length & 0xFFu);
				Array.Copy(bytes2, 0, msg, offset, bytes2.Length);
				offset += bytes2.Length;
				break;
			}
			case AttributeType.ErrorCode:
			{
				byte[] bytes = Encoding.ASCII.GetBytes(ErrorCode.ReasonText);
				msg[offset++] = 0;
				msg[offset++] = 9;
				msg[offset++] = 0;
				msg[offset++] = (byte)(4 + bytes.Length);
				msg[offset++] = 0;
				msg[offset++] = 0;
				msg[offset++] = (byte)Math.Floor((double)(ErrorCode.Code / 100));
				msg[offset++] = (byte)((uint)ErrorCode.Code & 0xFFu);
				Array.Copy(bytes, msg, bytes.Length);
				offset += bytes.Length;
				break;
			}
			}
		}
	}

	private unsafe void ReadAttribute(byte* data, ref int offset)
	{
		AttributeType attributeType = (AttributeType)((data[offset++] << 8) | data[offset++]);
		int num = (data[offset++] << 8) | data[offset++];
		int num2 = offset;
		try
		{
			switch (attributeType)
			{
			case AttributeType.XorMappedAddress:
				InternalLogStreams.LogTraceStun?.Log("AttributeType.XorMappedAddress");
				MappedAddress = ParseXorEndPoint(data, ref offset);
				break;
			case AttributeType.MappedAddress:
				InternalLogStreams.LogTraceStun?.Log("AttributeType.MappedAddress");
				MappedAddress = ParseEndPoint(data, ref offset);
				break;
			case AttributeType.UnknownAttribute:
				InternalLogStreams.LogTraceStun?.Error("UnknownAttribute");
				break;
			}
		}
		catch (Exception message)
		{
			InternalLogStreams.LogDebug?.Error(message);
		}
		offset = num2 + num;
	}

	private unsafe IPEndPoint ParseEndPoint(byte* data, ref int offset)
	{
		offset++;
		byte b = data[offset++];
		int port = (data[offset++] << 8) | data[offset++];
		return new IPEndPoint(new IPAddress(new byte[4]
		{
			data[offset++],
			data[offset++],
			data[offset++],
			data[offset++]
		}), port);
	}

	private unsafe IPEndPoint ParseXorEndPoint(byte* data, ref int offset)
	{
		offset++;
		IPFamily iPFamily = (IPFamily)data[offset++];
		int num = (data[offset++] << 8) | data[offset++];
		num ^= 0x2112;
		switch (iPFamily)
		{
		case IPFamily.IPv4:
		{
			byte[] array2 = new byte[4]
			{
				data[offset++],
				data[offset++],
				data[offset++],
				data[offset++]
			};
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(array2);
			}
			uint num6 = BitConverter.ToUInt32(array2, 0);
			num6 ^= 0x2112A442u;
			byte[] bytes3 = BitConverter.GetBytes(num6);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes3);
			}
			num6 = BitConverter.ToUInt32(bytes3, 0);
			return new IPEndPoint(num6, num);
		}
		case IPFamily.IPv6:
		{
			ulong num2 = ((ulong)data[offset++] << 56) | ((ulong)data[offset++] << 48) | ((ulong)data[offset++] << 40) | ((ulong)data[offset++] << 32) | ((ulong)data[offset++] << 24) | ((ulong)data[offset++] << 16) | ((ulong)data[offset++] << 8) | data[offset++];
			ulong num3 = ((ulong)data[offset++] << 56) | ((ulong)data[offset++] << 48) | ((ulong)data[offset++] << 40) | ((ulong)data[offset++] << 32) | ((ulong)data[offset++] << 24) | ((ulong)data[offset++] << 16) | ((ulong)data[offset++] << 8) | data[offset++];
			ulong num4 = BitConverter.ToUInt32(TransactionID, 0);
			ulong num5 = BitConverter.ToUInt64(TransactionID, 4);
			num4 = (num4 << 32) | 0x42A41221;
			ulong value = num2 ^ num4;
			ulong value2 = num3 ^ num5;
			byte[] bytes = BitConverter.GetBytes(value);
			byte[] bytes2 = BitConverter.GetBytes(value2);
			byte[] array = new byte[16];
			Array.Copy(bytes, 0, array, 0, bytes.Length);
			Array.Copy(bytes2, 0, array, 8, bytes2.Length);
			return new IPEndPoint(new IPAddress(array), num);
		}
		default:
			return null;
		}
	}

	private void StoreEndPoint(AttributeType type, IPEndPoint endPoint, byte[] message, ref int offset)
	{
		message[offset++] = (byte)((int)type >> 8);
		message[offset++] = (byte)(type & (AttributeType)255);
		message[offset++] = 0;
		message[offset++] = 8;
		message[offset++] = 0;
		message[offset++] = 1;
		message[offset++] = (byte)(endPoint.Port >> 8);
		message[offset++] = (byte)((uint)endPoint.Port & 0xFFu);
		byte[] addressBytes = endPoint.Address.GetAddressBytes();
		message[offset++] = addressBytes[0];
		message[offset++] = addressBytes[0];
		message[offset++] = addressBytes[0];
		message[offset++] = addressBytes[0];
	}
}
