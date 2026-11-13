using System;
using GorillaExtensions;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

public class GorillaStatusToThermalTemperatureMono : MonoBehaviour, ISpawnable
{
	[Serializable]
	private struct _MaterialIndexToTemperature
	{
		public int[] matIndexes;

		public float temperature;
	}

	private const string preLog = "[GorillaStatusToThermalTemperatureMono]  ";

	private const string preErr = "[GorillaStatusToThermalTemperatureMono]  ERROR!!!  ";

	[Tooltip("Should either be assigned here or via another script.")]
	[SerializeField]
	private VRRig m_rig;

	[SerializeField]
	private ThermalSourceVolume m_thermalSourceVolume;

	[SerializeField]
	private _MaterialIndexToTemperature[] m_materialIndexesToTemperatures;

	[DebugReadout]
	private float[] _runtimeMatIndexes_to_temperatures;

	private bool _isSubscribed;

	public bool hasRig { get; private set; }

	public VRRig rig => m_rig;

	public bool IsSpawned { get; set; }

	public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

	protected void Awake()
	{
		hasRig = m_rig != null;
		if (m_thermalSourceVolume != null)
		{
			m_thermalSourceVolume.bodyHeatRig = m_rig;
		}
	}

	protected void OnEnable()
	{
		if (!ApplicationQuittingState.IsQuitting)
		{
			if (m_thermalSourceVolume == null)
			{
				GTDev.LogError("[GorillaStatusToThermalTemperatureMono]  ERROR!!!  Disabling because thermal source is not assigned. Path=" + base.transform.GetPathQ(), this);
				base.enabled = false;
			}
			else
			{
				Subscribe();
			}
		}
	}

	protected void OnDisable()
	{
		if (!ApplicationQuittingState.IsQuitting)
		{
			Unsubscribe();
		}
	}

	public void SetRig(VRRig newRig)
	{
		if (newRig == m_rig)
		{
			return;
		}
		Unsubscribe();
		m_rig = newRig;
		hasRig = newRig != null;
		if (!(m_thermalSourceVolume == null))
		{
			m_thermalSourceVolume.bodyHeatRig = newRig;
			if (base.isActiveAndEnabled)
			{
				Subscribe();
			}
		}
	}

	private void Subscribe()
	{
		if (!hasRig)
		{
			SetSourceTemperature(float.NaN);
			return;
		}
		_InitRuntimeArray();
		if (!_isSubscribed)
		{
			VRRig vRRig = m_rig;
			vRRig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Combine(vRRig.OnMaterialIndexChanged, new Action<int, int>(_OnMatChanged));
			_isSubscribed = true;
		}
		_OnMatChanged(-1, m_rig.setMatIndex);
	}

	private void Unsubscribe()
	{
		if (_isSubscribed)
		{
			if ((object)m_rig != null)
			{
				VRRig vRRig = m_rig;
				vRRig.OnMaterialIndexChanged = (Action<int, int>)Delegate.Remove(vRRig.OnMaterialIndexChanged, new Action<int, int>(_OnMatChanged));
			}
			_isSubscribed = false;
		}
		SetSourceTemperature(float.NaN);
	}

	private void _InitRuntimeArray()
	{
		if (_runtimeMatIndexes_to_temperatures != null)
		{
			return;
		}
		Material[] materialsToChangeTo = m_rig.materialsToChangeTo;
		int num = ((materialsToChangeTo != null) ? materialsToChangeTo.Length : 0);
		float[] array = new float[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = float.NaN;
		}
		if (m_materialIndexesToTemperatures != null)
		{
			_MaterialIndexToTemperature[] materialIndexesToTemperatures = m_materialIndexesToTemperatures;
			for (int j = 0; j < materialIndexesToTemperatures.Length; j++)
			{
				_MaterialIndexToTemperature materialIndexToTemperature = materialIndexesToTemperatures[j];
				if (materialIndexToTemperature.matIndexes == null)
				{
					continue;
				}
				int[] matIndexes = materialIndexToTemperature.matIndexes;
				foreach (int num2 in matIndexes)
				{
					if (num2 >= 0 && num2 < num)
					{
						array[num2] = materialIndexToTemperature.temperature;
					}
				}
			}
		}
		_runtimeMatIndexes_to_temperatures = array;
		if (!Application.isEditor)
		{
			m_materialIndexesToTemperatures = null;
		}
	}

	private void _OnMatChanged(int oldIndex, int newIndex)
	{
		float[] runtimeMatIndexes_to_temperatures = _runtimeMatIndexes_to_temperatures;
		float sourceTemperature = ((runtimeMatIndexes_to_temperatures != null && (uint)newIndex < (uint)runtimeMatIndexes_to_temperatures.Length) ? runtimeMatIndexes_to_temperatures[newIndex] : float.NaN);
		SetSourceTemperature(sourceTemperature);
	}

	private void SetSourceTemperature(float celsius)
	{
		if (!(m_thermalSourceVolume == null))
		{
			bool flag = !float.IsNaN(celsius);
			if (flag)
			{
				m_thermalSourceVolume.celsius = celsius;
			}
			m_thermalSourceVolume.enabled = flag;
		}
	}

	public void OnSpawn(VRRig newRig)
	{
		SetRig(newRig);
	}

	public void OnDespawn()
	{
		SetRig(null);
	}
}
