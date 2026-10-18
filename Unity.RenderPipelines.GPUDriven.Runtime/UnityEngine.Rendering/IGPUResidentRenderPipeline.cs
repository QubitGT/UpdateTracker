namespace UnityEngine.Rendering;

public interface IGPUResidentRenderPipeline
{
	GPUResidentDrawerSettings gpuResidentDrawerSettings { get; }

	GPUResidentDrawerMode gpuResidentDrawerMode { get; set; }

	static void ReinitializeGPUResidentDrawer()
	{
		GPUResidentDrawer.Reinitialize();
	}

	bool IsGPUResidentDrawerSupportedBySRP(bool logReason = false)
	{
		string message;
		LogType severity;
		bool flag = IsGPUResidentDrawerSupportedBySRP(out message, out severity);
		if (logReason && !flag)
		{
			GPUResidentDrawer.LogMessage(message, severity);
		}
		return flag;
	}

	bool IsGPUResidentDrawerSupportedBySRP(out string message, out LogType severity)
	{
		message = string.Empty;
		severity = LogType.Log;
		return true;
	}

	static bool IsGPUResidentDrawerSupportedByProjectConfiguration(bool logReason = false)
	{
		string message;
		LogType severity;
		bool result = GPUResidentDrawer.IsProjectSupported(out message, out severity);
		if (logReason && !string.IsNullOrEmpty(message))
		{
			Debug.LogWarning(message);
		}
		return result;
	}

	static bool IsGPUResidentDrawerEnabled()
	{
		return GPUResidentDrawer.IsEnabled();
	}
}
