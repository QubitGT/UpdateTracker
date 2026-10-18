using System;
using System.Collections.Generic;
using System.IO;

namespace g3;

public class BinaryG3Writer : IMeshWriter
{
	public IOWriteResult Write(BinaryWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		int count = vMeshes.Count;
		writer.Write(count);
		for (int i = 0; i < vMeshes.Count; i++)
		{
			gSerialization.Store((vMeshes[i].Mesh as DMesh3) ?? throw new NotImplementedException("BinaryG3Writer.Write: can only write DMesh3 meshes"), writer);
		}
		return new IOWriteResult(IOCode.Ok, "");
	}

	public IOWriteResult Write(TextWriter writer, List<WriteMesh> vMeshes, WriteOptions options)
	{
		throw new NotSupportedException("BinaryG3 Writer does not support ascii mode");
	}
}
