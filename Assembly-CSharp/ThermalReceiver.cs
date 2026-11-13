using System;
using System.Collections.Generic;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

public class ThermalReceiver : MonoBehaviour, IDynamicFloat, IResettableItem, ISpawnable
{
	public float radius = 0.2f;

	[Tooltip("How fast the temperature should change overtime. 1.0 would be instantly.")]
	public float conductivity = 0.3f;

	public ContinuousPropertyArray continuousProperties;

	[Tooltip("Optional: Fire events if temperature goes below or above this threshold - Celsius")]
	public float temperatureThreshold;

	[Tooltip("Exclude these thermal sources from impacting this receiver")]
	public List<ThermalSourceVolume> exclusionSources = new List<ThermalSourceVolume>();

	[Tooltip("Ignore the body heat of the gorilla this receiver is attached to (e.g. worn glasses shouldn't fog from their own wearer being tagged). Other gorillas' body heat still applies.")]
	public bool ignoreOwnBodyHeat;

	[Space]
	public UnityEvent OnAboveThreshold;

	public UnityEvent OnBelowThreshold;

	[DebugOption]
	public float celsius;

	public bool wasAboveThreshold;

	private float defaultCelsius;

	[NonSerialized]
	internal int managerIndex = -1;

	[NonSerialized]
	internal Transform cachedTransform;

	[NonSerialized]
	private float disabledTime = -1f;

	[NonSerialized]
	private bool wasAboveWhenDisabled;

	[NonSerialized]
	internal VRRig ownerRig;

	[NonSerialized]
	private VRRig spawnedRig;

	public float Farenheit => celsius * 1.8f + 32f;

	public float floatValue => celsius;

	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	protected void Awake()
	{
		if (celsius == 0f)
		{
			celsius = 20f;
		}
		defaultCelsius = celsius;
		wasAboveThreshold = false;
	}

	protected void OnEnable()
	{
		if (cachedTransform == null)
		{
			cachedTransform = base.transform;
		}
		ResolveOwnerRig();
		ApplyCooldownWhileDisabled();
		RestoreThresholdState();
		ThermalManager.Register(this);
	}

	protected void OnDisable()
	{
		wasAboveWhenDisabled = wasAboveThreshold;
		wasAboveThreshold = false;
		disabledTime = Time.time;
		ThermalManager.Unregister(this);
	}

	private void ApplyCooldownWhileDisabled()
	{
		if (!(disabledTime < 0f))
		{
			float num = Time.time - disabledTime;
			disabledTime = -1f;
			if (!(num <= 0f))
			{
				float num2 = 20f;
				celsius = num2 + (celsius - num2) * Mathf.Exp((0f - conductivity) * num);
			}
		}
	}

	private void RestoreThresholdState()
	{
		if (wasAboveWhenDisabled && celsius < temperatureThreshold)
		{
			wasAboveThreshold = true;
		}
		wasAboveWhenDisabled = false;
	}

	public void ResetToDefaultState()
	{
		celsius = defaultCelsius;
		wasAboveThreshold = false;
		disabledTime = -1f;
		wasAboveWhenDisabled = false;
	}

	private void ResolveOwnerRig()
	{
		if (!ignoreOwnBodyHeat)
		{
			ownerRig = null;
		}
		else
		{
			ownerRig = ((spawnedRig != null) ? spawnedRig : GetComponentInParent<VRRig>(includeInactive: true));
		}
	}

	public void OnSpawn(VRRig rig)
	{
		spawnedRig = rig;
		ResolveOwnerRig();
	}

	public void OnDespawn()
	{
		spawnedRig = null;
		ResolveOwnerRig();
	}
}
