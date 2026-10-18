using System;
using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip;

public interface IEntryFactory
{
	INameTransform NameTransform { get; set; }

	ZipEntryFactory.TimeSetting Setting { get; }

	DateTime FixedDateTime { get; }

	ZipEntry MakeFileEntry(string fileName);

	ZipEntry MakeFileEntry(string fileName, bool useFileSystem);

	ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem);

	ZipEntry MakeDirectoryEntry(string directoryName);

	ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem);
}
