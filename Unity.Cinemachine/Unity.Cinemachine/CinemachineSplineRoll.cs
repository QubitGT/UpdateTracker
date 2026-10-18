using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Spline Roll")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineRoll.html")]
[SaveDuringPlay]
public class CinemachineSplineRoll : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	public struct RollData
	{
		[Tooltip("Roll (in degrees) around the forward direction for specific location on the track.\n- When placed on a SplineContainer, this is going to be a global override that affects all vcams using the Spline.\n- When placed on a CinemachineCamera, this is going to be a local override that only affects that CinemachineCamera.")]
		public float Value;

		public static implicit operator float(RollData roll)
		{
			return roll.Value;
		}

		public static implicit operator RollData(float roll)
		{
			RollData result = default(RollData);
			result.Value = roll;
			return result;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct LerpRollData : IInterpolator<RollData>
	{
		public RollData Interpolate(RollData a, RollData b, float t)
		{
			RollData result = default(RollData);
			result.Value = Mathf.Lerp(a.Value, b.Value, t);
			return result;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct LerpRollDataWithEasing : IInterpolator<RollData>
	{
		public RollData Interpolate(RollData a, RollData b, float t)
		{
			float num = t * t;
			float num2 = 1f - t;
			t = 3f * num2 * num + t * num;
			RollData result = default(RollData);
			result.Value = Mathf.Lerp(a.Value, b.Value, t);
			return result;
		}
	}

	internal struct RollCache
	{
		private CinemachineSplineRoll m_RollCache;

		public void Refresh(MonoBehaviour owner)
		{
			m_RollCache = null;
			if (!owner.TryGetComponent<CinemachineSplineRoll>(out m_RollCache) && owner is ISplineReferencer splineReferencer)
			{
				SplineContainer spline = splineReferencer.SplineSettings.Spline;
				if (spline != null)
				{
					spline?.TryGetComponent<CinemachineSplineRoll>(out m_RollCache);
				}
			}
		}

		public CinemachineSplineRoll GetSplineRoll(MonoBehaviour owner)
		{
			return m_RollCache;
		}
	}

	[Tooltip("When enabled, roll eases into and out of the data point values.  Otherwise, interpolation is linear.")]
	public bool Easing = true;

	[HideFoldout]
	public SplineData<RollData> Roll;

	[HideInInspector]
	[SerializeField]
	[NoSaveDuringPlay]
	private int m_StreamingVersion;

	public IInterpolator<RollData> GetInterpolator()
	{
		if (!Easing)
		{
			return default(LerpRollData);
		}
		return default(LerpRollDataWithEasing);
	}

	private void PerformLegacyUpgrade(int streamedVersion)
	{
		if (streamedVersion < 20240101)
		{
			for (int i = 0; i < Roll.Count; i++)
			{
				DataPoint<RollData> value = Roll[i];
				value.Value = 0f - (float)value.Value;
				Roll[i] = value;
			}
		}
	}

	private void Reset()
	{
		Roll?.Clear();
		Easing = true;
	}

	private void OnEnable()
	{
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (m_StreamingVersion < 20241001)
		{
			PerformLegacyUpgrade(m_StreamingVersion);
		}
		m_StreamingVersion = 20241001;
	}
}
