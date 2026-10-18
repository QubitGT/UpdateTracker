using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class OBJFormatReader : MeshFormatReader
{
	public List<string> SupportedExtensions => new List<string> { "obj" };

	public IOReadResult ReadFile(string sFilename, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		try
		{
			using FileStream stream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
			OBJReader oBJReader = new OBJReader();
			if (options.ReadMaterials)
			{
				oBJReader.MTLFileSearchPaths.Add(Path.GetDirectoryName(sFilename));
			}
			oBJReader.warningEvent += messages;
			return oBJReader.Read(new StreamReader(stream), options, builder);
		}
		catch (Exception ex)
		{
			return new IOReadResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for reading : " + ex.Message);
		}
	}

	public IOReadResult ReadFile(Stream stream, IMeshBuilder builder, ReadOptions options, ParsingMessagesHandler messages)
	{
		OBJReader oBJReader = new OBJReader();
		oBJReader.warningEvent += messages;
		return oBJReader.Read(new StreamReader(stream), options, builder);
	}
}
