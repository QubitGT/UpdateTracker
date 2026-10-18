namespace UnityEngine.Localization;

internal static class AssetAddress
{
	private const string k_SubAssetEntryStartBracket = "[";

	private const string k_SubAssetEntryEndBracket = "]";

	public static bool IsSubAsset(string address)
	{
		return address?.EndsWith("]") ?? false;
	}

	public static string GetGuid(string address)
	{
		if (!IsSubAsset(address))
		{
			return address;
		}
		return address[..address.IndexOf("[")];
	}

	public static string GetSubAssetName(string address)
	{
		if (!IsSubAsset(address))
		{
			return null;
		}
		int num = address.IndexOf("[");
		int length = address.Length - num - 2;
		return address.Substring(num + 1, length);
	}

	public static string FormatAddress(string guid, string subAssetName)
	{
		return guid + "[" + subAssetName + "]";
	}
}
