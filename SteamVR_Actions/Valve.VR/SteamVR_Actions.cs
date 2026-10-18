namespace Valve.VR;

public class SteamVR_Actions
{
	private static SteamVR_Action_Boolean p_gorillaTag_LeftTriggerTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftTriggerClick;

	private static SteamVR_Action_Single p_gorillaTag_LeftTriggerFloat;

	private static SteamVR_Action_Boolean p_gorillaTag_RightTriggerTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_RightTriggerClick;

	private static SteamVR_Action_Single p_gorillaTag_RightTriggerFloat;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftGripTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftGripClick;

	private static SteamVR_Action_Single p_gorillaTag_LeftGripFloat;

	private static SteamVR_Action_Boolean p_gorillaTag_RightGripTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_RightGripClick;

	private static SteamVR_Action_Single p_gorillaTag_RightGripFloat;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftPrimaryClick;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftPrimaryTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_RightPrimaryClick;

	private static SteamVR_Action_Boolean p_gorillaTag_RightPrimaryTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftSecondaryClick;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftSecondaryTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_RightSecondaryClick;

	private static SteamVR_Action_Boolean p_gorillaTag_RightSecondaryTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftJoystickTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_LeftJoystickClick;

	private static SteamVR_Action_Vector2 p_gorillaTag_LeftJoystick2DAxis;

	private static SteamVR_Action_Boolean p_gorillaTag_RightJoystickTouch;

	private static SteamVR_Action_Boolean p_gorillaTag_RightJoystickClick;

	private static SteamVR_Action_Vector2 p_gorillaTag_RightJoystick2DAxis;

	private static SteamVR_Action_Boolean p_gorillaTag_System;

	private static SteamVR_Action_Vibration p_gorillaTag_LeftHaptics;

	private static SteamVR_Action_Vibration p_gorillaTag_RightHaptics;

	private static SteamVR_Input_ActionSet_GorillaTag p_GorillaTag;

	public static SteamVR_Action_Boolean gorillaTag_LeftTriggerTouch => p_gorillaTag_LeftTriggerTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftTriggerClick => p_gorillaTag_LeftTriggerClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Single gorillaTag_LeftTriggerFloat => p_gorillaTag_LeftTriggerFloat.GetCopy<SteamVR_Action_Single>();

	public static SteamVR_Action_Boolean gorillaTag_RightTriggerTouch => p_gorillaTag_RightTriggerTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightTriggerClick => p_gorillaTag_RightTriggerClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Single gorillaTag_RightTriggerFloat => p_gorillaTag_RightTriggerFloat.GetCopy<SteamVR_Action_Single>();

	public static SteamVR_Action_Boolean gorillaTag_LeftGripTouch => p_gorillaTag_LeftGripTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftGripClick => p_gorillaTag_LeftGripClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Single gorillaTag_LeftGripFloat => p_gorillaTag_LeftGripFloat.GetCopy<SteamVR_Action_Single>();

	public static SteamVR_Action_Boolean gorillaTag_RightGripTouch => p_gorillaTag_RightGripTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightGripClick => p_gorillaTag_RightGripClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Single gorillaTag_RightGripFloat => p_gorillaTag_RightGripFloat.GetCopy<SteamVR_Action_Single>();

	public static SteamVR_Action_Boolean gorillaTag_LeftPrimaryClick => p_gorillaTag_LeftPrimaryClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftPrimaryTouch => p_gorillaTag_LeftPrimaryTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightPrimaryClick => p_gorillaTag_RightPrimaryClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightPrimaryTouch => p_gorillaTag_RightPrimaryTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftSecondaryClick => p_gorillaTag_LeftSecondaryClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftSecondaryTouch => p_gorillaTag_LeftSecondaryTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightSecondaryClick => p_gorillaTag_RightSecondaryClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightSecondaryTouch => p_gorillaTag_RightSecondaryTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftJoystickTouch => p_gorillaTag_LeftJoystickTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_LeftJoystickClick => p_gorillaTag_LeftJoystickClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Vector2 gorillaTag_LeftJoystick2DAxis => p_gorillaTag_LeftJoystick2DAxis.GetCopy<SteamVR_Action_Vector2>();

	public static SteamVR_Action_Boolean gorillaTag_RightJoystickTouch => p_gorillaTag_RightJoystickTouch.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Boolean gorillaTag_RightJoystickClick => p_gorillaTag_RightJoystickClick.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Vector2 gorillaTag_RightJoystick2DAxis => p_gorillaTag_RightJoystick2DAxis.GetCopy<SteamVR_Action_Vector2>();

	public static SteamVR_Action_Boolean gorillaTag_System => p_gorillaTag_System.GetCopy<SteamVR_Action_Boolean>();

	public static SteamVR_Action_Vibration gorillaTag_LeftHaptics => p_gorillaTag_LeftHaptics.GetCopy<SteamVR_Action_Vibration>();

	public static SteamVR_Action_Vibration gorillaTag_RightHaptics => p_gorillaTag_RightHaptics.GetCopy<SteamVR_Action_Vibration>();

	public static SteamVR_Input_ActionSet_GorillaTag GorillaTag => p_GorillaTag.GetCopy<SteamVR_Input_ActionSet_GorillaTag>();

	private static void InitializeActionArrays()
	{
		SteamVR_Input.actions = new SteamVR_Action[29]
		{
			gorillaTag_LeftTriggerTouch, gorillaTag_LeftTriggerClick, gorillaTag_LeftTriggerFloat, gorillaTag_RightTriggerTouch, gorillaTag_RightTriggerClick, gorillaTag_RightTriggerFloat, gorillaTag_LeftGripTouch, gorillaTag_LeftGripClick, gorillaTag_LeftGripFloat, gorillaTag_RightGripTouch,
			gorillaTag_RightGripClick, gorillaTag_RightGripFloat, gorillaTag_LeftPrimaryClick, gorillaTag_LeftPrimaryTouch, gorillaTag_RightPrimaryClick, gorillaTag_RightPrimaryTouch, gorillaTag_LeftSecondaryClick, gorillaTag_LeftSecondaryTouch, gorillaTag_RightSecondaryClick, gorillaTag_RightSecondaryTouch,
			gorillaTag_LeftJoystickTouch, gorillaTag_LeftJoystickClick, gorillaTag_LeftJoystick2DAxis, gorillaTag_RightJoystickTouch, gorillaTag_RightJoystickClick, gorillaTag_RightJoystick2DAxis, gorillaTag_System, gorillaTag_LeftHaptics, gorillaTag_RightHaptics
		};
		SteamVR_Input.actionsIn = new ISteamVR_Action_In[27]
		{
			gorillaTag_LeftTriggerTouch, gorillaTag_LeftTriggerClick, gorillaTag_LeftTriggerFloat, gorillaTag_RightTriggerTouch, gorillaTag_RightTriggerClick, gorillaTag_RightTriggerFloat, gorillaTag_LeftGripTouch, gorillaTag_LeftGripClick, gorillaTag_LeftGripFloat, gorillaTag_RightGripTouch,
			gorillaTag_RightGripClick, gorillaTag_RightGripFloat, gorillaTag_LeftPrimaryClick, gorillaTag_LeftPrimaryTouch, gorillaTag_RightPrimaryClick, gorillaTag_RightPrimaryTouch, gorillaTag_LeftSecondaryClick, gorillaTag_LeftSecondaryTouch, gorillaTag_RightSecondaryClick, gorillaTag_RightSecondaryTouch,
			gorillaTag_LeftJoystickTouch, gorillaTag_LeftJoystickClick, gorillaTag_LeftJoystick2DAxis, gorillaTag_RightJoystickTouch, gorillaTag_RightJoystickClick, gorillaTag_RightJoystick2DAxis, gorillaTag_System
		};
		SteamVR_Input.actionsOut = new ISteamVR_Action_Out[2] { gorillaTag_LeftHaptics, gorillaTag_RightHaptics };
		SteamVR_Input.actionsVibration = new SteamVR_Action_Vibration[2] { gorillaTag_LeftHaptics, gorillaTag_RightHaptics };
		SteamVR_Input.actionsPose = new SteamVR_Action_Pose[0];
		SteamVR_Input.actionsBoolean = new SteamVR_Action_Boolean[21]
		{
			gorillaTag_LeftTriggerTouch, gorillaTag_LeftTriggerClick, gorillaTag_RightTriggerTouch, gorillaTag_RightTriggerClick, gorillaTag_LeftGripTouch, gorillaTag_LeftGripClick, gorillaTag_RightGripTouch, gorillaTag_RightGripClick, gorillaTag_LeftPrimaryClick, gorillaTag_LeftPrimaryTouch,
			gorillaTag_RightPrimaryClick, gorillaTag_RightPrimaryTouch, gorillaTag_LeftSecondaryClick, gorillaTag_LeftSecondaryTouch, gorillaTag_RightSecondaryClick, gorillaTag_RightSecondaryTouch, gorillaTag_LeftJoystickTouch, gorillaTag_LeftJoystickClick, gorillaTag_RightJoystickTouch, gorillaTag_RightJoystickClick,
			gorillaTag_System
		};
		SteamVR_Input.actionsSingle = new SteamVR_Action_Single[4] { gorillaTag_LeftTriggerFloat, gorillaTag_RightTriggerFloat, gorillaTag_LeftGripFloat, gorillaTag_RightGripFloat };
		SteamVR_Input.actionsVector2 = new SteamVR_Action_Vector2[2] { gorillaTag_LeftJoystick2DAxis, gorillaTag_RightJoystick2DAxis };
		SteamVR_Input.actionsVector3 = new SteamVR_Action_Vector3[0];
		SteamVR_Input.actionsSkeleton = new SteamVR_Action_Skeleton[0];
		SteamVR_Input.actionsNonPoseNonSkeletonIn = new ISteamVR_Action_In[27]
		{
			gorillaTag_LeftTriggerTouch, gorillaTag_LeftTriggerClick, gorillaTag_LeftTriggerFloat, gorillaTag_RightTriggerTouch, gorillaTag_RightTriggerClick, gorillaTag_RightTriggerFloat, gorillaTag_LeftGripTouch, gorillaTag_LeftGripClick, gorillaTag_LeftGripFloat, gorillaTag_RightGripTouch,
			gorillaTag_RightGripClick, gorillaTag_RightGripFloat, gorillaTag_LeftPrimaryClick, gorillaTag_LeftPrimaryTouch, gorillaTag_RightPrimaryClick, gorillaTag_RightPrimaryTouch, gorillaTag_LeftSecondaryClick, gorillaTag_LeftSecondaryTouch, gorillaTag_RightSecondaryClick, gorillaTag_RightSecondaryTouch,
			gorillaTag_LeftJoystickTouch, gorillaTag_LeftJoystickClick, gorillaTag_LeftJoystick2DAxis, gorillaTag_RightJoystickTouch, gorillaTag_RightJoystickClick, gorillaTag_RightJoystick2DAxis, gorillaTag_System
		};
	}

	private static void PreInitActions()
	{
		p_gorillaTag_LeftTriggerTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftTriggerTouch");
		p_gorillaTag_LeftTriggerClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftTriggerClick");
		p_gorillaTag_LeftTriggerFloat = SteamVR_Action.Create<SteamVR_Action_Single>("/actions/GorillaTag/in/LeftTriggerFloat");
		p_gorillaTag_RightTriggerTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightTriggerTouch");
		p_gorillaTag_RightTriggerClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightTriggerClick");
		p_gorillaTag_RightTriggerFloat = SteamVR_Action.Create<SteamVR_Action_Single>("/actions/GorillaTag/in/RightTriggerFloat");
		p_gorillaTag_LeftGripTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftGripTouch");
		p_gorillaTag_LeftGripClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftGripClick");
		p_gorillaTag_LeftGripFloat = SteamVR_Action.Create<SteamVR_Action_Single>("/actions/GorillaTag/in/LeftGripFloat");
		p_gorillaTag_RightGripTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightGripTouch");
		p_gorillaTag_RightGripClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightGripClick");
		p_gorillaTag_RightGripFloat = SteamVR_Action.Create<SteamVR_Action_Single>("/actions/GorillaTag/in/RightGripFloat");
		p_gorillaTag_LeftPrimaryClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftPrimaryClick");
		p_gorillaTag_LeftPrimaryTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftPrimaryTouch");
		p_gorillaTag_RightPrimaryClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightPrimaryClick");
		p_gorillaTag_RightPrimaryTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightPrimaryTouch");
		p_gorillaTag_LeftSecondaryClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftSecondaryClick");
		p_gorillaTag_LeftSecondaryTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftSecondaryTouch");
		p_gorillaTag_RightSecondaryClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightSecondaryClick");
		p_gorillaTag_RightSecondaryTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightSecondaryTouch");
		p_gorillaTag_LeftJoystickTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftJoystickTouch");
		p_gorillaTag_LeftJoystickClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/LeftJoystickClick");
		p_gorillaTag_LeftJoystick2DAxis = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/GorillaTag/in/LeftJoystick2DAxis");
		p_gorillaTag_RightJoystickTouch = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightJoystickTouch");
		p_gorillaTag_RightJoystickClick = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/RightJoystickClick");
		p_gorillaTag_RightJoystick2DAxis = SteamVR_Action.Create<SteamVR_Action_Vector2>("/actions/GorillaTag/in/RightJoystick2DAxis");
		p_gorillaTag_System = SteamVR_Action.Create<SteamVR_Action_Boolean>("/actions/GorillaTag/in/System");
		p_gorillaTag_LeftHaptics = SteamVR_Action.Create<SteamVR_Action_Vibration>("/actions/GorillaTag/out/LeftHaptics");
		p_gorillaTag_RightHaptics = SteamVR_Action.Create<SteamVR_Action_Vibration>("/actions/GorillaTag/out/RightHaptics");
	}

	private static void StartPreInitActionSets()
	{
		p_GorillaTag = SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_GorillaTag>("/actions/GorillaTag");
		SteamVR_Input.actionSets = new SteamVR_ActionSet[1] { GorillaTag };
	}

	public static void PreInitialize()
	{
		StartPreInitActionSets();
		SteamVR_Input.PreinitializeActionSetDictionaries();
		PreInitActions();
		InitializeActionArrays();
		SteamVR_Input.PreinitializeActionDictionaries();
		SteamVR_Input.PreinitializeFinishActionSets();
	}
}
