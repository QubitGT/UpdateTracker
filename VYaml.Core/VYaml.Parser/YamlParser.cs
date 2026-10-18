using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VYaml.Internal;

namespace VYaml.Parser;

public ref struct YamlParser
{
	[ThreadStatic]
	private static Dictionary<string, int>? anchorsBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<ParseState>? stateStackBufferStatic;

	private Utf8YamlTokenizer tokenizer;

	private ParseState currentState;

	private Scalar? currentScalar;

	private Tag? currentTag;

	private Anchor? currentAnchor;

	private int lastAnchorId;

	private readonly Dictionary<string, int> anchors;

	private readonly ExpandBuffer<ParseState> stateStack;

	public ParseEventType CurrentEventType { get; private set; }

	public bool UnityStrippedMark { get; private set; }

	public readonly Marker CurrentMark
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return tokenizer.CurrentMark;
		}
	}

	public bool End => CurrentEventType == ParseEventType.StreamEnd;

	private TokenType CurrentTokenType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return tokenizer.CurrentTokenType;
		}
	}

	public static YamlParser FromBytes(Memory<byte> bytes)
	{
		return new YamlParser(new ReadOnlySequence<byte>(bytes));
	}

	public static YamlParser FromSequence(in ReadOnlySequence<byte> sequence)
	{
		return new YamlParser(sequence);
	}

	public YamlParser(ReadOnlySequence<byte> sequence)
	{
		tokenizer = new Utf8YamlTokenizer(sequence);
		currentState = ParseState.StreamStart;
		CurrentEventType = ParseEventType.Nothing;
		lastAnchorId = -1;
		anchors = anchorsBufferStatic ?? (anchorsBufferStatic = new Dictionary<string, int>());
		anchors.Clear();
		stateStack = stateStackBufferStatic ?? (stateStackBufferStatic = new ExpandBuffer<ParseState>(16));
		stateStack.Clear();
		currentScalar = null;
		currentTag = null;
		currentAnchor = null;
		UnityStrippedMark = false;
	}

	public YamlParser(ref Utf8YamlTokenizer tokenizer)
	{
		this.tokenizer = tokenizer;
		currentState = ParseState.StreamStart;
		CurrentEventType = ParseEventType.Nothing;
		lastAnchorId = -1;
		anchors = new Dictionary<string, int>();
		stateStack = new ExpandBuffer<ParseState>(16);
		currentScalar = null;
		currentTag = null;
		currentAnchor = null;
		UnityStrippedMark = false;
	}

	public bool Read()
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			ScalarPool.Shared.Return(scalar);
			currentScalar = null;
		}
		if (currentState == ParseState.End)
		{
			CurrentEventType = ParseEventType.StreamEnd;
			return false;
		}
		switch (currentState)
		{
		case ParseState.StreamStart:
			ParseStreamStart();
			break;
		case ParseState.ImplicitDocumentStart:
			ParseDocumentStart(implicitStarted: true);
			break;
		case ParseState.DocumentStart:
			ParseDocumentStart(implicitStarted: false);
			break;
		case ParseState.DocumentContent:
			ParseDocumentContent();
			break;
		case ParseState.DocumentEnd:
			ParseDocumentEnd();
			break;
		case ParseState.BlockNode:
			ParseNode(block: true, indentlessSequence: false);
			break;
		case ParseState.BlockMappingFirstKey:
			ParseBlockMappingKey(first: true);
			break;
		case ParseState.BlockMappingKey:
			ParseBlockMappingKey(first: false);
			break;
		case ParseState.BlockMappingValue:
			ParseBlockMappingValue();
			break;
		case ParseState.BlockSequenceFirstEntry:
			ParseBlockSequenceEntry(first: true);
			break;
		case ParseState.BlockSequenceEntry:
			ParseBlockSequenceEntry(first: false);
			break;
		case ParseState.FlowSequenceFirstEntry:
			ParseFlowSequenceEntry(first: true);
			break;
		case ParseState.FlowSequenceEntry:
			ParseFlowSequenceEntry(first: false);
			break;
		case ParseState.FlowMappingFirstKey:
			ParseFlowMappingKey(first: true);
			break;
		case ParseState.FlowMappingKey:
			ParseFlowMappingKey(first: false);
			break;
		case ParseState.FlowMappingValue:
			ParseFlowMappingValue(empty: false);
			break;
		case ParseState.IndentlessSequenceEntry:
			ParseIndentlessSequenceEntry();
			break;
		case ParseState.FlowSequenceEntryMappingKey:
			ParseFlowSequenceEntryMappingKey();
			break;
		case ParseState.FlowSequenceEntryMappingValue:
			ParseFlowSequenceEntryMappingValue();
			break;
		case ParseState.FlowSequenceEntryMappingEnd:
			ParseFlowSequenceEntryMappingEnd();
			break;
		case ParseState.FlowMappingEmptyValue:
			ParseFlowMappingValue(empty: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ReadWithVerify(ParseEventType eventType)
	{
		if (CurrentEventType != eventType)
		{
			Marker marker = CurrentMark;
			throw new YamlParserException(in marker, $"Did not find expected event : `{eventType}`");
		}
		Read();
	}

	public void SkipAfter(ParseEventType eventType)
	{
		while (CurrentEventType != eventType && Read())
		{
		}
		if (CurrentEventType == eventType)
		{
			Read();
		}
	}

	public void SkipCurrentNode()
	{
		switch (CurrentEventType)
		{
		case ParseEventType.Alias:
		case ParseEventType.Scalar:
			Read();
			break;
		case ParseEventType.SequenceStart:
		{
			int num2 = 1;
			while (Read())
			{
				switch (CurrentEventType)
				{
				case ParseEventType.SequenceStart:
					num2++;
					break;
				case ParseEventType.SequenceEnd:
					if (--num2 <= 0)
					{
						Read();
						return;
					}
					break;
				}
			}
			break;
		}
		case ParseEventType.MappingStart:
		{
			int num = 1;
			while (Read())
			{
				switch (CurrentEventType)
				{
				case ParseEventType.MappingStart:
					num++;
					break;
				case ParseEventType.MappingEnd:
					if (--num <= 0)
					{
						Read();
						return;
					}
					break;
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ParseStreamStart()
	{
		if (CurrentTokenType == TokenType.None)
		{
			tokenizer.Read();
		}
		ThrowIfCurrentTokenUnless(TokenType.StreamStart);
		currentState = ParseState.ImplicitDocumentStart;
		tokenizer.Read();
		CurrentEventType = ParseEventType.StreamStart;
	}

	private void ParseDocumentStart(bool implicitStarted)
	{
		if (!implicitStarted)
		{
			while (tokenizer.CurrentTokenType == TokenType.DocumentEnd)
			{
				tokenizer.Read();
			}
		}
		switch (tokenizer.CurrentTokenType)
		{
		case TokenType.StreamEnd:
			currentState = ParseState.End;
			tokenizer.Read();
			CurrentEventType = ParseEventType.StreamEnd;
			return;
		case TokenType.VersionDirective:
		case TokenType.TagDirective:
		case TokenType.DocumentStart:
			ParseExplicitDocumentStart();
			return;
		}
		if (implicitStarted)
		{
			ProcessDirectives();
			PushState(ParseState.DocumentEnd);
			currentState = ParseState.BlockNode;
			CurrentEventType = ParseEventType.DocumentStart;
		}
		else
		{
			ParseExplicitDocumentStart();
		}
	}

	private void ParseExplicitDocumentStart()
	{
		ProcessDirectives();
		ThrowIfCurrentTokenUnless(TokenType.DocumentStart);
		PushState(ParseState.DocumentEnd);
		currentState = ParseState.DocumentContent;
		tokenizer.Read();
		CurrentEventType = ParseEventType.DocumentStart;
	}

	private void ParseDocumentContent()
	{
		TokenType currentTokenType = tokenizer.CurrentTokenType;
		if (currentTokenType - 2 <= TokenType.TagDirective)
		{
			PopState();
			EmptyScalar();
		}
		else
		{
			ParseNode(block: true, indentlessSequence: false);
		}
	}

	private void ParseDocumentEnd()
	{
		if (CurrentTokenType == TokenType.DocumentEnd)
		{
			tokenizer.Read();
		}
		currentState = ParseState.DocumentStart;
		CurrentEventType = ParseEventType.DocumentEnd;
	}

	private void ParseNode(bool block, bool indentlessSequence)
	{
		currentAnchor = null;
		currentTag = null;
		UnityStrippedMark = false;
		switch (CurrentTokenType)
		{
		case TokenType.Alias:
		{
			PopState();
			string text3 = tokenizer.TakeCurrentTokenContent<Scalar>().ToString();
			tokenizer.Read();
			if (anchors.TryGetValue(text3, out var value))
			{
				currentAnchor = new Anchor(text3, value);
				CurrentEventType = ParseEventType.Alias;
				return;
			}
			Marker marker = CurrentMark;
			throw new YamlParserException(in marker, "While parsing node, found unknown anchor");
		}
		case TokenType.Anchor:
		{
			string text2 = tokenizer.TakeCurrentTokenContent<Scalar>().ToString();
			int id2 = RegisterAnchor(text2);
			currentAnchor = new Anchor(text2, id2);
			tokenizer.Read();
			if (CurrentTokenType == TokenType.Tag)
			{
				currentTag = tokenizer.TakeCurrentTokenContent<Tag>();
				tokenizer.Read();
			}
			break;
		}
		case TokenType.Tag:
			currentTag = tokenizer.TakeCurrentTokenContent<Tag>();
			tokenizer.Read();
			if (CurrentTokenType == TokenType.Anchor)
			{
				string text = tokenizer.TakeCurrentTokenContent<Scalar>().ToString();
				int id = RegisterAnchor(text);
				currentAnchor = new Anchor(text, id);
				if (CurrentEventType == ParseEventType.DocumentStart && currentTag?.Handle == "!u!")
				{
					UnityStrippedMark = tokenizer.TrySkipUnityStrippedSymbol();
				}
				tokenizer.Read();
			}
			break;
		}
		int num;
		switch (CurrentTokenType)
		{
		case TokenType.BlockEntryStart:
			if (indentlessSequence)
			{
				currentState = ParseState.IndentlessSequenceEntry;
				CurrentEventType = ParseEventType.SequenceStart;
				return;
			}
			num = 1;
			break;
		case TokenType.PlainScalar:
		case TokenType.SingleQuotedScaler:
		case TokenType.DoubleQuotedScaler:
		case TokenType.LiteralScalar:
		case TokenType.FoldedScalar:
			PopState();
			currentScalar = tokenizer.TakeCurrentTokenContent<Scalar>();
			tokenizer.Read();
			CurrentEventType = ParseEventType.Scalar;
			return;
		case TokenType.FlowSequenceStart:
			currentState = ParseState.FlowSequenceFirstEntry;
			CurrentEventType = ParseEventType.SequenceStart;
			return;
		case TokenType.FlowMappingStart:
			currentState = ParseState.FlowMappingFirstKey;
			CurrentEventType = ParseEventType.MappingStart;
			return;
		case TokenType.BlockSequenceStart:
			if (block)
			{
				currentState = ParseState.BlockSequenceFirstEntry;
				CurrentEventType = ParseEventType.SequenceStart;
				return;
			}
			goto default;
		case TokenType.BlockMappingStart:
			if (block)
			{
				currentState = ParseState.BlockMappingFirstKey;
				CurrentEventType = ParseEventType.MappingStart;
				return;
			}
			goto default;
		default:
			num = 5;
			break;
		}
		if (currentAnchor == null && currentTag == null)
		{
			if (num != 1)
			{
				if (num != 5)
				{
					goto IL_0297;
				}
			}
			else if (currentState == ParseState.IndentlessSequenceEntry)
			{
				PopState();
				EmptyScalar();
				return;
			}
			Marker marker = tokenizer.CurrentMark;
			throw new YamlTokenizerException(in marker, "while parsing a node, did not find expected node content");
		}
		goto IL_0297;
		IL_0297:
		PopState();
		EmptyScalar();
	}

	private void ParseBlockMappingKey(bool first)
	{
		if (first)
		{
			tokenizer.Read();
		}
		switch (CurrentTokenType)
		{
		case TokenType.KeyStart:
		{
			tokenizer.Read();
			TokenType currentTokenType = CurrentTokenType;
			if (currentTokenType == TokenType.KeyStart || currentTokenType == TokenType.ValueStart || currentTokenType == TokenType.BlockEnd)
			{
				currentState = ParseState.BlockMappingValue;
				EmptyScalar();
			}
			else
			{
				PushState(ParseState.BlockMappingValue);
				ParseNode(block: true, indentlessSequence: true);
			}
			break;
		}
		case TokenType.ValueStart:
			currentState = ParseState.BlockMappingValue;
			EmptyScalar();
			break;
		case TokenType.BlockEnd:
			PopState();
			tokenizer.Read();
			CurrentEventType = ParseEventType.MappingEnd;
			break;
		default:
		{
			Marker marker = CurrentMark;
			throw new YamlParserException(in marker, "while parsing a block mapping, did not find expected key");
		}
		}
	}

	private void ParseBlockMappingValue()
	{
		if (CurrentTokenType == TokenType.ValueStart)
		{
			tokenizer.Read();
			TokenType currentTokenType = CurrentTokenType;
			if (currentTokenType == TokenType.KeyStart || currentTokenType == TokenType.ValueStart || currentTokenType == TokenType.BlockEnd)
			{
				currentState = ParseState.BlockMappingKey;
				EmptyScalar();
			}
			else
			{
				PushState(ParseState.BlockMappingKey);
				ParseNode(block: true, indentlessSequence: true);
			}
		}
		else
		{
			currentState = ParseState.BlockMappingKey;
			EmptyScalar();
		}
	}

	private void ParseBlockSequenceEntry(bool first)
	{
		if (first)
		{
			tokenizer.Read();
		}
		switch (CurrentTokenType)
		{
		case TokenType.BlockEnd:
			PopState();
			tokenizer.Read();
			CurrentEventType = ParseEventType.SequenceEnd;
			break;
		case TokenType.BlockEntryStart:
		{
			tokenizer.Read();
			TokenType currentTokenType = CurrentTokenType;
			if (currentTokenType == TokenType.BlockEntryStart || currentTokenType == TokenType.BlockEnd)
			{
				currentState = ParseState.BlockSequenceEntry;
				EmptyScalar();
			}
			else
			{
				PushState(ParseState.BlockSequenceEntry);
				ParseNode(block: true, indentlessSequence: false);
			}
			break;
		}
		default:
		{
			Marker marker = CurrentMark;
			throw new YamlParserException(in marker, "while parsing a block collection, did not find expected '-' indicator");
		}
		}
	}

	private void ParseFlowSequenceEntry(bool first)
	{
		if (first)
		{
			tokenizer.Read();
		}
		TokenType currentTokenType = CurrentTokenType;
		if (currentTokenType != TokenType.FlowSequenceEnd)
		{
			if (currentTokenType == TokenType.FlowEntryStart && !first)
			{
				tokenizer.Read();
			}
			else if (!first)
			{
				Marker marker = CurrentMark;
				throw new YamlParserException(in marker, "while parsing a flow sequence, expected ',' or ']'");
			}
			switch (CurrentTokenType)
			{
			case TokenType.FlowSequenceEnd:
				PopState();
				tokenizer.Read();
				CurrentEventType = ParseEventType.SequenceEnd;
				break;
			case TokenType.KeyStart:
				currentState = ParseState.FlowSequenceEntryMappingKey;
				tokenizer.Read();
				CurrentEventType = ParseEventType.MappingStart;
				break;
			default:
				PushState(ParseState.FlowSequenceEntry);
				ParseNode(block: false, indentlessSequence: false);
				break;
			}
		}
		else
		{
			PopState();
			tokenizer.Read();
			CurrentEventType = ParseEventType.SequenceEnd;
		}
	}

	private void ParseFlowMappingKey(bool first)
	{
		if (first)
		{
			tokenizer.Read();
		}
		if (CurrentTokenType == TokenType.FlowMappingEnd)
		{
			PopState();
			tokenizer.Read();
			CurrentEventType = ParseEventType.MappingEnd;
			return;
		}
		if (!first)
		{
			if (CurrentTokenType != TokenType.FlowEntryStart)
			{
				Marker marker = CurrentMark;
				throw new YamlParserException(in marker, "While parsing a flow mapping, did not find expected ',' or '}'");
			}
			tokenizer.Read();
		}
		switch (CurrentTokenType)
		{
		case TokenType.KeyStart:
		{
			tokenizer.Read();
			TokenType currentTokenType = CurrentTokenType;
			if (currentTokenType == TokenType.ValueStart || currentTokenType == TokenType.FlowEntryStart || currentTokenType == TokenType.FlowMappingEnd)
			{
				currentState = ParseState.FlowMappingValue;
				EmptyScalar();
			}
			else
			{
				PushState(ParseState.FlowMappingValue);
				ParseNode(block: false, indentlessSequence: false);
			}
			break;
		}
		case TokenType.ValueStart:
			currentState = ParseState.FlowMappingValue;
			EmptyScalar();
			break;
		case TokenType.FlowMappingEnd:
			PopState();
			tokenizer.Read();
			CurrentEventType = ParseEventType.MappingEnd;
			break;
		default:
			PushState(ParseState.FlowMappingEmptyValue);
			ParseNode(block: false, indentlessSequence: false);
			break;
		}
	}

	private void ParseFlowMappingValue(bool empty)
	{
		if (empty)
		{
			currentState = ParseState.FlowMappingKey;
			EmptyScalar();
			return;
		}
		if (CurrentTokenType == TokenType.ValueStart)
		{
			tokenizer.Read();
			if (CurrentTokenType != TokenType.FlowEntryStart && CurrentTokenType != TokenType.FlowMappingEnd)
			{
				PushState(ParseState.FlowMappingKey);
				ParseNode(block: false, indentlessSequence: false);
				return;
			}
		}
		currentState = ParseState.FlowMappingKey;
		EmptyScalar();
	}

	private void ParseIndentlessSequenceEntry()
	{
		if (CurrentTokenType != TokenType.BlockEntryStart)
		{
			PopState();
			CurrentEventType = ParseEventType.SequenceEnd;
			return;
		}
		tokenizer.Read();
		TokenType currentTokenType = CurrentTokenType;
		if (currentTokenType == TokenType.KeyStart || currentTokenType == TokenType.ValueStart || currentTokenType == TokenType.BlockEnd)
		{
			currentState = ParseState.IndentlessSequenceEntry;
			EmptyScalar();
		}
		else
		{
			PushState(ParseState.IndentlessSequenceEntry);
			ParseNode(block: true, indentlessSequence: false);
		}
	}

	private void ParseFlowSequenceEntryMappingKey()
	{
		TokenType currentTokenType = CurrentTokenType;
		if (currentTokenType == TokenType.ValueStart || currentTokenType == TokenType.FlowEntryStart || currentTokenType == TokenType.FlowSequenceEnd)
		{
			tokenizer.Read();
			currentState = ParseState.FlowSequenceEntryMappingValue;
			EmptyScalar();
		}
		else
		{
			PushState(ParseState.FlowSequenceEntryMappingValue);
			ParseNode(block: false, indentlessSequence: false);
		}
	}

	private void ParseFlowSequenceEntryMappingValue()
	{
		if (CurrentTokenType == TokenType.ValueStart)
		{
			tokenizer.Read();
			currentState = ParseState.FlowSequenceEntryMappingValue;
			TokenType currentTokenType = CurrentTokenType;
			if (currentTokenType == TokenType.FlowEntryStart || currentTokenType == TokenType.FlowSequenceEnd)
			{
				currentState = ParseState.FlowSequenceEntryMappingEnd;
				EmptyScalar();
			}
			else
			{
				PushState(ParseState.FlowSequenceEntryMappingEnd);
				ParseNode(block: false, indentlessSequence: false);
			}
		}
		else
		{
			currentState = ParseState.FlowSequenceEntryMappingEnd;
			EmptyScalar();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseFlowSequenceEntryMappingEnd()
	{
		currentState = ParseState.FlowSequenceEntry;
		CurrentEventType = ParseEventType.MappingEnd;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PopState()
	{
		currentState = stateStack.Pop();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PushState(ParseState state)
	{
		stateStack.Add(state);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EmptyScalar()
	{
		currentScalar = null;
		CurrentEventType = ParseEventType.Scalar;
	}

	private void ProcessDirectives()
	{
		while (true)
		{
			TokenType currentTokenType = tokenizer.CurrentTokenType;
			if (currentTokenType != TokenType.VersionDirective && currentTokenType != TokenType.TagDirective)
			{
				break;
			}
			tokenizer.Read();
		}
	}

	private int RegisterAnchor(string anchorName)
	{
		int num = ++lastAnchorId;
		anchors[anchorName] = num;
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ThrowIfCurrentTokenUnless(TokenType expectedTokenType)
	{
		if (CurrentTokenType != expectedTokenType)
		{
			Marker marker = tokenizer.CurrentMark;
			throw new YamlParserException(in marker, $"Did not find expected token of  `{expectedTokenType}`");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool IsNullScalar()
	{
		if (CurrentEventType == ParseEventType.Scalar)
		{
			if (currentScalar != null)
			{
				return currentScalar.IsNull();
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string? GetScalarAsString()
	{
		return currentScalar?.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<byte> GetScalarAsUtf8()
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.AsUtf8();
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as utf8 : {CurrentEventType} {currentScalar}");
		return default(ReadOnlySpan<byte>);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsSpan(out ReadOnlySpan<byte> span)
	{
		if (currentScalar == null)
		{
			span = default(ReadOnlySpan<byte>);
			return false;
		}
		span = currentScalar.AsSpan();
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool GetScalarAsBool()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetBool(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as bool : {CurrentEventType} {currentScalar}");
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int GetScalarAsInt32()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetInt32(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as Int32: {CurrentEventType} {currentScalar}");
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly long GetScalarAsInt64()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetInt64(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as Int64: {CurrentEventType} {currentScalar}");
		return 0L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly uint GetScalarAsUInt32()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetUInt32(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as UInt32 : {CurrentEventType} {currentScalar}");
		return 0u;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ulong GetScalarAsUInt64()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetUInt64(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as UInt64 : {CurrentEventType} ({currentScalar})");
		return 0uL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly float GetScalarAsFloat()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetFloat(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect scalar value as float : {CurrentEventType} {currentScalar}");
		return 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly double GetScalarAsDouble()
	{
		Scalar scalar = currentScalar;
		if (scalar != null && scalar.TryGetDouble(out var value))
		{
			return value;
		}
		Marker marker = CurrentMark;
		YamlParserException.Throw(in marker, $"Cannot detect a scalar value as double : {CurrentEventType} {currentScalar}");
		return 0.0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string? ReadScalarAsString()
	{
		string result = currentScalar?.ToString();
		ReadWithVerify(ParseEventType.Scalar);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ReadScalarAsBool()
	{
		bool scalarAsBool = GetScalarAsBool();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsBool;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ReadScalarAsInt32()
	{
		int scalarAsInt = GetScalarAsInt32();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsInt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long ReadScalarAsInt64()
	{
		long scalarAsInt = GetScalarAsInt64();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsInt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint ReadScalarAsUInt32()
	{
		uint scalarAsUInt = GetScalarAsUInt32();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsUInt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ulong ReadScalarAsUInt64()
	{
		ulong scalarAsUInt = GetScalarAsUInt64();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsUInt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float ReadScalarAsFloat()
	{
		float scalarAsFloat = GetScalarAsFloat();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsFloat;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double ReadScalarAsDouble()
	{
		double scalarAsDouble = GetScalarAsDouble();
		ReadWithVerify(ParseEventType.Scalar);
		return scalarAsDouble;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsString(out string? result)
	{
		if (CurrentEventType != ParseEventType.Scalar)
		{
			result = null;
			return false;
		}
		result = currentScalar?.ToString();
		ReadWithVerify(ParseEventType.Scalar);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsBool(out bool result)
	{
		if (TryGetScalarAsBool(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsInt32(out int result)
	{
		if (TryGetScalarAsInt32(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsInt64(out long result)
	{
		if (TryGetScalarAsInt64(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsUInt32(out uint result)
	{
		if (TryGetScalarAsUInt32(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsUInt64(out ulong result)
	{
		if (TryGetScalarAsUInt64(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsFloat(out float result)
	{
		if (TryGetScalarAsFloat(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryReadScalarAsDouble(out double result)
	{
		if (TryGetScalarAsDouble(out result))
		{
			ReadWithVerify(ParseEventType.Scalar);
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsString(out string? value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			value = (scalar.IsNull() ? null : scalar.ToString());
			return true;
		}
		value = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsBool(out bool value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetBool(out value);
		}
		value = false;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsInt32(out int value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetInt32(out value);
		}
		value = 0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsUInt32(out uint value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetUInt32(out value);
		}
		value = 0u;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsInt64(out long value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetInt64(out value);
		}
		value = 0L;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsUInt64(out ulong value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetUInt64(out value);
		}
		value = 0uL;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsFloat(out float value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetFloat(out value);
		}
		value = 0f;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetScalarAsDouble(out double value)
	{
		Scalar scalar = currentScalar;
		if (scalar != null)
		{
			return scalar.TryGetDouble(out value);
		}
		value = 0.0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetCurrentTag(out Tag tag)
	{
		if (currentTag != null)
		{
			tag = currentTag;
			return true;
		}
		tag = null;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryGetCurrentAnchor(out Anchor anchor)
	{
		if (currentAnchor != null)
		{
			anchor = currentAnchor;
			return true;
		}
		anchor = null;
		return false;
	}
}
