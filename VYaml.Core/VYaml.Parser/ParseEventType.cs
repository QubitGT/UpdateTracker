namespace VYaml.Parser;

public enum ParseEventType : byte
{
	Nothing,
	StreamStart,
	StreamEnd,
	DocumentStart,
	DocumentEnd,
	Alias,
	Scalar,
	SequenceStart,
	SequenceEnd,
	MappingStart,
	MappingEnd
}
