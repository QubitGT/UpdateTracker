using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liv.Lck;

[CreateAssetMenu(fileName = "LckQualityConfig", menuName = "LIV/LCK/QualityConfig")]
public class LckQualityConfig : ScriptableObject, ILckQualityConfig
{
	[Header("Android")]
	public List<QualityOption> BaseAndroidQualityOptions = new List<QualityOption>();

	public List<QualityOptionOverride> AndroidOptionsDeviceOverrides = new List<QualityOptionOverride>();

	[Header("Desktop")]
	public List<QualityOption> DesktopQualityOptions = new List<QualityOption>();

	public List<QualityOption> GetQualityOptionsForSystem()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.Android:
		{
			DeviceModel? currentDeviceModel = GetCurrentDeviceModel();
			if (currentDeviceModel.HasValue)
			{
				foreach (QualityOptionOverride androidOptionsDeviceOverride in AndroidOptionsDeviceOverrides)
				{
					if (androidOptionsDeviceOverride.DeviceModel == currentDeviceModel)
					{
						return androidOptionsDeviceOverride.QualityOptions;
					}
				}
			}
			return BaseAndroidQualityOptions;
		}
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
			return DesktopQualityOptions;
		default:
			throw new NotImplementedException($"LCK does not support {Application.platform} platform");
		}
	}

	private DeviceModel? GetCurrentDeviceModel()
	{
		return null;
	}
}
