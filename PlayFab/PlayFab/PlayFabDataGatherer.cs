using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlayFab;

public class PlayFabDataGatherer
{
	public string ProductName;

	public string ProductBundle;

	public string Version;

	public string Company;

	public RuntimePlatform Platform;

	public bool GraphicsMultiThreaded;

	public GraphicsDeviceType GraphicsType;

	public string DataPath;

	public string PersistentDataPath;

	public string StreamingAssetsPath;

	public int TargetFrameRate;

	public string UnityVersion;

	public bool RunInBackground;

	public string DeviceModel;

	public DeviceType DeviceType;

	public string DeviceUniqueId;

	public string OperatingSystem;

	public int GraphicsDeviceId;

	public string GraphicsDeviceName;

	public int GraphicsMemorySize;

	public int GraphicsShaderLevel;

	public int SystemMemorySize;

	public int ProcessorCount;

	public int ProcessorFrequency;

	public string ProcessorType;

	public bool SupportsAccelerometer;

	public bool SupportsGyroscope;

	public bool SupportsLocationService;

	public PlayFabDataGatherer()
	{
		ProductName = Application.productName;
		Version = Application.version;
		Company = Application.companyName;
		Platform = Application.platform;
		GraphicsMultiThreaded = SystemInfo.graphicsMultiThreaded;
		GraphicsType = SystemInfo.graphicsDeviceType;
		DataPath = Application.dataPath;
		PersistentDataPath = Application.persistentDataPath;
		StreamingAssetsPath = Application.streamingAssetsPath;
		TargetFrameRate = Application.targetFrameRate;
		UnityVersion = Application.unityVersion;
		DeviceModel = SystemInfo.deviceModel;
		DeviceType = SystemInfo.deviceType;
		DeviceUniqueId = PlayFabSettings.DeviceUniqueIdentifier;
		OperatingSystem = SystemInfo.operatingSystem;
		GraphicsDeviceId = SystemInfo.graphicsDeviceID;
		GraphicsDeviceName = SystemInfo.graphicsDeviceName;
		GraphicsMemorySize = SystemInfo.graphicsMemorySize;
		GraphicsShaderLevel = SystemInfo.graphicsShaderLevel;
		SystemMemorySize = SystemInfo.systemMemorySize;
		ProcessorCount = SystemInfo.processorCount;
		ProcessorFrequency = SystemInfo.processorFrequency;
		ProcessorType = SystemInfo.processorType;
		SupportsAccelerometer = SystemInfo.supportsAccelerometer;
		SupportsGyroscope = SystemInfo.supportsGyroscope;
		SupportsLocationService = SystemInfo.supportsLocationService;
	}

	public string GenerateReport()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Logging System Info: ========================================\n");
		FieldInfo[] fields = GetType().GetTypeInfo().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			string arg = fieldInfo.GetValue(this).ToString();
			stringBuilder.AppendFormat("System Info - {0}: {1}\n", fieldInfo.Name, arg);
		}
		return stringBuilder.ToString();
	}
}
