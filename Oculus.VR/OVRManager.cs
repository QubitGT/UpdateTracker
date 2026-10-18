using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-add-camera-rig/#configure-settings")]
public class OVRManager : MonoBehaviour, OVRMixedRealityCaptureConfiguration
{
	public enum XrApi
	{
		Unknown,
		CAPI,
		VRAPI,
		OpenXR
	}

	public enum TrackingOrigin
	{
		EyeLevel = 0,
		FloorLevel = 1,
		Stage = 2,
		Stationary = 6
	}

	public enum EyeTextureFormat
	{
		Default = 0,
		R16G16B16A16_FP = 2,
		R11G11B10_FP = 3
	}

	public enum FoveatedRenderingLevel
	{
		Off,
		Low,
		Medium,
		High,
		HighTop
	}

	[Obsolete("Please use FoveatedRenderingLevel instead")]
	public enum FixedFoveatedRenderingLevel
	{
		Off,
		Low,
		Medium,
		High,
		HighTop
	}

	[Obsolete("Please use FoveatedRenderingLevel instead")]
	public enum TiledMultiResLevel
	{
		Off,
		LMSLow,
		LMSMedium,
		LMSHigh,
		LMSHighTop
	}

	public enum SystemHeadsetType
	{
		None = 0,
		Oculus_Quest = 8,
		Oculus_Quest_2 = 9,
		Meta_Quest_Pro = 10,
		Meta_Quest_3 = 11,
		Meta_Quest_3S = 12,
		Placeholder_13 = 13,
		Placeholder_14 = 14,
		Placeholder_15 = 15,
		Placeholder_16 = 16,
		Placeholder_17 = 17,
		Placeholder_18 = 18,
		Placeholder_19 = 19,
		Placeholder_20 = 20,
		Rift_DK1 = 4096,
		Rift_DK2 = 4097,
		Rift_CV1 = 4098,
		Rift_CB = 4099,
		Rift_S = 4100,
		Oculus_Link_Quest = 4101,
		Oculus_Link_Quest_2 = 4102,
		Meta_Link_Quest_Pro = 4103,
		Meta_Link_Quest_3 = 4104,
		Meta_Link_Quest_3S = 4105,
		PC_Placeholder_4106 = 4106,
		PC_Placeholder_4107 = 4107,
		PC_Placeholder_4108 = 4108,
		PC_Placeholder_4109 = 4109,
		PC_Placeholder_4110 = 4110,
		PC_Placeholder_4111 = 4111,
		PC_Placeholder_4112 = 4112,
		PC_Placeholder_4113 = 4113
	}

	public enum SystemHeadsetTheme
	{
		Dark,
		Light
	}

	public enum XRDevice
	{
		Unknown,
		Oculus,
		OpenVR
	}

	public enum ColorSpace
	{
		Unknown,
		Unmanaged,
		Rec_2020,
		Rec_709,
		Rift_CV1,
		Rift_S,
		[InspectorName("Quest 1")]
		Quest,
		[InspectorName("DCI-P3 (Recommended)")]
		P3,
		Adobe_RGB
	}

	public enum ProcessorPerformanceLevel
	{
		PowerSavings,
		SustainedLow,
		SustainedHigh,
		Boost
	}

	public enum ControllerDrivenHandPosesType
	{
		None,
		ConformingToController,
		Natural
	}

	public interface EventListener
	{
		void OnEvent(OVRPlugin.EventDataBuffer eventData);
	}

	public enum CompositionMethod
	{
		External,
		[Obsolete("Deprecated. Direct composition is no longer supported", false)]
		Direct
	}

	[Obsolete("Deprecated", false)]
	public enum CameraDevice
	{
		WebCamera0,
		WebCamera1,
		ZEDCamera
	}

	[Obsolete("Deprecated", false)]
	public enum DepthQuality
	{
		Low,
		Medium,
		High
	}

	[Obsolete("Deprecated", false)]
	public enum VirtualGreenScreenType
	{
		Off,
		[Obsolete("Deprecated. This enum value will not be supported in OpenXR", false)]
		OuterBoundary,
		PlayArea
	}

	public enum MrcActivationMode
	{
		Automatic,
		Disabled
	}

	public enum MrcCameraType
	{
		Normal,
		Foreground,
		Background
	}

	public delegate GameObject InstantiateMrcCameraDelegate(GameObject mainCameraGameObject, MrcCameraType cameraType);

	private enum PassthroughInitializationState
	{
		Unspecified,
		Pending,
		Initialized,
		Failed
	}

	public class PassthroughCapabilities
	{
		public bool SupportsPassthrough { get; }

		public bool SupportsColorPassthrough { get; }

		public uint MaxColorLutResolution { get; }

		public PassthroughCapabilities(bool supportsPassthrough, bool supportsColorPassthrough, uint maxColorLutResolution)
		{
			SupportsPassthrough = supportsPassthrough;
			SupportsColorPassthrough = supportsColorPassthrough;
			MaxColorLutResolution = maxColorLutResolution;
		}
	}

	private class Observable<T>
	{
		private T _value;

		public Action<T> OnChanged;

		public T Value
		{
			get
			{
				return _value;
			}
			set
			{
				_ = _value;
				_value = value;
				if (OnChanged != null)
				{
					OnChanged(value);
				}
			}
		}

		public Observable()
		{
		}

		public Observable(T defaultValue)
		{
			_value = defaultValue;
		}

		public Observable(T defaultValue, Action<T> callback)
			: this(defaultValue)
		{
			OnChanged = (Action<T>)Delegate.Combine(OnChanged, callback);
		}
	}

	protected static OVRProfile _profile;

	protected IEnumerable<Camera> disabledCameras;

	private static int _isHmdPresentCacheFrame = -1;

	private static bool _isHmdPresent = false;

	private static bool _wasHmdPresent = false;

	private static bool _hasVrFocusCached = false;

	private static bool _hasVrFocus = false;

	private static bool _hadVrFocus = false;

	private static bool _hadInputFocus = true;

	[Header("Performance/Quality")]
	[SerializeField]
	[Tooltip("If true, both eyes will see the same image, rendered from the center eye pose, saving performance.")]
	private bool _monoscopic;

	[SerializeField]
	[Tooltip("The sharpen filter of the eye buffer. This amplifies contrast and fine details.")]
	private OVRPlugin.LayerSharpenType _sharpenType;

	[HideInInspector]
	private ColorSpace _colorGamut = ColorSpace.P3;

	[SerializeField]
	[HideInInspector]
	[Tooltip("Enable Dynamic Resolution. This will allocate render buffers to maxDynamicResolutionScale size and will change the viewport to adapt performance. Mobile only.")]
	private bool _enableDynamicResolution;

	[HideInInspector]
	public float minDynamicResolutionScale = 1f;

	[HideInInspector]
	public float maxDynamicResolutionScale = 1f;

	[SerializeField]
	[HideInInspector]
	public float quest2MinDynamicResolutionScale = 0.7f;

	[SerializeField]
	[HideInInspector]
	public float quest2MaxDynamicResolutionScale = 1.3f;

	[SerializeField]
	[HideInInspector]
	public float quest3MinDynamicResolutionScale = 0.7f;

	[SerializeField]
	[HideInInspector]
	public float quest3MaxDynamicResolutionScale = 1.6f;

	private const int _pixelStepPerFrame = 32;

	[Range(0.5f, 2f)]
	[HideInInspector]
	[Tooltip("Min RenderScale the app can reach under adaptive resolution mode")]
	[Obsolete("Deprecated. Use minDynamicRenderScale instead.", false)]
	public float minRenderScale = 0.7f;

	[Range(0.5f, 2f)]
	[HideInInspector]
	[Tooltip("Max RenderScale the app can reach under adaptive resolution mode")]
	[Obsolete("Deprecated. Use maxDynamicRenderScale instead.", false)]
	public float maxRenderScale = 1f;

	[SerializeField]
	[Tooltip("Set the relative offset rotation of head poses")]
	private Vector3 _headPoseRelativeOffsetRotation;

	[SerializeField]
	[Tooltip("Set the relative offset translation of head poses")]
	private Vector3 _headPoseRelativeOffsetTranslation;

	public int profilerTcpPort = 32419;

	[HideInInspector]
	public bool expandMixedRealityCapturePropertySheet;

	[HideInInspector]
	[Tooltip("If true, Mixed Reality mode will be enabled. It would be always set to false when the game is launching without editor")]
	public bool enableMixedReality;

	[HideInInspector]
	public CompositionMethod compositionMethod;

	[HideInInspector]
	[Tooltip("Extra hidden layers")]
	public LayerMask extraHiddenLayers;

	[HideInInspector]
	[Tooltip("Extra visible layers")]
	public LayerMask extraVisibleLayers;

	[HideInInspector]
	[Tooltip("Dynamic Culling Mask")]
	public bool dynamicCullingMask = true;

	[HideInInspector]
	[Tooltip("Backdrop color for Rift (External Compositon)")]
	public Color externalCompositionBackdropColorRift = Color.green;

	[HideInInspector]
	[Tooltip("Backdrop color for Quest (External Compositon)")]
	public Color externalCompositionBackdropColorQuest = Color.clear;

	[HideInInspector]
	[Tooltip("The camera device for direct composition")]
	[Obsolete("Deprecated", false)]
	public CameraDevice capturingCameraDevice;

	[HideInInspector]
	[Tooltip("Flip the camera frame horizontally")]
	[Obsolete("Deprecated", false)]
	public bool flipCameraFrameHorizontally;

	[HideInInspector]
	[Tooltip("Flip the camera frame vertically")]
	[Obsolete("Deprecated", false)]
	public bool flipCameraFrameVertically;

	[HideInInspector]
	[Tooltip("Delay the touch controller pose by a short duration (0 to 0.5 second) to match the physical camera latency")]
	[Obsolete("Deprecated", false)]
	public float handPoseStateLatency;

	[HideInInspector]
	[Tooltip("Delay the foreground / background image in the sandwich composition to match the physical camera latency. The maximum duration is sandwichCompositionBufferedFrames / {Game FPS}")]
	[Obsolete("Deprecated", false)]
	public float sandwichCompositionRenderLatency;

	[HideInInspector]
	[Tooltip("The number of frames are buffered in the SandWich composition. The more buffered frames, the more memory it would consume.")]
	[Obsolete("Deprecated", false)]
	public int sandwichCompositionBufferedFrames = 8;

	[HideInInspector]
	[Tooltip("Chroma Key Color")]
	[Obsolete("Deprecated", false)]
	public Color chromaKeyColor = Color.green;

	[HideInInspector]
	[Tooltip("Chroma Key Similarity")]
	[Obsolete("Deprecated", false)]
	public float chromaKeySimilarity = 0.6f;

	[HideInInspector]
	[Tooltip("Chroma Key Smooth Range")]
	[Obsolete("Deprecated", false)]
	public float chromaKeySmoothRange = 0.03f;

	[HideInInspector]
	[Tooltip("Chroma Key Spill Range")]
	[Obsolete("Deprecated", false)]
	public float chromaKeySpillRange = 0.06f;

	[HideInInspector]
	[Tooltip("Use dynamic lighting (Depth sensor required)")]
	[Obsolete("Deprecated", false)]
	public bool useDynamicLighting;

	[HideInInspector]
	[Tooltip("The quality level of depth image. The lighting could be more smooth and accurate with high quality depth, but it would also be more costly in performance.")]
	[Obsolete("Deprecated", false)]
	public DepthQuality depthQuality = DepthQuality.Medium;

	[HideInInspector]
	[Tooltip("Smooth factor in dynamic lighting. Larger is smoother")]
	[Obsolete("Deprecated", false)]
	public float dynamicLightingSmoothFactor = 8f;

	[HideInInspector]
	[Tooltip("The maximum depth variation across the edges. Make it smaller to smooth the lighting on the edges.")]
	[Obsolete("Deprecated", false)]
	public float dynamicLightingDepthVariationClampingValue = 0.001f;

	[HideInInspector]
	[Tooltip("Type of virutal green screen ")]
	[Obsolete("Deprecated", false)]
	public VirtualGreenScreenType virtualGreenScreenType;

	[HideInInspector]
	[Tooltip("Top Y of virtual green screen")]
	[Obsolete("Deprecated", false)]
	public float virtualGreenScreenTopY = 10f;

	[HideInInspector]
	[Tooltip("Bottom Y of virtual green screen")]
	[Obsolete("Deprecated", false)]
	public float virtualGreenScreenBottomY = -10f;

	[HideInInspector]
	[Tooltip("When using a depth camera (e.g. ZED), whether to use the depth in virtual green screen culling.")]
	[Obsolete("Deprecated", false)]
	public bool virtualGreenScreenApplyDepthCulling;

	[HideInInspector]
	[Tooltip("The tolerance value (in meter) when using the virtual green screen with a depth camera. Make it bigger if the foreground objects got culled incorrectly.")]
	[Obsolete("Deprecated", false)]
	public float virtualGreenScreenDepthTolerance = 0.2f;

	[HideInInspector]
	[Tooltip("(Quest-only) control if the mixed reality capture mode can be activated automatically through remote network connection.")]
	public MrcActivationMode mrcActivationMode;

	public InstantiateMrcCameraDelegate instantiateMixedRealityCameraGameObject;

	[HideInInspector]
	[Tooltip("Specify if simultaneous hands and controllers should be enabled. ")]
	public bool launchSimultaneousHandsControllersOnStartup;

	[HideInInspector]
	[Tooltip("Specify if Insight Passthrough should be enabled. Passthrough layers can only be used if passthrough is enabled.")]
	public bool isInsightPassthroughEnabled;

	[HideInInspector]
	public bool shouldBoundaryVisibilityBeSuppressed;

	private bool _updateBoundaryLogOnce;

	[SerializeField]
	[HideInInspector]
	internal bool requestBodyTrackingPermissionOnStartup;

	[SerializeField]
	[HideInInspector]
	internal bool requestFaceTrackingPermissionOnStartup;

	[SerializeField]
	[HideInInspector]
	internal bool requestEyeTrackingPermissionOnStartup;

	[SerializeField]
	[HideInInspector]
	internal bool requestScenePermissionOnStartup;

	[SerializeField]
	[HideInInspector]
	internal bool requestRecordAudioPermissionOnStartup;

	public static string OCULUS_UNITY_NAME_STR = "Oculus";

	public static string OPENVR_UNITY_NAME_STR = "OpenVR";

	public static XRDevice loadedXRDevice;

	private static bool _isSystemHeadsetThemeCached = false;

	private static SystemHeadsetTheme _cachedSystemHeadsetTheme = SystemHeadsetTheme.Dark;

	protected static Vector3 OpenVRTouchRotationOffsetEulerLeft = new Vector3(40f, 0f, 0f);

	protected static Vector3 OpenVRTouchRotationOffsetEulerRight = new Vector3(40f, 0f, 0f);

	protected static Vector3 OpenVRTouchPositionOffsetLeft = new Vector3(0.0075f, -0.005f, -0.0525f);

	protected static Vector3 OpenVRTouchPositionOffsetRight = new Vector3(-0.0075f, -0.005f, -0.0525f);

	protected static WeakReference<Camera> m_lastSpaceWarpCamera;

	protected static bool m_SpaceWarpEnabled;

	protected static Transform m_AppSpaceTransform;

	protected static DepthTextureMode m_CachedDepthTextureMode;

	[SerializeField]
	[Tooltip("Available only for devices that support local dimming. It improves visual quality with a better display contrast ratio, but at a minor GPU performance cost.")]
	private bool _localDimming = true;

	[Header("Tracking")]
	[SerializeField]
	[Tooltip("Defines the current tracking origin type.")]
	private TrackingOrigin _trackingOriginType = TrackingOrigin.FloorLevel;

	[Tooltip("If true, head tracking will affect the position of each OVRCameraRig's cameras.")]
	public bool usePositionTracking = true;

	[HideInInspector]
	public bool useRotationTracking = true;

	[Tooltip("If true, the distance between the user's eyes will affect the position of each OVRCameraRig's cameras.")]
	public bool useIPDInPositionTracking = true;

	[Tooltip("If true, each scene load will cause the head pose to reset. This function only works on Rift.")]
	public bool resetTrackerOnLoad;

	[Tooltip("If true, the Reset View in the universal menu will cause the pose to be reset in PC VR. This should generally be enabled for applications with a stationary position in the virtual world and will allow the View Reset command to place the person back to a predefined location (such as a cockpit seat). Set this to false if you have a locomotion system because resetting the view would effectively teleport the player to potentially invalid locations.")]
	public bool AllowRecenter = true;

	[Tooltip("If true, rendered controller latency is reduced by several ms, as the left/right controllers will have their positions updated right before rendering.")]
	public bool LateControllerUpdate = true;

	[Tooltip("Late latching is a feature that can reduce rendered head/controller latency by a substantial amount. Before enabling, be sure to go over the documentation to ensure that the feature is used correctly. This feature must also be enabled through the Oculus XR Plugin settings.")]
	public bool LateLatching;

	private static ControllerDrivenHandPosesType _readOnlyControllerDrivenHandPosesType = ControllerDrivenHandPosesType.None;

	[Tooltip("Defines if hand poses can be populated by controller data.")]
	public ControllerDrivenHandPosesType controllerDrivenHandPosesType;

	[Tooltip("Allows the application to use simultaneous hands and controllers functionality. This option must be enabled at build time.")]
	public bool SimultaneousHandsAndControllersEnabled;

	[SerializeField]
	[HideInInspector]
	private bool _readOnlyWideMotionModeHandPosesEnabled;

	[Tooltip("Defines if hand poses can leverage algorithms to retrieve hand poses outside of the normal tracking area.")]
	public bool wideMotionModeHandPosesEnabled;

	private static bool _isUserPresentCached = false;

	private static bool _isUserPresent = false;

	private static bool _wasUserPresent = false;

	private static bool prevAudioOutIdIsCached = false;

	private static bool prevAudioInIdIsCached = false;

	private static string prevAudioOutId = string.Empty;

	private static string prevAudioInId = string.Empty;

	private static bool wasPositionTracked = false;

	private static OVRPlugin.EventDataBuffer eventDataBuffer = default(OVRPlugin.EventDataBuffer);

	private HashSet<EventListener> eventListeners = new HashSet<EventListener>();

	public static string UnityAlphaOrBetaVersionWarningMessage = "WARNING: It's not recommended to use Unity alpha/beta release in Oculus development. Use a stable release if you encounter any issue.";

	public static int MaxDynamicResolutionVersion = 1;

	[SerializeField]
	[HideInInspector]
	public int dynamicResolutionVersion;

	public static bool OVRManagerinitialized = false;

	private static List<XRDisplaySubsystem> s_displaySubsystems;

	private static List<XRDisplaySubsystemDescriptor> s_displaySubsystemDescriptors;

	private static List<XRInputSubsystem> s_inputSubsystems;

	private static bool multipleMainCameraWarningPresented = false;

	private static bool suppressUnableToFindMainCameraMessage = false;

	private static WeakReference<Camera> lastFoundMainCamera = null;

	public static bool staticMixedRealityCaptureInitialized = false;

	public static bool staticPrevEnableMixedRealityCapture = false;

	public static OVRMixedRealityCaptureSettings staticMrcSettings = null;

	private static bool suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;

	public static Action<bool> OnPassthroughInitializedStateChange;

	private static Observable<PassthroughInitializationState> _passthroughInitializationState = new Observable<PassthroughInitializationState>(PassthroughInitializationState.Unspecified, delegate(PassthroughInitializationState newValue)
	{
		OnPassthroughInitializedStateChange?.Invoke(newValue == PassthroughInitializationState.Initialized);
	});

	private static PassthroughCapabilities _passthroughCapabilities;

	public static OVRManager instance { get; private set; }

	public static OVRDisplay display { get; private set; }

	public static OVRTracker tracker { get; private set; }

	public static OVRBoundary boundary { get; private set; }

	public static OVRRuntimeSettings runtimeSettings { get; private set; }

	public static OVRProfile profile
	{
		get
		{
			if (_profile == null)
			{
				_profile = new OVRProfile();
			}
			return _profile;
		}
	}

	public static bool isHmdPresent
	{
		get
		{
			if (_isHmdPresentCacheFrame != Time.frameCount)
			{
				_isHmdPresentCacheFrame = Time.frameCount;
				_isHmdPresent = OVRNodeStateProperties.IsHmdPresent();
			}
			return _isHmdPresent;
		}
	}

	public static string audioOutId => OVRPlugin.audioOutId;

	public static string audioInId => OVRPlugin.audioInId;

	public static bool hasVrFocus
	{
		get
		{
			if (!_hasVrFocusCached)
			{
				_hasVrFocusCached = true;
				_hasVrFocus = OVRPlugin.hasVrFocus;
			}
			return _hasVrFocus;
		}
		private set
		{
			_hasVrFocusCached = true;
			_hasVrFocus = value;
		}
	}

	public static bool hasInputFocus => OVRPlugin.hasInputFocus;

	public bool chromatic
	{
		get
		{
			if (!isHmdPresent)
			{
				return false;
			}
			return OVRPlugin.chromatic;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.chromatic = value;
			}
		}
	}

	public bool monoscopic
	{
		get
		{
			if (!isHmdPresent)
			{
				return _monoscopic;
			}
			return OVRPlugin.monoscopic;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.monoscopic = value;
				_monoscopic = value;
			}
		}
	}

	public OVRPlugin.LayerSharpenType sharpenType
	{
		get
		{
			return _sharpenType;
		}
		set
		{
			_sharpenType = value;
			OVRPlugin.SetEyeBufferSharpenType(_sharpenType);
		}
	}

	public ColorSpace colorGamut
	{
		get
		{
			return _colorGamut;
		}
		set
		{
			_colorGamut = value;
			OVRPlugin.SetClientColorDesc((OVRPlugin.ColorSpace)_colorGamut);
		}
	}

	public ColorSpace nativeColorGamut => (ColorSpace)OVRPlugin.GetHmdColorDesc();

	public bool enableDynamicResolution
	{
		get
		{
			return _enableDynamicResolution;
		}
		set
		{
			_enableDynamicResolution = value;
		}
	}

	public Vector3 headPoseRelativeOffsetRotation
	{
		get
		{
			return _headPoseRelativeOffsetRotation;
		}
		set
		{
			if (OVRPlugin.GetHeadPoseModifier(out var relativeRotation, out var relativeTranslation))
			{
				relativeRotation = Quaternion.Euler(value).ToQuatf();
				OVRPlugin.SetHeadPoseModifier(ref relativeRotation, ref relativeTranslation);
			}
			_headPoseRelativeOffsetRotation = value;
		}
	}

	public Vector3 headPoseRelativeOffsetTranslation
	{
		get
		{
			return _headPoseRelativeOffsetTranslation;
		}
		set
		{
			if (OVRPlugin.GetHeadPoseModifier(out var relativeRotation, out var relativeTranslation) && relativeTranslation.FromFlippedZVector3f() != value)
			{
				relativeTranslation = value.ToFlippedZVector3f();
				OVRPlugin.SetHeadPoseModifier(ref relativeRotation, ref relativeTranslation);
			}
			_headPoseRelativeOffsetTranslation = value;
		}
	}

	[HideInInspector]
	public static bool eyeFovPremultipliedAlphaModeEnabled
	{
		get
		{
			return OVRPlugin.eyeFovPremultipliedAlphaModeEnabled;
		}
		set
		{
			OVRPlugin.eyeFovPremultipliedAlphaModeEnabled = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.enableMixedReality
	{
		get
		{
			return enableMixedReality;
		}
		set
		{
			enableMixedReality = value;
		}
	}

	LayerMask OVRMixedRealityCaptureConfiguration.extraHiddenLayers
	{
		get
		{
			return extraHiddenLayers;
		}
		set
		{
			extraHiddenLayers = value;
		}
	}

	LayerMask OVRMixedRealityCaptureConfiguration.extraVisibleLayers
	{
		get
		{
			return extraVisibleLayers;
		}
		set
		{
			extraVisibleLayers = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.dynamicCullingMask
	{
		get
		{
			return dynamicCullingMask;
		}
		set
		{
			dynamicCullingMask = value;
		}
	}

	CompositionMethod OVRMixedRealityCaptureConfiguration.compositionMethod
	{
		get
		{
			return compositionMethod;
		}
		set
		{
			compositionMethod = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.externalCompositionBackdropColorRift
	{
		get
		{
			return externalCompositionBackdropColorRift;
		}
		set
		{
			externalCompositionBackdropColorRift = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.externalCompositionBackdropColorQuest
	{
		get
		{
			return externalCompositionBackdropColorQuest;
		}
		set
		{
			externalCompositionBackdropColorQuest = value;
		}
	}

	[Obsolete("Deprecated", false)]
	CameraDevice OVRMixedRealityCaptureConfiguration.capturingCameraDevice
	{
		get
		{
			return capturingCameraDevice;
		}
		set
		{
			capturingCameraDevice = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.flipCameraFrameHorizontally
	{
		get
		{
			return flipCameraFrameHorizontally;
		}
		set
		{
			flipCameraFrameHorizontally = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.flipCameraFrameVertically
	{
		get
		{
			return flipCameraFrameVertically;
		}
		set
		{
			flipCameraFrameVertically = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.handPoseStateLatency
	{
		get
		{
			return handPoseStateLatency;
		}
		set
		{
			handPoseStateLatency = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.sandwichCompositionRenderLatency
	{
		get
		{
			return sandwichCompositionRenderLatency;
		}
		set
		{
			sandwichCompositionRenderLatency = value;
		}
	}

	int OVRMixedRealityCaptureConfiguration.sandwichCompositionBufferedFrames
	{
		get
		{
			return sandwichCompositionBufferedFrames;
		}
		set
		{
			sandwichCompositionBufferedFrames = value;
		}
	}

	Color OVRMixedRealityCaptureConfiguration.chromaKeyColor
	{
		get
		{
			return chromaKeyColor;
		}
		set
		{
			chromaKeyColor = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySimilarity
	{
		get
		{
			return chromaKeySimilarity;
		}
		set
		{
			chromaKeySimilarity = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySmoothRange
	{
		get
		{
			return chromaKeySmoothRange;
		}
		set
		{
			chromaKeySmoothRange = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.chromaKeySpillRange
	{
		get
		{
			return chromaKeySpillRange;
		}
		set
		{
			chromaKeySpillRange = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.useDynamicLighting
	{
		get
		{
			return useDynamicLighting;
		}
		set
		{
			useDynamicLighting = value;
		}
	}

	[Obsolete("Deprecated", false)]
	DepthQuality OVRMixedRealityCaptureConfiguration.depthQuality
	{
		get
		{
			return depthQuality;
		}
		set
		{
			depthQuality = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.dynamicLightingSmoothFactor
	{
		get
		{
			return dynamicLightingSmoothFactor;
		}
		set
		{
			dynamicLightingSmoothFactor = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.dynamicLightingDepthVariationClampingValue
	{
		get
		{
			return dynamicLightingDepthVariationClampingValue;
		}
		set
		{
			dynamicLightingDepthVariationClampingValue = value;
		}
	}

	[Obsolete("Deprecated", false)]
	VirtualGreenScreenType OVRMixedRealityCaptureConfiguration.virtualGreenScreenType
	{
		get
		{
			return virtualGreenScreenType;
		}
		set
		{
			virtualGreenScreenType = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenTopY
	{
		get
		{
			return virtualGreenScreenTopY;
		}
		set
		{
			virtualGreenScreenTopY = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenBottomY
	{
		get
		{
			return virtualGreenScreenBottomY;
		}
		set
		{
			virtualGreenScreenBottomY = value;
		}
	}

	bool OVRMixedRealityCaptureConfiguration.virtualGreenScreenApplyDepthCulling
	{
		get
		{
			return virtualGreenScreenApplyDepthCulling;
		}
		set
		{
			virtualGreenScreenApplyDepthCulling = value;
		}
	}

	float OVRMixedRealityCaptureConfiguration.virtualGreenScreenDepthTolerance
	{
		get
		{
			return virtualGreenScreenDepthTolerance;
		}
		set
		{
			virtualGreenScreenDepthTolerance = value;
		}
	}

	MrcActivationMode OVRMixedRealityCaptureConfiguration.mrcActivationMode
	{
		get
		{
			return mrcActivationMode;
		}
		set
		{
			mrcActivationMode = value;
		}
	}

	InstantiateMrcCameraDelegate OVRMixedRealityCaptureConfiguration.instantiateMixedRealityCameraGameObject
	{
		get
		{
			return instantiateMixedRealityCameraGameObject;
		}
		set
		{
			instantiateMixedRealityCameraGameObject = value;
		}
	}

	public bool isBoundaryVisibilitySuppressed { get; private set; }

	public XrApi xrApi => (XrApi)OVRPlugin.nativeXrApi;

	public ulong xrInstance => OVRPlugin.GetNativeOpenXRInstance();

	public ulong xrSession => OVRPlugin.GetNativeOpenXRSession();

	public int vsyncCount
	{
		get
		{
			if (!isHmdPresent)
			{
				return 1;
			}
			return OVRPlugin.vsyncCount;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.vsyncCount = value;
			}
		}
	}

	[Obsolete("Deprecated. Please use SystemInfo.batteryLevel", false)]
	public static float batteryLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 1f;
			}
			return OVRPlugin.batteryLevel;
		}
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static float batteryTemperature
	{
		get
		{
			if (!isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.batteryTemperature;
		}
	}

	[Obsolete("Deprecated. Please use SystemInfo.batteryStatus", false)]
	public static int batteryStatus
	{
		get
		{
			if (!isHmdPresent)
			{
				return -1;
			}
			return (int)OVRPlugin.batteryStatus;
		}
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static float volumeLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 0f;
			}
			return OVRPlugin.systemVolume;
		}
	}

	public static ProcessorPerformanceLevel suggestedCpuPerfLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return ProcessorPerformanceLevel.PowerSavings;
			}
			return (ProcessorPerformanceLevel)OVRPlugin.suggestedCpuPerfLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.suggestedCpuPerfLevel = (OVRPlugin.ProcessorPerformanceLevel)value;
			}
		}
	}

	public static ProcessorPerformanceLevel suggestedGpuPerfLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return ProcessorPerformanceLevel.PowerSavings;
			}
			return (ProcessorPerformanceLevel)OVRPlugin.suggestedGpuPerfLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.suggestedGpuPerfLevel = (OVRPlugin.ProcessorPerformanceLevel)value;
			}
		}
	}

	[Obsolete("Deprecated. Please use suggestedCpuPerfLevel", false)]
	public static int cpuLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.cpuLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.cpuLevel = value;
			}
		}
	}

	[Obsolete("Deprecated. Please use suggestedGpuPerfLevel", false)]
	public static int gpuLevel
	{
		get
		{
			if (!isHmdPresent)
			{
				return 2;
			}
			return OVRPlugin.gpuLevel;
		}
		set
		{
			if (isHmdPresent)
			{
				OVRPlugin.gpuLevel = value;
			}
		}
	}

	public static bool isPowerSavingActive
	{
		get
		{
			if (!isHmdPresent)
			{
				return false;
			}
			return OVRPlugin.powerSaving;
		}
	}

	public static EyeTextureFormat eyeTextureFormat
	{
		get
		{
			return (EyeTextureFormat)OVRPlugin.GetDesiredEyeTextureFormat();
		}
		set
		{
			OVRPlugin.SetDesiredEyeTextureFormat((OVRPlugin.EyeTextureFormat)value);
		}
	}

	public static bool eyeTrackedFoveatedRenderingSupported => GetEyeTrackedFoveatedRenderingSupported();

	public static bool eyeTrackedFoveatedRenderingEnabled
	{
		get
		{
			return GetEyeTrackedFoveatedRenderingEnabled();
		}
		set
		{
			if (!eyeTrackedFoveatedRenderingSupported)
			{
				return;
			}
			if (value)
			{
				if (OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.EyeTracking))
				{
					SetEyeTrackedFoveatedRenderingEnabled(value);
				}
			}
			else
			{
				SetEyeTrackedFoveatedRenderingEnabled(value);
			}
		}
	}

	public static FoveatedRenderingLevel foveatedRenderingLevel
	{
		get
		{
			return GetFoveatedRenderingLevel();
		}
		set
		{
			SetFoveatedRenderingLevel(value);
		}
	}

	public static bool fixedFoveatedRenderingSupported => GetFixedFoveatedRenderingSupported();

	[Obsolete("Please use foveatedRenderingLevel instead", false)]
	public static FixedFoveatedRenderingLevel fixedFoveatedRenderingLevel
	{
		get
		{
			return (FixedFoveatedRenderingLevel)OVRPlugin.fixedFoveatedRenderingLevel;
		}
		set
		{
			OVRPlugin.fixedFoveatedRenderingLevel = (OVRPlugin.FixedFoveatedRenderingLevel)value;
		}
	}

	public static bool useDynamicFoveatedRendering
	{
		get
		{
			return GetDynamicFoveatedRenderingEnabled();
		}
		set
		{
			SetDynamicFoveatedRenderingEnabled(value);
		}
	}

	[Obsolete("Please use useDynamicFoveatedRendering instead", false)]
	public static bool useDynamicFixedFoveatedRendering
	{
		get
		{
			return OVRPlugin.useDynamicFixedFoveatedRendering;
		}
		set
		{
			OVRPlugin.useDynamicFixedFoveatedRendering = value;
		}
	}

	[Obsolete("Please use fixedFoveatedRenderingSupported instead", false)]
	public static bool tiledMultiResSupported => OVRPlugin.tiledMultiResSupported;

	[Obsolete("Please use foveatedRenderingLevel instead", false)]
	public static TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			return (TiledMultiResLevel)OVRPlugin.tiledMultiResLevel;
		}
		set
		{
			OVRPlugin.tiledMultiResLevel = (OVRPlugin.TiledMultiResLevel)value;
		}
	}

	public static bool gpuUtilSupported => OVRPlugin.gpuUtilSupported;

	public static float gpuUtilLevel
	{
		get
		{
			if (!OVRPlugin.gpuUtilSupported)
			{
				Debug.LogWarning("GPU Util is not supported");
			}
			return OVRPlugin.gpuUtilLevel;
		}
	}

	public static SystemHeadsetType systemHeadsetType => (SystemHeadsetType)OVRPlugin.GetSystemHeadsetType();

	public static SystemHeadsetTheme systemHeadsetTheme => GetSystemHeadsetTheme();

	public TrackingOrigin trackingOriginType
	{
		get
		{
			if (!isHmdPresent)
			{
				return _trackingOriginType;
			}
			return (TrackingOrigin)OVRPlugin.GetTrackingOriginType();
		}
		set
		{
			if (!isHmdPresent)
			{
				_trackingOriginType = value;
				return;
			}
			if (OVRPlugin.UnityOpenXR.Enabled)
			{
				if (GetCurrentInputSubsystem() == null)
				{
					Debug.LogError("InputSubsystem not found");
					return;
				}
				TrackingOriginModeFlags trackingOriginModeFlags = TrackingOriginModeFlags.Unknown;
				switch (value)
				{
				case TrackingOrigin.EyeLevel:
					trackingOriginModeFlags = TrackingOriginModeFlags.Device;
					break;
				case TrackingOrigin.FloorLevel:
					trackingOriginModeFlags = TrackingOriginModeFlags.Floor;
					OpenXRSettings.SetAllowRecentering(allowRecentering: true);
					break;
				case TrackingOrigin.Stage:
					trackingOriginModeFlags = TrackingOriginModeFlags.Floor;
					OpenXRSettings.SetAllowRecentering(allowRecentering: false);
					break;
				}
				if (trackingOriginModeFlags != 0)
				{
					if (!GetCurrentInputSubsystem().TrySetTrackingOriginMode(trackingOriginModeFlags))
					{
						Debug.LogError($"Unable to set TrackingOrigin {trackingOriginModeFlags} to Unity Input Subsystem");
						return;
					}
					_trackingOriginType = value;
					OpenXRSettings.RefreshRecenterSpace();
					return;
				}
			}
			if (OVRPlugin.SetTrackingOriginType((OVRPlugin.TrackingOrigin)value))
			{
				_trackingOriginType = value;
			}
		}
	}

	public bool IsSimultaneousHandsAndControllersSupported
	{
		get
		{
			if (_readOnlyControllerDrivenHandPosesType == ControllerDrivenHandPosesType.None)
			{
				return launchSimultaneousHandsControllersOnStartup;
			}
			return true;
		}
	}

	public bool isSupportedPlatform { get; private set; }

	public bool isUserPresent
	{
		get
		{
			if (!_isUserPresentCached)
			{
				_isUserPresentCached = true;
				_isUserPresent = OVRPlugin.userPresent;
			}
			return _isUserPresent;
		}
		private set
		{
			_isUserPresentCached = true;
			_isUserPresent = value;
		}
	}

	public static Version utilitiesVersion => OVRPlugin.wrapperVersion;

	public static Version pluginVersion => OVRPlugin.version;

	public static Version sdkVersion => OVRPlugin.nativeSDKVersion;

	public static event Action HMDAcquired;

	public static event Action HMDLost;

	public static event Action HMDMounted;

	public static event Action HMDUnmounted;

	public static event Action VrFocusAcquired;

	public static event Action VrFocusLost;

	public static event Action InputFocusAcquired;

	public static event Action InputFocusLost;

	public static event Action AudioOutChanged;

	public static event Action AudioInChanged;

	public static event Action TrackingAcquired;

	public static event Action TrackingLost;

	public static event Action<float, float> DisplayRefreshRateChanged;

	public static event Action<ulong, bool, OVRSpace, Guid> SpatialAnchorCreateComplete;

	public static event Action<ulong, bool, OVRSpace, Guid, OVRPlugin.SpaceComponentType, bool> SpaceSetComponentStatusComplete;

	public static event Action<ulong> SpaceQueryResults;

	public static event Action<ulong, bool> SpaceQueryComplete;

	public static event Action<ulong, OVRSpace, bool, Guid> SpaceSaveComplete;

	public static event Action<ulong, bool, Guid, OVRPlugin.SpaceStorageLocation> SpaceEraseComplete;

	public static event Action<ulong, OVRSpatialAnchor.OperationResult> ShareSpacesComplete;

	public static event Action<ulong, OVRSpatialAnchor.OperationResult> SpaceListSaveComplete;

	public static event Action<ulong, bool> SceneCaptureComplete;

	public static event Action<int> PassthroughLayerResumed;

	public static event Action<OVRPlugin.BoundaryVisibility> BoundaryVisibilityChanged;

	public static event Action<TrackingOrigin, OVRPose?> TrackingOriginChangePending;

	[Obsolete]
	public static event Action HSWDismissed;

	[Obsolete("Deprecated. Use Dynamic Render Scaling instead.", false)]
	public static bool IsAdaptiveResSupportedByEngine()
	{
		return true;
	}

	protected static void OnPermissionGranted(string permissionId)
	{
		if (permissionId == OVRPermissionsRequester.GetPermissionId(OVRPermissionsRequester.Permission.EyeTracking))
		{
			OVRPermissionsRequester.PermissionGranted -= OnPermissionGranted;
			SetEyeTrackedFoveatedRenderingEnabled(enabled: true);
		}
	}

	private static SystemHeadsetTheme GetSystemHeadsetTheme()
	{
		if (!_isSystemHeadsetThemeCached)
		{
			_isSystemHeadsetThemeCached = true;
		}
		return _cachedSystemHeadsetTheme;
	}

	public static void SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, bool applyToAllLayers)
	{
		SetColorScaleAndOffset_Internal(colorScale, colorOffset, applyToAllLayers);
	}

	public static void SetOpenVRLocalPose(Vector3 leftPos, Vector3 rightPos, Quaternion leftRot, Quaternion rightRot)
	{
		if (loadedXRDevice == XRDevice.OpenVR)
		{
			OVRInput.SetOpenVRLocalPose(leftPos, rightPos, leftRot, rightRot);
		}
	}

	public static OVRPose GetOpenVRControllerOffset(XRNode hand)
	{
		OVRPose identity = OVRPose.identity;
		if ((hand == XRNode.LeftHand || hand == XRNode.RightHand) && loadedXRDevice == XRDevice.OpenVR)
		{
			int num = ((hand != XRNode.LeftHand) ? 1 : 0);
			if (OVRInput.openVRControllerDetails[num].controllerType == OVRInput.OpenVRController.OculusTouch)
			{
				Vector3 vector = ((hand == XRNode.LeftHand) ? OpenVRTouchRotationOffsetEulerLeft : OpenVRTouchRotationOffsetEulerRight);
				identity.orientation = Quaternion.Euler(vector.x, vector.y, vector.z);
				identity.position = ((hand == XRNode.LeftHand) ? OpenVRTouchPositionOffsetLeft : OpenVRTouchPositionOffsetRight);
			}
		}
		return identity;
	}

	public static void SetSpaceWarp(bool enabled)
	{
		Camera camera = FindMainCamera();
		if (enabled)
		{
			if (camera != null)
			{
				PrepareCameraForSpaceWarp(camera);
				m_lastSpaceWarpCamera = new WeakReference<Camera>(camera);
			}
		}
		else
		{
			if (camera != null && m_lastSpaceWarpCamera != null && m_lastSpaceWarpCamera.TryGetTarget(out var target) && target == camera)
			{
				camera.depthTextureMode = m_CachedDepthTextureMode;
			}
			m_AppSpaceTransform = null;
			m_lastSpaceWarpCamera = null;
		}
		SetSpaceWarp_Internal(enabled);
		m_SpaceWarpEnabled = enabled;
	}

	private static void PrepareCameraForSpaceWarp(Camera camera)
	{
		m_CachedDepthTextureMode = camera.depthTextureMode;
		camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
		m_AppSpaceTransform = camera.transform.parent;
	}

	public static bool GetSpaceWarp()
	{
		return m_SpaceWarpEnabled;
	}

	public void RegisterEventListener(EventListener listener)
	{
		eventListeners.Add(listener);
	}

	public void DeregisterEventListener(EventListener listener)
	{
		eventListeners.Remove(listener);
	}

	private static bool MixedRealityEnabledFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-mixedreality")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseDirectCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-directcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool UseExternalCompositionFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-externalcomposition")
			{
				return true;
			}
		}
		return false;
	}

	private static bool CreateMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-create_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	private static bool LoadMixedRealityCaptureConfigurationFileFromCmd()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].ToLower() == "-load_mrc_config")
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsUnityAlphaOrBetaVersion()
	{
		string unityVersion = Application.unityVersion;
		int num = unityVersion.Length - 1;
		while (num >= 0 && unityVersion[num] >= '0' && unityVersion[num] <= '9')
		{
			num--;
		}
		if (num >= 0 && (unityVersion[num] == 'a' || unityVersion[num] == 'b'))
		{
			return true;
		}
		return false;
	}

	private void Reset()
	{
		dynamicResolutionVersion = MaxDynamicResolutionVersion;
	}

	private void InitOVRManager()
	{
		using OVRTelemetryMarker marker = new OVRTelemetryMarker(163069401, 0, -1L);
		marker.AddSDKVersionAnnotation();
		if (instance != null)
		{
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
			marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
			return;
		}
		instance = this;
		runtimeSettings = OVRRuntimeSettings.GetRuntimeSettings();
		string message = "Unity v" + Application.unityVersion + ", Oculus Utilities v" + OVRPlugin.wrapperVersion?.ToString() + ", OVRPlugin v" + OVRPlugin.version?.ToString() + ", SDK v" + OVRPlugin.nativeSDKVersion?.ToString() + ".";
		if (OVRPlugin.version < OVRPlugin.wrapperVersion)
		{
			Debug.LogWarning(message);
			Debug.LogWarning("You are using an old version of OVRPlugin. Some features may not work correctly. You will be prompted to restart the Editor for any OVRPlugin changes.");
		}
		else
		{
			Debug.Log(message);
		}
		Debug.LogFormat("SystemHeadset {0}, API {1}", OVRManager.systemHeadsetType.ToString(), xrApi.ToString());
		if (xrApi == XrApi.OpenXR)
		{
			Debug.LogFormat("OpenXR instance 0x{0:X} session 0x{1:X}", xrInstance, xrSession);
		}
		if (IsUnityAlphaOrBetaVersion())
		{
			Debug.LogWarning(UnityAlphaOrBetaVersionWarningMessage);
		}
		string text = GraphicsDeviceType.Direct3D11.ToString() + ", " + GraphicsDeviceType.Direct3D12;
		if (!text.Contains(SystemInfo.graphicsDeviceType.ToString()))
		{
			Debug.LogWarning("VR rendering requires one of the following device types: (" + text + "). Your graphics device: " + SystemInfo.graphicsDeviceType);
		}
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer)
		{
			isSupportedPlatform = true;
		}
		else
		{
			isSupportedPlatform = false;
		}
		if (!isSupportedPlatform)
		{
			Debug.LogWarning("This platform is unsupported");
			marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
			return;
		}
		enableMixedReality = false;
		if (!staticMixedRealityCaptureInitialized)
		{
			bool flag = LoadMixedRealityCaptureConfigurationFileFromCmd();
			bool flag2 = CreateMixedRealityCaptureConfigurationFileFromCmd();
			if (flag || flag2)
			{
				OVRMixedRealityCaptureSettings oVRMixedRealityCaptureSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
				oVRMixedRealityCaptureSettings.ReadFrom(this);
				if (flag)
				{
					oVRMixedRealityCaptureSettings.CombineWithConfigurationFile();
					oVRMixedRealityCaptureSettings.ApplyTo(this);
				}
				if (flag2)
				{
					oVRMixedRealityCaptureSettings.WriteToConfigurationFile();
				}
				UnityEngine.Object.Destroy(oVRMixedRealityCaptureSettings);
			}
			if (MixedRealityEnabledFromCmd())
			{
				enableMixedReality = true;
			}
			if (enableMixedReality)
			{
				Debug.Log("OVR: Mixed Reality mode enabled");
				if (UseDirectCompositionFromCmd())
				{
					Debug.Log("DirectionComposition deprecated. Fallback to ExternalComposition");
					compositionMethod = CompositionMethod.External;
				}
				if (UseExternalCompositionFromCmd())
				{
					compositionMethod = CompositionMethod.External;
				}
				Debug.Log("OVR: CompositionMethod : " + compositionMethod);
			}
		}
		StaticInitializeMixedRealityCapture(this);
		Initialize();
		InitPermissionRequest();
		marker.AddPoint(OVRTelemetryConstants.OVRManager.InitPermissionRequest);
		Debug.LogFormat("Current display frequency {0}, available frequencies [{1}]", display.displayFrequency, string.Join(", ", display.displayFrequenciesAvailable.Select((float f) => f.ToString()).ToArray()));
		if (resetTrackerOnLoad)
		{
			display.RecenterPose();
		}
		if (Debug.isDebugBuild)
		{
			if (GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>() == null)
			{
				base.gameObject.AddComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
			}
			OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer component = GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
			component.listeningPort = profilerTcpPort;
			if (!component.enabled)
			{
				component.enabled = true;
			}
			OVRPlugin.SetDeveloperMode(OVRPlugin.Bool.True);
		}
		ColorSpace colorSpace = runtimeSettings.colorSpace;
		colorGamut = colorSpace;
		OVRPlugin.SetEyeBufferSharpenType(_sharpenType);
		OVRPlugin.occlusionMesh = true;
		if (!OVRPlugin.SetSimultaneousHandsAndControllersEnabled(launchSimultaneousHandsControllersOnStartup))
		{
			Debug.Log("Failed to set multimodal hands and controllers mode!");
		}
		if (isInsightPassthroughEnabled)
		{
			InitializeInsightPassthrough();
			marker.AddPoint(OVRTelemetryConstants.OVRManager.InitializeInsightPassthrough);
		}
		if (!OVRPlugin.localDimmingSupported)
		{
			Debug.LogWarning("Local Dimming feature is not supported");
			_localDimming = false;
		}
		else
		{
			OVRPlugin.localDimming = _localDimming;
		}
		UpdateDynamicResolutionVersion();
		SystemHeadsetType systemHeadsetType = OVRManager.systemHeadsetType;
		if ((uint)(systemHeadsetType - 9) <= 1u)
		{
			minDynamicResolutionScale = quest2MinDynamicResolutionScale;
			maxDynamicResolutionScale = quest2MaxDynamicResolutionScale;
		}
		else
		{
			minDynamicResolutionScale = quest3MinDynamicResolutionScale;
			maxDynamicResolutionScale = quest3MaxDynamicResolutionScale;
		}
		InitializeBoundary();
		if (OVRPlugin.HandSkeletonVersion != runtimeSettings.HandSkeletonVersion)
		{
			OVRPlugin.SetHandSkeletonVersion(runtimeSettings.HandSkeletonVersion);
		}
		Debug.Log($"[OVRManager] Current hand skeleton version is {OVRPlugin.HandSkeletonVersion}");
		OpenXRSettings openXRSettings = OpenXRSettings.Instance;
		if (openXRSettings != null)
		{
			MetaXRSubsampledLayout feature = openXRSettings.GetFeature<MetaXRSubsampledLayout>();
			MetaXRSpaceWarp feature2 = openXRSettings.GetFeature<MetaXRSpaceWarp>();
			bool flag3 = false;
			if (feature != null)
			{
				flag3 = feature.enabled;
			}
			bool flag4 = false;
			if (feature2 != null)
			{
				flag4 = feature2.enabled;
			}
			Debug.Log($"OpenXR Meta Quest Runtime Settings:\nDepth Submission Mode - {openXRSettings.depthSubmissionMode}\nRendering Mode - {openXRSettings.renderMode}\nOptimize Buffer Discards - {openXRSettings.optimizeBufferDiscards}\nSymmetric Projection - {openXRSettings.symmetricProjection}\nSubsampled Layout - {flag3}\nSpace Warp - {flag4}");
		}
		OVRManagerinitialized = true;
	}

	private void InitPermissionRequest()
	{
		HashSet<OVRPermissionsRequester.Permission> hashSet = new HashSet<OVRPermissionsRequester.Permission>();
		if (requestBodyTrackingPermissionOnStartup)
		{
			hashSet.Add(OVRPermissionsRequester.Permission.BodyTracking);
		}
		if (requestFaceTrackingPermissionOnStartup)
		{
			hashSet.Add(OVRPermissionsRequester.Permission.FaceTracking);
		}
		if (requestEyeTrackingPermissionOnStartup)
		{
			hashSet.Add(OVRPermissionsRequester.Permission.EyeTracking);
		}
		if (requestScenePermissionOnStartup)
		{
			hashSet.Add(OVRPermissionsRequester.Permission.Scene);
		}
		if (requestRecordAudioPermissionOnStartup)
		{
			hashSet.Add(OVRPermissionsRequester.Permission.RecordAudio);
		}
		OVRPermissionsRequester.Request(hashSet);
	}

	private void Awake()
	{
		if (OVRPlugin.initialized)
		{
			InitOVRManager();
		}
	}

	private void SetCurrentXRDevice()
	{
		XRDisplaySubsystem currentDisplaySubsystem = GetCurrentDisplaySubsystem();
		XRDisplaySubsystemDescriptor currentDisplaySubsystemDescriptor = GetCurrentDisplaySubsystemDescriptor();
		if (OVRPlugin.initialized)
		{
			loadedXRDevice = XRDevice.Oculus;
		}
		else if (currentDisplaySubsystem != null && currentDisplaySubsystemDescriptor != null && currentDisplaySubsystem.running)
		{
			if (currentDisplaySubsystemDescriptor.id == OPENVR_UNITY_NAME_STR)
			{
				loadedXRDevice = XRDevice.OpenVR;
			}
			else
			{
				loadedXRDevice = XRDevice.Unknown;
			}
		}
		else
		{
			loadedXRDevice = XRDevice.Unknown;
		}
	}

	public static XRDisplaySubsystem GetCurrentDisplaySubsystem()
	{
		if (s_displaySubsystems == null)
		{
			s_displaySubsystems = new List<XRDisplaySubsystem>();
		}
		SubsystemManager.GetSubsystems(s_displaySubsystems);
		if (s_displaySubsystems.Count > 0)
		{
			return s_displaySubsystems[0];
		}
		return null;
	}

	public static XRDisplaySubsystemDescriptor GetCurrentDisplaySubsystemDescriptor()
	{
		if (s_displaySubsystemDescriptors == null)
		{
			s_displaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
		}
		SubsystemManager.GetSubsystemDescriptors(s_displaySubsystemDescriptors);
		if (s_displaySubsystemDescriptors.Count > 0)
		{
			return s_displaySubsystemDescriptors[0];
		}
		return null;
	}

	public static XRInputSubsystem GetCurrentInputSubsystem()
	{
		if (s_inputSubsystems == null)
		{
			s_inputSubsystems = new List<XRInputSubsystem>();
		}
		SubsystemManager.GetSubsystems(s_inputSubsystems);
		if (s_inputSubsystems.Count > 0)
		{
			return s_inputSubsystems[0];
		}
		return null;
	}

	private void Initialize()
	{
		if (display == null)
		{
			display = new OVRDisplay();
		}
		if (tracker == null)
		{
			tracker = new OVRTracker();
		}
		if (boundary == null)
		{
			boundary = new OVRBoundary();
		}
		SetCurrentXRDevice();
	}

	private void Update()
	{
		if (!OVRManagerinitialized)
		{
			XRDisplaySubsystem currentDisplaySubsystem = GetCurrentDisplaySubsystem();
			XRDisplaySubsystemDescriptor currentDisplaySubsystemDescriptor = GetCurrentDisplaySubsystemDescriptor();
			if (currentDisplaySubsystem == null || currentDisplaySubsystemDescriptor == null || !OVRPlugin.initialized)
			{
				return;
			}
			InitOVRManager();
		}
		SetCurrentXRDevice();
		if (OVRPlugin.shouldQuit)
		{
			Debug.Log("[OVRManager] OVRPlugin.shouldQuit detected");
			StaticShutdownMixedRealityCapture(instance);
			ShutdownInsightPassthrough();
			Application.Quit();
		}
		if (AllowRecenter && OVRPlugin.shouldRecenter)
		{
			display.RecenterPose();
		}
		if (trackingOriginType != _trackingOriginType)
		{
			trackingOriginType = _trackingOriginType;
		}
		tracker.isEnabled = usePositionTracking;
		OVRPlugin.rotation = useRotationTracking;
		OVRPlugin.useIPDInPositionTracking = useIPDInPositionTracking;
		if (monoscopic != _monoscopic)
		{
			monoscopic = _monoscopic;
		}
		if (headPoseRelativeOffsetRotation != _headPoseRelativeOffsetRotation)
		{
			headPoseRelativeOffsetRotation = _headPoseRelativeOffsetRotation;
		}
		if (headPoseRelativeOffsetTranslation != _headPoseRelativeOffsetTranslation)
		{
			headPoseRelativeOffsetTranslation = _headPoseRelativeOffsetTranslation;
		}
		if (_wasHmdPresent && !isHmdPresent)
		{
			try
			{
				Debug.Log("[OVRManager] HMDLost event");
				if (OVRManager.HMDLost != null)
				{
					OVRManager.HMDLost();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Caught Exception: " + ex);
			}
		}
		if (!_wasHmdPresent && isHmdPresent)
		{
			try
			{
				Debug.Log("[OVRManager] HMDAcquired event");
				if (OVRManager.HMDAcquired != null)
				{
					OVRManager.HMDAcquired();
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError("Caught Exception: " + ex2);
			}
		}
		_wasHmdPresent = isHmdPresent;
		isUserPresent = OVRPlugin.userPresent;
		if (_wasUserPresent && !isUserPresent)
		{
			try
			{
				Debug.Log("[OVRManager] HMDUnmounted event");
				if (OVRManager.HMDUnmounted != null)
				{
					OVRManager.HMDUnmounted();
				}
			}
			catch (Exception ex3)
			{
				Debug.LogError("Caught Exception: " + ex3);
			}
		}
		if (!_wasUserPresent && isUserPresent)
		{
			try
			{
				Debug.Log("[OVRManager] HMDMounted event");
				if (OVRManager.HMDMounted != null)
				{
					OVRManager.HMDMounted();
				}
			}
			catch (Exception ex4)
			{
				Debug.LogError("Caught Exception: " + ex4);
			}
		}
		_wasUserPresent = isUserPresent;
		hasVrFocus = OVRPlugin.hasVrFocus;
		if (_hadVrFocus && !hasVrFocus)
		{
			try
			{
				Debug.Log("[OVRManager] VrFocusLost event");
				if (OVRManager.VrFocusLost != null)
				{
					OVRManager.VrFocusLost();
				}
			}
			catch (Exception ex5)
			{
				Debug.LogError("Caught Exception: " + ex5);
			}
		}
		if (!_hadVrFocus && hasVrFocus)
		{
			try
			{
				Debug.Log("[OVRManager] VrFocusAcquired event");
				if (OVRManager.VrFocusAcquired != null)
				{
					OVRManager.VrFocusAcquired();
				}
			}
			catch (Exception ex6)
			{
				Debug.LogError("Caught Exception: " + ex6);
			}
		}
		_hadVrFocus = hasVrFocus;
		bool flag = OVRPlugin.hasInputFocus;
		if (_hadInputFocus && !flag)
		{
			try
			{
				Debug.Log("[OVRManager] InputFocusLost event");
				if (OVRManager.InputFocusLost != null)
				{
					OVRManager.InputFocusLost();
				}
			}
			catch (Exception ex7)
			{
				Debug.LogError("Caught Exception: " + ex7);
			}
		}
		if (!_hadInputFocus && flag)
		{
			try
			{
				Debug.Log("[OVRManager] InputFocusAcquired event");
				if (OVRManager.InputFocusAcquired != null)
				{
					OVRManager.InputFocusAcquired();
				}
			}
			catch (Exception ex8)
			{
				Debug.LogError("Caught Exception: " + ex8);
			}
		}
		_hadInputFocus = flag;
		string text = OVRPlugin.audioOutId;
		if (!prevAudioOutIdIsCached)
		{
			prevAudioOutId = text;
			prevAudioOutIdIsCached = true;
		}
		else if (text != prevAudioOutId)
		{
			try
			{
				Debug.Log("[OVRManager] AudioOutChanged event");
				if (OVRManager.AudioOutChanged != null)
				{
					OVRManager.AudioOutChanged();
				}
			}
			catch (Exception ex9)
			{
				Debug.LogError("Caught Exception: " + ex9);
			}
			prevAudioOutId = text;
		}
		string text2 = OVRPlugin.audioInId;
		if (!prevAudioInIdIsCached)
		{
			prevAudioInId = text2;
			prevAudioInIdIsCached = true;
		}
		else if (text2 != prevAudioInId)
		{
			try
			{
				Debug.Log("[OVRManager] AudioInChanged event");
				if (OVRManager.AudioInChanged != null)
				{
					OVRManager.AudioInChanged();
				}
			}
			catch (Exception ex10)
			{
				Debug.LogError("Caught Exception: " + ex10);
			}
			prevAudioInId = text2;
		}
		if (wasPositionTracked && !tracker.isPositionTracked)
		{
			try
			{
				Debug.Log("[OVRManager] TrackingLost event");
				if (OVRManager.TrackingLost != null)
				{
					OVRManager.TrackingLost();
				}
			}
			catch (Exception ex11)
			{
				Debug.LogError("Caught Exception: " + ex11);
			}
		}
		if (!wasPositionTracked && tracker.isPositionTracked)
		{
			try
			{
				Debug.Log("[OVRManager] TrackingAcquired event");
				if (OVRManager.TrackingAcquired != null)
				{
					OVRManager.TrackingAcquired();
				}
			}
			catch (Exception ex12)
			{
				Debug.LogError("Caught Exception: " + ex12);
			}
		}
		wasPositionTracked = tracker.isPositionTracked;
		display.Update();
		if (_readOnlyControllerDrivenHandPosesType != controllerDrivenHandPosesType)
		{
			_readOnlyControllerDrivenHandPosesType = controllerDrivenHandPosesType;
			switch (_readOnlyControllerDrivenHandPosesType)
			{
			case ControllerDrivenHandPosesType.None:
				OVRPlugin.SetControllerDrivenHandPoses(controllerDrivenHandPoses: false);
				OVRPlugin.SetControllerDrivenHandPosesAreNatural(controllerDrivenHandPosesAreNatural: false);
				break;
			case ControllerDrivenHandPosesType.ConformingToController:
				OVRPlugin.SetControllerDrivenHandPoses(controllerDrivenHandPoses: true);
				OVRPlugin.SetControllerDrivenHandPosesAreNatural(controllerDrivenHandPosesAreNatural: false);
				break;
			case ControllerDrivenHandPosesType.Natural:
				OVRPlugin.SetControllerDrivenHandPoses(controllerDrivenHandPoses: true);
				OVRPlugin.SetControllerDrivenHandPosesAreNatural(controllerDrivenHandPosesAreNatural: true);
				break;
			}
		}
		if (_readOnlyWideMotionModeHandPosesEnabled != wideMotionModeHandPosesEnabled)
		{
			_readOnlyWideMotionModeHandPosesEnabled = wideMotionModeHandPosesEnabled;
			OVRPlugin.SetWideMotionModeHandPoses(_readOnlyWideMotionModeHandPosesEnabled);
		}
		OVRInput.Update();
		UpdateHMDEvents();
		StaticUpdateMixedRealityCapture(this, base.gameObject, trackingOriginType);
		UpdateInsightPassthrough(isInsightPassthroughEnabled);
		UpdateBoundary();
	}

	private unsafe void UpdateHMDEvents()
	{
		while (OVRPlugin.PollEvent(ref eventDataBuffer))
		{
			switch (eventDataBuffer.EventType)
			{
			case OVRPlugin.EventType.DisplayRefreshRateChanged:
				if (OVRManager.DisplayRefreshRateChanged != null)
				{
					OVRDeserialize.DisplayRefreshRateChangedData displayRefreshRateChangedData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.DisplayRefreshRateChangedData>(eventDataBuffer.EventData);
					OVRManager.DisplayRefreshRateChanged(displayRefreshRateChangedData.FromRefreshRate, displayRefreshRateChangedData.ToRefreshRate);
				}
				continue;
			case OVRPlugin.EventType.SpatialAnchorCreateComplete:
			{
				OVRDeserialize.SpatialAnchorCreateCompleteData spatialAnchorCreateCompleteData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpatialAnchorCreateCompleteData>(eventDataBuffer.EventData);
				OVRTask.SetResult(spatialAnchorCreateCompleteData.RequestId, (spatialAnchorCreateCompleteData.Result >= 0) ? new OVRAnchor(spatialAnchorCreateCompleteData.Space, spatialAnchorCreateCompleteData.Uuid) : OVRAnchor.Null);
				OVRManager.SpatialAnchorCreateComplete?.Invoke(spatialAnchorCreateCompleteData.RequestId, spatialAnchorCreateCompleteData.Result >= 0, spatialAnchorCreateCompleteData.Space, spatialAnchorCreateCompleteData.Uuid);
				continue;
			}
			case OVRPlugin.EventType.SpaceSetComponentStatusComplete:
			{
				OVRDeserialize.SpaceSetComponentStatusCompleteData eventData3 = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceSetComponentStatusCompleteData>(eventDataBuffer.EventData);
				OVRManager.SpaceSetComponentStatusComplete?.Invoke(eventData3.RequestId, eventData3.Result >= 0, eventData3.Space, eventData3.Uuid, eventData3.ComponentType, eventData3.Enabled != 0);
				OVRTask.SetResult(eventData3.RequestId, eventData3.Result >= 0);
				OVRAnchor.OnSpaceSetComponentStatusComplete(eventData3);
				continue;
			}
			case OVRPlugin.EventType.SpaceQueryResults:
				if (OVRManager.SpaceQueryResults != null)
				{
					OVRDeserialize.SpaceQueryResultsData spaceQueryResultsData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceQueryResultsData>(eventDataBuffer.EventData);
					OVRManager.SpaceQueryResults(spaceQueryResultsData.RequestId);
				}
				continue;
			case OVRPlugin.EventType.SpaceQueryComplete:
			{
				OVRDeserialize.SpaceQueryCompleteData data = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceQueryCompleteData>(eventDataBuffer.EventData);
				OVRManager.SpaceQueryComplete?.Invoke(data.RequestId, data.Result >= 0);
				OVRAnchor.OnSpaceQueryComplete(data);
				continue;
			}
			case OVRPlugin.EventType.SpaceSaveComplete:
				if (OVRManager.SpaceSaveComplete != null)
				{
					OVRDeserialize.SpaceSaveCompleteData spaceSaveCompleteData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceSaveCompleteData>(eventDataBuffer.EventData);
					OVRManager.SpaceSaveComplete(spaceSaveCompleteData.RequestId, spaceSaveCompleteData.Space, spaceSaveCompleteData.Result >= 0, spaceSaveCompleteData.Uuid);
				}
				continue;
			case OVRPlugin.EventType.SpaceEraseComplete:
			{
				OVRDeserialize.SpaceEraseCompleteData eventData2 = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceEraseCompleteData>(eventDataBuffer.EventData);
				bool flag = eventData2.Result >= 0;
				OVRAnchor.OnSpaceEraseComplete(eventData2);
				OVRManager.SpaceEraseComplete?.Invoke(eventData2.RequestId, flag, eventData2.Uuid, eventData2.Location);
				OVRTask.SetResult(eventData2.RequestId, flag);
				continue;
			}
			case OVRPlugin.EventType.SpaceShareResult:
			{
				OVRDeserialize.SpaceShareResultData spaceShareResultData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceShareResultData>(eventDataBuffer.EventData);
				OVRTask.SetResult(spaceShareResultData.RequestId, OVRResult.From((OVRAnchor.ShareResult)spaceShareResultData.Result));
				OVRManager.ShareSpacesComplete?.Invoke(spaceShareResultData.RequestId, (OVRSpatialAnchor.OperationResult)spaceShareResultData.Result);
				continue;
			}
			case OVRPlugin.EventType.SpaceListSaveResult:
			{
				OVRDeserialize.SpaceListSaveResultData eventData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceListSaveResultData>(eventDataBuffer.EventData);
				OVRAnchor.OnSpaceListSaveResult(eventData);
				OVRManager.SpaceListSaveComplete?.Invoke(eventData.RequestId, (OVRSpatialAnchor.OperationResult)eventData.Result);
				continue;
			}
			case OVRPlugin.EventType.SpaceShareToGroupsComplete:
			{
				OVRDeserialize.ShareSpacesToGroupsCompleteData shareSpacesToGroupsCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.ShareSpacesToGroupsCompleteData>();
				OVRAnchor.OnShareAnchorsToGroupsComplete(shareSpacesToGroupsCompleteData.RequestId, shareSpacesToGroupsCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.SceneCaptureComplete:
			{
				OVRDeserialize.SceneCaptureCompleteData sceneCaptureCompleteData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SceneCaptureCompleteData>(eventDataBuffer.EventData);
				OVRManager.SceneCaptureComplete?.Invoke(sceneCaptureCompleteData.RequestId, sceneCaptureCompleteData.Result >= 0);
				OVRTask.SetResult(sceneCaptureCompleteData.RequestId, sceneCaptureCompleteData.Result >= 0);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionStartAdvertisementComplete:
			{
				OVRDeserialize.StartColocationSessionAdvertisementCompleteData startColocationSessionAdvertisementCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.StartColocationSessionAdvertisementCompleteData>();
				OVRColocationSession.OnColocationSessionStartAdvertisementComplete(startColocationSessionAdvertisementCompleteData.RequestId, startColocationSessionAdvertisementCompleteData.Result, startColocationSessionAdvertisementCompleteData.AdvertisementUuid);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionStopAdvertisementComplete:
			{
				OVRDeserialize.StopColocationSessionAdvertisementCompleteData stopColocationSessionAdvertisementCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.StopColocationSessionAdvertisementCompleteData>();
				OVRColocationSession.OnColocationSessionStopAdvertisementComplete(stopColocationSessionAdvertisementCompleteData.RequestId, stopColocationSessionAdvertisementCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionStartDiscoveryComplete:
			{
				OVRDeserialize.StartColocationSessionDiscoveryCompleteData startColocationSessionDiscoveryCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.StartColocationSessionDiscoveryCompleteData>();
				OVRColocationSession.OnColocationSessionStartDiscoveryComplete(startColocationSessionDiscoveryCompleteData.RequestId, startColocationSessionDiscoveryCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionStopDiscoveryComplete:
			{
				OVRDeserialize.StopColocationSessionDiscoveryCompleteData stopColocationSessionDiscoveryCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.StopColocationSessionDiscoveryCompleteData>();
				OVRColocationSession.OnColocationSessionStopDiscoveryComplete(stopColocationSessionDiscoveryCompleteData.RequestId, stopColocationSessionDiscoveryCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionDiscoveryResult:
			{
				OVRDeserialize.ColocationSessionDiscoveryResultData colocationSessionDiscoveryResultData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.ColocationSessionDiscoveryResultData>();
				OVRColocationSession.OnColocationSessionDiscoveryResult(colocationSessionDiscoveryResultData.RequestId, colocationSessionDiscoveryResultData.AdvertisementUuid, colocationSessionDiscoveryResultData.AdvertisementMetadataCount, colocationSessionDiscoveryResultData.AdvertisementMetadata);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionAdvertisementComplete:
			{
				OVRDeserialize.ColocationSessionAdvertisementCompleteData colocationSessionAdvertisementCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.ColocationSessionAdvertisementCompleteData>();
				OVRColocationSession.OnColocationSessionAdvertisementComplete(colocationSessionAdvertisementCompleteData.RequestId, colocationSessionAdvertisementCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.ColocationSessionDiscoveryComplete:
			{
				OVRDeserialize.ColocationSessionDiscoveryCompleteData colocationSessionDiscoveryCompleteData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.ColocationSessionDiscoveryCompleteData>();
				OVRColocationSession.OnColocationSessionDiscoveryComplete(colocationSessionDiscoveryCompleteData.RequestId, colocationSessionDiscoveryCompleteData.Result);
				continue;
			}
			case OVRPlugin.EventType.SpaceDiscoveryComplete:
				OVRAnchor.OnSpaceDiscoveryComplete(OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceDiscoveryCompleteData>(eventDataBuffer.EventData));
				continue;
			case OVRPlugin.EventType.SpaceDiscoveryResultsAvailable:
				OVRAnchor.OnSpaceDiscoveryResultsAvailable(OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpaceDiscoveryResultsData>(eventDataBuffer.EventData));
				continue;
			case OVRPlugin.EventType.SpacesSaveResult:
			{
				OVRDeserialize.SpacesSaveResultData eventData5 = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpacesSaveResultData>(eventDataBuffer.EventData);
				OVRAnchor.OnSaveSpacesResult(eventData5);
				OVRTask.SetResult(eventData5.RequestId, OVRResult.From(eventData5.Result));
				continue;
			}
			case OVRPlugin.EventType.SpacesEraseResult:
			{
				OVRDeserialize.SpacesEraseResultData eventData4 = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.SpacesEraseResultData>(eventDataBuffer.EventData);
				OVRAnchor.OnEraseSpacesResult(eventData4);
				OVRTask.SetResult(eventData4.RequestId, OVRResult.From(eventData4.Result));
				continue;
			}
			case OVRPlugin.EventType.PassthroughLayerResumed:
				if (OVRManager.PassthroughLayerResumed != null)
				{
					OVRDeserialize.PassthroughLayerResumedData passthroughLayerResumedData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.PassthroughLayerResumedData>(eventDataBuffer.EventData);
					OVRManager.PassthroughLayerResumed(passthroughLayerResumedData.LayerId);
				}
				continue;
			case OVRPlugin.EventType.BoundaryVisibilityChanged:
			{
				OVRDeserialize.BoundaryVisibilityChangedData boundaryVisibilityChangedData = OVRDeserialize.ByteArrayToStructure<OVRDeserialize.BoundaryVisibilityChangedData>(eventDataBuffer.EventData);
				OVRManager.BoundaryVisibilityChanged?.Invoke(boundaryVisibilityChangedData.BoundaryVisibility);
				isBoundaryVisibilitySuppressed = boundaryVisibilityChangedData.BoundaryVisibility == OVRPlugin.BoundaryVisibility.Suppressed;
				continue;
			}
			case OVRPlugin.EventType.CreateDynamicObjectTrackerResult:
			{
				OVRDeserialize.CreateDynamicObjectTrackerResultData createDynamicObjectTrackerResultData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.CreateDynamicObjectTrackerResultData>();
				OVRTask.SetResult(OVRTask.GetId(createDynamicObjectTrackerResultData.Tracker, createDynamicObjectTrackerResultData.EventType), OVRResult<ulong, OVRPlugin.Result>.From(createDynamicObjectTrackerResultData.Tracker, createDynamicObjectTrackerResultData.Result));
				continue;
			}
			case OVRPlugin.EventType.SetDynamicObjectTrackedClassesResult:
			{
				OVRDeserialize.SetDynamicObjectTrackedClassesResultData setDynamicObjectTrackedClassesResultData = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.SetDynamicObjectTrackedClassesResultData>();
				OVRTask.SetResult(OVRTask.GetId(setDynamicObjectTrackedClassesResultData.Tracker, setDynamicObjectTrackedClassesResultData.EventType), OVRResult<OVRPlugin.Result>.From(setDynamicObjectTrackedClassesResultData.Result));
				continue;
			}
			case OVRPlugin.EventType.ReferenceSpaceChangePending:
			{
				OVRDeserialize.EventDataReferenceSpaceChangePending eventDataReferenceSpaceChangePending = eventDataBuffer.MarshalEntireStructAs<OVRDeserialize.EventDataReferenceSpaceChangePending>();
				OVRManager.TrackingOriginChangePending?.Invoke((TrackingOrigin)eventDataReferenceSpaceChangePending.ReferenceSpaceType, (eventDataReferenceSpaceChangePending.PoseValid == OVRPlugin.Bool.True) ? new OVRPose?(eventDataReferenceSpaceChangePending.PoseInPreviousSpace.ToOVRPose()) : null);
				continue;
			}
			}
			foreach (EventListener eventListener in eventListeners)
			{
				eventListener.OnEvent(eventDataBuffer);
			}
		}
	}

	public void UpdateDynamicResolutionVersion()
	{
		if (dynamicResolutionVersion == 0)
		{
			quest2MinDynamicResolutionScale = minDynamicResolutionScale;
			quest2MaxDynamicResolutionScale = maxDynamicResolutionScale;
			quest3MinDynamicResolutionScale = minDynamicResolutionScale;
			quest3MaxDynamicResolutionScale = maxDynamicResolutionScale;
		}
		dynamicResolutionVersion = MaxDynamicResolutionVersion;
	}

	public static Camera FindMainCamera()
	{
		if (lastFoundMainCamera != null && lastFoundMainCamera.TryGetTarget(out var target) && target != null && target.isActiveAndEnabled && target.CompareTag("MainCamera"))
		{
			return target;
		}
		Camera camera = null;
		GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
		List<Camera> list = new List<Camera>(4);
		GameObject[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Camera component = array2[i].GetComponent<Camera>();
			if (component != null && component.enabled)
			{
				OVRCameraRig componentInParent = component.GetComponentInParent<OVRCameraRig>();
				if (componentInParent != null && componentInParent.trackingSpace != null)
				{
					list.Add(component);
				}
			}
		}
		if (list.Count == 0)
		{
			camera = Camera.main;
		}
		else if (list.Count == 1)
		{
			camera = list[0];
		}
		else
		{
			if (!multipleMainCameraWarningPresented)
			{
				Debug.LogWarning("Multiple MainCamera found. Assume the real MainCamera is the camera with the least depth");
				multipleMainCameraWarningPresented = true;
			}
			list.Sort((Camera c0, Camera c1) => (!(c0.depth < c1.depth)) ? ((c0.depth > c1.depth) ? 1 : 0) : (-1));
			camera = list[0];
		}
		if (camera != null)
		{
			suppressUnableToFindMainCameraMessage = false;
		}
		else if (!suppressUnableToFindMainCameraMessage)
		{
			Debug.Log("[OVRManager] unable to find a valid camera");
			suppressUnableToFindMainCameraMessage = true;
		}
		lastFoundMainCamera = new WeakReference<Camera>(camera);
		return camera;
	}

	private void OnDisable()
	{
		OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer component = GetComponent<OVRSystemPerfMetrics.OVRSystemPerfMetricsTcpServer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void LateUpdate()
	{
		OVRHaptics.Process();
		if (!m_SpaceWarpEnabled)
		{
			return;
		}
		Camera camera = FindMainCamera();
		if (camera != null)
		{
			Camera target = null;
			if (m_lastSpaceWarpCamera != null)
			{
				m_lastSpaceWarpCamera.TryGetTarget(out target);
			}
			if (camera != target)
			{
				Debug.Log("Main camera changed. Updating new camera for space warp.");
				PrepareCameraForSpaceWarp(camera);
				m_lastSpaceWarpCamera = new WeakReference<Camera>(camera);
			}
			Vector3 position = m_AppSpaceTransform.position;
			Quaternion rotation = m_AppSpaceTransform.rotation;
			Vector3 lossyScale = m_AppSpaceTransform.lossyScale;
			SetAppSpacePosition(position.x / lossyScale.x, position.y / lossyScale.y, position.z / lossyScale.z);
			SetAppSpaceRotation(rotation.x, rotation.y, rotation.z, rotation.w);
		}
		else
		{
			SetAppSpacePosition(0f, 0f, 0f);
			SetAppSpaceRotation(0f, 0f, 0f, 1f);
		}
	}

	private void FixedUpdate()
	{
		OVRInput.FixedUpdate();
	}

	private void OnDestroy()
	{
		Debug.Log("[OVRManager] OnDestroy");
		OVRManagerinitialized = false;
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			Debug.Log("[OVRManager] OnApplicationPause(true)");
		}
		else
		{
			Debug.Log("[OVRManager] OnApplicationPause(false)");
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			Debug.Log("[OVRManager] OnApplicationFocus(true)");
		}
		else
		{
			Debug.Log("[OVRManager] OnApplicationFocus(false)");
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("[OVRManager] OnApplicationQuit");
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public void ReturnToLauncher()
	{
		PlatformUIConfirmQuit();
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static void PlatformUIConfirmQuit()
	{
		if (isHmdPresent)
		{
			OVRPlugin.ShowUI(OVRPlugin.PlatformUI.ConfirmQuit);
		}
	}

	public static void StaticInitializeMixedRealityCapture(OVRMixedRealityCaptureConfiguration configuration)
	{
		if (!staticMixedRealityCaptureInitialized)
		{
			staticMrcSettings = ScriptableObject.CreateInstance<OVRMixedRealityCaptureSettings>();
			staticMrcSettings.ReadFrom(configuration);
			staticPrevEnableMixedRealityCapture = false;
			staticMixedRealityCaptureInitialized = true;
		}
		else
		{
			staticMrcSettings.ApplyTo(configuration);
		}
	}

	public static void StaticUpdateMixedRealityCapture(OVRMixedRealityCaptureConfiguration configuration, GameObject gameObject, TrackingOrigin trackingOrigin)
	{
		if (!staticMixedRealityCaptureInitialized)
		{
			return;
		}
		if (configuration.enableMixedReality)
		{
			Camera camera = FindMainCamera();
			if (camera != null)
			{
				if (!staticPrevEnableMixedRealityCapture)
				{
					OVRPlugin.SendEvent("mixed_reality_capture", "activated");
					Debug.Log("MixedRealityCapture: activate");
					staticPrevEnableMixedRealityCapture = true;
				}
				OVRMixedReality.Update(gameObject, camera, configuration, trackingOrigin);
				suppressDisableMixedRealityBecauseOfNoMainCameraWarning = false;
			}
			else if (!suppressDisableMixedRealityBecauseOfNoMainCameraWarning)
			{
				Debug.LogWarning("Main Camera is not set, Mixed Reality disabled");
				suppressDisableMixedRealityBecauseOfNoMainCameraWarning = true;
			}
		}
		else if (staticPrevEnableMixedRealityCapture)
		{
			Debug.Log("MixedRealityCapture: deactivate");
			staticPrevEnableMixedRealityCapture = false;
			OVRMixedReality.Cleanup();
		}
		staticMrcSettings.ReadFrom(configuration);
	}

	public static void StaticShutdownMixedRealityCapture(OVRMixedRealityCaptureConfiguration configuration)
	{
		if (staticMixedRealityCaptureInitialized)
		{
			UnityEngine.Object.Destroy(staticMrcSettings);
			staticMrcSettings = null;
			OVRMixedReality.Cleanup();
			staticMixedRealityCaptureInitialized = false;
		}
	}

	private static bool PassthroughInitializedOrPending(PassthroughInitializationState state)
	{
		if (state != PassthroughInitializationState.Pending)
		{
			return state == PassthroughInitializationState.Initialized;
		}
		return true;
	}

	private static bool InitializeInsightPassthrough()
	{
		if (PassthroughInitializedOrPending(_passthroughInitializationState.Value))
		{
			return false;
		}
		OVRPlugin.InitializeInsightPassthrough();
		OVRPlugin.Result insightPassthroughInitializationState = OVRPlugin.GetInsightPassthroughInitializationState();
		if (insightPassthroughInitializationState < OVRPlugin.Result.Success)
		{
			_passthroughInitializationState.Value = PassthroughInitializationState.Failed;
			Debug.LogError("Failed to initialize Insight Passthrough. Passthrough will be unavailable. Error " + insightPassthroughInitializationState.ToString() + ".");
		}
		else if (insightPassthroughInitializationState == OVRPlugin.Result.Success_Pending)
		{
			_passthroughInitializationState.Value = PassthroughInitializationState.Pending;
		}
		else
		{
			_passthroughInitializationState.Value = PassthroughInitializationState.Initialized;
		}
		return PassthroughInitializedOrPending(_passthroughInitializationState.Value);
	}

	private static void ShutdownInsightPassthrough()
	{
		if (PassthroughInitializedOrPending(_passthroughInitializationState.Value))
		{
			if (OVRPlugin.ShutdownInsightPassthrough())
			{
				_passthroughInitializationState.Value = PassthroughInitializationState.Unspecified;
			}
			else if (OVRPlugin.IsInsightPassthroughInitialized())
			{
				Debug.LogError("Failed to shut down passthrough. It may be still in use.");
			}
			else
			{
				_passthroughInitializationState.Value = PassthroughInitializationState.Unspecified;
			}
		}
		else
		{
			_passthroughInitializationState.Value = PassthroughInitializationState.Unspecified;
		}
	}

	private static void UpdateInsightPassthrough(bool shouldBeEnabled)
	{
		if (shouldBeEnabled != PassthroughInitializedOrPending(_passthroughInitializationState.Value))
		{
			if (shouldBeEnabled)
			{
				if (_passthroughInitializationState.Value != PassthroughInitializationState.Failed)
				{
					InitializeInsightPassthrough();
				}
			}
			else
			{
				ShutdownInsightPassthrough();
			}
		}
		else if (_passthroughInitializationState.Value == PassthroughInitializationState.Pending)
		{
			OVRPlugin.Result insightPassthroughInitializationState = OVRPlugin.GetInsightPassthroughInitializationState();
			if (insightPassthroughInitializationState == OVRPlugin.Result.Success)
			{
				_passthroughInitializationState.Value = PassthroughInitializationState.Initialized;
			}
			else if (insightPassthroughInitializationState < OVRPlugin.Result.Success)
			{
				_passthroughInitializationState.Value = PassthroughInitializationState.Failed;
				Debug.LogError("Failed to initialize Insight Passthrough. Passthrough will be unavailable. Error " + insightPassthroughInitializationState.ToString() + ".");
			}
		}
	}

	private void InitializeBoundary()
	{
		OVRPlugin.BoundaryVisibility boundaryVisibility;
		switch (OVRPlugin.GetBoundaryVisibility(out boundaryVisibility))
		{
		case OVRPlugin.Result.Success:
			isBoundaryVisibilitySuppressed = boundaryVisibility == OVRPlugin.BoundaryVisibility.Suppressed;
			break;
		case OVRPlugin.Result.Failure_NotYetImplemented:
		case OVRPlugin.Result.Failure_Unsupported:
			isBoundaryVisibilitySuppressed = false;
			shouldBoundaryVisibilityBeSuppressed = false;
			break;
		default:
			Debug.LogWarning("Could not retrieve initial boundary visibility state. Defaulting to not suppressed.");
			isBoundaryVisibilitySuppressed = false;
			break;
		}
	}

	private void UpdateBoundary()
	{
		if (shouldBoundaryVisibilityBeSuppressed == isBoundaryVisibilitySuppressed || !PassthroughInitializedOrPending(_passthroughInitializationState.Value) || !isInsightPassthroughEnabled)
		{
			return;
		}
		switch (OVRPlugin.RequestBoundaryVisibility((!shouldBoundaryVisibilityBeSuppressed) ? OVRPlugin.BoundaryVisibility.NotSuppressed : OVRPlugin.BoundaryVisibility.Suppressed))
		{
		case OVRPlugin.Result.Warning_BoundaryVisibilitySuppressionNotAllowed:
			if (!_updateBoundaryLogOnce)
			{
				_updateBoundaryLogOnce = true;
				Debug.LogWarning("Cannot suppress boundary visibility as it's required to be on.");
			}
			break;
		case OVRPlugin.Result.Success:
			_updateBoundaryLogOnce = false;
			isBoundaryVisibilitySuppressed = shouldBoundaryVisibilityBeSuppressed;
			break;
		}
	}

	public static bool IsMultimodalHandsControllersSupported()
	{
		return OVRPlugin.IsMultimodalHandsControllersSupported();
	}

	public static bool IsInsightPassthroughSupported()
	{
		return OVRPlugin.IsInsightPassthroughSupported();
	}

	public static PassthroughCapabilities GetPassthroughCapabilities()
	{
		if (_passthroughCapabilities == null)
		{
			OVRPlugin.PassthroughCapabilities outCapabilities = default(OVRPlugin.PassthroughCapabilities);
			if (!OVRPlugin.GetPassthroughCapabilities(ref outCapabilities).IsSuccess())
			{
				outCapabilities.Flags = OVRPlugin.GetPassthroughCapabilityFlags();
				outCapabilities.MaxColorLutResolution = 64u;
			}
			_passthroughCapabilities = new PassthroughCapabilities((outCapabilities.Flags & OVRPlugin.PassthroughCapabilityFlags.Passthrough) == OVRPlugin.PassthroughCapabilityFlags.Passthrough, (outCapabilities.Flags & OVRPlugin.PassthroughCapabilityFlags.Color) == OVRPlugin.PassthroughCapabilityFlags.Color, outCapabilities.MaxColorLutResolution);
		}
		return _passthroughCapabilities;
	}

	public static bool IsInsightPassthroughInitialized()
	{
		return _passthroughInitializationState.Value == PassthroughInitializationState.Initialized;
	}

	public static bool HasInsightPassthroughInitFailed()
	{
		return _passthroughInitializationState.Value == PassthroughInitializationState.Failed;
	}

	public static bool IsInsightPassthroughInitPending()
	{
		return _passthroughInitializationState.Value == PassthroughInitializationState.Pending;
	}

	public static bool IsPassthroughRecommended()
	{
		OVRPlugin.GetPassthroughPreferences(out var preferences);
		return (preferences.Flags & OVRPlugin.PassthroughPreferenceFlags.DefaultToActive) == OVRPlugin.PassthroughPreferenceFlags.DefaultToActive;
	}

	public static bool GetFixedFoveatedRenderingSupported()
	{
		if (IsOpenXRLoaderActive())
		{
			string[] array = "XR_FB_foveation XR_FB_foveation_configuration XR_FB_foveation_vulkan ".Split(' ', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				if (!OpenXRRuntime.IsExtensionEnabled(array[i]))
				{
					return false;
				}
			}
			return true;
		}
		return OVRPlugin.fixedFoveatedRenderingSupported;
	}

	public static FoveatedRenderingLevel GetFoveatedRenderingLevel()
	{
		if (IsOpenXRLoaderActive())
		{
			return MetaXRFoveationFeature.foveatedRenderingLevel;
		}
		return (FoveatedRenderingLevel)OVRPlugin.foveatedRenderingLevel;
	}

	public static void SetFoveatedRenderingLevel(FoveatedRenderingLevel level)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXRFoveationFeature.foveatedRenderingLevel = level;
		}
		else
		{
			OVRPlugin.foveatedRenderingLevel = (OVRPlugin.FoveatedRenderingLevel)level;
		}
	}

	public static bool GetDynamicFoveatedRenderingEnabled()
	{
		if (IsOpenXRLoaderActive())
		{
			return MetaXRFoveationFeature.useDynamicFoveatedRendering;
		}
		return OVRPlugin.useDynamicFoveatedRendering;
	}

	public static void SetDynamicFoveatedRenderingEnabled(bool enabled)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXRFoveationFeature.useDynamicFoveatedRendering = enabled;
		}
		else
		{
			OVRPlugin.useDynamicFoveatedRendering = enabled;
		}
	}

	public static bool GetEyeTrackedFoveatedRenderingSupported()
	{
		if (IsOpenXRLoaderActive())
		{
			return MetaXREyeTrackedFoveationFeature.eyeTrackedFoveatedRenderingSupported;
		}
		return OVRPlugin.eyeTrackedFoveatedRenderingSupported;
	}

	public static bool GetEyeTrackedFoveatedRenderingEnabled()
	{
		if (IsOpenXRLoaderActive())
		{
			return MetaXREyeTrackedFoveationFeature.eyeTrackedFoveatedRenderingEnabled;
		}
		return OVRPlugin.eyeTrackedFoveatedRenderingEnabled;
	}

	public static void SetEyeTrackedFoveatedRenderingEnabled(bool enabled)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXREyeTrackedFoveationFeature.eyeTrackedFoveatedRenderingEnabled = enabled;
		}
		else
		{
			OVRPlugin.eyeTrackedFoveatedRenderingEnabled = enabled;
		}
	}

	public static void SetSpaceWarp_Internal(bool enabled)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXRSpaceWarp.SetSpaceWarp(enabled);
		}
		else
		{
			Debug.Log("Failed to set Space Warp. Current XR Loader does not support this feature.");
		}
	}

	public static void SetAppSpacePosition(float x, float y, float z)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXRSpaceWarp.SetAppSpacePosition(x, y, z);
		}
		else
		{
			Debug.Log("Failed to set Space Warp App Position. Current XR Loader does not support this feature.");
		}
	}

	public static void SetAppSpaceRotation(float x, float y, float z, float w)
	{
		if (IsOpenXRLoaderActive())
		{
			MetaXRSpaceWarp.SetAppSpaceRotation(x, y, z, w);
		}
		else
		{
			Debug.Log("Failed to set Space Warp App Rotation. Current XR Loader does not support this feature.");
		}
	}

	public static bool SetColorScaleAndOffset_Internal(Vector4 colorScale, Vector4 colorOffset, bool applyToAllLayers)
	{
		if (IsOpenXRLoaderActive())
		{
			return OVRPlugin.SetColorScaleAndOffset(colorScale, colorOffset, applyToAllLayers);
		}
		return false;
	}

	private static bool IsOpenXRLoaderActive()
	{
		return XRGeneralSettings.Instance.Manager.activeLoader as OpenXRLoader != null;
	}
}
