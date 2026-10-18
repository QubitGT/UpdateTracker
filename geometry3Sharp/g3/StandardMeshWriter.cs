using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace g3;

public class StandardMeshWriter : IDisposable
{
	public bool WriteInvariantCulture = true;

	public Func<string, Stream> OpenStreamF = (string sFilename) => File.Open(sFilename, FileMode.Create);

	public Action<Stream> CloseStreamF = delegate(Stream stream)
	{
		stream.Close();
		stream.Dispose();
	};

	public void Dispose()
	{
	}

	public static IOWriteResult WriteMeshes(string sFilename, List<DMesh3> vMeshes, WriteOptions options)
	{
		List<WriteMesh> list = new List<WriteMesh>();
		foreach (DMesh3 vMesh in vMeshes)
		{
			list.Add(new WriteMesh(vMesh));
		}
		return new StandardMeshWriter().Write(sFilename, list, options);
	}

	public static IOWriteResult WriteFile(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		return new StandardMeshWriter().Write(sFilename, vMeshes, options);
	}

	public static IOWriteResult WriteMesh(string sFilename, IMesh mesh, WriteOptions options)
	{
		return new StandardMeshWriter().Write(sFilename, new List<WriteMesh>
		{
			new WriteMesh(mesh)
		}, options);
	}

	public IOWriteResult Write(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		Func<string, List<WriteMesh>, WriteOptions, IOWriteResult> func = null;
		string extension = Path.GetExtension(sFilename);
		if (extension.Equals(".obj", StringComparison.OrdinalIgnoreCase))
		{
			func = Write_OBJ;
		}
		else if (extension.Equals(".stl", StringComparison.OrdinalIgnoreCase))
		{
			func = Write_STL;
		}
		else if (extension.Equals(".off", StringComparison.OrdinalIgnoreCase))
		{
			func = Write_OFF;
		}
		else if (extension.Equals(".g3mesh", StringComparison.OrdinalIgnoreCase))
		{
			func = Write_G3Mesh;
		}
		if (func == null)
		{
			return new IOWriteResult(IOCode.UnknownFormatError, "format " + extension + " is not supported");
		}
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			if (WriteInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			}
			IOWriteResult result = func(sFilename, vMeshes, options);
			if (WriteInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return result;
		}
		catch (Exception ex)
		{
			if (WriteInvariantCulture)
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
			return new IOWriteResult(IOCode.WriterError, "Unknown error : exception : " + ex.Message);
		}
	}

	private IOWriteResult Write_OBJ(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		Stream stream = OpenStreamF(sFilename);
		if (stream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for writing");
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			IOWriteResult result = new OBJWriter
			{
				OpenStreamF = OpenStreamF,
				CloseStreamF = CloseStreamF
			}.Write(streamWriter, vMeshes, options);
			streamWriter.Flush();
			return result;
		}
		finally
		{
			CloseStreamF(stream);
		}
	}

	private IOWriteResult Write_OFF(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		Stream stream = OpenStreamF(sFilename);
		if (stream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for writing");
		}
		try
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			IOWriteResult result = new OFFWriter().Write(streamWriter, vMeshes, options);
			streamWriter.Flush();
			return result;
		}
		finally
		{
			CloseStreamF(stream);
		}
	}

	private IOWriteResult Write_STL(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		Stream stream = OpenStreamF(sFilename);
		if (stream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for writing");
		}
		try
		{
			if (options.bWriteBinary)
			{
				BinaryWriter binaryWriter = new BinaryWriter(stream);
				IOWriteResult result = new STLWriter().Write(binaryWriter, vMeshes, options);
				binaryWriter.Flush();
				return result;
			}
			StreamWriter streamWriter = new StreamWriter(stream);
			IOWriteResult result2 = new STLWriter().Write(streamWriter, vMeshes, options);
			streamWriter.Flush();
			return result2;
		}
		finally
		{
			CloseStreamF(stream);
		}
	}

	private IOWriteResult Write_G3Mesh(string sFilename, List<WriteMesh> vMeshes, WriteOptions options)
	{
		Stream stream = OpenStreamF(sFilename);
		if (stream == null)
		{
			return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for writing");
		}
		try
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			IOWriteResult result = new BinaryG3Writer().Write(binaryWriter, vMeshes, options);
			binaryWriter.Flush();
			return result;
		}
		finally
		{
			CloseStreamF(stream);
		}
	}
}
