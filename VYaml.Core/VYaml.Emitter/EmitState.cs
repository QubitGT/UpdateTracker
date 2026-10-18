namespace VYaml.Emitter;

internal enum EmitState
{
	None,
	BlockSequenceEntry,
	BlockMappingKey,
	BlockMappingValue,
	FlowSequenceEntry,
	FlowMappingKey,
	FlowMappingValue
}
