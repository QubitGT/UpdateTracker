using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

public class CinemachineImpulseManager
{
	[Serializable]
	public struct EnvelopeDefinition
	{
		[Tooltip("Normalized curve defining the shape of the start of the envelope.  If blank a default curve will be used")]
		[FormerlySerializedAs("m_AttackShape")]
		public AnimationCurve AttackShape;

		[Tooltip("Normalized curve defining the shape of the end of the envelope.  If blank a default curve will be used")]
		[FormerlySerializedAs("m_DecayShape")]
		public AnimationCurve DecayShape;

		[Tooltip("Duration in seconds of the attack.  Attack curve will be scaled to fit.  Must be >= 0.")]
		[FormerlySerializedAs("m_AttackTime")]
		public float AttackTime;

		[Tooltip("Duration in seconds of the central fully-scaled part of the envelope.  Must be >= 0.")]
		[FormerlySerializedAs("m_SustainTime")]
		public float SustainTime;

		[Tooltip("Duration in seconds of the decay.  Decay curve will be scaled to fit.  Must be >= 0.")]
		[FormerlySerializedAs("m_DecayTime")]
		public float DecayTime;

		[Tooltip("If checked, signal amplitude scaling will also be applied to the time envelope of the signal.  Stronger signals will last longer.")]
		[FormerlySerializedAs("m_ScaleWithImpact")]
		public bool ScaleWithImpact;

		[Tooltip("If true, then duration is infinite.")]
		[FormerlySerializedAs("m_HoldForever")]
		public bool HoldForever;

		public static EnvelopeDefinition Default
		{
			get
			{
				EnvelopeDefinition result = default(EnvelopeDefinition);
				result.DecayTime = 0.7f;
				result.SustainTime = 0.2f;
				result.ScaleWithImpact = true;
				return result;
			}
		}

		public readonly float Duration
		{
			get
			{
				if (!HoldForever)
				{
					return AttackTime + SustainTime + DecayTime;
				}
				return -1f;
			}
		}

		public readonly float GetValueAt(float offset)
		{
			if (offset >= 0f)
			{
				if (offset < AttackTime && AttackTime > 0.0001f)
				{
					if (AttackShape == null || AttackShape.length < 2)
					{
						return Damper.Damp(1f, AttackTime, offset);
					}
					return AttackShape.Evaluate(offset / AttackTime);
				}
				offset -= AttackTime;
				if (HoldForever || offset < SustainTime)
				{
					return 1f;
				}
				offset -= SustainTime;
				if (offset < DecayTime && DecayTime > 0.0001f)
				{
					if (DecayShape == null || DecayShape.length < 2)
					{
						return 1f - Damper.Damp(1f, DecayTime, offset);
					}
					return DecayShape.Evaluate(offset / DecayTime);
				}
			}
			return 0f;
		}

		public void ChangeStopTime(float offset, bool forceNoDecay)
		{
			if (offset < 0f)
			{
				offset = 0f;
			}
			if (offset < AttackTime)
			{
				AttackTime = 0f;
			}
			SustainTime = offset - AttackTime;
			if (forceNoDecay)
			{
				DecayTime = 0f;
			}
		}

		public void Clear()
		{
			AttackShape = (DecayShape = null);
			AttackTime = (SustainTime = (DecayTime = 0f));
		}

		public void Validate()
		{
			AttackTime = Mathf.Max(0f, AttackTime);
			DecayTime = Mathf.Max(0f, DecayTime);
			SustainTime = Mathf.Max(0f, SustainTime);
		}
	}

	public class ImpulseEvent
	{
		public enum DirectionModes
		{
			Fixed,
			RotateTowardSource
		}

		public enum DissipationModes
		{
			LinearDecay,
			SoftDecay,
			ExponentialDecay
		}

		public float StartTime;

		public EnvelopeDefinition Envelope;

		public ISignalSource6D SignalSource;

		public Vector3 Position;

		public float Radius;

		public DirectionModes DirectionMode;

		public int Channel;

		public DissipationModes DissipationMode;

		public float DissipationDistance;

		public float CustomDissipation;

		public float PropagationSpeed;

		public bool Expired
		{
			get
			{
				float duration = Envelope.Duration;
				float num = Radius + DissipationDistance;
				float num2 = Instance.CurrentTime - num / Mathf.Max(1f, PropagationSpeed);
				if (duration > 0f)
				{
					return StartTime + duration <= num2;
				}
				return false;
			}
		}

		public void Cancel(float time, bool forceNoDecay)
		{
			Envelope.HoldForever = false;
			Envelope.ChangeStopTime(time - StartTime, forceNoDecay);
		}

		public float DistanceDecay(float distance)
		{
			float num = Mathf.Max(Radius, 0f);
			if (distance < num)
			{
				return 1f;
			}
			distance -= num;
			if (distance >= DissipationDistance)
			{
				return 0f;
			}
			if (CustomDissipation >= 0f)
			{
				return EvaluateDissipationScale(CustomDissipation, distance / DissipationDistance);
			}
			return DissipationMode switch
			{
				DissipationModes.SoftDecay => 0.5f * (1f + Mathf.Cos(MathF.PI * (distance / DissipationDistance))), 
				DissipationModes.ExponentialDecay => 1f - Damper.Damp(1f, DissipationDistance, distance), 
				_ => Mathf.Lerp(1f, 0f, distance / DissipationDistance), 
			};
		}

		public bool GetDecayedSignal(Vector3 listenerPosition, bool use2D, out Vector3 pos, out Quaternion rot)
		{
			if (SignalSource != null)
			{
				float num = (use2D ? Vector2.Distance(listenerPosition, Position) : Vector3.Distance(listenerPosition, Position));
				float num2 = Instance.CurrentTime - StartTime - num / Mathf.Max(1f, PropagationSpeed);
				float num3 = Envelope.GetValueAt(num2) * DistanceDecay(num);
				if (num3 != 0f)
				{
					SignalSource.GetSignal(num2, out pos, out rot);
					pos *= num3;
					rot = Quaternion.SlerpUnclamped(Quaternion.identity, rot, num3);
					if (DirectionMode == DirectionModes.RotateTowardSource && num > 0.0001f)
					{
						Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, listenerPosition - Position);
						if (Radius > 0.0001f)
						{
							float num4 = Mathf.Clamp01(num / Radius);
							quaternion = Quaternion.Slerp(quaternion, Quaternion.identity, Mathf.Cos(MathF.PI * num4 / 2f));
						}
						pos = quaternion * pos;
					}
					return true;
				}
			}
			pos = Vector3.zero;
			rot = Quaternion.identity;
			return false;
		}

		public void Clear()
		{
			Envelope.Clear();
			StartTime = 0f;
			SignalSource = null;
			Position = Vector3.zero;
			Channel = 0;
			Radius = 0f;
			DissipationDistance = 100f;
			DissipationMode = DissipationModes.ExponentialDecay;
			CustomDissipation = -1f;
		}

		internal ImpulseEvent()
		{
		}
	}

	private static CinemachineImpulseManager s_Instance;

	private const float Epsilon = 0.0001f;

	private List<ImpulseEvent> m_ExpiredEvents;

	private List<ImpulseEvent> m_ActiveEvents;

	public bool IgnoreTimeScale;

	public static CinemachineImpulseManager Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new CinemachineImpulseManager();
			}
			return s_Instance;
		}
	}

	public float CurrentTime
	{
		get
		{
			if (!IgnoreTimeScale)
			{
				return CinemachineCore.CurrentTime;
			}
			return Time.realtimeSinceStartup;
		}
	}

	private CinemachineImpulseManager()
	{
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeModule()
	{
		if (s_Instance != null)
		{
			s_Instance.Clear();
		}
	}

	internal static float EvaluateDissipationScale(float spread, float normalizedDistance)
	{
		float num = -0.8f + 1.6f * (1f - spread);
		num = (1f - num) * 0.5f;
		float t = Mathf.Clamp01(normalizedDistance) / ((1f / Mathf.Clamp01(num) - 2f) * (1f - normalizedDistance) + 1f);
		return 1f - SplineHelpers.Bezier1(t, 0f, 0f, 1f, 1f);
	}

	public bool GetImpulseAt(Vector3 listenerLocation, bool distance2D, int channelMask, out Vector3 pos, out Quaternion rot)
	{
		bool result = false;
		pos = Vector3.zero;
		rot = Quaternion.identity;
		if (m_ActiveEvents != null)
		{
			for (int num = m_ActiveEvents.Count - 1; num >= 0; num--)
			{
				ImpulseEvent impulseEvent = m_ActiveEvents[num];
				Vector3 pos2;
				Quaternion rot2;
				if (impulseEvent == null || impulseEvent.Expired)
				{
					m_ActiveEvents.RemoveAt(num);
					if (impulseEvent != null)
					{
						if (m_ExpiredEvents == null)
						{
							m_ExpiredEvents = new List<ImpulseEvent>();
						}
						impulseEvent.Clear();
						m_ExpiredEvents.Add(impulseEvent);
					}
				}
				else if ((impulseEvent.Channel & channelMask) != 0 && impulseEvent.GetDecayedSignal(listenerLocation, distance2D, out pos2, out rot2))
				{
					result = true;
					pos += pos2;
					rot *= rot2;
				}
			}
		}
		return result;
	}

	public bool GetStrongestImpulseAt(Vector3 listenerLocation, bool distance2D, int channelMask, out Vector3 pos, out Quaternion rot)
	{
		bool result = false;
		Vector3 vector = Vector3.zero;
		Quaternion quaternion = Quaternion.identity;
		if (m_ActiveEvents != null)
		{
			float num = 0f;
			for (int num2 = m_ActiveEvents.Count - 1; num2 >= 0; num2--)
			{
				ImpulseEvent impulseEvent = m_ActiveEvents[num2];
				Vector3 pos2;
				Quaternion rot2;
				if (impulseEvent == null || impulseEvent.Expired)
				{
					m_ActiveEvents.RemoveAt(num2);
					if (impulseEvent != null)
					{
						if (m_ExpiredEvents == null)
						{
							m_ExpiredEvents = new List<ImpulseEvent>();
						}
						impulseEvent.Clear();
						m_ExpiredEvents.Add(impulseEvent);
					}
				}
				else if ((impulseEvent.Channel & channelMask) != 0 && impulseEvent.GetDecayedSignal(listenerLocation, distance2D, out pos2, out rot2))
				{
					result = true;
					float sqrMagnitude = pos2.sqrMagnitude;
					if (sqrMagnitude > num)
					{
						num = sqrMagnitude;
						vector = pos2;
						quaternion = rot2;
					}
				}
			}
		}
		pos = vector;
		rot = quaternion;
		return result;
	}

	public ImpulseEvent NewImpulseEvent()
	{
		if (m_ExpiredEvents == null || m_ExpiredEvents.Count == 0)
		{
			return new ImpulseEvent
			{
				CustomDissipation = -1f
			};
		}
		ImpulseEvent result = m_ExpiredEvents[m_ExpiredEvents.Count - 1];
		m_ExpiredEvents.RemoveAt(m_ExpiredEvents.Count - 1);
		return result;
	}

	public void AddImpulseEvent(ImpulseEvent e)
	{
		if (m_ActiveEvents == null)
		{
			m_ActiveEvents = new List<ImpulseEvent>();
		}
		if (e != null)
		{
			e.StartTime = CurrentTime;
			m_ActiveEvents.Add(e);
		}
	}

	public void Clear()
	{
		if (m_ActiveEvents != null)
		{
			for (int i = 0; i < m_ActiveEvents.Count; i++)
			{
				m_ActiveEvents[i].Clear();
			}
			m_ActiveEvents.Clear();
		}
	}
}
