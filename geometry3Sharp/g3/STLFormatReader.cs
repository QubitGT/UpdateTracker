using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class STLFormatReader : MeshFormatReader
{
	public List<string> SupportedExtensions => new List<string> { "stl" };

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
		bool num = Util.IsBinaryStream(stream, 500);
		stream.Seek(0L, SeekOrigin.Begin);
		STLReader sTLReader = new STLReader();
		sTLReader.warningEvent += messages;
		if (!num)
		{
			return sTLReader.Read(new StreamReader(stream), options, builder);
		}
		return sTLReader.Read(new BinaryReader(stream), options, builder);
	}
}
