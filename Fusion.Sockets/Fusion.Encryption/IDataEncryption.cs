using System;

namespace Fusion.Encryption;

public interface IDataEncryption : IDisposable
{
	void Setup(byte[] key);

	byte[] GenerateKey();

	unsafe bool EncryptData(byte* buffer, ref int bufferLength, int capacity);

	unsafe bool DecryptData(byte* buffer, ref int bufferLength, int capacity);

	unsafe bool ComputeHash(byte* buffer, ref int bufferLength, int capacity);

	unsafe bool VerifyHash(byte* buffer, ref int bufferLength, int capacity);
}
