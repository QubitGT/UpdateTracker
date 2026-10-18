using System.IO;

namespace g3;

public interface IMeshReader
{
	IOReadResult Read(TextReader reader, ReadOptions options, IMeshBuilder builder);

	IOReadResult Read(BinaryReader reader, ReadOptions options, IMeshBuilder builder);
}
