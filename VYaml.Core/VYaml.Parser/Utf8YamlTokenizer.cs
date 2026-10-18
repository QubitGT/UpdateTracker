using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml.Parser;

public ref struct Utf8YamlTokenizer
{
	[ThreadStatic]
	private static InsertionQueue<Token>? tokensBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<SimpleKeyState>? simpleKeyBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<int>? indentsBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<byte>? lineBreaksBufferStatic;

	private SequenceReader<byte> reader;

	private Marker mark;

	private Token currentToken;

	private bool streamStartProduced;

	private bool streamEndProduced;

	private byte currentCode;

	private int indent;

	private bool simpleKeyAllowed;

	private int adjacentValueAllowedAt;

	private int flowLevel;

	private int tokensParsed;

	private bool tokenAvailable;

	private readonly InsertionQueue<Token> tokens;

	private readonly ExpandBuffer<SimpleKeyState> simpleKeyCandidates;

	private readonly ExpandBuffer<int> indents;

	public TokenType CurrentTokenType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return currentToken.Type;
		}
	}

	public Marker CurrentMark
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return mark;
		}
	}

	public Utf8YamlTokenizer(ReadOnlySequence<byte> sequence)
	{
		reader = new SequenceReader<byte>(sequence);
		mark = new Marker(0, 1, 0);
		indent = -1;
		flowLevel = 0;
		adjacentValueAllowedAt = 0;
		tokensParsed = 0;
		simpleKeyAllowed = false;
		streamStartProduced = false;
		streamEndProduced = false;
		tokenAvailable = false;
		currentToken = default(Token);
		tokens = tokensBufferStatic ?? (tokensBufferStatic = new InsertionQueue<Token>(16));
		tokens.Clear();
		simpleKeyCandidates = simpleKeyBufferStatic ?? (simpleKeyBufferStatic = new ExpandBuffer<SimpleKeyState>(16));
		simpleKeyCandidates.Clear();
		indents = indentsBufferStatic ?? (indentsBufferStatic = new ExpandBuffer<int>(16));
		indents.Clear();
		reader.TryPeek(out currentCode);
	}

	public bool Read()
	{
		if (streamEndProduced)
		{
			return false;
		}
		if (!tokenAvailable)
		{
			ConsumeMoreTokens();
		}
		if (currentToken.Content is Scalar scalar)
		{
			ScalarPool.Shared.Return(scalar);
		}
		currentToken = tokens.Dequeue();
		tokenAvailable = false;
		tokensParsed++;
		if (currentToken.Type == TokenType.StreamEnd)
		{
			streamEndProduced = true;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal T TakeCurrentTokenContent<T>() where T : ITokenContent
	{
		Token token = currentToken;
		currentToken = default(Token);
		return (T)token.Content;
	}

	internal bool TrySkipUnityStrippedSymbol()
	{
		while (currentCode == 32)
		{
			Advance(1);
		}
		if (reader.IsNext(YamlCodes.UnityStrippedSymbol))
		{
			Advance(YamlCodes.UnityStrippedSymbol.Length);
			return true;
		}
		return false;
	}

	private void ConsumeMoreTokens()
	{
		while (true)
		{
			bool flag = tokens.Count <= 0;
			if (!flag)
			{
				StaleSimpleKeyCandidates();
				for (int i = 0; i < simpleKeyCandidates.Length; i++)
				{
					ref SimpleKeyState reference = ref simpleKeyCandidates[i];
					if (reference.Possible && reference.TokenNumber == tokensParsed)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				break;
			}
			ConsumeNextToken();
		}
		tokenAvailable = true;
	}

	private void ConsumeNextToken()
	{
		if (!streamStartProduced)
		{
			ConsumeStreamStart();
			return;
		}
		SkipToNextToken();
		StaleSimpleKeyCandidates();
		UnrollIndent(mark.Col);
		if (reader.End)
		{
			ConsumeStreamEnd();
			return;
		}
		if (mark.Col == 0)
		{
			switch (currentCode)
			{
			case 37:
				ConsumeDirective();
				return;
			case 45:
				if (reader.IsNext(YamlCodes.StreamStart) && IsEmptyNext(YamlCodes.StreamStart.Length))
				{
					ConsumeDocumentIndicator(TokenType.DocumentStart);
					return;
				}
				break;
			case 46:
				if (reader.IsNext(YamlCodes.DocStart) && IsEmptyNext(YamlCodes.DocStart.Length))
				{
					ConsumeDocumentIndicator(TokenType.DocumentEnd);
					return;
				}
				break;
			}
		}
		byte value;
		switch (currentCode)
		{
		case 91:
			ConsumeFlowCollectionStart(TokenType.FlowSequenceStart);
			return;
		case 123:
			ConsumeFlowCollectionStart(TokenType.FlowMappingStart);
			return;
		case 93:
			ConsumeFlowCollectionEnd(TokenType.FlowSequenceEnd);
			return;
		case 125:
			ConsumeFlowCollectionEnd(TokenType.FlowMappingEnd);
			return;
		case 44:
			ConsumeFlowEntryStart();
			return;
		case 45:
		{
			if (!TryPeek(1L, out var value3) || YamlCodes.IsEmpty(value3))
			{
				ConsumeBlockEntry();
				return;
			}
			if (!TryPeek(1L, out var value4) || YamlCodes.IsBlank(value4))
			{
				ConsumePlainScalar();
				return;
			}
			break;
		}
		case 63:
		{
			if (!TryPeek(1L, out var value5) || YamlCodes.IsEmpty(value5))
			{
				ConsumeComplexKeyStart();
				return;
			}
			goto IL_0278;
		}
		case 58:
		{
			if ((TryPeek(1L, out var value2) && YamlCodes.IsEmpty(value2)) || (flowLevel > 0 && (YamlCodes.IsAnyFlowSymbol(value2) || mark.Position == adjacentValueAllowedAt)))
			{
				ConsumeValueStart();
				return;
			}
			goto IL_0278;
		}
		case 42:
			ConsumeAnchor(alias: true);
			return;
		case 38:
			ConsumeAnchor(alias: false);
			return;
		case 33:
			ConsumeTag();
			return;
		case 124:
			if (flowLevel == 0)
			{
				ConsumeBlockScaler(literal: true);
				return;
			}
			break;
		case 62:
			if (flowLevel == 0)
			{
				ConsumeBlockScaler(literal: false);
				return;
			}
			break;
		case 39:
			ConsumeFlowScaler(singleQuote: true);
			return;
		case 34:
			ConsumeFlowScaler(singleQuote: false);
			return;
		case 37:
		case 64:
		case 96:
			{
				throw new YamlTokenizerException(in mark, $"Unexpected character: '{currentCode}'");
			}
			IL_0278:
			if (flowLevel == 0 && (!TryPeek(1L, out value) || YamlCodes.IsBlank(value)))
			{
				ConsumePlainScalar();
				return;
			}
			break;
		}
		ConsumePlainScalar();
	}

	private void ConsumeStreamStart()
	{
		indent = -1;
		streamStartProduced = true;
		simpleKeyAllowed = true;
		tokens.Enqueue(new Token(TokenType.StreamStart));
		simpleKeyCandidates.Add(default(SimpleKeyState));
	}

	private void ConsumeStreamEnd()
	{
		if (mark.Col != 0)
		{
			mark.Col = 0;
			mark.Line++;
		}
		UnrollIndent(-1);
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		tokens.Enqueue(new Token(TokenType.StreamEnd));
	}

	private void ConsumeBom()
	{
		if (reader.IsNext(YamlCodes.Utf8Bom))
		{
			bool num = mark.Position == 0;
			Advance(YamlCodes.Utf8Bom.Length);
			mark.Col = 0;
			if (!num && CurrentTokenType != TokenType.DocumentEnd && (tokens.Count <= 0 || tokens.Peek().Type != TokenType.DocumentEnd) && !reader.IsNext(YamlCodes.DocStart))
			{
				Marker marker = CurrentMark;
				throw new YamlTokenizerException(in marker, "BOM must be at the beginning of the stream or document.");
			}
		}
	}

	private void ConsumeDirective()
	{
		UnrollIndent(-1);
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		Advance(1);
		Scalar scalar = ScalarPool.Shared.Rent();
		try
		{
			ConsumeDirectiveName(scalar);
			if (scalar.SequenceEqual(YamlCodes.YamlDirectiveName))
			{
				ConsumeVersionDirectiveValue();
			}
			else if (scalar.SequenceEqual(YamlCodes.TagDirectiveName))
			{
				ConsumeTagDirectiveValue();
			}
			else
			{
				while (!reader.End && !YamlCodes.IsLineBreak(currentCode))
				{
					Advance(1);
				}
				tokens.Enqueue(new Token(TokenType.TagDirective));
			}
		}
		finally
		{
			ScalarPool.Shared.Return(scalar);
		}
		while (YamlCodes.IsBlank(currentCode))
		{
			Advance(1);
		}
		if (currentCode == 35)
		{
			while (!reader.End && !YamlCodes.IsLineBreak(currentCode))
			{
				Advance(1);
			}
		}
		if (!reader.End && !YamlCodes.IsLineBreak(currentCode))
		{
			Marker marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "While scanning a directive, did not find expected comment or line break");
		}
		if (YamlCodes.IsLineBreak(currentCode))
		{
			ConsumeLineBreaks();
		}
	}

	private void ConsumeDirectiveName(Scalar result)
	{
		while (YamlCodes.IsAlphaNumericDashOrUnderscore(currentCode))
		{
			result.Write(currentCode);
			Advance(1);
		}
		if (result.Length <= 0)
		{
			Marker marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "While scanning a directive, could not find expected directive name");
		}
		if (!reader.End && !YamlCodes.IsBlank(currentCode))
		{
			Marker marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "While scanning a directive, found unexpected non-alphabetical character");
		}
	}

	private void ConsumeVersionDirectiveValue()
	{
		while (YamlCodes.IsBlank(currentCode))
		{
			Advance(1);
		}
		int major = ConsumeVersionDirectiveNumber();
		if (currentCode != 46)
		{
			Marker marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "while scanning a YAML directive, did not find expected digit or '.' character");
		}
		Advance(1);
		int minor = ConsumeVersionDirectiveNumber();
		tokens.Enqueue(new Token(TokenType.VersionDirective, new VersionDirective(major, minor)));
	}

	private int ConsumeVersionDirectiveNumber()
	{
		int num = 0;
		int num2 = 0;
		while (YamlCodes.IsNumber(currentCode))
		{
			if (num2 + 1 > 9)
			{
				Marker marker = CurrentMark;
				throw new YamlTokenizerException(in marker, "While scanning a YAML directive, found exteremely long version number");
			}
			num2++;
			num = num * 10 + YamlCodes.AsHex(currentCode);
			Advance(1);
		}
		if (num2 == 0)
		{
			Marker marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "While scanning a YAML directive, did not find expected version number");
		}
		return num;
	}

	private void ConsumeTagDirectiveValue()
	{
		Scalar scalar = ScalarPool.Shared.Rent();
		Scalar scalar2 = ScalarPool.Shared.Rent();
		try
		{
			while (YamlCodes.IsBlank(currentCode))
			{
				Advance(1);
			}
			ConsumeTagHandle(directive: true, scalar);
			Marker marker;
			if (!YamlCodes.IsBlank(currentCode))
			{
				marker = CurrentMark;
				throw new YamlTokenizerException(in marker, "While scanning a TAG directive, did not find expected whitespace after tag handle.");
			}
			while (YamlCodes.IsBlank(currentCode))
			{
				Advance(1);
			}
			ConsumeTagPrefix(scalar2);
			if (YamlCodes.IsEmpty(currentCode) || reader.End)
			{
				tokens.Enqueue(new Token(TokenType.TagDirective, new Tag(scalar.ToString(), scalar2.ToString())));
				return;
			}
			marker = CurrentMark;
			throw new YamlTokenizerException(in marker, "While scanning TAG, did not find expected whitespace or line break");
		}
		finally
		{
			ScalarPool.Shared.Return(scalar);
			ScalarPool.Shared.Return(scalar2);
		}
	}

	private void ConsumeDocumentIndicator(TokenType tokenType)
	{
		UnrollIndent(-1);
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		Advance(3);
		tokens.Enqueue(new Token(tokenType));
	}

	private void ConsumeFlowCollectionStart(TokenType tokenType)
	{
		SaveSimpleKeyCandidate();
		IncreaseFlowLevel();
		simpleKeyAllowed = true;
		Advance(1);
		tokens.Enqueue(new Token(tokenType));
	}

	private void ConsumeFlowCollectionEnd(TokenType tokenType)
	{
		RemoveSimpleKeyCandidate();
		DecreaseFlowLevel();
		simpleKeyAllowed = false;
		Advance(1);
		tokens.Enqueue(new Token(tokenType));
	}

	private void ConsumeFlowEntryStart()
	{
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = true;
		Advance(1);
		tokens.Enqueue(new Token(TokenType.FlowEntryStart));
	}

	private void ConsumeBlockEntry()
	{
		if (flowLevel != 0)
		{
			throw new YamlTokenizerException(in mark, "'-' is only valid inside a block");
		}
		if (!simpleKeyAllowed)
		{
			throw new YamlTokenizerException(in mark, "Block sequence entries are not allowed in this context");
		}
		int col = mark.Col;
		Token nextToken = new Token(TokenType.BlockSequenceStart);
		RollIndent(col, in nextToken);
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = true;
		Advance(1);
		tokens.Enqueue(new Token(TokenType.BlockEntryStart));
	}

	private void ConsumeComplexKeyStart()
	{
		if (flowLevel == 0)
		{
			if (!simpleKeyAllowed)
			{
				throw new YamlTokenizerException(in mark, "Mapping keys are not allowed in this context");
			}
			int col = mark.Col;
			Token nextToken = new Token(TokenType.BlockMappingStart);
			RollIndent(col, in nextToken);
		}
		RemoveSimpleKeyCandidate();
		simpleKeyAllowed = flowLevel == 0;
		Advance(1);
		tokens.Enqueue(new Token(TokenType.KeyStart));
	}

	private void ConsumeValueStart()
	{
		ExpandBuffer<SimpleKeyState> expandBuffer = simpleKeyCandidates;
		ref SimpleKeyState reference = ref expandBuffer[expandBuffer.Length - 1];
		if (reference.Possible)
		{
			Token item = new Token(TokenType.KeyStart);
			tokens.Insert(reference.TokenNumber - tokensParsed, item);
			int col = reference.Start.Col;
			Token nextToken = new Token(TokenType.BlockMappingStart);
			RollIndent(col, in nextToken, reference.TokenNumber);
			ExpandBuffer<SimpleKeyState> expandBuffer2 = simpleKeyCandidates;
			expandBuffer2[expandBuffer2.Length - 1].Possible = false;
			simpleKeyAllowed = false;
		}
		else
		{
			if (flowLevel == 0)
			{
				if (!simpleKeyAllowed)
				{
					throw new YamlTokenizerException(in mark, "Mapping values are not allowed in this context");
				}
				int col2 = mark.Col;
				Token nextToken = new Token(TokenType.BlockMappingStart);
				RollIndent(col2, in nextToken);
			}
			simpleKeyAllowed = flowLevel == 0;
		}
		Advance(1);
		tokens.Enqueue(new Token(TokenType.ValueStart));
	}

	private void ConsumeAnchor(bool alias)
	{
		SaveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		Scalar scalar = ScalarPool.Shared.Rent();
		Advance(1);
		while (YamlCodes.IsAlphaNumericDashOrUnderscore(currentCode))
		{
			scalar.Write(currentCode);
			Advance(1);
		}
		if (scalar.Length <= 0)
		{
			throw new YamlTokenizerException(in mark, "while scanning an anchor or alias, did not find expected alphabetic or numeric character");
		}
		if (!YamlCodes.IsEmpty(currentCode) && !reader.End && currentCode != 63 && currentCode != 58 && currentCode != 44 && currentCode != 93 && currentCode != 125 && currentCode != 37 && currentCode != 64 && currentCode != 96)
		{
			throw new YamlTokenizerException(in mark, "while scanning an anchor or alias, did not find expected alphabetic or numeric character");
		}
		tokens.Enqueue(alias ? new Token(TokenType.Alias, scalar) : new Token(TokenType.Anchor, scalar));
	}

	private void ConsumeTag()
	{
		SaveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		Scalar scalar = ScalarPool.Shared.Rent();
		Scalar scalar2 = ScalarPool.Shared.Rent();
		try
		{
			if (TryPeek(1L, out var value) && value == 60)
			{
				Advance(2);
				while (TryConsumeUriChar(scalar2))
				{
				}
				if (scalar2.Length <= 0)
				{
					throw new YamlTokenizerException(in mark, "While scanning a verbatim tag, did not find valid characters.");
				}
				if (currentCode != 62)
				{
					throw new YamlTokenizerException(in mark, "While scanning a tag, did not find the expected '>'");
				}
				Advance(1);
			}
			else
			{
				ConsumeTagHandle(directive: false, scalar);
				if (scalar.Length >= 2)
				{
					Span<byte> span = scalar.AsSpan();
					if (span[span.Length - 1] == 33)
					{
						while (TryConsumeTagChar(scalar2))
						{
						}
						if (scalar2.Length <= 0)
						{
							throw new YamlTokenizerException(in mark, "While scanning a tag, did not find any tag-shorthand suffix.");
						}
						goto IL_0116;
					}
				}
				scalar2.Write(scalar.AsSpan(1, scalar.Length - 1));
				scalar.Clear();
				scalar.Write(33);
				while (TryConsumeTagChar(scalar2))
				{
				}
				if (scalar2.Length <= 0)
				{
					Scalar scalar3 = scalar2;
					Scalar scalar4 = scalar;
					scalar = scalar3;
					scalar2 = scalar4;
				}
			}
			goto IL_0116;
			IL_0116:
			if (YamlCodes.IsEmpty(currentCode) || reader.End || YamlCodes.IsAnyFlowSymbol(currentCode))
			{
				tokens.Enqueue(new Token(TokenType.Tag, new Tag(scalar.ToString(), scalar2.ToString())));
				return;
			}
			throw new YamlTokenizerException(in mark, "While scanning a tag, did not find expected whitespace or line break or flow");
		}
		finally
		{
			ScalarPool.Shared.Return(scalar);
			ScalarPool.Shared.Return(scalar2);
		}
	}

	private void ConsumeTagHandle(bool directive, Scalar buf)
	{
		if (currentCode != 33)
		{
			throw new YamlTokenizerException(in mark, "While scanning a tag, did not find expected '!'");
		}
		buf.Write(currentCode);
		Advance(1);
		while (YamlCodes.IsWordChar(currentCode))
		{
			buf.Write(currentCode);
			Advance(1);
		}
		if (currentCode == 33)
		{
			buf.Write(currentCode);
			Advance(1);
		}
		else if (directive)
		{
			Span<byte> span = stackalloc byte[1] { 33 };
			if (!buf.SequenceEqual(span))
			{
				throw new YamlTokenizerException(in mark, "While parsing a tag directive, did not find expected '!'");
			}
		}
	}

	private void ConsumeTagPrefix(Scalar prefix)
	{
		if (currentCode == 33)
		{
			prefix.Write(currentCode);
			Advance(1);
			while (TryConsumeUriChar(prefix))
			{
			}
			return;
		}
		if (YamlCodes.IsTagChar(currentCode))
		{
			prefix.Write(currentCode);
			Advance(1);
			while (TryConsumeUriChar(prefix))
			{
			}
			return;
		}
		throw new YamlTokenizerException(in mark, "While parsing a tag, did not find expected tag prefix");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryConsumeUriChar(Scalar scalar)
	{
		if (currentCode == 37)
		{
			scalar.WriteUnicodeCodepoint(ConsumeUriEscapes());
			return true;
		}
		if (YamlCodes.IsUriChar(currentCode))
		{
			scalar.Write(currentCode);
			Advance(1);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryConsumeTagChar(Scalar scalar)
	{
		if (currentCode == 37)
		{
			scalar.WriteUnicodeCodepoint(ConsumeUriEscapes());
			return true;
		}
		if (YamlCodes.IsTagChar(currentCode))
		{
			scalar.Write(currentCode);
			Advance(1);
			return true;
		}
		return false;
	}

	private int ConsumeUriEscapes()
	{
		int num = 0;
		int result = 0;
		while (!reader.End)
		{
			TryPeek(1L, out var value);
			TryPeek(2L, out var value2);
			if (currentCode != 37 || !YamlCodes.IsHex(value) || !YamlCodes.IsHex(value2))
			{
				throw new YamlTokenizerException(in mark, "While parsing a tag, did not find URI escaped octet");
			}
			int num2 = (YamlCodes.AsHex(value) << 4) + YamlCodes.AsHex(value2);
			if (num == 0)
			{
				int num3;
				if ((num2 & 0x80) == 0)
				{
					num3 = 1;
				}
				else if ((num2 & 0xE0) == 192)
				{
					num3 = 2;
				}
				else if ((num2 & 0xF0) == 224)
				{
					num3 = 3;
				}
				else
				{
					if ((num2 & 0xF8) != 240)
					{
						throw new YamlTokenizerException(in mark, "While parsing a tag, found an incorrect leading utf8 octet");
					}
					num3 = 4;
				}
				num = num3;
				result = num2;
			}
			else
			{
				if ((num2 & 0xC0) != 128)
				{
					throw new YamlTokenizerException(in mark, "While parsing a tag, found an incorrect trailing utf8 octet");
				}
				result = (currentCode << 8) + num2;
			}
			Advance(3);
			num--;
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	private void ConsumeBlockScaler(bool literal)
	{
		SaveSimpleKeyCandidate();
		simpleKeyAllowed = true;
		int num = 0;
		int num2 = 0;
		int blockIndent = 0;
		bool flag = false;
		bool flag2 = false;
		LineBreakState lineBreakState = LineBreakState.None;
		Scalar scalar = ScalarPool.Shared.Rent();
		if (lineBreaksBufferStatic == null)
		{
			lineBreaksBufferStatic = new ExpandBuffer<byte>(64);
		}
		lineBreaksBufferStatic.Clear();
		Advance(1);
		byte b = currentCode;
		if (b == 43 || b == 45)
		{
			num = ((currentCode == 43) ? 1 : (-1));
			Advance(1);
			if (YamlCodes.IsNumber(currentCode))
			{
				if (currentCode == 48)
				{
					throw new YamlTokenizerException(in mark, "While scanning a block scalar, found an indentation indicator equal to 0");
				}
				num2 = YamlCodes.AsHex(currentCode);
				Advance(1);
			}
		}
		else if (YamlCodes.IsNumber(currentCode))
		{
			if (currentCode == 48)
			{
				throw new YamlTokenizerException(in mark, "While scanning a block scalar, found an indentation indicator equal to 0");
			}
			num2 = YamlCodes.AsHex(currentCode);
			Advance(1);
			b = currentCode;
			if (b == 43 || b == 45)
			{
				num = ((currentCode == 43) ? 1 : (-1));
				Advance(1);
			}
		}
		while (YamlCodes.IsBlank(currentCode))
		{
			Advance(1);
		}
		if (currentCode == 35)
		{
			while (!reader.End && !YamlCodes.IsLineBreak(currentCode))
			{
				Advance(1);
			}
		}
		if (!reader.End && !YamlCodes.IsLineBreak(currentCode))
		{
			throw new YamlTokenizerException(in mark, "While scanning a block scalar, did not find expected commnet or line break");
		}
		if (YamlCodes.IsLineBreak(currentCode))
		{
			ConsumeLineBreaks();
		}
		if (num2 > 0)
		{
			blockIndent = ((indent >= 0) ? (indent + num2) : num2);
		}
		ConsumeBlockScalarBreaks(ref blockIndent, ref lineBreaksBufferStatic);
		while (mark.Col == blockIndent)
		{
			flag = YamlCodes.IsBlank(currentCode);
			if (!literal && lineBreakState != 0 && !flag2 && !flag)
			{
				if (lineBreaksBufferStatic.Length <= 0)
				{
					scalar.Write(32);
				}
			}
			else
			{
				scalar.Write(lineBreakState);
			}
			scalar.Write(lineBreaksBufferStatic.AsSpan());
			flag2 = YamlCodes.IsBlank(currentCode);
			lineBreaksBufferStatic.Clear();
			while (!reader.End && !YamlCodes.IsLineBreak(currentCode))
			{
				scalar.Write(currentCode);
				Advance(1);
			}
			if (reader.End)
			{
				lineBreakState = LineBreakState.Lf;
				break;
			}
			lineBreakState = ConsumeLineBreaks();
			ConsumeBlockScalarBreaks(ref blockIndent, ref lineBreaksBufferStatic);
		}
		if (num != -1)
		{
			scalar.Write(lineBreakState);
		}
		if (num == 1)
		{
			scalar.Write(lineBreaksBufferStatic.AsSpan());
		}
		TokenType type = (literal ? TokenType.LiteralScalar : TokenType.FoldedScalar);
		tokens.Enqueue(new Token(type, scalar));
	}

	private void ConsumeBlockScalarBreaks(ref int blockIndent, ref ExpandBuffer<byte> blockLineBreaks)
	{
		int num = 0;
		while (true)
		{
			if ((blockIndent == 0 || mark.Col < blockIndent) && currentCode == 32)
			{
				Advance(1);
				continue;
			}
			if (mark.Col > num)
			{
				num = mark.Col;
			}
			if ((blockIndent == 0 || mark.Col < blockIndent) && currentCode == 9)
			{
				throw new YamlTokenizerException(in mark, "while scanning a block scalar, found a tab character where an indentation space is expected");
			}
			if (!YamlCodes.IsLineBreak(currentCode))
			{
				break;
			}
			switch (ConsumeLineBreaks())
			{
			case LineBreakState.Lf:
				blockLineBreaks.Add(10);
				break;
			case LineBreakState.CrLf:
				blockLineBreaks.Add(13);
				blockLineBreaks.Add(10);
				break;
			case LineBreakState.Cr:
				blockLineBreaks.Add(13);
				break;
			}
		}
		if (blockIndent == 0)
		{
			blockIndent = num;
			if (blockIndent < indent + 1)
			{
				blockIndent = indent + 1;
			}
			else if (blockIndent < 1)
			{
				blockIndent = 1;
			}
		}
	}

	private void ConsumeFlowScaler(bool singleQuote)
	{
		SaveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		LineBreakState lineBreakState = LineBreakState.None;
		LineBreakState lineBreakState2 = LineBreakState.None;
		bool flag = false;
		Scalar scalar = ScalarPool.Shared.Rent();
		Span<byte> span = stackalloc byte[32];
		int num = 0;
		Advance(1);
		while (true)
		{
			if (mark.Col == 0 && (reader.IsNext(YamlCodes.StreamStart) || reader.IsNext(YamlCodes.DocStart)) && !TryPeek(3L, out var value))
			{
				throw new YamlTokenizerException(in mark, "while scanning a quoted scalar, found unexpected document indicator");
			}
			if (reader.End)
			{
				break;
			}
			flag = false;
			while (!reader.End && !YamlCodes.IsEmpty(currentCode))
			{
				value = currentCode;
				if (value != 34)
				{
					if (value != 39)
					{
						if (value == 92)
						{
							if (!singleQuote && TryPeek(1L, out var value2) && YamlCodes.IsLineBreak(value2))
							{
								Advance(1);
								ConsumeLineBreaks();
								flag = true;
								continue;
							}
							if (!singleQuote)
							{
								int num2 = 0;
								TryPeek(1L, out var value3);
								switch (value3)
								{
								case 48:
									scalar.Write((byte)0);
									break;
								case 97:
									scalar.Write(7);
									break;
								case 98:
									scalar.Write(8);
									break;
								case 116:
									scalar.Write(9);
									break;
								case 110:
									scalar.Write(10);
									break;
								case 118:
									scalar.Write(11);
									break;
								case 102:
									scalar.Write(12);
									break;
								case 114:
									scalar.Write(13);
									break;
								case 101:
									scalar.Write(27);
									break;
								case 32:
									scalar.Write(32);
									break;
								case 34:
									scalar.Write(34);
									break;
								case 39:
									scalar.Write(39);
									break;
								case 92:
									scalar.Write(92);
									break;
								case 78:
									scalar.WriteUnicodeCodepoint(133);
									break;
								case 95:
									scalar.WriteUnicodeCodepoint(160);
									break;
								case 76:
									scalar.WriteUnicodeCodepoint(8232);
									break;
								case 80:
									scalar.WriteUnicodeCodepoint(8233);
									break;
								case 120:
									num2 = 2;
									break;
								case 117:
									num2 = 4;
									break;
								case 85:
									num2 = 8;
									break;
								default:
									throw new YamlTokenizerException(in mark, "while parsing a quoted scalar, found unknown escape character");
								}
								Advance(2);
								if (num2 > 0)
								{
									int num3 = 0;
									for (int i = 0; i < num2; i++)
									{
										if (TryPeek(i, out var value4) && YamlCodes.IsHex(value4))
										{
											num3 = (num3 << 4) + YamlCodes.AsHex(value4);
											continue;
										}
										throw new YamlTokenizerException(in mark, "While parsing a quoted scalar, did not find expected hexadecimal number");
									}
									scalar.WriteUnicodeCodepoint(num3);
								}
								Advance(num2);
								continue;
							}
						}
					}
					else
					{
						if (TryPeek(1L, out var value5) && value5 == 39 && singleQuote)
						{
							scalar.Write(39);
							Advance(2);
							continue;
						}
						if (singleQuote)
						{
							goto IL_048d;
						}
					}
				}
				else if (!singleQuote)
				{
					goto IL_048d;
				}
				scalar.Write(currentCode);
				Advance(1);
				continue;
				IL_048d:
				Advance(1);
				simpleKeyAllowed = flag;
				adjacentValueAllowedAt = mark.Position;
				tokens.Enqueue(new Token(singleQuote ? TokenType.SingleQuotedScaler : TokenType.DoubleQuotedScaler, scalar));
				return;
			}
			while (YamlCodes.IsBlank(currentCode) || YamlCodes.IsLineBreak(currentCode))
			{
				if (YamlCodes.IsBlank(currentCode))
				{
					if (!flag)
					{
						if (span.Length <= num)
						{
							span = new byte[span.Length * 2];
						}
						span[num++] = currentCode;
					}
					Advance(1);
				}
				else if (flag)
				{
					lineBreakState2 = ConsumeLineBreaks();
				}
				else
				{
					num = 0;
					lineBreakState = ConsumeLineBreaks();
					flag = true;
				}
			}
			if (flag)
			{
				if (lineBreakState == LineBreakState.None)
				{
					scalar.Write(lineBreakState2);
					lineBreakState2 = LineBreakState.None;
					continue;
				}
				if (lineBreakState2 == LineBreakState.None)
				{
					scalar.Write(32);
				}
				else
				{
					scalar.Write(lineBreakState2);
					lineBreakState2 = LineBreakState.None;
				}
				lineBreakState = LineBreakState.None;
			}
			else
			{
				Span<byte> span2 = span;
				scalar.Write(span2.Slice(0, num));
				num = 0;
			}
		}
		throw new YamlTokenizerException(in mark, "while scanning a quoted scalar, found unexpected end of stream");
	}

	private void ConsumePlainScalar()
	{
		SaveSimpleKeyCandidate();
		simpleKeyAllowed = false;
		int num = indent + 1;
		LineBreakState lineBreakState = LineBreakState.None;
		LineBreakState lineBreakState2 = LineBreakState.None;
		bool flag = false;
		Scalar scalar = ScalarPool.Shared.Rent();
		Span<byte> span = stackalloc byte[16];
		int num2 = 0;
		while ((mark.Col != 0 || ((currentCode != 45 || !reader.IsNext(YamlCodes.StreamStart) || !IsEmptyNext(YamlCodes.StreamStart.Length)) && (currentCode != 46 || !reader.IsNext(YamlCodes.DocStart) || !IsEmptyNext(YamlCodes.DocStart.Length)))) && currentCode != 35)
		{
			while (!reader.End && !YamlCodes.IsEmpty(currentCode))
			{
				if (currentCode == 58)
				{
					if (!TryPeek(1L, out var value) || YamlCodes.IsEmpty(value) || (flowLevel > 0 && YamlCodes.IsAnyFlowSymbol(value)))
					{
						break;
					}
				}
				else if (flowLevel > 0 && YamlCodes.IsAnyFlowSymbol(currentCode))
				{
					break;
				}
				if (flag || num2 > 0)
				{
					if (flag)
					{
						if (lineBreakState == LineBreakState.None)
						{
							scalar.Write(lineBreakState2);
							lineBreakState2 = LineBreakState.None;
						}
						else
						{
							if (lineBreakState2 == LineBreakState.None)
							{
								scalar.Write(32);
							}
							else
							{
								scalar.Write(lineBreakState2);
								lineBreakState2 = LineBreakState.None;
							}
							lineBreakState = LineBreakState.None;
						}
						flag = false;
					}
					else
					{
						Span<byte> span2 = span;
						scalar.Write(span2.Slice(0, num2));
						num2 = 0;
					}
				}
				scalar.Write(currentCode);
				Advance(1);
			}
			if (!YamlCodes.IsEmpty(currentCode))
			{
				break;
			}
			while (YamlCodes.IsEmpty(currentCode))
			{
				if (YamlCodes.IsBlank(currentCode))
				{
					if (flag && mark.Col < num && currentCode == 9)
					{
						throw new YamlTokenizerException(in mark, "While scanning a plain scaler, found a tab");
					}
					if (!flag)
					{
						if (num2 >= span.Length)
						{
							span = new byte[span.Length * 2];
						}
						span[num2++] = currentCode;
					}
					Advance(1);
				}
				else if (flag)
				{
					lineBreakState2 = ConsumeLineBreaks();
				}
				else
				{
					lineBreakState = ConsumeLineBreaks();
					flag = true;
					num2 = 0;
				}
			}
			if (flowLevel == 0 && mark.Col < num)
			{
				break;
			}
		}
		simpleKeyAllowed = flag;
		tokens.Enqueue(new Token(TokenType.PlainScalar, scalar));
	}

	private void SkipToNextToken()
	{
		while (true)
		{
			switch (currentCode)
			{
			case 32:
				Advance(1);
				break;
			case 9:
				if (flowLevel > 0 || !simpleKeyAllowed)
				{
					Advance(1);
					break;
				}
				return;
			case 10:
			case 13:
				ConsumeLineBreaks();
				if (flowLevel == 0)
				{
					simpleKeyAllowed = true;
				}
				break;
			case 35:
				while (!reader.End && !YamlCodes.IsLineBreak(currentCode))
				{
					Advance(1);
				}
				break;
			case 239:
				ConsumeBom();
				break;
			default:
				return;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Advance(int offset)
	{
		for (int i = 0; i < offset; i++)
		{
			mark.Position++;
			if (currentCode == 10)
			{
				mark.Line++;
				mark.Col = 0;
			}
			else
			{
				mark.Col++;
			}
			reader.Advance(1L);
			reader.TryPeek(out currentCode);
		}
	}

	private LineBreakState ConsumeLineBreaks()
	{
		if (reader.End)
		{
			return LineBreakState.None;
		}
		switch (currentCode)
		{
		case 13:
		{
			if (TryPeek(1L, out var value) && value == 10)
			{
				Advance(2);
				return LineBreakState.CrLf;
			}
			Advance(1);
			return LineBreakState.Cr;
		}
		case 10:
			Advance(1);
			return LineBreakState.Lf;
		default:
			return LineBreakState.None;
		}
	}

	private void StaleSimpleKeyCandidates()
	{
		for (int i = 0; i < simpleKeyCandidates.Length; i++)
		{
			ref SimpleKeyState reference = ref simpleKeyCandidates[i];
			if (reference.Possible && (reference.Start.Line < mark.Line || reference.Start.Position + 1024 < mark.Position))
			{
				if (reference.Required)
				{
					throw new YamlTokenizerException(in mark, "Simple key expect ':'");
				}
				reference.Possible = false;
			}
		}
	}

	private void SaveSimpleKeyCandidate()
	{
		if (simpleKeyAllowed)
		{
			ExpandBuffer<SimpleKeyState> expandBuffer = simpleKeyCandidates;
			SimpleKeyState simpleKeyState = expandBuffer[expandBuffer.Length - 1];
			if (simpleKeyState.Possible && simpleKeyState.Required)
			{
				throw new YamlTokenizerException(in mark, "Simple key expected");
			}
			ExpandBuffer<SimpleKeyState> expandBuffer2 = simpleKeyCandidates;
			expandBuffer2[expandBuffer2.Length - 1] = new SimpleKeyState
			{
				Start = mark,
				Possible = true,
				Required = (flowLevel > 0 && indent == mark.Col),
				TokenNumber = tokensParsed + tokens.Count
			};
		}
	}

	private void RemoveSimpleKeyCandidate()
	{
		ExpandBuffer<SimpleKeyState> expandBuffer = simpleKeyCandidates;
		ref SimpleKeyState reference = ref expandBuffer[expandBuffer.Length - 1];
		SimpleKeyState simpleKeyState = reference;
		if (simpleKeyState.Possible && simpleKeyState.Required)
		{
			throw new YamlTokenizerException(in mark, "Simple key expected");
		}
		reference.Possible = false;
	}

	private void RollIndent(int colTo, in Token nextToken, int insertNumber = -1)
	{
		if (flowLevel <= 0 && indent < colTo)
		{
			indents.Add(indent);
			indent = colTo;
			if (insertNumber >= 0)
			{
				tokens.Insert(insertNumber - tokensParsed, nextToken);
			}
			else
			{
				tokens.Enqueue(nextToken);
			}
		}
	}

	private void UnrollIndent(int col)
	{
		if (flowLevel <= 0)
		{
			while (indent > col)
			{
				tokens.Enqueue(new Token(TokenType.BlockEnd));
				indent = indents.Pop();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void IncreaseFlowLevel()
	{
		simpleKeyCandidates.Add(default(SimpleKeyState));
		flowLevel++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DecreaseFlowLevel()
	{
		if (flowLevel > 0)
		{
			flowLevel--;
			_ = ref simpleKeyCandidates.Pop();
		}
	}

	private readonly bool IsEmptyNext(int offset)
	{
		if (reader.End || reader.Remaining <= offset)
		{
			return true;
		}
		if (reader.CurrentSpanIndex + offset <= reader.CurrentSpan.Length - 1)
		{
			return YamlCodes.IsEmpty(reader.CurrentSpan[reader.CurrentSpanIndex + offset]);
		}
		int num = offset;
		SequencePosition position = reader.Position;
		ReadOnlyMemory<byte> memory;
		while (reader.Sequence.TryGet(ref position, out memory))
		{
			if (memory.Length > 0)
			{
				if (num < memory.Length)
				{
					break;
				}
				num -= memory.Length;
			}
		}
		return YamlCodes.IsEmpty(memory.Span[num]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly bool TryPeek(long offset, out byte value)
	{
		if (reader.End || reader.Remaining <= offset)
		{
			value = 0;
			return false;
		}
		if (reader.CurrentSpanIndex + offset <= reader.CurrentSpan.Length - 1)
		{
			value = reader.CurrentSpan[reader.CurrentSpanIndex + (int)offset];
			return true;
		}
		long num = offset;
		SequencePosition position = reader.Position;
		ReadOnlyMemory<byte> memory;
		while (reader.Sequence.TryGet(ref position, out memory))
		{
			if (memory.Length > 0)
			{
				if (num < memory.Length)
				{
					break;
				}
				num -= memory.Length;
			}
		}
		value = memory.Span[(int)num];
		return true;
	}
}
