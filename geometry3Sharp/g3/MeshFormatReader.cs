using System.Collections.Generic;
using System.IO;

namespace g3;

public interface MeshFormatReader
{
	List<string> SupportedExtensions { get; }

	IOReadResult ReadFile(string sFilename, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler warnings);

	IOReadResult ReadFile(Stream stream, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler warnings);
}
