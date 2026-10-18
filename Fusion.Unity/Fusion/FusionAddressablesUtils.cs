using System;
using UnityEngine.AddressableAssets;

namespace Fusion;

public static class FusionAddressablesUtils
{
	public static bool TryParseAddress(string address, out string mainPart, out string subObjectName)
	{
		if (string.IsNullOrEmpty(address))
		{
			mainPart = null;
			subObjectName = null;
			return false;
		}
		int num = address.IndexOf('[');
		int num2 = address.IndexOf(']');
		if (num == 0 || (num < 0 && num2 >= 0) || (num > 0 && num2 != address.Length - 1) || (num > 0 && num2 - num <= 1))
		{
			mainPart = null;
			subObjectName = null;
			return false;
		}
		if (num < 0)
		{
			mainPart = address;
			subObjectName = null;
			return true;
		}
		mainPart = address.Substring(0, num);
		subObjectName = address.Substring(num + 1, address.Length - num - 2);
		return true;
	}

	public static AssetReference CreateAssetReference(string address)
	{
		if (TryParseAddress(address, out var mainPart, out var subObjectName))
		{
			if (Guid.TryParse(mainPart, out var _))
			{
				return new AssetReference(mainPart)
				{
					SubObjectName = subObjectName
				};
			}
			throw new ArgumentException("The main part of the address is not a guid: " + mainPart, "address");
		}
		throw new ArgumentException("Not a valid address: " + address, "address");
	}
}
