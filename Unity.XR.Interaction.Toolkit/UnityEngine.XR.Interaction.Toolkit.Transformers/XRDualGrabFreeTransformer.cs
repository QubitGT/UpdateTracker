using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

[AddComponentMenu("XR/Transformers/XR Dual Grab Free Transformer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRDualGrabFreeTransformer.html")]
public class XRDualGrabFreeTransformer : XRBaseGrabTransformer
{
	public enum PoseContributor
	{
		First,
		Second,
		Average
	}

	[SerializeField]
	private PoseContributor m_MultiSelectPosition;

	[SerializeField]
	private PoseContributor m_MultiSelectRotation = PoseContributor.Average;

	private Vector3 m_LastUp;

	public PoseContributor multiSelectPosition
	{
		get
		{
			return m_MultiSelectPosition;
		}
		set
		{
			m_MultiSelectPosition = value;
		}
	}

	public PoseContributor multiSelectRotation
	{
		get
		{
			return m_MultiSelectRotation;
		}
		set
		{
			m_MultiSelectRotation = value;
		}
	}

	protected override RegistrationMode registrationMode => RegistrationMode.Multiple;

	internal Pose lastInteractorAttachPose { get; private set; }

	protected virtual void OnDrawGizmosSelected()
	{
	}

	public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
	{
		base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
		if (grabInteractable.interactorsSelecting.Count == 2)
		{
			m_LastUp = grabInteractable.transform.up;
		}
	}

	public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
		{
			UpdateTarget(grabInteractable, ref targetPose);
		}
	}

	private void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose)
	{
		if (grabInteractable.interactorsSelecting.Count == 1)
		{
			XRSingleGrabFreeTransformer.UpdateTarget(grabInteractable, ref targetPose);
		}
		else
		{
			UpdateTargetMulti(grabInteractable, ref targetPose);
		}
	}

	private void UpdateTargetMulti(XRGrabInteractable grabInteractable, ref Pose targetPose)
	{
		Pose worldPose = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).GetWorldPose();
		Pose worldPose2 = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable).GetWorldPose();
		Pose pose = worldPose;
		switch (m_MultiSelectPosition)
		{
		default:
			pose.position = worldPose.position;
			break;
		case PoseContributor.Second:
			pose.position = worldPose2.position;
			break;
		case PoseContributor.Average:
			pose.position = (worldPose.position + worldPose2.position) * 0.5f;
			break;
		}
		Vector3 vector = (worldPose2.position - worldPose.position).normalized;
		Vector3 rhs;
		Vector3 vector2;
		switch (m_MultiSelectRotation)
		{
		default:
			vector2 = worldPose.up;
			rhs = worldPose.right;
			if (vector == Vector3.zero)
			{
				vector = worldPose.forward;
			}
			break;
		case PoseContributor.Second:
			vector2 = worldPose2.up;
			rhs = worldPose2.right;
			if (vector == Vector3.zero)
			{
				vector = worldPose2.forward;
			}
			break;
		case PoseContributor.Average:
			vector2 = Vector3.Slerp(worldPose.up, worldPose2.up, 0.5f);
			rhs = Vector3.Slerp(worldPose.right, worldPose2.right, 0.5f);
			if (vector == Vector3.zero)
			{
				vector = worldPose.forward;
			}
			break;
		}
		Vector3 a = Vector3.Cross(vector, rhs);
		float num = Mathf.PingPong(Vector3.Angle(vector2, vector), 90f);
		vector2 = Vector3.Slerp(a, vector2, num / 90f);
		Vector3 rhs2 = Vector3.Cross(vector2, vector);
		vector2 = Vector3.Cross(vector, rhs2);
		if (Vector3.Dot(vector2, m_LastUp) <= 0f)
		{
			vector2 = -vector2;
		}
		m_LastUp = vector2;
		pose.rotation = Quaternion.LookRotation(vector, vector2);
		lastInteractorAttachPose = pose;
		if (!grabInteractable.trackRotation)
		{
			targetPose.position = pose.position;
			return;
		}
		if (m_MultiSelectRotation == PoseContributor.First || m_MultiSelectRotation == PoseContributor.Second)
		{
			int index = ((m_MultiSelectRotation != 0) ? 1 : 0);
			Transform attachTransform = grabInteractable.GetAttachTransform(grabInteractable.interactorsSelecting[index]);
			Vector3 direction = grabInteractable.transform.GetWorldPose().position - attachTransform.position;
			Vector3 vector3 = attachTransform.InverseTransformDirection(direction);
			targetPose.position = pose.rotation * vector3 + pose.position;
		}
		else if (m_MultiSelectRotation == PoseContributor.Average)
		{
			targetPose.position = pose.position;
		}
		targetPose.rotation = pose.rotation;
	}
}
