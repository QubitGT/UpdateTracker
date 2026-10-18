using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class DS3FormatReader : MeshFormatReader
{
	public List<string> SupportedExtensions => new List<string> { "3ds" };

	public IOReadResult ReadFile(string sFilename, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		try
		{
			using FileStream stream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
			return ReadFile(stream, builder, options, messages);
		}
		catch (Exception ex)
		{
			return new IOReadResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for reading : " + ex.Message);
		}
	}

	public IOReadResult ReadFile(Stream stream, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		DS3Reader dS3Reader = new DS3Reader();
		dS3Reader.warningEvent += messages;
		return dS3Reader.Read(new BinaryReader(stream), options, builder);
	}
}
