namespace g3;

public enum IOCode
{
	Ok = 0,
	FileAccessError = 1,
	UnknownFormatError = 2,
	FormatNotSupportedError = 3,
	InvalidFilenameError = 4,
	FileParsingError = 100,
	GarbageDataError = 101,
	GenericReaderError = 102,
	GenericReaderWarning = 103,
	WriterError = 200,
	ComputingInWorkerThread = 1000
}
