using System;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform.BuildingBlocks;

public class EntitlementCheck : MonoBehaviour
{
	public bool quitAppOnNotEntitled;

	public event Action UserFailedEntitlementCheck;

	public event Action UserPassedEntitlementCheck;

	private void Start()
	{
		if (quitAppOnNotEntitled)
		{
			UserFailedEntitlementCheck += QuitAppOnFailure;
		}
		PerformUserEntitlementCheck();
	}

	public void PerformUserEntitlementCheck()
	{
		if (!Core.IsInitialized())
		{
			try
			{
				Core.AsyncInitialize().OnComplete(PlatformInitializeCallback);
			}
			catch (UnityException ex)
			{
				Debug.LogError("Exception occured during OvrPlatform init - " + ex.Message);
				this.UserFailedEntitlementCheck?.Invoke();
			}
		}
	}

	public void PlatformInitializeCallback(Message<PlatformInitialize> msg)
	{
		PlatformInitialize data = msg.Data;
		if (data == null || data.Result != 0)
		{
			Debug.LogError($"OvrPlatform init resulted in failure. - {msg.Data.Result}\n{msg.GetError().Message}");
			this.UserFailedEntitlementCheck?.Invoke();
			return;
		}
		try
		{
			Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCheckCallback);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception occured during Entitlement Check - " + ex.Message);
			this.UserFailedEntitlementCheck?.Invoke();
		}
	}

	private void EntitlementCheckCallback(Message msg)
	{
		if (!msg.IsError)
		{
			Debug.Log("You are entitled to use this app.");
			this.UserPassedEntitlementCheck?.Invoke();
		}
		else
		{
			Debug.LogError("You are NOT entitled to use this app.");
			this.UserFailedEntitlementCheck?.Invoke();
		}
	}

	private void QuitAppOnFailure()
	{
		Debug.LogError("Oculus user entitlement check failed. Exiting now...");
		UnityEngine.Application.Quit();
	}
}
