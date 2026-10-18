using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Serializable]
public struct CinemachineBlendDefinition
{
	public delegate CinemachineBlendDefinition LookupBlendDelegate(ICinemachineCamera outgoing, ICinemachineCamera incoming);

	public enum Styles
	{
		Cut,
		EaseInOut,
		EaseIn,
		EaseOut,
		HardIn,
		HardOut,
		Linear,
		Custom
	}

	[Tooltip("Shape of the blend curve")]
	[FormerlySerializedAs("m_Style")]
	public Styles Style;

	[Tooltip("Duration of the blend, in seconds")]
	[FormerlySerializedAs("m_Time")]
	public float Time;

	[FormerlySerializedAs("m_CustomCurve")]
	public AnimationCurve CustomCurve;

	private static AnimationCurve[] s_StandardCurves;

	public float BlendTime
	{
		get
		{
			if (Style != 0)
			{
				return Time;
			}
			return 0f;
		}
	}

	public AnimationCurve BlendCurve
	{
		get
		{
			if (Style == Styles.Custom)
			{
				if (CustomCurve == null)
				{
					CustomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				}
				return CustomCurve;
			}
			if (s_StandardCurves == null)
			{
				CreateStandardCurves();
			}
			return s_StandardCurves[(int)Style];
		}
	}

	public CinemachineBlendDefinition(Styles style, float time)
	{
		Style = style;
		Time = time;
		CustomCurve = null;
	}

	private void CreateStandardCurves()
	{
		s_StandardCurves = new AnimationCurve[7];
		s_StandardCurves[0] = null;
		s_StandardCurves[1] = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		s_StandardCurves[2] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		Keyframe[] keys = s_StandardCurves[2].keys;
		keys[0].outTangent = 1.4f;
		keys[1].inTangent = 0f;
		s_StandardCurves[2].keys = keys;
		s_StandardCurves[3] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		keys = s_StandardCurves[3].keys;
		keys[0].outTangent = 0f;
		keys[1].inTangent = 1.4f;
		s_StandardCurves[3].keys = keys;
		s_StandardCurves[4] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		keys = s_StandardCurves[4].keys;
		keys[0].outTangent = 0f;
		keys[1].inTangent = 3f;
		s_StandardCurves[4].keys = keys;
		s_StandardCurves[5] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		keys = s_StandardCurves[5].keys;
		keys[0].outTangent = 3f;
		keys[1].inTangent = 0f;
		s_StandardCurves[5].keys = keys;
		s_StandardCurves[6] = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	}
}
