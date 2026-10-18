using System;
using System.Buffers;
using System.Buffers.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VYaml.Internal;

namespace VYaml.Emitter;

public ref struct Utf8YamlEmitter
{
	private static byte[] whiteSpaces = new byte[32]
	{
		32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
		32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
		32, 32, 32, 32, 32, 32, 32, 32, 32, 32,
		32, 32
	};

	private static readonly byte[] BlockSequenceEntryHeader = new byte[2] { 45, 32 };

	private static readonly byte[] FlowSequenceEmpty = new byte[2] { 91, 93 };

	private static readonly byte[] FlowSequenceSeparator = new byte[2] { 44, 32 };

	private static readonly byte[] MappingKeyFooter = new byte[2] { 58, 32 };

	private static readonly byte[] FlowMappingHeader = new byte[2] { 123, 32 };

	private static readonly byte[] FlowMappingFooter = new byte[2] { 32, 125 };

	private static readonly byte[] FlowMappingEmpty = new byte[2] { 123, 125 };

	[ThreadStatic]
	private static ExpandBuffer<char>? stringBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<EmitState>? stateBufferStatic;

	[ThreadStatic]
	private static ExpandBuffer<int>? elementCountBufferSTatic;

	[ThreadStatic]
	private static ExpandBuffer<string>? tagBufferStatic;

	private readonly IBufferWriter<byte> writer;

	private readonly YamlEmitOptions options;

	private readonly ExpandBuffer<char> stringBuffer;

	private readonly ExpandBuffer<EmitState> stateStack;

	private readonly ExpandBuffer<int> elementCountStack;

	private readonly ExpandBuffer<string> tagStack;

	private int currentIndentLevel;

	private int currentElementCount;

	private EmitState CurrentState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			ExpandBuffer<EmitState> expandBuffer = stateStack;
			return expandBuffer[expandBuffer.Length - 1];
		}
	}

	private EmitState PreviousState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			ExpandBuffer<EmitState> expandBuffer = stateStack;
			return expandBuffer[expandBuffer.Length - 2];
		}
	}

	private bool IsFirstElement
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return currentElementCount <= 0;
		}
	}

	public Utf8YamlEmitter(IBufferWriter<byte> writer, YamlEmitOptions? options = null)
	{
		this.writer = writer;
		this.options = options ?? YamlEmitOptions.Default;
		currentIndentLevel = 0;
		stringBuffer = stringBufferStatic ?? (stringBufferStatic = new ExpandBuffer<char>(1024));
		stringBuffer.Clear();
		stateStack = stateBufferStatic ?? (stateBufferStatic = new ExpandBuffer<EmitState>(16));
		stateStack.Clear();
		elementCountStack = elementCountBufferSTatic ?? (elementCountBufferSTatic = new ExpandBuffer<int>(16));
		elementCountStack.Clear();
		stateStack.Add(EmitState.None);
		currentElementCount = 0;
		tagStack = new ExpandBuffer<string>(4);
	}

	internal readonly IBufferWriter<byte> GetWriter()
	{
		return writer;
	}

	public void BeginSequence(SequenceStyle style = SequenceStyle.Block)
	{
		switch (style)
		{
		case SequenceStyle.Block:
			switch (CurrentState)
			{
			case EmitState.BlockSequenceEntry:
				WriteBlockSequenceEntryHeader();
				break;
			case EmitState.FlowSequenceEntry:
				throw new YamlEmitterException("To start block-sequence in the flow-sequence is not supported.");
			case EmitState.BlockMappingKey:
				throw new YamlEmitterException("To start block-sequence in the mapping key is not supported.");
			}
			PushState(EmitState.BlockSequenceEntry);
			break;
		case SequenceStyle.Flow:
			switch (CurrentState)
			{
			case EmitState.BlockMappingKey:
				throw new YamlEmitterException("To start flow-sequence in the mapping key is not supported.");
			case EmitState.BlockSequenceEntry:
			{
				Span<byte> span2 = writer.GetSpan(currentIndentLevel * options.IndentWidth + BlockSequenceEntryHeader.Length + 1);
				int offset2 = 0;
				WriteBlockSequenceEntryHeader(span2, ref offset2);
				span2[offset2++] = 91;
				writer.Advance(offset2);
				break;
			}
			case EmitState.FlowSequenceEntry:
			{
				Span<byte> span3 = writer.GetSpan(FlowSequenceSeparator.Length + 1);
				int num = 0;
				if (currentElementCount > 0)
				{
					FlowSequenceSeparator.CopyTo(span3);
					num += FlowSequenceSeparator.Length;
				}
				span3[num++] = 91;
				writer.Advance(num);
				break;
			}
			default:
			{
				Span<byte> span = writer.GetSpan(GetTagLength() + 2);
				int offset = 0;
				if (TryWriteTag(span, ref offset))
				{
					span[offset++] = 32;
				}
				span[offset++] = 91;
				writer.Advance(offset);
				break;
			}
			}
			PushState(EmitState.FlowSequenceEntry);
			break;
		default:
			throw new ArgumentOutOfRangeException("style", style, null);
		}
	}

	public void EndSequence()
	{
		switch (CurrentState)
		{
		case EmitState.BlockSequenceEntry:
		{
			bool flag2 = currentElementCount <= 0;
			PopState();
			if (flag2)
			{
				EmitState currentState = CurrentState;
				bool lineBreak = currentState == EmitState.BlockSequenceEntry || currentState == EmitState.BlockMappingValue;
				WriteRaw(FlowSequenceEmpty, indent: false, lineBreak);
			}
			switch (CurrentState)
			{
			case EmitState.BlockSequenceEntry:
				if (!flag2)
				{
					DecreaseIndent();
				}
				currentElementCount++;
				break;
			case EmitState.BlockMappingKey:
				throw new YamlEmitterException("Complex key is not supported.");
			case EmitState.BlockMappingValue:
				ReplaceCurrentState(EmitState.BlockMappingKey);
				currentElementCount++;
				break;
			case EmitState.FlowSequenceEntry:
				currentElementCount++;
				break;
			}
			break;
		}
		case EmitState.FlowSequenceEntry:
		{
			PopState();
			bool flag = false;
			switch (CurrentState)
			{
			case EmitState.BlockSequenceEntry:
				flag = true;
				currentElementCount++;
				break;
			case EmitState.BlockMappingValue:
				ReplaceCurrentState(EmitState.BlockMappingKey);
				flag = true;
				currentElementCount++;
				break;
			case EmitState.FlowSequenceEntry:
				currentElementCount++;
				break;
			case EmitState.FlowMappingValue:
				ReplaceCurrentState(EmitState.FlowMappingKey);
				currentElementCount++;
				break;
			}
			int num = 1;
			if (flag)
			{
				num++;
			}
			int count = 0;
			Span<byte> span = writer.GetSpan(num);
			span[count++] = 93;
			if (flag)
			{
				span[count++] = 10;
			}
			writer.Advance(count);
			break;
		}
		default:
			throw new YamlEmitterException($"Current state is not sequence: {CurrentState}");
		}
	}

	public void BeginMapping(MappingStyle style = MappingStyle.Block)
	{
		switch (style)
		{
		case MappingStyle.Block:
			switch (CurrentState)
			{
			case EmitState.BlockMappingKey:
				throw new YamlEmitterException("To start block-mapping in the mapping key is not supported.");
			case EmitState.FlowSequenceEntry:
				throw new YamlEmitterException("Cannot start block-mapping in the flow-sequence");
			case EmitState.BlockSequenceEntry:
				WriteBlockSequenceEntryHeader();
				break;
			}
			PushState(EmitState.BlockMappingKey);
			break;
		case MappingStyle.Flow:
			switch (CurrentState)
			{
			case EmitState.BlockMappingKey:
				throw new YamlEmitterException("To start flow-mapping in the mapping key is not supported.");
			case EmitState.BlockSequenceEntry:
			{
				Span<byte> span2 = writer.GetSpan(currentIndentLevel * options.IndentWidth + BlockSequenceEntryHeader.Length + FlowMappingHeader.Length + GetTagLength() + 1);
				int offset2 = 0;
				WriteBlockSequenceEntryHeader(span2, ref offset2);
				if (TryWriteTag(span2, ref offset2))
				{
					span2[offset2++] = 32;
				}
				span2[offset2++] = 123;
				writer.Advance(offset2);
				break;
			}
			case EmitState.FlowSequenceEntry:
			{
				Span<byte> span3 = writer.GetSpan(FlowSequenceSeparator.Length + FlowMappingHeader.Length);
				int num = 0;
				if (!IsFirstElement)
				{
					FlowSequenceSeparator.CopyTo(span3);
					num += FlowSequenceSeparator.Length;
				}
				span3[num++] = 123;
				writer.Advance(num);
				break;
			}
			default:
			{
				Span<byte> span = writer.GetSpan(GetTagLength() + 2);
				int offset = 0;
				if (TryWriteTag(span, ref offset))
				{
					span[offset++] = 32;
				}
				span[offset++] = 123;
				writer.Advance(offset);
				break;
			}
			}
			PushState(EmitState.FlowMappingKey);
			break;
		default:
			throw new ArgumentOutOfRangeException("style", style, null);
		}
	}

	public void EndMapping()
	{
		switch (CurrentState)
		{
		case EmitState.BlockMappingKey:
		{
			bool flag3 = currentElementCount <= 0;
			PopState();
			if (flag3)
			{
				EmitState currentState = CurrentState;
				bool lineBreak = currentState == EmitState.BlockSequenceEntry || currentState == EmitState.BlockMappingValue;
				if (tagStack.TryPop(out string value))
				{
					byte[] bytes = StringEncoding.Utf8.GetBytes(value + " ");
					WriteRaw(bytes, FlowMappingEmpty, indent: false, lineBreak);
				}
				else
				{
					WriteRaw(FlowMappingEmpty, indent: false, lineBreak);
				}
			}
			switch (CurrentState)
			{
			case EmitState.BlockSequenceEntry:
				if (!flag3)
				{
					DecreaseIndent();
				}
				currentElementCount++;
				break;
			case EmitState.BlockMappingValue:
				if (!flag3)
				{
					DecreaseIndent();
				}
				ReplaceCurrentState(EmitState.BlockMappingKey);
				currentElementCount++;
				break;
			}
			break;
		}
		case EmitState.FlowMappingKey:
		{
			bool flag = currentElementCount <= 0;
			PopState();
			bool flag2 = false;
			switch (CurrentState)
			{
			case EmitState.BlockSequenceEntry:
				flag2 = true;
				currentElementCount++;
				break;
			case EmitState.BlockMappingValue:
				ReplaceCurrentState(EmitState.BlockMappingKey);
				flag2 = true;
				currentElementCount++;
				break;
			case EmitState.FlowSequenceEntry:
				currentElementCount++;
				break;
			case EmitState.FlowMappingValue:
				ReplaceCurrentState(EmitState.FlowMappingKey);
				currentElementCount++;
				break;
			}
			int num = FlowMappingFooter.Length;
			if (flag2)
			{
				num++;
			}
			int count = 0;
			Span<byte> span = writer.GetSpan(num);
			if (!flag)
			{
				span[count++] = 32;
			}
			span[count++] = 125;
			if (flag2)
			{
				span[count++] = 10;
			}
			writer.Advance(count);
			break;
		}
		default:
			throw new YamlEmitterException($"Invalid mapping end: {CurrentState}");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteRaw(ReadOnlySpan<byte> value, bool indent, bool lineBreak)
	{
		int num = value.Length + (indent ? (currentIndentLevel * options.IndentWidth) : 0) + (lineBreak ? 1 : 0);
		int offset = 0;
		Span<byte> span = writer.GetSpan(num);
		if (indent)
		{
			WriteIndent(span, ref offset);
		}
		Span<byte> span2 = span;
		int num2 = offset;
		value.CopyTo(span2.Slice(num2, span2.Length - num2));
		if (lineBreak)
		{
			span[num - 1] = 10;
		}
		writer.Advance(num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteRaw(ReadOnlySpan<byte> value1, ReadOnlySpan<byte> value2, bool indent, bool lineBreak)
	{
		int num = value1.Length + value2.Length + (indent ? (currentIndentLevel * options.IndentWidth) : 0) + (lineBreak ? 1 : 0);
		int offset = 0;
		Span<byte> span = writer.GetSpan(num);
		if (indent)
		{
			WriteIndent(span, ref offset);
		}
		Span<byte> span2 = span;
		int num2 = offset;
		value1.CopyTo(span2.Slice(num2, span2.Length - num2));
		offset += value1.Length;
		span2 = span;
		num2 = offset;
		value2.CopyTo(span2.Slice(num2, span2.Length - num2));
		if (lineBreak)
		{
			span[num - 1] = 10;
		}
		writer.Advance(num);
	}

	public void Tag(string value)
	{
		tagStack.Add(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteScalar(ReadOnlySpan<byte> value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(value.Length));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		value.CopyTo(span2.Slice(num, span2.Length - num));
		offset += value.Length;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteNull()
	{
		WriteScalar(YamlCodes.Null0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteBool(bool value)
	{
		WriteScalar(value ? YamlCodes.True0 : YamlCodes.False0);
	}

	public void WriteInt32(int value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(11));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteUInt32(uint value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(10));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteInt64(long value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(20));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteUInt64(ulong value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(20));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteFloat(float value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(12));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteDouble(double value)
	{
		int offset = 0;
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(17));
		BeginScalar(span, ref offset);
		Span<byte> span2 = span;
		int num = offset;
		if (!Utf8Formatter.TryFormat(value, span2.Slice(num, span2.Length - num), out var bytesWritten))
		{
			throw new YamlEmitterException($"Failed to emit : {value}");
		}
		offset += bytesWritten;
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	public void WriteString(string value, ScalarStyle style = ScalarStyle.Any)
	{
		if (style == ScalarStyle.Any)
		{
			style = EmitStringAnalyzer.Analyze(value).SuggestScalarStyle();
		}
		switch (style)
		{
		case ScalarStyle.Plain:
			WritePlainScalar(value);
			break;
		case ScalarStyle.SingleQuoted:
			WriteQuotedScalar(value, doubleQuote: false);
			break;
		case ScalarStyle.DoubleQuoted:
			WriteQuotedScalar(value);
			break;
		case ScalarStyle.Literal:
			WriteLiteralScalar(value);
			break;
		case ScalarStyle.Folded:
			throw new NotSupportedException();
		default:
			throw new ArgumentOutOfRangeException("style", style, null);
		}
	}

	private void WritePlainScalar(string value)
	{
		int maxByteCount = StringEncoding.Utf8.GetMaxByteCount(value.Length);
		Span<byte> span = writer.GetSpan(CalculateMaxScalarBufferLength(maxByteCount));
		int offset = 0;
		BeginScalar(span, ref offset);
		int num = offset;
		Encoding utf = StringEncoding.Utf8;
		ReadOnlySpan<char> chars = value;
		Span<byte> span2 = span;
		int num2 = offset;
		offset = num + utf.GetBytes(chars, span2.Slice(num2, span2.Length - num2));
		EndScalar(span, ref offset);
		writer.Advance(offset);
	}

	private void WriteLiteralScalar(string value)
	{
		int indentCharCount = (currentIndentLevel + 1) * options.IndentWidth;
		StringBuilder stringBuilder = EmitStringAnalyzer.BuildLiteralScalar(value, indentCharCount);
		Span<char> span = stringBuffer.AsSpan(stringBuilder.Length);
		stringBuilder.CopyTo(0, span, stringBuilder.Length);
		EmitState currentState = CurrentState;
		if (currentState == EmitState.BlockMappingValue || currentState == EmitState.BlockSequenceEntry)
		{
			Span<char> span2 = span;
			span = span2.Slice(0, span2.Length - 1);
		}
		int maxByteCount = StringEncoding.Utf8.GetMaxByteCount(span.Length);
		int offset = 0;
		Span<byte> span3 = writer.GetSpan(CalculateMaxScalarBufferLength(maxByteCount));
		BeginScalar(span3, ref offset);
		int num = offset;
		Encoding utf = StringEncoding.Utf8;
		ReadOnlySpan<char> chars = span;
		Span<byte> span4 = span3;
		int num2 = offset;
		offset = num + utf.GetBytes(chars, span4.Slice(num2, span4.Length - num2));
		EndScalar(span3, ref offset);
		writer.Advance(offset);
	}

	private void WriteQuotedScalar(string value, bool doubleQuote = true)
	{
		StringBuilder stringBuilder = EmitStringAnalyzer.BuildQuotedScalar(value, doubleQuote);
		Span<char> span = stringBuffer.AsSpan(stringBuilder.Length);
		stringBuilder.CopyTo(0, span, stringBuilder.Length);
		int maxByteCount = StringEncoding.Utf8.GetMaxByteCount(span.Length);
		int offset = 0;
		Span<byte> span2 = writer.GetSpan(CalculateMaxScalarBufferLength(maxByteCount));
		BeginScalar(span2, ref offset);
		int num = offset;
		Encoding utf = StringEncoding.Utf8;
		ReadOnlySpan<char> chars = span;
		Span<byte> span3 = span2;
		int num2 = offset;
		offset = num + utf.GetBytes(chars, span3.Slice(num2, span3.Length - num2));
		EndScalar(span2, ref offset);
		writer.Advance(offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteRaw1(byte value)
	{
		writer.GetSpan(1)[0] = value;
		writer.Advance(1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteBlockSequenceEntryHeader()
	{
		Span<byte> span = writer.GetSpan(BlockSequenceEntryHeader.Length + currentIndentLevel * options.IndentWidth + 2);
		int offset = 0;
		WriteBlockSequenceEntryHeader(span, ref offset);
		writer.Advance(offset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteBlockSequenceEntryHeader(Span<byte> output, ref int offset)
	{
		if (IsFirstElement)
		{
			switch (PreviousState)
			{
			case EmitState.BlockSequenceEntry:
				output[offset++] = 10;
				IncreaseIndent();
				break;
			case EmitState.BlockMappingValue:
				output[offset++] = 10;
				break;
			}
		}
		WriteIndent(output, ref offset);
		byte[] blockSequenceEntryHeader = BlockSequenceEntryHeader;
		Span<byte> span = output;
		int num = offset;
		blockSequenceEntryHeader.CopyTo(span.Slice(num, span.Length - num));
		offset += BlockSequenceEntryHeader.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteIndent(Span<byte> output, ref int offset, int forceWidth = -1)
	{
		int num;
		if (forceWidth > -1)
		{
			if (forceWidth <= 0)
			{
				return;
			}
			num = forceWidth;
		}
		else
		{
			if (currentIndentLevel <= 0)
			{
				return;
			}
			num = currentIndentLevel * options.IndentWidth;
		}
		if (num > whiteSpaces.Length)
		{
			whiteSpaces = Enumerable.Repeat((byte)32, num * 2).ToArray();
		}
		Span<byte> span = whiteSpaces.AsSpan(0, num);
		Span<byte> span2 = output;
		int num2 = offset;
		span.CopyTo(span2.Slice(num2, span2.Length - num2));
		offset += num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int CalculateMaxScalarBufferLength(int length)
	{
		return length + (currentIndentLevel + 1) * options.IndentWidth + 3 + GetTagLength();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void BeginScalar(Span<byte> output, ref int offset)
	{
		switch (CurrentState)
		{
		case EmitState.BlockSequenceEntry:
			WriteBlockSequenceEntryHeader(output, ref offset);
			if (TryWriteTag(output, ref offset))
			{
				output[offset++] = 32;
			}
			break;
		case EmitState.BlockMappingKey:
			if (IsFirstElement)
			{
				switch (PreviousState)
				{
				case EmitState.BlockSequenceEntry:
				{
					IncreaseIndent();
					if (tagStack.TryPop(out string value))
					{
						int num2 = offset;
						Encoding utf = StringEncoding.Utf8;
						ReadOnlySpan<char> chars = value;
						Span<byte> span = output;
						int num = offset;
						offset = num2 + utf.GetBytes(chars, span.Slice(num, span.Length - num));
						output[offset++] = 10;
						WriteIndent(output, ref offset);
					}
					else
					{
						WriteIndent(output, ref offset, options.IndentWidth - 2);
					}
					break;
				}
				case EmitState.BlockMappingValue:
					IncreaseIndent();
					TryWriteTag(output, ref offset);
					output[offset++] = 10;
					WriteIndent(output, ref offset);
					break;
				default:
					WriteIndent(output, ref offset);
					break;
				}
				if (TryWriteTag(output, ref offset))
				{
					output[offset++] = 10;
					WriteIndent(output, ref offset);
				}
			}
			else
			{
				WriteIndent(output, ref offset);
			}
			break;
		case EmitState.BlockMappingValue:
			if (TryWriteTag(output, ref offset))
			{
				output[offset++] = 32;
			}
			break;
		case EmitState.FlowSequenceEntry:
			if (!IsFirstElement)
			{
				byte[] flowSequenceSeparator2 = FlowSequenceSeparator;
				Span<byte> span = output;
				int num = offset;
				flowSequenceSeparator2.CopyTo(span.Slice(num, span.Length - num));
				offset += FlowSequenceSeparator.Length;
			}
			if (TryWriteTag(output, ref offset))
			{
				output[offset++] = 32;
			}
			break;
		case EmitState.FlowMappingKey:
		{
			if (IsFirstElement)
			{
				output[offset++] = 32;
				break;
			}
			byte[] flowSequenceSeparator = FlowSequenceSeparator;
			Span<byte> span = output;
			int num = offset;
			flowSequenceSeparator.CopyTo(span.Slice(num, span.Length - num));
			offset += FlowSequenceSeparator.Length;
			break;
		}
		case EmitState.None:
		case EmitState.FlowMappingValue:
			if (TryWriteTag(output, ref offset))
			{
				output[offset++] = 32;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EndScalar(Span<byte> output, ref int offset)
	{
		switch (CurrentState)
		{
		case EmitState.BlockSequenceEntry:
			output[offset++] = 10;
			currentElementCount++;
			break;
		case EmitState.BlockMappingKey:
		{
			byte[] mappingKeyFooter2 = MappingKeyFooter;
			Span<byte> span = output;
			int num = offset;
			mappingKeyFooter2.CopyTo(span.Slice(num, span.Length - num));
			offset += MappingKeyFooter.Length;
			ReplaceCurrentState(EmitState.BlockMappingValue);
			break;
		}
		case EmitState.BlockMappingValue:
			output[offset++] = 10;
			ReplaceCurrentState(EmitState.BlockMappingKey);
			currentElementCount++;
			break;
		case EmitState.FlowSequenceEntry:
			currentElementCount++;
			break;
		case EmitState.FlowMappingKey:
		{
			byte[] mappingKeyFooter = MappingKeyFooter;
			Span<byte> span = output;
			int num = offset;
			mappingKeyFooter.CopyTo(span.Slice(num, span.Length - num));
			offset += MappingKeyFooter.Length;
			ReplaceCurrentState(EmitState.FlowMappingValue);
			break;
		}
		case EmitState.FlowMappingValue:
			ReplaceCurrentState(EmitState.FlowMappingKey);
			currentElementCount++;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EmitState.None:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ReplaceCurrentState(EmitState newState)
	{
		ExpandBuffer<EmitState> expandBuffer = stateStack;
		expandBuffer[expandBuffer.Length - 1] = newState;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PushState(EmitState state)
	{
		stateStack.Add(state);
		elementCountStack.Add(currentElementCount);
		currentElementCount = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PopState()
	{
		_ = ref stateStack.Pop();
		currentElementCount = ((elementCountStack.Length > 0) ? elementCountStack.Pop() : 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void IncreaseIndent()
	{
		currentIndentLevel++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DecreaseIndent()
	{
		if (currentIndentLevel > 0)
		{
			currentIndentLevel--;
		}
	}

	private bool TryWriteTag(Span<byte> output, ref int offset)
	{
		if (tagStack.TryPop(out string value))
		{
			int num = offset;
			Encoding utf = StringEncoding.Utf8;
			ReadOnlySpan<char> chars = value;
			Span<byte> span = output;
			int num2 = offset;
			offset = num + utf.GetBytes(chars, span.Slice(num2, span.Length - num2));
			return true;
		}
		return false;
	}

	private int GetTagLength()
	{
		if (tagStack.Length <= 0)
		{
			return 0;
		}
		return StringEncoding.Utf8.GetMaxByteCount(tagStack.Peek().Length);
	}
}
