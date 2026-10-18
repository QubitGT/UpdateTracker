using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace Oculus.Voice.Core.Bindings.Android.PlatformLogger;

public class VoiceSDKLoggerBinding : BaseServiceBinding
{
	private readonly TaskScheduler _scheduler;

	[Preserve]
	public VoiceSDKLoggerBinding(AndroidJavaObject loggerInstance)
		: base(loggerInstance)
	{
		_scheduler = TaskScheduler.FromCurrentSynchronizationContext();
	}

	public void Connect()
	{
		Call<bool>("connect", Array.Empty<object>());
	}

	public void LogInteractionStart(string requestId, string startTime)
	{
		Call("logInteractionStart", requestId, startTime);
	}

	public void LogInteractionEndSuccess(string endTime)
	{
		Call("logInteractionEndSuccess", endTime);
	}

	public void LogInteractionEndFailure(string endTime, string errorMessage)
	{
		Call("logInteractionEndFailure", endTime, errorMessage);
	}

	public void LogInteractionPoint(string interactionPoint, string time)
	{
		Call("logInteractionPoint", interactionPoint, time);
	}

	public void LogAnnotation(string annotationKey, string annotationValue)
	{
		Call("logAnnotation", annotationKey, annotationValue);
	}

	private Task Call(string methodName, params object[] parameters)
	{
		Task task = new Task(delegate
		{
			binding.Call(methodName, parameters);
		});
		task.Start(_scheduler);
		return task;
	}

	private Task<TReturnType> Call<TReturnType>(string methodName, params object[] parameters)
	{
		Task<TReturnType> task = new Task<TReturnType>(() => binding.Call<TReturnType>(methodName, parameters));
		task.Start(_scheduler);
		return task;
	}
}
