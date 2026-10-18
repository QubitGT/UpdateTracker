using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[AddComponentMenu("XR/Teleportation Anchor", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportationAnchor : BaseTeleportationInteractable
{
	[SerializeField]
	[Tooltip("The Transform that represents the teleportation destination.")]
	private Transform m_TeleportAnchorTransform;

	public Transform teleportAnchorTransform
	{
		get
		{
			return m_TeleportAnchorTransform;
		}
		set
		{
			m_TeleportAnchorTransform = value;
		}
	}

	protected void OnValidate()
	{
		if (m_TeleportAnchorTransform == null)
		{
			m_TeleportAnchorTransform = base.transform;
		}
	}

	protected override void Reset()
	{
		m_TeleportAnchorTransform = base.transform;
	}

	protected void OnDrawGizmos()
	{
		if (!(m_TeleportAnchorTransform == null))
		{
			Gizmos.color = Color.blue;
			GizmoHelpers.DrawWireCubeOriented(m_TeleportAnchorTransform.position, m_TeleportAnchorTransform.rotation, 1f);
			GizmoHelpers.DrawAxisArrows(m_TeleportAnchorTransform, 1f);
		}
	}

	public override Transform GetAttachTransform(IXRInteractor interactor)
	{
		return m_TeleportAnchorTransform;
	}

	public void RequestTeleport()
	{
		SendTeleportRequest(null);
	}

	protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
	{
		if (m_TeleportAnchorTransform == null)
		{
			return false;
		}
		Pose worldPose = m_TeleportAnchorTransform.GetWorldPose();
		teleportRequest.destinationPosition = worldPose.position;
		teleportRequest.destinationRotation = worldPose.rotation;
		return true;
	}

	[ContextMenu("Teleport to anchor", false)]
	private void RequestTeleportFromEditor()
	{
		RequestTeleport();
	}

	[ContextMenu("Teleport to anchor", true)]
	private bool RequestTeleportFromEditorValidate()
	{
		return Application.isPlaying;
	}
}
