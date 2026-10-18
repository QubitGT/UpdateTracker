using System;

namespace UnityEngine.ProBuilder.MeshOperations;

[Serializable]
public sealed class MeshImportSettings
{
	[SerializeField]
	private bool m_Quads = true;

	[SerializeField]
	private bool m_Smoothing = true;

	[SerializeField]
	private float m_SmoothingThreshold = 1f;

	public bool quads
	{
		get
		{
			return m_Quads;
		}
		set
		{
			m_Quads = value;
		}
	}

	public bool smoothing
	{
		get
		{
			return m_Smoothing;
		}
		set
		{
			m_Smoothing = value;
		}
	}

	public float smoothingAngle
	{
		get
		{
			return m_SmoothingThreshold;
		}
		set
		{
			m_SmoothingThreshold = value;
		}
	}

	public override string ToString()
	{
		return $"quads: {quads}\nsmoothing: {smoothing}\nthreshold: {smoothingAngle}";
	}
}
