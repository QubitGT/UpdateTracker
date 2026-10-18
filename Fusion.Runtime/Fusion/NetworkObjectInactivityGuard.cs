#define TRACE
using System;
using UnityEngine;

namespace Fusion;

[AddComponentMenu("")]
internal class NetworkObjectInactivityGuard : Behaviour
{
	[NonSerialized]
	public NetworkObject Object;

	private void OnEnable()
	{
		if (!BehaviourUtils.IsNull(Object))
		{
			NetworkRunner runner = Object.Runner;
			Object = null;
			if ((bool)runner)
			{
				InternalLogStreams.LogTraceObject?.Log(Object, "NetworkObjectInactivityGuard: object has been activated, returning to a pool");
				runner._inactivityGuardPool.Push(this);
				base.transform.SetParent(runner.transform);
			}
			else
			{
				InternalLogStreams.LogTraceObject?.Log(Object, "NetworkObjectInactivityGuard: object has been activated but there's no runner, destroying");
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		if (!BehaviourUtils.IsNull(Object) && !Object.RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.HadAwake))
		{
			InternalLogStreams.LogTraceObject?.Log(Object, "NetworkObjectInactivityGuard: Invoking OnDestroyNeverActive");
			Object.OnDestroyNeverActive();
		}
	}
}
