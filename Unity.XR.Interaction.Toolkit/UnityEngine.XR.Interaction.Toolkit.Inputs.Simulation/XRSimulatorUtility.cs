using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

internal static class XRSimulatorUtility
{
	internal static readonly float cameraMaxXAngle = 80f;

	internal static readonly Vector3 leftDeviceDefaultInitialPosition = new Vector3(-0.1f, -0.05f, 0.3f);

	internal static readonly Vector3 rightDeviceDefaultInitialPosition = new Vector3(0.1f, -0.05f, 0.3f);

	internal static SimulatedDeviceLifecycleManager FindCreateSimulatedDeviceLifecycleManager(GameObject simulator)
	{
		if (ComponentLocatorUtility<SimulatedDeviceLifecycleManager>.TryFindComponent(out var component))
		{
			return component;
		}
		return simulator.AddComponent<SimulatedDeviceLifecycleManager>();
	}

	internal static SimulatedHandExpressionManager FindCreateSimulatedHandExpressionManager(GameObject simulator)
	{
		if (ComponentLocatorUtility<SimulatedHandExpressionManager>.TryFindComponent(out var component))
		{
			return component;
		}
		return simulator.AddComponent<SimulatedHandExpressionManager>();
	}

	internal static void Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
	{
		InputAction inputAction = GetInputAction(reference);
		if (inputAction != null)
		{
			if (performed != null)
			{
				inputAction.performed += performed;
			}
			if (canceled != null)
			{
				inputAction.canceled += canceled;
			}
		}
	}

	internal static void Unsubscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
	{
		InputAction inputAction = GetInputAction(reference);
		if (inputAction != null)
		{
			if (performed != null)
			{
				inputAction.performed -= performed;
			}
			if (canceled != null)
			{
				inputAction.canceled -= canceled;
			}
		}
	}

	private static InputAction GetInputAction(InputActionReference actionReference)
	{
		if (!(actionReference != null))
		{
			return null;
		}
		return actionReference.action;
	}

	internal static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedControllerState state, in Quaternion inverseCameraParentRotation)
	{
		return GetDeltaRotation(translateSpace, state.deviceRotation, in inverseCameraParentRotation);
	}

	internal static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHandState state, in Quaternion inverseCameraParentRotation)
	{
		return GetDeltaRotation(translateSpace, state.rotation, in inverseCameraParentRotation);
	}

	internal static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHMDState state, in Quaternion inverseCameraParentRotation)
	{
		return GetDeltaRotation(translateSpace, state.centerEyeRotation, in inverseCameraParentRotation);
	}

	internal static void GetAxes(Space translateSpace, Transform cameraTransform, out Vector3 right, out Vector3 up, out Vector3 forward)
	{
		if (cameraTransform == null)
		{
			throw new ArgumentNullException("cameraTransform");
		}
		switch (translateSpace)
		{
		case Space.Local:
		{
			Transform parent = cameraTransform.parent;
			if (parent != null)
			{
				right = parent.TransformDirection(Vector3.right);
				up = parent.TransformDirection(Vector3.up);
				forward = parent.TransformDirection(Vector3.forward);
			}
			else
			{
				right = Vector3.right;
				up = Vector3.up;
				forward = Vector3.forward;
			}
			break;
		}
		case Space.Parent:
			right = Vector3.right;
			up = Vector3.up;
			forward = Vector3.forward;
			break;
		case Space.Screen:
			right = cameraTransform.TransformDirection(Vector3.right);
			up = cameraTransform.TransformDirection(Vector3.up);
			forward = cameraTransform.TransformDirection(Vector3.forward);
			break;
		default:
			right = Vector3.right;
			up = Vector3.up;
			forward = Vector3.forward;
			break;
		}
	}

	internal static Quaternion GetDeltaRotation(Space translateSpace, Quaternion rotation, in Quaternion inverseCameraParentRotation)
	{
		return translateSpace switch
		{
			Space.Local => rotation * inverseCameraParentRotation, 
			Space.Parent => Quaternion.identity, 
			Space.Screen => inverseCameraParentRotation, 
			_ => Quaternion.identity, 
		};
	}

	internal static bool FindCameraTransform(ref (Transform transform, Camera camera) cachedCamera, ref Transform cameraTransform)
	{
		if (cachedCamera.transform != cameraTransform)
		{
			cachedCamera = (transform: cameraTransform, camera: (cameraTransform != null) ? cameraTransform.GetComponent<Camera>() : null);
		}
		if (cachedCamera.transform == null || (cachedCamera.camera != null && !cachedCamera.camera.isActiveAndEnabled))
		{
			Camera main = Camera.main;
			if (main == null)
			{
				return false;
			}
			cameraTransform = main.transform;
			cachedCamera = (transform: cameraTransform, camera: cameraTransform.GetComponent<Camera>());
		}
		return true;
	}

	internal unsafe static bool TryExecuteCommand(InputDeviceCommand* commandPtr, out long result)
	{
		FourCC type = commandPtr->type;
		if (type == RequestSyncCommand.Type)
		{
			result = 1L;
			return true;
		}
		if (type == QueryCanRunInBackground.Type)
		{
			((QueryCanRunInBackground*)commandPtr)->canRunInBackground = true;
			result = 1L;
			return true;
		}
		result = 0L;
		return false;
	}

	internal static Vector3 GetTranslationInDeviceSpace(float xTranslateInput, float yTranslateInput, float zTranslateInput, Transform cameraTransform, Quaternion cameraParentRotation, Quaternion inverseCameraParentRotation)
	{
		Vector3 translationInWorldSpace = GetTranslationInWorldSpace(xTranslateInput, yTranslateInput, zTranslateInput, cameraTransform, cameraParentRotation);
		return inverseCameraParentRotation * translationInWorldSpace;
	}

	internal static Vector3 GetTranslationInWorldSpace(float xTranslateInput, float yTranslateInput, float zTranslateInput, Transform cameraTransform, Quaternion cameraParentRotation)
	{
		Vector3 vector = new Vector3(xTranslateInput, yTranslateInput, zTranslateInput);
		Vector3 vector2 = cameraTransform.forward;
		Vector3 vector3 = cameraParentRotation * Vector3.up;
		if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(vector2, vector3)), 1f))
		{
			vector2 = -cameraTransform.up;
		}
		return Quaternion.LookRotation(Vector3.ProjectOnPlane(vector2, vector3), vector3) * vector;
	}
}
