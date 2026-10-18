using System;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.EventSystems;

[Feature(Feature.VirtualKeyboard)]
public class OVRVirtualKeyboardSampleInputHandler : MonoBehaviour
{
	private const float RAY_MAX_DISTANCE = 100f;

	private const float THUMBSTICK_DEADZONE = 0.2f;

	private const float COLLISION_BOUNDS_ADDED_BLEED_PERCENT = 0.1f;

	private const float LINEPOINTER_THINNING_THRESHOLD = 0.015f;

	public OVRVirtualKeyboard OVRVirtualKeyboard;

	[SerializeField]
	private OVRRaycaster raycaster;

	[SerializeField]
	private OVRInputModule inputModule;

	[SerializeField]
	private LineRenderer leftLinePointer;

	[SerializeField]
	private LineRenderer rightLinePointer;

	private OVRInput.Controller? interactionDevice_;

	private float linePointerInitialWidth_;

	public float AnalogStickX => ApplyDeadzone(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x);

	public float AnalogStickY => ApplyDeadzone(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y);

	public Vector3 InputRayPosition => inputModule.rayTransform.position;

	public Quaternion InputRayRotation
	{
		get
		{
			if (interactionDevice_ != OVRInput.Controller.LHand)
			{
				return inputModule.rayTransform.rotation;
			}
			return inputModule.rayTransform.rotation * Quaternion.Euler(Vector3.forward * 180f);
		}
	}

	private static float ApplyDeadzone(float value)
	{
		if (value > 0.2f)
		{
			return (value - 0.2f) / 0.8f;
		}
		if (value < -0.2f)
		{
			return (value + 0.2f) / 0.8f;
		}
		return 0f;
	}

	private void Start()
	{
		LineRenderer lineRenderer = rightLinePointer;
		bool flag2 = (leftLinePointer.enabled = false);
		lineRenderer.enabled = flag2;
		linePointerInitialWidth_ = Math.Max(rightLinePointer.startWidth, leftLinePointer.startWidth);
	}

	private void Update()
	{
		UpdateInteractionAnchor();
		UpdateLineRenderer();
	}

	private void UpdateLineRenderer()
	{
		if (leftLinePointer != null)
		{
			leftLinePointer.enabled = false;
			UpdateLineRendererFromSource(OVRVirtualKeyboard.InputSource.ControllerLeft);
			UpdateLineRendererFromSource(OVRVirtualKeyboard.InputSource.HandLeft);
		}
		if (rightLinePointer != null)
		{
			rightLinePointer.enabled = false;
			UpdateLineRendererFromSource(OVRVirtualKeyboard.InputSource.ControllerRight);
			UpdateLineRendererFromSource(OVRVirtualKeyboard.InputSource.HandRight);
		}
	}

	private void UpdateLineRendererFromSource(OVRVirtualKeyboard.InputSource source)
	{
		Transform transform = null;
		switch (source)
		{
		case OVRVirtualKeyboard.InputSource.ControllerLeft:
			transform = ((OVRInput.IsControllerConnected(OVRInput.Controller.LTouch) && (OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandLeft) == OVRInput.ControllerInHandState.NoHand || OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandLeft) == OVRInput.ControllerInHandState.ControllerInHand)) ? OVRVirtualKeyboard.leftControllerDirectTransform : null);
			break;
		case OVRVirtualKeyboard.InputSource.ControllerRight:
			transform = ((OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && (OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandRight) == OVRInput.ControllerInHandState.NoHand || OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandRight) == OVRInput.ControllerInHandState.ControllerInHand)) ? OVRVirtualKeyboard.rightControllerDirectTransform : null);
			break;
		case OVRVirtualKeyboard.InputSource.HandLeft:
			transform = (OVRVirtualKeyboard.handLeft.IsPointerPoseValid ? OVRVirtualKeyboard.handLeft.PointerPose : null);
			break;
		case OVRVirtualKeyboard.InputSource.HandRight:
			transform = (OVRVirtualKeyboard.handRight.IsPointerPoseValid ? OVRVirtualKeyboard.handRight.PointerPose : null);
			break;
		}
		if (transform == null || transform.position == Vector3.zero)
		{
			return;
		}
		Vector3 position = transform.position;
		LineRenderer lineRenderer = ((source == OVRVirtualKeyboard.InputSource.ControllerLeft || source == OVRVirtualKeyboard.InputSource.HandLeft) ? leftLinePointer : rightLinePointer);
		lineRenderer.startWidth = linePointerInitialWidth_;
		if ((bool)OVRVirtualKeyboard && OVRVirtualKeyboard.isActiveAndEnabled && (bool)OVRVirtualKeyboard.Collider)
		{
			Vector3 vector = OVRVirtualKeyboard.transform.InverseTransformPoint(position) * OVRVirtualKeyboard.transform.localScale.x;
			Bounds bounds = default(Bounds);
			bounds.size = OVRVirtualKeyboard.Collider.bounds.size;
			Bounds bounds2 = bounds;
			bounds2.Expand(Vector3.one * 0.1f);
			if (bounds2.Contains(vector))
			{
				lineRenderer.enabled = false;
				return;
			}
			float magnitude = (bounds2.ClosestPoint(vector) - vector).magnitude;
			if (magnitude < 0.015f)
			{
				lineRenderer.startWidth = Mathf.Lerp(0f, linePointerInitialWidth_, magnitude / 0.015f);
			}
		}
		lineRenderer.endWidth = lineRenderer.startWidth;
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, transform.position);
		Ray ray = new Ray(position, transform.rotation * Vector3.forward);
		if ((bool)OVRVirtualKeyboard.Collider && OVRVirtualKeyboard.Collider.Raycast(ray, out var hitInfo, 100f))
		{
			lineRenderer.SetPosition(1, hitInfo.point);
		}
		else
		{
			lineRenderer.SetPosition(1, position + ray.direction * 2.5f);
		}
	}

	private void UpdateInteractionAnchor()
	{
		OVRInput.Controller controller = OVRInput.Controller.None;
		controller = ((OVRVirtualKeyboard.leftControllerRootTransform != null && OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)) ? OVRInput.Controller.LTouch : controller);
		controller = ((OVRVirtualKeyboard.rightControllerRootTransform != null && OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) ? OVRInput.Controller.RTouch : controller);
		controller = ((OVRVirtualKeyboard.handLeft != null && OVRVirtualKeyboard.handLeft.GetFingerIsPinching(OVRHand.HandFinger.Index)) ? OVRInput.Controller.LHand : controller);
		controller = ((OVRVirtualKeyboard.handRight != null && OVRVirtualKeyboard.handRight.GetFingerIsPinching(OVRHand.HandFinger.Index)) ? OVRInput.Controller.RHand : controller);
		int num;
		switch (controller)
		{
		case OVRInput.Controller.None:
			return;
		default:
			num = ((controller == OVRInput.Controller.LTouch) ? 1 : 0);
			break;
		case OVRInput.Controller.LHand:
			num = 1;
			break;
		}
		bool flag = (byte)num != 0;
		raycaster.pointer = (flag ? OVRVirtualKeyboard.handLeft.gameObject : OVRVirtualKeyboard.handRight.gameObject);
		interactionDevice_ = controller;
		OVRInputModule oVRInputModule = inputModule;
		oVRInputModule.rayTransform = controller switch
		{
			OVRInput.Controller.LHand => OVRVirtualKeyboard.handLeft.PointerPose, 
			OVRInput.Controller.LTouch => OVRVirtualKeyboard.handLeft.transform, 
			OVRInput.Controller.RHand => OVRVirtualKeyboard.handRight.PointerPose, 
			_ => OVRVirtualKeyboard.handRight.transform, 
		};
	}
}
