using System;
using UnityEngine.SpatialTracking;

namespace UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class XRControllerState
{
	public double time;

	public InputTrackingState inputTrackingState;

	public bool isTracked;

	public Vector3 position;

	public Quaternion rotation;

	public InteractionState selectInteractionState;

	public InteractionState activateInteractionState;

	public InteractionState uiPressInteractionState;

	public Vector2 uiScrollValue;

	[Obsolete("poseDataFlags has been deprecated. Use inputTrackingState instead.", true)]
	public PoseDataFlags poseDataFlags
	{
		get
		{
			return PoseDataFlags.NoData;
		}
		set
		{
		}
	}

	protected XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState, bool isTracked)
	{
		this.time = time;
		this.position = position;
		this.rotation = rotation;
		this.inputTrackingState = inputTrackingState;
		this.isTracked = isTracked;
	}

	public XRControllerState()
		: this(0.0, Vector3.zero, Quaternion.identity, InputTrackingState.None, isTracked: false)
	{
	}

	public XRControllerState(XRControllerState value)
	{
		time = value.time;
		position = value.position;
		rotation = value.rotation;
		inputTrackingState = value.inputTrackingState;
		isTracked = value.isTracked;
		selectInteractionState = value.selectInteractionState;
		activateInteractionState = value.activateInteractionState;
		uiPressInteractionState = value.uiPressInteractionState;
		uiScrollValue = value.uiScrollValue;
	}

	public XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState, bool isTracked, bool selectActive, bool activateActive, bool pressActive)
		: this(time, position, rotation, inputTrackingState, isTracked)
	{
		selectInteractionState.SetFrameState(selectActive);
		activateInteractionState.SetFrameState(activateActive);
		uiPressInteractionState.SetFrameState(pressActive);
	}

	public XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState, bool isTracked, bool selectActive, bool activateActive, bool pressActive, float selectValue, float activateValue, float pressValue)
		: this(time, position, rotation, inputTrackingState, isTracked)
	{
		selectInteractionState.SetFrameState(selectActive, selectValue);
		activateInteractionState.SetFrameState(activateActive, activateValue);
		uiPressInteractionState.SetFrameState(pressActive, pressValue);
	}

	public void ResetFrameDependentStates()
	{
		selectInteractionState.ResetFrameDependent();
		activateInteractionState.ResetFrameDependent();
		uiPressInteractionState.ResetFrameDependent();
	}

	public override string ToString()
	{
		return $"time: {time}, position: {position}, rotation: {rotation}, selectActive: {selectInteractionState.active}, activateActive: {activateInteractionState.active}, pressActive: {uiPressInteractionState.active}, isTracked: {isTracked}, inputTrackingState: {inputTrackingState}";
	}

	[Obsolete("This constructor has been deprecated. Use the constructors with the inputTrackingState parameter.", true)]
	public XRControllerState(double time, Vector3 position, Quaternion rotation, bool selectActive, bool activateActive, bool pressActive)
		: this(time, position, rotation, InputTrackingState.Position | InputTrackingState.Rotation, selectActive, activateActive, pressActive)
	{
	}

	[Obsolete("This constructor has been deprecated. Use the constructor with the isTracked parameter.", true)]
	protected XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState)
		: this(time, position, rotation, inputTrackingState, isTracked: true)
	{
	}

	[Obsolete("This constructor has been deprecated. Use the constructor with the isTracked parameter.", true)]
	public XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState, bool selectActive, bool activateActive, bool pressActive)
		: this(time, position, rotation, inputTrackingState, isTracked: true)
	{
	}

	[Obsolete("This constructor has been deprecated. Use the constructor with the isTracked parameter.", true)]
	public XRControllerState(double time, Vector3 position, Quaternion rotation, InputTrackingState inputTrackingState, bool selectActive, bool activateActive, bool pressActive, float selectValue, float activateValue, float pressValue)
		: this(time, position, rotation, inputTrackingState, isTracked: true)
	{
	}

	[Obsolete("ResetInputs has been renamed. Use ResetFrameDependentStates instead. (UnityUpgradable) -> ResetFrameDependentStates()", true)]
	public void ResetInputs()
	{
	}
}
