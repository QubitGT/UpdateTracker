using Meta.XR.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-handtracking/")]
[Feature(Feature.Hands)]
public class OVRHand : MonoBehaviour, OVRInputModule.InputSource, OVRSkeleton.IOVRSkeletonDataProvider, OVRSkeletonRenderer.IOVRSkeletonRendererDataProvider, OVRMesh.IOVRMeshDataProvider, OVRMeshRenderer.IOVRMeshRendererDataProvider
{
	public enum Hand
	{
		None = -1,
		HandLeft,
		HandRight
	}

	public enum HandFinger
	{
		Thumb,
		Index,
		Middle,
		Ring,
		Pinky,
		Max
	}

	public enum TrackingConfidence
	{
		Low = 0,
		High = 1065353216
	}

	public enum MicrogestureType
	{
		NoGesture = 0,
		SwipeLeft = 1,
		SwipeRight = 2,
		SwipeForward = 3,
		SwipeBackward = 4,
		ThumbTap = 5,
		Invalid = -1
	}

	[SerializeField]
	internal Hand HandType = Hand.None;

	[SerializeField]
	private Transform _pointerPoseRoot;

	public OVRInput.InputDeviceShowState m_showState = OVRInput.InputDeviceShowState.ControllerNotInHand;

	public OVRRayHelper RayHelper;

	private GameObject _pointerPoseGO;

	private OVRPlugin.HandState _handState;

	private bool _wasIndexPinching;

	private bool _wasReleased;

	private OVRPlugin.HandTrackingState _handTrackingState;

	private bool _handTrackingStateValid;

	private static OVRHandSkeletonVersion GlobalHandSkeletonVersion => OVRRuntimeSettings.Instance.HandSkeletonVersion;

	public bool IsDataValid { get; private set; }

	public bool IsDataHighConfidence { get; private set; }

	public bool IsTracked { get; private set; }

	public bool IsSystemGestureInProgress { get; private set; }

	public bool IsPointerPoseValid { get; private set; }

	public Transform PointerPose
	{
		get
		{
			if (_pointerPoseGO == null)
			{
				InitializePointerPose();
			}
			return _pointerPoseGO.transform;
		}
	}

	public float HandScale { get; private set; }

	public TrackingConfidence HandConfidence { get; private set; }

	public bool IsDominantHand { get; private set; }

	private void InitializePointerPose()
	{
		_pointerPoseGO = new GameObject(string.Format("{0} {1}", HandType, "PointerPose"));
		Object.DontDestroyOnLoad(_pointerPoseGO);
		_pointerPoseGO.hideFlags = HideFlags.HideAndDontSave;
		if (_pointerPoseRoot != null)
		{
			PointerPose.SetParent(_pointerPoseRoot, worldPositionStays: false);
		}
	}

	private void Awake()
	{
		if (_pointerPoseGO == null)
		{
			InitializePointerPose();
		}
		if (RayHelper != null)
		{
			RayHelper.transform.SetParent(PointerPose, worldPositionStays: false);
		}
		GetHandState(OVRPlugin.Step.Render);
	}

	private void Update()
	{
		GetHandState(OVRPlugin.Step.Render);
		bool fingerIsPinching = GetFingerIsPinching(HandFinger.Index);
		_wasReleased = !fingerIsPinching && _wasIndexPinching;
		_wasIndexPinching = fingerIsPinching;
		if ((bool)RayHelper && !IsActive() && RayHelper.isActiveAndEnabled)
		{
			RayHelper.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		if (OVRPlugin.nativeXrApi != OVRPlugin.XrApi.OpenXR)
		{
			GetHandState(OVRPlugin.Step.Physics);
		}
		if (RayHelper != null)
		{
			RayHelper.gameObject.SetActive(IsDataValid);
		}
	}

	private void OnDestroy()
	{
		if (_pointerPoseGO != null)
		{
			Object.Destroy(_pointerPoseGO);
		}
	}

	private void GetHandState(OVRPlugin.Step step)
	{
		if (OVRPlugin.GetHandState(step, (OVRPlugin.Hand)HandType, ref _handState))
		{
			IsTracked = (_handState.Status & OVRPlugin.HandStatus.HandTracked) != 0;
			IsSystemGestureInProgress = (_handState.Status & OVRPlugin.HandStatus.SystemGestureInProgress) != 0;
			IsPointerPoseValid = (_handState.Status & OVRPlugin.HandStatus.InputStateValid) != 0;
			IsDominantHand = (_handState.Status & OVRPlugin.HandStatus.DominantHand) != 0;
			PointerPose.localPosition = _handState.PointerPose.Position.FromFlippedZVector3f();
			PointerPose.localRotation = _handState.PointerPose.Orientation.FromFlippedZQuatf();
			HandScale = _handState.HandScale;
			HandConfidence = (TrackingConfidence)_handState.HandConfidence;
			IsDataValid = true;
			IsDataHighConfidence = IsTracked && HandConfidence == TrackingConfidence.High;
			_handTrackingStateValid = OVRPlugin.GetHandTrackingState(step, (OVRPlugin.Hand)HandType, ref _handTrackingState);
			OVRInput.ControllerInHandState controllerIsInHandState = OVRInput.GetControllerIsInHandState((OVRInput.Hand)HandType);
			if (controllerIsInHandState == OVRInput.ControllerInHandState.ControllerInHand)
			{
				IsSystemGestureInProgress = false;
				IsPointerPoseValid = false;
			}
			switch (m_showState)
			{
			case OVRInput.InputDeviceShowState.ControllerInHandOrNoHand:
				if (controllerIsInHandState == OVRInput.ControllerInHandState.ControllerNotInHand)
				{
					IsDataValid = false;
				}
				break;
			case OVRInput.InputDeviceShowState.ControllerInHand:
				if (controllerIsInHandState != OVRInput.ControllerInHandState.ControllerInHand)
				{
					IsDataValid = false;
				}
				break;
			case OVRInput.InputDeviceShowState.ControllerNotInHand:
				if (controllerIsInHandState != OVRInput.ControllerInHandState.ControllerNotInHand)
				{
					IsDataValid = false;
				}
				break;
			case OVRInput.InputDeviceShowState.NoHand:
				if (controllerIsInHandState != 0)
				{
					IsDataValid = false;
				}
				break;
			}
			if (OVRPlugin.HandSkeletonVersion != GlobalHandSkeletonVersion)
			{
				IsDataValid = false;
			}
		}
		else
		{
			IsTracked = false;
			IsSystemGestureInProgress = false;
			IsPointerPoseValid = false;
			PointerPose.localPosition = Vector3.zero;
			PointerPose.localRotation = Quaternion.identity;
			HandScale = 1f;
			HandConfidence = TrackingConfidence.Low;
			IsDataValid = false;
			IsDataHighConfidence = false;
			_handTrackingStateValid = false;
		}
	}

	public bool GetFingerIsPinching(HandFinger finger)
	{
		if (IsDataValid)
		{
			return ((uint)_handState.Pinches & (uint)(1 << (int)finger)) != 0;
		}
		return false;
	}

	public float GetFingerPinchStrength(HandFinger finger)
	{
		if (IsDataValid && _handState.PinchStrength != null && _handState.PinchStrength.Length == 5)
		{
			return _handState.PinchStrength[(int)finger];
		}
		return 0f;
	}

	public TrackingConfidence GetFingerConfidence(HandFinger finger)
	{
		if (IsDataValid && _handState.FingerConfidences != null && _handState.FingerConfidences.Length == 5)
		{
			return (TrackingConfidence)_handState.FingerConfidences[(int)finger];
		}
		return TrackingConfidence.Low;
	}

	OVRSkeleton.SkeletonType OVRSkeleton.IOVRSkeletonDataProvider.GetSkeletonType()
	{
		return HandType switch
		{
			Hand.HandLeft => GlobalHandSkeletonVersion switch
			{
				OVRHandSkeletonVersion.OVR => OVRSkeleton.SkeletonType.HandLeft, 
				OVRHandSkeletonVersion.OpenXR => OVRSkeleton.SkeletonType.XRHandLeft, 
				_ => OVRSkeleton.SkeletonType.None, 
			}, 
			Hand.HandRight => GlobalHandSkeletonVersion switch
			{
				OVRHandSkeletonVersion.OVR => OVRSkeleton.SkeletonType.HandRight, 
				OVRHandSkeletonVersion.OpenXR => OVRSkeleton.SkeletonType.XRHandRight, 
				_ => OVRSkeleton.SkeletonType.None, 
			}, 
			_ => OVRSkeleton.SkeletonType.None, 
		};
	}

	OVRSkeleton.SkeletonPoseData OVRSkeleton.IOVRSkeletonDataProvider.GetSkeletonPoseData()
	{
		OVRSkeleton.SkeletonPoseData result = default(OVRSkeleton.SkeletonPoseData);
		result.IsDataValid = IsDataValid;
		if (IsDataValid)
		{
			result.RootPose = _handState.RootPose;
			result.RootScale = _handState.HandScale;
			result.BoneRotations = _handState.BoneRotations;
			result.BoneTranslations = _handState.BonePositions;
			result.IsDataHighConfidence = IsTracked && HandConfidence == TrackingConfidence.High;
		}
		return result;
	}

	OVRSkeletonRenderer.SkeletonRendererData OVRSkeletonRenderer.IOVRSkeletonRendererDataProvider.GetSkeletonRendererData()
	{
		OVRSkeletonRenderer.SkeletonRendererData result = default(OVRSkeletonRenderer.SkeletonRendererData);
		result.IsDataValid = IsDataValid;
		if (IsDataValid)
		{
			result.RootScale = _handState.HandScale;
			result.IsDataHighConfidence = IsTracked && HandConfidence == TrackingConfidence.High;
			result.ShouldUseSystemGestureMaterial = IsSystemGestureInProgress;
		}
		return result;
	}

	public MicrogestureType GetMicrogestureType()
	{
		OVRPlugin.SendMicrogestureHint();
		if (!_handTrackingStateValid)
		{
			return MicrogestureType.Invalid;
		}
		int microgesture = (int)_handTrackingState.Microgesture;
		if (microgesture < 0 || microgesture > 5)
		{
			return MicrogestureType.Invalid;
		}
		return (MicrogestureType)microgesture;
	}

	OVRMesh.MeshType OVRMesh.IOVRMeshDataProvider.GetMeshType()
	{
		return HandType switch
		{
			Hand.None => OVRMesh.MeshType.None, 
			Hand.HandLeft => GlobalHandSkeletonVersion switch
			{
				OVRHandSkeletonVersion.OVR => OVRMesh.MeshType.HandLeft, 
				OVRHandSkeletonVersion.OpenXR => OVRMesh.MeshType.XRHandLeft, 
				_ => OVRMesh.MeshType.None, 
			}, 
			Hand.HandRight => GlobalHandSkeletonVersion switch
			{
				OVRHandSkeletonVersion.OVR => OVRMesh.MeshType.HandRight, 
				OVRHandSkeletonVersion.OpenXR => OVRMesh.MeshType.XRHandRight, 
				_ => OVRMesh.MeshType.None, 
			}, 
			_ => OVRMesh.MeshType.None, 
		};
	}

	OVRMeshRenderer.MeshRendererData OVRMeshRenderer.IOVRMeshRendererDataProvider.GetMeshRendererData()
	{
		OVRMeshRenderer.MeshRendererData result = default(OVRMeshRenderer.MeshRendererData);
		result.IsDataValid = IsDataValid;
		if (IsDataValid)
		{
			result.IsDataHighConfidence = IsTracked && HandConfidence == TrackingConfidence.High;
			result.ShouldUseSystemGestureMaterial = IsSystemGestureInProgress;
		}
		return result;
	}

	public void OnEnable()
	{
		OVRInputModule.TrackInputSource(this);
		SceneManager.activeSceneChanged += OnSceneChanged;
		if ((bool)RayHelper && ShouldShowHandUIRay())
		{
			RayHelper.gameObject.SetActive(value: true);
		}
	}

	public void OnDisable()
	{
		OVRInputModule.UntrackInputSource(this);
		SceneManager.activeSceneChanged -= OnSceneChanged;
		if ((bool)RayHelper)
		{
			RayHelper.gameObject.SetActive(value: false);
		}
	}

	private void OnSceneChanged(Scene unloading, Scene loading)
	{
		OVRInputModule.TrackInputSource(this);
	}

	public void OnValidate()
	{
		OVRSkeleton component = GetComponent<OVRSkeleton>();
		if (component != null && component.GetSkeletonType() != HandType.AsSkeletonType(GlobalHandSkeletonVersion))
		{
			component.SetSkeletonType(HandType.AsSkeletonType(GlobalHandSkeletonVersion));
		}
		OVRMesh component2 = GetComponent<OVRMesh>();
		if (component2 != null && component2.GetMeshType() != HandType.AsMeshType(GlobalHandSkeletonVersion))
		{
			component2.SetMeshType(HandType.AsMeshType(GlobalHandSkeletonVersion));
		}
	}

	public bool IsPressed()
	{
		return GetFingerIsPinching(HandFinger.Index);
	}

	public bool IsReleased()
	{
		return _wasReleased;
	}

	public Transform GetPointerRayTransform()
	{
		PointerPose.name = base.name;
		return PointerPose;
	}

	private bool ShouldShowHandUIRay()
	{
		if (m_showState == OVRInput.InputDeviceShowState.ControllerInHand)
		{
			return OVRPlugin.AreControllerDrivenHandPosesNatural();
		}
		return true;
	}

	public bool IsValid()
	{
		return this != null;
	}

	public bool IsActive()
	{
		if (ShouldShowHandUIRay())
		{
			return IsDataValid;
		}
		return false;
	}

	public OVRPlugin.Hand GetHand()
	{
		return (OVRPlugin.Hand)HandType;
	}

	public void UpdatePointerRay(OVRInputRayData rayData)
	{
		if ((bool)RayHelper)
		{
			rayData.ActivationStrength = GetFingerPinchStrength(HandFinger.Index);
			RayHelper.UpdatePointerRay(rayData);
		}
	}

	bool OVRSkeleton.IOVRSkeletonDataProvider.get_enabled()
	{
		return base.enabled;
	}
}
