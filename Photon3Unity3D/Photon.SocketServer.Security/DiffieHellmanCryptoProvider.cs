using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Photon.SocketServer.Security;

internal class DiffieHellmanCryptoProvider : ICryptoProvider, IDisposable
{
	private static readonly BigInteger primeRoot = new BigInteger(OakleyGroups.Generator);

	private readonly BigInteger prime;

	private readonly BigInteger secret;

	private readonly BigInteger publicKey;

	private Rijndael crypto;

	private byte[] sharedKey;

	public bool IsInitialized => crypto != null;

	public byte[] PublicKey
	{
		get
		{
			BigInteger bigInteger = publicKey;
			return MsBigIntArrayToPhotonBigIntArray(bigInteger.ToByteArray());
		}
	}

	public DiffieHellmanCryptoProvider()
	{
		prime = new BigInteger(OakleyGroups.OakleyPrime768);
		secret = GenerateRandomSecret(160);
		publicKey = CalculatePublicKey();
	}

	public DiffieHellmanCryptoProvider(byte[] cryptoKey)
	{
		crypto = new RijndaelManaged();
		crypto.Key = cryptoKey;
		crypto.IV = new byte[16];
		crypto.Padding = PaddingMode.PKCS7;
	}

	public void DeriveSharedKey(byte[] otherPartyPublicKey)
	{
		otherPartyPublicKey = PhotonBigIntArrayToMsBigIntArray(otherPartyPublicKey);
		BigInteger otherPartyPublicKey2 = new BigInteger(otherPartyPublicKey);
		sharedKey = MsBigIntArrayToPhotonBigIntArray(CalculateSharedKey(otherPartyPublicKey2).ToByteArray());
		byte[] key;
		using (SHA256 sHA = new SHA256Managed())
		{
			key = sHA.ComputeHash(sharedKey);
		}
		crypto = new RijndaelManaged();
		crypto.Key = key;
		crypto.IV = new byte[16];
		crypto.Padding = PaddingMode.PKCS7;
	}

	private byte[] PhotonBigIntArrayToMsBigIntArray(byte[] array)
	{
		Array.Reverse((Array)array);
		if ((array[^1] & 0x80) == 128)
		{
			byte[] array2 = new byte[array.Length + 1];
			Buffer.BlockCopy(array, 0, array2, 0, array.Length);
			return array2;
		}
		return array;
	}

	private byte[] MsBigIntArrayToPhotonBigIntArray(byte[] array)
	{
		Array.Reverse((Array)array);
		if (array[0] == 0)
		{
			byte[] array2 = new byte[array.Length - 1];
			Buffer.BlockCopy(array, 1, array2, 0, array.Length - 1);
			return array2;
		}
		return array;
	}

	public byte[] Encrypt(byte[] data)
	{
		return Encrypt(data, 0, data.Length);
	}

	public byte[] Encrypt(byte[] data, int offset, int count)
	{
		using ICryptoTransform cryptoTransform = crypto.CreateEncryptor();
		return cryptoTransform.TransformFinalBlock(data, offset, count);
	}

	public byte[] Decrypt(byte[] data)
	{
		return Decrypt(data, 0, data.Length);
	}

	public byte[] Decrypt(byte[] data, int offset, int count)
	{
		using ICryptoTransform cryptoTransform = crypto.CreateDecryptor();
		return cryptoTransform.TransformFinalBlock(data, offset, count);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (disposing)
		{
		}
	}

	private BigInteger CalculatePublicKey()
	{
		return BigInteger.ModPow(primeRoot, secret, prime);
	}

	private BigInteger CalculateSharedKey(BigInteger otherPartyPublicKey)
	{
		return BigInteger.ModPow(otherPartyPublicKey, secret, prime);
	}

	private BigInteger GenerateRandomSecret(int secretLength)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[secretLength / 8];
		BigInteger bigInteger;
		do
		{
			rNGCryptoServiceProvider.GetBytes(array);
			bigInteger = new BigInteger(array);
		}
		while (bigInteger >= prime - 1 || bigInteger < 2L);
		return bigInteger;
	}
}
