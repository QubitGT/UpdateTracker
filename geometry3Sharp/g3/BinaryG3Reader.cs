using System;
using System.IO;

namespace g3;

public class BinaryG3Reader : IMeshReader
{
	public IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder)
	{
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			DMesh3 dMesh = new DMesh3();
			gSerialization.Restore(dMesh, reader);
			builder.AppendNewMesh(dMesh);
		}
		return new IOReadResult(IOCode.Ok, "");
	}

	public IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder)
	{
		throw new NotSupportedException("BinaryG3Reader Writer does not support ascii mode");
	}
}
