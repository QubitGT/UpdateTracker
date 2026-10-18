using System.Collections.Generic;
using System.IO;

namespace g3;

public interface IMeshWriter
{
	IOWriteResult Write(TextWriter writer, List<WriteMesh> vMeshes, WriteOptions options);

	IOWriteResult Write(BinaryWriter writer, List<WriteMesh> vMeshes, WriteOptions options);
}
