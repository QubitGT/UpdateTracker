namespace VYaml.Parser;

internal enum ParseState
{
	StreamStart,
	ImplicitDocumentStart,
	DocumentStart,
	DocumentContent,
	DocumentEnd,
	BlockNode,
	BlockSequenceFirstEntry,
	BlockSequenceEntry,
	IndentlessSequenceEntry,
	BlockMappingFirstKey,
	BlockMappingKey,
	BlockMappingValue,
	FlowSequenceFirstEntry,
	FlowSequenceEntry,
	FlowSequenceEntryMappingKey,
	FlowSequenceEntryMappingValue,
	FlowSequenceEntryMappingEnd,
	FlowMappingFirstKey,
	FlowMappingKey,
	FlowMappingValue,
	FlowMappingEmptyValue,
	End
}
