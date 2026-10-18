using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Modio.FileIO;

public class MD5ComputingStreamWrapper : Stream
{
	private Stream _baseStream;

	private MD5 _md5;

	private bool _hasTransformedFinalBlock;

	public int TotalBytesRead { get; private set; }

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => _baseStream.Length;

	public override long Position
	{
		get
		{
			return _baseStream.Position;
		}
		set
		{
			_baseStream.Position = value;
		}
	}

	public MD5ComputingStreamWrapper(Stream baseStream)
	{
		_baseStream = baseStream;
		_md5 = MD5.Create();
	}

	public override void Flush()
	{
		_baseStream.Flush();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_md5.Dispose();
			_baseStream.Dispose();
		}
		base.Dispose(disposing);
	}

	public async Task<string> GetMD5HashAsync()
	{
		byte[] buffer = new byte[4096];
		while (!_hasTransformedFinalBlock && await ReadAsync(buffer, 0, buffer.Length) != 0)
		{
		}
		return BitConverter.ToString(_md5.Hash);
	}

	public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		int num = await _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
		if (num != 0)
		{
			_md5.TransformBlock(buffer, offset, num, null, 0);
		}
		else if (!_hasTransformedFinalBlock)
		{
			_hasTransformedFinalBlock = true;
			_md5.TransformFinalBlock(buffer, 0, 0);
		}
		TotalBytesRead += num;
		return num;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = _baseStream.Read(buffer, offset, count);
		if (num != 0)
		{
			_md5.TransformBlock(buffer, offset, num, null, 0);
		}
		else if (!_hasTransformedFinalBlock)
		{
			_hasTransformedFinalBlock = true;
			_md5.TransformFinalBlock(buffer, 0, 0);
		}
		TotalBytesRead += num;
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _baseStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
