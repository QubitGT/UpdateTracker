using System;
using System.Linq;
using System.Net.NetworkInformation;
using Backtrace.Unity.Extensions;
using UnityEngine;

namespace Backtrace.Unity.Model;

internal class MachineIdStorage
{
	internal const string MachineIdentifierKey = "backtrace-machine-id";

	internal string GenerateMachineId()
	{
		string text = FetchMachineIdFromStorage();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		string text2 = UseUnityIdentifier();
		if (!GuidHelper.IsNullOrEmpty(text2))
		{
			StoreMachineId(text2);
			return text2;
		}
		string text3 = UseNetworkingIdentifier();
		if (!GuidHelper.IsNullOrEmpty(text3))
		{
			StoreMachineId(text3);
			return text3;
		}
		string text4 = Guid.NewGuid().ToString();
		StoreMachineId(text4);
		return text4;
	}

	private string FetchMachineIdFromStorage()
	{
		return PlayerPrefs.GetString("backtrace-machine-id");
	}

	private void StoreMachineId(string machineId)
	{
		PlayerPrefs.SetString("backtrace-machine-id", machineId);
	}

	protected virtual string UseUnityIdentifier()
	{
		if (SystemInfo.deviceUniqueIdentifier == "n/a")
		{
			return null;
		}
		return SystemInfo.deviceUniqueIdentifier;
	}

	protected virtual string UseNetworkingIdentifier()
	{
		foreach (NetworkInterface item in from n in NetworkInterface.GetAllNetworkInterfaces()
			where n.OperationalStatus == OperationalStatus.Up
			select n)
		{
			PhysicalAddress physicalAddress = item.GetPhysicalAddress();
			if (physicalAddress != null)
			{
				string text = physicalAddress.ToString();
				if (!string.IsNullOrEmpty(text))
				{
					return GuidHelper.FromLong(Convert.ToInt64(text.Replace(":", string.Empty), 16)).ToString();
				}
			}
		}
		return null;
	}
}
