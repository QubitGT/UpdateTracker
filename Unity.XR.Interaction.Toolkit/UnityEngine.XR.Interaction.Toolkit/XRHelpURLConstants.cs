using System;

namespace UnityEngine.XR.Interaction.Toolkit;

internal static class XRHelpURLConstants
{
	private const string k_CurrentDocsVersion = "3.2";

	private const string k_BaseApi = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/";

	private const string k_HtmlFileSuffix = ".html";

	private const string k_BaseNamespace = "UnityEngine.XR.Interaction.Toolkit.";

	private const string k_ARNamespace = "AR.";

	private const string k_AttachmentNamespace = "Attachment.";

	private const string k_BodyUINamespace = "BodyUI.";

	private const string k_CastersNamespace = "Casters.";

	private const string k_FeedbackNamespace = "Feedback.";

	private const string k_FilteringNamespace = "Filtering.";

	private const string k_GazeNamespace = "Gaze.";

	private const string k_HapticsNamespace = "Haptics.";

	private const string k_InputsNamespace = "Inputs.";

	private const string k_InteractorsNamespace = "Interactors.";

	private const string k_InteractablesNamespace = "Interactables.";

	private const string k_LocomotionNamespace = "Locomotion.";

	private const string k_ReadersNamespace = "Readers.";

	private const string k_SimulationNamespace = "Simulation.";

	private const string k_TransformersNamespace = "Transformers.";

	private const string k_UINamespace = "UI.";

	private const string k_UtilitiesNamespace = "Utilities.";

	private const string k_VisualsNamespace = "Visuals.";

	private const string k_JumpNamespace = "Jump.";

	private const string k_GravityNamespace = "Gravity.";

	private const string k_ClimbingNamespace = "Climbing.";

	private const string k_MovementNamespace = "Movement.";

	private const string k_TeleportationNamespace = "Teleportation.";

	private const string k_TurningNamespace = "Turning.";

	private const string k_ComfortNamespace = "Comfort.";

	public const string k_ScreenSpacePinchScaleInput = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpacePinchScaleInput.html";

	public const string k_ScreenSpaceRayPoseDriver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceRayPoseDriver.html";

	public const string k_ScreenSpaceRotateInput = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceRotateInput.html";

	public const string k_ScreenSpaceSelectInput = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceSelectInput.html";

	public const string k_TouchscreenGestureInputLoader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.TouchscreenGestureInputLoader.html";

	public const string k_InteractionAttachController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Attachment.InteractionAttachController.html";

	public const string k_ClimbInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbInteractable.html";

	public const string k_ClimbProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbProvider.html";

	public const string k_ClimbSettingsDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbSettingsDatum.html";

	public const string k_ClimbTeleportInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing.ClimbTeleportInteractor.html";

	public const string k_ContinuousMoveProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider.html";

	public const string k_ContinuousTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.ContinuousTurnProvider.html";

	public const string k_SimpleAudioFeedback = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Feedback.SimpleAudioFeedback.html";

	public const string k_SimpleHapticFeedback = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Feedback.SimpleHapticFeedback.html";

	public const string k_PokeThresholdDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.PokeThresholdDatum.html";

	public const string k_TouchscreenHoverFilter = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.TouchscreenHoverFilter.html";

	public const string k_XRPokeFilter = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.XRPokeFilter.html";

	public const string k_XRTargetFilter = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Filtering.XRTargetFilter.html";

	public const string k_FurthestTeleportationAnchorFilter = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.FurthestTeleportationAnchorFilter.html";

	public const string k_GazeTeleportationAnchorFilter = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.GazeTeleportationAnchorFilter.html";

	public const string k_GrabMoveProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.GrabMoveProvider.html";

	public const string k_HapticImpulsePlayer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer.html";

	public const string k_XRInputDeviceHapticImpulseProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.XRInputDeviceHapticImpulseProvider.html";

	public const string k_InputActionManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager.html";

	public const string k_XRInputDeviceBoolValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceBoolValueReader.html";

	public const string k_XRInputDeviceButtonReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceButtonReader.html";

	public const string k_XRInputDeviceFloatValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceFloatValueReader.html";

	public const string k_XRInputDeviceInputTrackingStateValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceInputTrackingStateValueReader.html";

	public const string k_XRInputDeviceQuaternionValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceQuaternionValueReader.html";

	public const string k_XRInputDeviceVector2ValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceVector2ValueReader.html";

	public const string k_XRInputDeviceVector3ValueReader = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceVector3ValueReader.html";

	public const string k_SimulatedDeviceLifecycleManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.SimulatedDeviceLifecycleManager.html";

	public const string k_SimulatedHandExpressionManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.SimulatedHandExpressionManager.html";

	public const string k_XRDeviceSimulator = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator.html";

	public const string k_XRInteractionSimulator = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator.html";

	public const string k_XRHandSkeletonPokeDisplacer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRHandSkeletonPokeDisplacer.html";

	public const string k_XRInputModalityManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager.html";

	public const string k_XRTransformStabilizer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRTransformStabilizer.html";

	public const string k_CurveInteractionCaster = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Casters.CurveInteractionCaster.html";

	public const string k_SphereInteractionCaster = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Casters.SphereInteractionCaster.html";

	public const string k_NearFarInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor.html";

	public const string k_CurveVisualController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.CurveVisualController.html";

	public const string k_CharacterControllerBodyManipulator = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.CharacterControllerBodyManipulator.html";

	public const string k_GravityProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity.GravityProvider.html";

	public const string k_JumpProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump.JumpProvider.html";

	public const string k_LocomotionMediator = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionMediator.html";

	public const string k_SnapTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning.SnapTurnProvider.html";

	public const string k_UnderCameraBodyPositionEvaluator = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.UnderCameraBodyPositionEvaluator.html";

	public const string k_XRBodyTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.XRBodyTransformer.html";

	public const string k_TeleportationAnchor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationAnchor.html";

	public const string k_TeleportationArea = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea.html";

	public const string k_TeleportationMultiAnchorVolume = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationMultiAnchorVolume.html";

	public const string k_TeleportationProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider.html";

	public const string k_TeleportVolumeDestinationSettingsDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportVolumeDestinationSettingsDatum.html";

	public const string k_XRDualGrabFreeTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRDualGrabFreeTransformer.html";

	public const string k_XRGeneralGrabTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRGeneralGrabTransformer.html";

	public const string k_XRSingleGrabFreeTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRSingleGrabFreeTransformer.html";

	public const string k_XRSingleGrabOffsetPreserveTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.k_XRSingleGrabOffsetPreserveTransformer.html";

	public const string k_XRSocketGrabTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRSocketGrabTransformer.html";

	public const string k_TunnelingVignetteController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController.html";

	public const string k_TwoHandedGrabMoveProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.TwoHandedGrabMoveProvider.html";

	public const string k_HandMenu = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.BodyUI.HandMenu.html";

	public const string k_FollowPresetDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.BodyUI.FollowPresetDatum.html";

	public const string k_CanvasOptimizer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.CanvasOptimizer.html";

	public const string k_CanvasTracker = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.CanvasTracker.html";

	public const string k_LazyFollow = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow.html";

	public const string k_TrackedDeviceGraphicRaycaster = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster.html";

	public const string k_TrackedDevicePhysicsRaycaster = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.TrackedDevicePhysicsRaycaster.html";

	public const string k_XRUIInputModule = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule.html";

	public const string k_XRUIToolkitManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.XRUIToolkitManager.html";

	public const string k_DisposableManagerSingleton = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Utilities.DisposableManagerSingleton.html";

	public const string k_XRDebugLineVisualizer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Utilities.XRDebugLineVisualizer.html";

	public const string k_XRControllerRecorder = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRControllerRecorder.html";

	public const string k_XRControllerRecording = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRControllerRecording.html";

	public const string k_XRDirectInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor.html";

	public const string k_XRGazeAssistance = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Gaze.XRGazeAssistance.html";

	public const string k_XRGazeInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRGazeInteractor.html";

	public const string k_XRGrabInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable.html";

	public const string k_XRInteractableSnapVolume = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.XRInteractableSnapVolume.html";

	public const string k_XRInteractionGroup = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup.html";

	public const string k_XRInteractionManager = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRInteractionManager.html";

	public const string k_XRInteractorLineVisual = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual.html";

	public const string k_XRInteractorReticleVisual = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorReticleVisual.html";

	public const string k_XRPokeInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor.html";

	public const string k_XRRayInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor.html";

	public const string k_XRSimpleInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable.html";

	public const string k_XRSocketInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor.html";

	public const string k_XRTintInteractableVisual = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactables.Visuals.XRTintInteractableVisual.html";

	[Obsolete("k_XRRig is now deprecated since XRRig was replaced by XROrigin. Please use documentation from com.unity.xr.core-utils instead.", true)]
	public const string k_XRRig = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html";

	[Obsolete("k_XRLegacyGrabTransformer is now deprecated since XRLegacyGrabTransformer was deprecated.", true)]
	public const string k_XRLegacyGrabTransformer = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRLegacyGrabTransformer.html";

	[Obsolete("k_LocomotionSystem has been deprecated in version 3.0.0.")]
	public const string k_LocomotionSystem = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.LocomotionSystem.html";

	[Obsolete("k_XRController has been deprecated in version 3.0.0.")]
	public const string k_XRController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRController.html";

	[Obsolete("k_XRScreenSpaceController has been deprecated in version 3.0.0.")]
	public const string k_XRScreenSpaceController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRScreenSpaceController.html";

	[Obsolete("k_DeviceBasedContinuousMoveProvider has been deprecated in version 3.0.0.")]
	public const string k_DeviceBasedContinuousMoveProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.DeviceBasedContinuousMoveProvider.html";

	[Obsolete("k_DeviceBasedContinuousTurnProvider has been deprecated in version 3.0.0.")]
	public const string k_DeviceBasedContinuousTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.DeviceBasedContinuousTurnProvider.html";

	[Obsolete("k_DeviceBasedSnapTurnProvider has been deprecated in version 3.0.0.")]
	public const string k_DeviceBasedSnapTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.DeviceBasedSnapTurnProvider.html";

	[Obsolete("k_ActionBasedContinuousMoveProvider has been deprecated in version 3.0.0.")]
	public const string k_ActionBasedContinuousMoveProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider.html";

	[Obsolete("k_ActionBasedContinuousTurnProvider has been deprecated in version 3.0.0.")]
	public const string k_ActionBasedContinuousTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousTurnProvider.html";

	[Obsolete("k_ActionBasedController has been deprecated in version 3.0.0.")]
	public const string k_ActionBasedController = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedController.html";

	[Obsolete("k_ActionBasedSnapTurnProvider has been deprecated in version 3.0.0.")]
	public const string k_ActionBasedSnapTurnProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.ActionBasedSnapTurnProvider.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_AudioAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Audio.AudioAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_ColorAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.ColorAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_FloatAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.FloatAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_QuaternionAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.QuaternionAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_QuaternionEulerAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.QuaternionEulerAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector2AffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector2AffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector3AffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector3AffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector4AffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives.Vector4AffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_BlendShapeAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.BlendShapeAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_ColorGradientLineRendererAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.ColorGradientLineRendererAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_ColorMaterialPropertyAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.ColorMaterialPropertyAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_FloatMaterialPropertyAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.FloatMaterialPropertyAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector2MaterialPropertyAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.Vector2MaterialPropertyAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector3MaterialPropertyAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.Vector3MaterialPropertyAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector4MaterialPropertyAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering.Vector4MaterialPropertyAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_ImageColorAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.UI.ImageColorAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_UniformTransformScaleAffordanceReceiver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Transformation.UniformTransformScaleAffordanceReceiver.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_MaterialInstanceHelper = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering.MaterialInstanceHelper.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_MaterialPropertyBlockHelper = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering.MaterialPropertyBlockHelper.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_XRInteractableAffordanceStateProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State.XRInteractableAffordanceStateProvider.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_XRInteractorAffordanceStateProvider = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State.XRInteractorAffordanceStateProvider.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_AudioAffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio.AudioAffordanceThemeDatum.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_ColorAffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives.ColorAffordanceThemeDatum.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_FloatAffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives.FloatAffordanceThemeDatum.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector2AffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives.Vector2AffordanceThemeDatum.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector3AffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives.Vector3AffordanceThemeDatum.html";

	[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
	public const string k_Vector4AffordanceThemeDatum = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives.Vector4AffordanceThemeDatum.html";

	[Obsolete("ARAnnotationInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARAnnotationInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARAnnotationInteractable.html";

	[Obsolete("ARGestureInteractor is marked for deprecation and will be removed in a future version.")]
	public const string k_ARGestureInteractor = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARGestureInteractor.html";

	[Obsolete("ARPlacementInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARPlacementInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARPlacementInteractable.html";

	[Obsolete("ARRotationInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARRotationInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARRotationInteractable.html";

	[Obsolete("ARScaleInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARScaleInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARScaleInteractable.html";

	[Obsolete("ARSelectionInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARSelectionInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARSelectionInteractable.html";

	[Obsolete("ARTranslationInteractable is marked for deprecation and will be removed in a future version.")]
	public const string k_ARTranslationInteractable = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.ARTranslationInteractable.html";

	[Obsolete("k_CharacterControllerDriver has been deprecated in version 3.0.0.")]
	public const string k_CharacterControllerDriver = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.CharacterControllerDriver.html";

	internal static string currentDocsVersion => "3.2";
}
