using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

[ExecuteInEditMode]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-add-camera-rig/")]
public class OVRCameraRig : MonoBehaviour
{
	public bool usePerEyeCameras;

	public bool useFixedUpdateForTracking;

	public bool disableEyeAnchorCameras;

	protected bool _skipUpdate;

	protected readonly string trackingSpaceName = "TrackingSpace";

	protected readonly string trackerAnchorName = "TrackerAnchor";

	protected readonly string leftEyeAnchorName = "LeftEyeAnchor";

	protected readonly string centerEyeAnchorName = "CenterEyeAnchor";

	protected readonly string rightEyeAnchorName = "RightEyeAnchor";

	protected readonly string leftHandAnchorName = "LeftHandAnchor";

	protected readonly string rightHandAnchorName = "RightHandAnchor";

	protected readonly string leftControllerAnchorName = "LeftControllerAnchor";

	protected readonly string rightControllerAnchorName = "RightControllerAnchor";

	protected readonly string leftHandAnchorDetachedName = "LeftHandAnchorDetached";

	protected readonly string rightHandAnchorDetachedName = "RightHandAnchorDetached";

	protected readonly string leftControllerInHandAnchorName = "LeftControllerInHandAnchor";

	protected readonly string leftHandOnControllerAnchorName = "LeftHandOnControllerAnchor";

	protected readonly string rightControllerInHandAnchorName = "RightControllerInHandAnchor";

	protected readonly string rightHandOnControllerAnchorName = "RightHandOnControllerAnchor";

	protected Camera _centerEyeCamera;

	protected Camera _leftEyeCamera;

	protected Camera _rightEyeCamera;

	protected Matrix4x4 _previousTrackingSpaceTransform;

	public Camera leftEyeCamera
	{
		get
		{
			if (!usePerEyeCameras)
			{
				return _centerEyeCamera;
			}
			return _leftEyeCamera;
		}
	}

	public Camera rightEyeCamera
	{
		get
		{
			if (!usePerEyeCameras)
			{
				return _centerEyeCamera;
			}
			return _rightEyeCamera;
		}
	}

	public Transform trackingSpace { get; private set; }

	public Transform leftEyeAnchor { get; private set; }

	public Transform centerEyeAnchor { get; private set; }

	public Transform rightEyeAnchor { get; private set; }

	public Transform leftHandAnchor { get; private set; }

	public Transform rightHandAnchor { get; private set; }

	public Transform leftHandAnchorDetached { get; private set; }

	public Transform rightHandAnchorDetached { get; private set; }

	public Transform leftControllerInHandAnchor { get; private set; }

	public Transform leftHandOnControllerAnchor { get; private set; }

	public Transform rightControllerInHandAnchor { get; private set; }

	public Transform rightHandOnControllerAnchor { get; private set; }

	public Transform leftControllerAnchor { get; private set; }

	public Transform rightControllerAnchor { get; private set; }

	public Transform trackerAnchor { get; private set; }

	public event Action<OVRCameraRig> UpdatedAnchors;

	public event Action<Transform> TrackingSpaceChanged;

	protected virtual void Awake()
	{
		_skipUpdate = true;
		EnsureGameObjectIntegrity();
	}

	protected virtual void Start()
	{
		UpdateAnchors(updateEyeAnchors: true, updateHandAnchors: true);
		Application.onBeforeRender += OnBeforeRenderCallback;
	}

	protected virtual void FixedUpdate()
	{
		if (useFixedUpdateForTracking)
		{
			UpdateAnchors(updateEyeAnchors: true, updateHandAnchors: true);
		}
	}

	protected virtual void Update()
	{
		_skipUpdate = false;
		if (!useFixedUpdateForTracking)
		{
			UpdateAnchors(updateEyeAnchors: true, updateHandAnchors: true);
		}
	}

	protected virtual void OnDestroy()
	{
		Application.onBeforeRender -= OnBeforeRenderCallback;
	}

	protected virtual void UpdateAnchors(bool updateEyeAnchors, bool updateHandAnchors)
	{
		if (!OVRManager.OVRManagerinitialized)
		{
			return;
		}
		EnsureGameObjectIntegrity();
		if (!Application.isPlaying)
		{
			return;
		}
		if (_skipUpdate)
		{
			centerEyeAnchor.FromOVRPose(OVRPose.identity, isLocal: true);
			leftEyeAnchor.FromOVRPose(OVRPose.identity, isLocal: true);
			rightEyeAnchor.FromOVRPose(OVRPose.identity, isLocal: true);
			return;
		}
		bool monoscopic = OVRManager.instance.monoscopic;
		bool flag = OVRNodeStateProperties.IsHmdPresent();
		OVRPose pose = OVRManager.tracker.GetPose();
		trackerAnchor.localRotation = pose.orientation;
		Quaternion localRotation = Quaternion.Euler(0f - OVRManager.instance.headPoseRelativeOffsetRotation.x, 0f - OVRManager.instance.headPoseRelativeOffsetRotation.y, OVRManager.instance.headPoseRelativeOffsetRotation.z);
		if (updateEyeAnchors)
		{
			if (flag)
			{
				Vector3 retVec = Vector3.zero;
				Quaternion retQuat = Quaternion.identity;
				if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out retVec))
				{
					centerEyeAnchor.localPosition = retVec;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out retQuat))
				{
					centerEyeAnchor.localRotation = retQuat;
				}
			}
			else
			{
				centerEyeAnchor.localRotation = localRotation;
				centerEyeAnchor.localPosition = OVRManager.instance.headPoseRelativeOffsetTranslation;
			}
			if (!flag || monoscopic)
			{
				leftEyeAnchor.localPosition = centerEyeAnchor.localPosition;
				rightEyeAnchor.localPosition = centerEyeAnchor.localPosition;
				leftEyeAnchor.localRotation = centerEyeAnchor.localRotation;
				rightEyeAnchor.localRotation = centerEyeAnchor.localRotation;
			}
			else
			{
				Vector3 retVec2 = Vector3.zero;
				Vector3 retVec3 = Vector3.zero;
				Quaternion retQuat2 = Quaternion.identity;
				Quaternion retQuat3 = Quaternion.identity;
				if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render, out retVec2))
				{
					leftEyeAnchor.localPosition = retVec2;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render, out retVec3))
				{
					rightEyeAnchor.localPosition = retVec3;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.LeftEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render, out retQuat2))
				{
					leftEyeAnchor.localRotation = retQuat2;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.RightEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render, out retQuat3))
				{
					rightEyeAnchor.localRotation = retQuat3;
				}
			}
		}
		if (updateHandAnchors)
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				Vector3 retVec4 = Vector3.zero;
				Vector3 retVec5 = Vector3.zero;
				Quaternion retQuat4 = Quaternion.identity;
				Quaternion retQuat5 = Quaternion.identity;
				if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandLeft, OVRPlugin.Step.Render, out retVec4))
				{
					leftHandAnchor.localPosition = retVec4;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandRight, OVRPlugin.Step.Render, out retVec5))
				{
					rightHandAnchor.localPosition = retVec5;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.LeftHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandLeft, OVRPlugin.Step.Render, out retQuat4))
				{
					leftHandAnchor.localRotation = retQuat4;
				}
				if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.RightHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandRight, OVRPlugin.Step.Render, out retQuat5))
				{
					rightHandAnchor.localRotation = retQuat5;
				}
			}
			else
			{
				OVRInput.Controller controller = OVRInput.GetActiveControllerForHand(OVRInput.Handedness.LeftHanded);
				OVRInput.Controller controller2 = OVRInput.GetActiveControllerForHand(OVRInput.Handedness.RightHanded);
				if (controller == OVRInput.Controller.None)
				{
					if (OVRInput.GetControllerPositionValid(OVRInput.Controller.LHand))
					{
						controller = OVRInput.Controller.LHand;
					}
					else if (OVRInput.GetControllerPositionValid(OVRInput.Controller.LTouch))
					{
						controller = OVRInput.Controller.LTouch;
					}
				}
				if (controller2 == OVRInput.Controller.None)
				{
					if (OVRInput.GetControllerPositionValid(OVRInput.Controller.RHand))
					{
						controller2 = OVRInput.Controller.RHand;
					}
					else if (OVRInput.GetControllerPositionValid(OVRInput.Controller.RTouch))
					{
						controller2 = OVRInput.Controller.RTouch;
					}
				}
				leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(controller);
				rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(controller2);
				leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(controller);
				rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(controller2);
				switch (OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandLeft))
				{
				case OVRInput.ControllerInHandState.ControllerNotInHand:
					leftHandAnchorDetached.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
					leftHandAnchorDetached.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
					leftHandOnControllerAnchor.localPosition = Vector3.zero;
					leftHandOnControllerAnchor.localRotation = Quaternion.identity;
					break;
				case OVRInput.ControllerInHandState.ControllerInHand:
				{
					Vector3 position = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand));
					leftHandOnControllerAnchor.localPosition = leftHandAnchor.InverseTransformPoint(position);
					leftHandOnControllerAnchor.localRotation = Quaternion.Inverse(leftHandAnchor.localRotation) * OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand);
					leftHandAnchorDetached.localPosition = Vector3.zero;
					leftHandAnchorDetached.localRotation = Quaternion.identity;
					break;
				}
				default:
					leftHandAnchorDetached.localPosition = Vector3.zero;
					leftHandAnchorDetached.localRotation = Quaternion.identity;
					leftHandOnControllerAnchor.localPosition = Vector3.zero;
					leftHandOnControllerAnchor.localRotation = Quaternion.identity;
					break;
				}
				switch (OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandRight))
				{
				case OVRInput.ControllerInHandState.ControllerNotInHand:
					rightHandAnchorDetached.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
					rightHandAnchorDetached.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
					rightHandOnControllerAnchor.localPosition = Vector3.zero;
					rightHandOnControllerAnchor.localRotation = Quaternion.identity;
					break;
				case OVRInput.ControllerInHandState.ControllerInHand:
				{
					Vector3 position2 = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand));
					rightHandOnControllerAnchor.localPosition = rightHandAnchor.InverseTransformPoint(position2);
					rightHandOnControllerAnchor.localRotation = Quaternion.Inverse(rightHandAnchor.localRotation) * OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand);
					rightHandAnchorDetached.localPosition = Vector3.zero;
					rightHandAnchorDetached.localRotation = Quaternion.identity;
					break;
				}
				default:
					rightHandAnchorDetached.localPosition = Vector3.zero;
					rightHandAnchorDetached.localRotation = Quaternion.identity;
					rightHandOnControllerAnchor.localPosition = Vector3.zero;
					rightHandOnControllerAnchor.localRotation = Quaternion.identity;
					break;
				}
			}
			trackerAnchor.localPosition = pose.position;
			OVRPose oVRPose = OVRPose.identity;
			OVRPose oVRPose2 = OVRPose.identity;
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				oVRPose = OVRManager.GetOpenVRControllerOffset(XRNode.LeftHand);
				oVRPose2 = OVRManager.GetOpenVRControllerOffset(XRNode.RightHand);
				OVRManager.SetOpenVRLocalPose(trackingSpace.InverseTransformPoint(leftControllerAnchor.position), trackingSpace.InverseTransformPoint(rightControllerAnchor.position), Quaternion.Inverse(trackingSpace.rotation) * leftControllerAnchor.rotation, Quaternion.Inverse(trackingSpace.rotation) * rightControllerAnchor.rotation);
			}
			rightControllerAnchor.localPosition = oVRPose2.position;
			rightControllerAnchor.localRotation = oVRPose2.orientation;
			leftControllerAnchor.localPosition = oVRPose.position;
			leftControllerAnchor.localRotation = oVRPose.orientation;
		}
		if (OVRManager.instance.LateLatching)
		{
			XRDisplaySubsystem currentDisplaySubsystem = OVRManager.GetCurrentDisplaySubsystem();
			if (currentDisplaySubsystem != null)
			{
				currentDisplaySubsystem.MarkTransformLateLatched(centerEyeAnchor.transform, XRDisplaySubsystem.LateLatchNode.Head);
				currentDisplaySubsystem.MarkTransformLateLatched(leftHandAnchor, XRDisplaySubsystem.LateLatchNode.LeftHand);
				currentDisplaySubsystem.MarkTransformLateLatched(rightHandAnchor, XRDisplaySubsystem.LateLatchNode.RightHand);
			}
		}
		RaiseUpdatedAnchorsEvent();
		CheckForTrackingSpaceChangesAndRaiseEvent();
	}

	protected virtual void OnBeforeRenderCallback()
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			bool lateControllerUpdate = OVRManager.instance.LateControllerUpdate;
			UpdateAnchors(updateEyeAnchors: true, lateControllerUpdate);
		}
	}

	protected virtual void CheckForTrackingSpaceChangesAndRaiseEvent()
	{
		if (!(trackingSpace == null))
		{
			Matrix4x4 localToWorldMatrix = trackingSpace.localToWorldMatrix;
			bool num = this.TrackingSpaceChanged != null && !_previousTrackingSpaceTransform.Equals(localToWorldMatrix);
			_previousTrackingSpaceTransform = localToWorldMatrix;
			if (num)
			{
				this.TrackingSpaceChanged(trackingSpace);
			}
		}
	}

	protected virtual void RaiseUpdatedAnchorsEvent()
	{
		if (this.UpdatedAnchors != null)
		{
			this.UpdatedAnchors(this);
		}
	}

	public virtual void EnsureGameObjectIntegrity()
	{
		bool flag = OVRManager.instance != null && OVRManager.instance.monoscopic;
		if (trackingSpace == null)
		{
			trackingSpace = ConfigureAnchor(null, trackingSpaceName);
			_previousTrackingSpaceTransform = trackingSpace.localToWorldMatrix;
		}
		if (leftEyeAnchor == null)
		{
			leftEyeAnchor = ConfigureAnchor(trackingSpace, leftEyeAnchorName);
		}
		if (centerEyeAnchor == null)
		{
			centerEyeAnchor = ConfigureAnchor(trackingSpace, centerEyeAnchorName);
		}
		if (rightEyeAnchor == null)
		{
			rightEyeAnchor = ConfigureAnchor(trackingSpace, rightEyeAnchorName);
		}
		if (leftHandAnchor == null)
		{
			leftHandAnchor = ConfigureAnchor(trackingSpace, leftHandAnchorName);
		}
		if (rightHandAnchor == null)
		{
			rightHandAnchor = ConfigureAnchor(trackingSpace, rightHandAnchorName);
		}
		if (leftHandAnchorDetached == null)
		{
			leftHandAnchorDetached = ConfigureAnchor(trackingSpace, leftHandAnchorDetachedName);
		}
		if (rightHandAnchorDetached == null)
		{
			rightHandAnchorDetached = ConfigureAnchor(trackingSpace, rightHandAnchorDetachedName);
		}
		if (leftControllerInHandAnchor == null)
		{
			leftControllerInHandAnchor = ConfigureAnchor(leftHandAnchor, leftControllerInHandAnchorName);
		}
		if (leftHandOnControllerAnchor == null)
		{
			leftHandOnControllerAnchor = ConfigureAnchor(leftControllerInHandAnchor, leftHandOnControllerAnchorName);
		}
		if (rightControllerInHandAnchor == null)
		{
			rightControllerInHandAnchor = ConfigureAnchor(rightHandAnchor, rightControllerInHandAnchorName);
		}
		if (rightHandOnControllerAnchor == null)
		{
			rightHandOnControllerAnchor = ConfigureAnchor(rightControllerInHandAnchor, rightHandOnControllerAnchorName);
		}
		if (trackerAnchor == null)
		{
			trackerAnchor = ConfigureAnchor(trackingSpace, trackerAnchorName);
		}
		if (leftControllerAnchor == null)
		{
			leftControllerAnchor = ConfigureAnchor(leftHandAnchor, leftControllerAnchorName);
		}
		if (rightControllerAnchor == null)
		{
			rightControllerAnchor = ConfigureAnchor(rightHandAnchor, rightControllerAnchorName);
		}
		if (_centerEyeCamera == null || _leftEyeCamera == null || _rightEyeCamera == null)
		{
			_centerEyeCamera = centerEyeAnchor.GetComponent<Camera>();
			_leftEyeCamera = leftEyeAnchor.GetComponent<Camera>();
			_rightEyeCamera = rightEyeAnchor.GetComponent<Camera>();
			if (_centerEyeCamera == null)
			{
				_centerEyeCamera = centerEyeAnchor.gameObject.AddComponent<Camera>();
				_centerEyeCamera.tag = "MainCamera";
			}
			if (_leftEyeCamera == null)
			{
				_leftEyeCamera = leftEyeAnchor.gameObject.AddComponent<Camera>();
				_leftEyeCamera.tag = "MainCamera";
			}
			if (_rightEyeCamera == null)
			{
				_rightEyeCamera = rightEyeAnchor.gameObject.AddComponent<Camera>();
				_rightEyeCamera.tag = "MainCamera";
			}
			if (GraphicsSettings.currentRenderPipeline == null)
			{
				_centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Both;
				_leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
				_rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;
			}
		}
		if (GraphicsSettings.currentRenderPipeline == null)
		{
			if (flag && !OVRPlugin.EyeTextureArrayEnabled)
			{
				if (_centerEyeCamera.stereoTargetEye != StereoTargetEyeMask.Left)
				{
					_centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
				}
			}
			else if (_centerEyeCamera.stereoTargetEye != StereoTargetEyeMask.Both)
			{
				_centerEyeCamera.stereoTargetEye = StereoTargetEyeMask.Both;
			}
		}
		if (disableEyeAnchorCameras)
		{
			_centerEyeCamera.enabled = false;
			_leftEyeCamera.enabled = false;
			_rightEyeCamera.enabled = false;
			return;
		}
		if (_centerEyeCamera.enabled == usePerEyeCameras || _leftEyeCamera.enabled == !usePerEyeCameras || _rightEyeCamera.enabled == (!usePerEyeCameras || (flag && !OVRPlugin.EyeTextureArrayEnabled)))
		{
			_skipUpdate = true;
		}
		_centerEyeCamera.enabled = !usePerEyeCameras;
		_leftEyeCamera.enabled = usePerEyeCameras;
		_rightEyeCamera.enabled = usePerEyeCameras && (!flag || OVRPlugin.EyeTextureArrayEnabled);
	}

	protected virtual Transform ConfigureAnchor(Transform root, string name)
	{
		Transform transform = ((root != null) ? root.Find(name) : null);
		if (transform == null)
		{
			transform = base.transform.Find(name);
		}
		if (transform == null)
		{
			transform = new GameObject(name).transform;
		}
		transform.name = name;
		transform.parent = ((root != null) ? root : base.transform);
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	public virtual Matrix4x4 ComputeTrackReferenceMatrix()
	{
		if (centerEyeAnchor == null)
		{
			Debug.LogError("centerEyeAnchor is required");
			return Matrix4x4.identity;
		}
		OVRPose identity = OVRPose.identity;
		if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Position, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec))
		{
			identity.position = retVec;
		}
		if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.Head, NodeStatePropertyType.Orientation, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retQuat))
		{
			identity.orientation = retQuat;
		}
		OVRPose oVRPose = identity.Inverse();
		Matrix4x4 matrix4x = Matrix4x4.TRS(oVRPose.position, oVRPose.orientation, Vector3.one);
		return centerEyeAnchor.localToWorldMatrix * matrix4x;
	}

	protected void CheckForAnchorsInParent()
	{
		Transform parent = base.transform.parent;
		while ((bool)parent)
		{
			Check<OVRSpatialAnchor>(parent);
			Check<OVRSceneAnchor>(parent);
			parent = parent.parent;
		}
		void Check<T>(Transform node) where T : MonoBehaviour
		{
			T component = node.GetComponent<T>();
			if ((bool)component && component.enabled)
			{
				component.enabled = false;
				Debug.LogError("The " + typeof(T).Name + " '" + component.name + "' is a parent of the OVRCameraRig '" + base.name + "', which is not allowed. An " + typeof(T).Name + " may not be the parent of an OVRCameraRig because the OVRCameraRig defines the tracking space for the anchor, and its transform is relative to the OVRCameraRig.");
			}
		}
	}
}
