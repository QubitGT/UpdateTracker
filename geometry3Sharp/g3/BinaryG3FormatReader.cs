using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class BinaryG3FormatReader : MeshFormatReader
{
	public List<string> SupportedExtensions => new List<string> { "g3mesh" };

	public IOReadResult ReadFile(string sFilename, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		try
		{
			using FileStream stream = File.Open(sFilename, FileMode.Open, FileAccess.Read);
			return ReadFile(stream, builder, options, messages);
		}
		catch (Exception ex)
		{
			return new IOReadResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for reading : " + ex.Message);
		}
	}

	public IOReadResult ReadFile(Stream stream, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		return new BinaryG3Reader().Read(new BinaryReader(stream), options, builder);
	}
}
