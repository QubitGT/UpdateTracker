using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Input;

[Obsolete("OpenXR.Input.PoseControl is deprecated. Please use UnityEngine.InputSystem.XR.PoseControl instead.", false)]
public class PoseControl : InputControl<Pose>
{
	[Preserve]
	[InputControl(offset = 0u)]
	public ButtonControl isTracked { get; private set; }

	[Preserve]
	[InputControl(offset = 4u)]
	public IntegerControl trackingState { get; private set; }

	[Preserve]
	[InputControl(offset = 8u, noisy = true)]
	public Vector3Control position { get; private set; }

	[Preserve]
	[InputControl(offset = 20u, noisy = true)]
	public QuaternionControl rotation { get; private set; }

	[Preserve]
	[InputControl(offset = 36u, noisy = true)]
	public Vector3Control velocity { get; private set; }

	[Preserve]
	[InputControl(offset = 48u, noisy = true)]
	public Vector3Control angularVelocity { get; private set; }

	protected override void FinishSetup()
	{
		isTracked = GetChildControl<ButtonControl>("isTracked");
		trackingState = GetChildControl<IntegerControl>("trackingState");
		position = GetChildControl<Vector3Control>("position");
		rotation = GetChildControl<QuaternionControl>("rotation");
		velocity = GetChildControl<Vector3Control>("velocity");
		angularVelocity = GetChildControl<Vector3Control>("angularVelocity");
		base.FinishSetup();
	}

	public unsafe override Pose ReadUnprocessedValueFromState(void* statePtr)
	{
		Pose result = default(Pose);
		result.isTracked = isTracked.ReadUnprocessedValueFromState(statePtr) > 0.5f;
		result.trackingState = (InputTrackingState)trackingState.ReadUnprocessedValueFromState(statePtr);
		result.position = position.ReadUnprocessedValueFromState(statePtr);
		result.rotation = rotation.ReadUnprocessedValueFromState(statePtr);
		result.velocity = velocity.ReadUnprocessedValueFromState(statePtr);
		result.angularVelocity = angularVelocity.ReadUnprocessedValueFromState(statePtr);
		return result;
	}

	public unsafe override void WriteValueIntoState(Pose value, void* statePtr)
	{
		isTracked.WriteValueIntoState(value.isTracked, statePtr);
		trackingState.WriteValueIntoState((uint)value.trackingState, statePtr);
		position.WriteValueIntoState(value.position, statePtr);
		rotation.WriteValueIntoState(value.rotation, statePtr);
		velocity.WriteValueIntoState(value.velocity, statePtr);
		angularVelocity.WriteValueIntoState(value.angularVelocity, statePtr);
	}
}
