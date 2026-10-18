using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportVolumeDestinationSettings
{
	[SerializeField]
	[Tooltip("Whether to delay evaluation of the destination anchor until the user has hovered over the volume for a certain amount of time.")]
	private bool m_EnableDestinationEvaluationDelay;

	[SerializeField]
	[Tooltip("The amount of time, in seconds, for which the user must hover over the volume before it designates a destination anchor.")]
	private float m_DestinationEvaluationDelayTime = 1f;

	[SerializeField]
	[Tooltip("Whether to periodically query the filter for its calculated destination. If the determined anchor is not the current destination, the volume will initiate re-evaluation of the destination anchor.")]
	private bool m_PollForDestinationChange;

	[SerializeField]
	[Tooltip("The amount of time, in seconds, between queries to the filter for its calculated destination anchor.")]
	private float m_DestinationPollFrequency = 1f;

	[SerializeField]
	[RequireInterface(typeof(ITeleportationVolumeAnchorFilter))]
	[Tooltip("The anchor filter used to evaluate a teleportation destination. If set to None, the volume will use the anchor furthest from the user as the destination.")]
	private Object m_DestinationFilterObject;

	private ITeleportationVolumeAnchorFilter m_DestinationEvaluationFilter;

	[NonSerialized]
	private bool m_AssignedFilter;

	public bool enableDestinationEvaluationDelay
	{
		get
		{
			return m_EnableDestinationEvaluationDelay;
		}
		set
		{
			m_EnableDestinationEvaluationDelay = value;
		}
	}

	public float destinationEvaluationDelayTime
	{
		get
		{
			return m_DestinationEvaluationDelayTime;
		}
		set
		{
			m_DestinationEvaluationDelayTime = value;
		}
	}

	public bool pollForDestinationChange
	{
		get
		{
			return m_PollForDestinationChange;
		}
		set
		{
			m_PollForDestinationChange = value;
		}
	}

	public float destinationPollFrequency
	{
		get
		{
			return m_DestinationPollFrequency;
		}
		set
		{
			m_DestinationPollFrequency = value;
		}
	}

	public Object destinationFilterObject
	{
		get
		{
			return m_DestinationFilterObject;
		}
		set
		{
			m_DestinationFilterObject = value;
			m_DestinationEvaluationFilter = value as ITeleportationVolumeAnchorFilter;
			m_AssignedFilter = true;
		}
	}

	public ITeleportationVolumeAnchorFilter destinationEvaluationFilter
	{
		get
		{
			if (!m_AssignedFilter)
			{
				m_DestinationEvaluationFilter = m_DestinationFilterObject as ITeleportationVolumeAnchorFilter;
				m_AssignedFilter = true;
			}
			return m_DestinationEvaluationFilter;
		}
		set
		{
			m_DestinationEvaluationFilter = value;
			m_AssignedFilter = true;
		}
	}
}
