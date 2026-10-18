namespace SouthPointe.Serialization.MessagePack;

public struct Format
{
	public readonly byte Value;

	public const byte PositiveFixIntMin = 0;

	public const byte PositiveFixIntMax = 127;

	public const byte FixMapMin = 128;

	public const byte FixMapMax = 143;

	public const byte FixArrayMin = 144;

	public const byte FixArrayMax = 159;

	public const byte FixStrMin = 160;

	public const byte FixStrMax = 191;

	public const byte Nil = 192;

	public const byte NeverUsed = 193;

	public const byte False = 194;

	public const byte True = 195;

	public const byte Bin8 = 196;

	public const byte Bin16 = 197;

	public const byte Bin32 = 198;

	public const byte Ext8 = 199;

	public const byte Ext16 = 200;

	public const byte Ext32 = 201;

	public const byte Float32 = 202;

	public const byte Float64 = 203;

	public const byte UInt8 = 204;

	public const byte UInt16 = 205;

	public const byte UInt32 = 206;

	public const byte UInt64 = 207;

	public const byte Int8 = 208;

	public const byte Int16 = 209;

	public const byte Int32 = 210;

	public const byte Int64 = 211;

	public const byte FixExt1 = 212;

	public const byte FixExt2 = 213;

	public const byte FixExt4 = 214;

	public const byte FixExt8 = 215;

	public const byte FixExt16 = 216;

	public const byte Str8 = 217;

	public const byte Str16 = 218;

	public const byte Str32 = 219;

	public const byte Array16 = 220;

	public const byte Array32 = 221;

	public const byte Map16 = 222;

	public const byte Map32 = 223;

	public const byte NegativeFixIntMin = 224;

	public const byte NegativeFixIntMax = byte.MaxValue;

	public bool IsPositiveFixInt => Between(0, 127);

	public bool IsFixMap => Between(128, 143);

	public bool IsFixArray => Between(144, 159);

	public bool IsFixStr => Between(160, 191);

	public bool IsNil => Value == 192;

	public bool IsNeverUsed => Value == 193;

	public bool IsFalse => Value == 194;

	public bool IsTrue => Value == 195;

	public bool IsBin8 => Value == 196;

	public bool IsBin16 => Value == 197;

	public bool IsBin32 => Value == 198;

	public bool IsExt8 => Value == 199;

	public bool IsExt16 => Value == 200;

	public bool IsExt32 => Value == 201;

	public bool IsFloat32 => Value == 202;

	public bool IsFloat64 => Value == 203;

	public bool IsUInt8 => Value == 204;

	public bool IsUInt16 => Value == 205;

	public bool IsUInt32 => Value == 206;

	public bool IsUInt64 => Value == 207;

	public bool IsInt8 => Value == 208;

	public bool IsInt16 => Value == 209;

	public bool IsInt32 => Value == 210;

	public bool IsInt64 => Value == 211;

	public bool IsFixExt1 => Value == 212;

	public bool IsFixExt2 => Value == 213;

	public bool IsFixExt4 => Value == 214;

	public bool IsFixExt8 => Value == 215;

	public bool IsFixExt16 => Value == 216;

	public bool IsStr8 => Value == 217;

	public bool IsStr16 => Value == 218;

	public bool IsStr32 => Value == 219;

	public bool IsArray16 => Value == 220;

	public bool IsArray32 => Value == 221;

	public bool IsMap16 => Value == 222;

	public bool IsMap32 => Value == 223;

	public bool IsNegativeFixInt => Between(224, byte.MaxValue);

	public bool IsEmptyArray => Value == 144;

	public bool IsIntFamily
	{
		get
		{
			if (!IsPositiveFixInt && !IsNegativeFixInt && !IsInt8 && !IsUInt8 && !IsInt16 && !IsUInt16 && !IsInt32 && !IsUInt32 && !IsInt64)
			{
				return IsUInt64;
			}
			return true;
		}
	}

	public bool IsBoolFamily
	{
		get
		{
			if (!IsFalse)
			{
				return IsTrue;
			}
			return true;
		}
	}

	public bool IsFloatFamily
	{
		get
		{
			if (!IsFloat32)
			{
				return IsFloat64;
			}
			return true;
		}
	}

	public bool IsStringFamily
	{
		get
		{
			if (!IsFixStr && !IsStr8 && !IsStr16)
			{
				return IsStr32;
			}
			return true;
		}
	}

	public bool IsBinaryFamily
	{
		get
		{
			if (!IsBin8 && !IsBin16)
			{
				return IsBin32;
			}
			return true;
		}
	}

	public bool IsArrayFamily
	{
		get
		{
			if (!IsFixArray && !IsArray16)
			{
				return IsArray32;
			}
			return true;
		}
	}

	public bool IsMapFamily
	{
		get
		{
			if (!IsFixMap && !IsMap16)
			{
				return IsMap32;
			}
			return true;
		}
	}

	public bool IsExtFamily
	{
		get
		{
			if (!IsFixExt1 && !IsFixExt2 && !IsFixExt4 && !IsFixExt8 && !IsFixExt16 && !IsExt8 && !IsExt16)
			{
				return IsExt32;
			}
			return true;
		}
	}

	public Format(byte value)
	{
		Value = value;
	}

	private bool Between(byte min, byte max)
	{
		if (Value >= min)
		{
			return Value <= max;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is Format)
		{
			return Value == ((Format)obj).Value;
		}
		if (obj is byte)
		{
			return Value == (byte)obj;
		}
		return false;
	}

	public static byte operator &(Format f1, byte value)
	{
		return (byte)(f1.Value & value);
	}

	public static bool operator ==(Format f1, Format f2)
	{
		return f1.Value == f2.Value;
	}

	public static bool operator !=(Format f1, Format f2)
	{
		return f1.Value != f2.Value;
	}

	public override string ToString()
	{
		return "0x" + Value.ToString("X2");
	}
}
