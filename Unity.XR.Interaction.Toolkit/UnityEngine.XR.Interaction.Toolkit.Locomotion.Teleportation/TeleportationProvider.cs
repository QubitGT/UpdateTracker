using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[AddComponentMenu("XR/Locomotion/Teleportation Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider.html")]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class TeleportationProvider : LocomotionProvider
{
	[SerializeField]
	[Tooltip("The time (in seconds) to delay the teleportation once it is activated.")]
	private float m_DelayTime;

	private float m_DelayStartTime;

	protected TeleportRequest currentRequest { get; set; }

	protected bool validRequest { get; set; }

	public float delayTime
	{
		get
		{
			return m_DelayTime;
		}
		set
		{
			m_DelayTime = value;
		}
	}

	public override bool canStartMoving
	{
		get
		{
			if (!(m_DelayTime <= 0f))
			{
				return Time.time - m_DelayStartTime >= m_DelayTime;
			}
			return true;
		}
	}

	public XROriginUpAlignment upTransformation { get; set; } = new XROriginUpAlignment();


	public XRCameraForwardXZAlignment forwardTransformation { get; set; } = new XRCameraForwardXZAlignment();


	public XRBodyGroundPosition positionTransformation { get; set; } = new XRBodyGroundPosition();


	public virtual bool QueueTeleportRequest(TeleportRequest teleportRequest)
	{
		currentRequest = teleportRequest;
		validRequest = true;
		return true;
	}

	protected virtual void Update()
	{
		if (!validRequest)
		{
			return;
		}
		if (base.locomotionState == LocomotionState.Idle)
		{
			if (m_DelayTime > 0f)
			{
				if (TryPrepareLocomotion())
				{
					m_DelayStartTime = Time.time;
				}
			}
			else
			{
				TryStartLocomotionImmediately();
			}
		}
		if (base.locomotionState == LocomotionState.Moving)
		{
			switch (currentRequest.matchOrientation)
			{
			case MatchOrientation.WorldSpaceUp:
				upTransformation.targetUp = Vector3.up;
				TryQueueTransformation(upTransformation);
				break;
			case MatchOrientation.TargetUp:
				upTransformation.targetUp = currentRequest.destinationRotation * Vector3.up;
				TryQueueTransformation(upTransformation);
				break;
			case MatchOrientation.TargetUpAndForward:
				upTransformation.targetUp = currentRequest.destinationRotation * Vector3.up;
				TryQueueTransformation(upTransformation);
				forwardTransformation.targetDirection = currentRequest.destinationRotation * Vector3.forward;
				TryQueueTransformation(forwardTransformation);
				break;
			}
			positionTransformation.targetPosition = currentRequest.destinationPosition;
			TryQueueTransformation(positionTransformation);
			TryEndLocomotion();
			validRequest = false;
		}
	}
}
