using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class ClimbSettings
{
	[SerializeField]
	[Tooltip("Controls whether to allow unconstrained movement along the climb interactable's x-axis.")]
	private bool m_AllowFreeXMovement = true;

	[SerializeField]
	[Tooltip("Controls whether to allow unconstrained movement along the climb interactable's y-axis.")]
	private bool m_AllowFreeYMovement = true;

	[SerializeField]
	[Tooltip("Controls whether to allow unconstrained movement along the climb interactable's z-axis.")]
	private bool m_AllowFreeZMovement = true;

	public bool allowFreeXMovement
	{
		get
		{
			return m_AllowFreeXMovement;
		}
		set
		{
			m_AllowFreeXMovement = value;
		}
	}

	public bool allowFreeYMovement
	{
		get
		{
			return m_AllowFreeYMovement;
		}
		set
		{
			m_AllowFreeYMovement = value;
		}
	}

	public bool allowFreeZMovement
	{
		get
		{
			return m_AllowFreeZMovement;
		}
		set
		{
			m_AllowFreeZMovement = value;
		}
	}
}
