using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal;

public static class YamlCodes
{
	public static readonly byte[] YamlDirectiveName = new byte[4] { 89, 65, 77, 76 };

	public static readonly byte[] TagDirectiveName = new byte[3] { 84, 65, 71 };

	public static readonly byte[] Utf8Bom = new byte[3] { 239, 187, 191 };

	public static readonly byte[] StreamStart = new byte[3] { 45, 45, 45 };

	public static readonly byte[] DocStart = new byte[3] { 46, 46, 46 };

	public static readonly byte[] CrLf = new byte[2] { 13, 10 };

	public static readonly byte[] Null0 = new byte[4] { 110, 117, 108, 108 };

	public static readonly byte[] Null1 = new byte[4] { 78, 117, 108, 108 };

	public static readonly byte[] Null2 = new byte[4] { 78, 85, 76, 76 };

	public const byte NullAlias = 126;

	public static readonly byte[] True0 = new byte[4] { 116, 114, 117, 101 };

	public static readonly byte[] True1 = new byte[4] { 84, 114, 117, 101 };

	public static readonly byte[] True2 = new byte[4] { 84, 82, 85, 69 };

	public static readonly byte[] False0 = new byte[5] { 102, 97, 108, 115, 101 };

	public static readonly byte[] False1 = new byte[5] { 70, 97, 108, 115, 101 };

	public static readonly byte[] False2 = new byte[5] { 70, 65, 76, 83, 69 };

	public static readonly byte[] Inf0 = new byte[4] { 46, 105, 110, 102 };

	public static readonly byte[] Inf1 = new byte[4] { 46, 73, 110, 102 };

	public static readonly byte[] Inf2 = new byte[4] { 46, 73, 78, 70 };

	public static readonly byte[] Inf3 = new byte[5] { 43, 46, 105, 110, 102 };

	public static readonly byte[] Inf4 = new byte[5] { 43, 46, 73, 110, 102 };

	public static readonly byte[] Inf5 = new byte[5] { 43, 46, 73, 78, 70 };

	public static readonly byte[] Yes0 = new byte[3] { 121, 101, 115 };

	public static readonly byte[] Yes1 = new byte[3] { 89, 101, 115 };

	public static readonly byte[] Yes2 = new byte[3] { 89, 69, 83 };

	public static readonly byte[] No0 = new byte[2] { 110, 111 };

	public static readonly byte[] No1 = new byte[2] { 78, 111 };

	public static readonly byte[] No2 = new byte[2] { 78, 79 };

	public static readonly byte[] On0 = new byte[2] { 111, 110 };

	public static readonly byte[] On1 = new byte[2] { 79, 110 };

	public static readonly byte[] On2 = new byte[2] { 79, 78 };

	public static readonly byte[] Off0 = new byte[3] { 111, 102, 102 };

	public static readonly byte[] Off1 = new byte[3] { 79, 102, 102 };

	public static readonly byte[] Off2 = new byte[3] { 79, 70, 70 };

	public static readonly byte[] NegInf0 = new byte[5] { 45, 46, 105, 110, 102 };

	public static readonly byte[] NegInf1 = new byte[5] { 45, 46, 73, 110, 102 };

	public static readonly byte[] NegInf2 = new byte[5] { 45, 46, 73, 78, 70 };

	public static readonly byte[] Nan0 = new byte[4] { 46, 110, 97, 110 };

	public static readonly byte[] Nan1 = new byte[4] { 46, 78, 97, 78 };

	public static readonly byte[] Nan2 = new byte[4] { 46, 78, 65, 78 };

	public static readonly byte[] HexPrefix = new byte[2] { 48, 120 };

	public static readonly byte[] HexPrefixNegative = new byte[3] { 45, 48, 120 };

	public static readonly byte[] OctalPrefix = new byte[2] { 48, 111 };

	public static readonly byte[] UnityStrippedSymbol = new byte[8] { 115, 116, 114, 105, 112, 112, 101, 100 };

	public const byte Space = 32;

	public const byte Tab = 9;

	public const byte Lf = 10;

	public const byte Cr = 13;

	public const byte Comment = 35;

	public const byte DirectiveLine = 37;

	public const byte Alias = 42;

	public const byte Anchor = 38;

	public const byte Tag = 33;

	public const byte SingleQuote = 39;

	public const byte DoubleQuote = 34;

	public const byte LiteralScalerHeader = 124;

	public const byte FoldedScalerHeader = 62;

	public const byte Comma = 44;

	public const byte BlockEntryIndent = 45;

	public const byte ExplicitKeyIndent = 63;

	public const byte MapValueIndent = 58;

	public const byte FlowMapStart = 123;

	public const byte FlowMapEnd = 125;

	public const byte FlowSequenceStart = 91;

	public const byte FlowSequenceEnd = 93;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAlphaNumericDashOrUnderscore(byte code)
	{
		switch (code)
		{
		case 45:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 95:
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
		case 109:
		case 110:
		case 111:
		case 112:
		case 113:
		case 114:
		case 115:
		case 116:
		case 117:
		case 118:
		case 119:
		case 120:
		case 121:
		case 122:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWordChar(byte code)
	{
		switch (code)
		{
		case 45:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
		case 109:
		case 110:
		case 111:
		case 112:
		case 113:
		case 114:
		case 115:
		case 116:
		case 117:
		case 118:
		case 119:
		case 120:
		case 121:
		case 122:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsUriChar(byte code)
	{
		switch (code)
		{
		case 33:
		case 35:
		case 36:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 61:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 91:
		case 93:
		case 95:
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
		case 109:
		case 110:
		case 111:
		case 112:
		case 113:
		case 114:
		case 115:
		case 116:
		case 117:
		case 118:
		case 119:
		case 120:
		case 121:
		case 122:
		case 126:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTagChar(byte code)
	{
		switch (code)
		{
		case 35:
		case 36:
		case 38:
		case 39:
		case 42:
		case 43:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 61:
		case 63:
		case 64:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
		case 95:
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
		case 109:
		case 110:
		case 111:
		case 112:
		case 113:
		case 114:
		case 115:
		case 116:
		case 117:
		case 118:
		case 119:
		case 120:
		case 121:
		case 122:
		case 126:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAscii(byte code)
	{
		return code <= 127;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNumber(byte code)
	{
		if (code >= 48)
		{
			return code <= 57;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty(byte code)
	{
		if (code != 32 && code != 9 && code != 10)
		{
			return code == 13;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLineBreak(byte code)
	{
		if (code != 10)
		{
			return code == 13;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBlank(byte code)
	{
		if (code != 32)
		{
			return code == 9;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNumberRepresentation(byte code)
	{
		switch (code)
		{
		case 43:
		case 45:
		case 46:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsHex(byte code)
	{
		switch (code)
		{
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsAnyFlowSymbol(byte code)
	{
		if (code != 44 && code != 91 && code != 93 && code != 123)
		{
			return code == 125;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte AsHex(byte code)
	{
		switch (code)
		{
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
			return (byte)(code - 48);
		case 97:
		case 98:
		case 99:
		case 100:
		case 101:
		case 102:
			return (byte)(code - 97 + 10);
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
			return (byte)(code - 65 + 10);
		default:
			throw new InvalidOperationException();
		}
	}
}
