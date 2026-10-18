using System.Linq;

namespace Fusion.Encryption;

internal class EncryptionToken
{
	public byte[] Key;

	public byte[] KeyEncrypted;

	public override string ToString()
	{
		return "[EncryptionToken: Key=" + BinUtils.BytesToHex(Key?.Take(5).ToArray()) + ", KeyEncrypted=" + BinUtils.BytesToHex(KeyEncrypted?.Take(5).ToArray()) + "]";
	}
}
