using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class PLYFormatReader : MeshFormatReader
{
	public List<string> SupportedExtensions => new List<string> { "ply" };

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
		PLYReader pLYReader = new PLYReader();
		pLYReader.warningEvent += messages;
		return pLYReader.Read(new StreamReader(stream), options, builder);
	}
}
