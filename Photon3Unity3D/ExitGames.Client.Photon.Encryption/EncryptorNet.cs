using System;
using System.Security.Cryptography;

namespace ExitGames.Client.Photon.Encryption;

public class EncryptorNet : IPhotonEncryptor
{
	protected Aes encryptorIn;

	protected Aes encryptorOut;

	protected HMACSHA256 hmacsha256In;

	protected HMACSHA256 hmacsha256Out;

	public void Init(byte[] encryptionSecret, byte[] hmacSecret, byte[] ivBytes = null, bool chainingModeGCM = false, int mtu = 1200)
	{
		throw new NotImplementedException();
	}

	public void Encrypt2(byte[] data, int len, byte[] header, byte[] output, int outOffset, ref int outSize)
	{
		throw new NotImplementedException();
	}

	public byte[] Decrypt2(byte[] data, int offset, int len, byte[] header, out int outLen)
	{
		throw new NotImplementedException();
	}

	public int CalculateEncryptedSize(int unencryptedSize)
	{
		throw new NotImplementedException();
	}

	public int CalculateFragmentLength()
	{
		throw new NotImplementedException();
	}
}
