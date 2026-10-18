using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Impulse Listener")]
[ExecuteAlways]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulseListener.html")]
public class CinemachineImpulseListener : CinemachineExtension
{
	public enum SignalCombinationModes
	{
		Additive,
		UseLargest
	}

	[Serializable]
	public struct ImpulseReaction
	{
		[Tooltip("Secondary shake that will be triggered by the primary impulse.")]
		public NoiseSettings m_SecondaryNoise;

		[Tooltip("Gain to apply to the amplitudes defined in the signal source.  1 is normal.  Setting this to 0 completely mutes the signal.")]
		[FormerlySerializedAs("m_AmplitudeGain")]
		public float AmplitudeGain;

		[Tooltip("Scale factor to apply to the time axis.  1 is normal.  Larger magnitudes will make the signal progress more rapidly.")]
		[FormerlySerializedAs("m_FrequencyGain")]
		public float FrequencyGain;

		[Tooltip("How long the secondary reaction lasts.")]
		[FormerlySerializedAs("m_Duration")]
		public float Duration;

		private float m_CurrentAmount;

		private float m_CurrentTime;

		private float m_CurrentDamping;

		private bool m_Initialized;

		[SerializeField]
		[HideInInspector]
		[NoSaveDuringPlay]
		private Vector3 m_NoiseOffsets;

		public void ReSeed()
		{
			m_NoiseOffsets = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));
		}

		public bool GetReaction(float deltaTime, Vector3 impulsePos, out Vector3 pos, out Quaternion rot)
		{
			if (!m_Initialized)
			{
				m_Initialized = true;
				m_CurrentAmount = 0f;
				m_CurrentDamping = 0f;
				m_CurrentTime = CinemachineCore.CurrentTime * FrequencyGain;
				if (m_NoiseOffsets == Vector3.zero)
				{
					ReSeed();
				}
			}
			pos = Vector3.zero;
			rot = Quaternion.identity;
			float sqrMagnitude = impulsePos.sqrMagnitude;
			if (m_SecondaryNoise == null || (sqrMagnitude < 0.001f && m_CurrentAmount < 0.0001f))
			{
				return false;
			}
			if (TargetPositionCache.CacheMode == TargetPositionCache.Mode.Playback && TargetPositionCache.HasCurrentTime)
			{
				m_CurrentTime = TargetPositionCache.CurrentTime * FrequencyGain;
			}
			else
			{
				m_CurrentTime += deltaTime * FrequencyGain;
			}
			m_CurrentAmount = Mathf.Max(m_CurrentAmount, Mathf.Sqrt(sqrMagnitude));
			m_CurrentDamping = Mathf.Max(m_CurrentDamping, Mathf.Max(1f, Mathf.Sqrt(m_CurrentAmount)) * Duration);
			float num = m_CurrentAmount * AmplitudeGain;
			pos = NoiseSettings.GetCombinedFilterResults(m_SecondaryNoise.PositionNoise, m_CurrentTime, m_NoiseOffsets) * num;
			rot = Quaternion.Euler(NoiseSettings.GetCombinedFilterResults(m_SecondaryNoise.OrientationNoise, m_CurrentTime, m_NoiseOffsets) * num);
			m_CurrentAmount -= Damper.Damp(m_CurrentAmount, m_CurrentDamping, deltaTime);
			m_CurrentDamping -= Damper.Damp(m_CurrentDamping, m_CurrentDamping, deltaTime);
			return true;
		}
	}

	[Tooltip("When to apply the impulse reaction.  Default is after the Noise stage.  Modify this if necessary to influence the ordering of extension effects")]
	[FormerlySerializedAs("m_ApplyAfter")]
	public CinemachineCore.Stage ApplyAfter = CinemachineCore.Stage.Aim;

	[Tooltip("Impulse events on channels not included in the mask will be ignored.")]
	[CinemachineImpulseChannelProperty]
	[FormerlySerializedAs("m_ChannelMask")]
	public int ChannelMask;

	[Tooltip("Gain to apply to the Impulse signal.  1 is normal strength.  Setting this to 0 completely mutes the signal.")]
	[FormerlySerializedAs("m_Gain")]
	public float Gain;

	[Tooltip("Enable this to perform distance calculation in 2D (ignore Z)")]
	[FormerlySerializedAs("m_Use2DDistance")]
	public bool Use2DDistance;

	[Tooltip("Enable this to process all impulse signals in camera space")]
	[FormerlySerializedAs("m_UseCameraSpace")]
	public bool UseCameraSpace;

	[Tooltip("Controls how the Impulse Listener combines multiple impulses active at the current point in space.\n\n<b>Additive</b>: Combines all the active signals together, like sound waves.  This is the default.\n\n<b>Use Largest</b>: Considers only the signal with the largest amplitude; ignores any others.")]
	public SignalCombinationModes SignalCombinationMode;

	[Tooltip("This controls the secondary reaction of the listener to the incoming impulse.  The impulse might be for example a sharp shock, and the secondary reaction could be a vibration whose amplitude and duration is controlled by the size of the original impulse.  This allows different listeners to respond in different ways to the same impulse signal.")]
	[FormerlySerializedAs("m_ReactionSettings")]
	public ImpulseReaction ReactionSettings;

	private void Reset()
	{
		ApplyAfter = CinemachineCore.Stage.Noise;
		ChannelMask = 1;
		Gain = 1f;
		Use2DDistance = false;
		UseCameraSpace = true;
		SignalCombinationMode = SignalCombinationModes.Additive;
		ReactionSettings = new ImpulseReaction
		{
			AmplitudeGain = 1f,
			FrequencyGain = 1f,
			Duration = 1f
		};
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != ApplyAfter || !(deltaTime >= 0f))
		{
			return;
		}
		bool flag = false;
		Vector3 pos = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		flag = ((SignalCombinationMode != 0) ? CinemachineImpulseManager.Instance.GetStrongestImpulseAt(state.GetFinalPosition(), Use2DDistance, ChannelMask, out pos, out rot) : CinemachineImpulseManager.Instance.GetImpulseAt(state.GetFinalPosition(), Use2DDistance, ChannelMask, out pos, out rot));
		Vector3 pos2;
		Quaternion rot2;
		bool reaction = ReactionSettings.GetReaction(deltaTime, pos, out pos2, out rot2);
		if (flag)
		{
			rot = Quaternion.SlerpUnclamped(Quaternion.identity, rot, Gain);
			pos *= Gain;
		}
		if (reaction)
		{
			pos += pos2;
			rot *= rot2;
		}
		if (flag || reaction)
		{
			if (UseCameraSpace)
			{
				pos = state.RawOrientation * pos;
			}
			state.PositionCorrection += pos;
			state.OrientationCorrection *= rot;
		}
	}
}
