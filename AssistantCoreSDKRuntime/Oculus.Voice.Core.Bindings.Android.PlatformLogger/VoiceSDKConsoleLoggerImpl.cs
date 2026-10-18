using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.Voice.Core.Utilities;
using UnityEngine;
using UnityEngine.Device;

namespace Oculus.Voice.Core.Bindings.Android.PlatformLogger;

public class VoiceSDKConsoleLoggerImpl : IVoiceSDKLogger
{
	private static readonly string TAG = "VoiceSDKConsoleLogger";

	private bool loggedFirstTranscriptionTime;

	public bool IsUsingPlatformIntegration { get; set; }

	public string WitApplication { get; set; }

	public string PackageName { get; }

	public bool ShouldLogToConsole { get; set; }

	public VoiceSDKConsoleLoggerImpl()
	{
		PackageName = UnityEngine.Device.Application.identifier;
	}

	public void LogInteractionStart(string requestId, string witApi)
	{
		if (ShouldLogToConsole)
		{
			loggedFirstTranscriptionTime = false;
			Debug.Log(TAG + ": Interaction started with request ID: " + requestId);
			Debug.Log(TAG + ": WitApi: " + witApi);
			Debug.Log(TAG + ": request_start_time: " + DateTimeUtility.ElapsedMilliseconds);
			Debug.Log(TAG + ": WitAppID: " + WitApplication);
			Debug.Log(TAG + ": PackageName: " + PackageName);
		}
	}

	public void LogInteractionEndSuccess()
	{
		if (ShouldLogToConsole)
		{
			Debug.Log(TAG + ": Interaction finished successfully");
			Debug.Log(TAG + ": request_end_time: " + DateTimeUtility.ElapsedMilliseconds);
		}
	}

	public void LogInteractionEndFailure(string errorMessage)
	{
		if (ShouldLogToConsole)
		{
			Debug.Log(TAG + ": Interaction finished with error: " + errorMessage);
			Debug.Log(TAG + ": request_end_time: " + DateTimeUtility.ElapsedMilliseconds);
		}
	}

	public void LogInteractionPoint(string interactionPoint)
	{
		if (ShouldLogToConsole)
		{
			Debug.Log(TAG + ": Interaction point: " + interactionPoint);
			Debug.Log(TAG + ": " + interactionPoint + "_start_time: " + DateTimeUtility.ElapsedMilliseconds);
		}
	}

	public void LogAnnotation(string annotationKey, string annotationValue)
	{
		if (ShouldLogToConsole)
		{
			Debug.Log(TAG + ": Logging key-value pair: " + annotationKey + "::" + annotationValue);
		}
	}

	public void LogFirstTranscriptionTime()
	{
		if (!loggedFirstTranscriptionTime)
		{
			loggedFirstTranscriptionTime = true;
			LogInteractionPoint("firstPartialTranscriptionTime");
		}
	}
}
