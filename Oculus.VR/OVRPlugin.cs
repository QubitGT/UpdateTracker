using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;

public static class OVRPlugin
{
	[StructLayout(LayoutKind.Sequential)]
	private class GUID
	{
		public int a;

		public short b;

		public short c;

		public byte d0;

		public byte d1;

		public byte d2;

		public byte d3;

		public byte d4;

		public byte d5;

		public byte d6;

		public byte d7;
	}

	public enum Bool
	{
		False,
		True
	}

	public enum OptionalBool
	{
		False,
		True,
		Unknown
	}

	[OVRResultStatus]
	public enum Result
	{
		Success = 0,
		Success_EventUnavailable = 1,
		Success_Pending = 2,
		Success_ColocationSessionAlreadyAdvertising = 3001,
		Success_ColocationSessionAlreadyDiscovering = 3002,
		Failure = -1000,
		Failure_InvalidParameter = -1001,
		Failure_NotInitialized = -1002,
		Failure_InvalidOperation = -1003,
		Failure_Unsupported = -1004,
		Failure_NotYetImplemented = -1005,
		Failure_OperationFailed = -1006,
		Failure_InsufficientSize = -1007,
		Failure_DataIsInvalid = -1008,
		Failure_DeprecatedOperation = -1009,
		Failure_ErrorLimitReached = -1010,
		Failure_ErrorInitializationFailed = -1011,
		Failure_RuntimeUnavailable = -1012,
		Failure_HandleInvalid = -1013,
		Failure_SpaceCloudStorageDisabled = -2000,
		Failure_SpaceMappingInsufficient = -2001,
		Failure_SpaceLocalizationFailed = -2002,
		Failure_SpaceNetworkTimeout = -2003,
		Failure_SpaceNetworkRequestFailed = -2004,
		Failure_SpaceComponentNotSupported = -2005,
		Failure_SpaceComponentNotEnabled = -2006,
		Failure_SpaceComponentStatusPending = -2007,
		Failure_SpaceComponentStatusAlreadySet = -2008,
		Failure_SpaceGroupNotFound = -2009,
		Failure_ColocationSessionNetworkFailed = -3002,
		Failure_ColocationSessionNoDiscoveryMethodAvailable = -3003,
		Failure_SpaceInsufficientResources = -9000,
		Failure_SpaceStorageAtCapacity = -9001,
		Failure_SpaceInsufficientView = -9002,
		Failure_SpacePermissionInsufficient = -9003,
		Failure_SpaceRateLimited = -9004,
		Failure_SpaceTooDark = -9005,
		Failure_SpaceTooBright = -9006,
		Warning_BoundaryVisibilitySuppressionNotAllowed = 9030,
		Failure_FuturePending = -10000,
		Failure_FutureInvalid = -10001
	}

	public enum LogLevel
	{
		Debug,
		Info,
		Error
	}

	public delegate void LogCallback2DelegateType(LogLevel logLevel, IntPtr message, int size);

	public enum CameraStatus
	{
		CameraStatus_None = 0,
		CameraStatus_Connected = 1,
		CameraStatus_Calibrating = 2,
		CameraStatus_CalibrationFailed = 3,
		CameraStatus_Calibrated = 4,
		CameraStatus_ThirdPerson = 5,
		CameraStatus_EnumSize = int.MaxValue
	}

	public enum CameraAnchorType
	{
		CameraAnchorType_PreDefined = 0,
		CameraAnchorType_Custom = 1,
		CameraAnchorType_Count = 2,
		CameraAnchorType_EnumSize = int.MaxValue
	}

	public enum XrApi
	{
		Unknown = 0,
		CAPI = 1,
		VRAPI = 2,
		OpenXR = 3,
		EnumSize = int.MaxValue
	}

	public enum Eye
	{
		None = -1,
		Left,
		Right,
		Count
	}

	public enum Tracker
	{
		None = -1,
		Zero,
		One,
		Two,
		Three,
		Count
	}

	public enum Node
	{
		None = -1,
		EyeLeft,
		EyeRight,
		EyeCenter,
		HandLeft,
		HandRight,
		TrackerZero,
		TrackerOne,
		TrackerTwo,
		TrackerThree,
		Head,
		DeviceObjectZero,
		TrackedKeyboard,
		ControllerLeft,
		ControllerRight,
		Count
	}

	public enum ActionTypes
	{
		Boolean = 1,
		Float = 2,
		Vector2 = 3,
		Pose = 4,
		Vibration = 100
	}

	public enum Controller
	{
		None = 0,
		LTouch = 1,
		RTouch = 2,
		Touch = 3,
		Remote = 4,
		Gamepad = 16,
		LHand = 32,
		RHand = 64,
		Hands = 96,
		Active = int.MinValue,
		All = -1
	}

	public enum InteractionProfile
	{
		None = 0,
		Touch = 1,
		TouchPro = 2,
		TouchPlus = 4
	}

	public enum Handedness
	{
		Unsupported,
		LeftHanded,
		RightHanded
	}

	public enum TrackingOrigin
	{
		EyeLevel = 0,
		FloorLevel = 1,
		Stage = 2,
		View = 4,
		Stationary = 6,
		Count = 7
	}

	public enum SpaceFlags
	{
		None,
		AllowRecentering
	}

	public enum RecenterFlags
	{
		Default = 0,
		IgnoreAll = int.MinValue,
		Count = -2147483647
	}

	public enum BatteryStatus
	{
		Charging,
		Discharging,
		Full,
		NotCharging,
		Unknown
	}

	public enum EyeTextureFormat
	{
		Default = 0,
		R8G8B8A8_sRGB = 0,
		R8G8B8A8 = 1,
		R16G16B16A16_FP = 2,
		R11G11B10_FP = 3,
		B8G8R8A8_sRGB = 4,
		B8G8R8A8 = 5,
		R5G6B5 = 11,
		EnumSize = int.MaxValue
	}

	public enum PlatformUI
	{
		None = -1,
		ConfirmQuit = 1,
		GlobalMenuTutorial = 2
	}

	public enum SystemRegion
	{
		Unspecified,
		Japan,
		China
	}

	public enum SystemHeadset
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

	public enum OverlayShape
	{
		Quad = 0,
		Cylinder = 1,
		Cubemap = 2,
		OffcenterCubemap = 4,
		Equirect = 5,
		ReconstructionPassthrough = 7,
		SurfaceProjectedPassthrough = 8,
		Fisheye = 9,
		KeyboardHandsPassthrough = 10,
		KeyboardMaskedHandsPassthrough = 11
	}

	public enum LayerSuperSamplingType
	{
		None = 0,
		Normal = 4096,
		Quality = 256
	}

	public enum LayerSharpenType
	{
		None = 0,
		Normal = 0x2000,
		Quality = 0x10000,
		Automatic = 0x40000
	}

	public enum Step
	{
		Render = -1,
		Physics
	}

	public enum CameraDevice
	{
		None = 0,
		WebCamera0 = 100,
		WebCamera1 = 101,
		ZEDCamera = 300
	}

	public enum CameraDeviceDepthSensingMode
	{
		Standard,
		Fill
	}

	public enum CameraDeviceDepthQuality
	{
		Low,
		Medium,
		High
	}

	public enum FoveatedRenderingLevel
	{
		Off = 0,
		Low = 1,
		Medium = 2,
		High = 3,
		HighTop = 4,
		EnumSize = int.MaxValue
	}

	[Obsolete("Please use FoveatedRenderingLevel instead", false)]
	public enum FixedFoveatedRenderingLevel
	{
		Off = 0,
		Low = 1,
		Medium = 2,
		High = 3,
		HighTop = 4,
		EnumSize = int.MaxValue
	}

	[Obsolete("Please use FixedFoveatedRenderingLevel instead", false)]
	public enum TiledMultiResLevel
	{
		Off = 0,
		LMSLow = 1,
		LMSMedium = 2,
		LMSHigh = 3,
		LMSHighTop = 4,
		EnumSize = int.MaxValue
	}

	public enum PerfMetrics
	{
		App_CpuTime_Float = 0,
		App_GpuTime_Float = 1,
		Compositor_CpuTime_Float = 3,
		Compositor_GpuTime_Float = 4,
		Compositor_DroppedFrameCount_Int = 5,
		System_GpuUtilPercentage_Float = 7,
		System_CpuUtilAveragePercentage_Float = 8,
		System_CpuUtilWorstPercentage_Float = 9,
		Device_CpuClockFrequencyInMHz_Float = 10,
		Device_GpuClockFrequencyInMHz_Float = 11,
		Device_CpuClockLevel_Int = 12,
		Device_GpuClockLevel_Int = 13,
		Compositor_SpaceWarp_Mode_Int = 14,
		Device_CpuCore0UtilPercentage_Float = 32,
		Device_CpuCore1UtilPercentage_Float = 33,
		Device_CpuCore2UtilPercentage_Float = 34,
		Device_CpuCore3UtilPercentage_Float = 35,
		Device_CpuCore4UtilPercentage_Float = 36,
		Device_CpuCore5UtilPercentage_Float = 37,
		Device_CpuCore6UtilPercentage_Float = 38,
		Device_CpuCore7UtilPercentage_Float = 39,
		Count = 40,
		EnumSize = int.MaxValue
	}

	public enum ProcessorPerformanceLevel
	{
		PowerSavings = 0,
		SustainedLow = 1,
		SustainedHigh = 2,
		Boost = 3,
		EnumSize = int.MaxValue
	}

	public enum FeatureType
	{
		HandTracking = 0,
		KeyboardTracking = 1,
		EyeTracking = 2,
		FaceTracking = 3,
		BodyTracking = 4,
		Passthrough = 5,
		GazeBasedFoveatedRendering = 6,
		Count = 7,
		EnumSize = int.MaxValue
	}

	public struct CameraDeviceIntrinsicsParameters
	{
		private float fx;

		private float fy;

		private float cx;

		private float cy;

		private double disto0;

		private double disto1;

		private double disto2;

		private double disto3;

		private double disto4;

		private float v_fov;

		private float h_fov;

		private float d_fov;

		private int w;

		private int h;
	}

	private enum OverlayFlag
	{
		None = 0,
		OnTop = 1,
		HeadLocked = 2,
		NoDepth = 4,
		ExpensiveSuperSample = 8,
		EfficientSuperSample = 16,
		EfficientSharpen = 32,
		BicubicFiltering = 64,
		ExpensiveSharpen = 128,
		SecureContent = 256,
		ShapeFlag_Quad = 0,
		ShapeFlag_Cylinder = 16,
		ShapeFlag_Cubemap = 32,
		ShapeFlag_OffcenterCubemap = 64,
		ShapeFlagRangeMask = 240,
		Hidden = 512,
		AutoFiltering = 1024,
		PremultipliedAlpha = 1048576
	}

	public struct Vector2f
	{
		public float x;

		public float y;
	}

	public struct Vector3f
	{
		public float x;

		public float y;

		public float z;

		public static readonly Vector3f zero = new Vector3f
		{
			x = 0f,
			y = 0f,
			z = 0f
		};

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", x, y, z);
		}
	}

	public struct Vector4f
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public static readonly Vector4f zero = new Vector4f
		{
			x = 0f,
			y = 0f,
			z = 0f,
			w = 0f
		};

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
		}
	}

	public struct Vector4s
	{
		public short x;

		public short y;

		public short z;

		public short w;

		public static readonly Vector4s zero = new Vector4s
		{
			x = 0,
			y = 0,
			z = 0,
			w = 0
		};

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
		}
	}

	public struct Quatf
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public static readonly Quatf identity = new Quatf
		{
			x = 0f,
			y = 0f,
			z = 0f,
			w = 1f
		};

		public Quatf(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
		}
	}

	public struct Posef
	{
		public Quatf Orientation;

		public Vector3f Position;

		public static readonly Posef identity = new Posef
		{
			Orientation = Quatf.identity,
			Position = Vector3f.zero
		};

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Position ({0}), Orientation({1})", Position, Orientation);
		}
	}

	public struct TextureRectMatrixf
	{
		public Rect leftRect;

		public Rect rightRect;

		public Vector4 leftScaleBias;

		public Vector4 rightScaleBias;

		public static readonly TextureRectMatrixf zero = new TextureRectMatrixf
		{
			leftRect = new Rect(0f, 0f, 1f, 1f),
			rightRect = new Rect(0f, 0f, 1f, 1f),
			leftScaleBias = new Vector4(1f, 1f, 0f, 0f),
			rightScaleBias = new Vector4(1f, 1f, 0f, 0f)
		};

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Rect Left ({0}), Rect Right({1}), Scale Bias Left ({2}), Scale Bias Right({3})", leftRect, rightRect, leftScaleBias, rightScaleBias);
		}
	}

	public struct PoseStatef
	{
		public Posef Pose;

		public Vector3f Velocity;

		[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
		public Vector3f Acceleration;

		public Vector3f AngularVelocity;

		[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
		public Vector3f AngularAcceleration;

		public double Time;

		public static readonly PoseStatef identity = new PoseStatef
		{
			Pose = Posef.identity,
			Velocity = Vector3f.zero,
			AngularVelocity = Vector3f.zero,
			Acceleration = Vector3f.zero,
			AngularAcceleration = Vector3f.zero
		};
	}

	public enum HapticsLocation
	{
		None = 0,
		Hand = 1,
		Thumb = 2,
		Index = 4
	}

	public struct ControllerState6
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte LBatteryPercentRemaining;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte RBatteryPercentRemaining;

		public byte LRecenterCount;

		public byte RRecenterCount;

		public float LThumbRestForce;

		public float RThumbRestForce;

		public float LStylusForce;

		public float RStylusForce;

		public float LIndexTriggerCurl;

		public float RIndexTriggerCurl;

		public float LIndexTriggerSlide;

		public float RIndexTriggerSlide;

		public float LIndexTriggerForce;

		public float RIndexTriggerForce;

		public ControllerState6(ControllerState5 cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = cs.LTouchpad;
			RTouchpad = cs.RTouchpad;
			LBatteryPercentRemaining = cs.LBatteryPercentRemaining;
			RBatteryPercentRemaining = cs.RBatteryPercentRemaining;
			LRecenterCount = cs.LRecenterCount;
			RRecenterCount = cs.RRecenterCount;
			LThumbRestForce = cs.LThumbRestForce;
			RThumbRestForce = cs.RThumbRestForce;
			LStylusForce = cs.LStylusForce;
			RStylusForce = cs.RStylusForce;
			LIndexTriggerCurl = cs.LIndexTriggerCurl;
			RIndexTriggerCurl = cs.RIndexTriggerCurl;
			LIndexTriggerSlide = cs.LIndexTriggerSlide;
			RIndexTriggerSlide = cs.RIndexTriggerSlide;
			LIndexTriggerForce = 0f;
			RIndexTriggerForce = 0f;
		}
	}

	public struct ControllerState5
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte LBatteryPercentRemaining;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte RBatteryPercentRemaining;

		public byte LRecenterCount;

		public byte RRecenterCount;

		public float LThumbRestForce;

		public float RThumbRestForce;

		public float LStylusForce;

		public float RStylusForce;

		public float LIndexTriggerCurl;

		public float RIndexTriggerCurl;

		public float LIndexTriggerSlide;

		public float RIndexTriggerSlide;

		public ControllerState5(ControllerState4 cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = cs.LTouchpad;
			RTouchpad = cs.RTouchpad;
			LBatteryPercentRemaining = cs.LBatteryPercentRemaining;
			RBatteryPercentRemaining = cs.RBatteryPercentRemaining;
			LRecenterCount = cs.LRecenterCount;
			RRecenterCount = cs.RRecenterCount;
			LThumbRestForce = 0f;
			RThumbRestForce = 0f;
			LStylusForce = 0f;
			RStylusForce = 0f;
			LIndexTriggerCurl = 0f;
			RIndexTriggerCurl = 0f;
			LIndexTriggerSlide = 0f;
			RIndexTriggerSlide = 0f;
		}
	}

	public struct ControllerState4
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte LBatteryPercentRemaining;

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public byte RBatteryPercentRemaining;

		public byte LRecenterCount;

		public byte RRecenterCount;

		public byte Reserved_27;

		public byte Reserved_26;

		public byte Reserved_25;

		public byte Reserved_24;

		public byte Reserved_23;

		public byte Reserved_22;

		public byte Reserved_21;

		public byte Reserved_20;

		public byte Reserved_19;

		public byte Reserved_18;

		public byte Reserved_17;

		public byte Reserved_16;

		public byte Reserved_15;

		public byte Reserved_14;

		public byte Reserved_13;

		public byte Reserved_12;

		public byte Reserved_11;

		public byte Reserved_10;

		public byte Reserved_09;

		public byte Reserved_08;

		public byte Reserved_07;

		public byte Reserved_06;

		public byte Reserved_05;

		public byte Reserved_04;

		public byte Reserved_03;

		public byte Reserved_02;

		public byte Reserved_01;

		public byte Reserved_00;

		public ControllerState4(ControllerState2 cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = cs.LTouchpad;
			RTouchpad = cs.RTouchpad;
			LBatteryPercentRemaining = 0;
			RBatteryPercentRemaining = 0;
			LRecenterCount = 0;
			RRecenterCount = 0;
			Reserved_27 = 0;
			Reserved_26 = 0;
			Reserved_25 = 0;
			Reserved_24 = 0;
			Reserved_23 = 0;
			Reserved_22 = 0;
			Reserved_21 = 0;
			Reserved_20 = 0;
			Reserved_19 = 0;
			Reserved_18 = 0;
			Reserved_17 = 0;
			Reserved_16 = 0;
			Reserved_15 = 0;
			Reserved_14 = 0;
			Reserved_13 = 0;
			Reserved_12 = 0;
			Reserved_11 = 0;
			Reserved_10 = 0;
			Reserved_09 = 0;
			Reserved_08 = 0;
			Reserved_07 = 0;
			Reserved_06 = 0;
			Reserved_05 = 0;
			Reserved_04 = 0;
			Reserved_03 = 0;
			Reserved_02 = 0;
			Reserved_01 = 0;
			Reserved_00 = 0;
		}
	}

	public struct ControllerState2
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;

		public Vector2f LTouchpad;

		public Vector2f RTouchpad;

		public ControllerState2(ControllerState cs)
		{
			ConnectedControllers = cs.ConnectedControllers;
			Buttons = cs.Buttons;
			Touches = cs.Touches;
			NearTouches = cs.NearTouches;
			LIndexTrigger = cs.LIndexTrigger;
			RIndexTrigger = cs.RIndexTrigger;
			LHandTrigger = cs.LHandTrigger;
			RHandTrigger = cs.RHandTrigger;
			LThumbstick = cs.LThumbstick;
			RThumbstick = cs.RThumbstick;
			LTouchpad = new Vector2f
			{
				x = 0f,
				y = 0f
			};
			RTouchpad = new Vector2f
			{
				x = 0f,
				y = 0f
			};
		}
	}

	public struct ControllerState
	{
		public uint ConnectedControllers;

		public uint Buttons;

		public uint Touches;

		public uint NearTouches;

		public float LIndexTrigger;

		public float RIndexTrigger;

		public float LHandTrigger;

		public float RHandTrigger;

		public Vector2f LThumbstick;

		public Vector2f RThumbstick;
	}

	public struct HapticsBuffer
	{
		public IntPtr Samples;

		public int SamplesCount;
	}

	public struct HapticsState
	{
		public int SamplesAvailable;

		public int SamplesQueued;
	}

	public struct HapticsDesc
	{
		public int SampleRateHz;

		public int SampleSizeInBytes;

		public int MinimumSafeSamplesQueued;

		public int MinimumBufferSamplesCount;

		public int OptimalBufferSamplesCount;

		public int MaximumBufferSamplesCount;
	}

	public struct HapticsAmplitudeEnvelopeVibration
	{
		public float Duration;

		public uint AmplitudeCount;

		public IntPtr Amplitudes;
	}

	public struct HapticsPcmVibration
	{
		public uint BufferSize;

		public IntPtr Buffer;

		public float SampleRateHz;

		public Bool Append;

		public IntPtr SamplesConsumed;
	}

	public enum HapticsConstants
	{
		ParametricHapticsUnspecifiedFrequency = 0,
		MaxSamples = 4000
	}

	public struct AppPerfFrameStats
	{
		public int HmdVsyncIndex;

		public int AppFrameIndex;

		public int AppDroppedFrameCount;

		public float AppMotionToPhotonLatency;

		public float AppQueueAheadTime;

		public float AppCpuElapsedTime;

		public float AppGpuElapsedTime;

		public int CompositorFrameIndex;

		public int CompositorDroppedFrameCount;

		public float CompositorLatency;

		public float CompositorCpuElapsedTime;

		public float CompositorGpuElapsedTime;

		public float CompositorCpuStartToGpuEndElapsedTime;

		public float CompositorGpuEndToVsyncElapsedTime;
	}

	public struct AppPerfStats
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public AppPerfFrameStats[] FrameStats;

		public int FrameStatsCount;

		public Bool AnyFrameStatsDropped;

		public float AdaptiveGpuPerformanceScale;
	}

	public struct Sizei : IEquatable<Sizei>
	{
		public int w;

		public int h;

		public static readonly Sizei zero = new Sizei
		{
			w = 0,
			h = 0
		};

		public bool Equals(Sizei other)
		{
			if (w == other.w)
			{
				return h == other.h;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Sizei other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (w * 397) ^ h;
		}
	}

	public struct Sizef
	{
		public float w;

		public float h;

		public static readonly Sizef zero = new Sizef
		{
			w = 0f,
			h = 0f
		};
	}

	public struct Size3f
	{
		public float w;

		public float h;

		public float d;

		public static readonly Size3f zero = new Size3f
		{
			w = 0f,
			h = 0f,
			d = 0f
		};
	}

	public struct Vector2i
	{
		public int x;

		public int y;
	}

	public struct Recti
	{
		public Vector2i Pos;

		public Sizei Size;
	}

	public struct RectiPair
	{
		public Recti Rect0;

		public Recti Rect1;

		public Recti this[int i]
		{
			get
			{
				return i switch
				{
					0 => Rect0, 
					1 => Rect1, 
					_ => throw new IndexOutOfRangeException($"{i} was not in range [0,2)"), 
				};
			}
			set
			{
				switch (i)
				{
				case 0:
					Rect0 = value;
					break;
				case 1:
					Rect1 = value;
					break;
				default:
					throw new IndexOutOfRangeException($"{i} was not in range [0,2)");
				}
			}
		}
	}

	public struct Rectf
	{
		public Vector2f Pos;

		public Sizef Size;
	}

	public struct RectfPair
	{
		public Rectf Rect0;

		public Rectf Rect1;

		public Rectf this[int i]
		{
			get
			{
				return i switch
				{
					0 => Rect0, 
					1 => Rect1, 
					_ => throw new IndexOutOfRangeException($"{i} was not in range [0,2)"), 
				};
			}
			set
			{
				switch (i)
				{
				case 0:
					Rect0 = value;
					break;
				case 1:
					Rect1 = value;
					break;
				default:
					throw new IndexOutOfRangeException($"{i} was not in range [0,2)");
				}
			}
		}
	}

	public struct Boundsf
	{
		public Vector3f Pos;

		public Size3f Size;
	}

	public struct Frustumf
	{
		public float zNear;

		public float zFar;

		public float fovX;

		public float fovY;
	}

	public struct Frustumf2
	{
		public float zNear;

		public float zFar;

		public Fovf Fov;
	}

	public enum BoundaryType
	{
		[Obsolete("Deprecated. This enum value will not be supported in OpenXR", false)]
		OuterBoundary = 1,
		PlayArea = 0x100
	}

	[Obsolete("Deprecated. This struct will not be supported in OpenXR", false)]
	public struct BoundaryTestResult
	{
		public Bool IsTriggering;

		public float ClosestDistance;

		public Vector3f ClosestPoint;

		public Vector3f ClosestPointNormal;
	}

	public struct BoundaryGeometry
	{
		public BoundaryType BoundaryType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public Vector3f[] Points;

		public int PointsCount;
	}

	public struct Colorf
	{
		public float r;

		public float g;

		public float b;

		public float a;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "R:{0:F3} G:{1:F3} B:{2:F3} A:{3:F3}", r, g, b, a);
		}
	}

	public struct Fovf
	{
		public float UpTan;

		public float DownTan;

		public float LeftTan;

		public float RightTan;
	}

	public struct FovfPair
	{
		public Fovf Fov0;

		public Fovf Fov1;

		public Fovf this[int i]
		{
			get
			{
				return i switch
				{
					0 => Fov0, 
					1 => Fov1, 
					_ => throw new IndexOutOfRangeException($"{i} was not in range [0,2)"), 
				};
			}
			set
			{
				switch (i)
				{
				case 0:
					Fov0 = value;
					break;
				case 1:
					Fov1 = value;
					break;
				default:
					throw new IndexOutOfRangeException($"{i} was not in range [0,2)");
				}
			}
		}
	}

	public struct CameraIntrinsics
	{
		public Bool IsValid;

		public double LastChangedTimeSeconds;

		public Fovf FOVPort;

		public float VirtualNearPlaneDistanceMeters;

		public float VirtualFarPlaneDistanceMeters;

		public Sizei ImageSensorPixelResolution;
	}

	public struct CameraExtrinsics
	{
		public Bool IsValid;

		public double LastChangedTimeSeconds;

		public CameraStatus CameraStatusData;

		public Node AttachedToNode;

		public Posef RelativePose;
	}

	public enum LayerLayout
	{
		Stereo = 0,
		Mono = 1,
		DoubleWide = 2,
		Array = 3,
		EnumSize = 15
	}

	public enum LayerFlags
	{
		Static = 1,
		LoadingScreen = 2,
		SymmetricFov = 4,
		TextureOriginAtBottomLeft = 8,
		ChromaticAberrationCorrection = 0x10,
		NoAllocation = 0x20,
		ProtectedContent = 0x40,
		AndroidSurfaceSwapChain = 0x80,
		BicubicFiltering = 0x4000
	}

	public struct LayerDesc
	{
		public OverlayShape Shape;

		public LayerLayout Layout;

		public Sizei TextureSize;

		public int MipLevels;

		public int SampleCount;

		public EyeTextureFormat Format;

		public int LayerFlags;

		public FovfPair Fov;

		public RectfPair VisibleRect;

		public Sizei MaxViewportSize;

		public EyeTextureFormat DepthFormat;

		public EyeTextureFormat MotionVectorFormat;

		public EyeTextureFormat MotionVectorDepthFormat;

		public Sizei MotionVectorTextureSize;

		public override string ToString()
		{
			string text = ", ";
			return Shape.ToString() + text + Layout.ToString() + text + TextureSize.w + "x" + TextureSize.h + text + MipLevels + text + SampleCount + text + Format.ToString() + text + LayerFlags;
		}
	}

	public enum BlendFactor
	{
		Zero,
		One,
		SrcAlpha,
		OneMinusSrcAlpha,
		DstAlpha,
		OneMinusDstAlpha
	}

	public struct LayerSubmit
	{
		private int LayerId;

		private int TextureStage;

		private RectiPair ViewportRect;

		private Posef Pose;

		private int LayerSubmitFlags;
	}

	public enum TrackingConfidence
	{
		Low = 0,
		High = 1065353216
	}

	public enum Hand
	{
		None = -1,
		HandLeft,
		HandRight
	}

	[Flags]
	public enum HandStatus
	{
		HandTracked = 1,
		InputStateValid = 2,
		SystemGestureInProgress = 0x40,
		DominantHand = 0x80,
		MenuPressed = 0x100
	}

	public enum BoneId
	{
		Invalid = -1,
		Hand_Start = 0,
		Hand_WristRoot = 0,
		Hand_ForearmStub = 1,
		Hand_Thumb0 = 2,
		Hand_Thumb1 = 3,
		Hand_Thumb2 = 4,
		Hand_Thumb3 = 5,
		Hand_Index1 = 6,
		Hand_Index2 = 7,
		Hand_Index3 = 8,
		Hand_Middle1 = 9,
		Hand_Middle2 = 10,
		Hand_Middle3 = 11,
		Hand_Ring1 = 12,
		Hand_Ring2 = 13,
		Hand_Ring3 = 14,
		Hand_Pinky0 = 15,
		Hand_Pinky1 = 16,
		Hand_Pinky2 = 17,
		Hand_Pinky3 = 18,
		Hand_MaxSkinnable = 19,
		Hand_ThumbTip = 19,
		Hand_IndexTip = 20,
		Hand_MiddleTip = 21,
		Hand_RingTip = 22,
		Hand_PinkyTip = 23,
		Hand_End = 24,
		XRHand_Start = 0,
		XRHand_Palm = 0,
		XRHand_Wrist = 1,
		XRHand_ThumbMetacarpal = 2,
		XRHand_ThumbProximal = 3,
		XRHand_ThumbDistal = 4,
		XRHand_ThumbTip = 5,
		XRHand_IndexMetacarpal = 6,
		XRHand_IndexProximal = 7,
		XRHand_IndexIntermediate = 8,
		XRHand_IndexDistal = 9,
		XRHand_IndexTip = 10,
		XRHand_MiddleMetacarpal = 11,
		XRHand_MiddleProximal = 12,
		XRHand_MiddleIntermediate = 13,
		XRHand_MiddleDistal = 14,
		XRHand_MiddleTip = 15,
		XRHand_RingMetacarpal = 16,
		XRHand_RingProximal = 17,
		XRHand_RingIntermediate = 18,
		XRHand_RingDistal = 19,
		XRHand_RingTip = 20,
		XRHand_LittleMetacarpal = 21,
		XRHand_LittleProximal = 22,
		XRHand_LittleIntermediate = 23,
		XRHand_LittleDistal = 24,
		XRHand_LittleTip = 25,
		XRHand_Max = 26,
		XRHand_End = 26,
		Body_Start = 0,
		Body_Root = 0,
		Body_Hips = 1,
		Body_SpineLower = 2,
		Body_SpineMiddle = 3,
		Body_SpineUpper = 4,
		Body_Chest = 5,
		Body_Neck = 6,
		Body_Head = 7,
		Body_LeftShoulder = 8,
		Body_LeftScapula = 9,
		Body_LeftArmUpper = 10,
		Body_LeftArmLower = 11,
		Body_LeftHandWristTwist = 12,
		Body_RightShoulder = 13,
		Body_RightScapula = 14,
		Body_RightArmUpper = 15,
		Body_RightArmLower = 16,
		Body_RightHandWristTwist = 17,
		Body_LeftHandPalm = 18,
		Body_LeftHandWrist = 19,
		Body_LeftHandThumbMetacarpal = 20,
		Body_LeftHandThumbProximal = 21,
		Body_LeftHandThumbDistal = 22,
		Body_LeftHandThumbTip = 23,
		Body_LeftHandIndexMetacarpal = 24,
		Body_LeftHandIndexProximal = 25,
		Body_LeftHandIndexIntermediate = 26,
		Body_LeftHandIndexDistal = 27,
		Body_LeftHandIndexTip = 28,
		Body_LeftHandMiddleMetacarpal = 29,
		Body_LeftHandMiddleProximal = 30,
		Body_LeftHandMiddleIntermediate = 31,
		Body_LeftHandMiddleDistal = 32,
		Body_LeftHandMiddleTip = 33,
		Body_LeftHandRingMetacarpal = 34,
		Body_LeftHandRingProximal = 35,
		Body_LeftHandRingIntermediate = 36,
		Body_LeftHandRingDistal = 37,
		Body_LeftHandRingTip = 38,
		Body_LeftHandLittleMetacarpal = 39,
		Body_LeftHandLittleProximal = 40,
		Body_LeftHandLittleIntermediate = 41,
		Body_LeftHandLittleDistal = 42,
		Body_LeftHandLittleTip = 43,
		Body_RightHandPalm = 44,
		Body_RightHandWrist = 45,
		Body_RightHandThumbMetacarpal = 46,
		Body_RightHandThumbProximal = 47,
		Body_RightHandThumbDistal = 48,
		Body_RightHandThumbTip = 49,
		Body_RightHandIndexMetacarpal = 50,
		Body_RightHandIndexProximal = 51,
		Body_RightHandIndexIntermediate = 52,
		Body_RightHandIndexDistal = 53,
		Body_RightHandIndexTip = 54,
		Body_RightHandMiddleMetacarpal = 55,
		Body_RightHandMiddleProximal = 56,
		Body_RightHandMiddleIntermediate = 57,
		Body_RightHandMiddleDistal = 58,
		Body_RightHandMiddleTip = 59,
		Body_RightHandRingMetacarpal = 60,
		Body_RightHandRingProximal = 61,
		Body_RightHandRingIntermediate = 62,
		Body_RightHandRingDistal = 63,
		Body_RightHandRingTip = 64,
		Body_RightHandLittleMetacarpal = 65,
		Body_RightHandLittleProximal = 66,
		Body_RightHandLittleIntermediate = 67,
		Body_RightHandLittleDistal = 68,
		Body_RightHandLittleTip = 69,
		Body_End = 70,
		FullBody_Start = 0,
		FullBody_Root = 0,
		FullBody_Hips = 1,
		FullBody_SpineLower = 2,
		FullBody_SpineMiddle = 3,
		FullBody_SpineUpper = 4,
		FullBody_Chest = 5,
		FullBody_Neck = 6,
		FullBody_Head = 7,
		FullBody_LeftShoulder = 8,
		FullBody_LeftScapula = 9,
		FullBody_LeftArmUpper = 10,
		FullBody_LeftArmLower = 11,
		FullBody_LeftHandWristTwist = 12,
		FullBody_RightShoulder = 13,
		FullBody_RightScapula = 14,
		FullBody_RightArmUpper = 15,
		FullBody_RightArmLower = 16,
		FullBody_RightHandWristTwist = 17,
		FullBody_LeftHandPalm = 18,
		FullBody_LeftHandWrist = 19,
		FullBody_LeftHandThumbMetacarpal = 20,
		FullBody_LeftHandThumbProximal = 21,
		FullBody_LeftHandThumbDistal = 22,
		FullBody_LeftHandThumbTip = 23,
		FullBody_LeftHandIndexMetacarpal = 24,
		FullBody_LeftHandIndexProximal = 25,
		FullBody_LeftHandIndexIntermediate = 26,
		FullBody_LeftHandIndexDistal = 27,
		FullBody_LeftHandIndexTip = 28,
		FullBody_LeftHandMiddleMetacarpal = 29,
		FullBody_LeftHandMiddleProximal = 30,
		FullBody_LeftHandMiddleIntermediate = 31,
		FullBody_LeftHandMiddleDistal = 32,
		FullBody_LeftHandMiddleTip = 33,
		FullBody_LeftHandRingMetacarpal = 34,
		FullBody_LeftHandRingProximal = 35,
		FullBody_LeftHandRingIntermediate = 36,
		FullBody_LeftHandRingDistal = 37,
		FullBody_LeftHandRingTip = 38,
		FullBody_LeftHandLittleMetacarpal = 39,
		FullBody_LeftHandLittleProximal = 40,
		FullBody_LeftHandLittleIntermediate = 41,
		FullBody_LeftHandLittleDistal = 42,
		FullBody_LeftHandLittleTip = 43,
		FullBody_RightHandPalm = 44,
		FullBody_RightHandWrist = 45,
		FullBody_RightHandThumbMetacarpal = 46,
		FullBody_RightHandThumbProximal = 47,
		FullBody_RightHandThumbDistal = 48,
		FullBody_RightHandThumbTip = 49,
		FullBody_RightHandIndexMetacarpal = 50,
		FullBody_RightHandIndexProximal = 51,
		FullBody_RightHandIndexIntermediate = 52,
		FullBody_RightHandIndexDistal = 53,
		FullBody_RightHandIndexTip = 54,
		FullBody_RightHandMiddleMetacarpal = 55,
		FullBody_RightHandMiddleProximal = 56,
		FullBody_RightHandMiddleIntermediate = 57,
		FullBody_RightHandMiddleDistal = 58,
		FullBody_RightHandMiddleTip = 59,
		FullBody_RightHandRingMetacarpal = 60,
		FullBody_RightHandRingProximal = 61,
		FullBody_RightHandRingIntermediate = 62,
		FullBody_RightHandRingDistal = 63,
		FullBody_RightHandRingTip = 64,
		FullBody_RightHandLittleMetacarpal = 65,
		FullBody_RightHandLittleProximal = 66,
		FullBody_RightHandLittleIntermediate = 67,
		FullBody_RightHandLittleDistal = 68,
		FullBody_RightHandLittleTip = 69,
		FullBody_LeftUpperLeg = 70,
		FullBody_LeftLowerLeg = 71,
		FullBody_LeftFootAnkleTwist = 72,
		FullBody_LeftFootAnkle = 73,
		FullBody_LeftFootSubtalar = 74,
		FullBody_LeftFootTransverse = 75,
		FullBody_LeftFootBall = 76,
		FullBody_RightUpperLeg = 77,
		FullBody_RightLowerLeg = 78,
		FullBody_RightFootAnkleTwist = 79,
		FullBody_RightFootAnkle = 80,
		FullBody_RightFootSubtalar = 81,
		FullBody_RightFootTransverse = 82,
		FullBody_RightFootBall = 83,
		FullBody_End = 84,
		FullBody_Invalid = 85,
		Max = 84
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

	[Flags]
	public enum HandFingerPinch
	{
		Thumb = 1,
		Index = 2,
		Middle = 4,
		Ring = 8,
		Pinky = 0x10
	}

	public struct HandState
	{
		public HandStatus Status;

		public Posef RootPose;

		public Quatf[] BoneRotations;

		public Vector3f[] BonePositions;

		public HandFingerPinch Pinches;

		public float[] PinchStrength;

		public Posef PointerPose;

		public float HandScale;

		public TrackingConfidence HandConfidence;

		public TrackingConfidence[] FingerConfidences;

		public double RequestedTimeStamp;

		public double SampleTimeStamp;
	}

	public struct HandTrackingState
	{
		public MicrogestureType Microgesture;
	}

	private struct HandTrackingStateInternal
	{
		public MicrogestureType Microgesture;
	}

	private struct HandStateInternal
	{
		public HandStatus Status;

		public Posef RootPose;

		public Quatf BoneRotations_0;

		public Quatf BoneRotations_1;

		public Quatf BoneRotations_2;

		public Quatf BoneRotations_3;

		public Quatf BoneRotations_4;

		public Quatf BoneRotations_5;

		public Quatf BoneRotations_6;

		public Quatf BoneRotations_7;

		public Quatf BoneRotations_8;

		public Quatf BoneRotations_9;

		public Quatf BoneRotations_10;

		public Quatf BoneRotations_11;

		public Quatf BoneRotations_12;

		public Quatf BoneRotations_13;

		public Quatf BoneRotations_14;

		public Quatf BoneRotations_15;

		public Quatf BoneRotations_16;

		public Quatf BoneRotations_17;

		public Quatf BoneRotations_18;

		public Quatf BoneRotations_19;

		public Quatf BoneRotations_20;

		public Quatf BoneRotations_21;

		public Quatf BoneRotations_22;

		public Quatf BoneRotations_23;

		public HandFingerPinch Pinches;

		public float PinchStrength_0;

		public float PinchStrength_1;

		public float PinchStrength_2;

		public float PinchStrength_3;

		public float PinchStrength_4;

		public Posef PointerPose;

		public float HandScale;

		public TrackingConfidence HandConfidence;

		public TrackingConfidence FingerConfidences_0;

		public TrackingConfidence FingerConfidences_1;

		public TrackingConfidence FingerConfidences_2;

		public TrackingConfidence FingerConfidences_3;

		public TrackingConfidence FingerConfidences_4;

		public double RequestedTimeStamp;

		public double SampleTimeStamp;
	}

	private struct HandState3Internal
	{
		public HandStatus Status;

		public Posef RootPose;

		public Posef BonePoses_0;

		public Posef BonePoses_1;

		public Posef BonePoses_2;

		public Posef BonePoses_3;

		public Posef BonePoses_4;

		public Posef BonePoses_5;

		public Posef BonePoses_6;

		public Posef BonePoses_7;

		public Posef BonePoses_8;

		public Posef BonePoses_9;

		public Posef BonePoses_10;

		public Posef BonePoses_11;

		public Posef BonePoses_12;

		public Posef BonePoses_13;

		public Posef BonePoses_14;

		public Posef BonePoses_15;

		public Posef BonePoses_16;

		public Posef BonePoses_17;

		public Posef BonePoses_18;

		public Posef BonePoses_19;

		public Posef BonePoses_20;

		public Posef BonePoses_21;

		public Posef BonePoses_22;

		public Posef BonePoses_23;

		public Posef BonePoses_24;

		public Posef BonePoses_25;

		public HandFingerPinch Pinches;

		public float PinchStrength_0;

		public float PinchStrength_1;

		public float PinchStrength_2;

		public float PinchStrength_3;

		public float PinchStrength_4;

		public Posef PointerPose;

		public float HandScale;

		public TrackingConfidence HandConfidence;

		public TrackingConfidence FingerConfidences_0;

		public TrackingConfidence FingerConfidences_1;

		public TrackingConfidence FingerConfidences_2;

		public TrackingConfidence FingerConfidences_3;

		public TrackingConfidence FingerConfidences_4;

		public double RequestedTimeStamp;

		public double SampleTimeStamp;
	}

	public struct BoneCapsule
	{
		public short BoneIndex;

		public Vector3f StartPoint;

		public Vector3f EndPoint;

		public float Radius;
	}

	public struct Bone
	{
		public BoneId Id;

		public short ParentBoneIndex;

		public Posef Pose;
	}

	public enum SkeletonConstants
	{
		MaxHandBones = 24,
		MaxXRHandBones = 26,
		MaxBodyBones = 70,
		MaxBones = 84,
		MaxBoneCapsules = 19
	}

	public enum SkeletonType
	{
		None = -1,
		HandLeft,
		HandRight,
		Body,
		FullBody,
		XRHandLeft,
		XRHandRight
	}

	public struct Skeleton
	{
		public SkeletonType Type;

		public uint NumBones;

		public uint NumBoneCapsules;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public Bone[] Bones;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
		public BoneCapsule[] BoneCapsules;
	}

	public struct Skeleton2
	{
		public SkeletonType Type;

		public uint NumBones;

		public uint NumBoneCapsules;

		public Bone[] Bones;

		public BoneCapsule[] BoneCapsules;
	}

	private struct Skeleton2Internal
	{
		public SkeletonType Type;

		public uint NumBones;

		public uint NumBoneCapsules;

		public Bone Bones_0;

		public Bone Bones_1;

		public Bone Bones_2;

		public Bone Bones_3;

		public Bone Bones_4;

		public Bone Bones_5;

		public Bone Bones_6;

		public Bone Bones_7;

		public Bone Bones_8;

		public Bone Bones_9;

		public Bone Bones_10;

		public Bone Bones_11;

		public Bone Bones_12;

		public Bone Bones_13;

		public Bone Bones_14;

		public Bone Bones_15;

		public Bone Bones_16;

		public Bone Bones_17;

		public Bone Bones_18;

		public Bone Bones_19;

		public Bone Bones_20;

		public Bone Bones_21;

		public Bone Bones_22;

		public Bone Bones_23;

		public Bone Bones_24;

		public Bone Bones_25;

		public Bone Bones_26;

		public Bone Bones_27;

		public Bone Bones_28;

		public Bone Bones_29;

		public Bone Bones_30;

		public Bone Bones_31;

		public Bone Bones_32;

		public Bone Bones_33;

		public Bone Bones_34;

		public Bone Bones_35;

		public Bone Bones_36;

		public Bone Bones_37;

		public Bone Bones_38;

		public Bone Bones_39;

		public Bone Bones_40;

		public Bone Bones_41;

		public Bone Bones_42;

		public Bone Bones_43;

		public Bone Bones_44;

		public Bone Bones_45;

		public Bone Bones_46;

		public Bone Bones_47;

		public Bone Bones_48;

		public Bone Bones_49;

		public Bone Bones_50;

		public Bone Bones_51;

		public Bone Bones_52;

		public Bone Bones_53;

		public Bone Bones_54;

		public Bone Bones_55;

		public Bone Bones_56;

		public Bone Bones_57;

		public Bone Bones_58;

		public Bone Bones_59;

		public Bone Bones_60;

		public Bone Bones_61;

		public Bone Bones_62;

		public Bone Bones_63;

		public Bone Bones_64;

		public Bone Bones_65;

		public Bone Bones_66;

		public Bone Bones_67;

		public Bone Bones_68;

		public Bone Bones_69;

		public BoneCapsule BoneCapsules_0;

		public BoneCapsule BoneCapsules_1;

		public BoneCapsule BoneCapsules_2;

		public BoneCapsule BoneCapsules_3;

		public BoneCapsule BoneCapsules_4;

		public BoneCapsule BoneCapsules_5;

		public BoneCapsule BoneCapsules_6;

		public BoneCapsule BoneCapsules_7;

		public BoneCapsule BoneCapsules_8;

		public BoneCapsule BoneCapsules_9;

		public BoneCapsule BoneCapsules_10;

		public BoneCapsule BoneCapsules_11;

		public BoneCapsule BoneCapsules_12;

		public BoneCapsule BoneCapsules_13;

		public BoneCapsule BoneCapsules_14;

		public BoneCapsule BoneCapsules_15;

		public BoneCapsule BoneCapsules_16;

		public BoneCapsule BoneCapsules_17;

		public BoneCapsule BoneCapsules_18;
	}

	private struct Skeleton3Internal
	{
		public SkeletonType Type;

		public uint NumBones;

		public uint NumBoneCapsules;

		public Bone Bones_0;

		public Bone Bones_1;

		public Bone Bones_2;

		public Bone Bones_3;

		public Bone Bones_4;

		public Bone Bones_5;

		public Bone Bones_6;

		public Bone Bones_7;

		public Bone Bones_8;

		public Bone Bones_9;

		public Bone Bones_10;

		public Bone Bones_11;

		public Bone Bones_12;

		public Bone Bones_13;

		public Bone Bones_14;

		public Bone Bones_15;

		public Bone Bones_16;

		public Bone Bones_17;

		public Bone Bones_18;

		public Bone Bones_19;

		public Bone Bones_20;

		public Bone Bones_21;

		public Bone Bones_22;

		public Bone Bones_23;

		public Bone Bones_24;

		public Bone Bones_25;

		public Bone Bones_26;

		public Bone Bones_27;

		public Bone Bones_28;

		public Bone Bones_29;

		public Bone Bones_30;

		public Bone Bones_31;

		public Bone Bones_32;

		public Bone Bones_33;

		public Bone Bones_34;

		public Bone Bones_35;

		public Bone Bones_36;

		public Bone Bones_37;

		public Bone Bones_38;

		public Bone Bones_39;

		public Bone Bones_40;

		public Bone Bones_41;

		public Bone Bones_42;

		public Bone Bones_43;

		public Bone Bones_44;

		public Bone Bones_45;

		public Bone Bones_46;

		public Bone Bones_47;

		public Bone Bones_48;

		public Bone Bones_49;

		public Bone Bones_50;

		public Bone Bones_51;

		public Bone Bones_52;

		public Bone Bones_53;

		public Bone Bones_54;

		public Bone Bones_55;

		public Bone Bones_56;

		public Bone Bones_57;

		public Bone Bones_58;

		public Bone Bones_59;

		public Bone Bones_60;

		public Bone Bones_61;

		public Bone Bones_62;

		public Bone Bones_63;

		public Bone Bones_64;

		public Bone Bones_65;

		public Bone Bones_66;

		public Bone Bones_67;

		public Bone Bones_68;

		public Bone Bones_69;

		public Bone Bones_70;

		public Bone Bones_71;

		public Bone Bones_72;

		public Bone Bones_73;

		public Bone Bones_74;

		public Bone Bones_75;

		public Bone Bones_76;

		public Bone Bones_77;

		public Bone Bones_78;

		public Bone Bones_79;

		public Bone Bones_80;

		public Bone Bones_81;

		public Bone Bones_82;

		public Bone Bones_83;

		public BoneCapsule BoneCapsules_0;

		public BoneCapsule BoneCapsules_1;

		public BoneCapsule BoneCapsules_2;

		public BoneCapsule BoneCapsules_3;

		public BoneCapsule BoneCapsules_4;

		public BoneCapsule BoneCapsules_5;

		public BoneCapsule BoneCapsules_6;

		public BoneCapsule BoneCapsules_7;

		public BoneCapsule BoneCapsules_8;

		public BoneCapsule BoneCapsules_9;

		public BoneCapsule BoneCapsules_10;

		public BoneCapsule BoneCapsules_11;

		public BoneCapsule BoneCapsules_12;

		public BoneCapsule BoneCapsules_13;

		public BoneCapsule BoneCapsules_14;

		public BoneCapsule BoneCapsules_15;

		public BoneCapsule BoneCapsules_16;

		public BoneCapsule BoneCapsules_17;

		public BoneCapsule BoneCapsules_18;
	}

	public enum MeshConstants
	{
		MaxVertices = 3000,
		MaxIndices = 18000
	}

	public enum MeshType
	{
		None = -1,
		HandLeft = 0,
		HandRight = 1,
		XRHandLeft = 4,
		XRHandRight = 5
	}

	[StructLayout(LayoutKind.Sequential)]
	public class Mesh
	{
		public MeshType Type;

		public uint NumVertices;

		public uint NumIndices;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
		public Vector3f[] VertexPositions;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18000)]
		public short[] Indices;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
		public Vector3f[] VertexNormals;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
		public Vector2f[] VertexUV0;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
		public Vector4s[] BlendIndices;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
		public Vector4f[] BlendWeights;
	}

	[Flags]
	public enum SpaceLocationFlags : ulong
	{
		OrientationValid = 1uL,
		PositionValid = 2uL,
		OrientationTracked = 4uL,
		PositionTracked = 8uL
	}

	public struct SpaceLocationf
	{
		public SpaceLocationFlags locationFlags;

		public Posef pose;
	}

	public enum BodyJointSet
	{
		[InspectorName(null)]
		None = -1,
		UpperBody,
		FullBody
	}

	public enum BodyTrackingFidelity2
	{
		Low = 1,
		High
	}

	public enum BodyTrackingCalibrationState
	{
		Valid = 1,
		Calibrating,
		Invalid
	}

	public struct BodyTrackingCalibrationInfo
	{
		public float BodyHeight;
	}

	public struct BodyJointLocation
	{
		public SpaceLocationFlags LocationFlags;

		public Posef Pose;

		public static readonly BodyJointLocation invalid = new BodyJointLocation
		{
			LocationFlags = (SpaceLocationFlags)0uL,
			Pose = Posef.identity
		};

		public bool OrientationValid => (LocationFlags & SpaceLocationFlags.OrientationValid) != 0;

		public bool PositionValid => (LocationFlags & SpaceLocationFlags.PositionValid) != 0;

		public bool OrientationTracked => (LocationFlags & SpaceLocationFlags.OrientationTracked) != 0;

		public bool PositionTracked => (LocationFlags & SpaceLocationFlags.PositionTracked) != 0;
	}

	public struct BodyState
	{
		public BodyJointLocation[] JointLocations;

		public float Confidence;

		public uint SkeletonChangedCount;

		public double Time;

		public BodyJointSet JointSet;

		public BodyTrackingCalibrationState CalibrationStatus;

		public BodyTrackingFidelity2 Fidelity;
	}

	private struct BodyStateInternal
	{
		public Bool IsActive;

		public float Confidence;

		public uint SkeletonChangedCount;

		public double Time;

		public BodyJointLocation JointLocation_0;

		public BodyJointLocation JointLocation_1;

		public BodyJointLocation JointLocation_2;

		public BodyJointLocation JointLocation_3;

		public BodyJointLocation JointLocation_4;

		public BodyJointLocation JointLocation_5;

		public BodyJointLocation JointLocation_6;

		public BodyJointLocation JointLocation_7;

		public BodyJointLocation JointLocation_8;

		public BodyJointLocation JointLocation_9;

		public BodyJointLocation JointLocation_10;

		public BodyJointLocation JointLocation_11;

		public BodyJointLocation JointLocation_12;

		public BodyJointLocation JointLocation_13;

		public BodyJointLocation JointLocation_14;

		public BodyJointLocation JointLocation_15;

		public BodyJointLocation JointLocation_16;

		public BodyJointLocation JointLocation_17;

		public BodyJointLocation JointLocation_18;

		public BodyJointLocation JointLocation_19;

		public BodyJointLocation JointLocation_20;

		public BodyJointLocation JointLocation_21;

		public BodyJointLocation JointLocation_22;

		public BodyJointLocation JointLocation_23;

		public BodyJointLocation JointLocation_24;

		public BodyJointLocation JointLocation_25;

		public BodyJointLocation JointLocation_26;

		public BodyJointLocation JointLocation_27;

		public BodyJointLocation JointLocation_28;

		public BodyJointLocation JointLocation_29;

		public BodyJointLocation JointLocation_30;

		public BodyJointLocation JointLocation_31;

		public BodyJointLocation JointLocation_32;

		public BodyJointLocation JointLocation_33;

		public BodyJointLocation JointLocation_34;

		public BodyJointLocation JointLocation_35;

		public BodyJointLocation JointLocation_36;

		public BodyJointLocation JointLocation_37;

		public BodyJointLocation JointLocation_38;

		public BodyJointLocation JointLocation_39;

		public BodyJointLocation JointLocation_40;

		public BodyJointLocation JointLocation_41;

		public BodyJointLocation JointLocation_42;

		public BodyJointLocation JointLocation_43;

		public BodyJointLocation JointLocation_44;

		public BodyJointLocation JointLocation_45;

		public BodyJointLocation JointLocation_46;

		public BodyJointLocation JointLocation_47;

		public BodyJointLocation JointLocation_48;

		public BodyJointLocation JointLocation_49;

		public BodyJointLocation JointLocation_50;

		public BodyJointLocation JointLocation_51;

		public BodyJointLocation JointLocation_52;

		public BodyJointLocation JointLocation_53;

		public BodyJointLocation JointLocation_54;

		public BodyJointLocation JointLocation_55;

		public BodyJointLocation JointLocation_56;

		public BodyJointLocation JointLocation_57;

		public BodyJointLocation JointLocation_58;

		public BodyJointLocation JointLocation_59;

		public BodyJointLocation JointLocation_60;

		public BodyJointLocation JointLocation_61;

		public BodyJointLocation JointLocation_62;

		public BodyJointLocation JointLocation_63;

		public BodyJointLocation JointLocation_64;

		public BodyJointLocation JointLocation_65;

		public BodyJointLocation JointLocation_66;

		public BodyJointLocation JointLocation_67;

		public BodyJointLocation JointLocation_68;

		public BodyJointLocation JointLocation_69;
	}

	private struct BodyState4Internal
	{
		public Bool IsActive;

		public float Confidence;

		public uint SkeletonChangedCount;

		public double Time;

		public BodyJointLocation JointLocation_0;

		public BodyJointLocation JointLocation_1;

		public BodyJointLocation JointLocation_2;

		public BodyJointLocation JointLocation_3;

		public BodyJointLocation JointLocation_4;

		public BodyJointLocation JointLocation_5;

		public BodyJointLocation JointLocation_6;

		public BodyJointLocation JointLocation_7;

		public BodyJointLocation JointLocation_8;

		public BodyJointLocation JointLocation_9;

		public BodyJointLocation JointLocation_10;

		public BodyJointLocation JointLocation_11;

		public BodyJointLocation JointLocation_12;

		public BodyJointLocation JointLocation_13;

		public BodyJointLocation JointLocation_14;

		public BodyJointLocation JointLocation_15;

		public BodyJointLocation JointLocation_16;

		public BodyJointLocation JointLocation_17;

		public BodyJointLocation JointLocation_18;

		public BodyJointLocation JointLocation_19;

		public BodyJointLocation JointLocation_20;

		public BodyJointLocation JointLocation_21;

		public BodyJointLocation JointLocation_22;

		public BodyJointLocation JointLocation_23;

		public BodyJointLocation JointLocation_24;

		public BodyJointLocation JointLocation_25;

		public BodyJointLocation JointLocation_26;

		public BodyJointLocation JointLocation_27;

		public BodyJointLocation JointLocation_28;

		public BodyJointLocation JointLocation_29;

		public BodyJointLocation JointLocation_30;

		public BodyJointLocation JointLocation_31;

		public BodyJointLocation JointLocation_32;

		public BodyJointLocation JointLocation_33;

		public BodyJointLocation JointLocation_34;

		public BodyJointLocation JointLocation_35;

		public BodyJointLocation JointLocation_36;

		public BodyJointLocation JointLocation_37;

		public BodyJointLocation JointLocation_38;

		public BodyJointLocation JointLocation_39;

		public BodyJointLocation JointLocation_40;

		public BodyJointLocation JointLocation_41;

		public BodyJointLocation JointLocation_42;

		public BodyJointLocation JointLocation_43;

		public BodyJointLocation JointLocation_44;

		public BodyJointLocation JointLocation_45;

		public BodyJointLocation JointLocation_46;

		public BodyJointLocation JointLocation_47;

		public BodyJointLocation JointLocation_48;

		public BodyJointLocation JointLocation_49;

		public BodyJointLocation JointLocation_50;

		public BodyJointLocation JointLocation_51;

		public BodyJointLocation JointLocation_52;

		public BodyJointLocation JointLocation_53;

		public BodyJointLocation JointLocation_54;

		public BodyJointLocation JointLocation_55;

		public BodyJointLocation JointLocation_56;

		public BodyJointLocation JointLocation_57;

		public BodyJointLocation JointLocation_58;

		public BodyJointLocation JointLocation_59;

		public BodyJointLocation JointLocation_60;

		public BodyJointLocation JointLocation_61;

		public BodyJointLocation JointLocation_62;

		public BodyJointLocation JointLocation_63;

		public BodyJointLocation JointLocation_64;

		public BodyJointLocation JointLocation_65;

		public BodyJointLocation JointLocation_66;

		public BodyJointLocation JointLocation_67;

		public BodyJointLocation JointLocation_68;

		public BodyJointLocation JointLocation_69;

		public BodyJointLocation JointLocation_70;

		public BodyJointLocation JointLocation_71;

		public BodyJointLocation JointLocation_72;

		public BodyJointLocation JointLocation_73;

		public BodyJointLocation JointLocation_74;

		public BodyJointLocation JointLocation_75;

		public BodyJointLocation JointLocation_76;

		public BodyJointLocation JointLocation_77;

		public BodyJointLocation JointLocation_78;

		public BodyJointLocation JointLocation_79;

		public BodyJointLocation JointLocation_80;

		public BodyJointLocation JointLocation_81;

		public BodyJointLocation JointLocation_82;

		public BodyJointLocation JointLocation_83;

		public BodyTrackingCalibrationState CalibrationStatus;

		public BodyTrackingFidelity2 Fidelity;
	}

	public struct FaceExpressionStatus
	{
		public bool IsValid;

		public bool IsEyeFollowingBlendshapesValid;
	}

	public struct FaceVisemesState
	{
		public bool IsValid;

		public float[] Visemes;

		public double Time;
	}

	public struct FaceState
	{
		public float[] ExpressionWeights;

		public float[] ExpressionWeightConfidences;

		public FaceExpressionStatus Status;

		public FaceTrackingDataSource DataSource;

		public double Time;
	}

	private struct FaceExpressionStatusInternal
	{
		public Bool IsValid;

		public Bool IsEyeFollowingBlendshapesValid;

		public FaceExpressionStatus ToFaceExpressionStatus()
		{
			FaceExpressionStatus result = default(FaceExpressionStatus);
			result.IsValid = IsValid == Bool.True;
			result.IsEyeFollowingBlendshapesValid = IsEyeFollowingBlendshapesValid == Bool.True;
			return result;
		}
	}

	private struct FaceStateInternal
	{
		public float ExpressionWeights_0;

		public float ExpressionWeights_1;

		public float ExpressionWeights_2;

		public float ExpressionWeights_3;

		public float ExpressionWeights_4;

		public float ExpressionWeights_5;

		public float ExpressionWeights_6;

		public float ExpressionWeights_7;

		public float ExpressionWeights_8;

		public float ExpressionWeights_9;

		public float ExpressionWeights_10;

		public float ExpressionWeights_11;

		public float ExpressionWeights_12;

		public float ExpressionWeights_13;

		public float ExpressionWeights_14;

		public float ExpressionWeights_15;

		public float ExpressionWeights_16;

		public float ExpressionWeights_17;

		public float ExpressionWeights_18;

		public float ExpressionWeights_19;

		public float ExpressionWeights_20;

		public float ExpressionWeights_21;

		public float ExpressionWeights_22;

		public float ExpressionWeights_23;

		public float ExpressionWeights_24;

		public float ExpressionWeights_25;

		public float ExpressionWeights_26;

		public float ExpressionWeights_27;

		public float ExpressionWeights_28;

		public float ExpressionWeights_29;

		public float ExpressionWeights_30;

		public float ExpressionWeights_31;

		public float ExpressionWeights_32;

		public float ExpressionWeights_33;

		public float ExpressionWeights_34;

		public float ExpressionWeights_35;

		public float ExpressionWeights_36;

		public float ExpressionWeights_37;

		public float ExpressionWeights_38;

		public float ExpressionWeights_39;

		public float ExpressionWeights_40;

		public float ExpressionWeights_41;

		public float ExpressionWeights_42;

		public float ExpressionWeights_43;

		public float ExpressionWeights_44;

		public float ExpressionWeights_45;

		public float ExpressionWeights_46;

		public float ExpressionWeights_47;

		public float ExpressionWeights_48;

		public float ExpressionWeights_49;

		public float ExpressionWeights_50;

		public float ExpressionWeights_51;

		public float ExpressionWeights_52;

		public float ExpressionWeights_53;

		public float ExpressionWeights_54;

		public float ExpressionWeights_55;

		public float ExpressionWeights_56;

		public float ExpressionWeights_57;

		public float ExpressionWeights_58;

		public float ExpressionWeights_59;

		public float ExpressionWeights_60;

		public float ExpressionWeights_61;

		public float ExpressionWeights_62;

		public float ExpressionWeightConfidences_0;

		public float ExpressionWeightConfidences_1;

		public FaceExpressionStatusInternal Status;

		public double Time;
	}

	private struct FaceState2Internal
	{
		public float ExpressionWeights_0;

		public float ExpressionWeights_1;

		public float ExpressionWeights_2;

		public float ExpressionWeights_3;

		public float ExpressionWeights_4;

		public float ExpressionWeights_5;

		public float ExpressionWeights_6;

		public float ExpressionWeights_7;

		public float ExpressionWeights_8;

		public float ExpressionWeights_9;

		public float ExpressionWeights_10;

		public float ExpressionWeights_11;

		public float ExpressionWeights_12;

		public float ExpressionWeights_13;

		public float ExpressionWeights_14;

		public float ExpressionWeights_15;

		public float ExpressionWeights_16;

		public float ExpressionWeights_17;

		public float ExpressionWeights_18;

		public float ExpressionWeights_19;

		public float ExpressionWeights_20;

		public float ExpressionWeights_21;

		public float ExpressionWeights_22;

		public float ExpressionWeights_23;

		public float ExpressionWeights_24;

		public float ExpressionWeights_25;

		public float ExpressionWeights_26;

		public float ExpressionWeights_27;

		public float ExpressionWeights_28;

		public float ExpressionWeights_29;

		public float ExpressionWeights_30;

		public float ExpressionWeights_31;

		public float ExpressionWeights_32;

		public float ExpressionWeights_33;

		public float ExpressionWeights_34;

		public float ExpressionWeights_35;

		public float ExpressionWeights_36;

		public float ExpressionWeights_37;

		public float ExpressionWeights_38;

		public float ExpressionWeights_39;

		public float ExpressionWeights_40;

		public float ExpressionWeights_41;

		public float ExpressionWeights_42;

		public float ExpressionWeights_43;

		public float ExpressionWeights_44;

		public float ExpressionWeights_45;

		public float ExpressionWeights_46;

		public float ExpressionWeights_47;

		public float ExpressionWeights_48;

		public float ExpressionWeights_49;

		public float ExpressionWeights_50;

		public float ExpressionWeights_51;

		public float ExpressionWeights_52;

		public float ExpressionWeights_53;

		public float ExpressionWeights_54;

		public float ExpressionWeights_55;

		public float ExpressionWeights_56;

		public float ExpressionWeights_57;

		public float ExpressionWeights_58;

		public float ExpressionWeights_59;

		public float ExpressionWeights_60;

		public float ExpressionWeights_61;

		public float ExpressionWeights_62;

		public float ExpressionWeights_63;

		public float ExpressionWeights_64;

		public float ExpressionWeights_65;

		public float ExpressionWeights_66;

		public float ExpressionWeights_67;

		public float ExpressionWeights_68;

		public float ExpressionWeights_69;

		public float ExpressionWeightConfidences_0;

		public float ExpressionWeightConfidences_1;

		public FaceExpressionStatusInternal Status;

		public FaceTrackingDataSource DataSource;

		public double Time;
	}

	private struct FaceVisemesStateInternal
	{
		public Bool IsValid;

		public float Visemes_0;

		public float Visemes_1;

		public float Visemes_2;

		public float Visemes_3;

		public float Visemes_4;

		public float Visemes_5;

		public float Visemes_6;

		public float Visemes_7;

		public float Visemes_8;

		public float Visemes_9;

		public float Visemes_10;

		public float Visemes_11;

		public float Visemes_12;

		public float Visemes_13;

		public float Visemes_14;

		public double Time;
	}

	public enum FaceRegionConfidence
	{
		Lower,
		Upper,
		Max
	}

	public enum FaceExpression
	{
		Invalid = -1,
		Brow_Lowerer_L,
		Brow_Lowerer_R,
		Cheek_Puff_L,
		Cheek_Puff_R,
		Cheek_Raiser_L,
		Cheek_Raiser_R,
		Cheek_Suck_L,
		Cheek_Suck_R,
		Chin_Raiser_B,
		Chin_Raiser_T,
		Dimpler_L,
		Dimpler_R,
		Eyes_Closed_L,
		Eyes_Closed_R,
		Eyes_Look_Down_L,
		Eyes_Look_Down_R,
		Eyes_Look_Left_L,
		Eyes_Look_Left_R,
		Eyes_Look_Right_L,
		Eyes_Look_Right_R,
		Eyes_Look_Up_L,
		Eyes_Look_Up_R,
		Inner_Brow_Raiser_L,
		Inner_Brow_Raiser_R,
		Jaw_Drop,
		Jaw_Sideways_Left,
		Jaw_Sideways_Right,
		Jaw_Thrust,
		Lid_Tightener_L,
		Lid_Tightener_R,
		Lip_Corner_Depressor_L,
		Lip_Corner_Depressor_R,
		Lip_Corner_Puller_L,
		Lip_Corner_Puller_R,
		Lip_Funneler_LB,
		Lip_Funneler_LT,
		Lip_Funneler_RB,
		Lip_Funneler_RT,
		Lip_Pressor_L,
		Lip_Pressor_R,
		Lip_Pucker_L,
		Lip_Pucker_R,
		Lip_Stretcher_L,
		Lip_Stretcher_R,
		Lip_Suck_LB,
		Lip_Suck_LT,
		Lip_Suck_RB,
		Lip_Suck_RT,
		Lip_Tightener_L,
		Lip_Tightener_R,
		Lips_Toward,
		Lower_Lip_Depressor_L,
		Lower_Lip_Depressor_R,
		Mouth_Left,
		Mouth_Right,
		Nose_Wrinkler_L,
		Nose_Wrinkler_R,
		Outer_Brow_Raiser_L,
		Outer_Brow_Raiser_R,
		Upper_Lid_Raiser_L,
		Upper_Lid_Raiser_R,
		Upper_Lip_Raiser_L,
		Upper_Lip_Raiser_R,
		Max
	}

	public enum FaceExpression2
	{
		Invalid = -1,
		Brow_Lowerer_L,
		Brow_Lowerer_R,
		Cheek_Puff_L,
		Cheek_Puff_R,
		Cheek_Raiser_L,
		Cheek_Raiser_R,
		Cheek_Suck_L,
		Cheek_Suck_R,
		Chin_Raiser_B,
		Chin_Raiser_T,
		Dimpler_L,
		Dimpler_R,
		Eyes_Closed_L,
		Eyes_Closed_R,
		Eyes_Look_Down_L,
		Eyes_Look_Down_R,
		Eyes_Look_Left_L,
		Eyes_Look_Left_R,
		Eyes_Look_Right_L,
		Eyes_Look_Right_R,
		Eyes_Look_Up_L,
		Eyes_Look_Up_R,
		Inner_Brow_Raiser_L,
		Inner_Brow_Raiser_R,
		Jaw_Drop,
		Jaw_Sideways_Left,
		Jaw_Sideways_Right,
		Jaw_Thrust,
		Lid_Tightener_L,
		Lid_Tightener_R,
		Lip_Corner_Depressor_L,
		Lip_Corner_Depressor_R,
		Lip_Corner_Puller_L,
		Lip_Corner_Puller_R,
		Lip_Funneler_LB,
		Lip_Funneler_LT,
		Lip_Funneler_RB,
		Lip_Funneler_RT,
		Lip_Pressor_L,
		Lip_Pressor_R,
		Lip_Pucker_L,
		Lip_Pucker_R,
		Lip_Stretcher_L,
		Lip_Stretcher_R,
		Lip_Suck_LB,
		Lip_Suck_LT,
		Lip_Suck_RB,
		Lip_Suck_RT,
		Lip_Tightener_L,
		Lip_Tightener_R,
		Lips_Toward,
		Lower_Lip_Depressor_L,
		Lower_Lip_Depressor_R,
		Mouth_Left,
		Mouth_Right,
		Nose_Wrinkler_L,
		Nose_Wrinkler_R,
		Outer_Brow_Raiser_L,
		Outer_Brow_Raiser_R,
		Upper_Lid_Raiser_L,
		Upper_Lid_Raiser_R,
		Upper_Lip_Raiser_L,
		Upper_Lip_Raiser_R,
		Tongue_Tip_Interdental,
		Tongue_Tip_Alveolar,
		Tongue_Front_Dorsal_Palate,
		Tongue_Mid_Dorsal_Palate,
		Tongue_Back_Dorsal_Velar,
		Tongue_Out,
		Tongue_Retreat,
		Max
	}

	public enum FaceTrackingDataSource
	{
		Visual,
		Audio,
		Count
	}

	public enum FaceViseme
	{
		Invalid = -1,
		SIL,
		PP,
		FF,
		TH,
		DD,
		KK,
		CH,
		SS,
		NN,
		RR,
		AA,
		E,
		IH,
		OH,
		OU,
		Count
	}

	public enum FaceConstants
	{
		MaxFaceExpressions = 63,
		MaxFaceRegionConfidences = 2,
		MaxFaceExpressions2 = 70,
		FaceVisemesCount = 15
	}

	public struct EyeGazeState
	{
		public Posef Pose;

		public float Confidence;

		internal Bool _isValid;

		public bool IsValid => _isValid == Bool.True;
	}

	public struct EyeGazesState
	{
		public EyeGazeState[] EyeGazes;

		public double Time;
	}

	private struct EyeGazesStateInternal
	{
		public EyeGazeState EyeGazes_0;

		public EyeGazeState EyeGazes_1;

		public double Time;
	}

	public enum ColorSpace
	{
		Unknown,
		Unmanaged,
		Rec_2020,
		Rec_709,
		Rift_CV1,
		Rift_S,
		Quest,
		P3,
		Adobe_RGB
	}

	public enum EventType
	{
		Unknown = 0,
		DisplayRefreshRateChanged = 1,
		SpatialAnchorCreateComplete = 49,
		SpaceSetComponentStatusComplete = 50,
		SpaceQueryResults = 51,
		SpaceQueryComplete = 52,
		SpaceSaveComplete = 53,
		SpaceEraseComplete = 54,
		SpaceShareResult = 56,
		SpaceListSaveResult = 57,
		SpaceShareToGroupsComplete = 58,
		SceneCaptureComplete = 100,
		VirtualKeyboardCommitText = 201,
		VirtualKeyboardBackspace = 202,
		VirtualKeyboardEnter = 203,
		VirtualKeyboardShown = 204,
		VirtualKeyboardHidden = 205,
		SpaceDiscoveryResultsAvailable = 300,
		SpaceDiscoveryComplete = 301,
		SpacesSaveResult = 302,
		SpacesEraseResult = 303,
		ColocationSessionStartAdvertisementComplete = 370,
		ColocationSessionAdvertisementComplete = 371,
		ColocationSessionStopAdvertisementComplete = 372,
		ColocationSessionStartDiscoveryComplete = 373,
		ColocationSessionDiscoveryResult = 374,
		ColocationSessionDiscoveryComplete = 375,
		ColocationSessionStopDiscoveryComplete = 376,
		PassthroughLayerResumed = 500,
		BoundaryVisibilityChanged = 510,
		CreateDynamicObjectTrackerResult = 650,
		SetDynamicObjectTrackedClassesResult = 651,
		ReferenceSpaceChangePending = 1160
	}

	public struct EventDataBuffer
	{
		public EventType EventType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4000)]
		public byte[] EventData;
	}

	public struct RenderModelProperties
	{
		public string ModelName;

		public ulong ModelKey;

		public uint VendorId;

		public uint ModelVersion;
	}

	private struct RenderModelPropertiesInternal
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] ModelName;

		public ulong ModelKey;

		public uint VendorId;

		public uint ModelVersion;
	}

	[Flags]
	public enum RenderModelFlags
	{
		SupportsGltf20Subset1 = 1,
		SupportsGltf20Subset2 = 2
	}

	public enum VirtualKeyboardLocationType
	{
		Custom,
		Far,
		Direct
	}

	public struct VirtualKeyboardSpaceCreateInfo
	{
		public VirtualKeyboardLocationType locationType;

		public Posef pose;

		public TrackingOrigin trackingOriginType;
	}

	public struct VirtualKeyboardLocationInfo
	{
		public VirtualKeyboardLocationType locationType;

		public Posef pose;

		public float scale;

		public TrackingOrigin trackingOriginType;
	}

	public struct VirtualKeyboardCreateInfo
	{
	}

	public enum VirtualKeyboardInputSource
	{
		Invalid = 0,
		ControllerRayLeft = 1,
		ControllerRayRight = 2,
		HandRayLeft = 3,
		HandRayRight = 4,
		ControllerDirectLeft = 5,
		ControllerDirectRight = 6,
		HandDirectIndexTipLeft = 7,
		HandDirectIndexTipRight = 8,
		EnumSize = int.MaxValue
	}

	[Flags]
	public enum VirtualKeyboardInputStateFlags : ulong
	{
		IsPressed = 1uL
	}

	public struct VirtualKeyboardInputInfo
	{
		public VirtualKeyboardInputSource inputSource;

		public Posef inputPose;

		public VirtualKeyboardInputStateFlags inputState;

		public TrackingOrigin inputTrackingOriginType;
	}

	public struct VirtualKeyboardModelAnimationState
	{
		public int AnimationIndex;

		public float Fraction;
	}

	public struct VirtualKeyboardModelAnimationStates
	{
		public VirtualKeyboardModelAnimationState[] States;
	}

	public struct VirtualKeyboardModelAnimationStatesInternal
	{
		public uint StateCapacityInput;

		public uint StateCountOutput;

		public IntPtr StatesBuffer;
	}

	public struct VirtualKeyboardTextureIds
	{
		public ulong[] TextureIds;
	}

	public struct VirtualKeyboardTextureIdsInternal
	{
		public uint TextureIdCapacityInput;

		public uint TextureIdCountOutput;

		public IntPtr TextureIdsBuffer;
	}

	public struct VirtualKeyboardTextureData
	{
		public uint TextureWidth;

		public uint TextureHeight;

		public uint BufferCapacityInput;

		public uint BufferCountOutput;

		public IntPtr Buffer;
	}

	public struct VirtualKeyboardModelVisibility
	{
		internal Bool _visible;

		public bool Visible
		{
			get
			{
				return _visible == Bool.True;
			}
			set
			{
				_visible = (value ? Bool.True : Bool.False);
			}
		}
	}

	public enum InsightPassthroughColorMapType
	{
		None = 0,
		MonoToRgba = 1,
		MonoToMono = 2,
		BrightnessContrastSaturation = 4,
		ColorLut = 6,
		InterpolatedColorLut = 7
	}

	public enum InsightPassthroughStyleFlags
	{
		HasTextureOpacityFactor = 1,
		HasEdgeColor = 2,
		HasTextureColorMap = 4
	}

	public struct InsightPassthroughStyle
	{
		public InsightPassthroughStyleFlags Flags;

		public float TextureOpacityFactor;

		public Colorf EdgeColor;

		public InsightPassthroughColorMapType TextureColorMapType;

		public uint TextureColorMapDataSize;

		public IntPtr TextureColorMapData;
	}

	public struct InsightPassthroughStyle2
	{
		public InsightPassthroughStyleFlags Flags;

		public float TextureOpacityFactor;

		public Colorf EdgeColor;

		public InsightPassthroughColorMapType TextureColorMapType;

		public uint TextureColorMapDataSize;

		public IntPtr TextureColorMapData;

		public ulong LutSource;

		public ulong LutTarget;

		public float LutWeight;

		public void CopyTo(ref InsightPassthroughStyle target)
		{
			target.Flags = Flags;
			target.TextureOpacityFactor = TextureOpacityFactor;
			target.EdgeColor = EdgeColor;
			target.TextureColorMapType = TextureColorMapType;
			target.TextureColorMapDataSize = TextureColorMapDataSize;
			target.TextureColorMapData = TextureColorMapData;
		}
	}

	public enum PassthroughColorLutChannels
	{
		Rgb = 1,
		Rgba
	}

	public struct PassthroughColorLutData
	{
		public uint BufferSize;

		public IntPtr Buffer;
	}

	public struct InsightPassthroughKeyboardHandsIntensity
	{
		public float LeftHandIntensity;

		public float RightHandIntensity;
	}

	public enum PassthroughCapabilityFlags
	{
		Passthrough = 1,
		Color = 2,
		Depth = 4
	}

	public enum PassthroughCapabilityFields
	{
		Flags = 1,
		MaxColorLutResolution
	}

	public struct PassthroughCapabilities
	{
		public PassthroughCapabilityFields Fields;

		public PassthroughCapabilityFlags Flags;

		public uint MaxColorLutResolution;
	}

	public enum SpaceComponentType
	{
		Locatable = 0,
		Storable = 1,
		Sharable = 2,
		Bounded2D = 3,
		Bounded3D = 4,
		SemanticLabels = 5,
		RoomLayout = 6,
		SpaceContainer = 7,
		MarkerPayload = 1000576000,
		TriangleMesh = 1000269000,
		DynamicObject = 1000288007
	}

	public enum SpaceStorageLocation
	{
		Invalid,
		Local,
		Cloud
	}

	public enum SpaceStoragePersistenceMode
	{
		Invalid,
		Indefinite
	}

	public enum SpaceQueryActionType
	{
		Load
	}

	public enum SpaceQueryType
	{
		Action
	}

	public enum SpaceQueryFilterType
	{
		None,
		Ids,
		Components,
		Group
	}

	public struct SpatialAnchorCreateInfo
	{
		public TrackingOrigin BaseTracking;

		public Posef PoseInSpace;

		public double Time;
	}

	public struct SpaceFilterInfoIds
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
		public Guid[] Ids;

		public int NumIds;
	}

	public struct SpaceFilterInfoComponents
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public SpaceComponentType[] Components;

		public int NumComponents;
	}

	public struct SpaceQueryInfo
	{
		public SpaceQueryType QueryType;

		public int MaxQuerySpaces;

		public double Timeout;

		public SpaceStorageLocation Location;

		public SpaceQueryActionType ActionType;

		public SpaceQueryFilterType FilterType;

		public SpaceFilterInfoIds IdInfo;

		public SpaceFilterInfoComponents ComponentsInfo;
	}

	public struct SpaceQueryInfo2
	{
		public SpaceQueryType QueryType;

		public int MaxQuerySpaces;

		public double Timeout;

		public SpaceStorageLocation Location;

		public SpaceQueryActionType ActionType;

		public SpaceQueryFilterType FilterType;

		public SpaceFilterInfoIds IdInfo;

		public SpaceFilterInfoComponents ComponentsInfo;

		public Guid GroupUuidInfo;
	}

	public struct SpaceQueryResult
	{
		public ulong space;

		public Guid uuid;
	}

	public struct ColocationSessionStartAdvertisementInfo
	{
		public uint PeerMetadataCount;

		public unsafe byte* GroupMetadata;
	}

	public enum ShareSpacesRecipientType
	{
		Group = 1
	}

	public struct ShareSpacesRecipientInfoBase
	{
	}

	public struct ShareSpacesInfo
	{
		public ShareSpacesRecipientType RecipientType;

		public unsafe ShareSpacesRecipientInfoBase* RecipientInfo;

		public uint SpaceCount;

		public unsafe ulong* Spaces;
	}

	public struct ShareSpacesGroupRecipientInfo
	{
		public uint GroupCount;

		public unsafe Guid* GroupUuids;
	}

	public enum SpaceDiscoveryFilterType
	{
		None = 0,
		Ids = 2,
		Component = 3
	}

	public class Media
	{
		public enum MrcActivationMode
		{
			Automatic = 0,
			Disabled = 1,
			EnumSize = int.MaxValue
		}

		public enum PlatformCameraMode
		{
			Disabled = -1,
			Initialized = 0,
			UserControlled = 1,
			SmartNavigated = 2,
			StabilizedPoV = 3,
			RemoteDroneControlled = 4,
			RemoteSpatialMapped = 5,
			SpectatorMode = 6,
			MobileMRC = 7,
			EnumSize = int.MaxValue
		}

		public enum InputVideoBufferType
		{
			Memory = 0,
			TextureHandle = 1,
			EnumSize = int.MaxValue
		}

		private static Texture2D cachedTexture;

		public static bool Initialize()
		{
			if (version >= OVRP_1_38_0.version)
			{
				return OVRP_1_38_0.ovrp_Media_Initialize() == Result.Success;
			}
			return false;
		}

		public static bool Shutdown()
		{
			if (version >= OVRP_1_38_0.version)
			{
				return OVRP_1_38_0.ovrp_Media_Shutdown() == Result.Success;
			}
			return false;
		}

		public static bool GetInitialized()
		{
			if (version >= OVRP_1_38_0.version)
			{
				Bool initialized = Bool.False;
				if (OVRP_1_38_0.ovrp_Media_GetInitialized(out initialized) == Result.Success)
				{
					if (initialized != Bool.True)
					{
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool Update()
		{
			if (version >= OVRP_1_38_0.version)
			{
				return OVRP_1_38_0.ovrp_Media_Update() == Result.Success;
			}
			return false;
		}

		public static MrcActivationMode GetMrcActivationMode()
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_GetMrcActivationMode(out var activationMode) == Result.Success)
				{
					return activationMode;
				}
				return MrcActivationMode.Automatic;
			}
			return MrcActivationMode.Automatic;
		}

		public static bool SetMrcActivationMode(MrcActivationMode mode)
		{
			if (version >= OVRP_1_38_0.version)
			{
				return OVRP_1_38_0.ovrp_Media_SetMrcActivationMode(mode) == Result.Success;
			}
			return false;
		}

		public static bool SetPlatformInitialized()
		{
			if (version >= OVRP_1_54_0.version)
			{
				return OVRP_1_54_0.ovrp_Media_SetPlatformInitialized() == Result.Success;
			}
			return false;
		}

		public static PlatformCameraMode GetPlatformCameraMode()
		{
			if (version >= OVRP_1_57_0.version)
			{
				if (OVRP_1_57_0.ovrp_Media_GetPlatformCameraMode(out var platformCameraMode) == Result.Success)
				{
					return platformCameraMode;
				}
				return PlatformCameraMode.Initialized;
			}
			return PlatformCameraMode.Initialized;
		}

		public static bool SetPlatformCameraMode(PlatformCameraMode mode)
		{
			if (version >= OVRP_1_57_0.version)
			{
				return OVRP_1_57_0.ovrp_Media_SetPlatformCameraMode(mode) == Result.Success;
			}
			return false;
		}

		public static bool IsMrcEnabled()
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_IsMrcEnabled(out var mrcEnabled) == Result.Success)
				{
					if (mrcEnabled != Bool.True)
					{
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool IsMrcActivated()
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_IsMrcActivated(out var mrcActivated) == Result.Success)
				{
					if (mrcActivated != Bool.True)
					{
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool UseMrcDebugCamera()
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_UseMrcDebugCamera(out var useMrcDebugCamera) == Result.Success)
				{
					if (useMrcDebugCamera != Bool.True)
					{
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool SetMrcInputVideoBufferType(InputVideoBufferType videoBufferType)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_SetMrcInputVideoBufferType(videoBufferType) == Result.Success)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static InputVideoBufferType GetMrcInputVideoBufferType()
		{
			if (version >= OVRP_1_38_0.version)
			{
				InputVideoBufferType inputVideoBufferType = InputVideoBufferType.Memory;
				OVRP_1_38_0.ovrp_Media_GetMrcInputVideoBufferType(ref inputVideoBufferType);
				return inputVideoBufferType;
			}
			return InputVideoBufferType.Memory;
		}

		public static bool SetMrcFrameSize(int frameWidth, int frameHeight)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_SetMrcFrameSize(frameWidth, frameHeight) == Result.Success)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static void GetMrcFrameSize(out int frameWidth, out int frameHeight)
		{
			frameWidth = -1;
			frameHeight = -1;
			if (version >= OVRP_1_38_0.version)
			{
				OVRP_1_38_0.ovrp_Media_GetMrcFrameSize(ref frameWidth, ref frameHeight);
			}
		}

		public static bool SetMrcAudioSampleRate(int sampleRate)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_SetMrcAudioSampleRate(sampleRate) == Result.Success)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static int GetMrcAudioSampleRate()
		{
			int sampleRate = 0;
			if (version >= OVRP_1_38_0.version)
			{
				OVRP_1_38_0.ovrp_Media_GetMrcAudioSampleRate(ref sampleRate);
			}
			return sampleRate;
		}

		public static bool SetMrcFrameImageFlipped(bool imageFlipped)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (OVRP_1_38_0.ovrp_Media_SetMrcFrameImageFlipped(imageFlipped ? Bool.True : Bool.False) == Result.Success)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool GetMrcFrameImageFlipped()
		{
			Bool flipped = Bool.False;
			if (version >= OVRP_1_38_0.version)
			{
				OVRP_1_38_0.ovrp_Media_GetMrcFrameImageFlipped(ref flipped);
			}
			if (flipped != Bool.True)
			{
				return false;
			}
			return true;
		}

		public static bool EncodeMrcFrame(IntPtr textureHandle, IntPtr fgTextureHandle, float[] audioData, int audioFrames, int audioChannels, double timestamp, double poseTime, ref int outSyncId)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (textureHandle == IntPtr.Zero)
				{
					Debug.LogError("EncodeMrcFrame: textureHandle is null");
					return false;
				}
				if (GetMrcInputVideoBufferType() != InputVideoBufferType.TextureHandle)
				{
					Debug.LogError("EncodeMrcFrame: videoBufferType mismatch");
					return false;
				}
				GCHandle gCHandle = default(GCHandle);
				IntPtr intPtr = IntPtr.Zero;
				int audioDataLen = 0;
				if (audioData != null)
				{
					gCHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
					intPtr = gCHandle.AddrOfPinnedObject();
					audioDataLen = audioFrames * 4;
				}
				Result result = ((fgTextureHandle == IntPtr.Zero) ? ((!(version >= OVRP_1_49_0.version)) ? OVRP_1_38_0.ovrp_Media_EncodeMrcFrame(textureHandle, intPtr, audioDataLen, audioChannels, timestamp, ref outSyncId) : OVRP_1_49_0.ovrp_Media_EncodeMrcFrameWithPoseTime(textureHandle, intPtr, audioDataLen, audioChannels, timestamp, poseTime, ref outSyncId)) : ((!(version >= OVRP_1_49_0.version)) ? OVRP_1_38_0.ovrp_Media_EncodeMrcFrameWithDualTextures(textureHandle, fgTextureHandle, intPtr, audioDataLen, audioChannels, timestamp, ref outSyncId) : OVRP_1_49_0.ovrp_Media_EncodeMrcFrameDualTexturesWithPoseTime(textureHandle, fgTextureHandle, intPtr, audioDataLen, audioChannels, timestamp, poseTime, ref outSyncId)));
				if (audioData != null)
				{
					gCHandle.Free();
				}
				return result == Result.Success;
			}
			return false;
		}

		public static bool EncodeMrcFrame(RenderTexture frame, float[] audioData, int audioFrames, int audioChannels, double timestamp, double poseTime, ref int outSyncId)
		{
			if (version >= OVRP_1_38_0.version)
			{
				if (frame == null)
				{
					Debug.LogError("EncodeMrcFrame: frame is null");
					return false;
				}
				if (GetMrcInputVideoBufferType() != 0)
				{
					Debug.LogError("EncodeMrcFrame: videoBufferType mismatch");
					return false;
				}
				GCHandle gCHandle = default(GCHandle);
				IntPtr zero = IntPtr.Zero;
				if (cachedTexture == null || cachedTexture.width != frame.width || cachedTexture.height != frame.height)
				{
					cachedTexture = new Texture2D(frame.width, frame.height, TextureFormat.ARGB32, mipChain: false);
				}
				RenderTexture active = RenderTexture.active;
				RenderTexture.active = frame;
				cachedTexture.ReadPixels(new Rect(0f, 0f, frame.width, frame.height), 0, 0);
				RenderTexture.active = active;
				gCHandle = GCHandle.Alloc(cachedTexture.GetPixels32(0), GCHandleType.Pinned);
				zero = gCHandle.AddrOfPinnedObject();
				GCHandle gCHandle2 = default(GCHandle);
				IntPtr audioDataPtr = IntPtr.Zero;
				int audioDataLen = 0;
				if (audioData != null)
				{
					gCHandle2 = GCHandle.Alloc(audioData, GCHandleType.Pinned);
					audioDataPtr = gCHandle2.AddrOfPinnedObject();
					audioDataLen = audioFrames * 4;
				}
				Result result = ((!(version >= OVRP_1_49_0.version)) ? OVRP_1_38_0.ovrp_Media_EncodeMrcFrame(zero, audioDataPtr, audioDataLen, audioChannels, timestamp, ref outSyncId) : OVRP_1_49_0.ovrp_Media_EncodeMrcFrameWithPoseTime(zero, audioDataPtr, audioDataLen, audioChannels, timestamp, poseTime, ref outSyncId));
				gCHandle.Free();
				if (audioData != null)
				{
					gCHandle2.Free();
				}
				return result == Result.Success;
			}
			return false;
		}

		public static bool SyncMrcFrame(int syncId)
		{
			if (version >= OVRP_1_38_0.version)
			{
				return OVRP_1_38_0.ovrp_Media_SyncMrcFrame(syncId) == Result.Success;
			}
			return false;
		}

		public static bool SetAvailableQueueIndexVulkan(uint queueIndexVk)
		{
			if (version >= OVRP_1_45_0.version)
			{
				return OVRP_1_45_0.ovrp_Media_SetAvailableQueueIndexVulkan(queueIndexVk) == Result.Success;
			}
			return false;
		}

		public static bool SetMrcHeadsetControllerPose(Posef headsetPose, Posef leftControllerPose, Posef rightControllerPose)
		{
			if (version >= OVRP_1_49_0.version)
			{
				return OVRP_1_49_0.ovrp_Media_SetHeadsetControllerPose(headsetPose, leftControllerPose, rightControllerPose) == Result.Success;
			}
			return false;
		}

		public static bool IsCastingToRemoteClient()
		{
			if (version >= OVRP_1_66_0.version)
			{
				Bool isCasting = Bool.False;
				if (OVRP_1_66_0.ovrp_Media_IsCastingToRemoteClient(out isCasting) == Result.Success)
				{
					return isCasting == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	private delegate Bone GetBoneSkeleton2Delegate();

	private delegate Bone GetBoneSkeleton3Delegate();

	public delegate IntPtr VirtualKeyboardModelAnimationStateBufferProvider(int minimumBufferLength, int stateCount);

	public delegate void VirtualKeyboardModelAnimationStateHandler(ref VirtualKeyboardModelAnimationState state);

	public delegate void OpenXREventDelegateType(IntPtr data, IntPtr context);

	private struct SpaceContainerInternal
	{
		public int uuidCapacityInput;

		public int uuidCountOutput;

		public IntPtr uuids;
	}

	private struct SpaceSemanticLabelInternal
	{
		public int byteCapacityInput;

		public int byteCountOutput;

		public IntPtr labels;
	}

	public struct RoomLayout
	{
		public Guid floorUuid;

		public Guid ceilingUuid;

		public Guid[] wallUuids;
	}

	internal struct RoomLayoutInternal
	{
		public Guid floorUuid;

		public Guid ceilingUuid;

		public int wallUuidCapacityInput;

		public int wallUuidCountOutput;

		public IntPtr wallUuids;
	}

	private struct PolygonalBoundary2DInternal
	{
		public int vertexCapacityInput;

		public int vertexCountOutput;

		public IntPtr vertices;
	}

	private struct SceneCaptureRequestInternal
	{
		public int requestByteCount;

		[MarshalAs(UnmanagedType.LPStr)]
		public string request;
	}

	private struct PinnedArray<T> : IDisposable where T : unmanaged
	{
		private GCHandle _handle;

		public PinnedArray(T[] array)
		{
			_handle = GCHandle.Alloc(array, GCHandleType.Pinned);
		}

		public void Dispose()
		{
			_handle.Free();
		}

		public static implicit operator IntPtr(PinnedArray<T> pinnedArray)
		{
			return pinnedArray._handle.AddrOfPinnedObject();
		}
	}

	public struct SpaceDiscoveryResult
	{
		public ulong Space;

		public Guid Uuid;
	}

	public struct SpaceDiscoveryResults
	{
		public uint ResultCapacityInput;

		public uint ResultCountOutput;

		public unsafe SpaceDiscoveryResult* Results;
	}

	public struct SpaceDiscoveryFilterInfoHeader
	{
		public SpaceDiscoveryFilterType Type;
	}

	public struct SpaceDiscoveryFilterInfoIds
	{
		public SpaceDiscoveryFilterType Type;

		public int NumIds;

		public unsafe Guid* Ids;
	}

	public struct SpaceDiscoveryFilterInfoComponents
	{
		public SpaceDiscoveryFilterType Type;

		public SpaceComponentType Component;
	}

	public struct SpaceDiscoveryInfo
	{
		public uint NumFilters;

		public unsafe SpaceDiscoveryFilterInfoHeader** Filters;
	}

	private struct TriangleMeshInternal
	{
		public int vertexCapacityInput;

		public int vertexCountOutput;

		public IntPtr vertices;

		public int indexCapacityInput;

		public int indexCountOutput;

		public IntPtr indices;
	}

	[Flags]
	public enum PassthroughPreferenceFields
	{
		Flags = 1
	}

	[Flags]
	public enum PassthroughPreferenceFlags : long
	{
		DefaultToActive = 1L
	}

	public struct PassthroughPreferences
	{
		public PassthroughPreferenceFields Fields;

		public PassthroughPreferenceFlags Flags;
	}

	public class Ktx
	{
		public static IntPtr LoadKtxFromMemory(IntPtr dataPtr, uint length)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return IntPtr.Zero;
			}
			if (version >= OVRP_1_65_0.version)
			{
				IntPtr texture = IntPtr.Zero;
				OVRP_1_65_0.ovrp_KtxLoadFromMemory(ref dataPtr, length, ref texture);
				return texture;
			}
			return IntPtr.Zero;
		}

		public static uint GetKtxTextureWidth(IntPtr texture)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return 0u;
			}
			if (version >= OVRP_1_65_0.version)
			{
				uint width = 0u;
				OVRP_1_65_0.ovrp_KtxTextureWidth(texture, ref width);
				return width;
			}
			return 0u;
		}

		public static uint GetKtxTextureHeight(IntPtr texture)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return 0u;
			}
			if (version >= OVRP_1_65_0.version)
			{
				uint height = 0u;
				OVRP_1_65_0.ovrp_KtxTextureHeight(texture, ref height);
				return height;
			}
			return 0u;
		}

		public static bool TranscodeKtxTexture(IntPtr texture, uint format)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return false;
			}
			if (version >= OVRP_1_65_0.version)
			{
				return OVRP_1_65_0.ovrp_KtxTranscode(texture, format) == Result.Success;
			}
			return false;
		}

		public static uint GetKtxTextureSize(IntPtr texture)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return 0u;
			}
			if (version >= OVRP_1_65_0.version)
			{
				uint size = 0u;
				OVRP_1_65_0.ovrp_KtxTextureSize(texture, ref size);
				return size;
			}
			return 0u;
		}

		public static bool GetKtxTextureData(IntPtr texture, IntPtr textureData, uint bufferSize)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return false;
			}
			if (version >= OVRP_1_65_0.version)
			{
				return OVRP_1_65_0.ovrp_KtxGetTextureData(texture, textureData, bufferSize) == Result.Success;
			}
			return false;
		}

		public static bool DestroyKtxTexture(IntPtr texture)
		{
			if (nativeXrApi != XrApi.OpenXR)
			{
				Debug.LogWarning("KTX features are only supported in OpenXR.");
				return false;
			}
			if (version >= OVRP_1_65_0.version)
			{
				return OVRP_1_65_0.ovrp_KtxDestroy(texture) == Result.Success;
			}
			return false;
		}
	}

	public enum BoundaryVisibility
	{
		NotSuppressed = 1,
		Suppressed
	}

	public enum DynamicObjectClass
	{
		None = 0,
		Keyboard = 1000587000
	}

	public struct DynamicObjectTrackedClassesSetInfo
	{
		public unsafe DynamicObjectClass* Classes;

		public uint ClassCount;
	}

	public struct DynamicObjectData
	{
		public DynamicObjectClass ClassType;
	}

	public class UnityOpenXR
	{
		public static bool Enabled;

		public static void SetClientVersion()
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_SetClientVersion(wrapperVersion.Major, wrapperVersion.Minor, wrapperVersion.Build);
			}
		}

		public static IntPtr HookGetInstanceProcAddr(IntPtr func)
		{
			if (version >= OVRP_1_71_0.version)
			{
				return OVRP_1_71_0.ovrp_UnityOpenXR_HookGetInstanceProcAddr(func);
			}
			return func;
		}

		public static bool OnInstanceCreate(ulong xrInstance)
		{
			if (version >= OVRP_1_71_0.version)
			{
				return OVRP_1_71_0.ovrp_UnityOpenXR_OnInstanceCreate(xrInstance) == Result.Success;
			}
			return false;
		}

		public static void OnInstanceDestroy(ulong xrInstance)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnInstanceDestroy(xrInstance);
			}
		}

		public static void OnSessionCreate(ulong xrSession)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionCreate(xrSession);
			}
		}

		public static void OnAppSpaceChange(ulong xrSpace)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnAppSpaceChange(xrSpace);
			}
		}

		public static void OnAppSpaceChange2(ulong xrSpace, int spaceFlags)
		{
			if (version >= OVRP_1_108_0.version)
			{
				OVRP_1_108_0.ovrp_UnityOpenXR_OnAppSpaceChange2(xrSpace, spaceFlags);
			}
		}

		public static void AllowVisibilityMesh(bool enabled)
		{
			if (version >= OVRP_1_109_0.version)
			{
				OVRP_1_109_0.ovrp_AllowVisibilityMask(enabled ? Bool.True : Bool.False);
			}
		}

		public static void OnSessionStateChange(int oldState, int newState)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionStateChange(oldState, newState);
			}
		}

		public static void OnSessionBegin(ulong xrSession)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionBegin(xrSession);
			}
		}

		public static void OnSessionEnd(ulong xrSession)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionEnd(xrSession);
			}
		}

		public static void OnSessionExiting(ulong xrSession)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionExiting(xrSession);
			}
		}

		public static void OnSessionDestroy(ulong xrSession)
		{
			if (version >= OVRP_1_71_0.version)
			{
				OVRP_1_71_0.ovrp_UnityOpenXR_OnSessionDestroy(xrSession);
			}
		}
	}

	public enum MarkerType
	{
		QRCode = 1
	}

	public enum SpaceMarkerPayloadType
	{
		InvalidQRCode = 1,
		StringQRCode,
		BinaryQRCode
	}

	public struct MarkerTrackerCreateInfo
	{
		public uint MarkerTypeCount;

		public unsafe MarkerType* MarkerTypes;
	}

	public struct MarkerTrackerCreateCompletion
	{
		public Result FutureResult;

		public ulong MarkerTracker;
	}

	public struct SpaceMarkerPayload
	{
		public uint BufferCapacityInput;

		public uint BufferCountOutput;

		public unsafe byte* Buffer;

		public SpaceMarkerPayloadType PayloadType;
	}

	public enum FutureState
	{
		Pending = 1,
		Ready
	}

	public static class Qpl
	{
		public enum ResultType : short
		{
			Success = 2,
			Fail,
			Cancel
		}

		public enum VariantType
		{
			None,
			String,
			Int,
			Double,
			Bool,
			StringArray,
			IntArray,
			DoubleArray,
			BoolArray
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Variant
		{
			[FieldOffset(0)]
			public VariantType Type;

			[FieldOffset(4)]
			public int Count;

			[FieldOffset(8)]
			public unsafe byte* StringValue;

			[FieldOffset(8)]
			public long LongValue;

			[FieldOffset(8)]
			public double DoubleValue;

			[FieldOffset(8)]
			public Bool BoolValue;

			[FieldOffset(8)]
			public unsafe byte** StringValues;

			[FieldOffset(8)]
			public unsafe long* LongValues;

			[FieldOffset(8)]
			public unsafe double* DoubleValues;

			[FieldOffset(8)]
			public unsafe Bool* BoolValues;

			public unsafe static Variant From(byte* value)
			{
				Variant result = default(Variant);
				result.Type = VariantType.String;
				result.StringValue = value;
				return result;
			}

			public static Variant From(long value)
			{
				Variant result = default(Variant);
				result.Type = VariantType.Int;
				result.LongValue = value;
				return result;
			}

			public static Variant From(double value)
			{
				Variant result = default(Variant);
				result.Type = VariantType.Double;
				result.DoubleValue = value;
				return result;
			}

			public static Variant From(bool value)
			{
				Variant result = default(Variant);
				result.Type = VariantType.Bool;
				result.BoolValue = (value ? Bool.True : Bool.False);
				return result;
			}

			public unsafe static Variant From(byte** values, int count)
			{
				Variant result = default(Variant);
				result.Type = VariantType.StringArray;
				result.Count = count;
				result.StringValues = values;
				return result;
			}

			public unsafe static Variant From(long* values, int count)
			{
				Variant result = default(Variant);
				result.Type = VariantType.IntArray;
				result.Count = count;
				result.LongValues = values;
				return result;
			}

			public unsafe static Variant From(double* values, int count)
			{
				Variant result = default(Variant);
				result.Type = VariantType.DoubleArray;
				result.Count = count;
				result.DoubleValues = values;
				return result;
			}

			public unsafe static Variant From(Bool* values, int count)
			{
				Variant result = default(Variant);
				result.Type = VariantType.BoolArray;
				result.Count = count;
				result.BoolValues = values;
				return result;
			}
		}

		public readonly struct Annotation
		{
			public struct Builder : IDisposable
			{
				private struct Entry
				{
					public IntPtr Key;

					public Variant Value;
				}

				private List<Entry> _entries;

				private List<IntPtr> _ownedStrings;

				public int Count => _entries?.Count ?? 0;

				private IntPtr Copy(string str)
				{
					IntPtr intPtr = Marshal.StringToCoTaskMemUTF8(str);
					_ownedStrings.Add(intPtr);
					return intPtr;
				}

				public static Builder Create()
				{
					Builder result = default(Builder);
					result._entries = OVRObjectPool.List<Entry>();
					result._ownedStrings = OVRObjectPool.List<IntPtr>();
					return result;
				}

				public Builder Add(string key, Variant value)
				{
					_entries.Add(new Entry
					{
						Key = Copy(key),
						Value = value
					});
					return this;
				}

				public unsafe Builder Add(string key, string value)
				{
					return Add(key, Variant.From((byte*)(void*)Copy(value)));
				}

				public unsafe Builder Add(string key, byte* value)
				{
					return Add(key, Variant.From(value));
				}

				public Builder Add(string key, long value)
				{
					return Add(key, Variant.From(value));
				}

				public Builder Add(string key, double value)
				{
					return Add(key, Variant.From(value));
				}

				public Builder Add(string key, bool value)
				{
					return Add(key, Variant.From(value));
				}

				public unsafe Builder Add(string key, byte** value, int count)
				{
					return Add(key, Variant.From(value, count));
				}

				public unsafe Builder Add(string key, long* value, int count)
				{
					return Add(key, Variant.From(value, count));
				}

				public unsafe Builder Add(string key, double* value, int count)
				{
					return Add(key, Variant.From(value, count));
				}

				public unsafe Builder Add(string key, Bool* value, int count)
				{
					return Add(key, Variant.From(value, count));
				}

				public unsafe NativeArray<Annotation> ToNativeArray(Allocator allocator = Allocator.Temp)
				{
					NativeArray<Annotation> result = new NativeArray<Annotation>(Count, allocator);
					if (_entries != null)
					{
						int num = 0;
						foreach (Entry entry in _entries)
						{
							result[num++] = new Annotation((byte*)(void*)entry.Key, entry.Value);
						}
					}
					return result;
				}

				public void Dispose()
				{
					if (_ownedStrings != null)
					{
						foreach (IntPtr ownedString in _ownedStrings)
						{
							Marshal.FreeCoTaskMem(ownedString);
						}
						OVRObjectPool.Return(_ownedStrings);
						_ownedStrings = null;
					}
					if (_entries != null)
					{
						OVRObjectPool.Return(_entries);
						_entries = null;
					}
				}
			}

			public unsafe readonly byte* Key;

			public readonly Variant Value;

			public unsafe string KeyStr => Marshal.PtrToStringUTF8(new IntPtr(Key));

			public unsafe Annotation(byte* key, Variant value)
			{
				Key = key;
				Value = value;
			}
		}

		public const int DefaultInstanceKey = 0;

		public const long AutoSetTimestampMs = -1L;

		public const int AutoSetTimeoutMs = 0;

		public static void SetConsent(Bool consent)
		{
			if (!(version < OVRP_1_92_0.version))
			{
				OVRP_1_92_0.ovrp_QplSetConsent(consent);
			}
		}

		public static void MarkerStart(int markerId, int instanceKey = 0, long timestampMs = -1L)
		{
			if (!(version < OVRP_1_84_0.version))
			{
				OVRP_1_84_0.ovrp_QplMarkerStart(markerId, instanceKey, timestampMs);
			}
		}

		public static void MarkerStartForJoin(int markerId, string joinId, Bool cancelMarkerIfAppBackgrounded, int instanceKey = 0, long timestampMs = -1L)
		{
			if (!(version < OVRP_1_105_0.version))
			{
				OVRP_1_105_0.ovrp_QplMarkerStartForJoin(markerId, joinId, cancelMarkerIfAppBackgrounded, instanceKey, timestampMs);
			}
		}

		public static void MarkerEnd(int markerId, ResultType resultTypeId = ResultType.Success, int instanceKey = 0, long timestampMs = -1L)
		{
			if (!(version < OVRP_1_84_0.version))
			{
				OVRP_1_84_0.ovrp_QplMarkerEnd(markerId, resultTypeId, instanceKey, timestampMs);
			}
		}

		public static Result MarkerPoint(int markerId, string name, int instanceKey, long timestampMs)
		{
			if (!(version < OVRP_1_84_0.version))
			{
				return OVRP_1_84_0.ovrp_QplMarkerPoint(markerId, name, instanceKey, timestampMs);
			}
			return Result.Failure_NotYetImplemented;
		}

		public unsafe static Result MarkerPoint(int markerId, string name, Annotation* annotations, int annotationCount, int instanceKey, long timestampMs)
		{
			if (!(version < OVRP_1_96_0.version))
			{
				return OVRP_1_96_0.ovrp_QplMarkerPointData(markerId, name, annotations, annotationCount, instanceKey, timestampMs);
			}
			return Result.Failure_NotYetImplemented;
		}

		public static void MarkerPointCached(int markerId, int nameHandle, int instanceKey = 0, long timestampMs = -1L)
		{
			if (!(version < OVRP_1_84_0.version))
			{
				OVRP_1_84_0.ovrp_QplMarkerPointCached(markerId, nameHandle, instanceKey, timestampMs);
			}
		}

		public static void MarkerAnnotation(int markerId, string annotationKey, string annotationValue, int instanceKey = 0)
		{
			if (!(version < OVRP_1_84_0.version))
			{
				OVRP_1_84_0.ovrp_QplMarkerAnnotation(markerId, annotationKey, annotationValue, instanceKey);
			}
		}

		public static Result MarkerAnnotation(int markerId, string annotationKey, Variant annotationValue, int instanceKey)
		{
			if (!(version < OVRP_1_96_0.version))
			{
				return OVRP_1_96_0.ovrp_QplMarkerAnnotationVariant(markerId, annotationKey, in annotationValue, instanceKey);
			}
			return Result.Failure_NotYetImplemented;
		}

		public static bool CreateMarkerHandle(string name, out int nameHandle)
		{
			if (version < OVRP_1_84_0.version)
			{
				nameHandle = 0;
				return false;
			}
			return OVRP_1_84_0.ovrp_QplCreateMarkerHandle(name, out nameHandle) == Result.Success;
		}

		public static bool DestroyMarkerHandle(int nameHandle)
		{
			if (version < OVRP_1_84_0.version)
			{
				return false;
			}
			return OVRP_1_84_0.ovrp_QplDestroyMarkerHandle(nameHandle) == Result.Success;
		}
	}

	public static class UnifiedConsent
	{
		private const int ToolId = 1;

		public static Result SaveUnifiedConsent(bool consentValue)
		{
			if (version < OVRP_1_106_0.version)
			{
				return Result.Failure_Unsupported;
			}
			return OVRP_1_106_0.ovrp_SaveUnifiedConsent(1, consentValue ? Bool.True : Bool.False);
		}

		public static Result SaveUnifiedConsentWithOlderVersion(bool consentValue, int consentVersion)
		{
			if (version < OVRP_1_106_0.version)
			{
				return Result.Failure_Unsupported;
			}
			return OVRP_1_106_0.ovrp_SaveUnifiedConsentWithOlderVersion(1, consentValue ? Bool.True : Bool.False, consentVersion);
		}

		public static bool? GetUnifiedConsent()
		{
			if (version < OVRP_1_106_0.version)
			{
				return false;
			}
			return OVRP_1_106_0.ovrp_GetUnifiedConsent(1) switch
			{
				OptionalBool.True => true, 
				OptionalBool.False => false, 
				_ => null, 
			};
		}

		public static string GetConsentTitle()
		{
			if (version < OVRP_1_106_0.version)
			{
				return "";
			}
			IntPtr intPtr = Marshal.AllocHGlobal(256);
			if (OVRP_1_106_0.ovrp_GetConsentTitle(intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return "";
			}
			string result = Marshal.PtrToStringAnsi(intPtr);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		public static string GetConsentMarkdownText()
		{
			if (version < OVRP_1_106_0.version)
			{
				return "";
			}
			IntPtr intPtr = Marshal.AllocHGlobal(2048);
			if (OVRP_1_106_0.ovrp_GetConsentMarkdownText(intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return "";
			}
			string result = Marshal.PtrToStringAnsi(intPtr);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		public static string GetConsentNotificationMarkdownText()
		{
			if (version < OVRP_1_106_0.version)
			{
				return "";
			}
			IntPtr intPtr = Marshal.AllocHGlobal(1024);
			IntPtr intPtr2 = Marshal.StringToHGlobalAnsi("Edit > Preferences > Meta XR");
			if (OVRP_1_106_0.ovrp_GetConsentNotificationMarkdownText(intPtr2, intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return "";
			}
			string result = Marshal.PtrToStringAnsi(intPtr);
			Marshal.FreeHGlobal(intPtr);
			Marshal.FreeHGlobal(intPtr2);
			return result;
		}

		public static string GetConsentSettingsChangeText()
		{
			if (version < OVRP_1_106_0.version)
			{
				return "";
			}
			IntPtr intPtr = Marshal.AllocHGlobal(1024);
			if (OVRP_1_106_0.ovrp_GetConsentSettingsChangeText(intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return "";
			}
			string result = Marshal.PtrToStringAnsi(intPtr);
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		public static bool ShouldShowTelemetryConsentWindow()
		{
			if (version < OVRP_1_106_0.version)
			{
				return false;
			}
			return OVRP_1_106_0.ovrp_ShouldShowTelemetryConsentWindow(1) == Bool.True;
		}

		public static bool IsConsentSettingsChangeEnabled()
		{
			if (version < OVRP_1_106_0.version)
			{
				return true;
			}
			return OVRP_1_106_0.ovrp_IsConsentSettingsChangeEnabled(1) == Bool.True;
		}

		public static bool ShouldShowTelemetryNotification()
		{
			if (version < OVRP_1_106_0.version)
			{
				return false;
			}
			return OVRP_1_106_0.ovrp_ShouldShowTelemetryNotification(1) == Bool.True;
		}

		public static Result SetNotificationShown()
		{
			if (version < OVRP_1_106_0.version)
			{
				return Result.Failure_Unsupported;
			}
			return OVRP_1_106_0.ovrp_SetNotificationShown(1);
		}
	}

	private static class OVRP_0_1_0
	{
		public static readonly Version version = new Version(0, 1, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Sizei ovrp_GetEyeTextureSize(Eye eyeId);
	}

	private static class OVRP_0_1_1
	{
		public static readonly Version version = new Version(0, 1, 1);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetOverlayQuad2(Bool onTop, Bool headLocked, IntPtr texture, IntPtr device, Posef pose, Vector3f scale);
	}

	private static class OVRP_0_1_2
	{
		public static readonly Version version = new Version(0, 1, 2);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodePose(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);
	}

	private static class OVRP_0_1_3
	{
		public static readonly Version version = new Version(0, 1, 3);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeVelocity(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
		public static extern Posef ovrp_GetNodeAcceleration(Node nodeId);
	}

	private static class OVRP_0_5_0
	{
		public static readonly Version version = new Version(0, 5, 0);
	}

	private static class OVRP_1_0_0
	{
		public static readonly Version version = new Version(1, 0, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern TrackingOrigin ovrp_GetTrackingOriginType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingOriginType(TrackingOrigin originType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetTrackingCalibratedOrigin();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_RecenterTrackingOrigin(uint flags);
	}

	private static class OVRP_1_1_0
	{
		public static readonly Version version = new Version(1, 1, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")]
		private static extern IntPtr _ovrp_GetVersion();

		public static string ovrp_GetVersion()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetNativeSDKVersion")]
		private static extern IntPtr _ovrp_GetNativeSDKVersion();

		public static string ovrp_GetNativeSDKVersion()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetNativeSDKVersion());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioOutId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_GetAudioInId();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeTextureScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetEyeTextureScale(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingOrientationSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingOrientationEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingOrientationEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingPositionSupported();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingPositionEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingPositionEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodePresent(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodeOrientationTracked(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetNodePositionTracked(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Frustumf ovrp_GetNodeFrustum(Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern ControllerState ovrp_GetControllerState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Replaced by ovrp_GetSuggestedCpuPerformanceLevel", false)]
		public static extern int ovrp_GetSystemCpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Replaced by ovrp_SetSuggestedCpuPerformanceLevel", false)]
		public static extern Bool ovrp_SetSystemCpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Replaced by ovrp_GetSuggestedGpuPerformanceLevel", false)]
		public static extern int ovrp_GetSystemGpuLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Replaced by ovrp_SetSuggestedGpuPerformanceLevel", false)]
		public static extern Bool ovrp_SetSystemGpuLevel(int value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetSystemPowerSavingMode();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemDisplayFrequency();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemVSyncCount();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemVolume();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BatteryStatus ovrp_GetSystemBatteryStatus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryLevel();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetSystemBatteryTemperature();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetSystemProductName")]
		private static extern IntPtr _ovrp_GetSystemProductName();

		public static string ovrp_GetSystemProductName()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetSystemProductName());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ShowSystemUI(PlatformUI ui);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppMonoscopic();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetAppMonoscopic(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppHasVrFocus();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppShouldQuit();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppShouldRecenter();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetAppLatencyTimings")]
		private static extern IntPtr _ovrp_GetAppLatencyTimings();

		public static string ovrp_GetAppLatencyTimings()
		{
			return Marshal.PtrToStringAnsi(_ovrp_GetAppLatencyTimings());
		}

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetUserPresent();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserIPD();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserIPD(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeDepth();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserEyeDepth(float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetUserEyeHeight();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetUserEyeHeight(float value);
	}

	private static class OVRP_1_2_0
	{
		public static readonly Version version = new Version(1, 2, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetSystemVSyncCount(int vsyncCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrpi_SetTrackingCalibratedOrigin();
	}

	private static class OVRP_1_3_0
	{
		public static readonly Version version = new Version(1, 3, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetEyeOcclusionMeshEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetEyeOcclusionMeshEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetSystemHeadphonesPresent();
	}

	private static class OVRP_1_5_0
	{
		public static readonly Version version = new Version(1, 5, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemRegion ovrp_GetSystemRegion();
	}

	private static class OVRP_1_6_0
	{
		public static readonly Version version = new Version(1, 6, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetTrackingIPDEnabled();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetTrackingIPDEnabled(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern HapticsState ovrp_GetControllerHapticsState(uint controllerMask);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetOverlayQuad3(uint flags, IntPtr textureLeft, IntPtr textureRight, IntPtr device, Posef pose, Vector3f scale, int layerIndex);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetEyeRecommendedResolutionScale();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppCpuStartToGpuEndTime();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ovrp_GetSystemRecommendedMSAALevel();
	}

	private static class OVRP_1_7_0
	{
		public static readonly Version version = new Version(1, 7, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetAppChromaticCorrection();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetAppChromaticCorrection(Bool value);
	}

	private static class OVRP_1_8_0
	{
		public static readonly Version version = new Version(1, 8, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetBoundaryConfigured();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
		public static extern BoundaryTestResult ovrp_TestBoundaryNode(Node nodeId, BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
		public static extern BoundaryTestResult ovrp_TestBoundaryPoint(Vector3f point, BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern BoundaryGeometry ovrp_GetBoundaryGeometry(BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Vector3f ovrp_GetBoundaryDimensions(BoundaryType boundaryType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
		public static extern Bool ovrp_GetBoundaryVisible();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
		public static extern Bool ovrp_SetBoundaryVisible(Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodePose2(int stateId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Posef ovrp_GetNodeVelocity2(int stateId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
		public static extern Posef ovrp_GetNodeAcceleration2(int stateId, Node nodeId);
	}

	private static class OVRP_1_9_0
	{
		public static readonly Version version = new Version(1, 9, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern SystemHeadset ovrp_GetSystemHeadsetType();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Controller ovrp_GetActiveController();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Controller ovrp_GetConnectedControllers();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern AppPerfStats ovrp_GetAppPerfStats();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ResetAppPerfStats();
	}

	private static class OVRP_1_10_0
	{
		public static readonly Version version = new Version(1, 10, 0);
	}

	private static class OVRP_1_11_0
	{
		public static readonly Version version = new Version(1, 11, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_SetDesiredEyeTextureFormat(EyeTextureFormat value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern EyeTextureFormat ovrp_GetDesiredEyeTextureFormat();
	}

	private static class OVRP_1_12_0
	{
		public static readonly Version version = new Version(1, 12, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern float ovrp_GetAppFramerate();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern PoseStatef ovrp_GetNodePoseState(Step stepId, Node nodeId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern ControllerState2 ovrp_GetControllerState2(uint controllerMask);
	}

	private static class OVRP_1_15_0
	{
		public static readonly Version version = new Version(1, 15, 0);

		public const int OVRP_EXTERNAL_CAMERA_NAME_SIZE = 32;

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_InitializeMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ShutdownMixedReality();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetMixedRealityInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdateExternalCamera();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraCount(out int cameraCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraName(int cameraId, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] char[] cameraName);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraIntrinsics(int cameraId, out CameraIntrinsics cameraIntrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraExtrinsics(int cameraId, out CameraExtrinsics cameraExtrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CalculateLayerDesc(OverlayShape shape, LayerLayout layout, ref Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags, ref LayerDesc layerDesc);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSetupLayer(ref LayerDesc desc, IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueDestroyLayer(IntPtr layerId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerTextureStageCount(int layerId, ref int layerTextureStageCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerTexturePtr(int layerId, int stage, Eye eyeId, ref IntPtr textureHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSubmitLayer(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref Posef pose, ref Vector3f scale, int layerIndex);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodeFrustum2(Node nodeId, out Frustumf2 nodeFrustum);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetEyeTextureArrayEnabled();
	}

	private static class OVRP_1_16_0
	{
		public static readonly Version version = new Version(1, 16, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdateCameraDevices();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_IsCameraDeviceAvailable(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, Sizei preferredColorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_OpenCameraDevice(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CloseCameraDevice(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_HasCameraDeviceOpened(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceColorFrameSize(CameraDevice cameraDevice, out Sizei colorFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceColorFrameBgraPixels(CameraDevice cameraDevice, out IntPtr colorFrameBgraPixels, out int colorFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerState4(uint controllerMask, ref ControllerState4 controllerState);
	}

	private static class OVRP_1_17_0
	{
		public static readonly Version version = new Version(1, 17, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetExternalCameraPose(CameraDevice camera, out Posef cameraPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ConvertPoseToCameraSpace(CameraDevice camera, ref Posef trackingSpacePose, out Posef cameraSpacePose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceIntrinsicsParameters(CameraDevice camera, out Bool supportIntrinsics, out CameraDeviceIntrinsicsParameters intrinsicsParameters);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DoesCameraDeviceSupportDepth(CameraDevice camera, out Bool supportDepth);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthSensingMode(CameraDevice camera, out CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDevicePreferredDepthQuality(CameraDevice camera, out CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsCameraDeviceDepthFrameAvailable(CameraDevice camera, out Bool available);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthFrameSize(CameraDevice camera, out Sizei depthFrameSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthFramePixels(CameraDevice cameraDevice, out IntPtr depthFramePixels, out int depthFrameRowPitch);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCameraDeviceDepthConfidencePixels(CameraDevice cameraDevice, out IntPtr depthConfidencePixels, out int depthConfidenceRowPitch);
	}

	private static class OVRP_1_18_0
	{
		public static readonly Version version = new Version(1, 18, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetHandNodePoseStateLatency(double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandNodePoseStateLatency(out double latencyInSeconds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetAppHasInputFocus(out Bool appHasInputFocus);
	}

	private static class OVRP_1_19_0
	{
		public static readonly Version version = new Version(1, 19, 0);
	}

	private static class OVRP_1_21_0
	{
		public static readonly Version version = new Version(1, 21, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTiledMultiResSupported(out Bool foveationSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTiledMultiResLevel(out FoveatedRenderingLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetTiledMultiResLevel(FoveatedRenderingLevel level);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetGPUUtilSupported(out Bool gpuUtilSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetGPUUtilLevel(out float gpuUtil);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSystemDisplayFrequency2(out float systemDisplayFrequency);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSystemDisplayAvailableFrequencies(IntPtr systemDisplayAvailableFrequencies, ref int numFrequencies);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSystemDisplayFrequency(float requestedFrequency);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetAppAsymmetricFov(out Bool useAsymmetricFov);
	}

	private static class OVRP_1_28_0
	{
		public static readonly Version version = new Version(1, 28, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetDominantHand(out Handedness dominantHand);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendEvent(string name, string param);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSetupLayer2(ref LayerDesc desc, int compositionDepth, IntPtr layerId);
	}

	private static class OVRP_1_29_0
	{
		public static readonly Version version = new Version(1, 29, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerAndroidSurfaceObject(int layerId, ref IntPtr surfaceObject);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetHeadPoseModifier(ref Quatf relativeRotation, ref Vector3f relativeTranslation);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHeadPoseModifier(out Quatf relativeRotation, out Vector3f relativeTranslation);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodePoseStateRaw(Step stepId, int frameIndex, Node nodeId, out PoseStatef nodePoseState);
	}

	private static class OVRP_1_30_0
	{
		public static readonly Version version = new Version(1, 30, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCurrentTrackingTransformPose(out Posef trackingTransformPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTrackingTransformRawPose(out Posef trackingTransformRawPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendEvent2(string name, string param, string source);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsPerfMetricsSupported(PerfMetrics perfMetrics, out Bool isSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPerfMetricsFloat(PerfMetrics perfMetrics, out float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPerfMetricsInt(PerfMetrics perfMetrics, out int value);
	}

	private static class OVRP_1_31_0
	{
		public static readonly Version version = new Version(1, 31, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTimeInSeconds(out double value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, Bool applyToAllLayers);
	}

	private static class OVRP_1_32_0
	{
		public static readonly Version version = new Version(1, 32, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_AddCustomMetadata(string name, string param);
	}

	private static class OVRP_1_34_0
	{
		public static readonly Version version = new Version(1, 34, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnqueueSubmitLayer2(uint flags, IntPtr textureLeft, IntPtr textureRight, int layerId, int frameIndex, ref Posef pose, ref Vector3f scale, int layerIndex, Bool overrideTextureRectMatrix, ref TextureRectMatrixf textureRectMatrix, Bool overridePerLayerColorScaleAndOffset, ref Vector4 colorScale, ref Vector4 colorOffset);
	}

	private static class OVRP_1_35_0
	{
		public static readonly Version version = new Version(1, 35, 0);
	}

	private static class OVRP_1_36_0
	{
		public static readonly Version version = new Version(1, 36, 0);
	}

	private static class OVRP_1_37_0
	{
		public static readonly Version version = new Version(1, 37, 0);
	}

	private static class OVRP_1_38_0
	{
		public static readonly Version version = new Version(1, 38, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTrackingTransformRelativePose(ref Posef trackingTransformRelativePose, TrackingOrigin trackingOrigin);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_Initialize();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_Shutdown();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetInitialized(out Bool initialized);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_Update();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetMrcActivationMode(out Media.MrcActivationMode activationMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetMrcActivationMode(Media.MrcActivationMode activationMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_IsMrcEnabled(out Bool mrcEnabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_IsMrcActivated(out Bool mrcActivated);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_UseMrcDebugCamera(out Bool useMrcDebugCamera);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetMrcInputVideoBufferType(Media.InputVideoBufferType inputVideoBufferType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetMrcInputVideoBufferType(ref Media.InputVideoBufferType inputVideoBufferType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetMrcFrameSize(int frameWidth, int frameHeight);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetMrcFrameSize(ref int frameWidth, ref int frameHeight);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetMrcAudioSampleRate(int sampleRate);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetMrcAudioSampleRate(ref int sampleRate);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetMrcFrameImageFlipped(Bool flipped);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetMrcFrameImageFlipped(ref Bool flipped);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_EncodeMrcFrame(IntPtr rawBuffer, IntPtr audioDataPtr, int audioDataLen, int audioChannels, double timestamp, ref int outSyncId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_EncodeMrcFrameWithDualTextures(IntPtr backgroundTextureHandle, IntPtr foregroundTextureHandle, IntPtr audioData, int audioDataLen, int audioChannels, double timestamp, ref int outSyncId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SyncMrcFrame(int syncId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetDeveloperMode(Bool active);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodeOrientationValid(Node nodeId, ref Bool nodeOrientationValid);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodePositionValid(Node nodeId, ref Bool nodePositionValid);
	}

	private static class OVRP_1_39_0
	{
		public static readonly Version version = new Version(1, 39, 0);
	}

	private static class OVRP_1_40_0
	{
		public static readonly Version version = new Version(1, 40, 0);
	}

	private static class OVRP_1_41_0
	{
		public static readonly Version version = new Version(1, 41, 0);
	}

	private static class OVRP_1_42_0
	{
		public static readonly Version version = new Version(1, 42, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetAdaptiveGpuPerformanceScale2(ref float adaptiveGpuPerformanceScale);
	}

	private static class OVRP_1_43_0
	{
		public static readonly Version version = new Version(1, 43, 0);
	}

	private static class OVRP_1_44_0
	{
		public static readonly Version version = new Version(1, 44, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandTrackingEnabled(ref Bool handTrackingEnabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandState(Step stepId, Hand hand, out HandStateInternal handState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSkeleton(SkeletonType skeletonType, out Skeleton skeleton);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetMesh(MeshType meshType, IntPtr meshPtr);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_OverrideExternalCameraFov(int cameraId, Bool useOverriddenFov, ref Fovf fov);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetUseOverriddenExternalCameraFov(int cameraId, out Bool useOverriddenFov);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_OverrideExternalCameraStaticPose(int cameraId, Bool useOverriddenPose, ref Posef poseInStageOrigin);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetUseOverriddenExternalCameraStaticPose(int cameraId, out Bool useOverriddenStaticPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ResetDefaultExternalCamera();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetDefaultExternalCamera(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLocalTrackingSpaceRecenterCount(ref int recenterCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPredictedDisplayTime(int frameIndex, ref double predictedDisplayTime);
	}

	private static class OVRP_1_45_0
	{
		public static readonly Version version = new Version(1, 45, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSystemHmd3DofModeEnabled(ref Bool enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetAvailableQueueIndexVulkan(uint queueIndexVk);
	}

	private static class OVRP_1_46_0
	{
		public static readonly Version version = new Version(1, 46, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTiledMultiResDynamic(out Bool isDynamic);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetTiledMultiResDynamic(Bool isDynamic);
	}

	private static class OVRP_1_47_0
	{
		public static readonly Version version = new Version(1, 47, 0);
	}

	private static class OVRP_1_48_0
	{
		public static readonly Version version = new Version(1, 48, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetExternalCameraProperties(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics);
	}

	private static class OVRP_1_49_0
	{
		public static readonly Version version = new Version(1, 49, 0);

		public const int OVRP_ANCHOR_NAME_SIZE = 32;

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetClientColorDesc(ColorSpace colorSpace);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHmdColorDesc(ref ColorSpace colorSpace);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_EncodeMrcFrameWithPoseTime(IntPtr rawBuffer, IntPtr audioDataPtr, int audioDataLen, int audioChannels, double timestamp, double poseTime, ref int outSyncId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_EncodeMrcFrameDualTexturesWithPoseTime(IntPtr backgroundTextureHandle, IntPtr foregroundTextureHandle, IntPtr audioData, int audioDataLen, int audioChannels, double timestamp, double poseTime, ref int outSyncId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetHeadsetControllerPose(Posef headsetPose, Posef leftControllerPose, Posef rightControllerPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_EnumerateCameraAnchorHandles(ref int anchorCount, ref IntPtr CameraAnchorHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCurrentCameraAnchorHandle(ref IntPtr anchorHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCameraAnchorName(IntPtr anchorHandle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 32)] char[] cameraName);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCameraAnchorHandle(IntPtr anchorName, ref IntPtr anchorHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCameraAnchorType(IntPtr anchorHandle, ref CameraAnchorType anchorType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_CreateCustomCameraAnchor(IntPtr anchorName, ref IntPtr anchorHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_DestroyCustomCameraAnchor(IntPtr anchorHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCustomCameraAnchorPose(IntPtr anchorHandle, ref Posef pose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetCustomCameraAnchorPose(IntPtr anchorHandle, Posef pose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetCameraMinMaxDistance(IntPtr anchorHandle, ref double minDistance, ref double maxDistance);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetCameraMinMaxDistance(IntPtr anchorHandle, double minDistance, double maxDistance);
	}

	private static class OVRP_1_50_0
	{
		public static readonly Version version = new Version(1, 50, 0);
	}

	private static class OVRP_1_51_0
	{
		public static readonly Version version = new Version(1, 51, 0);
	}

	private static class OVRP_1_52_0
	{
		public static readonly Version version = new Version(1, 52, 0);
	}

	private static class OVRP_1_53_0
	{
		public static readonly Version version = new Version(1, 53, 0);
	}

	private static class OVRP_1_54_0
	{
		public static readonly Version version = new Version(1, 54, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetPlatformInitialized();
	}

	private static class OVRP_1_55_0
	{
		public static readonly Version version = new Version(1, 55, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSkeleton2(SkeletonType skeletonType, out Skeleton2Internal skeleton);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_PollEvent(ref EventDataBuffer eventDataBuffer);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNativeXrApiType(out XrApi xrApi);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNativeOpenXRHandles(out ulong xrInstance, out ulong xrSession);
	}

	private static class OVRP_1_55_1
	{
		public static readonly Version version = new Version(1, 55, 1);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_PollEvent2(ref EventType eventType, ref IntPtr eventData);
	}

	private static class OVRP_1_56_0
	{
		public static readonly Version version = new Version(1, 56, 0);
	}

	private static class OVRP_1_57_0
	{
		public static readonly Version version = new Version(1, 57, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_GetPlatformCameraMode(out Media.PlatformCameraMode platformCameraMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_SetPlatformCameraMode(Media.PlatformCameraMode platformCameraMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetEyeFovPremultipliedAlphaMode(Bool enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetEyeFovPremultipliedAlphaMode(ref Bool enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetKeyboardOverlayUV(Vector2f uv);
	}

	private static class OVRP_1_58_0
	{
		public static readonly Version version = new Version(1, 58, 0);
	}

	private static class OVRP_1_59_0
	{
		public static readonly Version version = new Version(1, 59, 0);
	}

	private static class OVRP_1_60_0
	{
		public static readonly Version version = new Version(1, 60, 0);
	}

	private static class OVRP_1_61_0
	{
		public static readonly Version version = new Version(1, 61, 0);
	}

	private static class OVRP_1_62_0
	{
		public static readonly Version version = new Version(1, 62, 0);
	}

	private static class OVRP_1_63_0
	{
		public static readonly Version version = new Version(1, 63, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_InitializeInsightPassthrough();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ShutdownInsightPassthrough();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_GetInsightPassthroughInitialized();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetInsightPassthroughStyle(int layerId, InsightPassthroughStyle style);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateInsightTriangleMesh(int layerId, IntPtr vertices, int vertexCount, IntPtr triangles, int triangleCount, out ulong meshHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyInsightTriangleMesh(ulong meshHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_AddInsightPassthroughSurfaceGeometry(int layerId, ulong meshHandle, Matrix4x4 T_world_model, out ulong geometryInstanceHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyInsightPassthroughGeometryInstance(ulong geometryInstanceHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdateInsightPassthroughGeometryTransform(ulong geometryInstanceHandle, Matrix4x4 T_world_model);
	}

	private static class OVRP_1_64_0
	{
		public static readonly Version version = new Version(1, 64, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_LocateSpace(ref Posef location, ref ulong space, TrackingOrigin trackingOrigin);
	}

	private static class OVRP_1_65_0
	{
		public static readonly Version version = new Version(1, 65, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxLoadFromMemory(ref IntPtr data, uint length, ref IntPtr texture);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxTextureWidth(IntPtr texture, ref uint width);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxTextureHeight(IntPtr texture, ref uint height);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxTranscode(IntPtr texture, uint format);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxGetTextureData(IntPtr texture, IntPtr data, uint bufferSize);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxTextureSize(IntPtr texture, ref uint size);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_KtxDestroy(IntPtr texture);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroySpace(ref ulong space);
	}

	private static class OVRP_1_66_0
	{
		public static readonly Version version = new Version(1, 66, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetInsightPassthroughInitializationState();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_Media_IsCastingToRemoteClient(out Bool isCasting);
	}

	private static class OVRP_1_67_0
	{
		public static readonly Version version = new Version(1, 67, 0);
	}

	private static class OVRP_1_68_0
	{
		public static readonly Version version = new Version(1, 68, 0);

		public const int OVRP_RENDER_MODEL_MAX_PATH_LENGTH = 256;

		public const int OVRP_RENDER_MODEL_MAX_NAME_LENGTH = 64;

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_LoadRenderModel(ulong modelKey, uint bufferInputCapacity, ref uint bufferCountOutput, IntPtr buffer);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetRenderModelPaths(uint index, IntPtr path);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetRenderModelProperties(string path, out RenderModelPropertiesInternal properties);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetInsightPassthroughKeyboardHandsIntensity(int layerId, InsightPassthroughKeyboardHandsIntensity intensity);
	}

	private static class OVRP_1_69_0
	{
		public static readonly Version version = new Version(1, 69, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodePoseStateImmediate(Node nodeId, out PoseStatef nodePoseState);
	}

	private static class OVRP_1_70_0
	{
		public static readonly Version version = new Version(1, 70, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetLogCallback2(LogCallback2DelegateType logCallback);
	}

	private static class OVRP_1_71_0
	{
		public static readonly Version version = new Version(1, 71, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsInsightPassthroughSupported(ref Bool supported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_SetClientVersion(int majorVersion, int minorVersion, int patchVersion);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ovrp_UnityOpenXR_HookGetInstanceProcAddr(IntPtr func);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UnityOpenXR_OnInstanceCreate(ulong xrInstance);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnInstanceDestroy(ulong xrInstance);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionCreate(ulong xrSession);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnAppSpaceChange(ulong xrSpace);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionStateChange(int oldState, int newState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionBegin(ulong xrSession);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionEnd(ulong xrSession);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionExiting(ulong xrSession);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnSessionDestroy(ulong xrSession);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSuggestedCpuPerformanceLevel(ProcessorPerformanceLevel perfLevel);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSuggestedCpuPerformanceLevel(out ProcessorPerformanceLevel perfLevel);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSuggestedGpuPerformanceLevel(ProcessorPerformanceLevel perfLevel);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSuggestedGpuPerformanceLevel(out ProcessorPerformanceLevel perfLevel);
	}

	private static class OVRP_1_72_0
	{
		public static readonly Version version = new Version(1, 72, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateSpatialAnchor(ref SpatialAnchorCreateInfo createInfo, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSpaceComponentStatus(ref ulong space, SpaceComponentType componentType, Bool enable, double timeout, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceComponentStatus(ref ulong space, SpaceComponentType componentType, out Bool enabled, out Bool changePending);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EnumerateSpaceSupportedComponents(ref ulong space, uint componentTypesCapacityInput, out uint componentTypesCountOutput, [In][Out][MarshalAs(UnmanagedType.LPArray)] SpaceComponentType[] componentTypes);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_EnumerateSpaceSupportedComponents(ref ulong space, uint componentTypesCapacityInput, out uint componentTypesCountOutput, SpaceComponentType* componentTypes);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SaveSpace(ref ulong space, SpaceStorageLocation location, SpaceStoragePersistenceMode mode, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QuerySpaces(ref SpaceQueryInfo queryInfo, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RetrieveSpaceQueryResults(ref ulong requestId, uint resultCapacityInput, ref uint resultCountOutput, IntPtr results);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EraseSpace(ref ulong space, SpaceStorageLocation location, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceContainer(ref ulong space, ref SpaceContainerInternal containerInternal);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceBoundingBox2D(ref ulong space, out Rectf rect);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceBoundingBox3D(ref ulong space, out Boundsf bounds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceSemanticLabels(ref ulong space, ref SpaceSemanticLabelInternal labelsInternal);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceRoomLayout(ref ulong space, ref RoomLayoutInternal roomLayoutInternal);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceBoundary2D(ref ulong space, ref PolygonalBoundary2DInternal boundaryInternal);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RequestSceneCapture(ref SceneCaptureRequestInternal request, out ulong requestId);
	}

	private static class OVRP_1_73_0
	{
		public static readonly Version version = new Version(1, 73, 0);
	}

	private static class OVRP_1_74_0
	{
		public static readonly Version version = new Version(1, 74, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceUuid(in ulong space, out Guid uuid);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateVirtualKeyboard(VirtualKeyboardCreateInfo createInfo);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyVirtualKeyboard();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendVirtualKeyboardInput(VirtualKeyboardInputInfo inputInfo, ref Posef interactorRootPose);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ChangeVirtualKeyboardTextContext(string textContext);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateVirtualKeyboardSpace(VirtualKeyboardSpaceCreateInfo createInfo, out ulong keyboardSpace);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SuggestVirtualKeyboardLocation(VirtualKeyboardLocationInfo locationInfo);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetVirtualKeyboardScale(out float location);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetRenderModelProperties2(string path, RenderModelFlags flags, out RenderModelPropertiesInternal properties);
	}

	private static class OVRP_1_75_0
	{
		public static readonly Version version = new Version(1, 75, 0);
	}

	private static class OVRP_1_76_0
	{
		public static readonly Version version = new Version(1, 76, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetNodePoseStateAtTime(double time, Node nodeId, out PoseStatef nodePoseState);
	}

	private static class OVRP_1_78_0
	{
		public static readonly Version version = new Version(1, 78, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPassthroughCapabilityFlags(ref PassthroughCapabilityFlags capabilityFlags);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFoveationEyeTrackedSupported(out Bool foveationSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFoveationEyeTracked(out Bool isEyeTrackedFoveation);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetFoveationEyeTracked(Bool isEyeTrackedFoveation);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartFaceTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopFaceTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartBodyTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopBodyTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartEyeTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopEyeTracking();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetEyeTrackingSupported(out Bool eyeTrackingSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceTrackingSupported(out Bool faceTrackingSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetBodyTrackingEnabled(out Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetBodyTrackingSupported(out Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetBodyState(Step stepId, int frameIndex, out BodyStateInternal bodyState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceTrackingEnabled(out Bool faceTrackingEnabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceState(Step stepId, int frameIndex, out FaceStateInternal faceState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetEyeTrackingEnabled(out Bool eyeTrackingEnabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetEyeGazesState(Step stepId, int frameIndex, out EyeGazesStateInternal eyeGazesState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerState5(uint controllerMask, ref ControllerState5 controllerState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetControllerLocalizedVibration(Controller controllerMask, HapticsLocation hapticsLocationMask, float frequency, float amplitude);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLocalDimmingSupported(out Bool localDimmingSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetLocalDimming(Bool localDimmingMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLocalDimming(out Bool localDimmingMode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCurrentInteractionProfile(Hand hand, out InteractionProfile interactionProfile);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetControllerHapticsAmplitudeEnvelope(Controller controllerMask, HapticsAmplitudeEnvelopeVibration hapticsVibration);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetControllerHapticsPcm(Controller controllerMask, HapticsPcmVibration hapticsVibration);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerSampleRateHz(Controller controller, out float sampleRateHz);
	}

	private static class OVRP_1_79_0
	{
		public static readonly Version version = new Version(1, 79, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_ShareSpaces(ulong* spaces, uint numSpaces, ulong* userHandles, uint numUsers, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_SaveSpaceList(ulong* spaces, uint numSpaces, SpaceStorageLocation location, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceUserId(in ulong spaceUserHandle, out ulong spaceUserId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateSpaceUser(in ulong spaceUserId, out ulong spaceUserHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroySpaceUser(in ulong userHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_LocateSpace2(out SpaceLocationf location, in ulong space, TrackingOrigin trackingOrigin);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DeclareUser(in ulong userId, out ulong userHandle);
	}

	private static class OVRP_1_81_0
	{
		public static readonly Version version = new Version(1, 81, 0);
	}

	private static class OVRP_1_82_0
	{
		public static readonly Version version = new Version(1, 82, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceTriangleMesh(ref ulong space, ref TriangleMeshInternal triangleMeshInternal);
	}

	private static class OVRP_1_83_0
	{
		public static readonly Version version = new Version(1, 83, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerState6(uint controllerMask, ref ControllerState6 controllerState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetVirtualKeyboardModelAnimationStates(ref VirtualKeyboardModelAnimationStatesInternal animationStates);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetVirtualKeyboardDirtyTextures(ref VirtualKeyboardTextureIdsInternal textureIds);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetVirtualKeyboardTextureData(ulong textureId, ref VirtualKeyboardTextureData textureData);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetVirtualKeyboardModelVisibility(ref VirtualKeyboardModelVisibility visibility);
	}

	private static class OVRP_1_84_0
	{
		public static readonly Version version = new Version(1, 84, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreatePassthroughColorLut(PassthroughColorLutChannels channels, uint resolution, PassthroughColorLutData data, out ulong colorLut);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyPassthroughColorLut(ulong colorLut);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UpdatePassthroughColorLut(ulong colorLut, PassthroughColorLutData data);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetInsightPassthroughStyle2(int layerId, in InsightPassthroughStyle2 style);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetLayerRecommendedResolution(int layerId, out Sizei recommendedDimensions);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetEyeLayerRecommendedResolution(out Sizei recommendedDimensions);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerStart(int markerId, int instanceKey, long timestampMs);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerEnd(int markerId, Qpl.ResultType resultTypeId, int instanceKey, long timestampMs);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerPoint(int markerId, [MarshalAs(UnmanagedType.LPStr)] string name, int instanceKey, long timestampMs);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerPointCached(int markerId, int nameHandle, int instanceKey, long timestampMs);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerAnnotation(int markerId, [MarshalAs(UnmanagedType.LPStr)] string annotationKey, [MarshalAs(UnmanagedType.LPStr)] string annotationValue, int instanceKey);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplCreateMarkerHandle([MarshalAs(UnmanagedType.LPStr)] string name, out int nameHandle);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplDestroyMarkerHandle(int nameHandle);
	}

	private static class OVRP_1_85_0
	{
		public static readonly Version version = new Version(1, 85, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_OnEditorShutdown();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPassthroughCapabilities(ref PassthroughCapabilities capabilityFlags);
	}

	private static class OVRP_1_86_0
	{
		public static readonly Version version = new Version(1, 86, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetControllerDrivenHandPoses(Bool controllerDrivenHandPoses);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsControllerDrivenHandPosesEnabled(ref Bool enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_AreHandPosesGeneratedByControllerData(Step stepId, Node nodeId, ref Bool isGeneratedByControllerData);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetMultimodalHandsControllersSupported(Bool supported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsMultimodalHandsControllersSupported(ref Bool supported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCurrentDetachedInteractionProfile(Hand hand, out InteractionProfile interactionProfile);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetControllerIsInHand(Step stepId, Node nodeId, ref Bool isInHand);
	}

	private static class OVRP_1_87_0
	{
		public static readonly Version version = new Version(1, 87, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetPassthroughPreferences(out PassthroughPreferences preferences);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetEyeBufferSharpenType(LayerSharpenType sharpenType);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetControllerDrivenHandPosesAreNatural(Bool controllerDrivenHandPosesAreNatural);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_AreControllerDrivenHandPosesNatural(ref Bool natural);
	}

	private static class OVRP_1_88_0
	{
		public static readonly Version version = new Version(1, 88, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetSimultaneousHandsAndControllersEnabled(Bool enabled);
	}

	private static class OVRP_1_89_0
	{
		public static readonly Version version = new Version(1, 89, 0);
	}

	private static class OVRP_1_90_0
	{
		public static readonly Version version = new Version(1, 90, 0);
	}

	private static class OVRP_1_91_0
	{
		public static readonly Version version = new Version(1, 91, 0);
	}

	private static class OVRP_1_92_0
	{
		public static readonly Version version = new Version(1, 92, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceState2(Step stepId, int frameIndex, out FaceState2Internal faceState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartFaceTracking2(FaceTrackingDataSource[] requestedDataSources, uint requestedDataSourcesCount);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopFaceTracking2();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceTracking2Enabled(out Bool faceTracking2Enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceTracking2Supported(out Bool faceTracking2Enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RequestBodyTrackingFidelity(BodyTrackingFidelity2 fidelity);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SuggestBodyTrackingCalibrationOverride(BodyTrackingCalibrationInfo calibrationInfo);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ResetBodyTrackingCalibration();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetBodyState4(Step stepId, int frameIndex, out BodyState4Internal bodyState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSkeleton3(SkeletonType skeletonType, out Skeleton3Internal skeleton);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartBodyTracking2(BodyJointSet jointSet);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplSetConsent(Bool consent);
	}

	private static class OVRP_1_93_0
	{
		public static readonly Version version = new Version(1, 93, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetWideMotionModeHandPoses(Bool wideMotionModeHandPoses);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_IsWideMotionModeHandPosesEnabled(ref Bool enabled);
	}

	private static class OVRP_1_94_0
	{
		public static readonly Version version = new Version(1, 94, 0);
	}

	private static class OVRP_1_95_0
	{
		public static readonly Version version = new Version(1, 95, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetActionStateBoolean(string path, ref Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetActionStateFloat(string path, ref float value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetActionStatePose(string path, ref Posef value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetDeveloperTelemetryConsent(Bool consent);
	}

	private static class OVRP_1_96_0
	{
		public static readonly Version version = new Version(1, 96, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerAnnotationVariant(int markerId, [MarshalAs(UnmanagedType.LPStr)] string annotationKey, in Qpl.Variant annotationValue, int instanceKey);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_QplMarkerPointData(int markerId, [MarshalAs(UnmanagedType.LPStr)] string name, Qpl.Annotation* annotations, int annotationCount, int instanceKey, long timestampMs);
	}

	private static class OVRP_1_97_0
	{
		public static readonly Version version = new Version(1, 97, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DiscoverSpaces(in SpaceDiscoveryInfo info, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RetrieveSpaceDiscoveryResults(ulong requestId, ref SpaceDiscoveryResults results);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_SaveSpaces(uint spaceCount, ulong* spaces, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public unsafe static extern Result ovrp_EraseSpaces(uint spaceCount, ulong* spaces, uint uuidCount, Guid* uuids, out ulong requestId);
	}

	private static class OVRP_1_98_0
	{
		public static readonly Version version = new Version(1, 98, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RequestBoundaryVisibility(BoundaryVisibility boundaryVisibility);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetBoundaryVisibility(out BoundaryVisibility boundaryVisibility);
	}

	private static class OVRP_1_99_0
	{
		public static readonly Version version = new Version(1, 99, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetTrackingPoseEnabledForInvisibleSession(out Bool trackingPoseEnabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetTrackingPoseEnabledForInvisibleSession(Bool trackingPoseEnabled);
	}

	private static class OVRP_1_100_0
	{
		public static readonly Version version = new Version(1, 100, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetCurrentInteractionProfileName(Hand hand, IntPtr interactionProfile);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetActionStatePose2(string path, Hand hand, ref Posef value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_TriggerVibrationAction(string actionName, Hand hand, float duration, float amplitude);
	}

	private static class OVRP_1_101_0
	{
		public static readonly Version version = new Version(1, 101, 0);
	}

	private static class OVRP_1_102_0
	{
		public static readonly Version version = new Version(1, 102, 0);
	}

	private static class OVRP_1_103_0
	{
		public static readonly Version version = new Version(1, 103, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetHandSkeletonVersion(OVRHandSkeletonVersion handSkeletonVersion);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandState3(Step stepId, int frameIndex, Hand hand, out HandState3Internal handState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_PollFuture(ulong future, out FutureState state);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CancelFuture(ulong future);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartColocationAdvertisement(in ColocationSessionStartAdvertisementInfo info, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopColocationAdvertisement(out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StartColocationDiscovery(out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_StopColocationDiscovery(out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_ShareSpaces2(in ShareSpacesInfo info, out ulong requestId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QuerySpaces2(ref SpaceQueryInfo2 queryInfo, out ulong requestId);
	}

	private static class OVRP_1_104_0
	{
		public static readonly Version version = new Version(1, 104, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetOpenXRInstanceProcAddrFunc(ref IntPtr func);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_RegisterOpenXREventHandler(OpenXREventDelegateType eventHandler, IntPtr context);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_UnregisterOpenXREventHandler(OpenXREventDelegateType eventHandler);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceVisemesState(Step stepId, int frameIndex, out FaceVisemesStateInternal faceVisemesState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetFaceTrackingVisemesSupported(out Bool faceTrackingVisemesSupported);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetFaceTrackingVisemesEnabled(Bool enabled);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateDynamicObjectTracker(out ulong tracker);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyDynamicObjectTracker(ulong tracker);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetDynamicObjectTrackedClasses(ulong tracker, in DynamicObjectTrackedClassesSetInfo setInfo);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceDynamicObjectData(ref ulong space, out DynamicObjectData data);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetDynamicObjectTrackerSupported(out Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetDynamicObjectKeyboardSupported(out Bool value);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_BeginProfilingRegion(string regionName);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_EndProfilingRegion();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetExternalLayerDynresEnabled(Bool enabled);
	}

	private static class OVRP_1_105_0
	{
		public static readonly Version version = new Version(1, 105, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_QplMarkerStartForJoin(int markerId, string joinId, Bool cancelMarkerIfAppBackgrounded, int instanceKey, long timestampMs);
	}

	private static class OVRP_1_106_0
	{
		public const int OVRP_CONSENT_TITLE_MAX_LENGTH = 256;

		public const int OVRP_CONSENT_TEXT_MAX_LENGTH = 2048;

		public const int OVRP_CONSENT_NOTIFICATION_MAX_LENGTH = 1024;

		public const int OVRP_CONSENT_SETTINGS_CHANGE_MAX_LENGTH = 1024;

		public static readonly Version version = new Version(1, 106, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetHandTrackingState(Step stepId, int frameIndex, Hand hand, out HandTrackingStateInternal handState);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SaveUnifiedConsent(int toolId, Bool consentValue);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SaveUnifiedConsentWithOlderVersion(int toolId, Bool consentValue, int consentVersion);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern OptionalBool ovrp_GetUnifiedConsent(int toolId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetConsentTitle(IntPtr title);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetConsentMarkdownText(IntPtr markdownText);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetConsentNotificationMarkdownText(IntPtr consentChangeLocationMarkdown, IntPtr markDownText);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ShouldShowTelemetryConsentWindow(int toolId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_IsConsentSettingsChangeEnabled(int toolId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Bool ovrp_ShouldShowTelemetryNotification(int toolId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendMicrogestureHint();

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SetNotificationShown(int tool);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetConsentSettingsChangeText(IntPtr consentSettingsChangeText);
	}

	private static class OVRP_1_107_0
	{
		public static readonly Version version = new Version(1, 107, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetAppSpace(ref ulong appSpace);
	}

	private static class OVRP_1_108_0
	{
		public static readonly Version version = new Version(1, 108, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_UnityOpenXR_OnAppSpaceChange2(ulong xrSpace, int spaceFlags);
	}

	private static class OVRP_1_109_0
	{
		public static readonly Version version = new Version(1, 109, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetStationaryReferenceSpaceId(out Guid generationId);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendUnifiedEvent(Bool isEssential, string productType, string eventName, string event_metadata_json, string project_name, string event_entrypoint, string project_guid, string event_type, string event_target, string error_msg, string is_internal);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovrp_AllowVisibilityMask(Bool enabled);
	}

	private static class OVRP_1_110_0
	{
		public static readonly Version version = new Version(1, 110, 0);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_SendUnifiedEventV2(Bool isEssential, string productType, string eventName, string event_metadata_json, string project_name, string event_entrypoint, string project_guid, string event_type, string event_target, string error_msg, string is_internal_build, string batch_mode);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateMarkerTrackerAsync(in MarkerTrackerCreateInfo createInfo, out ulong future);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_CreateMarkerTrackerComplete(ulong future, out MarkerTrackerCreateCompletion completion);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_DestroyMarkerTracker(ulong tracker);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetSpaceMarkerPayload(ulong space, ref SpaceMarkerPayload payload);

		[DllImport("OVRPlugin", CallingConvention = CallingConvention.Cdecl)]
		public static extern Result ovrp_GetMarkerTrackingSupported(out Bool value);
	}

	private static class OVRP_1_111_0
	{
		public static readonly Version version = new Version(1, 111, 0);
	}

	private static class OVRP_1_112_0
	{
		public static readonly Version version = new Version(1, 112, 0);
	}

	private static class OVRP_1_113_0
	{
		public static readonly Version version = new Version(1, 113, 0);
	}

	private static class OVRP_1_114_0
	{
		public static readonly Version version = new Version(1, 114, 0);
	}

	private static class OVRP_1_115_0
	{
		public static readonly Version version = new Version(1, 115, 0);
	}

	private static class OVRP_1_116_0
	{
		public static readonly Version version = new Version(1, 116, 0);
	}

	private static class OVRP_1_117_0
	{
		public static readonly Version version = new Version(1, 117, 0);
	}

	private static class OVRP_1_118_0
	{
		public static readonly Version version = new Version(1, 118, 0);
	}

	private static class OVRP_1_119_0
	{
		public static readonly Version version = new Version(1, 119, 0);
	}

	private static class OVRP_1_120_0
	{
		public static readonly Version version = new Version(1, 120, 0);
	}

	private static class OVRP_1_121_0
	{
		public static readonly Version version = new Version(1, 121, 0);
	}

	private static class OVRP_1_122_0
	{
		public static readonly Version version = new Version(1, 122, 0);
	}

	private static class OVRP_1_123_0
	{
		public static readonly Version version = new Version(1, 123, 0);
	}

	private static class OVRP_1_124_0
	{
		public static readonly Version version = new Version(1, 124, 0);
	}

	private static class OVRP_1_125_0
	{
		public static readonly Version version = new Version(1, 125, 0);
	}

	private static class OVRP_1_126_0
	{
		public static readonly Version version = new Version(1, 126, 0);
	}

	private static class OVRP_1_127_0
	{
		public static readonly Version version = new Version(1, 127, 0);
	}

	private static class OVRP_1_128_0
	{
		public static readonly Version version = new Version(1, 128, 0);
	}

	private static class OVRP_1_129_0
	{
		public static readonly Version version = new Version(1, 129, 0);
	}

	public const bool isSupportedPlatform = true;

	public static readonly Version wrapperVersion = OVRP_1_110_0.version;

	private static Version _version;

	private static Version _nativeSDKVersion;

	public static int MAX_CPU_CORES = 8;

	private const int OverlayShapeFlagShift = 4;

	public const int AppPerfFrameStatsMaxCount = 5;

	private const int EventDataBufferSize = 4000;

	public const int RENDER_MODEL_NULL_KEY = 0;

	public const int SpaceFilterInfoIdsMaxSize = 1024;

	public const int SpaceFilterInfoComponentsMaxSize = 16;

	public const int SpatialEntityMaxQueryResultsPerEvent = 128;

	public const int MaxQuerySpacesByGroup = 1024;

	private static XrApi? _nativeXrApi = null;

	private static GUID _nativeAudioOutGuid = new GUID();

	private static Guid _cachedAudioOutGuid;

	private static string _cachedAudioOutString;

	private static GUID _nativeAudioInGuid = new GUID();

	private static Guid _cachedAudioInGuid;

	private static string _cachedAudioInString;

	private static ProcessorPerformanceLevel m_suggestedCpuPerfLevelOpenXR = ProcessorPerformanceLevel.SustainedHigh;

	private static ProcessorPerformanceLevel m_suggestedGpuPerfLevelOpenXR = ProcessorPerformanceLevel.SustainedHigh;

	private static bool perfStatWarningPrinted = false;

	private static bool resetPerfStatWarningPrinted = false;

	private static Texture2D cachedCameraFrameTexture = null;

	private static Texture2D cachedCameraDepthTexture = null;

	private static Texture2D cachedCameraDepthConfidenceTexture = null;

	private static OVRNativeBuffer _nativeSystemDisplayFrequenciesAvailable = null;

	private static float[] _cachedSystemDisplayFrequenciesAvailable = null;

	private static HandStateInternal cachedHandState = default(HandStateInternal);

	private static HandState3Internal cachedHandState3 = default(HandState3Internal);

	private static Quaternion LeftBoneRotator = Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up);

	private static Quaternion RightBoneRotator = Quaternion.AngleAxis(270f, Vector3.up);

	private static HandTrackingStateInternal cachedHandTrackingState = default(HandTrackingStateInternal);

	private static Skeleton cachedSkeleton = default(Skeleton);

	private static Skeleton2Internal cachedSkeleton2 = default(Skeleton2Internal);

	private static GetBoneSkeleton2Delegate[] Skeleton2GetBone = new GetBoneSkeleton2Delegate[70]
	{
		() => cachedSkeleton2.Bones_0,
		() => cachedSkeleton2.Bones_1,
		() => cachedSkeleton2.Bones_2,
		() => cachedSkeleton2.Bones_3,
		() => cachedSkeleton2.Bones_4,
		() => cachedSkeleton2.Bones_5,
		() => cachedSkeleton2.Bones_6,
		() => cachedSkeleton2.Bones_7,
		() => cachedSkeleton2.Bones_8,
		() => cachedSkeleton2.Bones_9,
		() => cachedSkeleton2.Bones_10,
		() => cachedSkeleton2.Bones_11,
		() => cachedSkeleton2.Bones_12,
		() => cachedSkeleton2.Bones_13,
		() => cachedSkeleton2.Bones_14,
		() => cachedSkeleton2.Bones_15,
		() => cachedSkeleton2.Bones_16,
		() => cachedSkeleton2.Bones_17,
		() => cachedSkeleton2.Bones_18,
		() => cachedSkeleton2.Bones_19,
		() => cachedSkeleton2.Bones_20,
		() => cachedSkeleton2.Bones_21,
		() => cachedSkeleton2.Bones_22,
		() => cachedSkeleton2.Bones_23,
		() => cachedSkeleton2.Bones_24,
		() => cachedSkeleton2.Bones_25,
		() => cachedSkeleton2.Bones_26,
		() => cachedSkeleton2.Bones_27,
		() => cachedSkeleton2.Bones_28,
		() => cachedSkeleton2.Bones_29,
		() => cachedSkeleton2.Bones_30,
		() => cachedSkeleton2.Bones_31,
		() => cachedSkeleton2.Bones_32,
		() => cachedSkeleton2.Bones_33,
		() => cachedSkeleton2.Bones_34,
		() => cachedSkeleton2.Bones_35,
		() => cachedSkeleton2.Bones_36,
		() => cachedSkeleton2.Bones_37,
		() => cachedSkeleton2.Bones_38,
		() => cachedSkeleton2.Bones_39,
		() => cachedSkeleton2.Bones_40,
		() => cachedSkeleton2.Bones_41,
		() => cachedSkeleton2.Bones_42,
		() => cachedSkeleton2.Bones_43,
		() => cachedSkeleton2.Bones_44,
		() => cachedSkeleton2.Bones_45,
		() => cachedSkeleton2.Bones_46,
		() => cachedSkeleton2.Bones_47,
		() => cachedSkeleton2.Bones_48,
		() => cachedSkeleton2.Bones_49,
		() => cachedSkeleton2.Bones_50,
		() => cachedSkeleton2.Bones_51,
		() => cachedSkeleton2.Bones_52,
		() => cachedSkeleton2.Bones_53,
		() => cachedSkeleton2.Bones_54,
		() => cachedSkeleton2.Bones_55,
		() => cachedSkeleton2.Bones_56,
		() => cachedSkeleton2.Bones_57,
		() => cachedSkeleton2.Bones_58,
		() => cachedSkeleton2.Bones_59,
		() => cachedSkeleton2.Bones_60,
		() => cachedSkeleton2.Bones_61,
		() => cachedSkeleton2.Bones_62,
		() => cachedSkeleton2.Bones_63,
		() => cachedSkeleton2.Bones_64,
		() => cachedSkeleton2.Bones_65,
		() => cachedSkeleton2.Bones_66,
		() => cachedSkeleton2.Bones_67,
		() => cachedSkeleton2.Bones_68,
		() => cachedSkeleton2.Bones_69
	};

	private static Skeleton3Internal cachedSkeleton3 = default(Skeleton3Internal);

	private static GetBoneSkeleton3Delegate[] Skeleton3GetBone = new GetBoneSkeleton3Delegate[84]
	{
		() => cachedSkeleton3.Bones_0,
		() => cachedSkeleton3.Bones_1,
		() => cachedSkeleton3.Bones_2,
		() => cachedSkeleton3.Bones_3,
		() => cachedSkeleton3.Bones_4,
		() => cachedSkeleton3.Bones_5,
		() => cachedSkeleton3.Bones_6,
		() => cachedSkeleton3.Bones_7,
		() => cachedSkeleton3.Bones_8,
		() => cachedSkeleton3.Bones_9,
		() => cachedSkeleton3.Bones_10,
		() => cachedSkeleton3.Bones_11,
		() => cachedSkeleton3.Bones_12,
		() => cachedSkeleton3.Bones_13,
		() => cachedSkeleton3.Bones_14,
		() => cachedSkeleton3.Bones_15,
		() => cachedSkeleton3.Bones_16,
		() => cachedSkeleton3.Bones_17,
		() => cachedSkeleton3.Bones_18,
		() => cachedSkeleton3.Bones_19,
		() => cachedSkeleton3.Bones_20,
		() => cachedSkeleton3.Bones_21,
		() => cachedSkeleton3.Bones_22,
		() => cachedSkeleton3.Bones_23,
		() => cachedSkeleton3.Bones_24,
		() => cachedSkeleton3.Bones_25,
		() => cachedSkeleton3.Bones_26,
		() => cachedSkeleton3.Bones_27,
		() => cachedSkeleton3.Bones_28,
		() => cachedSkeleton3.Bones_29,
		() => cachedSkeleton3.Bones_30,
		() => cachedSkeleton3.Bones_31,
		() => cachedSkeleton3.Bones_32,
		() => cachedSkeleton3.Bones_33,
		() => cachedSkeleton3.Bones_34,
		() => cachedSkeleton3.Bones_35,
		() => cachedSkeleton3.Bones_36,
		() => cachedSkeleton3.Bones_37,
		() => cachedSkeleton3.Bones_38,
		() => cachedSkeleton3.Bones_39,
		() => cachedSkeleton3.Bones_40,
		() => cachedSkeleton3.Bones_41,
		() => cachedSkeleton3.Bones_42,
		() => cachedSkeleton3.Bones_43,
		() => cachedSkeleton3.Bones_44,
		() => cachedSkeleton3.Bones_45,
		() => cachedSkeleton3.Bones_46,
		() => cachedSkeleton3.Bones_47,
		() => cachedSkeleton3.Bones_48,
		() => cachedSkeleton3.Bones_49,
		() => cachedSkeleton3.Bones_50,
		() => cachedSkeleton3.Bones_51,
		() => cachedSkeleton3.Bones_52,
		() => cachedSkeleton3.Bones_53,
		() => cachedSkeleton3.Bones_54,
		() => cachedSkeleton3.Bones_55,
		() => cachedSkeleton3.Bones_56,
		() => cachedSkeleton3.Bones_57,
		() => cachedSkeleton3.Bones_58,
		() => cachedSkeleton3.Bones_59,
		() => cachedSkeleton3.Bones_60,
		() => cachedSkeleton3.Bones_61,
		() => cachedSkeleton3.Bones_62,
		() => cachedSkeleton3.Bones_63,
		() => cachedSkeleton3.Bones_64,
		() => cachedSkeleton3.Bones_65,
		() => cachedSkeleton3.Bones_66,
		() => cachedSkeleton3.Bones_67,
		() => cachedSkeleton3.Bones_68,
		() => cachedSkeleton3.Bones_69,
		() => cachedSkeleton3.Bones_70,
		() => cachedSkeleton3.Bones_71,
		() => cachedSkeleton3.Bones_72,
		() => cachedSkeleton3.Bones_73,
		() => cachedSkeleton3.Bones_74,
		() => cachedSkeleton3.Bones_75,
		() => cachedSkeleton3.Bones_76,
		() => cachedSkeleton3.Bones_77,
		() => cachedSkeleton3.Bones_78,
		() => cachedSkeleton3.Bones_79,
		() => cachedSkeleton3.Bones_80,
		() => cachedSkeleton3.Bones_81,
		() => cachedSkeleton3.Bones_82,
		() => cachedSkeleton3.Bones_83
	};

	private static FaceStateInternal cachedFaceState = default(FaceStateInternal);

	private static FaceState2Internal cachedFaceState2 = default(FaceState2Internal);

	private static FaceVisemesStateInternal cachedFaceVisemesState = default(FaceVisemesStateInternal);

	private static EyeGazesStateInternal cachedEyeGazesState = default(EyeGazesStateInternal);

	private static BodyJointSet _currentJointSet = BodyJointSet.None;

	private const string pluginName = "OVRPlugin";

	private static Version _versionZero = new Version(0, 0, 0);

	public static Version version
	{
		get
		{
			if (_version == null)
			{
				try
				{
					string text = OVRP_1_1_0.ovrp_GetVersion();
					if (text != null)
					{
						text = text.Split('-')[0];
						_version = new Version(text);
					}
					else
					{
						_version = _versionZero;
					}
				}
				catch
				{
					_version = _versionZero;
				}
				if (_version == OVRP_0_5_0.version)
				{
					_version = OVRP_0_1_0.version;
				}
				if (_version > _versionZero && _version < OVRP_1_3_0.version)
				{
					throw new PlatformNotSupportedException("Oculus Utilities version " + wrapperVersion?.ToString() + " is too new for OVRPlugin version " + _version.ToString() + ". Update to the latest version of Unity.");
				}
			}
			return _version;
		}
	}

	public static Version nativeSDKVersion
	{
		get
		{
			if (_nativeSDKVersion == null)
			{
				try
				{
					string empty = string.Empty;
					empty = ((!(version >= OVRP_1_1_0.version)) ? _versionZero.ToString() : OVRP_1_1_0.ovrp_GetNativeSDKVersion());
					if (empty != null)
					{
						empty = empty.Split('-')[0];
						_nativeSDKVersion = new Version(empty);
					}
					else
					{
						_nativeSDKVersion = _versionZero;
					}
				}
				catch
				{
					_nativeSDKVersion = _versionZero;
				}
			}
			return _nativeSDKVersion;
		}
	}

	public static bool initialized => OVRP_1_1_0.ovrp_GetInitialized() == Bool.True;

	public static XrApi nativeXrApi
	{
		get
		{
			if (!_nativeXrApi.HasValue)
			{
				_nativeXrApi = XrApi.Unknown;
				if (version >= OVRP_1_55_0.version && OVRP_1_55_0.ovrp_GetNativeXrApiType(out var xrApi) == Result.Success)
				{
					_nativeXrApi = xrApi;
				}
			}
			return _nativeXrApi.Value;
		}
	}

	public static bool chromatic
	{
		get
		{
			if (version >= OVRP_1_7_0.version)
			{
				if (initialized)
				{
					return OVRP_1_7_0.ovrp_GetAppChromaticCorrection() == Bool.True;
				}
				return false;
			}
			return true;
		}
		set
		{
			if (initialized && version >= OVRP_1_7_0.version)
			{
				OVRP_1_7_0.ovrp_SetAppChromaticCorrection(ToBool(value));
			}
		}
	}

	public static bool monoscopic
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetAppMonoscopic() == Bool.True;
			}
			return false;
		}
		set
		{
			if (initialized)
			{
				OVRP_1_1_0.ovrp_SetAppMonoscopic(ToBool(value));
			}
		}
	}

	public static bool rotation
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetTrackingOrientationEnabled() == Bool.True;
			}
			return false;
		}
		set
		{
			if (initialized)
			{
				OVRP_1_1_0.ovrp_SetTrackingOrientationEnabled(ToBool(value));
			}
		}
	}

	public static bool position
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetTrackingPositionEnabled() == Bool.True;
			}
			return false;
		}
		set
		{
			if (initialized)
			{
				OVRP_1_1_0.ovrp_SetTrackingPositionEnabled(ToBool(value));
			}
		}
	}

	public static bool useIPDInPositionTracking
	{
		get
		{
			if (initialized && version >= OVRP_1_6_0.version)
			{
				return OVRP_1_6_0.ovrp_GetTrackingIPDEnabled() == Bool.True;
			}
			return true;
		}
		set
		{
			if (initialized && version >= OVRP_1_6_0.version)
			{
				OVRP_1_6_0.ovrp_SetTrackingIPDEnabled(ToBool(value));
			}
		}
	}

	public static bool positionSupported
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetTrackingPositionSupported() == Bool.True;
			}
			return false;
		}
	}

	public static bool positionTracked
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetNodePositionTracked(Node.EyeCenter) == Bool.True;
			}
			return false;
		}
	}

	public static bool powerSaving
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetSystemPowerSavingMode() == Bool.True;
			}
			return false;
		}
	}

	public static bool hmdPresent
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetNodePresent(Node.EyeCenter) == Bool.True;
			}
			return false;
		}
	}

	public static bool userPresent
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_1_0.ovrp_GetUserPresent() == Bool.True;
			}
			return false;
		}
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static bool headphonesPresent
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_3_0.ovrp_GetSystemHeadphonesPresent() == Bool.True;
			}
			return false;
		}
	}

	public static int recommendedMSAALevel
	{
		get
		{
			if (version >= OVRP_1_6_0.version)
			{
				return OVRP_1_6_0.ovrp_GetSystemRecommendedMSAALevel();
			}
			return 2;
		}
	}

	public static SystemRegion systemRegion
	{
		get
		{
			if (initialized && version >= OVRP_1_5_0.version)
			{
				return OVRP_1_5_0.ovrp_GetSystemRegion();
			}
			return SystemRegion.Unspecified;
		}
	}

	public static string audioOutId
	{
		get
		{
			try
			{
				if (_nativeAudioOutGuid == null)
				{
					_nativeAudioOutGuid = new GUID();
				}
				IntPtr intPtr = OVRP_1_1_0.ovrp_GetAudioOutId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, _nativeAudioOutGuid);
					Guid guid = new Guid(_nativeAudioOutGuid.a, _nativeAudioOutGuid.b, _nativeAudioOutGuid.c, _nativeAudioOutGuid.d0, _nativeAudioOutGuid.d1, _nativeAudioOutGuid.d2, _nativeAudioOutGuid.d3, _nativeAudioOutGuid.d4, _nativeAudioOutGuid.d5, _nativeAudioOutGuid.d6, _nativeAudioOutGuid.d7);
					if (guid != _cachedAudioOutGuid)
					{
						_cachedAudioOutGuid = guid;
						_cachedAudioOutString = _cachedAudioOutGuid.ToString();
					}
					return _cachedAudioOutString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static string audioInId
	{
		get
		{
			try
			{
				if (_nativeAudioInGuid == null)
				{
					_nativeAudioInGuid = new GUID();
				}
				IntPtr intPtr = OVRP_1_1_0.ovrp_GetAudioInId();
				if (intPtr != IntPtr.Zero)
				{
					Marshal.PtrToStructure(intPtr, _nativeAudioInGuid);
					Guid guid = new Guid(_nativeAudioInGuid.a, _nativeAudioInGuid.b, _nativeAudioInGuid.c, _nativeAudioInGuid.d0, _nativeAudioInGuid.d1, _nativeAudioInGuid.d2, _nativeAudioInGuid.d3, _nativeAudioInGuid.d4, _nativeAudioInGuid.d5, _nativeAudioInGuid.d6, _nativeAudioInGuid.d7);
					if (guid != _cachedAudioInGuid)
					{
						_cachedAudioInGuid = guid;
						_cachedAudioInString = _cachedAudioInGuid.ToString();
					}
					return _cachedAudioInString;
				}
			}
			catch
			{
			}
			return string.Empty;
		}
	}

	public static bool hasVrFocus => OVRP_1_1_0.ovrp_GetAppHasVrFocus() == Bool.True;

	public static bool hasInputFocus
	{
		get
		{
			if (version >= OVRP_1_18_0.version)
			{
				Bool appHasInputFocus = Bool.False;
				if (OVRP_1_18_0.ovrp_GetAppHasInputFocus(out appHasInputFocus) == Result.Success)
				{
					return appHasInputFocus == Bool.True;
				}
				return false;
			}
			return true;
		}
	}

	public static bool shouldQuit => OVRP_1_1_0.ovrp_GetAppShouldQuit() == Bool.True;

	public static bool shouldRecenter => OVRP_1_1_0.ovrp_GetAppShouldRecenter() == Bool.True;

	public static string productName => OVRP_1_1_0.ovrp_GetSystemProductName();

	public static string latency
	{
		get
		{
			if (!initialized)
			{
				return string.Empty;
			}
			return OVRP_1_1_0.ovrp_GetAppLatencyTimings();
		}
	}

	public static float eyeDepth
	{
		get
		{
			if (!initialized)
			{
				return 0f;
			}
			return OVRP_1_1_0.ovrp_GetUserEyeDepth();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserEyeDepth(value);
		}
	}

	public static float eyeHeight
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetUserEyeHeight();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserEyeHeight(value);
		}
	}

	[Obsolete("Deprecated. Please use SystemInfo.batteryLevel", false)]
	public static float batteryLevel => OVRP_1_1_0.ovrp_GetSystemBatteryLevel();

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static float batteryTemperature => OVRP_1_1_0.ovrp_GetSystemBatteryTemperature();

	public static ProcessorPerformanceLevel suggestedCpuPerfLevel
	{
		get
		{
			return m_suggestedCpuPerfLevelOpenXR;
		}
		set
		{
			m_suggestedCpuPerfLevelOpenXR = value;
			XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Cpu, ProcessorPerformanceLevelToPerformanceLevelHint(value));
		}
	}

	public static ProcessorPerformanceLevel suggestedGpuPerfLevel
	{
		get
		{
			return m_suggestedGpuPerfLevelOpenXR;
		}
		set
		{
			m_suggestedGpuPerfLevelOpenXR = value;
			XrPerformanceSettingsFeature.SetPerformanceLevelHint(PerformanceDomain.Gpu, ProcessorPerformanceLevelToPerformanceLevelHint(value));
		}
	}

	[Obsolete("Deprecated. Please use suggestedCpuPerfLevel.", false)]
	public static int cpuLevel
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemCpuLevel();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetSystemCpuLevel(value);
		}
	}

	[Obsolete("Deprecated. Please use suggestedGpuPerfLevel.", false)]
	public static int gpuLevel
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemGpuLevel();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetSystemGpuLevel(value);
		}
	}

	public static int vsyncCount
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetSystemVSyncCount();
		}
		set
		{
			OVRP_1_2_0.ovrp_SetSystemVSyncCount(value);
		}
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static float systemVolume => OVRP_1_1_0.ovrp_GetSystemVolume();

	public static float ipd
	{
		get
		{
			return OVRP_1_1_0.ovrp_GetUserIPD();
		}
		set
		{
			OVRP_1_1_0.ovrp_SetUserIPD(value);
		}
	}

	public static bool occlusionMesh
	{
		get
		{
			if (initialized)
			{
				return OVRP_1_3_0.ovrp_GetEyeOcclusionMeshEnabled() == Bool.True;
			}
			return false;
		}
		set
		{
			if (initialized)
			{
				OVRP_1_3_0.ovrp_SetEyeOcclusionMeshEnabled(ToBool(value));
			}
		}
	}

	public static bool premultipliedAlphaLayersSupported
	{
		get
		{
			if (!Application.isMobilePlatform)
			{
				return version >= OVRP_1_3_0.version;
			}
			return false;
		}
	}

	public static bool unpremultipliedAlphaLayersSupported => Application.isMobilePlatform;

	[Obsolete("Deprecated. Please use SystemInfo.batteryStatus", false)]
	public static BatteryStatus batteryStatus => OVRP_1_1_0.ovrp_GetSystemBatteryStatus();

	private static bool foveatedRenderingSupported
	{
		get
		{
			if (!fixedFoveatedRenderingSupported)
			{
				return eyeTrackedFoveatedRenderingSupported;
			}
			return true;
		}
	}

	public static bool eyeTrackedFoveatedRenderingSupported
	{
		get
		{
			if (version >= OVRP_1_78_0.version)
			{
				OVRP_1_78_0.ovrp_GetFoveationEyeTrackedSupported(out var foveationSupported);
				return foveationSupported == Bool.True;
			}
			return false;
		}
	}

	public static bool eyeTrackedFoveatedRenderingEnabled
	{
		get
		{
			if (version >= OVRP_1_78_0.version && eyeTrackedFoveatedRenderingSupported)
			{
				OVRP_1_78_0.ovrp_GetFoveationEyeTracked(out var isEyeTrackedFoveation);
				return isEyeTrackedFoveation == Bool.True;
			}
			return false;
		}
		set
		{
			if (version >= OVRP_1_78_0.version && eyeTrackedFoveatedRenderingSupported)
			{
				OVRP_1_78_0.ovrp_SetFoveationEyeTracked(value ? Bool.True : Bool.False);
			}
		}
	}

	public static bool fixedFoveatedRenderingSupported
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				if (OVRP_1_21_0.ovrp_GetTiledMultiResSupported(out var foveationSupported) == Result.Success)
				{
					return foveationSupported == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	public static FoveatedRenderingLevel foveatedRenderingLevel
	{
		get
		{
			if (version >= OVRP_1_21_0.version && foveatedRenderingSupported)
			{
				OVRP_1_21_0.ovrp_GetTiledMultiResLevel(out var level);
				return level;
			}
			return FoveatedRenderingLevel.Off;
		}
		set
		{
			if (version >= OVRP_1_21_0.version && foveatedRenderingSupported)
			{
				if (value == FoveatedRenderingLevel.HighTop)
				{
					value = FoveatedRenderingLevel.High;
					Debug.LogWarning("FoveatedRenderingLevel.HighTop is not supported in OpenXR, changed to FoveatedRenderingLevel.High instead.");
				}
				OVRP_1_21_0.ovrp_SetTiledMultiResLevel(value);
			}
		}
	}

	[Obsolete("Please use foveatedRenderingLevel instead", false)]
	public static FixedFoveatedRenderingLevel fixedFoveatedRenderingLevel
	{
		get
		{
			return (FixedFoveatedRenderingLevel)foveatedRenderingLevel;
		}
		set
		{
			foveatedRenderingLevel = (FoveatedRenderingLevel)value;
		}
	}

	public static bool useDynamicFoveatedRendering
	{
		get
		{
			if (version >= OVRP_1_46_0.version && foveatedRenderingSupported)
			{
				Bool isDynamic = Bool.False;
				OVRP_1_46_0.ovrp_GetTiledMultiResDynamic(out isDynamic);
				return isDynamic != Bool.False;
			}
			return false;
		}
		set
		{
			if (version >= OVRP_1_46_0.version && foveatedRenderingSupported)
			{
				OVRP_1_46_0.ovrp_SetTiledMultiResDynamic(value ? Bool.True : Bool.False);
			}
		}
	}

	[Obsolete("Please use useDynamicFoveatedRendering instead", false)]
	public static bool useDynamicFixedFoveatedRendering
	{
		get
		{
			return useDynamicFoveatedRendering;
		}
		set
		{
			useDynamicFoveatedRendering = value;
		}
	}

	[Obsolete("Please use fixedFoveatedRenderingSupported instead", false)]
	public static bool tiledMultiResSupported => fixedFoveatedRenderingSupported;

	[Obsolete("Please use foveatedRenderingLevel instead", false)]
	public static TiledMultiResLevel tiledMultiResLevel
	{
		get
		{
			return (TiledMultiResLevel)foveatedRenderingLevel;
		}
		set
		{
			foveatedRenderingLevel = (FoveatedRenderingLevel)value;
		}
	}

	public static bool gpuUtilSupported
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				if (OVRP_1_21_0.ovrp_GetGPUUtilSupported(out var @bool) == Result.Success)
				{
					return @bool == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	public static float gpuUtilLevel
	{
		get
		{
			if (version >= OVRP_1_21_0.version && gpuUtilSupported)
			{
				if (OVRP_1_21_0.ovrp_GetGPUUtilLevel(out var gpuUtil) == Result.Success)
				{
					return gpuUtil;
				}
				return 0f;
			}
			return 0f;
		}
	}

	public static float[] systemDisplayFrequenciesAvailable
	{
		get
		{
			if (_cachedSystemDisplayFrequenciesAvailable == null)
			{
				_cachedSystemDisplayFrequenciesAvailable = new float[0];
				if (version >= OVRP_1_21_0.version)
				{
					int numFrequencies = 0;
					if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(IntPtr.Zero, ref numFrequencies) == Result.Success && numFrequencies > 0)
					{
						int num = numFrequencies;
						_nativeSystemDisplayFrequenciesAvailable = new OVRNativeBuffer(4 * num);
						if (OVRP_1_21_0.ovrp_GetSystemDisplayAvailableFrequencies(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), ref numFrequencies) == Result.Success)
						{
							int num2 = ((numFrequencies <= num) ? numFrequencies : num);
							if (num2 > 0)
							{
								_cachedSystemDisplayFrequenciesAvailable = new float[num2];
								Marshal.Copy(_nativeSystemDisplayFrequenciesAvailable.GetPointer(), _cachedSystemDisplayFrequenciesAvailable, 0, num2);
							}
						}
					}
				}
			}
			return _cachedSystemDisplayFrequenciesAvailable;
		}
	}

	public static float systemDisplayFrequency
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				if (OVRP_1_21_0.ovrp_GetSystemDisplayFrequency2(out var result) == Result.Success)
				{
					return result;
				}
				return 0f;
			}
			if (version >= OVRP_1_1_0.version)
			{
				return OVRP_1_1_0.ovrp_GetSystemDisplayFrequency();
			}
			return 0f;
		}
		set
		{
			if (version >= OVRP_1_21_0.version)
			{
				OVRP_1_21_0.ovrp_SetSystemDisplayFrequency(value);
			}
		}
	}

	public static bool eyeFovPremultipliedAlphaModeEnabled
	{
		get
		{
			Bool enabled = Bool.True;
			if (version >= OVRP_1_57_0.version)
			{
				OVRP_1_57_0.ovrp_GetEyeFovPremultipliedAlphaMode(ref enabled);
			}
			if (enabled != Bool.True)
			{
				return false;
			}
			return true;
		}
		set
		{
			if (version >= OVRP_1_57_0.version)
			{
				OVRP_1_57_0.ovrp_SetEyeFovPremultipliedAlphaMode(ToBool(value));
			}
		}
	}

	public static bool AsymmetricFovEnabled
	{
		get
		{
			if (version >= OVRP_1_21_0.version)
			{
				Bool useAsymmetricFov = Bool.False;
				if (OVRP_1_21_0.ovrp_GetAppAsymmetricFov(out useAsymmetricFov) != 0)
				{
					return false;
				}
				return useAsymmetricFov == Bool.True;
			}
			return false;
		}
	}

	public static bool EyeTextureArrayEnabled
	{
		get
		{
			if (version >= OVRP_1_15_0.version)
			{
				return OVRP_1_15_0.ovrp_GetEyeTextureArrayEnabled() == Bool.True;
			}
			return false;
		}
	}

	public static bool localDimmingSupported
	{
		get
		{
			if (version >= OVRP_1_78_0.version)
			{
				Bool @bool = Bool.False;
				if (OVRP_1_78_0.ovrp_GetLocalDimmingSupported(out @bool) == Result.Success)
				{
					return @bool == Bool.True;
				}
				return false;
			}
			return false;
		}
	}

	public static bool localDimming
	{
		get
		{
			if (version >= OVRP_1_78_0.version && localDimmingSupported)
			{
				Bool localDimmingMode = Bool.False;
				if (OVRP_1_78_0.ovrp_GetLocalDimming(out localDimmingMode) == Result.Success)
				{
					if (localDimmingMode != Bool.True)
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}
		set
		{
			if (version >= OVRP_1_78_0.version && localDimmingSupported)
			{
				OVRP_1_78_0.ovrp_SetLocalDimming(value ? Bool.True : Bool.False);
			}
		}
	}

	public static OVRHandSkeletonVersion HandSkeletonVersion { get; private set; } = OVRHandSkeletonVersion.OVR;


	public static bool bodyTrackingSupported
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetBodyTrackingSupported(out var value) == Result.Success)
			{
				return value == Bool.True;
			}
			return false;
		}
	}

	public static bool bodyTrackingEnabled
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetBodyTrackingEnabled(out var value) == Result.Success)
			{
				return value == Bool.True;
			}
			return false;
		}
	}

	public static bool faceTrackingEnabled
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetFaceTrackingEnabled(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool faceTrackingSupported
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetFaceTrackingSupported(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool eyeTrackingEnabled
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetEyeTrackingEnabled(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool eyeTrackingSupported
	{
		get
		{
			if (version >= OVRP_1_78_0.version && OVRP_1_78_0.ovrp_GetEyeTrackingSupported(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool faceTracking2Enabled
	{
		get
		{
			if (version >= OVRP_1_92_0.version && OVRP_1_92_0.ovrp_GetFaceTracking2Enabled(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool faceTracking2Supported
	{
		get
		{
			if (version >= OVRP_1_92_0.version && OVRP_1_92_0.ovrp_GetFaceTracking2Supported(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool faceTrackingVisemesSupported
	{
		get
		{
			if (version >= OVRP_1_104_0.version && OVRP_1_104_0.ovrp_GetFaceTrackingVisemesSupported(out var @bool) == Result.Success)
			{
				return @bool == Bool.True;
			}
			return false;
		}
	}

	public static bool IsSuccess(this Result result)
	{
		return result >= Result.Success;
	}

	public static void SetLogCallback2(LogCallback2DelegateType logCallback)
	{
		if (version >= OVRP_1_70_0.version && OVRP_1_70_0.ovrp_SetLogCallback2(logCallback) != 0)
		{
			Debug.LogWarning("OVRPlugin.SetLogCallback2() failed");
		}
	}

	public static bool IsPassthroughShape(OverlayShape shape)
	{
		if (shape != OverlayShape.ReconstructionPassthrough && shape != OverlayShape.KeyboardHandsPassthrough && shape != OverlayShape.KeyboardMaskedHandsPassthrough)
		{
			return shape == OverlayShape.SurfaceProjectedPassthrough;
		}
		return true;
	}

	public static bool IsPositionValid(this SpaceLocationFlags value)
	{
		return (value & SpaceLocationFlags.PositionValid) != 0;
	}

	public static bool IsOrientationValid(this SpaceLocationFlags value)
	{
		return (value & SpaceLocationFlags.OrientationValid) != 0;
	}

	public static bool IsPositionTracked(this SpaceLocationFlags value)
	{
		return (value & SpaceLocationFlags.PositionTracked) != 0;
	}

	public static bool IsOrientationTracked(this SpaceLocationFlags value)
	{
		return (value & SpaceLocationFlags.OrientationTracked) != 0;
	}

	public static string GuidToUuidString(Guid guid)
	{
		string text = BitConverter.ToString(guid.ToByteArray()).Replace("-", "").ToLower();
		StringBuilder stringBuilder = new StringBuilder(36);
		for (int i = 0; i < 32; i++)
		{
			stringBuilder.Append(text[i]);
			if (i == 7 || i == 11 || i == 15 || i == 19)
			{
				stringBuilder.Append("-");
			}
		}
		return stringBuilder.ToString();
	}

	private static PerformanceLevelHint ProcessorPerformanceLevelToPerformanceLevelHint(ProcessorPerformanceLevel level)
	{
		return level switch
		{
			ProcessorPerformanceLevel.PowerSavings => PerformanceLevelHint.PowerSavings, 
			ProcessorPerformanceLevel.SustainedLow => PerformanceLevelHint.SustainedLow, 
			ProcessorPerformanceLevel.SustainedHigh => PerformanceLevelHint.SustainedHigh, 
			ProcessorPerformanceLevel.Boost => PerformanceLevelHint.Boost, 
			_ => PerformanceLevelHint.SustainedHigh, 
		};
	}

	public static Frustumf GetEyeFrustum(Eye eyeId)
	{
		return OVRP_1_1_0.ovrp_GetNodeFrustum((Node)eyeId);
	}

	public static Sizei GetEyeTextureSize(Eye eyeId)
	{
		return OVRP_0_1_0.ovrp_GetEyeTextureSize(eyeId);
	}

	public static Posef GetTrackerPose(Tracker trackerId)
	{
		return GetNodePose((Node)(trackerId + 5), Step.Render);
	}

	public static Frustumf GetTrackerFrustum(Tracker trackerId)
	{
		return OVRP_1_1_0.ovrp_GetNodeFrustum((Node)(trackerId + 5));
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static bool ShowUI(PlatformUI ui)
	{
		return OVRP_1_1_0.ovrp_ShowSystemUI(ui) == Bool.True;
	}

	public static bool EnqueueSubmitLayer(bool onTop, bool headLocked, bool noDepthBufferTesting, IntPtr leftTexture, IntPtr rightTexture, int layerId, int frameIndex, Posef pose, Vector3f scale, int layerIndex = 0, OverlayShape shape = OverlayShape.Quad, bool overrideTextureRectMatrix = false, TextureRectMatrixf textureRectMatrix = default(TextureRectMatrixf), bool overridePerLayerColorScaleAndOffset = false, Vector4 colorScale = default(Vector4), Vector4 colorOffset = default(Vector4), bool expensiveSuperSample = false, bool bicubic = false, bool efficientSuperSample = false, bool efficientSharpen = false, bool expensiveSharpen = false, bool hidden = false, bool secureContent = false, bool automaticFiltering = false, bool premultipledAlpha = false)
	{
		if (!initialized)
		{
			return false;
		}
		if (version >= OVRP_1_6_0.version)
		{
			uint num = 0u;
			if (onTop)
			{
				num |= 1u;
			}
			if (headLocked)
			{
				num |= 2u;
			}
			if (noDepthBufferTesting)
			{
				num |= 4u;
			}
			if (expensiveSuperSample)
			{
				num |= 8u;
			}
			if (hidden)
			{
				num |= 0x200u;
			}
			if (efficientSuperSample)
			{
				num |= 0x10u;
			}
			if (expensiveSharpen)
			{
				num |= 0x80u;
			}
			if (efficientSharpen)
			{
				num |= 0x20u;
			}
			if (bicubic)
			{
				num |= 0x40u;
			}
			if (secureContent)
			{
				num |= 0x100u;
			}
			if (automaticFiltering)
			{
				num |= 0x400u;
			}
			if (premultipledAlpha)
			{
				num |= 0x100000u;
			}
			if (shape == OverlayShape.Cylinder || shape == OverlayShape.Cubemap)
			{
				if (shape == OverlayShape.Cubemap && version < OVRP_1_10_0.version)
				{
					return false;
				}
				if (shape == OverlayShape.Cylinder && version < OVRP_1_16_0.version)
				{
					return false;
				}
			}
			switch (shape)
			{
			case OverlayShape.OffcenterCubemap:
				return false;
			case OverlayShape.Equirect:
				return false;
			case OverlayShape.Fisheye:
				return false;
			default:
				if (version >= OVRP_1_34_0.version && layerId != -1)
				{
					return OVRP_1_34_0.ovrp_EnqueueSubmitLayer2(num, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex, overrideTextureRectMatrix ? Bool.True : Bool.False, ref textureRectMatrix, overridePerLayerColorScaleAndOffset ? Bool.True : Bool.False, ref colorScale, ref colorOffset) == Result.Success;
				}
				if (version >= OVRP_1_15_0.version && layerId != -1)
				{
					return OVRP_1_15_0.ovrp_EnqueueSubmitLayer(num, leftTexture, rightTexture, layerId, frameIndex, ref pose, ref scale, layerIndex) == Result.Success;
				}
				return OVRP_1_6_0.ovrp_SetOverlayQuad3(num, leftTexture, rightTexture, IntPtr.Zero, pose, scale, layerIndex) == Bool.True;
			}
		}
		if (layerIndex != 0)
		{
			return false;
		}
		return OVRP_0_1_1.ovrp_SetOverlayQuad2(ToBool(onTop), ToBool(headLocked), leftTexture, IntPtr.Zero, pose, scale) == Bool.True;
	}

	public static LayerDesc CalculateLayerDesc(OverlayShape shape, LayerLayout layout, Sizei textureSize, int mipLevels, int sampleCount, EyeTextureFormat format, int layerFlags)
	{
		if (!initialized || version < OVRP_1_15_0.version)
		{
			return default(LayerDesc);
		}
		LayerDesc layerDesc = default(LayerDesc);
		OVRP_1_15_0.ovrp_CalculateLayerDesc(shape, layout, ref textureSize, mipLevels, sampleCount, format, layerFlags, ref layerDesc);
		return layerDesc;
	}

	public static bool EnqueueSetupLayer(LayerDesc desc, int compositionDepth, IntPtr layerID)
	{
		if (!initialized)
		{
			return false;
		}
		if (version >= OVRP_1_28_0.version)
		{
			return OVRP_1_28_0.ovrp_EnqueueSetupLayer2(ref desc, compositionDepth, layerID) == Result.Success;
		}
		if (version >= OVRP_1_15_0.version)
		{
			if (compositionDepth != 0)
			{
				Debug.LogWarning("Use Oculus Plugin 1.28.0 or above to support non-zero compositionDepth");
			}
			return OVRP_1_15_0.ovrp_EnqueueSetupLayer(ref desc, layerID) == Result.Success;
		}
		return false;
	}

	public static bool EnqueueDestroyLayer(IntPtr layerID)
	{
		if (!initialized)
		{
			return false;
		}
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_EnqueueDestroyLayer(layerID) == Result.Success;
		}
		return false;
	}

	public static IntPtr GetLayerTexture(int layerId, int stage, Eye eyeId)
	{
		IntPtr textureHandle = IntPtr.Zero;
		if (!initialized)
		{
			return textureHandle;
		}
		if (version >= OVRP_1_15_0.version)
		{
			OVRP_1_15_0.ovrp_GetLayerTexturePtr(layerId, stage, eyeId, ref textureHandle);
		}
		return textureHandle;
	}

	public static int GetLayerTextureStageCount(int layerId)
	{
		if (!initialized)
		{
			return 1;
		}
		int layerTextureStageCount = 1;
		if (version >= OVRP_1_15_0.version)
		{
			OVRP_1_15_0.ovrp_GetLayerTextureStageCount(layerId, ref layerTextureStageCount);
		}
		return layerTextureStageCount;
	}

	public static IntPtr GetLayerAndroidSurfaceObject(int layerId)
	{
		IntPtr surfaceObject = IntPtr.Zero;
		if (!initialized)
		{
			return surfaceObject;
		}
		if (version >= OVRP_1_29_0.version)
		{
			OVRP_1_29_0.ovrp_GetLayerAndroidSurfaceObject(layerId, ref surfaceObject);
		}
		return surfaceObject;
	}

	public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_Update2(0, frameIndex, predictionSeconds) == Bool.True;
		}
		return false;
	}

	public static Posef GetNodePose(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Pose;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodePose2(0, nodeId);
		}
		return OVRP_0_1_2.ovrp_GetNodePose(nodeId);
	}

	public static Vector3f GetNodeVelocity(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Velocity;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodeVelocity2(0, nodeId).Position;
		}
		return OVRP_0_1_3.ovrp_GetNodeVelocity(nodeId).Position;
	}

	public static Vector3f GetNodeAngularVelocity(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularVelocity;
		}
		return default(Vector3f);
	}

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public static Vector3f GetNodeAcceleration(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).Acceleration;
		}
		if (version >= OVRP_1_8_0.version && stepId == Step.Physics)
		{
			return OVRP_1_8_0.ovrp_GetNodeAcceleration2(0, nodeId).Position;
		}
		return OVRP_0_1_3.ovrp_GetNodeAcceleration(nodeId).Position;
	}

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public static Vector3f GetNodeAngularAcceleration(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId).AngularAcceleration;
		}
		return default(Vector3f);
	}

	public static bool GetNodePresent(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodePresent(nodeId) == Bool.True;
	}

	public static bool GetNodeOrientationTracked(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodeOrientationTracked(nodeId) == Bool.True;
	}

	public static bool GetNodeOrientationValid(Node nodeId)
	{
		if (version >= OVRP_1_38_0.version)
		{
			Bool nodeOrientationValid = Bool.False;
			if (OVRP_1_38_0.ovrp_GetNodeOrientationValid(nodeId, ref nodeOrientationValid) == Result.Success)
			{
				return nodeOrientationValid == Bool.True;
			}
			return false;
		}
		return GetNodeOrientationTracked(nodeId);
	}

	public static bool GetNodePositionTracked(Node nodeId)
	{
		return OVRP_1_1_0.ovrp_GetNodePositionTracked(nodeId) == Bool.True;
	}

	public static bool GetNodePositionValid(Node nodeId)
	{
		if (version >= OVRP_1_38_0.version)
		{
			Bool nodePositionValid = Bool.False;
			if (OVRP_1_38_0.ovrp_GetNodePositionValid(nodeId, ref nodePositionValid) == Result.Success)
			{
				return nodePositionValid == Bool.True;
			}
			return false;
		}
		return GetNodePositionTracked(nodeId);
	}

	public static PoseStatef GetNodePoseStateRaw(Node nodeId, Step stepId)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_29_0.version)
		{
			if (OVRP_1_29_0.ovrp_GetNodePoseStateRaw(stepId, -1, nodeId, out var nodePoseState) == Result.Success)
			{
				return nodePoseState;
			}
			return PoseStatef.identity;
		}
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetNodePoseState(stepId, nodeId);
		}
		return PoseStatef.identity;
	}

	public static PoseStatef GetNodePoseStateAtTime(double time, Node nodeId)
	{
		if (version >= OVRP_1_76_0.version && OVRP_1_76_0.ovrp_GetNodePoseStateAtTime(time, nodeId, out var nodePoseState) == Result.Success)
		{
			return nodePoseState;
		}
		return PoseStatef.identity;
	}

	public static PoseStatef GetNodePoseStateImmediate(Node nodeId)
	{
		if (version >= OVRP_1_69_0.version)
		{
			if (OVRP_1_69_0.ovrp_GetNodePoseStateImmediate(nodeId, out var nodePoseState) == Result.Success)
			{
				return nodePoseState;
			}
			return PoseStatef.identity;
		}
		return PoseStatef.identity;
	}

	public static bool AreHandPosesGeneratedByControllerData(Step stepId, Node nodeId)
	{
		if (version >= OVRP_1_86_0.version)
		{
			Bool isGeneratedByControllerData = Bool.False;
			if (OVRP_1_86_0.ovrp_AreHandPosesGeneratedByControllerData(stepId, nodeId, ref isGeneratedByControllerData) == Result.Success && isGeneratedByControllerData == Bool.True)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool SetSimultaneousHandsAndControllersEnabled(bool enabled)
	{
		if (version >= OVRP_1_88_0.version && OVRP_1_88_0.ovrp_SetSimultaneousHandsAndControllersEnabled(enabled ? Bool.True : Bool.False) == Result.Success)
		{
			return true;
		}
		return false;
	}

	public static bool GetControllerIsInHand(Step stepId, Node nodeId)
	{
		if (version >= OVRP_1_86_0.version)
		{
			Bool isInHand = Bool.True;
			if (OVRP_1_86_0.ovrp_GetControllerIsInHand(stepId, nodeId, ref isInHand) == Result.Success && isInHand == Bool.False)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public static Posef GetCurrentTrackingTransformPose()
	{
		if (version >= OVRP_1_30_0.version)
		{
			if (OVRP_1_30_0.ovrp_GetCurrentTrackingTransformPose(out var trackingTransformPose) == Result.Success)
			{
				return trackingTransformPose;
			}
			return Posef.identity;
		}
		return Posef.identity;
	}

	public static Posef GetTrackingTransformRawPose()
	{
		if (version >= OVRP_1_30_0.version)
		{
			if (OVRP_1_30_0.ovrp_GetTrackingTransformRawPose(out var trackingTransformRawPose) == Result.Success)
			{
				return trackingTransformRawPose;
			}
			return Posef.identity;
		}
		return Posef.identity;
	}

	public static Posef GetTrackingTransformRelativePose(TrackingOrigin trackingOrigin)
	{
		if (version >= OVRP_1_38_0.version)
		{
			Posef trackingTransformRelativePose = Posef.identity;
			if (OVRP_1_38_0.ovrp_GetTrackingTransformRelativePose(ref trackingTransformRelativePose, trackingOrigin) == Result.Success)
			{
				return trackingTransformRelativePose;
			}
			return Posef.identity;
		}
		return Posef.identity;
	}

	public static ControllerState GetControllerState(uint controllerMask)
	{
		return OVRP_1_1_0.ovrp_GetControllerState(controllerMask);
	}

	public static ControllerState2 GetControllerState2(uint controllerMask)
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetControllerState2(controllerMask);
		}
		return new ControllerState2(OVRP_1_1_0.ovrp_GetControllerState(controllerMask));
	}

	public static ControllerState4 GetControllerState4(uint controllerMask)
	{
		if (version >= OVRP_1_16_0.version)
		{
			ControllerState4 controllerState = default(ControllerState4);
			OVRP_1_16_0.ovrp_GetControllerState4(controllerMask, ref controllerState);
			return controllerState;
		}
		return new ControllerState4(GetControllerState2(controllerMask));
	}

	public static ControllerState5 GetControllerState5(uint controllerMask)
	{
		if (version >= OVRP_1_78_0.version)
		{
			ControllerState5 controllerState = default(ControllerState5);
			OVRP_1_78_0.ovrp_GetControllerState5(controllerMask, ref controllerState);
			return controllerState;
		}
		return new ControllerState5(GetControllerState4(controllerMask));
	}

	public static ControllerState6 GetControllerState6(uint controllerMask)
	{
		if (version >= OVRP_1_83_0.version)
		{
			ControllerState6 controllerState = default(ControllerState6);
			OVRP_1_83_0.ovrp_GetControllerState6(controllerMask, ref controllerState);
			return controllerState;
		}
		return new ControllerState6(GetControllerState5(controllerMask));
	}

	public static InteractionProfile GetCurrentInteractionProfile(Hand hand)
	{
		InteractionProfile interactionProfile = InteractionProfile.None;
		if (version >= OVRP_1_78_0.version)
		{
			OVRP_1_78_0.ovrp_GetCurrentInteractionProfile(hand, out interactionProfile);
		}
		return interactionProfile;
	}

	public static InteractionProfile GetCurrentDetachedInteractionProfile(Hand hand)
	{
		InteractionProfile interactionProfile = InteractionProfile.None;
		if (version >= OVRP_1_86_0.version)
		{
			OVRP_1_86_0.ovrp_GetCurrentDetachedInteractionProfile(hand, out interactionProfile);
		}
		return interactionProfile;
	}

	public static string GetCurrentInteractionProfileName(Hand hand)
	{
		string result = string.Empty;
		if (version >= OVRP_1_100_0.version)
		{
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = Marshal.AllocHGlobal(256);
				if (OVRP_1_100_0.ovrp_GetCurrentInteractionProfileName(hand, intPtr) == Result.Success)
				{
					result = Marshal.PtrToStringAnsi(intPtr);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
		return result;
	}

	public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude)
	{
		return OVRP_0_1_2.ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == Bool.True;
	}

	public static bool SetControllerLocalizedVibration(Controller controllerMask, HapticsLocation hapticsLocationMask, float frequency, float amplitude)
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_SetControllerLocalizedVibration(controllerMask, hapticsLocationMask, frequency, amplitude) == Result.Success;
		}
		return false;
	}

	public static bool SetControllerHapticsAmplitudeEnvelope(Controller controllerMask, HapticsAmplitudeEnvelopeVibration hapticsVibration)
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_SetControllerHapticsAmplitudeEnvelope(controllerMask, hapticsVibration) == Result.Success;
		}
		return false;
	}

	public static bool SetControllerHapticsPcm(Controller controllerMask, HapticsPcmVibration hapticsVibration)
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_SetControllerHapticsPcm(controllerMask, hapticsVibration) == Result.Success;
		}
		return false;
	}

	public static bool GetControllerSampleRateHz(Controller controllerMask, out float sampleRateHz)
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_GetControllerSampleRateHz(controllerMask, out sampleRateHz) == Result.Success;
		}
		sampleRateHz = 0f;
		return false;
	}

	public static HapticsDesc GetControllerHapticsDesc(uint controllerMask)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetControllerHapticsDesc(controllerMask);
		}
		return default(HapticsDesc);
	}

	public static HapticsState GetControllerHapticsState(uint controllerMask)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetControllerHapticsState(controllerMask);
		}
		return default(HapticsState);
	}

	public static bool SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer)
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == Bool.True;
		}
		return false;
	}

	public static float GetEyeRecommendedResolutionScale()
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetEyeRecommendedResolutionScale();
		}
		return 1f;
	}

	public static float GetAppCpuStartToGpuEndTime()
	{
		if (version >= OVRP_1_6_0.version)
		{
			return OVRP_1_6_0.ovrp_GetAppCpuStartToGpuEndTime();
		}
		return 0f;
	}

	public static bool GetBoundaryConfigured()
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryConfigured() == Bool.True;
		}
		return false;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static BoundaryTestResult TestBoundaryNode(Node nodeId, BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_TestBoundaryNode(nodeId, boundaryType);
		}
		return default(BoundaryTestResult);
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static BoundaryTestResult TestBoundaryPoint(Vector3f point, BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_TestBoundaryPoint(point, boundaryType);
		}
		return default(BoundaryTestResult);
	}

	public static BoundaryGeometry GetBoundaryGeometry(BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryGeometry(boundaryType);
		}
		return default(BoundaryGeometry);
	}

	public static bool GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount)
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == Bool.True;
		}
		pointsCount = 0;
		return false;
	}

	public static AppPerfStats GetAppPerfStats()
	{
		if (nativeXrApi == XrApi.OpenXR)
		{
			if (!perfStatWarningPrinted)
			{
				Debug.LogWarning("GetAppPerfStats is currently unsupported on OpenXR.");
				perfStatWarningPrinted = true;
			}
			return default(AppPerfStats);
		}
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetAppPerfStats();
		}
		return default(AppPerfStats);
	}

	public static bool ResetAppPerfStats()
	{
		if (nativeXrApi == XrApi.OpenXR)
		{
			if (!resetPerfStatWarningPrinted)
			{
				Debug.LogWarning("ResetAppPerfStats is currently unsupported on OpenXR.");
				resetPerfStatWarningPrinted = true;
			}
			return false;
		}
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_ResetAppPerfStats() == Bool.True;
		}
		return false;
	}

	public static float GetAppFramerate()
	{
		if (version >= OVRP_1_12_0.version)
		{
			return OVRP_1_12_0.ovrp_GetAppFramerate();
		}
		return 0f;
	}

	public static bool SetHandNodePoseStateLatency(double latencyInSeconds)
	{
		if (version >= OVRP_1_18_0.version)
		{
			if (OVRP_1_18_0.ovrp_SetHandNodePoseStateLatency(latencyInSeconds) == Result.Success)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static double GetHandNodePoseStateLatency()
	{
		if (version >= OVRP_1_18_0.version)
		{
			double latencyInSeconds = 0.0;
			if (OVRP_1_18_0.ovrp_GetHandNodePoseStateLatency(out latencyInSeconds) == Result.Success)
			{
				return latencyInSeconds;
			}
			return 0.0;
		}
		return 0.0;
	}

	public static bool SetControllerDrivenHandPoses(bool controllerDrivenHandPoses)
	{
		if (version >= OVRP_1_86_0.version)
		{
			return OVRP_1_86_0.ovrp_SetControllerDrivenHandPoses(controllerDrivenHandPoses ? Bool.True : Bool.False) == Result.Success;
		}
		return false;
	}

	public static bool SetControllerDrivenHandPosesAreNatural(bool controllerDrivenHandPosesAreNatural)
	{
		if (version >= OVRP_1_87_0.version)
		{
			return OVRP_1_87_0.ovrp_SetControllerDrivenHandPosesAreNatural(controllerDrivenHandPosesAreNatural ? Bool.True : Bool.False) == Result.Success;
		}
		return false;
	}

	public static bool IsControllerDrivenHandPosesEnabled()
	{
		if (version >= OVRP_1_86_0.version)
		{
			Bool enabled = Bool.False;
			if (OVRP_1_86_0.ovrp_IsControllerDrivenHandPosesEnabled(ref enabled) == Result.Success)
			{
				return enabled == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool AreControllerDrivenHandPosesNatural()
	{
		if (version >= OVRP_1_87_0.version)
		{
			Bool natural = Bool.False;
			if (OVRP_1_87_0.ovrp_AreControllerDrivenHandPosesNatural(ref natural) == Result.Success)
			{
				return natural == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool SetHandSkeletonVersion(OVRHandSkeletonVersion skeletonVersion)
	{
		if (skeletonVersion == OVRHandSkeletonVersion.Uninitialized)
		{
			HandSkeletonVersion = skeletonVersion;
			return false;
		}
		if (version >= OVRP_1_103_0.version)
		{
			Result num = OVRP_1_103_0.ovrp_SetHandSkeletonVersion(skeletonVersion);
			if (num == Result.Success)
			{
				HandSkeletonVersion = skeletonVersion;
			}
			return num == Result.Success;
		}
		return false;
	}

	public static bool GetActionStateBoolean(string actionName, out bool result)
	{
		Bool value = Bool.False;
		result = false;
		if (version >= OVRP_1_95_0.version)
		{
			Result result2 = OVRP_1_95_0.ovrp_GetActionStateBoolean(actionName, ref value);
			if (result2 == Result.Success)
			{
				result = value == Bool.True;
				return true;
			}
			Debug.LogError($"Error calling GetActionStateBoolean: {result2}");
			return false;
		}
		return false;
	}

	public static bool GetActionStateFloat(string actionName, out float result)
	{
		result = 0f;
		if (version >= OVRP_1_95_0.version)
		{
			Result result2 = OVRP_1_95_0.ovrp_GetActionStateFloat(actionName, ref result);
			if (result2 == Result.Success)
			{
				return true;
			}
			Debug.LogError($"Error calling GetActionStateFloat: {result2}");
			return false;
		}
		return false;
	}

	public static bool GetActionStatePose(string actionName, out Posef result)
	{
		result = default(Posef);
		if (version >= OVRP_1_95_0.version)
		{
			Result result2 = OVRP_1_95_0.ovrp_GetActionStatePose(actionName, ref result);
			if (result2 == Result.Success)
			{
				return true;
			}
			Debug.LogError($"Error calling GetActionStatePose: {result2}");
			return false;
		}
		return false;
	}

	public static bool GetActionStatePose(string actionName, Hand hand, out Posef result)
	{
		result = default(Posef);
		if (version >= OVRP_1_100_0.version)
		{
			Result result2 = OVRP_1_100_0.ovrp_GetActionStatePose2(actionName, hand, ref result);
			if (result2 == Result.Success)
			{
				return true;
			}
			Debug.LogError($"Error calling GetActionStatePose2: {result2}");
			return false;
		}
		return false;
	}

	public static bool TriggerVibrationAction(string actionName, Hand hand, float duration, float amplitude)
	{
		if (version >= OVRP_1_100_0.version)
		{
			Result result = OVRP_1_100_0.ovrp_TriggerVibrationAction(actionName, hand, duration, amplitude);
			if (result == Result.Success)
			{
				return true;
			}
			Debug.LogError($"Error calling TriggerVibrationAction: {result}");
			return false;
		}
		return false;
	}

	public static bool SetWideMotionModeHandPoses(bool wideMotionModeFusionHandPoses)
	{
		if (version >= OVRP_1_93_0.version)
		{
			return OVRP_1_93_0.ovrp_SetWideMotionModeHandPoses(wideMotionModeFusionHandPoses ? Bool.True : Bool.False) == Result.Success;
		}
		return false;
	}

	public static bool IsWideMotionModeHandPosesEnabled()
	{
		if (version >= OVRP_1_93_0.version)
		{
			Bool enabled = Bool.False;
			if (OVRP_1_93_0.ovrp_IsWideMotionModeHandPosesEnabled(ref enabled) == Result.Success)
			{
				return enabled == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static EyeTextureFormat GetDesiredEyeTextureFormat()
	{
		if (version >= OVRP_1_11_0.version)
		{
			uint num = (uint)OVRP_1_11_0.ovrp_GetDesiredEyeTextureFormat();
			if (num == 1)
			{
				num = 0u;
			}
			return (EyeTextureFormat)num;
		}
		return EyeTextureFormat.Default;
	}

	public static bool SetDesiredEyeTextureFormat(EyeTextureFormat value)
	{
		if (version >= OVRP_1_11_0.version)
		{
			return OVRP_1_11_0.ovrp_SetDesiredEyeTextureFormat(value) == Bool.True;
		}
		return false;
	}

	public static bool InitializeMixedReality()
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_InitializeMixedReality() == Result.Success;
		}
		return false;
	}

	public static bool ShutdownMixedReality()
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_ShutdownMixedReality() == Result.Success;
		}
		return false;
	}

	public static bool IsMixedRealityInitialized()
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_GetMixedRealityInitialized() == Bool.True;
		}
		return false;
	}

	public static int GetExternalCameraCount()
	{
		if (version >= OVRP_1_15_0.version)
		{
			int cameraCount = 0;
			if (OVRP_1_15_0.ovrp_GetExternalCameraCount(out cameraCount) != 0)
			{
				return 0;
			}
			return cameraCount;
		}
		return 0;
	}

	public static bool UpdateExternalCamera()
	{
		if (version >= OVRP_1_15_0.version)
		{
			return OVRP_1_15_0.ovrp_UpdateExternalCamera() == Result.Success;
		}
		return false;
	}

	public static bool GetMixedRealityCameraInfo(int cameraId, out CameraExtrinsics cameraExtrinsics, out CameraIntrinsics cameraIntrinsics)
	{
		cameraExtrinsics = default(CameraExtrinsics);
		cameraIntrinsics = default(CameraIntrinsics);
		if (version >= OVRP_1_15_0.version)
		{
			bool result = true;
			if (OVRP_1_15_0.ovrp_GetExternalCameraExtrinsics(cameraId, out cameraExtrinsics) != 0)
			{
				result = false;
			}
			if (OVRP_1_15_0.ovrp_GetExternalCameraIntrinsics(cameraId, out cameraIntrinsics) != 0)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static bool OverrideExternalCameraFov(int cameraId, bool useOverriddenFov, Fovf fov)
	{
		if (version >= OVRP_1_44_0.version)
		{
			bool result = true;
			if (OVRP_1_44_0.ovrp_OverrideExternalCameraFov(cameraId, useOverriddenFov ? Bool.True : Bool.False, ref fov) != 0)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static bool GetUseOverriddenExternalCameraFov(int cameraId)
	{
		if (version >= OVRP_1_44_0.version)
		{
			bool result = true;
			Bool useOverriddenFov = Bool.False;
			if (OVRP_1_44_0.ovrp_GetUseOverriddenExternalCameraFov(cameraId, out useOverriddenFov) != 0)
			{
				result = false;
			}
			if (useOverriddenFov == Bool.False)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static bool OverrideExternalCameraStaticPose(int cameraId, bool useOverriddenPose, Posef poseInStageOrigin)
	{
		if (version >= OVRP_1_44_0.version)
		{
			bool result = true;
			if (OVRP_1_44_0.ovrp_OverrideExternalCameraStaticPose(cameraId, useOverriddenPose ? Bool.True : Bool.False, ref poseInStageOrigin) != 0)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static bool GetUseOverriddenExternalCameraStaticPose(int cameraId)
	{
		if (version >= OVRP_1_44_0.version)
		{
			bool result = true;
			Bool useOverriddenStaticPose = Bool.False;
			if (OVRP_1_44_0.ovrp_GetUseOverriddenExternalCameraStaticPose(cameraId, out useOverriddenStaticPose) != 0)
			{
				result = false;
			}
			if (useOverriddenStaticPose == Bool.False)
			{
				result = false;
			}
			return result;
		}
		return false;
	}

	public static bool ResetDefaultExternalCamera()
	{
		if (version >= OVRP_1_44_0.version)
		{
			if (OVRP_1_44_0.ovrp_ResetDefaultExternalCamera() != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SetDefaultExternalCamera(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics)
	{
		if (version >= OVRP_1_44_0.version)
		{
			if (OVRP_1_44_0.ovrp_SetDefaultExternalCamera(cameraName, ref cameraIntrinsics, ref cameraExtrinsics) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SetExternalCameraProperties(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics)
	{
		if (version >= OVRP_1_48_0.version)
		{
			if (OVRP_1_48_0.ovrp_SetExternalCameraProperties(cameraName, ref cameraIntrinsics, ref cameraExtrinsics) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SetMultimodalHandsControllersSupported(bool value)
	{
		if (version >= OVRP_1_86_0.version)
		{
			if (OVRP_1_86_0.ovrp_SetMultimodalHandsControllersSupported(ToBool(value)) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsMultimodalHandsControllersSupported()
	{
		if (version >= OVRP_1_86_0.version)
		{
			Bool supported = Bool.False;
			if (OVRP_1_86_0.ovrp_IsMultimodalHandsControllersSupported(ref supported) == Result.Success)
			{
				return supported == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool IsInsightPassthroughSupported()
	{
		if (version >= OVRP_1_71_0.version)
		{
			Bool supported = Bool.False;
			Result result = OVRP_1_71_0.ovrp_IsInsightPassthroughSupported(ref supported);
			if (result == Result.Success)
			{
				return supported == Bool.True;
			}
			Debug.LogError("Unable to determine whether passthrough is supported. Try calling IsInsightPassthroughSupported() while the XR plug-in is initialized. Failed with reason: " + result);
			return false;
		}
		return false;
	}

	public static bool InitializeInsightPassthrough()
	{
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_InitializeInsightPassthrough() != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool ShutdownInsightPassthrough()
	{
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_ShutdownInsightPassthrough() != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsInsightPassthroughInitialized()
	{
		if (version >= OVRP_1_63_0.version)
		{
			return OVRP_1_63_0.ovrp_GetInsightPassthroughInitialized() == Bool.True;
		}
		return false;
	}

	public static Result GetInsightPassthroughInitializationState()
	{
		if (version >= OVRP_1_66_0.version)
		{
			return OVRP_1_66_0.ovrp_GetInsightPassthroughInitializationState();
		}
		return Result.Failure_Unsupported;
	}

	public static bool CreateInsightTriangleMesh(int layerId, Vector3[] vertices, int[] triangles, out ulong meshHandle)
	{
		meshHandle = 0uL;
		if (version >= OVRP_1_63_0.version)
		{
			if (vertices == null || triangles == null || vertices.Length == 0 || triangles.Length == 0)
			{
				return false;
			}
			int vertexCount = vertices.Length;
			int triangleCount = triangles.Length / 3;
			GCHandle gCHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
			IntPtr vertices2 = gCHandle.AddrOfPinnedObject();
			GCHandle gCHandle2 = GCHandle.Alloc(triangles, GCHandleType.Pinned);
			IntPtr triangles2 = gCHandle2.AddrOfPinnedObject();
			Result num = OVRP_1_63_0.ovrp_CreateInsightTriangleMesh(layerId, vertices2, vertexCount, triangles2, triangleCount, out meshHandle);
			gCHandle2.Free();
			gCHandle.Free();
			if (num != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool DestroyInsightTriangleMesh(ulong meshHandle)
	{
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_DestroyInsightTriangleMesh(meshHandle) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool AddInsightPassthroughSurfaceGeometry(int layerId, ulong meshHandle, Matrix4x4 T_world_model, out ulong geometryInstanceHandle)
	{
		geometryInstanceHandle = 0uL;
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_AddInsightPassthroughSurfaceGeometry(layerId, meshHandle, T_world_model, out geometryInstanceHandle) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool DestroyInsightPassthroughGeometryInstance(ulong geometryInstanceHandle)
	{
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_DestroyInsightPassthroughGeometryInstance(geometryInstanceHandle) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool UpdateInsightPassthroughGeometryTransform(ulong geometryInstanceHandle, Matrix4x4 transform)
	{
		if (version >= OVRP_1_63_0.version)
		{
			if (OVRP_1_63_0.ovrp_UpdateInsightPassthroughGeometryTransform(geometryInstanceHandle, transform) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SetInsightPassthroughStyle(int layerId, InsightPassthroughStyle2 style)
	{
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_SetInsightPassthroughStyle2(layerId, in style).IsSuccess();
		}
		if (version >= OVRP_1_63_0.version)
		{
			if (style.TextureColorMapType == InsightPassthroughColorMapType.ColorLut || style.TextureColorMapType == InsightPassthroughColorMapType.InterpolatedColorLut)
			{
				Debug.LogError("Only OVRPlugn version 1.84.0 or higher supports Color LUTs");
				return false;
			}
			InsightPassthroughStyle target = default(InsightPassthroughStyle);
			style.CopyTo(ref target);
			return OVRP_1_63_0.ovrp_SetInsightPassthroughStyle(layerId, target).IsSuccess();
		}
		return false;
	}

	public static bool SetInsightPassthroughStyle(int layerId, InsightPassthroughStyle style)
	{
		if (version >= OVRP_1_63_0.version)
		{
			return OVRP_1_63_0.ovrp_SetInsightPassthroughStyle(layerId, style).IsSuccess();
		}
		return false;
	}

	public static bool CreatePassthroughColorLut(PassthroughColorLutChannels channels, uint resolution, PassthroughColorLutData data, out ulong colorLut)
	{
		colorLut = 0uL;
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_CreatePassthroughColorLut(channels, resolution, data, out colorLut).IsSuccess();
		}
		colorLut = 0uL;
		return false;
	}

	public static bool DestroyPassthroughColorLut(ulong colorLut)
	{
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_DestroyPassthroughColorLut(colorLut).IsSuccess();
		}
		return false;
	}

	public static bool UpdatePassthroughColorLut(ulong colorLut, PassthroughColorLutData data)
	{
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_UpdatePassthroughColorLut(colorLut, data).IsSuccess();
		}
		return false;
	}

	public static bool SetInsightPassthroughKeyboardHandsIntensity(int layerId, InsightPassthroughKeyboardHandsIntensity intensity)
	{
		if (version >= OVRP_1_68_0.version)
		{
			if (OVRP_1_68_0.ovrp_SetInsightPassthroughKeyboardHandsIntensity(layerId, intensity) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static PassthroughCapabilityFlags GetPassthroughCapabilityFlags()
	{
		if (version >= OVRP_1_78_0.version)
		{
			PassthroughCapabilityFlags capabilityFlags = (PassthroughCapabilityFlags)0;
			Result result = OVRP_1_78_0.ovrp_GetPassthroughCapabilityFlags(ref capabilityFlags);
			if (result == Result.Success)
			{
				return capabilityFlags;
			}
			Debug.LogError("Unable to retrieve passthrough capability flags. Try calling GetInsightPassthroughCapabilityFlags() while the XR plug-in is initialized. Failed with reason: " + result);
		}
		else
		{
			Debug.LogWarning("ovrp_GetPassthroughCapabilityFlags() not yet supported by OVRPlugin. Result of GetInsightPassthroughCapabilityFlags() is not accurate.");
		}
		if (!IsInsightPassthroughSupported())
		{
			return (PassthroughCapabilityFlags)0;
		}
		return PassthroughCapabilityFlags.Passthrough;
	}

	public static Result GetPassthroughCapabilities(ref PassthroughCapabilities outCapabilities)
	{
		if (version >= OVRP_1_85_0.version)
		{
			outCapabilities.Fields = (PassthroughCapabilityFields)3;
			return OVRP_1_85_0.ovrp_GetPassthroughCapabilities(ref outCapabilities);
		}
		return Result.Failure_Unsupported;
	}

	public static Vector3f GetBoundaryDimensions(BoundaryType boundaryType)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryDimensions(boundaryType);
		}
		return default(Vector3f);
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static bool GetBoundaryVisible()
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_GetBoundaryVisible() == Bool.True;
		}
		return false;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static bool SetBoundaryVisible(bool value)
	{
		if (version >= OVRP_1_8_0.version)
		{
			return OVRP_1_8_0.ovrp_SetBoundaryVisible(ToBool(value)) == Bool.True;
		}
		return false;
	}

	public static SystemHeadset GetSystemHeadsetType()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetSystemHeadsetType();
		}
		return SystemHeadset.None;
	}

	public static Controller GetActiveController()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetActiveController();
		}
		return Controller.None;
	}

	public static Controller GetConnectedControllers()
	{
		if (version >= OVRP_1_9_0.version)
		{
			return OVRP_1_9_0.ovrp_GetConnectedControllers();
		}
		return Controller.None;
	}

	private static Bool ToBool(bool b)
	{
		if (!b)
		{
			return Bool.False;
		}
		return Bool.True;
	}

	public static TrackingOrigin GetTrackingOriginType()
	{
		return OVRP_1_0_0.ovrp_GetTrackingOriginType();
	}

	public static bool SetTrackingOriginType(TrackingOrigin originType)
	{
		return OVRP_1_0_0.ovrp_SetTrackingOriginType(originType) == Bool.True;
	}

	public static Posef GetTrackingCalibratedOrigin()
	{
		return OVRP_1_0_0.ovrp_GetTrackingCalibratedOrigin();
	}

	public static bool SetTrackingCalibratedOrigin()
	{
		return OVRP_1_2_0.ovrpi_SetTrackingCalibratedOrigin() == Bool.True;
	}

	public static bool RecenterTrackingOrigin(RecenterFlags flags)
	{
		return OVRP_1_0_0.ovrp_RecenterTrackingOrigin((uint)flags) == Bool.True;
	}

	public static bool UpdateCameraDevices()
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_UpdateCameraDevices() == Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceAvailable(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_IsCameraDeviceAvailable(cameraDevice) == Bool.True;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredColorFrameSize(CameraDevice cameraDevice, int width, int height)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Sizei preferredColorFrameSize = default(Sizei);
			preferredColorFrameSize.w = width;
			preferredColorFrameSize.h = height;
			return OVRP_1_16_0.ovrp_SetCameraDevicePreferredColorFrameSize(cameraDevice, preferredColorFrameSize) == Result.Success;
		}
		return false;
	}

	public static bool OpenCameraDevice(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_OpenCameraDevice(cameraDevice) == Result.Success;
		}
		return false;
	}

	public static bool CloseCameraDevice(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_CloseCameraDevice(cameraDevice) == Result.Success;
		}
		return false;
	}

	public static bool HasCameraDeviceOpened(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_HasCameraDeviceOpened(cameraDevice) == Bool.True;
		}
		return false;
	}

	public static bool IsCameraDeviceColorFrameAvailable(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			return OVRP_1_16_0.ovrp_IsCameraDeviceColorFrameAvailable(cameraDevice) == Bool.True;
		}
		return false;
	}

	public static Texture2D GetCameraDeviceColorFrameTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_16_0.version)
		{
			Sizei colorFrameSize = default(Sizei);
			if (OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameSize(cameraDevice, out colorFrameSize) != 0)
			{
				return null;
			}
			if (OVRP_1_16_0.ovrp_GetCameraDeviceColorFrameBgraPixels(cameraDevice, out var colorFrameBgraPixels, out var colorFrameRowPitch) != 0)
			{
				return null;
			}
			if (colorFrameRowPitch != colorFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraFrameTexture || cachedCameraFrameTexture.width != colorFrameSize.w || cachedCameraFrameTexture.height != colorFrameSize.h)
			{
				cachedCameraFrameTexture = new Texture2D(colorFrameSize.w, colorFrameSize.h, TextureFormat.BGRA32, mipChain: false);
			}
			cachedCameraFrameTexture.LoadRawTextureData(colorFrameBgraPixels, colorFrameRowPitch * colorFrameSize.h);
			cachedCameraFrameTexture.Apply();
			return cachedCameraFrameTexture;
		}
		return null;
	}

	public static bool DoesCameraDeviceSupportDepth(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			if (OVRP_1_17_0.ovrp_DoesCameraDeviceSupportDepth(cameraDevice, out var supportDepth) == Result.Success)
			{
				return supportDepth == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool SetCameraDeviceDepthSensingMode(CameraDevice camera, CameraDeviceDepthSensingMode depthSensoringMode)
	{
		if (version >= OVRP_1_17_0.version)
		{
			return OVRP_1_17_0.ovrp_SetCameraDeviceDepthSensingMode(camera, depthSensoringMode) == Result.Success;
		}
		return false;
	}

	public static bool SetCameraDevicePreferredDepthQuality(CameraDevice camera, CameraDeviceDepthQuality depthQuality)
	{
		if (version >= OVRP_1_17_0.version)
		{
			return OVRP_1_17_0.ovrp_SetCameraDevicePreferredDepthQuality(camera, depthQuality) == Result.Success;
		}
		return false;
	}

	public static bool IsCameraDeviceDepthFrameAvailable(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			if (OVRP_1_17_0.ovrp_IsCameraDeviceDepthFrameAvailable(cameraDevice, out var available) == Result.Success)
			{
				return available == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static Texture2D GetCameraDeviceDepthFrameTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Sizei depthFrameSize = default(Sizei);
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out depthFrameSize) != 0)
			{
				return null;
			}
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFramePixels(cameraDevice, out var depthFramePixels, out var depthFrameRowPitch) != 0)
			{
				return null;
			}
			if (depthFrameRowPitch != depthFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraDepthTexture || cachedCameraDepthTexture.width != depthFrameSize.w || cachedCameraDepthTexture.height != depthFrameSize.h)
			{
				cachedCameraDepthTexture = new Texture2D(depthFrameSize.w, depthFrameSize.h, TextureFormat.RFloat, mipChain: false);
				cachedCameraDepthTexture.filterMode = FilterMode.Point;
			}
			cachedCameraDepthTexture.LoadRawTextureData(depthFramePixels, depthFrameRowPitch * depthFrameSize.h);
			cachedCameraDepthTexture.Apply();
			return cachedCameraDepthTexture;
		}
		return null;
	}

	public static Texture2D GetCameraDeviceDepthConfidenceTexture(CameraDevice cameraDevice)
	{
		if (version >= OVRP_1_17_0.version)
		{
			Sizei depthFrameSize = default(Sizei);
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthFrameSize(cameraDevice, out depthFrameSize) != 0)
			{
				return null;
			}
			if (OVRP_1_17_0.ovrp_GetCameraDeviceDepthConfidencePixels(cameraDevice, out var depthConfidencePixels, out var depthConfidenceRowPitch) != 0)
			{
				return null;
			}
			if (depthConfidenceRowPitch != depthFrameSize.w * 4)
			{
				return null;
			}
			if (!cachedCameraDepthConfidenceTexture || cachedCameraDepthConfidenceTexture.width != depthFrameSize.w || cachedCameraDepthConfidenceTexture.height != depthFrameSize.h)
			{
				cachedCameraDepthConfidenceTexture = new Texture2D(depthFrameSize.w, depthFrameSize.h, TextureFormat.RFloat, mipChain: false);
			}
			cachedCameraDepthConfidenceTexture.LoadRawTextureData(depthConfidencePixels, depthConfidenceRowPitch * depthFrameSize.h);
			cachedCameraDepthConfidenceTexture.Apply();
			return cachedCameraDepthConfidenceTexture;
		}
		return null;
	}

	public static bool GetNodeFrustum2(Node nodeId, out Frustumf2 frustum)
	{
		frustum = default(Frustumf2);
		if (version >= OVRP_1_15_0.version)
		{
			if (OVRP_1_15_0.ovrp_GetNodeFrustum2(nodeId, out frustum) != 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static Handedness GetDominantHand()
	{
		if (version >= OVRP_1_28_0.version && OVRP_1_28_0.ovrp_GetDominantHand(out var dominantHand) == Result.Success)
		{
			return dominantHand;
		}
		return Handedness.Unsupported;
	}

	public static bool SendEvent(string name, string param = "", string source = "")
	{
		if (version >= OVRP_1_30_0.version)
		{
			return OVRP_1_30_0.ovrp_SendEvent2(name, param, (source.Length == 0) ? "integration" : source) == Result.Success;
		}
		if (version >= OVRP_1_28_0.version)
		{
			return OVRP_1_28_0.ovrp_SendEvent(name, param) == Result.Success;
		}
		return false;
	}

	public static Result SendUnifiedEvent(Bool isEssential, string productType, string eventName, string event_metadata_json, string project_name = "", string event_entrypoint = "", string project_guid = "", string event_type = "", string event_target = "", string error_msg = "", string is_internal_build = "", string batch_mode = "")
	{
		if (version >= OVRP_1_110_0.version)
		{
			return OVRP_1_110_0.ovrp_SendUnifiedEventV2(isEssential, productType, eventName, event_metadata_json, project_name, event_entrypoint, project_guid, event_type, event_target, error_msg, is_internal_build, batch_mode);
		}
		if (version == OVRP_1_109_0.version)
		{
			return OVRP_1_109_0.ovrp_SendUnifiedEvent(isEssential, productType, eventName, event_metadata_json, project_name, event_entrypoint, project_guid, event_type, event_target, error_msg, is_internal_build);
		}
		return Result.Failure_Unsupported;
	}

	public static bool SetHeadPoseModifier(ref Quatf relativeRotation, ref Vector3f relativeTranslation)
	{
		if (version >= OVRP_1_29_0.version)
		{
			return OVRP_1_29_0.ovrp_SetHeadPoseModifier(ref relativeRotation, ref relativeTranslation) == Result.Success;
		}
		return false;
	}

	public static bool GetHeadPoseModifier(out Quatf relativeRotation, out Vector3f relativeTranslation)
	{
		if (version >= OVRP_1_29_0.version)
		{
			return OVRP_1_29_0.ovrp_GetHeadPoseModifier(out relativeRotation, out relativeTranslation) == Result.Success;
		}
		relativeRotation = Quatf.identity;
		relativeTranslation = Vector3f.zero;
		return false;
	}

	public static bool IsPerfMetricsSupported(PerfMetrics perfMetrics)
	{
		if (version >= OVRP_1_30_0.version)
		{
			if (OVRP_1_30_0.ovrp_IsPerfMetricsSupported(perfMetrics, out var isSupported) == Result.Success)
			{
				return isSupported == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static float? GetPerfMetricsFloat(PerfMetrics perfMetrics)
	{
		if (version >= OVRP_1_30_0.version)
		{
			if (OVRP_1_30_0.ovrp_GetPerfMetricsFloat(perfMetrics, out var value) == Result.Success)
			{
				return value;
			}
			return null;
		}
		return null;
	}

	public static int? GetPerfMetricsInt(PerfMetrics perfMetrics)
	{
		if (version >= OVRP_1_30_0.version)
		{
			if (OVRP_1_30_0.ovrp_GetPerfMetricsInt(perfMetrics, out var value) == Result.Success)
			{
				return value;
			}
			return null;
		}
		return null;
	}

	public static double GetTimeInSeconds()
	{
		if (version >= OVRP_1_31_0.version)
		{
			if (OVRP_1_31_0.ovrp_GetTimeInSeconds(out var value) == Result.Success)
			{
				return value;
			}
			return 0.0;
		}
		return 0.0;
	}

	public static bool SetColorScaleAndOffset(Vector4 colorScale, Vector4 colorOffset, bool applyToAllLayers)
	{
		if (version >= OVRP_1_31_0.version)
		{
			Bool applyToAllLayers2 = (applyToAllLayers ? Bool.True : Bool.False);
			return OVRP_1_31_0.ovrp_SetColorScaleAndOffset(colorScale, colorOffset, applyToAllLayers2) == Result.Success;
		}
		return false;
	}

	public static bool AddCustomMetadata(string name, string param = "")
	{
		if (version >= OVRP_1_32_0.version)
		{
			return OVRP_1_32_0.ovrp_AddCustomMetadata(name, param) == Result.Success;
		}
		return false;
	}

	public static bool SetDeveloperMode(Bool active)
	{
		if (version >= OVRP_1_38_0.version)
		{
			return OVRP_1_38_0.ovrp_SetDeveloperMode(active) == Result.Success;
		}
		return false;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public static float GetAdaptiveGPUPerformanceScale()
	{
		if (version >= OVRP_1_42_0.version)
		{
			float adaptiveGpuPerformanceScale = 1f;
			if (OVRP_1_42_0.ovrp_GetAdaptiveGpuPerformanceScale2(ref adaptiveGpuPerformanceScale) == Result.Success)
			{
				return adaptiveGpuPerformanceScale;
			}
			return 1f;
		}
		return 1f;
	}

	public static bool GetHandTrackingEnabled()
	{
		if (version >= OVRP_1_44_0.version)
		{
			Bool handTrackingEnabled = Bool.False;
			if (OVRP_1_44_0.ovrp_GetHandTrackingEnabled(ref handTrackingEnabled) == Result.Success)
			{
				return handTrackingEnabled == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool GetHandState(Step stepId, Hand hand, ref HandState handState)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_103_0.version && HandSkeletonVersion == OVRHandSkeletonVersion.OpenXR)
		{
			if (OVRP_1_103_0.ovrp_GetHandState3(stepId, -1, hand, out cachedHandState3) == Result.Success)
			{
				if (handState.BoneRotations == null || handState.BoneRotations.Length != 26)
				{
					handState.BoneRotations = new Quatf[26];
				}
				if (handState.BonePositions == null || handState.BonePositions.Length != 26)
				{
					handState.BonePositions = new Vector3f[26];
				}
				if (handState.PinchStrength == null || handState.PinchStrength.Length != 5)
				{
					handState.PinchStrength = new float[5];
				}
				if (handState.FingerConfidences == null || handState.FingerConfidences.Length != 5)
				{
					handState.FingerConfidences = new TrackingConfidence[5];
				}
				handState.Status = cachedHandState3.Status;
				handState.RootPose = cachedHandState3.RootPose;
				handState.BoneRotations[0] = cachedHandState3.BonePoses_0.Orientation;
				handState.BoneRotations[1] = cachedHandState3.BonePoses_1.Orientation;
				handState.BoneRotations[2] = cachedHandState3.BonePoses_2.Orientation;
				handState.BoneRotations[3] = cachedHandState3.BonePoses_3.Orientation;
				handState.BoneRotations[4] = cachedHandState3.BonePoses_4.Orientation;
				handState.BoneRotations[5] = cachedHandState3.BonePoses_5.Orientation;
				handState.BoneRotations[6] = cachedHandState3.BonePoses_6.Orientation;
				handState.BoneRotations[7] = cachedHandState3.BonePoses_7.Orientation;
				handState.BoneRotations[8] = cachedHandState3.BonePoses_8.Orientation;
				handState.BoneRotations[9] = cachedHandState3.BonePoses_9.Orientation;
				handState.BoneRotations[10] = cachedHandState3.BonePoses_10.Orientation;
				handState.BoneRotations[11] = cachedHandState3.BonePoses_11.Orientation;
				handState.BoneRotations[12] = cachedHandState3.BonePoses_12.Orientation;
				handState.BoneRotations[13] = cachedHandState3.BonePoses_13.Orientation;
				handState.BoneRotations[14] = cachedHandState3.BonePoses_14.Orientation;
				handState.BoneRotations[15] = cachedHandState3.BonePoses_15.Orientation;
				handState.BoneRotations[16] = cachedHandState3.BonePoses_16.Orientation;
				handState.BoneRotations[17] = cachedHandState3.BonePoses_17.Orientation;
				handState.BoneRotations[18] = cachedHandState3.BonePoses_18.Orientation;
				handState.BoneRotations[19] = cachedHandState3.BonePoses_19.Orientation;
				handState.BoneRotations[20] = cachedHandState3.BonePoses_20.Orientation;
				handState.BoneRotations[21] = cachedHandState3.BonePoses_21.Orientation;
				handState.BoneRotations[22] = cachedHandState3.BonePoses_22.Orientation;
				handState.BoneRotations[23] = cachedHandState3.BonePoses_23.Orientation;
				handState.BoneRotations[24] = cachedHandState3.BonePoses_24.Orientation;
				handState.BoneRotations[25] = cachedHandState3.BonePoses_25.Orientation;
				handState.BonePositions[0] = cachedHandState3.BonePoses_0.Position;
				handState.BonePositions[1] = cachedHandState3.BonePoses_1.Position;
				handState.BonePositions[2] = cachedHandState3.BonePoses_2.Position;
				handState.BonePositions[3] = cachedHandState3.BonePoses_3.Position;
				handState.BonePositions[4] = cachedHandState3.BonePoses_4.Position;
				handState.BonePositions[5] = cachedHandState3.BonePoses_5.Position;
				handState.BonePositions[6] = cachedHandState3.BonePoses_6.Position;
				handState.BonePositions[7] = cachedHandState3.BonePoses_7.Position;
				handState.BonePositions[8] = cachedHandState3.BonePoses_8.Position;
				handState.BonePositions[9] = cachedHandState3.BonePoses_9.Position;
				handState.BonePositions[10] = cachedHandState3.BonePoses_10.Position;
				handState.BonePositions[11] = cachedHandState3.BonePoses_11.Position;
				handState.BonePositions[12] = cachedHandState3.BonePoses_12.Position;
				handState.BonePositions[13] = cachedHandState3.BonePoses_13.Position;
				handState.BonePositions[14] = cachedHandState3.BonePoses_14.Position;
				handState.BonePositions[15] = cachedHandState3.BonePoses_15.Position;
				handState.BonePositions[16] = cachedHandState3.BonePoses_16.Position;
				handState.BonePositions[17] = cachedHandState3.BonePoses_17.Position;
				handState.BonePositions[18] = cachedHandState3.BonePoses_18.Position;
				handState.BonePositions[19] = cachedHandState3.BonePoses_19.Position;
				handState.BonePositions[20] = cachedHandState3.BonePoses_20.Position;
				handState.BonePositions[21] = cachedHandState3.BonePoses_21.Position;
				handState.BonePositions[22] = cachedHandState3.BonePoses_22.Position;
				handState.BonePositions[23] = cachedHandState3.BonePoses_23.Position;
				handState.BonePositions[24] = cachedHandState3.BonePoses_24.Position;
				handState.BonePositions[25] = cachedHandState3.BonePoses_25.Position;
				handState.Pinches = cachedHandState3.Pinches;
				handState.PinchStrength[0] = cachedHandState3.PinchStrength_0;
				handState.PinchStrength[1] = cachedHandState3.PinchStrength_1;
				handState.PinchStrength[2] = cachedHandState3.PinchStrength_2;
				handState.PinchStrength[3] = cachedHandState3.PinchStrength_3;
				handState.PinchStrength[4] = cachedHandState3.PinchStrength_4;
				handState.PointerPose = cachedHandState3.PointerPose;
				handState.HandScale = cachedHandState3.HandScale;
				handState.HandConfidence = cachedHandState3.HandConfidence;
				handState.FingerConfidences[0] = cachedHandState3.FingerConfidences_0;
				handState.FingerConfidences[1] = cachedHandState3.FingerConfidences_1;
				handState.FingerConfidences[2] = cachedHandState3.FingerConfidences_2;
				handState.FingerConfidences[3] = cachedHandState3.FingerConfidences_3;
				handState.FingerConfidences[4] = cachedHandState3.FingerConfidences_4;
				handState.RequestedTimeStamp = cachedHandState3.RequestedTimeStamp;
				handState.SampleTimeStamp = cachedHandState3.SampleTimeStamp;
				return true;
			}
			return false;
		}
		if (version >= OVRP_1_44_0.version)
		{
			if (OVRP_1_44_0.ovrp_GetHandState(stepId, hand, out cachedHandState) == Result.Success)
			{
				if (handState.BoneRotations == null || handState.BoneRotations.Length != 24)
				{
					handState.BoneRotations = new Quatf[24];
				}
				if (handState.PinchStrength == null || handState.PinchStrength.Length != 5)
				{
					handState.PinchStrength = new float[5];
				}
				if (handState.FingerConfidences == null || handState.FingerConfidences.Length != 5)
				{
					handState.FingerConfidences = new TrackingConfidence[5];
				}
				handState.Status = cachedHandState.Status;
				handState.RootPose = cachedHandState.RootPose;
				handState.BoneRotations[0] = cachedHandState.BoneRotations_0;
				handState.BoneRotations[1] = cachedHandState.BoneRotations_1;
				handState.BoneRotations[2] = cachedHandState.BoneRotations_2;
				handState.BoneRotations[3] = cachedHandState.BoneRotations_3;
				handState.BoneRotations[4] = cachedHandState.BoneRotations_4;
				handState.BoneRotations[5] = cachedHandState.BoneRotations_5;
				handState.BoneRotations[6] = cachedHandState.BoneRotations_6;
				handState.BoneRotations[7] = cachedHandState.BoneRotations_7;
				handState.BoneRotations[8] = cachedHandState.BoneRotations_8;
				handState.BoneRotations[9] = cachedHandState.BoneRotations_9;
				handState.BoneRotations[10] = cachedHandState.BoneRotations_10;
				handState.BoneRotations[11] = cachedHandState.BoneRotations_11;
				handState.BoneRotations[12] = cachedHandState.BoneRotations_12;
				handState.BoneRotations[13] = cachedHandState.BoneRotations_13;
				handState.BoneRotations[14] = cachedHandState.BoneRotations_14;
				handState.BoneRotations[15] = cachedHandState.BoneRotations_15;
				handState.BoneRotations[16] = cachedHandState.BoneRotations_16;
				handState.BoneRotations[17] = cachedHandState.BoneRotations_17;
				handState.BoneRotations[18] = cachedHandState.BoneRotations_18;
				handState.BoneRotations[19] = cachedHandState.BoneRotations_19;
				handState.BoneRotations[20] = cachedHandState.BoneRotations_20;
				handState.BoneRotations[21] = cachedHandState.BoneRotations_21;
				handState.BoneRotations[22] = cachedHandState.BoneRotations_22;
				handState.BoneRotations[23] = cachedHandState.BoneRotations_23;
				handState.Pinches = cachedHandState.Pinches;
				handState.PinchStrength[0] = cachedHandState.PinchStrength_0;
				handState.PinchStrength[1] = cachedHandState.PinchStrength_1;
				handState.PinchStrength[2] = cachedHandState.PinchStrength_2;
				handState.PinchStrength[3] = cachedHandState.PinchStrength_3;
				handState.PinchStrength[4] = cachedHandState.PinchStrength_4;
				handState.PointerPose = cachedHandState.PointerPose;
				handState.HandScale = cachedHandState.HandScale;
				handState.HandConfidence = cachedHandState.HandConfidence;
				handState.FingerConfidences[0] = cachedHandState.FingerConfidences_0;
				handState.FingerConfidences[1] = cachedHandState.FingerConfidences_1;
				handState.FingerConfidences[2] = cachedHandState.FingerConfidences_2;
				handState.FingerConfidences[3] = cachedHandState.FingerConfidences_3;
				handState.FingerConfidences[4] = cachedHandState.FingerConfidences_4;
				handState.RequestedTimeStamp = cachedHandState.RequestedTimeStamp;
				handState.SampleTimeStamp = cachedHandState.SampleTimeStamp;
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool GetHandTrackingState(Step stepId, Hand hand, ref HandTrackingState handTrackingState)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version < OVRP_1_81_0.version)
		{
			return false;
		}
		if (OVRP_1_106_0.ovrp_GetHandTrackingState(stepId, -1, hand, out cachedHandTrackingState) != 0)
		{
			return false;
		}
		if (version >= OVRP_1_106_0.version)
		{
			handTrackingState.Microgesture = cachedHandTrackingState.Microgesture;
		}
		return true;
	}

	public static bool IsValidBone(BoneId bone, SkeletonType skeletonType)
	{
		switch (skeletonType)
		{
		case SkeletonType.HandLeft:
		case SkeletonType.HandRight:
			if (bone >= BoneId.Hand_Start)
			{
				return bone <= BoneId.Hand_End;
			}
			return false;
		case SkeletonType.XRHandLeft:
		case SkeletonType.XRHandRight:
			if (bone >= BoneId.Hand_Start)
			{
				return bone <= BoneId.XRHand_Max;
			}
			return false;
		case SkeletonType.Body:
			if (bone >= BoneId.Hand_Start)
			{
				return bone <= BoneId.Body_End;
			}
			return false;
		case SkeletonType.FullBody:
			if (bone >= BoneId.Hand_Start)
			{
				return bone <= BoneId.FullBody_End;
			}
			return false;
		default:
			return false;
		}
	}

	public static bool GetSkeleton(SkeletonType skeletonType, out Skeleton skeleton)
	{
		if (version >= OVRP_1_44_0.version)
		{
			return OVRP_1_44_0.ovrp_GetSkeleton(skeletonType, out skeleton) == Result.Success;
		}
		skeleton = default(Skeleton);
		return false;
	}

	public static bool GetSkeleton2(SkeletonType skeletonType, ref Skeleton2 skeleton)
	{
		if (version >= OVRP_1_92_0.version)
		{
			SkeletonType skeletonType2 = skeletonType;
			if (OVRSkeleton.IsBodySkeleton((OVRSkeleton.SkeletonType)skeletonType))
			{
				switch (_currentJointSet)
				{
				case BodyJointSet.FullBody:
					skeletonType2 = SkeletonType.FullBody;
					break;
				case BodyJointSet.UpperBody:
					skeletonType2 = SkeletonType.Body;
					break;
				case BodyJointSet.None:
					Debug.LogError("Global joint set is invalid. Ensure that there is an OVRBody instance that is active an enabled with a valid joint set");
					return false;
				}
			}
			if (OVRP_1_92_0.ovrp_GetSkeleton3(skeletonType2, out cachedSkeleton3) == Result.Success)
			{
				if (skeleton.BoneCapsules == null || skeleton.BoneCapsules.Length != 19)
				{
					skeleton.BoneCapsules = new BoneCapsule[19];
				}
				skeleton.Type = cachedSkeleton3.Type;
				skeleton.NumBoneCapsules = cachedSkeleton3.NumBoneCapsules;
				if (skeletonType == SkeletonType.Body && skeletonType2 == SkeletonType.FullBody)
				{
					uint num = 0u;
					for (int i = 0; i < cachedSkeleton3.NumBones; i++)
					{
						if (Skeleton3GetBone[i]().Id < BoneId.Body_End)
						{
							num++;
						}
					}
					skeleton.NumBones = num;
					if (skeleton.Bones == null || skeleton.Bones.Length != num)
					{
						skeleton.Bones = new Bone[num];
					}
					int num2 = 0;
					for (int j = 0; j < cachedSkeleton3.NumBones; j++)
					{
						if (Skeleton3GetBone[j]().Id < BoneId.Body_End)
						{
							skeleton.Bones[num2++] = Skeleton3GetBone[j]();
						}
					}
				}
				else
				{
					skeleton.NumBones = cachedSkeleton3.NumBones;
					if (skeleton.Bones == null || skeleton.Bones.Length != skeleton.NumBones)
					{
						skeleton.Bones = new Bone[skeleton.NumBones];
					}
					for (int k = 0; k < skeleton.NumBones; k++)
					{
						skeleton.Bones[k] = Skeleton3GetBone[k]();
					}
				}
				skeleton.BoneCapsules[0] = cachedSkeleton3.BoneCapsules_0;
				skeleton.BoneCapsules[1] = cachedSkeleton3.BoneCapsules_1;
				skeleton.BoneCapsules[2] = cachedSkeleton3.BoneCapsules_2;
				skeleton.BoneCapsules[3] = cachedSkeleton3.BoneCapsules_3;
				skeleton.BoneCapsules[4] = cachedSkeleton3.BoneCapsules_4;
				skeleton.BoneCapsules[5] = cachedSkeleton3.BoneCapsules_5;
				skeleton.BoneCapsules[6] = cachedSkeleton3.BoneCapsules_6;
				skeleton.BoneCapsules[7] = cachedSkeleton3.BoneCapsules_7;
				skeleton.BoneCapsules[8] = cachedSkeleton3.BoneCapsules_8;
				skeleton.BoneCapsules[9] = cachedSkeleton3.BoneCapsules_9;
				skeleton.BoneCapsules[10] = cachedSkeleton3.BoneCapsules_10;
				skeleton.BoneCapsules[11] = cachedSkeleton3.BoneCapsules_11;
				skeleton.BoneCapsules[12] = cachedSkeleton3.BoneCapsules_12;
				skeleton.BoneCapsules[13] = cachedSkeleton3.BoneCapsules_13;
				skeleton.BoneCapsules[14] = cachedSkeleton3.BoneCapsules_14;
				skeleton.BoneCapsules[15] = cachedSkeleton3.BoneCapsules_15;
				skeleton.BoneCapsules[16] = cachedSkeleton3.BoneCapsules_16;
				skeleton.BoneCapsules[17] = cachedSkeleton3.BoneCapsules_17;
				skeleton.BoneCapsules[18] = cachedSkeleton3.BoneCapsules_18;
				return true;
			}
			return false;
		}
		if (version >= OVRP_1_55_0.version)
		{
			if (OVRP_1_55_0.ovrp_GetSkeleton2(skeletonType, out cachedSkeleton2) == Result.Success)
			{
				if (skeleton.BoneCapsules == null || skeleton.BoneCapsules.Length != 19)
				{
					skeleton.BoneCapsules = new BoneCapsule[19];
				}
				skeleton.Type = cachedSkeleton2.Type;
				skeleton.NumBones = cachedSkeleton2.NumBones;
				skeleton.NumBoneCapsules = cachedSkeleton2.NumBoneCapsules;
				if (skeleton.Bones == null || skeleton.Bones.Length != skeleton.NumBones)
				{
					skeleton.Bones = new Bone[skeleton.NumBones];
				}
				for (int l = 0; l < skeleton.NumBones; l++)
				{
					skeleton.Bones[l] = Skeleton2GetBone[l]();
				}
				skeleton.BoneCapsules[0] = cachedSkeleton2.BoneCapsules_0;
				skeleton.BoneCapsules[1] = cachedSkeleton2.BoneCapsules_1;
				skeleton.BoneCapsules[2] = cachedSkeleton2.BoneCapsules_2;
				skeleton.BoneCapsules[3] = cachedSkeleton2.BoneCapsules_3;
				skeleton.BoneCapsules[4] = cachedSkeleton2.BoneCapsules_4;
				skeleton.BoneCapsules[5] = cachedSkeleton2.BoneCapsules_5;
				skeleton.BoneCapsules[6] = cachedSkeleton2.BoneCapsules_6;
				skeleton.BoneCapsules[7] = cachedSkeleton2.BoneCapsules_7;
				skeleton.BoneCapsules[8] = cachedSkeleton2.BoneCapsules_8;
				skeleton.BoneCapsules[9] = cachedSkeleton2.BoneCapsules_9;
				skeleton.BoneCapsules[10] = cachedSkeleton2.BoneCapsules_10;
				skeleton.BoneCapsules[11] = cachedSkeleton2.BoneCapsules_11;
				skeleton.BoneCapsules[12] = cachedSkeleton2.BoneCapsules_12;
				skeleton.BoneCapsules[13] = cachedSkeleton2.BoneCapsules_13;
				skeleton.BoneCapsules[14] = cachedSkeleton2.BoneCapsules_14;
				skeleton.BoneCapsules[15] = cachedSkeleton2.BoneCapsules_15;
				skeleton.BoneCapsules[16] = cachedSkeleton2.BoneCapsules_16;
				skeleton.BoneCapsules[17] = cachedSkeleton2.BoneCapsules_17;
				skeleton.BoneCapsules[18] = cachedSkeleton2.BoneCapsules_18;
				return true;
			}
			return false;
		}
		if (GetSkeleton(skeletonType, out cachedSkeleton))
		{
			if (skeleton.Bones == null || skeleton.Bones.Length != 84)
			{
				skeleton.Bones = new Bone[84];
			}
			if (skeleton.BoneCapsules == null || skeleton.BoneCapsules.Length != 19)
			{
				skeleton.BoneCapsules = new BoneCapsule[19];
			}
			skeleton.Type = cachedSkeleton.Type;
			skeleton.NumBones = cachedSkeleton.NumBones;
			skeleton.NumBoneCapsules = cachedSkeleton.NumBoneCapsules;
			for (int m = 0; m < skeleton.NumBones; m++)
			{
				skeleton.Bones[m] = cachedSkeleton.Bones[m];
			}
			for (int n = 0; n < skeleton.NumBoneCapsules; n++)
			{
				skeleton.BoneCapsules[n] = cachedSkeleton.BoneCapsules[n];
			}
			return true;
		}
		return false;
	}

	public static bool GetBodyState(Step stepId, ref BodyState bodyState)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version < OVRP_1_78_0.version)
		{
			return false;
		}
		BodyJointLocation[] jointLocations = bodyState.JointLocations;
		if (jointLocations == null || jointLocations.Length != 70)
		{
			bodyState.JointLocations = new BodyJointLocation[70];
		}
		if (OVRP_1_78_0.ovrp_GetBodyState(stepId, -1, out var bodyState2) != 0)
		{
			return false;
		}
		if (bodyState2.IsActive != Bool.True)
		{
			return false;
		}
		bodyState.Confidence = bodyState2.Confidence;
		bodyState.SkeletonChangedCount = bodyState2.SkeletonChangedCount;
		bodyState.Time = bodyState2.Time;
		bodyState.JointLocations[0] = bodyState2.JointLocation_0;
		bodyState.JointLocations[1] = bodyState2.JointLocation_1;
		bodyState.JointLocations[2] = bodyState2.JointLocation_2;
		bodyState.JointLocations[3] = bodyState2.JointLocation_3;
		bodyState.JointLocations[4] = bodyState2.JointLocation_4;
		bodyState.JointLocations[5] = bodyState2.JointLocation_5;
		bodyState.JointLocations[6] = bodyState2.JointLocation_6;
		bodyState.JointLocations[7] = bodyState2.JointLocation_7;
		bodyState.JointLocations[8] = bodyState2.JointLocation_8;
		bodyState.JointLocations[9] = bodyState2.JointLocation_9;
		bodyState.JointLocations[10] = bodyState2.JointLocation_10;
		bodyState.JointLocations[11] = bodyState2.JointLocation_11;
		bodyState.JointLocations[12] = bodyState2.JointLocation_12;
		bodyState.JointLocations[13] = bodyState2.JointLocation_13;
		bodyState.JointLocations[14] = bodyState2.JointLocation_14;
		bodyState.JointLocations[15] = bodyState2.JointLocation_15;
		bodyState.JointLocations[16] = bodyState2.JointLocation_16;
		bodyState.JointLocations[17] = bodyState2.JointLocation_17;
		bodyState.JointLocations[18] = bodyState2.JointLocation_18;
		bodyState.JointLocations[19] = bodyState2.JointLocation_19;
		bodyState.JointLocations[20] = bodyState2.JointLocation_20;
		bodyState.JointLocations[21] = bodyState2.JointLocation_21;
		bodyState.JointLocations[22] = bodyState2.JointLocation_22;
		bodyState.JointLocations[23] = bodyState2.JointLocation_23;
		bodyState.JointLocations[24] = bodyState2.JointLocation_24;
		bodyState.JointLocations[25] = bodyState2.JointLocation_25;
		bodyState.JointLocations[26] = bodyState2.JointLocation_26;
		bodyState.JointLocations[27] = bodyState2.JointLocation_27;
		bodyState.JointLocations[28] = bodyState2.JointLocation_28;
		bodyState.JointLocations[29] = bodyState2.JointLocation_29;
		bodyState.JointLocations[30] = bodyState2.JointLocation_30;
		bodyState.JointLocations[31] = bodyState2.JointLocation_31;
		bodyState.JointLocations[32] = bodyState2.JointLocation_32;
		bodyState.JointLocations[33] = bodyState2.JointLocation_33;
		bodyState.JointLocations[34] = bodyState2.JointLocation_34;
		bodyState.JointLocations[35] = bodyState2.JointLocation_35;
		bodyState.JointLocations[36] = bodyState2.JointLocation_36;
		bodyState.JointLocations[37] = bodyState2.JointLocation_37;
		bodyState.JointLocations[38] = bodyState2.JointLocation_38;
		bodyState.JointLocations[39] = bodyState2.JointLocation_39;
		bodyState.JointLocations[40] = bodyState2.JointLocation_40;
		bodyState.JointLocations[41] = bodyState2.JointLocation_41;
		bodyState.JointLocations[42] = bodyState2.JointLocation_42;
		bodyState.JointLocations[43] = bodyState2.JointLocation_43;
		bodyState.JointLocations[44] = bodyState2.JointLocation_44;
		bodyState.JointLocations[45] = bodyState2.JointLocation_45;
		bodyState.JointLocations[46] = bodyState2.JointLocation_46;
		bodyState.JointLocations[47] = bodyState2.JointLocation_47;
		bodyState.JointLocations[48] = bodyState2.JointLocation_48;
		bodyState.JointLocations[49] = bodyState2.JointLocation_49;
		bodyState.JointLocations[50] = bodyState2.JointLocation_50;
		bodyState.JointLocations[51] = bodyState2.JointLocation_51;
		bodyState.JointLocations[52] = bodyState2.JointLocation_52;
		bodyState.JointLocations[53] = bodyState2.JointLocation_53;
		bodyState.JointLocations[54] = bodyState2.JointLocation_54;
		bodyState.JointLocations[55] = bodyState2.JointLocation_55;
		bodyState.JointLocations[56] = bodyState2.JointLocation_56;
		bodyState.JointLocations[57] = bodyState2.JointLocation_57;
		bodyState.JointLocations[58] = bodyState2.JointLocation_58;
		bodyState.JointLocations[59] = bodyState2.JointLocation_59;
		bodyState.JointLocations[60] = bodyState2.JointLocation_60;
		bodyState.JointLocations[61] = bodyState2.JointLocation_61;
		bodyState.JointLocations[62] = bodyState2.JointLocation_62;
		bodyState.JointLocations[63] = bodyState2.JointLocation_63;
		bodyState.JointLocations[64] = bodyState2.JointLocation_64;
		bodyState.JointLocations[65] = bodyState2.JointLocation_65;
		bodyState.JointLocations[66] = bodyState2.JointLocation_66;
		bodyState.JointLocations[67] = bodyState2.JointLocation_67;
		bodyState.JointLocations[68] = bodyState2.JointLocation_68;
		bodyState.JointLocations[69] = bodyState2.JointLocation_69;
		return true;
	}

	public static bool GetBodyState4(Step stepId, BodyJointSet jointSet, ref BodyState bodyState)
	{
		if (version >= OVRP_1_92_0.version)
		{
			int num = ((jointSet == BodyJointSet.FullBody) ? 84 : 70);
			BodyJointLocation[] jointLocations = bodyState.JointLocations;
			if (jointLocations == null || jointLocations.Length != num)
			{
				bodyState.JointLocations = new BodyJointLocation[num];
			}
			if (OVRP_1_92_0.ovrp_GetBodyState4(stepId, -1, out var bodyState2) != 0 || bodyState2.IsActive != Bool.True)
			{
				return false;
			}
			bodyState.Confidence = bodyState2.Confidence;
			bodyState.SkeletonChangedCount = bodyState2.SkeletonChangedCount;
			bodyState.Time = bodyState2.Time;
			bodyState.Fidelity = bodyState2.Fidelity;
			bodyState.CalibrationStatus = bodyState2.CalibrationStatus;
			bodyState.JointLocations[0] = bodyState2.JointLocation_0;
			bodyState.JointLocations[1] = bodyState2.JointLocation_1;
			bodyState.JointLocations[2] = bodyState2.JointLocation_2;
			bodyState.JointLocations[3] = bodyState2.JointLocation_3;
			bodyState.JointLocations[4] = bodyState2.JointLocation_4;
			bodyState.JointLocations[5] = bodyState2.JointLocation_5;
			bodyState.JointLocations[6] = bodyState2.JointLocation_6;
			bodyState.JointLocations[7] = bodyState2.JointLocation_7;
			bodyState.JointLocations[8] = bodyState2.JointLocation_8;
			bodyState.JointLocations[9] = bodyState2.JointLocation_9;
			bodyState.JointLocations[10] = bodyState2.JointLocation_10;
			bodyState.JointLocations[11] = bodyState2.JointLocation_11;
			bodyState.JointLocations[12] = bodyState2.JointLocation_12;
			bodyState.JointLocations[13] = bodyState2.JointLocation_13;
			bodyState.JointLocations[14] = bodyState2.JointLocation_14;
			bodyState.JointLocations[15] = bodyState2.JointLocation_15;
			bodyState.JointLocations[16] = bodyState2.JointLocation_16;
			bodyState.JointLocations[17] = bodyState2.JointLocation_17;
			bodyState.JointLocations[18] = bodyState2.JointLocation_18;
			bodyState.JointLocations[19] = bodyState2.JointLocation_19;
			bodyState.JointLocations[20] = bodyState2.JointLocation_20;
			bodyState.JointLocations[21] = bodyState2.JointLocation_21;
			bodyState.JointLocations[22] = bodyState2.JointLocation_22;
			bodyState.JointLocations[23] = bodyState2.JointLocation_23;
			bodyState.JointLocations[24] = bodyState2.JointLocation_24;
			bodyState.JointLocations[25] = bodyState2.JointLocation_25;
			bodyState.JointLocations[26] = bodyState2.JointLocation_26;
			bodyState.JointLocations[27] = bodyState2.JointLocation_27;
			bodyState.JointLocations[28] = bodyState2.JointLocation_28;
			bodyState.JointLocations[29] = bodyState2.JointLocation_29;
			bodyState.JointLocations[30] = bodyState2.JointLocation_30;
			bodyState.JointLocations[31] = bodyState2.JointLocation_31;
			bodyState.JointLocations[32] = bodyState2.JointLocation_32;
			bodyState.JointLocations[33] = bodyState2.JointLocation_33;
			bodyState.JointLocations[34] = bodyState2.JointLocation_34;
			bodyState.JointLocations[35] = bodyState2.JointLocation_35;
			bodyState.JointLocations[36] = bodyState2.JointLocation_36;
			bodyState.JointLocations[37] = bodyState2.JointLocation_37;
			bodyState.JointLocations[38] = bodyState2.JointLocation_38;
			bodyState.JointLocations[39] = bodyState2.JointLocation_39;
			bodyState.JointLocations[40] = bodyState2.JointLocation_40;
			bodyState.JointLocations[41] = bodyState2.JointLocation_41;
			bodyState.JointLocations[42] = bodyState2.JointLocation_42;
			bodyState.JointLocations[43] = bodyState2.JointLocation_43;
			bodyState.JointLocations[44] = bodyState2.JointLocation_44;
			bodyState.JointLocations[45] = bodyState2.JointLocation_45;
			bodyState.JointLocations[46] = bodyState2.JointLocation_46;
			bodyState.JointLocations[47] = bodyState2.JointLocation_47;
			bodyState.JointLocations[48] = bodyState2.JointLocation_48;
			bodyState.JointLocations[49] = bodyState2.JointLocation_49;
			bodyState.JointLocations[50] = bodyState2.JointLocation_50;
			bodyState.JointLocations[51] = bodyState2.JointLocation_51;
			bodyState.JointLocations[52] = bodyState2.JointLocation_52;
			bodyState.JointLocations[53] = bodyState2.JointLocation_53;
			bodyState.JointLocations[54] = bodyState2.JointLocation_54;
			bodyState.JointLocations[55] = bodyState2.JointLocation_55;
			bodyState.JointLocations[56] = bodyState2.JointLocation_56;
			bodyState.JointLocations[57] = bodyState2.JointLocation_57;
			bodyState.JointLocations[58] = bodyState2.JointLocation_58;
			bodyState.JointLocations[59] = bodyState2.JointLocation_59;
			bodyState.JointLocations[60] = bodyState2.JointLocation_60;
			bodyState.JointLocations[61] = bodyState2.JointLocation_61;
			bodyState.JointLocations[62] = bodyState2.JointLocation_62;
			bodyState.JointLocations[63] = bodyState2.JointLocation_63;
			bodyState.JointLocations[64] = bodyState2.JointLocation_64;
			bodyState.JointLocations[65] = bodyState2.JointLocation_65;
			bodyState.JointLocations[66] = bodyState2.JointLocation_66;
			bodyState.JointLocations[67] = bodyState2.JointLocation_67;
			bodyState.JointLocations[68] = bodyState2.JointLocation_68;
			bodyState.JointLocations[69] = bodyState2.JointLocation_69;
			if (jointSet == BodyJointSet.FullBody)
			{
				bodyState.JointLocations[70] = bodyState2.JointLocation_70;
				bodyState.JointLocations[71] = bodyState2.JointLocation_71;
				bodyState.JointLocations[72] = bodyState2.JointLocation_72;
				bodyState.JointLocations[73] = bodyState2.JointLocation_73;
				bodyState.JointLocations[74] = bodyState2.JointLocation_74;
				bodyState.JointLocations[75] = bodyState2.JointLocation_75;
				bodyState.JointLocations[76] = bodyState2.JointLocation_76;
				bodyState.JointLocations[77] = bodyState2.JointLocation_77;
				bodyState.JointLocations[78] = bodyState2.JointLocation_78;
				bodyState.JointLocations[79] = bodyState2.JointLocation_79;
				bodyState.JointLocations[80] = bodyState2.JointLocation_80;
				bodyState.JointLocations[81] = bodyState2.JointLocation_81;
				bodyState.JointLocations[82] = bodyState2.JointLocation_82;
				bodyState.JointLocations[83] = bodyState2.JointLocation_83;
			}
			return true;
		}
		return GetBodyState(stepId, ref bodyState);
	}

	public static bool GetMesh(MeshType meshType, out Mesh mesh)
	{
		if (version >= OVRP_1_44_0.version)
		{
			mesh = new Mesh();
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(mesh));
			Result num = OVRP_1_44_0.ovrp_GetMesh(meshType, intPtr);
			if (num == Result.Success)
			{
				Marshal.PtrToStructure(intPtr, mesh);
			}
			Marshal.FreeHGlobal(intPtr);
			return num == Result.Success;
		}
		mesh = new Mesh();
		return false;
	}

	public static Result CreateVirtualKeyboard(VirtualKeyboardCreateInfo createInfo)
	{
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_CreateVirtualKeyboard(createInfo);
		}
		return Result.Failure_Unsupported;
	}

	public static Result DestroyVirtualKeyboard()
	{
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_DestroyVirtualKeyboard();
		}
		return Result.Failure_Unsupported;
	}

	public static Result SendVirtualKeyboardInput(VirtualKeyboardInputInfo inputInfo, ref Posef interactorRootPose)
	{
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_SendVirtualKeyboardInput(inputInfo, ref interactorRootPose);
		}
		return Result.Failure_Unsupported;
	}

	public static Result ChangeVirtualKeyboardTextContext(string textContext)
	{
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_ChangeVirtualKeyboardTextContext(textContext);
		}
		return Result.Failure_Unsupported;
	}

	public static Result CreateVirtualKeyboardSpace(VirtualKeyboardSpaceCreateInfo createInfo, out ulong keyboardSpace)
	{
		keyboardSpace = 0uL;
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_CreateVirtualKeyboardSpace(createInfo, out keyboardSpace);
		}
		return Result.Failure_Unsupported;
	}

	public static Result SuggestVirtualKeyboardLocation(VirtualKeyboardLocationInfo locationInfo)
	{
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_SuggestVirtualKeyboardLocation(locationInfo);
		}
		return Result.Failure_Unsupported;
	}

	public static Result GetVirtualKeyboardScale(out float scale)
	{
		scale = 0f;
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_GetVirtualKeyboardScale(out scale);
		}
		return Result.Failure_Unsupported;
	}

	public static Result GetVirtualKeyboardModelAnimationStates(VirtualKeyboardModelAnimationStateBufferProvider bufferProvider, VirtualKeyboardModelAnimationStateHandler stateHandler)
	{
		if (version >= OVRP_1_83_0.version)
		{
			VirtualKeyboardModelAnimationStatesInternal virtualKeyboardModelAnimationStatesInternal = default(VirtualKeyboardModelAnimationStatesInternal);
			virtualKeyboardModelAnimationStatesInternal.StateCapacityInput = 0u;
			VirtualKeyboardModelAnimationStatesInternal animationStates = virtualKeyboardModelAnimationStatesInternal;
			Result result = OVRP_1_83_0.ovrp_GetVirtualKeyboardModelAnimationStates(ref animationStates);
			if (result != 0)
			{
				Debug.LogError("GetVirtualKeyboardModelAnimationStates failed: cannot query animation state data:" + result);
			}
			if (animationStates.StateCountOutput == 0 || result != 0)
			{
				return result;
			}
			int num = Marshal.SizeOf(typeof(VirtualKeyboardModelAnimationState));
			animationStates.StatesBuffer = bufferProvider((int)animationStates.StateCountOutput * num, (int)animationStates.StateCountOutput);
			animationStates.StateCapacityInput = animationStates.StateCountOutput;
			result = OVRP_1_83_0.ovrp_GetVirtualKeyboardModelAnimationStates(ref animationStates);
			if (result != 0)
			{
				Debug.LogError("GetVirtualKeyboardModelAnimationStates failed: cannot populate animation state data:" + result);
			}
			else
			{
				int[] array = new int[1];
				float[] array2 = new float[1];
				VirtualKeyboardModelAnimationState state = default(VirtualKeyboardModelAnimationState);
				for (int i = 0; i < animationStates.StateCountOutput; i++)
				{
					IntPtr intPtr = IntPtr.Add(animationStates.StatesBuffer, i * num);
					Marshal.Copy(intPtr, array, 0, 1);
					Marshal.Copy(IntPtr.Add(intPtr, 4), array2, 0, 1);
					state.AnimationIndex = array[0];
					state.Fraction = array2[0];
					stateHandler(ref state);
				}
			}
			return result;
		}
		return Result.Failure_Unsupported;
	}

	[Obsolete("Use GetVirtualKeyboardModelAnimationStates with delegates")]
	public static Result GetVirtualKeyboardModelAnimationStates(out VirtualKeyboardModelAnimationStates animationStates)
	{
		animationStates = default(VirtualKeyboardModelAnimationStates);
		if (version >= OVRP_1_83_0.version)
		{
			Marshal.SizeOf(typeof(VirtualKeyboardModelAnimationState));
			VirtualKeyboardModelAnimationState[] states = Array.Empty<VirtualKeyboardModelAnimationState>();
			int i = 0;
			IntPtr buffer = IntPtr.Zero;
			try
			{
				Result virtualKeyboardModelAnimationStates = GetVirtualKeyboardModelAnimationStates(delegate(int bufferSize, int stateCount)
				{
					buffer = Marshal.AllocHGlobal(bufferSize);
					states = new VirtualKeyboardModelAnimationState[stateCount];
					return buffer;
				}, delegate(ref VirtualKeyboardModelAnimationState state)
				{
					states[i++] = state;
				});
				animationStates.States = states;
				return virtualKeyboardModelAnimationStates;
			}
			finally
			{
				Marshal.FreeHGlobal(buffer);
			}
		}
		return Result.Failure_Unsupported;
	}

	public static Result GetVirtualKeyboardDirtyTextures(out VirtualKeyboardTextureIds textureIds)
	{
		textureIds = default(VirtualKeyboardTextureIds);
		if (version >= OVRP_1_83_0.version)
		{
			VirtualKeyboardTextureIdsInternal textureIds2 = default(VirtualKeyboardTextureIdsInternal);
			Result result = OVRP_1_83_0.ovrp_GetVirtualKeyboardDirtyTextures(ref textureIds2);
			textureIds.TextureIds = new ulong[textureIds2.TextureIdCountOutput];
			if (textureIds2.TextureIdCountOutput == 0)
			{
				if (result != 0)
				{
					Debug.LogError("GetVirtualKeyboardDirtyTextures failed: cannot query dirty textures data:" + result);
				}
				return result;
			}
			GCHandle gCHandle = GCHandle.Alloc(textureIds.TextureIds, GCHandleType.Pinned);
			try
			{
				textureIds2.TextureIdCapacityInput = textureIds2.TextureIdCountOutput;
				textureIds2.TextureIdsBuffer = gCHandle.AddrOfPinnedObject();
				result = OVRP_1_83_0.ovrp_GetVirtualKeyboardDirtyTextures(ref textureIds2);
				if (result != 0)
				{
					Debug.LogError("GetVirtualKeyboardDirtyTextures failed: cannot populate dirty textures data:" + result);
				}
				return result;
			}
			finally
			{
				gCHandle.Free();
			}
		}
		return Result.Failure_Unsupported;
	}

	public static Result GetVirtualKeyboardTextureData(ulong textureId, ref VirtualKeyboardTextureData textureData)
	{
		if (version >= OVRP_1_83_0.version)
		{
			return OVRP_1_83_0.ovrp_GetVirtualKeyboardTextureData(textureId, ref textureData);
		}
		return Result.Failure_Unsupported;
	}

	public static Result SetVirtualKeyboardModelVisibility(ref VirtualKeyboardModelVisibility visibility)
	{
		if (version >= OVRP_1_83_0.version)
		{
			return OVRP_1_83_0.ovrp_SetVirtualKeyboardModelVisibility(ref visibility);
		}
		return Result.Failure_Unsupported;
	}

	private static bool GetFaceStateInternal(Step stepId, int frameIndex, ref FaceState faceState)
	{
		if (OVRP_1_78_0.ovrp_GetFaceState(stepId, frameIndex, out cachedFaceState) != 0)
		{
			return false;
		}
		if (faceState.ExpressionWeights == null || faceState.ExpressionWeights.Length != 63)
		{
			faceState.ExpressionWeights = new float[63];
		}
		if (faceState.ExpressionWeightConfidences == null || faceState.ExpressionWeightConfidences.Length != 2)
		{
			faceState.ExpressionWeightConfidences = new float[2];
		}
		faceState.ExpressionWeights[0] = cachedFaceState.ExpressionWeights_0;
		faceState.ExpressionWeights[1] = cachedFaceState.ExpressionWeights_1;
		faceState.ExpressionWeights[2] = cachedFaceState.ExpressionWeights_2;
		faceState.ExpressionWeights[3] = cachedFaceState.ExpressionWeights_3;
		faceState.ExpressionWeights[4] = cachedFaceState.ExpressionWeights_4;
		faceState.ExpressionWeights[5] = cachedFaceState.ExpressionWeights_5;
		faceState.ExpressionWeights[6] = cachedFaceState.ExpressionWeights_6;
		faceState.ExpressionWeights[7] = cachedFaceState.ExpressionWeights_7;
		faceState.ExpressionWeights[8] = cachedFaceState.ExpressionWeights_8;
		faceState.ExpressionWeights[9] = cachedFaceState.ExpressionWeights_9;
		faceState.ExpressionWeights[10] = cachedFaceState.ExpressionWeights_10;
		faceState.ExpressionWeights[11] = cachedFaceState.ExpressionWeights_11;
		faceState.ExpressionWeights[12] = cachedFaceState.ExpressionWeights_12;
		faceState.ExpressionWeights[13] = cachedFaceState.ExpressionWeights_13;
		faceState.ExpressionWeights[14] = cachedFaceState.ExpressionWeights_14;
		faceState.ExpressionWeights[15] = cachedFaceState.ExpressionWeights_15;
		faceState.ExpressionWeights[16] = cachedFaceState.ExpressionWeights_16;
		faceState.ExpressionWeights[17] = cachedFaceState.ExpressionWeights_17;
		faceState.ExpressionWeights[18] = cachedFaceState.ExpressionWeights_18;
		faceState.ExpressionWeights[19] = cachedFaceState.ExpressionWeights_19;
		faceState.ExpressionWeights[20] = cachedFaceState.ExpressionWeights_20;
		faceState.ExpressionWeights[21] = cachedFaceState.ExpressionWeights_21;
		faceState.ExpressionWeights[22] = cachedFaceState.ExpressionWeights_22;
		faceState.ExpressionWeights[23] = cachedFaceState.ExpressionWeights_23;
		faceState.ExpressionWeights[24] = cachedFaceState.ExpressionWeights_24;
		faceState.ExpressionWeights[25] = cachedFaceState.ExpressionWeights_25;
		faceState.ExpressionWeights[26] = cachedFaceState.ExpressionWeights_26;
		faceState.ExpressionWeights[27] = cachedFaceState.ExpressionWeights_27;
		faceState.ExpressionWeights[28] = cachedFaceState.ExpressionWeights_28;
		faceState.ExpressionWeights[29] = cachedFaceState.ExpressionWeights_29;
		faceState.ExpressionWeights[30] = cachedFaceState.ExpressionWeights_30;
		faceState.ExpressionWeights[31] = cachedFaceState.ExpressionWeights_31;
		faceState.ExpressionWeights[32] = cachedFaceState.ExpressionWeights_32;
		faceState.ExpressionWeights[33] = cachedFaceState.ExpressionWeights_33;
		faceState.ExpressionWeights[34] = cachedFaceState.ExpressionWeights_34;
		faceState.ExpressionWeights[35] = cachedFaceState.ExpressionWeights_35;
		faceState.ExpressionWeights[36] = cachedFaceState.ExpressionWeights_36;
		faceState.ExpressionWeights[37] = cachedFaceState.ExpressionWeights_37;
		faceState.ExpressionWeights[38] = cachedFaceState.ExpressionWeights_38;
		faceState.ExpressionWeights[39] = cachedFaceState.ExpressionWeights_39;
		faceState.ExpressionWeights[40] = cachedFaceState.ExpressionWeights_40;
		faceState.ExpressionWeights[41] = cachedFaceState.ExpressionWeights_41;
		faceState.ExpressionWeights[42] = cachedFaceState.ExpressionWeights_42;
		faceState.ExpressionWeights[43] = cachedFaceState.ExpressionWeights_43;
		faceState.ExpressionWeights[44] = cachedFaceState.ExpressionWeights_44;
		faceState.ExpressionWeights[45] = cachedFaceState.ExpressionWeights_45;
		faceState.ExpressionWeights[46] = cachedFaceState.ExpressionWeights_46;
		faceState.ExpressionWeights[47] = cachedFaceState.ExpressionWeights_47;
		faceState.ExpressionWeights[48] = cachedFaceState.ExpressionWeights_48;
		faceState.ExpressionWeights[49] = cachedFaceState.ExpressionWeights_49;
		faceState.ExpressionWeights[50] = cachedFaceState.ExpressionWeights_50;
		faceState.ExpressionWeights[51] = cachedFaceState.ExpressionWeights_51;
		faceState.ExpressionWeights[52] = cachedFaceState.ExpressionWeights_52;
		faceState.ExpressionWeights[53] = cachedFaceState.ExpressionWeights_53;
		faceState.ExpressionWeights[54] = cachedFaceState.ExpressionWeights_54;
		faceState.ExpressionWeights[55] = cachedFaceState.ExpressionWeights_55;
		faceState.ExpressionWeights[56] = cachedFaceState.ExpressionWeights_56;
		faceState.ExpressionWeights[57] = cachedFaceState.ExpressionWeights_57;
		faceState.ExpressionWeights[58] = cachedFaceState.ExpressionWeights_58;
		faceState.ExpressionWeights[59] = cachedFaceState.ExpressionWeights_59;
		faceState.ExpressionWeights[60] = cachedFaceState.ExpressionWeights_60;
		faceState.ExpressionWeights[61] = cachedFaceState.ExpressionWeights_61;
		faceState.ExpressionWeights[62] = cachedFaceState.ExpressionWeights_62;
		faceState.ExpressionWeightConfidences[0] = cachedFaceState.ExpressionWeightConfidences_0;
		faceState.ExpressionWeightConfidences[1] = cachedFaceState.ExpressionWeightConfidences_1;
		faceState.Status = cachedFaceState.Status.ToFaceExpressionStatus();
		faceState.Time = cachedFaceState.Time;
		return true;
	}

	public static bool GetFaceState(Step stepId, int frameIndex, ref FaceState faceState)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			stepId = Step.Render;
		}
		if (version >= OVRP_1_78_0.version)
		{
			return GetFaceStateInternal(stepId, frameIndex, ref faceState);
		}
		return false;
	}

	public static bool GetFaceState2(Step stepId, int frameIndex, ref FaceState faceState)
	{
		if (version >= OVRP_1_92_0.version)
		{
			if (OVRP_1_92_0.ovrp_GetFaceState2(stepId, frameIndex, out cachedFaceState2) != 0)
			{
				return false;
			}
			if (faceState.ExpressionWeights == null || faceState.ExpressionWeights.Length != 70)
			{
				faceState.ExpressionWeights = new float[70];
			}
			if (faceState.ExpressionWeightConfidences == null || faceState.ExpressionWeightConfidences.Length != 2)
			{
				faceState.ExpressionWeightConfidences = new float[2];
			}
			faceState.ExpressionWeights[0] = cachedFaceState2.ExpressionWeights_0;
			faceState.ExpressionWeights[1] = cachedFaceState2.ExpressionWeights_1;
			faceState.ExpressionWeights[2] = cachedFaceState2.ExpressionWeights_2;
			faceState.ExpressionWeights[3] = cachedFaceState2.ExpressionWeights_3;
			faceState.ExpressionWeights[4] = cachedFaceState2.ExpressionWeights_4;
			faceState.ExpressionWeights[5] = cachedFaceState2.ExpressionWeights_5;
			faceState.ExpressionWeights[6] = cachedFaceState2.ExpressionWeights_6;
			faceState.ExpressionWeights[7] = cachedFaceState2.ExpressionWeights_7;
			faceState.ExpressionWeights[8] = cachedFaceState2.ExpressionWeights_8;
			faceState.ExpressionWeights[9] = cachedFaceState2.ExpressionWeights_9;
			faceState.ExpressionWeights[10] = cachedFaceState2.ExpressionWeights_10;
			faceState.ExpressionWeights[11] = cachedFaceState2.ExpressionWeights_11;
			faceState.ExpressionWeights[12] = cachedFaceState2.ExpressionWeights_12;
			faceState.ExpressionWeights[13] = cachedFaceState2.ExpressionWeights_13;
			faceState.ExpressionWeights[14] = cachedFaceState2.ExpressionWeights_14;
			faceState.ExpressionWeights[15] = cachedFaceState2.ExpressionWeights_15;
			faceState.ExpressionWeights[16] = cachedFaceState2.ExpressionWeights_16;
			faceState.ExpressionWeights[17] = cachedFaceState2.ExpressionWeights_17;
			faceState.ExpressionWeights[18] = cachedFaceState2.ExpressionWeights_18;
			faceState.ExpressionWeights[19] = cachedFaceState2.ExpressionWeights_19;
			faceState.ExpressionWeights[20] = cachedFaceState2.ExpressionWeights_20;
			faceState.ExpressionWeights[21] = cachedFaceState2.ExpressionWeights_21;
			faceState.ExpressionWeights[22] = cachedFaceState2.ExpressionWeights_22;
			faceState.ExpressionWeights[23] = cachedFaceState2.ExpressionWeights_23;
			faceState.ExpressionWeights[24] = cachedFaceState2.ExpressionWeights_24;
			faceState.ExpressionWeights[25] = cachedFaceState2.ExpressionWeights_25;
			faceState.ExpressionWeights[26] = cachedFaceState2.ExpressionWeights_26;
			faceState.ExpressionWeights[27] = cachedFaceState2.ExpressionWeights_27;
			faceState.ExpressionWeights[28] = cachedFaceState2.ExpressionWeights_28;
			faceState.ExpressionWeights[29] = cachedFaceState2.ExpressionWeights_29;
			faceState.ExpressionWeights[30] = cachedFaceState2.ExpressionWeights_30;
			faceState.ExpressionWeights[31] = cachedFaceState2.ExpressionWeights_31;
			faceState.ExpressionWeights[32] = cachedFaceState2.ExpressionWeights_32;
			faceState.ExpressionWeights[33] = cachedFaceState2.ExpressionWeights_33;
			faceState.ExpressionWeights[34] = cachedFaceState2.ExpressionWeights_34;
			faceState.ExpressionWeights[35] = cachedFaceState2.ExpressionWeights_35;
			faceState.ExpressionWeights[36] = cachedFaceState2.ExpressionWeights_36;
			faceState.ExpressionWeights[37] = cachedFaceState2.ExpressionWeights_37;
			faceState.ExpressionWeights[38] = cachedFaceState2.ExpressionWeights_38;
			faceState.ExpressionWeights[39] = cachedFaceState2.ExpressionWeights_39;
			faceState.ExpressionWeights[40] = cachedFaceState2.ExpressionWeights_40;
			faceState.ExpressionWeights[41] = cachedFaceState2.ExpressionWeights_41;
			faceState.ExpressionWeights[42] = cachedFaceState2.ExpressionWeights_42;
			faceState.ExpressionWeights[43] = cachedFaceState2.ExpressionWeights_43;
			faceState.ExpressionWeights[44] = cachedFaceState2.ExpressionWeights_44;
			faceState.ExpressionWeights[45] = cachedFaceState2.ExpressionWeights_45;
			faceState.ExpressionWeights[46] = cachedFaceState2.ExpressionWeights_46;
			faceState.ExpressionWeights[47] = cachedFaceState2.ExpressionWeights_47;
			faceState.ExpressionWeights[48] = cachedFaceState2.ExpressionWeights_48;
			faceState.ExpressionWeights[49] = cachedFaceState2.ExpressionWeights_49;
			faceState.ExpressionWeights[50] = cachedFaceState2.ExpressionWeights_50;
			faceState.ExpressionWeights[51] = cachedFaceState2.ExpressionWeights_51;
			faceState.ExpressionWeights[52] = cachedFaceState2.ExpressionWeights_52;
			faceState.ExpressionWeights[53] = cachedFaceState2.ExpressionWeights_53;
			faceState.ExpressionWeights[54] = cachedFaceState2.ExpressionWeights_54;
			faceState.ExpressionWeights[55] = cachedFaceState2.ExpressionWeights_55;
			faceState.ExpressionWeights[56] = cachedFaceState2.ExpressionWeights_56;
			faceState.ExpressionWeights[57] = cachedFaceState2.ExpressionWeights_57;
			faceState.ExpressionWeights[58] = cachedFaceState2.ExpressionWeights_58;
			faceState.ExpressionWeights[59] = cachedFaceState2.ExpressionWeights_59;
			faceState.ExpressionWeights[60] = cachedFaceState2.ExpressionWeights_60;
			faceState.ExpressionWeights[61] = cachedFaceState2.ExpressionWeights_61;
			faceState.ExpressionWeights[62] = cachedFaceState2.ExpressionWeights_62;
			faceState.ExpressionWeights[63] = cachedFaceState2.ExpressionWeights_63;
			faceState.ExpressionWeights[64] = cachedFaceState2.ExpressionWeights_64;
			faceState.ExpressionWeights[65] = cachedFaceState2.ExpressionWeights_65;
			faceState.ExpressionWeights[66] = cachedFaceState2.ExpressionWeights_66;
			faceState.ExpressionWeights[67] = cachedFaceState2.ExpressionWeights_67;
			faceState.ExpressionWeights[68] = cachedFaceState2.ExpressionWeights_68;
			faceState.ExpressionWeights[69] = cachedFaceState2.ExpressionWeights_69;
			faceState.ExpressionWeightConfidences[0] = cachedFaceState2.ExpressionWeightConfidences_0;
			faceState.ExpressionWeightConfidences[1] = cachedFaceState2.ExpressionWeightConfidences_1;
			faceState.Status = cachedFaceState2.Status.ToFaceExpressionStatus();
			faceState.Time = cachedFaceState2.Time;
			faceState.DataSource = cachedFaceState2.DataSource;
			return true;
		}
		return false;
	}

	public static Result GetFaceVisemesState(Step stepId, ref FaceVisemesState faceVisemesState)
	{
		if (version >= OVRP_1_104_0.version)
		{
			Result result = OVRP_1_104_0.ovrp_GetFaceVisemesState(stepId, -1, out cachedFaceVisemesState);
			if (result != 0)
			{
				return result;
			}
			if (faceVisemesState.Visemes == null || faceVisemesState.Visemes.Length != 15)
			{
				faceVisemesState.Visemes = new float[15];
			}
			faceVisemesState.Visemes[0] = cachedFaceVisemesState.Visemes_0;
			faceVisemesState.Visemes[1] = cachedFaceVisemesState.Visemes_1;
			faceVisemesState.Visemes[2] = cachedFaceVisemesState.Visemes_2;
			faceVisemesState.Visemes[3] = cachedFaceVisemesState.Visemes_3;
			faceVisemesState.Visemes[4] = cachedFaceVisemesState.Visemes_4;
			faceVisemesState.Visemes[5] = cachedFaceVisemesState.Visemes_5;
			faceVisemesState.Visemes[6] = cachedFaceVisemesState.Visemes_6;
			faceVisemesState.Visemes[7] = cachedFaceVisemesState.Visemes_7;
			faceVisemesState.Visemes[8] = cachedFaceVisemesState.Visemes_8;
			faceVisemesState.Visemes[9] = cachedFaceVisemesState.Visemes_9;
			faceVisemesState.Visemes[10] = cachedFaceVisemesState.Visemes_10;
			faceVisemesState.Visemes[11] = cachedFaceVisemesState.Visemes_11;
			faceVisemesState.Visemes[12] = cachedFaceVisemesState.Visemes_12;
			faceVisemesState.Visemes[13] = cachedFaceVisemesState.Visemes_13;
			faceVisemesState.Visemes[14] = cachedFaceVisemesState.Visemes_14;
			faceVisemesState.IsValid = cachedFaceVisemesState.IsValid == Bool.True;
			faceVisemesState.Time = cachedFaceVisemesState.Time;
			return Result.Success;
		}
		return Result.Failure_Unsupported;
	}

	public static Result SetFaceTrackingVisemesEnabled(bool enabled)
	{
		if (version >= OVRP_1_104_0.version)
		{
			return OVRP_1_104_0.ovrp_SetFaceTrackingVisemesEnabled(enabled ? Bool.True : Bool.False);
		}
		return Result.Failure_Unsupported;
	}

	public static bool GetEyeGazesState(Step stepId, int frameIndex, ref EyeGazesState eyeGazesState)
	{
		if (nativeXrApi == XrApi.OpenXR && stepId == Step.Physics)
		{
			Debug.LogWarning("Step.Physics is deprecated when using OpenXR");
			stepId = Step.Render;
		}
		if (version >= OVRP_1_78_0.version)
		{
			if (OVRP_1_78_0.ovrp_GetEyeGazesState(stepId, frameIndex, out cachedEyeGazesState) == Result.Success)
			{
				if (eyeGazesState.EyeGazes == null || eyeGazesState.EyeGazes.Length != 2)
				{
					eyeGazesState.EyeGazes = new EyeGazeState[2];
				}
				eyeGazesState.EyeGazes[0] = cachedEyeGazesState.EyeGazes_0;
				eyeGazesState.EyeGazes[1] = cachedEyeGazesState.EyeGazes_1;
				eyeGazesState.Time = cachedEyeGazesState.Time;
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool StartEyeTracking()
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StartEyeTracking() == Result.Success;
		}
		return false;
	}

	public static bool StopEyeTracking()
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StopEyeTracking() == Result.Success;
		}
		return false;
	}

	public static bool StartFaceTracking()
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StartFaceTracking() == Result.Success;
		}
		return false;
	}

	public static bool StopFaceTracking()
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StopFaceTracking() == Result.Success;
		}
		return false;
	}

	public static bool StartFaceTracking2(FaceTrackingDataSource[] requestedFaceTrackingDataSources)
	{
		if (version >= OVRP_1_92_0.version)
		{
			return OVRP_1_92_0.ovrp_StartFaceTracking2(requestedFaceTrackingDataSources, (requestedFaceTrackingDataSources != null) ? ((uint)requestedFaceTrackingDataSources.Length) : 0u) == Result.Success;
		}
		return false;
	}

	public static bool StopFaceTracking2()
	{
		if (version >= OVRP_1_92_0.version)
		{
			return OVRP_1_92_0.ovrp_StopFaceTracking2() == Result.Success;
		}
		return false;
	}

	public static bool StartBodyTracking2(BodyJointSet jointSet)
	{
		if (version >= OVRP_1_92_0.version)
		{
			if (_currentJointSet != BodyJointSet.None)
			{
				return true;
			}
			if (OVRP_1_92_0.ovrp_StartBodyTracking2(jointSet) == Result.Success)
			{
				_currentJointSet = jointSet;
				return true;
			}
			return false;
		}
		if (jointSet != BodyJointSet.FullBody)
		{
			return StartBodyTracking();
		}
		Debug.LogError("Full body joint set is not supported by this version of OVRPlugin.");
		return false;
	}

	public static bool StartBodyTracking()
	{
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StartBodyTracking() == Result.Success;
		}
		return false;
	}

	public static bool RequestBodyTrackingFidelity(BodyTrackingFidelity2 fidelity)
	{
		if (version >= OVRP_1_92_0.version)
		{
			return OVRP_1_92_0.ovrp_RequestBodyTrackingFidelity(fidelity) == Result.Success;
		}
		return false;
	}

	public static bool SuggestBodyTrackingCalibrationOverride(BodyTrackingCalibrationInfo calibrationInfo)
	{
		if (version >= OVRP_1_92_0.version)
		{
			return OVRP_1_92_0.ovrp_SuggestBodyTrackingCalibrationOverride(calibrationInfo) == Result.Success;
		}
		return false;
	}

	public static bool ResetBodyTrackingCalibration()
	{
		if (version >= OVRP_1_92_0.version)
		{
			return OVRP_1_92_0.ovrp_ResetBodyTrackingCalibration() == Result.Success;
		}
		return false;
	}

	public static bool StopBodyTracking()
	{
		if (version >= OVRP_1_92_0.version)
		{
			_currentJointSet = BodyJointSet.None;
		}
		if (version >= OVRP_1_78_0.version)
		{
			return OVRP_1_78_0.ovrp_StopBodyTracking() == Result.Success;
		}
		return false;
	}

	public static int GetLocalTrackingSpaceRecenterCount()
	{
		if (version >= OVRP_1_44_0.version)
		{
			int recenterCount = 0;
			if (OVRP_1_44_0.ovrp_GetLocalTrackingSpaceRecenterCount(ref recenterCount) == Result.Success)
			{
				return recenterCount;
			}
			return 0;
		}
		return 0;
	}

	public static bool GetSystemHmd3DofModeEnabled()
	{
		if (version >= OVRP_1_45_0.version)
		{
			Bool enabled = Bool.False;
			if (OVRP_1_45_0.ovrp_GetSystemHmd3DofModeEnabled(ref enabled) == Result.Success)
			{
				return enabled == Bool.True;
			}
			return false;
		}
		return false;
	}

	public static bool SetClientColorDesc(ColorSpace colorSpace)
	{
		if (version >= OVRP_1_49_0.version)
		{
			if (colorSpace == ColorSpace.Unknown)
			{
				Debug.LogWarning("A color gamut of Unknown is not supported. Defaulting to DCI-P3 color space instead.");
				colorSpace = ColorSpace.P3;
			}
			return OVRP_1_49_0.ovrp_SetClientColorDesc(colorSpace) == Result.Success;
		}
		return false;
	}

	public static ColorSpace GetHmdColorDesc()
	{
		ColorSpace colorSpace = ColorSpace.Unknown;
		if (version >= OVRP_1_49_0.version)
		{
			if (OVRP_1_49_0.ovrp_GetHmdColorDesc(ref colorSpace) != 0)
			{
				Debug.LogError("GetHmdColorDesc: Failed to get Hmd color description");
			}
			return colorSpace;
		}
		Debug.LogError("GetHmdColorDesc: Not supported on this version of OVRPlugin");
		return colorSpace;
	}

	public static bool PollEvent(ref EventDataBuffer eventDataBuffer)
	{
		if (version >= OVRP_1_55_1.version)
		{
			IntPtr eventData = IntPtr.Zero;
			if (eventDataBuffer.EventData == null)
			{
				eventDataBuffer.EventData = new byte[4000];
			}
			if (OVRP_1_55_1.ovrp_PollEvent2(ref eventDataBuffer.EventType, ref eventData) != 0 || eventData == IntPtr.Zero)
			{
				return false;
			}
			Marshal.Copy(eventData, eventDataBuffer.EventData, 0, 4000);
			return true;
		}
		if (version >= OVRP_1_55_0.version)
		{
			return OVRP_1_55_0.ovrp_PollEvent(ref eventDataBuffer) == Result.Success;
		}
		eventDataBuffer = default(EventDataBuffer);
		return false;
	}

	public static ulong GetNativeOpenXRInstance()
	{
		if (version >= OVRP_1_55_0.version && OVRP_1_55_0.ovrp_GetNativeOpenXRHandles(out var xrInstance, out var _) == Result.Success)
		{
			return xrInstance;
		}
		return 0uL;
	}

	public static ulong GetNativeOpenXRSession()
	{
		if (version >= OVRP_1_55_0.version && OVRP_1_55_0.ovrp_GetNativeOpenXRHandles(out var _, out var xrSession) == Result.Success)
		{
			return xrSession;
		}
		return 0uL;
	}

	internal static double GetPredictedDisplayTime()
	{
		if (version < OVRP_1_44_0.version)
		{
			return 0.0;
		}
		double predictedDisplayTime = 0.0;
		if (OVRP_1_44_0.ovrp_GetPredictedDisplayTime(-1, ref predictedDisplayTime) == Result.Success)
		{
			return predictedDisplayTime;
		}
		return 0.0;
	}

	internal static IntPtr GetOpenXRInstanceProcAddrFunc()
	{
		if (version < OVRP_1_104_0.version)
		{
			return IntPtr.Zero;
		}
		IntPtr func = IntPtr.Zero;
		if (OVRP_1_104_0.ovrp_GetOpenXRInstanceProcAddrFunc(ref func) == Result.Success)
		{
			return func;
		}
		return IntPtr.Zero;
	}

	internal static Result RegisterOpenXREventHandler(OpenXREventDelegateType eventHandler)
	{
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_104_0.ovrp_RegisterOpenXREventHandler(eventHandler, IntPtr.Zero);
	}

	internal static Result UnregisterOpenXREventHandler(OpenXREventDelegateType eventHandler)
	{
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_104_0.ovrp_UnregisterOpenXREventHandler(eventHandler);
	}

	internal static ulong GetAppSpace()
	{
		if (version < OVRP_1_107_0.version)
		{
			return 0uL;
		}
		ulong appSpace = 0uL;
		if (OVRP_1_107_0.ovrp_GetAppSpace(ref appSpace) == Result.Success)
		{
			return appSpace;
		}
		return 0uL;
	}

	public static bool SetKeyboardOverlayUV(Vector2f uv)
	{
		if (version >= OVRP_1_57_0.version)
		{
			return OVRP_1_57_0.ovrp_SetKeyboardOverlayUV(uv) == Result.Success;
		}
		return false;
	}

	public static bool CreateSpatialAnchor(SpatialAnchorCreateInfo createInfo, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_CreateSpatialAnchor(ref createInfo, out requestId) == Result.Success;
		}
		return false;
	}

	public static bool SetSpaceComponentStatus(ulong space, SpaceComponentType componentType, bool enable, double timeout, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_SetSpaceComponentStatus(ref space, componentType, ToBool(enable), timeout, out requestId) == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceComponentStatus(ulong space, SpaceComponentType componentType, out bool enabled, out bool changePending)
	{
		return GetSpaceComponentStatusInternal(space, componentType, out enabled, out changePending) == Result.Success;
	}

	internal static Result GetSpaceComponentStatusInternal(ulong space, SpaceComponentType componentType, out bool enabled, out bool changePending)
	{
		enabled = false;
		changePending = false;
		if (version >= OVRP_1_72_0.version)
		{
			Bool enabled2;
			Bool changePending2;
			Result result = OVRP_1_72_0.ovrp_GetSpaceComponentStatus(ref space, componentType, out enabled2, out changePending2);
			enabled = enabled2 == Bool.True;
			changePending = changePending2 == Bool.True;
			return result;
		}
		return Result.Failure_Unsupported;
	}

	[Obsolete("Use the overload of EnumerateSpaceSupportedComponents that accepts a pointer rather than a managed array.")]
	public static bool EnumerateSpaceSupportedComponents(ulong space, out uint numSupportedComponents, SpaceComponentType[] supportedComponents)
	{
		numSupportedComponents = 0u;
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_EnumerateSpaceSupportedComponents(ref space, (uint)supportedComponents.Length, out numSupportedComponents, supportedComponents) == Result.Success;
		}
		return false;
	}

	public unsafe static Result EnumerateSpaceSupportedComponents(ulong space, uint capacityInput, out uint countOutput, SpaceComponentType* buffer)
	{
		countOutput = 0u;
		if (!(version < OVRP_1_72_0.version))
		{
			return OVRP_1_72_0.ovrp_EnumerateSpaceSupportedComponents(ref space, capacityInput, out countOutput, buffer);
		}
		return Result.Failure_Unsupported;
	}

	public static bool SaveSpace(ulong space, SpaceStorageLocation location, SpaceStoragePersistenceMode mode, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_SaveSpace(ref space, location, mode, out requestId) == Result.Success;
		}
		return false;
	}

	public static bool EraseSpace(ulong space, SpaceStorageLocation location, out ulong requestId)
	{
		return EraseSpaceWithResult(space, location, out requestId).IsSuccess();
	}

	public static Result EraseSpaceWithResult(ulong space, SpaceStorageLocation location, out ulong requestId)
	{
		requestId = 0uL;
		if (!(version >= OVRP_1_72_0.version))
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_72_0.ovrp_EraseSpace(ref space, location, out requestId);
	}

	public static bool GetSpaceUuid(ulong space, out Guid uuid)
	{
		uuid = default(Guid);
		if (version >= OVRP_1_74_0.version)
		{
			return OVRP_1_74_0.ovrp_GetSpaceUuid(in space, out uuid) == Result.Success;
		}
		return false;
	}

	public static bool QuerySpaces(SpaceQueryInfo queryInfo, out ulong requestId)
	{
		return QuerySpacesWithResult(queryInfo, out requestId).IsSuccess();
	}

	public static Result QuerySpacesWithResult(SpaceQueryInfo queryInfo, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_72_0.version)
		{
			if (queryInfo.FilterType == SpaceQueryFilterType.Ids)
			{
				Guid[] ids = queryInfo.IdInfo.Ids;
				if (ids != null && ids.Length > 1024)
				{
					Debug.LogError("QuerySpaces attempted to query more uuids than the maximum number supported: " + 1024);
					return Result.Failure_InvalidParameter;
				}
			}
			else if (queryInfo.FilterType == SpaceQueryFilterType.Components)
			{
				SpaceComponentType[] components = queryInfo.ComponentsInfo.Components;
				if (components != null && components.Length > 16)
				{
					Debug.LogError("QuerySpaces attempted to query more components than the maximum number supported: " + 16);
					return Result.Failure_InvalidParameter;
				}
			}
			Guid[] ids2 = queryInfo.IdInfo.Ids;
			if (ids2 == null || ids2.Length != 1024)
			{
				Array.Resize(ref queryInfo.IdInfo.Ids, 1024);
			}
			SpaceComponentType[] components2 = queryInfo.ComponentsInfo.Components;
			if (components2 == null || components2.Length != 16)
			{
				Array.Resize(ref queryInfo.ComponentsInfo.Components, 16);
			}
			return OVRP_1_72_0.ovrp_QuerySpaces(ref queryInfo, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result QuerySpaces2(SpaceQueryInfo2 queryInfo, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			if (queryInfo.FilterType == SpaceQueryFilterType.Ids)
			{
				Guid[] ids = queryInfo.IdInfo.Ids;
				if (ids != null && ids.Length > 1024)
				{
					Debug.LogError("QuerySpaces attempted to query more uuids than the maximum number supported: " + 1024);
					return Result.Failure_InvalidParameter;
				}
			}
			else if (queryInfo.FilterType == SpaceQueryFilterType.Components)
			{
				SpaceComponentType[] components = queryInfo.ComponentsInfo.Components;
				if (components != null && components.Length > 16)
				{
					Debug.LogError("QuerySpaces attempted to query more components than the maximum number supported: " + 16);
					return Result.Failure_InvalidParameter;
				}
			}
			Guid[] ids2 = queryInfo.IdInfo.Ids;
			if (ids2 == null || ids2.Length != 1024)
			{
				Array.Resize(ref queryInfo.IdInfo.Ids, 1024);
			}
			SpaceComponentType[] components2 = queryInfo.ComponentsInfo.Components;
			if (components2 == null || components2.Length != 16)
			{
				Array.Resize(ref queryInfo.ComponentsInfo.Components, 16);
			}
			return OVRP_1_103_0.ovrp_QuerySpaces2(ref queryInfo, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public unsafe static bool RetrieveSpaceQueryResults(ulong requestId, out NativeArray<SpaceQueryResult> results, Allocator allocator)
	{
		results = default(NativeArray<SpaceQueryResult>);
		if (version < OVRP_1_72_0.version)
		{
			return false;
		}
		uint resultCountOutput = 0u;
		if (OVRP_1_72_0.ovrp_RetrieveSpaceQueryResults(ref requestId, 0u, ref resultCountOutput, (IntPtr)0) != 0)
		{
			return false;
		}
		results = new NativeArray<SpaceQueryResult>((int)resultCountOutput, allocator);
		if (OVRP_1_72_0.ovrp_RetrieveSpaceQueryResults(ref requestId, (uint)results.Length, ref resultCountOutput, new IntPtr(results.GetUnsafePtr())) != 0)
		{
			results.Dispose();
			return false;
		}
		return true;
	}

	public static bool RetrieveSpaceQueryResults(ulong requestId, out SpaceQueryResult[] results)
	{
		results = null;
		if (version >= OVRP_1_72_0.version)
		{
			IntPtr results2 = new IntPtr(0);
			uint resultCountOutput = 0u;
			if (OVRP_1_72_0.ovrp_RetrieveSpaceQueryResults(ref requestId, 0u, ref resultCountOutput, results2) != 0)
			{
				return false;
			}
			int num = Marshal.SizeOf(typeof(SpaceQueryResult));
			IntPtr intPtr = Marshal.AllocHGlobal((int)resultCountOutput * num);
			if (OVRP_1_72_0.ovrp_RetrieveSpaceQueryResults(ref requestId, resultCountOutput, ref resultCountOutput, intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return false;
			}
			results = new SpaceQueryResult[resultCountOutput];
			for (int i = 0; i < resultCountOutput; i++)
			{
				SpaceQueryResult spaceQueryResult = (SpaceQueryResult)Marshal.PtrToStructure(intPtr + i * num, typeof(SpaceQueryResult));
				results[i] = spaceQueryResult;
			}
			Marshal.FreeHGlobal(intPtr);
			return true;
		}
		return false;
	}

	public unsafe static Result SaveSpaceList(NativeArray<ulong> spaces, SpaceStorageLocation location, out ulong requestId)
	{
		return SaveSpaceList((ulong*)spaces.GetUnsafeReadOnlyPtr(), (uint)spaces.Length, location, out requestId);
	}

	public unsafe static Result SaveSpaceList(ulong* spaces, uint numSpaces, SpaceStorageLocation location, out ulong requestId)
	{
		requestId = 0uL;
		if (!(version >= OVRP_1_79_0.version))
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_79_0.ovrp_SaveSpaceList(spaces, numSpaces, location, out requestId);
	}

	public static bool GetSpaceUserId(ulong spaceUserHandle, out ulong spaceUserId)
	{
		spaceUserId = 0uL;
		if (version >= OVRP_1_79_0.version)
		{
			return OVRP_1_79_0.ovrp_GetSpaceUserId(in spaceUserHandle, out spaceUserId) == Result.Success;
		}
		return false;
	}

	public static bool CreateSpaceUser(ulong spaceUserId, out ulong spaceUserHandle)
	{
		spaceUserHandle = 0uL;
		if (version >= OVRP_1_79_0.version)
		{
			return OVRP_1_79_0.ovrp_CreateSpaceUser(in spaceUserId, out spaceUserHandle) == Result.Success;
		}
		return false;
	}

	public static bool DestroySpaceUser(ulong spaceUserHandle)
	{
		if (version >= OVRP_1_79_0.version)
		{
			return OVRP_1_79_0.ovrp_DestroySpaceUser(in spaceUserHandle) == Result.Success;
		}
		return false;
	}

	public unsafe static Result ShareSpaces(NativeArray<ulong> spaces, NativeArray<ulong> userHandles, out ulong requestId)
	{
		return ShareSpaces((ulong*)spaces.GetUnsafeReadOnlyPtr(), (uint)spaces.Length, (ulong*)userHandles.GetUnsafeReadOnlyPtr(), (uint)userHandles.Length, out requestId);
	}

	public unsafe static Result ShareSpaces(ulong* spaces, uint numSpaces, ulong* userHandles, uint numUsers, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_79_0.version)
		{
			return OVRP_1_79_0.ovrp_ShareSpaces(spaces, numSpaces, userHandles, numUsers, out requestId);
		}
		return Result.Failure_Unsupported;
	}

	public static bool TryLocateSpace(ulong space, TrackingOrigin baseOrigin, out Posef pose)
	{
		using (new OVRProfilerScope("TryLocateSpace"))
		{
			pose = Posef.identity;
			return version >= OVRP_1_64_0.version && OVRP_1_64_0.ovrp_LocateSpace(ref pose, ref space, baseOrigin) == Result.Success;
		}
	}

	[Obsolete("LocateSpace unconditionally returns a pose, even if the underlying OpenXR function fails. Instead, use TryLocateSpace, which indicates failure.")]
	public static Posef LocateSpace(ulong space, TrackingOrigin baseOrigin)
	{
		if (!TryLocateSpace(space, baseOrigin, out var pose))
		{
			return Posef.identity;
		}
		return pose;
	}

	public static bool TryLocateSpace(ulong space, TrackingOrigin baseOrigin, out Posef pose, out SpaceLocationFlags locationFlags)
	{
		pose = Posef.identity;
		locationFlags = (SpaceLocationFlags)0uL;
		if (version >= OVRP_1_79_0.version && OVRP_1_79_0.ovrp_LocateSpace2(out var location, in space, baseOrigin) == Result.Success)
		{
			pose = location.pose;
			locationFlags = location.locationFlags;
			return true;
		}
		return false;
	}

	public static bool DestroySpace(ulong space)
	{
		if (version >= OVRP_1_65_0.version)
		{
			return OVRP_1_65_0.ovrp_DestroySpace(ref space) == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceContainer(ulong space, out Guid[] containerUuids)
	{
		containerUuids = Array.Empty<Guid>();
		if (version < OVRP_1_72_0.version)
		{
			return false;
		}
		SpaceContainerInternal containerInternal = default(SpaceContainerInternal);
		if (OVRP_1_72_0.ovrp_GetSpaceContainer(ref space, ref containerInternal) != 0)
		{
			return false;
		}
		Guid[] array = new Guid[containerInternal.uuidCountOutput];
		using (PinnedArray<Guid> pinnedArray = new PinnedArray<Guid>(array))
		{
			containerInternal.uuidCapacityInput = containerInternal.uuidCountOutput;
			containerInternal.uuids = pinnedArray;
			if (OVRP_1_72_0.ovrp_GetSpaceContainer(ref space, ref containerInternal) != 0)
			{
				return false;
			}
		}
		containerUuids = array;
		return true;
	}

	public static bool GetSpaceBoundingBox2D(ulong space, out Rectf rect)
	{
		rect = default(Rectf);
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_GetSpaceBoundingBox2D(ref space, out rect) == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceBoundingBox3D(ulong space, out Boundsf bounds)
	{
		bounds = default(Boundsf);
		if (version >= OVRP_1_72_0.version)
		{
			return OVRP_1_72_0.ovrp_GetSpaceBoundingBox3D(ref space, out bounds) == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceSemanticLabels(ulong space, out string labels)
	{
		char[] buffer = null;
		if (GetSpaceSemanticLabelsNonAlloc(space, ref buffer, out var length))
		{
			labels = new string(buffer, 0, length);
			return true;
		}
		labels = null;
		return false;
	}

	internal unsafe static bool GetSpaceSemanticLabelsNonAlloc(ulong space, ref char[] buffer, out int length)
	{
		length = -1;
		if (version >= OVRP_1_72_0.version)
		{
			SpaceSemanticLabelInternal spaceSemanticLabelInternal = default(SpaceSemanticLabelInternal);
			spaceSemanticLabelInternal.byteCapacityInput = 0;
			spaceSemanticLabelInternal.byteCountOutput = 0;
			SpaceSemanticLabelInternal labelsInternal = spaceSemanticLabelInternal;
			Result result = OVRP_1_72_0.ovrp_GetSpaceSemanticLabels(ref space, ref labelsInternal);
			if (result == Result.Success)
			{
				labelsInternal.byteCapacityInput = labelsInternal.byteCountOutput;
				length = labelsInternal.byteCountOutput;
				labelsInternal.labels = Marshal.AllocHGlobal(length);
				result = OVRP_1_72_0.ovrp_GetSpaceSemanticLabels(ref space, ref labelsInternal);
				if (buffer == null)
				{
					buffer = new char[length];
				}
				else if (buffer.Length < length)
				{
					buffer = new char[Math.Max(buffer.Length * 2, length)];
				}
				byte* ptr = (byte*)(void*)labelsInternal.labels;
				for (int i = 0; i < length; i++)
				{
					buffer[i] = (char)ptr[i];
				}
				Marshal.FreeHGlobal(labelsInternal.labels);
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceRoomLayout(ulong space, out RoomLayout roomLayout)
	{
		roomLayout = default(RoomLayout);
		if (version < OVRP_1_72_0.version)
		{
			return false;
		}
		RoomLayoutInternal roomLayoutInternal = default(RoomLayoutInternal);
		if (OVRP_1_72_0.ovrp_GetSpaceRoomLayout(ref space, ref roomLayoutInternal) != 0)
		{
			return false;
		}
		Guid[] array = new Guid[roomLayoutInternal.wallUuidCountOutput];
		using (PinnedArray<Guid> pinnedArray = new PinnedArray<Guid>(array))
		{
			roomLayoutInternal.wallUuidCapacityInput = roomLayoutInternal.wallUuidCountOutput;
			roomLayoutInternal.wallUuids = pinnedArray;
			if (OVRP_1_72_0.ovrp_GetSpaceRoomLayout(ref space, ref roomLayoutInternal) != 0)
			{
				return false;
			}
		}
		roomLayout.ceilingUuid = roomLayoutInternal.ceilingUuid;
		roomLayout.floorUuid = roomLayoutInternal.floorUuid;
		roomLayout.wallUuids = array;
		return true;
	}

	public static bool GetSpaceBoundary2DCount(ulong space, out int count)
	{
		count = 0;
		if (version < OVRP_1_72_0.version)
		{
			return false;
		}
		PolygonalBoundary2DInternal boundaryInternal = default(PolygonalBoundary2DInternal);
		Result num = OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal);
		count = boundaryInternal.vertexCountOutput;
		return num == Result.Success;
	}

	public static bool GetSpaceBoundary2D(ulong space, NativeArray<Vector2> boundary)
	{
		int count;
		return GetSpaceBoundary2D(space, boundary, out count);
	}

	public unsafe static bool GetSpaceBoundary2D(ulong space, NativeArray<Vector2> boundary, out int count)
	{
		count = 0;
		if (version < OVRP_1_72_0.version)
		{
			return false;
		}
		PolygonalBoundary2DInternal polygonalBoundary2DInternal = default(PolygonalBoundary2DInternal);
		polygonalBoundary2DInternal.vertexCapacityInput = boundary.Length;
		polygonalBoundary2DInternal.vertices = new IntPtr(boundary.GetUnsafePtr());
		PolygonalBoundary2DInternal boundaryInternal = polygonalBoundary2DInternal;
		bool result = OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal) == Result.Success;
		count = boundaryInternal.vertexCountOutput;
		return result;
	}

	public unsafe static NativeArray<Vector2> GetSpaceBoundary2D(ulong space, Allocator allocator)
	{
		if (version < OVRP_1_72_0.version)
		{
			return default(NativeArray<Vector2>);
		}
		PolygonalBoundary2DInternal polygonalBoundary2DInternal = default(PolygonalBoundary2DInternal);
		polygonalBoundary2DInternal.vertexCapacityInput = 0;
		polygonalBoundary2DInternal.vertexCountOutput = 0;
		PolygonalBoundary2DInternal boundaryInternal = polygonalBoundary2DInternal;
		if (OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal) != 0)
		{
			return default(NativeArray<Vector2>);
		}
		NativeArray<Vector2> nativeArray = new NativeArray<Vector2>(boundaryInternal.vertexCountOutput, allocator);
		boundaryInternal.vertices = new IntPtr(nativeArray.GetUnsafePtr());
		boundaryInternal.vertexCapacityInput = nativeArray.Length;
		if (OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal) == Result.Success)
		{
			return nativeArray;
		}
		nativeArray.Dispose();
		return default(NativeArray<Vector2>);
	}

	[Obsolete("This method allocates managed arrays. Use GetSpaceBoundary2D(UInt64, Allocator) to avoid managed allocations.")]
	public static bool GetSpaceBoundary2D(ulong space, out Vector2[] boundary)
	{
		boundary = Array.Empty<Vector2>();
		if (version >= OVRP_1_72_0.version)
		{
			PolygonalBoundary2DInternal polygonalBoundary2DInternal = default(PolygonalBoundary2DInternal);
			polygonalBoundary2DInternal.vertexCapacityInput = 0;
			polygonalBoundary2DInternal.vertexCountOutput = 0;
			PolygonalBoundary2DInternal boundaryInternal = polygonalBoundary2DInternal;
			Result result = OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal);
			if (result == Result.Success)
			{
				boundaryInternal.vertexCapacityInput = boundaryInternal.vertexCountOutput;
				int num = Marshal.SizeOf(typeof(Vector2));
				boundaryInternal.vertices = Marshal.AllocHGlobal(boundaryInternal.vertexCountOutput * num);
				result = OVRP_1_72_0.ovrp_GetSpaceBoundary2D(ref space, ref boundaryInternal);
				if (result == Result.Success)
				{
					boundary = new Vector2[boundaryInternal.vertexCountOutput];
					IntPtr vertices = boundaryInternal.vertices;
					for (int i = 0; i < boundaryInternal.vertexCountOutput; i++)
					{
						IntPtr intPtr = new IntPtr(num);
						intPtr = vertices;
						vertices += num;
						boundary[i] = Marshal.PtrToStructure<Vector2>(intPtr);
					}
					Marshal.FreeHGlobal(boundaryInternal.vertices);
				}
			}
			return result == Result.Success;
		}
		return false;
	}

	public static bool RequestSceneCapture(out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_72_0.version)
		{
			SceneCaptureRequestInternal sceneCaptureRequestInternal = default(SceneCaptureRequestInternal);
			sceneCaptureRequestInternal.requestByteCount = 0;
			SceneCaptureRequestInternal request = sceneCaptureRequestInternal;
			return OVRP_1_72_0.ovrp_RequestSceneCapture(ref request, out requestId) == Result.Success;
		}
		return false;
	}

	public static bool GetSpaceTriangleMeshCounts(ulong space, out int vertexCount, out int triangleCount)
	{
		vertexCount = 0;
		triangleCount = 0;
		if (version < OVRP_1_82_0.version)
		{
			return false;
		}
		using (new OVRProfilerScope("GetSpaceTriangleMeshCounts"))
		{
			TriangleMeshInternal triangleMeshInternal = default(TriangleMeshInternal);
			Result num = OVRP_1_82_0.ovrp_GetSpaceTriangleMesh(ref space, ref triangleMeshInternal);
			vertexCount = triangleMeshInternal.vertexCountOutput;
			triangleCount = triangleMeshInternal.indexCountOutput / 3;
			return num == Result.Success;
		}
	}

	public unsafe static bool GetSpaceTriangleMesh(ulong space, NativeArray<Vector3> vertices, NativeArray<int> triangles)
	{
		if (version < OVRP_1_82_0.version)
		{
			return false;
		}
		using (new OVRProfilerScope("GetSpaceTriangleMesh"))
		{
			TriangleMeshInternal triangleMeshInternal = default(TriangleMeshInternal);
			triangleMeshInternal.vertices = new IntPtr(vertices.GetUnsafePtr());
			triangleMeshInternal.vertexCapacityInput = vertices.Length;
			triangleMeshInternal.indices = new IntPtr(triangles.GetUnsafePtr());
			triangleMeshInternal.indexCapacityInput = triangles.Length;
			TriangleMeshInternal triangleMeshInternal2 = triangleMeshInternal;
			return OVRP_1_82_0.ovrp_GetSpaceTriangleMesh(ref space, ref triangleMeshInternal2) == Result.Success;
		}
	}

	public static bool GetLayerRecommendedResolution(int layerId, out Sizei recommendedSize)
	{
		recommendedSize = default(Sizei);
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_GetLayerRecommendedResolution(layerId, out recommendedSize) == Result.Success;
		}
		return false;
	}

	public static bool GetEyeLayerRecommendedResolution(out Sizei recommendedSize)
	{
		recommendedSize = default(Sizei);
		if (version >= OVRP_1_84_0.version)
		{
			return OVRP_1_84_0.ovrp_GetEyeLayerRecommendedResolution(out recommendedSize) == Result.Success;
		}
		return false;
	}

	public static string[] GetRenderModelPaths()
	{
		if (version >= OVRP_1_68_0.version)
		{
			uint num = 0u;
			List<string> list = new List<string>();
			IntPtr intPtr;
			for (intPtr = Marshal.AllocHGlobal(256); OVRP_1_68_0.ovrp_GetRenderModelPaths(num, intPtr) == Result.Success; num++)
			{
				list.Add(Marshal.PtrToStringAnsi(intPtr));
			}
			Marshal.FreeHGlobal(intPtr);
			return list.ToArray();
		}
		return null;
	}

	public static bool GetRenderModelProperties(string modelPath, ref RenderModelProperties modelProperties)
	{
		Result result;
		RenderModelPropertiesInternal properties;
		if (version >= OVRP_1_74_0.version)
		{
			result = OVRP_1_74_0.ovrp_GetRenderModelProperties2(modelPath, RenderModelFlags.SupportsGltf20Subset2, out properties);
		}
		else
		{
			if (!(version >= OVRP_1_68_0.version))
			{
				return false;
			}
			result = OVRP_1_68_0.ovrp_GetRenderModelProperties(modelPath, out properties);
		}
		if (result != 0)
		{
			return false;
		}
		modelProperties.ModelName = Encoding.Default.GetString(properties.ModelName);
		modelProperties.ModelKey = properties.ModelKey;
		modelProperties.VendorId = properties.VendorId;
		modelProperties.ModelVersion = properties.ModelVersion;
		return true;
	}

	public static byte[] LoadRenderModel(ulong modelKey)
	{
		if (version >= OVRP_1_68_0.version)
		{
			uint bufferCountOutput = 0u;
			if (OVRP_1_68_0.ovrp_LoadRenderModel(modelKey, 0u, ref bufferCountOutput, IntPtr.Zero) != 0)
			{
				return null;
			}
			if (bufferCountOutput == 0)
			{
				return null;
			}
			IntPtr intPtr = Marshal.AllocHGlobal((int)bufferCountOutput);
			if (OVRP_1_68_0.ovrp_LoadRenderModel(modelKey, bufferCountOutput, ref bufferCountOutput, intPtr) != 0)
			{
				Marshal.FreeHGlobal(intPtr);
				return null;
			}
			byte[] array = new byte[bufferCountOutput];
			Marshal.Copy(intPtr, array, 0, (int)bufferCountOutput);
			Marshal.FreeHGlobal(intPtr);
			return array;
		}
		return null;
	}

	public static Result StartColocationSessionAdvertisement(ColocationSessionStartAdvertisementInfo info, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			return OVRP_1_103_0.ovrp_StartColocationAdvertisement(in info, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result StopColocationSessionAdvertisement(out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			return OVRP_1_103_0.ovrp_StopColocationAdvertisement(out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result StartColocationSessionDiscovery(out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			return OVRP_1_103_0.ovrp_StartColocationDiscovery(out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result StopColocationSessionDiscovery(out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			return OVRP_1_103_0.ovrp_StopColocationDiscovery(out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result ShareSpaces(in ShareSpacesInfo info, out ulong requestId)
	{
		requestId = 0uL;
		if (version >= OVRP_1_103_0.version)
		{
			return OVRP_1_103_0.ovrp_ShareSpaces2(in info, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result DiscoverSpaces(in SpaceDiscoveryInfo info, out ulong requestId)
	{
		requestId = 0uL;
		if (version < OVRP_1_97_0.version)
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_97_0.ovrp_DiscoverSpaces(in info, out requestId);
	}

	public unsafe static Result RetrieveSpaceDiscoveryResults(ulong requestId, SpaceDiscoveryResult* results, int capacityInput, out int countOutput)
	{
		countOutput = 0;
		if (version < OVRP_1_97_0.version)
		{
			return Result.Failure_NotYetImplemented;
		}
		SpaceDiscoveryResults spaceDiscoveryResults = default(SpaceDiscoveryResults);
		spaceDiscoveryResults.ResultCapacityInput = (uint)capacityInput;
		spaceDiscoveryResults.Results = results;
		SpaceDiscoveryResults results2 = spaceDiscoveryResults;
		Result result = OVRP_1_97_0.ovrp_RetrieveSpaceDiscoveryResults(requestId, ref results2);
		countOutput = (int)results2.ResultCountOutput;
		return result;
	}

	public unsafe static Result SaveSpaces(ulong* spaces, int count, out ulong requestId)
	{
		requestId = 0uL;
		if (!(version < OVRP_1_97_0.version))
		{
			return OVRP_1_97_0.ovrp_SaveSpaces((uint)count, spaces, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public unsafe static Result EraseSpaces(uint spaceCount, ulong* spaces, uint uuidCount, Guid* uuids, out ulong requestId)
	{
		requestId = 0uL;
		if (!(version < OVRP_1_97_0.version))
		{
			return OVRP_1_97_0.ovrp_EraseSpaces(spaceCount, spaces, uuidCount, uuids, out requestId);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result RequestBoundaryVisibility(BoundaryVisibility boundaryVisibility)
	{
		if (!(version < OVRP_1_98_0.version))
		{
			return OVRP_1_98_0.ovrp_RequestBoundaryVisibility(boundaryVisibility);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result GetBoundaryVisibility(out BoundaryVisibility boundaryVisibility)
	{
		boundaryVisibility = (BoundaryVisibility)0;
		if (!(version < OVRP_1_98_0.version))
		{
			return OVRP_1_98_0.ovrp_GetBoundaryVisibility(out boundaryVisibility);
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result CreateDynamicObjectTracker(out ulong tracker)
	{
		tracker = 0uL;
		if (!(version >= OVRP_1_104_0.version))
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_104_0.ovrp_CreateDynamicObjectTracker(out tracker);
	}

	public static OVRTask<OVRResult<ulong, Result>> CreateDynamicObjectTrackerAsync()
	{
		ulong tracker;
		return OVRTask.Build(CreateDynamicObjectTracker(out tracker), tracker, EventType.CreateDynamicObjectTrackerResult).ToTask<ulong, Result>();
	}

	public static Result DestroyDynamicObjectTracker(ulong tracker)
	{
		if (!(version >= OVRP_1_104_0.version))
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_104_0.ovrp_DestroyDynamicObjectTracker(tracker);
	}

	public unsafe static Result SetDynamicObjectTrackedClasses(ulong tracker, ReadOnlySpan<DynamicObjectClass> classes)
	{
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_Unsupported;
		}
		fixed (DynamicObjectClass* classes2 = classes)
		{
			DynamicObjectTrackedClassesSetInfo setInfo = new DynamicObjectTrackedClassesSetInfo
			{
				Classes = classes2,
				ClassCount = (uint)classes.Length
			};
			return OVRP_1_104_0.ovrp_SetDynamicObjectTrackedClasses(tracker, in setInfo);
		}
	}

	public static OVRTask<OVRResult<Result>> SetDynamicObjectTrackedClassesAsync(ulong tracker, ReadOnlySpan<DynamicObjectClass> classes)
	{
		return OVRTask.Build(SetDynamicObjectTrackedClasses(tracker, classes), tracker, EventType.SetDynamicObjectTrackedClassesResult).ToResultTask<Result>();
	}

	public static Result GetSpaceDynamicObjectData(ulong space, out DynamicObjectData data)
	{
		data = default(DynamicObjectData);
		if (!(version >= OVRP_1_104_0.version))
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_104_0.ovrp_GetSpaceDynamicObjectData(ref space, out data);
	}

	public static Result GetDynamicObjectTrackerSupported(out bool value)
	{
		value = false;
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_Unsupported;
		}
		Bool value2;
		Result result = OVRP_1_104_0.ovrp_GetDynamicObjectTrackerSupported(out value2);
		value = value2 == Bool.True;
		return result;
	}

	public static Result GetDynamicObjectKeyboardSupported(out bool value)
	{
		value = false;
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_Unsupported;
		}
		Bool value2;
		Result result = OVRP_1_104_0.ovrp_GetDynamicObjectKeyboardSupported(out value2);
		value = value2 == Bool.True;
		return result;
	}

	public static void OnEditorShutdown()
	{
		if (!(version < OVRP_1_85_0.version))
		{
			OVRP_1_85_0.ovrp_OnEditorShutdown();
		}
	}

	internal static Result GetPassthroughPreferences(out PassthroughPreferences preferences)
	{
		preferences = default(PassthroughPreferences);
		if (version < OVRP_1_87_0.version)
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_87_0.ovrp_GetPassthroughPreferences(out preferences);
	}

	public static bool SetEyeBufferSharpenType(LayerSharpenType sharpenType)
	{
		if (version >= OVRP_1_87_0.version)
		{
			return OVRP_1_87_0.ovrp_SetEyeBufferSharpenType(sharpenType) == Result.Success;
		}
		return false;
	}

	public unsafe static Result CreateMarkerTrackerAsync(ReadOnlySpan<MarkerType> markerTypes, out ulong future)
	{
		future = 0uL;
		fixed (MarkerType* markerTypes2 = markerTypes)
		{
			if (!(version >= OVRP_1_110_0.version))
			{
				return Result.Failure_NotYetImplemented;
			}
			MarkerTrackerCreateInfo createInfo = default(MarkerTrackerCreateInfo);
			createInfo.MarkerTypes = markerTypes2;
			createInfo.MarkerTypeCount = (uint)markerTypes.Length;
			return OVRP_1_110_0.ovrp_CreateMarkerTrackerAsync(in createInfo, out future);
		}
	}

	public static Result CreateMarkerTrackerComplete(ulong future, out MarkerTrackerCreateCompletion completion)
	{
		completion = default(MarkerTrackerCreateCompletion);
		if (!(version >= OVRP_1_110_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_110_0.ovrp_CreateMarkerTrackerComplete(future, out completion);
	}

	public static Result DestroyMarkerTracker(ulong markerTracker)
	{
		if (!(version >= OVRP_1_110_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_110_0.ovrp_DestroyMarkerTracker(markerTracker);
	}

	public static Result GetSpaceMarkerPayload(ulong space, ref SpaceMarkerPayload payload)
	{
		if (!(version >= OVRP_1_110_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_110_0.ovrp_GetSpaceMarkerPayload(space, ref payload);
	}

	public static Result GetMarkerTrackingSupported(out bool markerTrackingSupported)
	{
		markerTrackingSupported = false;
		if (version >= OVRP_1_110_0.version)
		{
			Bool value;
			Result result = OVRP_1_110_0.ovrp_GetMarkerTrackingSupported(out value);
			markerTrackingSupported = value == Bool.True;
			return result;
		}
		return Result.Failure_NotYetImplemented;
	}

	public static Result PollFuture(ulong future, out FutureState state)
	{
		state = (FutureState)0;
		if (!(version >= OVRP_1_103_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_103_0.ovrp_PollFuture(future, out state);
	}

	public static Result CancelFuture(ulong future)
	{
		if (!(version >= OVRP_1_103_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_103_0.ovrp_CancelFuture(future);
	}

	public static Result SetExternalLayerDynresEnabled(Bool enabled)
	{
		if (version < OVRP_1_104_0.version)
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_104_0.ovrp_SetExternalLayerDynresEnabled(enabled);
	}

	public static Result SetDeveloperTelemetryConsent(Bool consent)
	{
		if (version < OVRP_1_95_0.version)
		{
			return Result.Failure_Unsupported;
		}
		return OVRP_1_95_0.ovrp_SetDeveloperTelemetryConsent(consent);
	}

	public static bool BeginProfilingRegion(string regionName)
	{
		return OVRP_1_104_0.ovrp_BeginProfilingRegion(regionName) == Result.Success;
	}

	public static bool EndProfilingRegion()
	{
		return OVRP_1_104_0.ovrp_EndProfilingRegion() == Result.Success;
	}

	public static Result SendMicrogestureHint()
	{
		if (version < OVRP_1_106_0.version)
		{
			return Result.Failure_Unsupported;
		}
		OVRP_1_106_0.ovrp_SendMicrogestureHint();
		return Result.Success;
	}

	public static Result GetStationaryReferenceSpaceId(out Guid generationId)
	{
		generationId = default(Guid);
		if (!(version >= OVRP_1_109_0.version))
		{
			return Result.Failure_NotYetImplemented;
		}
		return OVRP_1_109_0.ovrp_GetStationaryReferenceSpaceId(out generationId);
	}
}
