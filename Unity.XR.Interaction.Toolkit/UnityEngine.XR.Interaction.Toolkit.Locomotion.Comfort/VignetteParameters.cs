using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public sealed class VignetteParameters
{
	internal static class Defaults
	{
		public const float apertureSizeMax = 1f;

		public const float featheringEffectMax = 1f;

		public const float apertureVerticalPositionMax = 0.2f;

		public const float apertureVerticalPositionMin = -0.2f;

		public const float apertureSizeDefault = 0.7f;

		public const float featheringEffectDefault = 0.2f;

		public const float easeInTimeDefault = 0.3f;

		public const float easeOutTimeDefault = 0.3f;

		public const bool easeInTimeLockDefault = false;

		public const float easeOutDelayTimeDefault = 0f;

		public static readonly Color vignetteColorDefault = Color.black;

		public static readonly Color vignetteColorBlendDefault = Color.black;

		public const float apertureVerticalPositionDefault = 0f;

		public static readonly VignetteParameters defaultEffect = new VignetteParameters
		{
			apertureSize = 0.7f,
			featheringEffect = 0.2f,
			easeInTime = 0.3f,
			easeOutTime = 0.3f,
			easeInTimeLock = false,
			easeOutDelayTime = 0f,
			vignetteColor = vignetteColorDefault,
			vignetteColorBlend = vignetteColorBlendDefault,
			apertureVerticalPosition = 0f
		};

		public static readonly VignetteParameters noEffect = new VignetteParameters
		{
			apertureSize = 1f,
			featheringEffect = 0f,
			easeInTime = 0f,
			easeOutTime = 0f,
			easeInTimeLock = false,
			easeOutDelayTime = 0f,
			vignetteColor = vignetteColorDefault,
			vignetteColorBlend = vignetteColorBlendDefault,
			apertureVerticalPosition = 0f
		};
	}

	[SerializeField]
	[Range(0f, 1f)]
	private float m_ApertureSize = 0.7f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_FeatheringEffect = 0.2f;

	[SerializeField]
	private float m_EaseInTime = 0.3f;

	[SerializeField]
	private float m_EaseOutTime = 0.3f;

	[SerializeField]
	private bool m_EaseInTimeLock;

	[SerializeField]
	private float m_EaseOutDelayTime;

	[SerializeField]
	private Color m_VignetteColor = Defaults.vignetteColorDefault;

	[SerializeField]
	private Color m_VignetteColorBlend = Defaults.vignetteColorBlendDefault;

	[SerializeField]
	[Range(-0.2f, 0.2f)]
	private float m_ApertureVerticalPosition;

	public float apertureSize
	{
		get
		{
			return m_ApertureSize;
		}
		set
		{
			m_ApertureSize = value;
		}
	}

	public float featheringEffect
	{
		get
		{
			return m_FeatheringEffect;
		}
		set
		{
			m_FeatheringEffect = value;
		}
	}

	public float easeInTime
	{
		get
		{
			return m_EaseInTime;
		}
		set
		{
			m_EaseInTime = value;
		}
	}

	public float easeOutTime
	{
		get
		{
			return m_EaseOutTime;
		}
		set
		{
			m_EaseOutTime = value;
		}
	}

	public bool easeInTimeLock
	{
		get
		{
			return m_EaseInTimeLock;
		}
		set
		{
			m_EaseInTimeLock = value;
		}
	}

	public float easeOutDelayTime
	{
		get
		{
			return m_EaseOutDelayTime;
		}
		set
		{
			m_EaseOutDelayTime = value;
		}
	}

	public Color vignetteColor
	{
		get
		{
			return m_VignetteColor;
		}
		set
		{
			m_VignetteColor = value;
		}
	}

	public Color vignetteColorBlend
	{
		get
		{
			return m_VignetteColorBlend;
		}
		set
		{
			m_VignetteColorBlend = value;
		}
	}

	public float apertureVerticalPosition
	{
		get
		{
			return m_ApertureVerticalPosition;
		}
		set
		{
			m_ApertureVerticalPosition = value;
		}
	}

	public void CopyFrom(VignetteParameters parameters)
	{
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		apertureSize = parameters.apertureSize;
		featheringEffect = parameters.featheringEffect;
		easeInTime = parameters.easeInTime;
		easeOutTime = parameters.easeOutTime;
		easeInTimeLock = parameters.easeInTimeLock;
		easeOutDelayTime = parameters.easeOutDelayTime;
		vignetteColor = parameters.vignetteColor;
		vignetteColorBlend = parameters.vignetteColorBlend;
		apertureVerticalPosition = parameters.apertureVerticalPosition;
	}
}
