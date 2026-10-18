using System;
using System.Collections.Generic;
using System.Globalization;
using Backtrace.Unity.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Backtrace.Unity.Model.Attributes;

internal sealed class MachineAttributeProvider : IScopeAttributeProvider
{
	private readonly MachineIdStorage _machineIdStorage = new MachineIdStorage();

	public void GetAttributes(IDictionary<string, string> attributes)
	{
		if (attributes != null)
		{
			attributes["guid"] = _machineIdStorage.GenerateMachineId();
			IncludeGraphicCardInformation(attributes);
			IncludeOsInformation(attributes);
		}
	}

	private void IncludeOsInformation(IDictionary<string, string> attributes)
	{
		string value = SystemHelper.CpuArchitecture();
		if (!string.IsNullOrEmpty(value))
		{
			attributes["uname.machine"] = value;
		}
		attributes["uname.sysname"] = SystemHelper.Name();
		attributes["uname.version"] = Environment.OSVersion.Version.ToString();
		attributes["uname.fullname"] = SystemInfo.operatingSystem;
		attributes["uname.family"] = SystemInfo.operatingSystemFamily.ToString();
		attributes["cpu.count"] = SystemInfo.processorCount.ToString(CultureInfo.InvariantCulture);
		attributes["cpu.frequency"] = SystemInfo.processorFrequency.ToString(CultureInfo.InvariantCulture);
		attributes["cpu.brand"] = SystemInfo.processorType;
		attributes["audio.supported"] = SystemInfo.supportsAudio.ToString(CultureInfo.InvariantCulture);
		int num = Environment.TickCount;
		if (num <= 0)
		{
			num = int.MaxValue;
		}
		attributes["cpu.boottime"] = num.ToString(CultureInfo.InvariantCulture);
		attributes["hostname"] = Environment.MachineName;
		if (SystemInfo.systemMemorySize != 0)
		{
			attributes["vm.rss.size"] = ((long)SystemInfo.systemMemorySize * 1048576L).ToString(CultureInfo.InvariantCulture);
		}
	}

	private void IncludeGraphicCardInformation(IDictionary<string, string> attributes)
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
		{
			attributes["graphic.id"] = SystemInfo.graphicsDeviceID.ToString(CultureInfo.InvariantCulture);
			attributes["graphic.name"] = SystemInfo.graphicsDeviceName;
			attributes["graphic.type"] = SystemInfo.graphicsDeviceType.ToString();
			attributes["graphic.vendor"] = SystemInfo.graphicsDeviceVendor;
			attributes["graphic.vendor.id"] = SystemInfo.graphicsDeviceVendorID.ToString(CultureInfo.InvariantCulture);
			attributes["graphic.driver.version"] = SystemInfo.graphicsDeviceVersion;
			attributes["graphic.memory"] = SystemInfo.graphicsMemorySize.ToString(CultureInfo.InvariantCulture);
			attributes["graphic.multithreaded"] = SystemInfo.graphicsMultiThreaded.ToString(CultureInfo.InvariantCulture);
			attributes["graphic.shader"] = SystemInfo.graphicsShaderLevel.ToString(CultureInfo.InvariantCulture);
			attributes["graphic.topUv"] = SystemInfo.graphicsUVStartsAtTop.ToString(CultureInfo.InvariantCulture);
		}
	}
}
