using System;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[Serializable]
public sealed class UnwrapParameters
{
	internal const float k_HardAngle = 88f;

	internal const float k_PackMargin = 20f;

	internal const float k_AngleError = 8f;

	internal const float k_AreaError = 15f;

	[Tooltip("Angle between neighbor triangles that will generate seam.")]
	[Range(1f, 180f)]
	[SerializeField]
	[FormerlySerializedAs("hardAngle")]
	private float m_HardAngle = 88f;

	[Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
	[Range(1f, 64f)]
	[SerializeField]
	[FormerlySerializedAs("packMargin")]
	private float m_PackMargin = 20f;

	[Tooltip("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
	[Range(1f, 75f)]
	[SerializeField]
	[FormerlySerializedAs("angleError")]
	private float m_AngleError = 8f;

	[Range(1f, 75f)]
	[SerializeField]
	[FormerlySerializedAs("areaError")]
	private float m_AreaError = 15f;

	public float hardAngle
	{
		get
		{
			return m_HardAngle;
		}
		set
		{
			m_HardAngle = value;
		}
	}

	public float packMargin
	{
		get
		{
			return m_PackMargin;
		}
		set
		{
			m_PackMargin = value;
		}
	}

	public float angleError
	{
		get
		{
			return m_AngleError;
		}
		set
		{
			m_AngleError = value;
		}
	}

	public float areaError
	{
		get
		{
			return m_AreaError;
		}
		set
		{
			m_AreaError = value;
		}
	}

	public UnwrapParameters()
	{
		Reset();
	}

	public UnwrapParameters(UnwrapParameters other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		hardAngle = other.hardAngle;
		packMargin = other.packMargin;
		angleError = other.angleError;
		areaError = other.areaError;
	}

	public void Reset()
	{
		hardAngle = 88f;
		packMargin = 20f;
		angleError = 8f;
		areaError = 15f;
	}

	public override string ToString()
	{
		return $"hardAngle: {hardAngle}\npackMargin: {packMargin}\nangleError: {angleError}\nareaError: {areaError}";
	}
}
