using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class ThermalManager : MonoBehaviour, IGorillaSliceableSimple
{
	private struct SourceData
	{
		public ThermalSourceVolume source;

		public Vector3 position;

		public float inner;

		public float outer;

		public float reach;

		public float celsiusDelta;

		public bool hasExclusions;

		public VRRig bodyHeatRig;
	}

	public static readonly List<ThermalSourceVolume> sources = new List<ThermalSourceVolume>(256);

	public static readonly List<ThermalReceiver> receivers = new List<ThermalReceiver>(256);

	internal const float k_roomTemperature = 20f;

	[NonSerialized]
	private static ThermalManager instance;

	private float lastTime;

	private static SourceData[] _sourceData = new SourceData[256];

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		sources.Clear();
		receivers.Clear();
		instance = null;
	}

	public void OnEnable()
	{
		if (instance != null && instance != this)
		{
			Debug.LogError("ThermalManager already exists!");
			return;
		}
		instance = this;
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		lastTime = Time.time;
	}

	public void OnDisable()
	{
		if (!(instance != this))
		{
			instance = null;
			GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		}
	}

	public void SliceUpdate()
	{
		float time = Time.time;
		float num = time - lastTime;
		lastTime = time;
		int num2 = CacheSources();
		for (int num3 = receivers.Count - 1; num3 >= 0; num3--)
		{
			if (num3 < receivers.Count)
			{
				ThermalReceiver thermalReceiver = receivers[num3];
				Transform cachedTransform = thermalReceiver.cachedTransform;
				Vector3 position = cachedTransform.position;
				float num4 = thermalReceiver.radius * Mathf.Abs(cachedTransform.lossyScale.x);
				bool flag = thermalReceiver.exclusionSources.Count > 0;
				VRRig vRRig = (thermalReceiver.ignoreOwnBodyHeat ? thermalReceiver.ownerRig : null);
				bool flag2 = (object)vRRig != null;
				float num5 = 20f;
				for (int i = 0; i < num2; i++)
				{
					ref SourceData reference = ref _sourceData[i];
					float num6 = reference.reach + num4;
					float sqrMagnitude = (reference.position - position).sqrMagnitude;
					if (!(sqrMagnitude > num6 * num6) && (!reference.hasExclusions || !reference.source.exclusionReceivers.Contains(thermalReceiver)) && (!flag || !thermalReceiver.exclusionSources.Contains(reference.source)) && (!flag2 || (object)reference.bodyHeatRig != vRRig))
					{
						float num7 = Mathf.Sqrt(sqrMagnitude) - num4;
						float num8 = ((reference.outer > reference.inner) ? (1f - Mathf.InverseLerp(reference.inner, reference.outer, num7)) : ((num7 <= reference.inner) ? 1f : 0f));
						num5 += reference.celsiusDelta * num8;
					}
				}
				float t = 1f - Mathf.Exp((0f - thermalReceiver.conductivity) * num);
				thermalReceiver.celsius = Mathf.Lerp(thermalReceiver.celsius, num5, t);
				thermalReceiver.continuousProperties?.ApplyAll(thermalReceiver.celsius);
				if (!thermalReceiver.wasAboveThreshold && thermalReceiver.celsius > thermalReceiver.temperatureThreshold)
				{
					thermalReceiver.wasAboveThreshold = true;
					thermalReceiver.OnAboveThreshold?.Invoke();
				}
				else if (thermalReceiver.wasAboveThreshold && thermalReceiver.celsius < thermalReceiver.temperatureThreshold)
				{
					thermalReceiver.wasAboveThreshold = false;
					thermalReceiver.OnBelowThreshold?.Invoke();
				}
			}
		}
	}

	private static int CacheSources()
	{
		int count = sources.Count;
		if (_sourceData.Length < count)
		{
			Array.Resize(ref _sourceData, Mathf.NextPowerOfTwo(count));
		}
		int result = 0;
		for (int i = 0; i < count; i++)
		{
			ThermalSourceVolume thermalSourceVolume = sources[i];
			float num = thermalSourceVolume.celsius - 20f;
			if (!(num <= 0f))
			{
				Transform cachedTransform = thermalSourceVolume.cachedTransform;
				float num2 = Mathf.Abs(cachedTransform.lossyScale.x);
				float num3 = thermalSourceVolume.innerRadius * num2;
				float num4 = thermalSourceVolume.outerRadius * num2;
				ref SourceData reference = ref _sourceData[result++];
				reference.source = thermalSourceVolume;
				reference.position = cachedTransform.position;
				reference.inner = num3;
				reference.outer = num4;
				reference.reach = ((num4 > num3) ? num4 : num3);
				reference.celsiusDelta = num;
				reference.hasExclusions = thermalSourceVolume.exclusionReceivers.Count > 0;
				reference.bodyHeatRig = thermalSourceVolume.bodyHeatRig;
			}
		}
		return result;
	}

	public static void Register(ThermalSourceVolume source)
	{
		if (source.managerIndex < 0)
		{
			source.managerIndex = sources.Count;
			sources.Add(source);
		}
	}

	public static void Unregister(ThermalSourceVolume source)
	{
		int managerIndex = source.managerIndex;
		if (managerIndex >= 0)
		{
			int index = sources.Count - 1;
			ThermalSourceVolume thermalSourceVolume = sources[index];
			sources[managerIndex] = thermalSourceVolume;
			thermalSourceVolume.managerIndex = managerIndex;
			sources.RemoveAt(index);
			source.managerIndex = -1;
		}
	}

	public static void Register(ThermalReceiver receiver)
	{
		if (receiver.managerIndex < 0)
		{
			receiver.managerIndex = receivers.Count;
			receivers.Add(receiver);
		}
	}

	public static void Unregister(ThermalReceiver receiver)
	{
		int managerIndex = receiver.managerIndex;
		if (managerIndex >= 0)
		{
			int index = receivers.Count - 1;
			ThermalReceiver thermalReceiver = receivers[index];
			receivers[managerIndex] = thermalReceiver;
			thermalReceiver.managerIndex = managerIndex;
			receivers.RemoveAt(index);
			receiver.managerIndex = -1;
		}
	}
}
