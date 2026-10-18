using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-wide-motion-mode/")]
public class OVRHandTrackingWideMotionModeSample : MonoBehaviour
{
	[SerializeField]
	public Toggle fusionToggle;

	[SerializeField]
	private LineRenderer leftLinePointer;

	[SerializeField]
	private LineRenderer rightLinePointer;

	[SerializeField]
	private OVRHand handLeft;

	[SerializeField]
	private OVRHand handRight;

	[SerializeField]
	private OVRInputModule inputModule;

	private void OnEnable()
	{
		fusionToggle.onValueChanged.AddListener(OnFusionToggleChanged);
	}

	private void OnDisable()
	{
		fusionToggle.onValueChanged.RemoveListener(OnFusionToggleChanged);
	}

	private void Update()
	{
		UpdateLineRenderer();
	}

	private void UpdateLineRenderer()
	{
		leftLinePointer.enabled = false;
		rightLinePointer.enabled = false;
		UpdateLineRendererForHand(isLeft: false);
		UpdateLineRendererForHand(isLeft: true);
	}

	private void UpdateLineRendererForHand(bool isLeft)
	{
		Transform transform = null;
		if (isLeft)
		{
			if (handLeft != null && handLeft.IsPointerPoseValid)
			{
				transform = handLeft.PointerPose;
				if (handLeft.GetFingerIsPinching(OVRHand.HandFinger.Index))
				{
					inputModule.rayTransform = transform;
				}
			}
			else
			{
				transform = null;
			}
		}
		else
		{
			if (handRight != null && handRight.IsPointerPoseValid)
			{
				transform = handRight.PointerPose;
				if (handRight.GetFingerIsPinching(OVRHand.HandFinger.Index))
				{
					inputModule.rayTransform = transform;
				}
			}
			else
			{
				transform = null;
			}
			transform = ((handRight != null && handRight.IsPointerPoseValid) ? handRight.PointerPose : null);
		}
		if (!(transform == null))
		{
			Vector3 position = transform.position;
			LineRenderer obj = (isLeft ? leftLinePointer : rightLinePointer);
			Ray ray = new Ray(position, transform.rotation * Vector3.forward);
			obj.enabled = true;
			obj.SetPosition(0, transform.position + ray.direction * 0.05f);
			obj.SetPosition(1, position + ray.direction * 2.5f);
		}
	}

	private void OnFusionToggleChanged(bool newValue)
	{
		OVRManager.instance.wideMotionModeHandPosesEnabled = newValue;
	}
}
