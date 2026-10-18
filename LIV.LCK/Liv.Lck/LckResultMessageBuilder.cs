using System.Collections.Generic;

namespace Liv.Lck;

internal static class LckResultMessageBuilder
{
	public static string BuildCameraIdNotFoundMessage(string missingCameraId, List<ILckCamera> existingCameras)
	{
		string text = "";
		for (int i = 0; i < existingCameras.Count; i++)
		{
			ILckCamera arg = existingCameras[i];
			text = ((i != existingCameras.Count - 1) ? (text + $"{arg}, ") : (text + $"{arg}"));
		}
		return "Camera with ID \"" + missingCameraId + "\" not found. The known Camera IDs are: [" + text + "]. Have you miss-spelt, or forgotten to set the ID on your LckCamera component?";
	}

	public static string BuildMonitorIdNotFoundMessage(string missingMonitorId, List<ILckMonitor> existingMonitors)
	{
		string text = "";
		for (int i = 0; i < existingMonitors.Count; i++)
		{
			ILckMonitor arg = existingMonitors[i];
			text = ((i != existingMonitors.Count - 1) ? (text + $"{arg}, ") : (text + $"{arg}"));
		}
		return "Monitor with ID \"" + missingMonitorId + "\" not found. The known Monitor IDs are: [" + text + "]. Have you miss-spelt, or forgotten to set the ID on your LckMonitor component?";
	}
}
