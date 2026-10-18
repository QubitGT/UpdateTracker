namespace VYaml.Parser;

public enum TokenType : byte
{
	None,
	StreamStart,
	StreamEnd,
	VersionDirective,
	TagDirective,
	DocumentStart,
	DocumentEnd,
	BlockSequenceStart,
	BlockMappingStart,
	BlockEnd,
	FlowSequenceStart,
	FlowSequenceEnd,
	FlowMappingStart,
	FlowMappingEnd,
	BlockEntryStart,
	FlowEntryStart,
	KeyStart,
	ValueStart,
	Alias,
	Anchor,
	Tag,
	PlainScalar,
	SingleQuotedScaler,
	DoubleQuotedScaler,
	LiteralScalar,
	FoldedScalar
}
