using VYaml.Emitter;

namespace VYaml.Internal;

internal readonly struct EmitStringInfo
{
	public readonly int Lines;

	public readonly bool NeedsQuotes;

	public readonly bool IsReservedWord;

	public EmitStringInfo(int lines, bool needsQuotes, bool isReservedWord)
	{
		Lines = lines;
		NeedsQuotes = needsQuotes;
		IsReservedWord = isReservedWord;
	}

	public ScalarStyle SuggestScalarStyle()
	{
		if (Lines <= 1)
		{
			if (!NeedsQuotes)
			{
				return ScalarStyle.Plain;
			}
			return ScalarStyle.DoubleQuoted;
		}
		return ScalarStyle.Literal;
	}
}
