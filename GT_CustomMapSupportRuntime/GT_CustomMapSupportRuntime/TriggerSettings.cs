using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public abstract class TriggerSettings : MonoBehaviour
{
	[Tooltip("Hands and Body/Head colliders have inherently different settings in GorillaTag and cannot be detected on the same trigger.")]
	public TriggerSource triggeredBy = TriggerSource.HeadOrBody;

	[Tooltip("Deprecated, use the 'triggeredBy' property instead. \nHands and Body/Head colliders have inherently different settings in GorillaTag and cannot be detected on the same trigger.")]
	[HideInInspector]
	public bool triggeredByHands = true;

	[Tooltip("Deprecated, use the 'triggeredBy' property instead. \nHands and Body/Head colliders have inherently different settings in GorillaTag and cannot be detected on the same trigger.")]
	[HideInInspector]
	public bool triggeredByBody = true;

	[Tooltip("Deprecated, use the 'triggeredBy' property instead. \nHands and Body/Head colliders have inherently different settings in GorillaTag and cannot be detected on the same trigger.")]
	[HideInInspector]
	public bool triggeredByHead = true;

	[Tooltip("Should this Trigger re-trigger if a player stays inside it for long enough?")]
	public bool retriggerAfterDuration;

	[Tooltip("(Seconds) If 'retriggerAfterDuration' is TRUE, how long does a player need to Stay inside the Trigger before it re-triggers? If 'generalRetriggerDelay' is larger, that value will be used instead.")]
	public double retriggerStayDuration = 2.0;

	[HideInInspector]
	public float retriggerDelay = 2f;

	[Tooltip("(Seconds) When this trigger is Enabled/Activated, it can't be triggered until this duration has passed.")]
	public double onEnableTriggerDelay;

	[Tooltip("(Seconds) After being triggered, how long before this trigger can be triggered again?")]
	public double generalRetriggerDelay;

	[Tooltip("How many times is this Trigger allowed to trigger? 0 means infinite")]
	public byte numAllowedTriggers;

	[Tooltip("Validation Distance is used to validate network synced trigger activations and is automatically calculated during the Map Export process for single-collider triggers using a Box, Sphere, or Capsule collider. To customize this, or if using a MeshCollider or multi-collider setup, you can set this override to a positive, non-zero value. Generally it should be equal to about 1.5 times the full collider radius (including scale). For example: if using a Sphere collider with radius 2.0 and its GameObject has a scale of 3.0 (resulting in an actual radius of 6.0), you would set this value to (2.0 * 3.0) * 1.5 = 9.0")]
	public float validationDistanceOverride = -1f;

	[HideInInspector]
	public byte triggerId;

	[HideInInspector]
	public float validationDistance = 1f;

	[HideInInspector]
	public bool syncedToAllPlayers_private;

	public virtual void PropagateProperties()
	{
	}
}
