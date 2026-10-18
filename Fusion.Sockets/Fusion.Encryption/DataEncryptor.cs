#define TRACE
#define DEBUG
using System;
using System.IO;
using System.Security.Cryptography;

namespace Fusion.Encryption;

public class DataEncryptor : IDataEncryption, IDisposable
{
	private const int TempBufferLength = 4096;

	private const int AesKeySize = 32;

	private const int HMACKeySize = 32;

	private const int IVSize = 16;

	private const int HASHSize = 32;

	private Aes _cryptoProvider;

	private HMACSHA256 _hmacsha256;

	private RandomNumberGenerator _rng;

	private byte[] _encryptBufferEncrypt;

	private byte[] _encryptBufferDecrypt;

	private byte[] _aesKey;

	private readonly byte[] _ivEncryptBuffer = new byte[16];

	private readonly byte[] _ivDecryptBuffer = new byte[16];

	public void Setup(byte[] key)
	{
		Assert.Check(key.Length == 64, "key.Length == AesKeySize + HMACKeySize");
		_aesKey = new byte[32];
		byte[] array = new byte[32];
		Buffer.BlockCopy(key, 0, _aesKey, 0, 32);
		Buffer.BlockCopy(key, 32, array, 0, 32);
		_cryptoProvider = BuildAesProvider(_aesKey);
		_hmacsha256 = BuildHMACSHA256(array);
		_rng = RandomNumberGenerator.Create();
		_encryptBufferEncrypt = new byte[4096];
		_encryptBufferDecrypt = new byte[4096];
	}

	public byte[] GenerateKey()
	{
		byte[] array = new byte[64];
		using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
		{
			randomNumberGenerator.GetBytes(array);
		}
		return array;
	}

	public unsafe bool EncryptData(byte* buffer, ref int bufferLength, int capacity)
	{
		if (buffer == null || bufferLength == 0 || capacity == 0)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Unable to encrypt data, invalid buffer");
			return false;
		}
		if (_cryptoProvider == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Encryption Provider was not initialized");
			return false;
		}
		byte[] bufferEncrypt = GetBufferEncrypt();
		if (bufferEncrypt == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Unable to allocate memory for encryption");
			return false;
		}
		int num;
		using (UnmanagedMemoryStream unmanagedMemoryStream = new UnmanagedMemoryStream(buffer, bufferLength))
		{
			using MemoryStream memoryStream = new MemoryStream(bufferEncrypt, writable: true);
			_rng.GetBytes(_ivEncryptBuffer);
			memoryStream.Write(_ivEncryptBuffer, 0, _ivEncryptBuffer.Length);
			Assert.Check(16 == memoryStream.Position, "IVSize == memoryStream.Position");
			using ICryptoTransform transform = _cryptoProvider.CreateEncryptor(_aesKey, _ivEncryptBuffer);
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			unmanagedMemoryStream.CopyTo(cryptoStream, bufferLength);
			cryptoStream.FlushFinalBlock();
			num = (int)memoryStream.Position;
		}
		Assert.Check(capacity >= num, "Unable to copy result, original buffer is too short. {0} vs {1}", capacity, num);
		fixed (byte* source = bufferEncrypt)
		{
			Native.MemCpy(buffer, source, num);
		}
		bufferLength = num;
		return true;
	}

	public unsafe bool DecryptData(byte* buffer, ref int bufferLength, int capacity)
	{
		if (buffer == null || bufferLength == 0 || capacity == 0)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Unable to encrypt data, invalid buffer");
			return false;
		}
		if (_cryptoProvider == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Encryption Provider was not initialized");
			return false;
		}
		byte[] bufferDecrypt = GetBufferDecrypt();
		if (bufferDecrypt == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Unable to allocate memory for encryption");
			return false;
		}
		int num2;
		using (UnmanagedMemoryStream unmanagedMemoryStream = new UnmanagedMemoryStream(buffer, bufferLength))
		{
			int num = unmanagedMemoryStream.Read(_ivDecryptBuffer, 0, 16);
			Assert.Check(num == 16, "read == IVSize");
			bufferLength -= 16;
			using MemoryStream memoryStream = new MemoryStream(bufferDecrypt, writable: true);
			using ICryptoTransform transform = _cryptoProvider.CreateDecryptor(_aesKey, _ivDecryptBuffer);
			using CryptoStream cryptoStream = new CryptoStream(unmanagedMemoryStream, transform, CryptoStreamMode.Read);
			cryptoStream.CopyTo(memoryStream, bufferLength);
			num2 = (int)memoryStream.Position;
		}
		Assert.Check(capacity >= num2, "Unable to copy result, original buffer is too short");
		fixed (byte* source = bufferDecrypt)
		{
			Native.MemCpy(buffer, source, num2);
		}
		bufferLength = num2;
		return true;
	}

	public unsafe bool ComputeHash(byte* buffer, ref int bufferLength, int capacity)
	{
		if (_hmacsha256 == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Hasher was not initialized");
			return false;
		}
		_hmacsha256.Initialize();
		using (UnmanagedMemoryStream inputStream = new UnmanagedMemoryStream(buffer, bufferLength))
		{
			byte[] array = _hmacsha256.ComputeHash(inputStream);
			Assert.Check(array.Length == 32, "hash.Length == HASHSize");
			Assert.Check(capacity >= bufferLength + 32, "Unable to copy hash, original buffer is too short");
			Native.CopyFromArray(buffer + bufferLength, array);
		}
		bufferLength += 32;
		return true;
	}

	public unsafe bool VerifyHash(byte* buffer, ref int bufferLength, int capacity)
	{
		if (_hmacsha256 == null)
		{
			InternalLogStreams.LogTraceEncryption?.Warn("Hasher was not initialized");
			return false;
		}
		_hmacsha256.Initialize();
		using UnmanagedMemoryStream inputStream = new UnmanagedMemoryStream(buffer, bufferLength - 32);
		byte[] array = _hmacsha256.ComputeHash(inputStream);
		bufferLength -= 32;
		fixed (byte* ptr = array)
		{
			return Native.MemCmp(buffer + bufferLength, ptr, 32) == 0;
		}
	}

	private static Aes BuildAesProvider(byte[] key)
	{
		Aes aes = Aes.Create();
		aes.Key = key;
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		return aes;
	}

	private static HMACSHA256 BuildHMACSHA256(byte[] key)
	{
		return new HMACSHA256(key);
	}

	private byte[] GetBufferEncrypt()
	{
		Array.Clear(_encryptBufferEncrypt, 0, 4096);
		return _encryptBufferEncrypt;
	}

	private byte[] GetBufferDecrypt()
	{
		Array.Clear(_encryptBufferDecrypt, 0, 4096);
		return _encryptBufferDecrypt;
	}

	public void Dispose()
	{
		InternalLogStreams.LogTraceEncryption?.Log("Disposing DataEncryptor...");
		_cryptoProvider?.Dispose();
		_cryptoProvider = null;
		_hmacsha256?.Dispose();
		_hmacsha256 = null;
		_rng?.Dispose();
		_rng = null;
		_encryptBufferEncrypt = null;
		_encryptBufferDecrypt = null;
	}
}
