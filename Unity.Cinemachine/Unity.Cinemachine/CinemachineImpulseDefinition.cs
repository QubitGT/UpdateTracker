using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Serializable]
public class CinemachineImpulseDefinition
{
	public enum ImpulseShapes
	{
		Custom,
		Recoil,
		Bump,
		Explosion,
		Rumble
	}

	public enum ImpulseTypes
	{
		Uniform,
		Dissipating,
		Propagating,
		Legacy
	}

	public enum RepeatModes
	{
		Stretch,
		Loop
	}

	private class SignalSource : ISignalSource6D
	{
		private CinemachineImpulseDefinition m_Def;

		private Vector3 m_Velocity;

		public float SignalDuration => m_Def.ImpulseDuration;

		public SignalSource(CinemachineImpulseDefinition def, Vector3 velocity)
		{
			m_Def = def;
			m_Velocity = velocity;
		}

		public void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot)
		{
			pos = m_Velocity * m_Def.ImpulseCurve.Evaluate(timeSinceSignalStart / SignalDuration);
			rot = Quaternion.identity;
		}
	}

	private class LegacySignalSource : ISignalSource6D
	{
		private CinemachineImpulseDefinition m_Def;

		private Vector3 m_Velocity;

		private float m_StartTimeOffset;

		public float SignalDuration => m_Def.RawSignal.SignalDuration;

		public LegacySignalSource(CinemachineImpulseDefinition def, Vector3 velocity)
		{
			m_Def = def;
			m_Velocity = velocity;
			if (m_Def.Randomize && m_Def.RawSignal.SignalDuration <= 0f)
			{
				m_StartTimeOffset = UnityEngine.Random.Range(-1000f, 1000f);
			}
		}

		public void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot)
		{
			float num = m_StartTimeOffset + timeSinceSignalStart * m_Def.FrequencyGain;
			float signalDuration = SignalDuration;
			if (signalDuration > 0f)
			{
				if (m_Def.RepeatMode == RepeatModes.Loop)
				{
					num %= signalDuration;
				}
				else if (m_Def.TimeEnvelope.Duration > 0.0001f)
				{
					num *= m_Def.TimeEnvelope.Duration / signalDuration;
				}
			}
			m_Def.RawSignal.GetSignal(num, out pos, out rot);
			float magnitude = m_Velocity.magnitude;
			magnitude *= m_Def.AmplitudeGain;
			pos *= magnitude;
			pos = Quaternion.FromToRotation(Vector3.down, m_Velocity) * pos;
			rot = Quaternion.SlerpUnclamped(Quaternion.identity, rot, magnitude);
		}
	}

	[CinemachineImpulseChannelProperty]
	[Tooltip("Impulse events generated here will appear on the channels included in the mask.")]
	[FormerlySerializedAs("m_ImpulseChannel")]
	public int ImpulseChannel = 1;

	[Tooltip("Shape of the impact signal")]
	[FormerlySerializedAs("m_ImpulseShape")]
	public ImpulseShapes ImpulseShape;

	[Tooltip("Defines the custom shape of the impact signal that will be generated.")]
	[FormerlySerializedAs("m_CustomImpulseShape")]
	public AnimationCurve CustomImpulseShape = new AnimationCurve();

	[Tooltip("The time during which the impact signal will occur.  The signal shape will be stretched to fill that time.")]
	[FormerlySerializedAs("m_ImpulseDuration")]
	public float ImpulseDuration = 0.2f;

	[Tooltip("How the impulse travels through space and time.")]
	[FormerlySerializedAs("m_ImpulseType")]
	public ImpulseTypes ImpulseType = ImpulseTypes.Legacy;

	[Tooltip("This defines how the widely signal will spread within the effect radius before dissipating with distance from the impact point")]
	[Range(0f, 1f)]
	[FormerlySerializedAs("m_DissipationRate")]
	public float DissipationRate;

	[Header("Signal Shape")]
	[Tooltip("Legacy mode only: Defines the signal that will be generated.")]
	[CinemachineEmbeddedAssetProperty(true)]
	[FormerlySerializedAs("m_RawSignal")]
	public SignalSourceAsset RawSignal;

	[Tooltip("Legacy mode only: Gain to apply to the amplitudes defined in the signal source.  1 is normal.  Setting this to 0 completely mutes the signal.")]
	[FormerlySerializedAs("m_AmplitudeGain")]
	public float AmplitudeGain = 1f;

	[Tooltip("Legacy mode only: Scale factor to apply to the time axis.  1 is normal.  Larger magnitudes will make the signal progress more rapidly.")]
	[FormerlySerializedAs("m_FrequencyGain")]
	public float FrequencyGain = 1f;

	[Tooltip("Legacy mode only: How to fit the signal into the envelope time")]
	[FormerlySerializedAs("m_RepeatMode")]
	public RepeatModes RepeatMode;

	[Tooltip("Legacy mode only: Randomize the signal start time")]
	[FormerlySerializedAs("m_Randomize")]
	public bool Randomize = true;

	[Tooltip("Legacy mode only: This defines the time-envelope of the signal.  The raw signal will be time-scaled to fit in the envelope.")]
	[FormerlySerializedAs("m_TimeEnvelope")]
	public CinemachineImpulseManager.EnvelopeDefinition TimeEnvelope = CinemachineImpulseManager.EnvelopeDefinition.Default;

	[Header("Spatial Range")]
	[Tooltip("Legacy mode only: The signal will have full amplitude in this radius surrounding the impact point.  Beyond that it will dissipate with distance.")]
	[FormerlySerializedAs("m_ImpactRadius")]
	public float ImpactRadius = 100f;

	[Tooltip("Legacy mode only: How the signal direction behaves as the listener moves away from the origin.")]
	[FormerlySerializedAs("m_DirectionMode")]
	public CinemachineImpulseManager.ImpulseEvent.DirectionModes DirectionMode;

	[Tooltip("Legacy mode only: This defines how the signal will dissipate with distance beyond the impact radius.")]
	[FormerlySerializedAs("m_DissipationMode")]
	public CinemachineImpulseManager.ImpulseEvent.DissipationModes DissipationMode = CinemachineImpulseManager.ImpulseEvent.DissipationModes.ExponentialDecay;

	[Tooltip("The signal will have no effect outside this radius surrounding the impact point.")]
	[FormerlySerializedAs("m_DissipationDistance")]
	public float DissipationDistance = 100f;

	[Tooltip("The speed (m/s) at which the impulse propagates through space.  High speeds allow listeners to react instantaneously, while slower speeds allow listeners in the scene to react as if to a wave spreading from the source.")]
	[FormerlySerializedAs("m_PropagationSpeed")]
	public float PropagationSpeed = 343f;

	private static AnimationCurve[] s_StandardShapes;

	internal AnimationCurve ImpulseCurve
	{
		get
		{
			if (ImpulseShape == ImpulseShapes.Custom)
			{
				if (CustomImpulseShape == null)
				{
					CustomImpulseShape = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				}
				return CustomImpulseShape;
			}
			return GetStandardCurve(ImpulseShape);
		}
	}

	public void OnValidate()
	{
		RuntimeUtility.NormalizeCurve(CustomImpulseShape, normalizeX: true, normalizeY: false);
		ImpulseDuration = Mathf.Max(0.0001f, ImpulseDuration);
		DissipationDistance = Mathf.Max(0.0001f, DissipationDistance);
		DissipationRate = Mathf.Clamp01(DissipationRate);
		PropagationSpeed = Mathf.Max(1f, PropagationSpeed);
		ImpactRadius = Mathf.Max(0f, ImpactRadius);
		TimeEnvelope.Validate();
		PropagationSpeed = Mathf.Max(1f, PropagationSpeed);
	}

	private static void CreateStandardShapes()
	{
		int num = 0;
		IEnumerator enumerator = Enum.GetValues(typeof(ImpulseShapes)).GetEnumerator();
		while (enumerator.MoveNext())
		{
			num = Mathf.Max(num, (int)enumerator.Current);
		}
		s_StandardShapes = new AnimationCurve[num + 1];
		s_StandardShapes[1] = new AnimationCurve(new Keyframe(0f, 1f, -3.2f, -3.2f), new Keyframe(1f, 0f, 0f, 0f));
		s_StandardShapes[2] = new AnimationCurve(new Keyframe(0f, 0f, -4.9f, -4.9f), new Keyframe(0.2f, 0f, 8.25f, 8.25f), new Keyframe(1f, 0f, -0.25f, -0.25f));
		s_StandardShapes[3] = new AnimationCurve(new Keyframe(0f, -1.4f, -7.9f, -7.9f), new Keyframe(0.27f, 0.78f, 23.4f, 23.4f), new Keyframe(0.54f, -0.12f, 22.6f, 22.6f), new Keyframe(0.75f, 0.042f, 9.23f, 9.23f), new Keyframe(0.9f, -0.02f, 5.8f, 5.8f), new Keyframe(0.95f, -0.006f, -3f, -3f), new Keyframe(1f, 0f, 0f, 0f));
		s_StandardShapes[4] = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.1f, 0.25f, 0f, 0f), new Keyframe(0.2f, 0f, 0f, 0f), new Keyframe(0.3f, 0.75f, 0f, 0f), new Keyframe(0.4f, 0f, 0f, 0f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(0.6f, 0f, 0f, 0f), new Keyframe(0.7f, 0.75f, 0f, 0f), new Keyframe(0.8f, 0f, 0f, 0f), new Keyframe(0.9f, 0.25f, 0f, 0f), new Keyframe(1f, 0f, 0f, 0f));
	}

	internal static AnimationCurve GetStandardCurve(ImpulseShapes shape)
	{
		if (s_StandardShapes == null)
		{
			CreateStandardShapes();
		}
		return s_StandardShapes[(int)shape];
	}

	public void CreateEvent(Vector3 position, Vector3 velocity)
	{
		CreateAndReturnEvent(position, velocity);
	}

	public CinemachineImpulseManager.ImpulseEvent CreateAndReturnEvent(Vector3 position, Vector3 velocity)
	{
		if (ImpulseType == ImpulseTypes.Legacy)
		{
			return LegacyCreateAndReturnEvent(position, velocity);
		}
		if ((ImpulseShape == ImpulseShapes.Custom && CustomImpulseShape == null) || Mathf.Abs(DissipationDistance) < 0.0001f || Mathf.Abs(ImpulseDuration) < 0.0001f)
		{
			return null;
		}
		CinemachineImpulseManager.ImpulseEvent impulseEvent = CinemachineImpulseManager.Instance.NewImpulseEvent();
		impulseEvent.Envelope = new CinemachineImpulseManager.EnvelopeDefinition
		{
			SustainTime = ImpulseDuration
		};
		impulseEvent.SignalSource = new SignalSource(this, velocity);
		impulseEvent.Position = position;
		impulseEvent.Radius = ((ImpulseType == ImpulseTypes.Uniform) ? 9999999f : 0f);
		impulseEvent.Channel = ImpulseChannel;
		impulseEvent.DirectionMode = CinemachineImpulseManager.ImpulseEvent.DirectionModes.Fixed;
		impulseEvent.DissipationDistance = ((ImpulseType == ImpulseTypes.Uniform) ? 0f : DissipationDistance);
		impulseEvent.PropagationSpeed = ((ImpulseType == ImpulseTypes.Propagating) ? PropagationSpeed : 9999999f);
		impulseEvent.CustomDissipation = DissipationRate;
		CinemachineImpulseManager.Instance.AddImpulseEvent(impulseEvent);
		return impulseEvent;
	}

	private CinemachineImpulseManager.ImpulseEvent LegacyCreateAndReturnEvent(Vector3 position, Vector3 velocity)
	{
		if (RawSignal == null || Mathf.Abs(TimeEnvelope.Duration) < 0.0001f)
		{
			return null;
		}
		CinemachineImpulseManager.ImpulseEvent impulseEvent = CinemachineImpulseManager.Instance.NewImpulseEvent();
		impulseEvent.Envelope = TimeEnvelope;
		impulseEvent.Envelope = TimeEnvelope;
		if (TimeEnvelope.ScaleWithImpact)
		{
			impulseEvent.Envelope.DecayTime *= Mathf.Sqrt(velocity.magnitude);
		}
		impulseEvent.SignalSource = new LegacySignalSource(this, velocity);
		impulseEvent.Position = position;
		impulseEvent.Radius = ImpactRadius;
		impulseEvent.Channel = ImpulseChannel;
		impulseEvent.DirectionMode = DirectionMode;
		impulseEvent.DissipationMode = DissipationMode;
		impulseEvent.DissipationDistance = DissipationDistance;
		impulseEvent.PropagationSpeed = PropagationSpeed;
		CinemachineImpulseManager.Instance.AddImpulseEvent(impulseEvent);
		return impulseEvent;
	}
}
