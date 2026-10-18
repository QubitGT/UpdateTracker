using System;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction;

public class FingerFeatureStateProviderRef : MonoBehaviour, IFingerFeatureStateProvider
{
	[SerializeField]
	[Interface(typeof(IFingerFeatureStateProvider), new Type[] { })]
	private UnityEngine.Object _fingerFeatureStateProvider;

	public IFingerFeatureStateProvider FingerFeatureStateProvider { get; private set; }

	protected virtual void Awake()
	{
		FingerFeatureStateProvider = _fingerFeatureStateProvider as IFingerFeatureStateProvider;
	}

	protected virtual void Start()
	{
	}

	public bool GetCurrentState(HandFinger finger, FingerFeature fingerFeature, out string currentState)
	{
		return FingerFeatureStateProvider.GetCurrentState(finger, fingerFeature, out currentState);
	}

	public bool IsStateActive(HandFinger finger, FingerFeature feature, FeatureStateActiveMode mode, string stateId)
	{
		return FingerFeatureStateProvider.IsStateActive(finger, feature, mode, stateId);
	}

	public float? GetFeatureValue(HandFinger finger, FingerFeature fingerFeature)
	{
		return FingerFeatureStateProvider.GetFeatureValue(finger, fingerFeature);
	}

	public void InjectAllFingerFeatureStateProviderRef(IFingerFeatureStateProvider fingerFeatureStateProvider)
	{
		InjectFingerFeatureStateProvider(fingerFeatureStateProvider);
	}

	public void InjectFingerFeatureStateProvider(IFingerFeatureStateProvider fingerFeatureStateProvider)
	{
		_fingerFeatureStateProvider = fingerFeatureStateProvider as UnityEngine.Object;
		FingerFeatureStateProvider = fingerFeatureStateProvider;
	}
}
