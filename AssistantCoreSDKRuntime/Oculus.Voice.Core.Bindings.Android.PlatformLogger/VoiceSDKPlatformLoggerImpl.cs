using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.Voice.Core.Utilities;
using UnityEngine;

namespace Oculus.Voice.Core.Bindings.Android.PlatformLogger;

public class VoiceSDKPlatformLoggerImpl : BaseAndroidConnectionImpl<VoiceSDKLoggerBinding>, IVoiceSDKLogger
{
	private VoiceSDKConsoleLoggerImpl consoleLoggerImpl = new VoiceSDKConsoleLoggerImpl();

	private bool loggedFirstTranscriptionTime;

	public bool IsUsingPlatformIntegration { get; set; }

	public string WitApplication { get; set; }

	public string PackageName { get; }

	public bool ShouldLogToConsole
	{
		get
		{
			return consoleLoggerImpl.ShouldLogToConsole;
		}
		set
		{
			consoleLoggerImpl.ShouldLogToConsole = value;
		}
	}

	public VoiceSDKPlatformLoggerImpl()
		: base("com.oculus.assistant.api.unity.logging.UnityPlatformLoggerServiceFragment")
	{
		PackageName = Application.identifier;
	}

	public override void Connect(string version)
	{
		base.Connect(version);
		if (service != null)
		{
			service.Connect();
			Debug.Log("Logging Platform integration initialization complete.");
		}
	}

	public override void Disconnect()
	{
		Debug.Log("Logging Platform integration shutdown");
		base.Disconnect();
	}

	public void LogInteractionStart(string requestId, string witApi)
	{
		loggedFirstTranscriptionTime = false;
		consoleLoggerImpl.LogInteractionStart(requestId, witApi);
		service?.LogInteractionStart(requestId, DateTimeUtility.ElapsedMilliseconds.ToString());
		LogAnnotation("isUsingPlatform", IsUsingPlatformIntegration.ToString());
		LogAnnotation("witApi", witApi);
		LogAnnotation("witAppId", WitApplication);
		LogAnnotation("package", PackageName);
	}

	public void LogInteractionEndSuccess()
	{
		consoleLoggerImpl.LogInteractionEndSuccess();
		service?.LogInteractionEndSuccess(DateTimeUtility.ElapsedMilliseconds.ToString());
	}

	public void LogInteractionEndFailure(string errorMessage)
	{
		consoleLoggerImpl.LogInteractionEndFailure(errorMessage);
		service?.LogInteractionEndFailure(DateTimeUtility.ElapsedMilliseconds.ToString(), errorMessage);
	}

	public void LogInteractionPoint(string interactionPoint)
	{
		consoleLoggerImpl.LogInteractionPoint(interactionPoint);
		service?.LogInteractionPoint(interactionPoint, DateTimeUtility.ElapsedMilliseconds.ToString());
	}

	public void LogAnnotation(string annotationKey, string annotationValue)
	{
		consoleLoggerImpl.LogAnnotation(annotationKey, annotationValue);
		service?.LogAnnotation(annotationKey, annotationValue);
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
