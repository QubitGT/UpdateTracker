using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.GZip;

public class GZipInputStream : InflaterInputStream
{
	protected Crc32 crc;

	private bool readGZIPHeader;

	private bool completedLastBlock;

	private string fileName;

	public GZipInputStream(Stream baseInputStream)
		: this(baseInputStream, 4096)
	{
	}

	public GZipInputStream(Stream baseInputStream, int size)
		: base(baseInputStream, new Inflater(noHeader: true), size)
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num;
		do
		{
			if (!readGZIPHeader)
			{
				try
				{
					if (!ReadHeader())
					{
						return 0;
					}
				}
				catch (Exception ex) when (completedLastBlock && (ex is GZipException || ex is EndOfStreamException))
				{
					return 0;
				}
			}
			num = base.Read(buffer, offset, count);
			if (num > 0)
			{
				crc.Update(new ArraySegment<byte>(buffer, offset, num));
			}
			if (inf.IsFinished)
			{
				ReadFooter();
			}
		}
		while (num <= 0 && count != 0);
		return num;
	}

	public string GetFilename()
	{
		return fileName;
	}

	private bool ReadHeader()
	{
		this.crc = new Crc32();
		if (inputBuffer.Available <= 0)
		{
			inputBuffer.Fill();
			if (inputBuffer.Available <= 0)
			{
				return false;
			}
		}
		Crc32 crc = new Crc32();
		byte b = inputBuffer.ReadLeByte();
		crc.Update(b);
		if (b != 31)
		{
			throw new GZipException("Error GZIP header, first magic byte doesn't match");
		}
		b = inputBuffer.ReadLeByte();
		if (b != 139)
		{
			throw new GZipException("Error GZIP header,  second magic byte doesn't match");
		}
		crc.Update(b);
		byte b2 = inputBuffer.ReadLeByte();
		if (b2 != 8)
		{
			throw new GZipException("Error GZIP header, data not in deflate format");
		}
		crc.Update(b2);
		byte b3 = inputBuffer.ReadLeByte();
		crc.Update(b3);
		if ((b3 & 0xE0u) != 0)
		{
			throw new GZipException("Reserved flag bits in GZIP header != 0");
		}
		GZipFlags gZipFlags = (GZipFlags)b3;
		for (int i = 0; i < 6; i++)
		{
			crc.Update(inputBuffer.ReadLeByte());
		}
		if (gZipFlags.HasFlag(GZipFlags.FEXTRA))
		{
			byte b4 = inputBuffer.ReadLeByte();
			byte b5 = inputBuffer.ReadLeByte();
			crc.Update(b4);
			crc.Update(b5);
			int num = (b5 << 8) | b4;
			for (int j = 0; j < num; j++)
			{
				crc.Update(inputBuffer.ReadLeByte());
			}
		}
		if (gZipFlags.HasFlag(GZipFlags.FNAME))
		{
			byte[] array = new byte[1024];
			int num2 = 0;
			int num3;
			while ((num3 = inputBuffer.ReadLeByte()) > 0)
			{
				if (num2 < 1024)
				{
					array[num2++] = (byte)num3;
				}
				crc.Update(num3);
			}
			crc.Update(num3);
			fileName = GZipConstants.Encoding.GetString(array, 0, num2);
		}
		else
		{
			fileName = null;
		}
		if (gZipFlags.HasFlag(GZipFlags.FCOMMENT))
		{
			int bval;
			while ((bval = inputBuffer.ReadLeByte()) > 0)
			{
				crc.Update(bval);
			}
			crc.Update(bval);
		}
		if (gZipFlags.HasFlag(GZipFlags.FHCRC))
		{
			byte num4 = inputBuffer.ReadLeByte();
			if (num4 < 0)
			{
				throw new EndOfStreamException("EOS reading GZIP header");
			}
			int num5 = inputBuffer.ReadLeByte();
			if (num5 < 0)
			{
				throw new EndOfStreamException("EOS reading GZIP header");
			}
			if (((num4 << 8) | num5) != ((int)crc.Value & 0xFFFF))
			{
				throw new GZipException("Header CRC value mismatch");
			}
		}
		readGZIPHeader = true;
		return true;
	}

	private void ReadFooter()
	{
		byte[] array = new byte[8];
		long num = inf.TotalOut & 0xFFFFFFFFu;
		inputBuffer.Available += inf.RemainingInput;
		inf.Reset();
		int num2 = 8;
		while (num2 > 0)
		{
			int num3 = inputBuffer.ReadClearTextBuffer(array, 8 - num2, num2);
			if (num3 <= 0)
			{
				throw new EndOfStreamException("EOS reading GZIP footer");
			}
			num2 -= num3;
		}
		int num4 = (array[0] & 0xFF) | ((array[1] & 0xFF) << 8) | ((array[2] & 0xFF) << 16) | (array[3] << 24);
		if (num4 != (int)crc.Value)
		{
			throw new GZipException("GZIP crc sum mismatch, theirs \"" + num4 + "\" and ours \"" + (int)crc.Value);
		}
		uint num5 = (array[4] & 0xFFu) | (uint)((array[5] & 0xFF) << 8) | (uint)((array[6] & 0xFF) << 16) | (uint)(array[7] << 24);
		if (num != num5)
		{
			throw new GZipException("Number of bytes mismatch in footer");
		}
		readGZIPHeader = false;
		completedLastBlock = true;
	}
}
