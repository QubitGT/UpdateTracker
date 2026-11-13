using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace VYaml.Internal;

internal static class EmitStringAnalyzer
{
	[ThreadStatic]
	private static StringBuilder? stringBuilderThreadStatic;

	private static char[] whiteSpaces = new char[32]
	{
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
		' ', ' '
	};

	public static EmitStringInfo Analyze(string value)
	{
		ReadOnlySpan<char> readOnlySpan = MemoryExtensions.AsSpan(value);
		if (readOnlySpan.Length <= 0)
		{
			return new EmitStringInfo(0, needsQuotes: true, isReservedWord: false);
		}
		bool flag = IsReservedWord(value);
		char c = readOnlySpan[0];
		char c2 = readOnlySpan[readOnlySpan.Length - 1];
		bool needsQuotes = flag || c == ' ' || c2 == ' ' || c == '&' || c == '*' || c == '?' || c == '|' || c == '-' || c == '<' || c == '>' || c == '=' || c == '!' || c == '%' || c == '@' || c == '.';
		int num = 1;
		ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			switch (readOnlySpan2[i])
			{
			case '"':
			case '#':
			case '\'':
			case ',':
			case ':':
			case '[':
			case ']':
			case '`':
			case '{':
				needsQuotes = true;
				break;
			case '\n':
				num++;
				break;
			}
		}
		if (c2 == '\n')
		{
			num--;
		}
		return new EmitStringInfo(num, needsQuotes, flag);
	}

	internal static StringBuilder BuildLiteralScalar(ReadOnlySpan<char> originalValue, int indentCharCount)
	{
		char c = '\0';
		if (originalValue.Length > 0)
		{
			if (originalValue[originalValue.Length - 1] == '\n')
			{
				if (originalValue[originalValue.Length - 2] == '\n')
				{
					goto IL_005c;
				}
				if (originalValue[originalValue.Length - 2] == '\r')
				{
					if (originalValue[originalValue.Length - 3] == '\n')
					{
						goto IL_005c;
					}
				}
				goto IL_0064;
			}
		}
		c = '-';
		goto IL_0064;
		IL_005c:
		c = '+';
		goto IL_0064;
		IL_0064:
		StringBuilder stringBuilder = (stringBuilderThreadStatic ?? (stringBuilderThreadStatic = new StringBuilder(1024))).Clear();
		stringBuilder.Append('|');
		if (c > '\0')
		{
			stringBuilder.Append(c);
		}
		stringBuilder.Append('\n');
		AppendWhiteSpace(stringBuilder, indentCharCount);
		for (int i = 0; i < originalValue.Length; i++)
		{
			char c2 = originalValue[i];
			stringBuilder.Append(c2);
			if (c2 == '\n' && i < originalValue.Length - 1)
			{
				AppendWhiteSpace(stringBuilder, indentCharCount);
			}
		}
		if (c == '-')
		{
			stringBuilder.Append('\n');
		}
		return stringBuilder;
	}

	internal static StringBuilder BuildQuotedScalar(ReadOnlySpan<char> originalValue, bool doubleQuote = true)
	{
		StringBuilder stringBuilder = GetStringBuilder();
		char value = (doubleQuote ? '"' : '\'');
		stringBuilder.Append(value);
		ReadOnlySpan<char> readOnlySpan = originalValue;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			switch (c)
			{
			case '"':
				if (doubleQuote)
				{
					stringBuilder.Append("\\\"");
					continue;
				}
				break;
			case '\'':
				if (!doubleQuote)
				{
					stringBuilder.Append("\\'");
					continue;
				}
				break;
			case '\0':
				stringBuilder.Append("\\0");
				continue;
			case '\u0001':
				stringBuilder.Append("\\1");
				continue;
			case '\u0002':
				stringBuilder.Append("\\2");
				continue;
			case '\u0003':
				stringBuilder.Append("\\3");
				continue;
			case '\u0004':
				stringBuilder.Append("\\4");
				continue;
			case '\u0005':
				stringBuilder.Append("\\5");
				continue;
			case '\u0006':
				stringBuilder.Append("\\6");
				continue;
			case '\a':
				stringBuilder.Append("\\a");
				continue;
			case '\b':
				stringBuilder.Append("\\b");
				continue;
			case '\t':
				stringBuilder.Append("\\t");
				continue;
			case '\n':
				stringBuilder.Append("\\n");
				continue;
			case '\v':
				stringBuilder.Append("\\v");
				continue;
			case '\f':
				stringBuilder.Append("\\f");
				continue;
			case '\r':
				stringBuilder.Append("\\r");
				continue;
			case '\u000e':
				stringBuilder.Append("\\r");
				continue;
			case '\\':
				stringBuilder.Append("\\\\");
				continue;
			case '\u0085':
				stringBuilder.Append("\\N");
				continue;
			case '\u00a0':
				stringBuilder.Append("\\_");
				continue;
			case '\u2028':
				stringBuilder.Append("\\L");
				continue;
			case '\u2029':
				stringBuilder.Append("\\P");
				continue;
			case '\u000f':
				stringBuilder.Append("\\u000f");
				continue;
			case '\u0010':
				stringBuilder.Append("\\u0010");
				continue;
			case '\u0011':
				stringBuilder.Append("\\u0011");
				continue;
			case '\u0012':
				stringBuilder.Append("\\u0012");
				continue;
			case '\u0013':
				stringBuilder.Append("\\u0013");
				continue;
			case '\u0014':
				stringBuilder.Append("\\u0014");
				continue;
			case '\u0015':
				stringBuilder.Append("\\u0015");
				continue;
			case '\u0016':
				stringBuilder.Append("\\u0016");
				continue;
			case '\u0017':
				stringBuilder.Append("\\u0017");
				continue;
			case '\u0018':
				stringBuilder.Append("\\u0018");
				continue;
			case '\u0019':
				stringBuilder.Append("\\u0019");
				continue;
			case '\u001a':
				stringBuilder.Append("\\u001a");
				continue;
			case '\u001b':
				stringBuilder.Append("\\u001b");
				continue;
			case '\u001c':
				stringBuilder.Append("\\u001c");
				continue;
			case '\u001d':
				stringBuilder.Append("\\u001d");
				continue;
			case '\u001e':
				stringBuilder.Append("\\u001e");
				continue;
			case '\u001f':
				stringBuilder.Append("\\u001f");
				continue;
			case '\u007f':
				stringBuilder.Append("\\u007F");
				continue;
			}
			stringBuilder.Append(c);
		}
		stringBuilder.Append(value);
		return stringBuilder;
	}

	private static bool IsReservedWord(string value)
	{
		new StringBuilder().Append('\n');
		switch (value.Length)
		{
		case 1:
			if (value == "~")
			{
				return true;
			}
			break;
		case 4:
			switch (value)
			{
			case "null":
			case "Null":
			case "NULL":
			case "true":
			case "True":
			case "TRUE":
				return true;
			}
			break;
		case 5:
			switch (value)
			{
			case "false":
			case "False":
			case "FALSE":
				return true;
			}
			break;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static StringBuilder GetStringBuilder()
	{
		return (stringBuilderThreadStatic ?? (stringBuilderThreadStatic = new StringBuilder(1024))).Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AppendWhiteSpace(StringBuilder stringBuilder, int length)
	{
		if (length > whiteSpaces.Length)
		{
			whiteSpaces = Enumerable.Repeat(' ', length * 2).ToArray();
		}
		stringBuilder.Append(MemoryExtensions.AsSpan(whiteSpaces, 0, length));
	}
}
