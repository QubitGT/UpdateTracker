using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[AddComponentMenu("XR/Teleportation Multi-Anchor Volume", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationMultiAnchorVolume.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportationMultiAnchorVolume : BaseTeleportationInteractable
{
	private static class DefaultDestinationFilterCache
	{
		private static FurthestTeleportationAnchorFilter s_FilterInstance;

		private static readonly HashSet<TeleportationMultiAnchorVolume> s_Users = new HashSet<TeleportationMultiAnchorVolume>();

		public static ITeleportationVolumeAnchorFilter SubscribeAndGetInstance(TeleportationMultiAnchorVolume user)
		{
			s_Users.Add(user);
			if (s_FilterInstance == null)
			{
				s_FilterInstance = ScriptableObject.CreateInstance<FurthestTeleportationAnchorFilter>();
			}
			return s_FilterInstance;
		}

		public static void Unsubscribe(TeleportationMultiAnchorVolume user)
		{
			s_Users.Remove(user);
			if (s_Users.Count == 0)
			{
				Object.Destroy(s_FilterInstance);
			}
		}
	}

	[SerializeField]
	[Tooltip("The transforms that represent the possible teleportation destinations.")]
	private List<Transform> m_AnchorTransforms = new List<Transform>();

	[SerializeField]
	[Tooltip("Settings for how this volume evaluates a destination anchor.")]
	private TeleportVolumeDestinationSettingsDatumProperty m_DestinationEvaluationSettings = new TeleportVolumeDestinationSettingsDatumProperty(new TeleportVolumeDestinationSettings());

	private ITeleportationVolumeAnchorFilter m_DefaultAnchorFilterCache;

	private bool m_WaitingToEvaluateDestination;

	private float m_WaitStartTime;

	private float m_LastDestinationQueryTime;

	public List<Transform> anchorTransforms => m_AnchorTransforms;

	public TeleportVolumeDestinationSettingsDatumProperty destinationEvaluationSettings
	{
		get
		{
			return m_DestinationEvaluationSettings;
		}
		set
		{
			m_DestinationEvaluationSettings = value;
		}
	}

	public ITeleportationVolumeAnchorFilter destinationEvaluationFilter
	{
		get
		{
			ITeleportationVolumeAnchorFilter teleportationVolumeAnchorFilter = m_DestinationEvaluationSettings.Value.destinationEvaluationFilter;
			if (teleportationVolumeAnchorFilter != null)
			{
				return teleportationVolumeAnchorFilter;
			}
			return m_DefaultAnchorFilterCache;
		}
	}

	public float destinationEvaluationProgress { get; private set; }

	public Transform destinationAnchor { get; private set; }

	private bool shouldDelayDestinationEvaluation
	{
		get
		{
			TeleportVolumeDestinationSettings value = m_DestinationEvaluationSettings.Value;
			if (value.enableDestinationEvaluationDelay)
			{
				return value.destinationEvaluationDelayTime > 0f;
			}
			return false;
		}
	}

	public event Action<TeleportationMultiAnchorVolume> destinationAnchorChanged;

	protected void OnDrawGizmosSelected()
	{
		foreach (Transform anchorTransform in m_AnchorTransforms)
		{
			Gizmos.color = Color.blue;
			GizmoHelpers.DrawWireCubeOriented(anchorTransform.position, anchorTransform.rotation, 1f);
			GizmoHelpers.DrawAxisArrows(anchorTransform, 1f);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_DefaultAnchorFilterCache = DefaultDestinationFilterCache.SubscribeAndGetInstance(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DefaultDestinationFilterCache.Unsubscribe(this);
	}

	protected override void OnHoverEntered(HoverEnterEventArgs args)
	{
		base.OnHoverEntered(args);
		if (base.interactorsHovering.Count == 1)
		{
			ClearDestinationAnchor();
			if (shouldDelayDestinationEvaluation)
			{
				m_WaitingToEvaluateDestination = true;
				m_WaitStartTime = Time.time;
			}
			else
			{
				EvaluateDestinationAnchor();
			}
		}
	}

	protected override void OnHoverExited(HoverExitEventArgs args)
	{
		base.OnHoverExited(args);
		if (!base.isHovered)
		{
			m_WaitingToEvaluateDestination = false;
			ClearDestinationAnchor();
		}
	}

	public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractable(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || !base.isHovered)
		{
			return;
		}
		TeleportVolumeDestinationSettings value = m_DestinationEvaluationSettings.Value;
		if (m_WaitingToEvaluateDestination)
		{
			destinationEvaluationProgress = (Time.time - m_WaitStartTime) / value.destinationEvaluationDelayTime;
			if (destinationEvaluationProgress >= 1f)
			{
				m_WaitingToEvaluateDestination = false;
				EvaluateDestinationAnchor();
			}
		}
		else
		{
			if (!value.pollForDestinationChange || !(Time.time - m_LastDestinationQueryTime > value.destinationPollFrequency))
			{
				return;
			}
			m_LastDestinationQueryTime = Time.time;
			int destinationAnchorIndex = destinationEvaluationFilter.GetDestinationAnchorIndex(this);
			if (destinationAnchorIndex >= 0 && destinationAnchorIndex < m_AnchorTransforms.Count && m_AnchorTransforms[destinationAnchorIndex] == destinationAnchor)
			{
				return;
			}
			ClearDestinationAnchor();
			if (shouldDelayDestinationEvaluation)
			{
				m_WaitingToEvaluateDestination = true;
				m_WaitStartTime = Time.time;
				return;
			}
			destinationEvaluationProgress = 1f;
			if (destinationAnchorIndex >= 0 && destinationAnchorIndex < m_AnchorTransforms.Count)
			{
				SetDestinationAtValidIndex(destinationAnchorIndex);
			}
		}
	}

	private void EvaluateDestinationAnchor()
	{
		destinationEvaluationProgress = 1f;
		m_LastDestinationQueryTime = Time.time;
		int destinationAnchorIndex = destinationEvaluationFilter.GetDestinationAnchorIndex(this);
		if (destinationAnchorIndex >= 0 && destinationAnchorIndex < m_AnchorTransforms.Count)
		{
			SetDestinationAtValidIndex(destinationAnchorIndex);
		}
	}

	private void SetDestinationAtValidIndex(int anchorIndex)
	{
		destinationAnchor = m_AnchorTransforms[anchorIndex];
		this.destinationAnchorChanged?.Invoke(this);
	}

	private void ClearDestinationAnchor()
	{
		destinationAnchor = null;
		destinationEvaluationProgress = 0f;
		this.destinationAnchorChanged?.Invoke(this);
	}

	public override Transform GetAttachTransform(IXRInteractor interactor)
	{
		if (!(destinationAnchor != null))
		{
			return base.GetAttachTransform(interactor);
		}
		return destinationAnchor;
	}

	protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
	{
		if (destinationAnchor == null)
		{
			return false;
		}
		Pose worldPose = destinationAnchor.GetWorldPose();
		teleportRequest.destinationPosition = worldPose.position;
		teleportRequest.destinationRotation = worldPose.rotation;
		ClearDestinationAnchor();
		return true;
	}
}
