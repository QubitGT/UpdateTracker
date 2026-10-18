using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace g3;

public class StandardMeshReader
{
	public bool ReadInvariantCulture = true;

	private List<MeshFormatReader> Readers = new List<MeshFormatReader>();

	public IMeshBuilder MeshBuilder { get; set; }

	public event ParsingMessagesHandler warningEvent;

	public StandardMeshReader(bool bIncludeDefaultReaders = true)
	{
		Readers = new List<MeshFormatReader>();
		MeshBuilder = new DMesh3Builder();
		if (bIncludeDefaultReaders)
		{
			Readers.Add(new OBJFormatReader());
			Readers.Add(new STLFormatReader());
			Readers.Add(new OFFFormatReader());
			Readers.Add(new BinaryG3FormatReader());
			Readers.Add(new DS3FormatReader());
			Readers.Add(new PLYFormatReader());
		}
	}

	public bool SupportsFormat(string sExtension)
	{
		foreach (MeshFormatReader reader in Readers)
		{
			foreach (string supportedExtension in reader.SupportedExtensions)
			{
				if (supportedExtension.Equals(sExtension, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddFormatHandler(MeshFormatReader reader)
	{
		foreach (string supportedExtension in reader.SupportedExtensions)
		{
			if (SupportsFormat(supportedExtension))
			{
				throw new Exception("StandardMeshReader.AddFormatHandler: format " + supportedExtension + " is already registered!");
			}
		}
		Readers.Add(reader);
	}

	public IOReadResult Read(string sFilename, ReadOptions options)
	{
		if (MeshBuilder == null)
		{
			return new IOReadResult(IOCode.GenericReaderError, "MeshBuilder is null!");
		}
		string extension = Path.GetExtension(sFilename);
		if (extension.Length < 2)
		{
			return new IOReadResult(IOCode.InvalidFilenameError, "filename " + sFilename + " does not contain valid extension");
		}
		extension = extension.Substring(1);
		MeshFormatReader meshFormatReader = null;
		foreach (MeshFormatReader reader in Readers)
		{
			foreach (string supportedExtension in reader.SupportedExtensions)
			{
				if (supportedExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
				{
					meshFormatReader = reader;
				}
			}
			if (meshFormatReader != null)
			{
				break;
			}
		}
		if (meshFormatReader == null)
		{
			return new IOReadResult(IOCode.UnknownFormatError, "format " + extension + " is not supported");
		}
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			}
			IOReadResult result = meshFormatReader.ReadFile(sFilename, MeshBuilder, options, on_warning);
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return result;
		}
		catch (Exception ex)
		{
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return new IOReadResult(IOCode.GenericReaderError, "Unknown error : exception : " + ex.Message);
		}
	}

	public IOReadResult Read(Stream stream, string sExtension, ReadOptions options)
	{
		if (MeshBuilder == null)
		{
			return new IOReadResult(IOCode.GenericReaderError, "MeshBuilder is null!");
		}
		MeshFormatReader meshFormatReader = null;
		foreach (MeshFormatReader reader in Readers)
		{
			foreach (string supportedExtension in reader.SupportedExtensions)
			{
				if (supportedExtension.Equals(sExtension, StringComparison.OrdinalIgnoreCase))
				{
					meshFormatReader = reader;
				}
			}
			if (meshFormatReader != null)
			{
				break;
			}
		}
		if (meshFormatReader == null)
		{
			return new IOReadResult(IOCode.UnknownFormatError, "format " + sExtension + " is not supported");
		}
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			}
			IOReadResult result = meshFormatReader.ReadFile(stream, MeshBuilder, options, on_warning);
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return result;
		}
		catch (Exception ex)
		{
			if (ReadInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return new IOReadResult(IOCode.GenericReaderError, "Unknown error : exception : " + ex.Message);
		}
	}

	public static IOReadResult ReadFile(string sFilename, ReadOptions options, IMeshBuilder builder)
	{
		return new StandardMeshReader
		{
			MeshBuilder = builder
		}.Read(sFilename, options);
	}

	public static IOReadResult ReadFile(Stream stream, string sExtension, ReadOptions options, IMeshBuilder builder)
	{
		return new StandardMeshReader
		{
			MeshBuilder = builder
		}.Read(stream, sExtension, options);
	}

	public static DMesh3 ReadMesh(string sFilename)
	{
		DMesh3Builder dMesh3Builder = new DMesh3Builder();
		if (ReadFile(sFilename, ReadOptions.Defaults, dMesh3Builder).code != 0)
		{
			return null;
		}
		return dMesh3Builder.Meshes[0];
	}

	public static DMesh3 ReadMesh(Stream stream, string sExtension)
	{
		DMesh3Builder dMesh3Builder = new DMesh3Builder();
		if (ReadFile(stream, sExtension, ReadOptions.Defaults, dMesh3Builder).code != 0)
		{
			return null;
		}
		return dMesh3Builder.Meshes[0];
	}

	private void on_warning(string message, object extra_data)
	{
		if (this.warningEvent != null)
		{
			this.warningEvent(message, extra_data);
		}
	}
}
