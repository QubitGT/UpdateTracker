using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-sf-distancegrab/")]
public class OVRPlayerController : MonoBehaviour
{
	public float Acceleration = 0.1f;

	public float Damping = 0.3f;

	public float BackAndSideDampen = 0.5f;

	public float JumpForce = 0.3f;

	public float RotationAmount = 1.5f;

	public float RotationRatchet = 45f;

	[Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
	public bool SnapRotation = true;

	[Obsolete]
	[Tooltip("[Deprecated] When enabled, snap rotation will happen about the center of the guardian rather than the center of the player/camera viewpoint. This (legacy) option should be left off except for edge cases that require extreme behavioral backwards compatibility.")]
	public bool RotateAroundGuardianCenter;

	[Tooltip("Sets the number of discrete speeds that will be used in continuous motion. If 0, motion speed is not discretized.")]
	public int FixedSpeedSteps;

	public bool HmdResetsY = true;

	public bool HmdRotatesY = true;

	public float GravityModifier = 0.379f;

	public bool useProfileData = true;

	[NonSerialized]
	public float CameraHeight;

	[NonSerialized]
	public bool Teleported;

	public bool EnableLinearMovement = true;

	public bool EnableRotation = true;

	public bool RotationEitherThumbstick;

	protected CharacterController Controller;

	protected OVRCameraRig CameraRig;

	private float MoveScale = 1f;

	private Vector3 MoveThrottle = Vector3.zero;

	private float FallSpeed;

	private OVRPose? InitialPose;

	private float MoveScaleMultiplier = 1f;

	private float RotationScaleMultiplier = 1f;

	private bool SkipMouseRotation = true;

	private bool HaltUpdateMovement;

	private bool prevHatLeft;

	private bool prevHatRight;

	private float SimulationRate = 60f;

	private float buttonRotation;

	private bool ReadyToSnapTurn;

	private bool playerControllerEnabled;

	private InputAction moveForwardAction;

	private InputAction moveLeftAction;

	private InputAction moveRightAction;

	private InputAction moveBackAction;

	private InputAction runAction;

	public float InitialYRotation { get; private set; }

	public event Action<Transform> TransformUpdated;

	public event Action CameraUpdated;

	public event Action PreCharacterMove;

	private void Start()
	{
		Vector3 localPosition = CameraRig.transform.localPosition;
		localPosition.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = localPosition;
		moveForwardAction = new InputAction(null, InputActionType.Value, "<Keyboard>/w");
		moveForwardAction.AddBinding("<Keyboard>/upArrow");
		moveLeftAction = new InputAction(null, InputActionType.Value, "<Keyboard>/a");
		moveLeftAction.AddBinding("<Keyboard>/leftArrow");
		moveRightAction = new InputAction(null, InputActionType.Value, "<Keyboard>/d");
		moveRightAction.AddBinding("<Keyboard>/rightArrow");
		moveBackAction = new InputAction(null, InputActionType.Value, "<Keyboard>/s");
		moveBackAction.AddBinding("<Keyboard>/downArrow");
		runAction = new InputAction(null, InputActionType.Value, "<Keyboard>/leftShift");
		runAction.AddBinding("<Keyboard>/rightShift");
		moveForwardAction.Enable();
		moveLeftAction.Enable();
		moveRightAction.Enable();
		moveBackAction.Enable();
		runAction.Enable();
	}

	private void Awake()
	{
		Controller = base.gameObject.GetComponent<CharacterController>();
		if (Controller == null)
		{
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");
		}
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		}
		else
		{
			CameraRig = componentsInChildren[0];
		}
		InitialYRotation = base.transform.rotation.eulerAngles.y;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		if (playerControllerEnabled)
		{
			OVRManager.display.RecenteredPose -= ResetOrientation;
			if (CameraRig != null)
			{
				CameraRig.UpdatedAnchors -= UpdateTransform;
			}
			playerControllerEnabled = false;
		}
		moveForwardAction.Disable();
		moveLeftAction.Disable();
		moveRightAction.Disable();
		moveBackAction.Disable();
		runAction.Disable();
	}

	private void Update()
	{
		if (!playerControllerEnabled && OVRManager.OVRManagerinitialized)
		{
			OVRManager.display.RecenteredPose += ResetOrientation;
			if (CameraRig != null)
			{
				CameraRig.UpdatedAnchors += UpdateTransform;
			}
			playerControllerEnabled = true;
		}
	}

	protected virtual void UpdateController()
	{
		if (useProfileData)
		{
			if (!InitialPose.HasValue)
			{
				InitialPose = new OVRPose
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}
			Vector3 localPosition = CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				localPosition.y = OVRManager.profile.eyeHeight - 0.5f * Controller.height + Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				localPosition.y = 0f - 0.5f * Controller.height + Controller.center.y;
			}
			CameraRig.transform.localPosition = localPosition;
		}
		else if (InitialPose.HasValue)
		{
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}
		CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;
		if (this.CameraUpdated != null)
		{
			this.CameraUpdated();
		}
		UpdateMovement();
		Vector3 zero = Vector3.zero;
		float num = 1f + Damping * SimulationRate * Time.deltaTime;
		MoveThrottle.x /= num;
		MoveThrottle.y = ((MoveThrottle.y > 0f) ? (MoveThrottle.y / num) : MoveThrottle.y);
		MoveThrottle.z /= num;
		zero += MoveThrottle * SimulationRate * Time.deltaTime;
		if (Controller.isGrounded && FallSpeed <= 0f)
		{
			FallSpeed = Physics.gravity.y * (GravityModifier * 0.002f);
		}
		else
		{
			FallSpeed += Physics.gravity.y * (GravityModifier * 0.002f) * SimulationRate * Time.deltaTime;
		}
		zero.y += FallSpeed * SimulationRate * Time.deltaTime;
		if (Controller.isGrounded && MoveThrottle.y <= base.transform.lossyScale.y * 0.001f)
		{
			float num2 = Mathf.Max(Controller.stepOffset, new Vector3(zero.x, 0f, zero.z).magnitude);
			zero -= num2 * Vector3.up;
		}
		if (this.PreCharacterMove != null)
		{
			this.PreCharacterMove();
			Teleported = false;
		}
		Vector3 vector = Vector3.Scale(Controller.transform.localPosition + zero, new Vector3(1f, 0f, 1f));
		Controller.Move(zero);
		Vector3 vector2 = Vector3.Scale(Controller.transform.localPosition, new Vector3(1f, 0f, 1f));
		if (vector != vector2)
		{
			MoveThrottle += (vector2 - vector) / (SimulationRate * Time.deltaTime);
		}
	}

	public virtual void UpdateMovement()
	{
		if (HaltUpdateMovement)
		{
			return;
		}
		if (EnableLinearMovement)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			flag = moveForwardAction.phase == InputActionPhase.Started;
			flag2 = moveLeftAction.phase == InputActionPhase.Started;
			flag3 = moveRightAction.phase == InputActionPhase.Started;
			flag4 = moveBackAction.phase == InputActionPhase.Started;
			bool flag5 = false;
			if (OVRInput.Get(OVRInput.Button.DpadUp))
			{
				flag = true;
				flag5 = true;
			}
			if (OVRInput.Get(OVRInput.Button.DpadDown))
			{
				flag4 = true;
				flag5 = true;
			}
			MoveScale = 1f;
			if ((flag && flag2) || (flag && flag3) || (flag4 && flag2) || (flag4 && flag3))
			{
				MoveScale = 0.70710677f;
			}
			if (!Controller.isGrounded)
			{
				MoveScale = 0f;
			}
			MoveScale *= SimulationRate * Time.deltaTime;
			float num = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
			if (flag5 || runAction.phase == InputActionPhase.Started)
			{
				num *= 2f;
			}
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.z = (eulerAngles.x = 0f);
			Quaternion quaternion = Quaternion.Euler(eulerAngles);
			if (flag)
			{
				MoveThrottle += quaternion * (base.transform.lossyScale.z * num * Vector3.forward);
			}
			if (flag4)
			{
				MoveThrottle += quaternion * (base.transform.lossyScale.z * num * BackAndSideDampen * Vector3.back);
			}
			if (flag2)
			{
				MoveThrottle += quaternion * (base.transform.lossyScale.x * num * BackAndSideDampen * Vector3.left);
			}
			if (flag3)
			{
				MoveThrottle += quaternion * (base.transform.lossyScale.x * num * BackAndSideDampen * Vector3.right);
			}
			num = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
			num *= 1f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
			Vector2 vector = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
			if (FixedSpeedSteps > 0)
			{
				vector.y = Mathf.Round(vector.y * (float)FixedSpeedSteps) / (float)FixedSpeedSteps;
				vector.x = Mathf.Round(vector.x * (float)FixedSpeedSteps) / (float)FixedSpeedSteps;
			}
			if (vector.y > 0f)
			{
				MoveThrottle += quaternion * (vector.y * base.transform.lossyScale.z * num * Vector3.forward);
			}
			if (vector.y < 0f)
			{
				MoveThrottle += quaternion * (Mathf.Abs(vector.y) * base.transform.lossyScale.z * num * BackAndSideDampen * Vector3.back);
			}
			if (vector.x < 0f)
			{
				MoveThrottle += quaternion * (Mathf.Abs(vector.x) * base.transform.lossyScale.x * num * BackAndSideDampen * Vector3.left);
			}
			if (vector.x > 0f)
			{
				MoveThrottle += quaternion * (vector.x * base.transform.lossyScale.x * num * BackAndSideDampen * Vector3.right);
			}
		}
		if (!EnableRotation)
		{
			return;
		}
		Vector3 euler = (RotateAroundGuardianCenter ? base.transform.rotation.eulerAngles : Vector3.zero);
		float num2 = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;
		bool flag6 = OVRInput.Get(OVRInput.Button.PrimaryShoulder);
		if (flag6 && !prevHatLeft)
		{
			euler.y -= RotationRatchet;
		}
		prevHatLeft = flag6;
		bool flag7 = OVRInput.Get(OVRInput.Button.SecondaryShoulder);
		if (flag7 && !prevHatRight)
		{
			euler.y += RotationRatchet;
		}
		prevHatRight = flag7;
		euler.y += buttonRotation;
		buttonRotation = 0f;
		if (!SkipMouseRotation)
		{
			euler.y += Input.GetAxis("Mouse X") * num2 * 3.25f;
		}
		if (SnapRotation)
		{
			if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) || (RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft)))
			{
				if (ReadyToSnapTurn)
				{
					euler.y -= RotationRatchet;
					ReadyToSnapTurn = false;
				}
			}
			else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) || (RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight)))
			{
				if (ReadyToSnapTurn)
				{
					euler.y += RotationRatchet;
					ReadyToSnapTurn = false;
				}
			}
			else
			{
				ReadyToSnapTurn = true;
			}
		}
		else
		{
			Vector2 vector2 = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
			if (RotationEitherThumbstick)
			{
				Vector2 vector3 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
				if (vector2.sqrMagnitude < vector3.sqrMagnitude)
				{
					vector2 = vector3;
				}
			}
			euler.y += vector2.x * num2;
		}
		if (RotateAroundGuardianCenter)
		{
			base.transform.rotation = Quaternion.Euler(euler);
		}
		else
		{
			base.transform.RotateAround(base.transform.position, Vector3.up, euler.y);
		}
	}

	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform trackingSpace = CameraRig.trackingSpace;
		Transform centerEyeAnchor = CameraRig.centerEyeAnchor;
		if (HmdRotatesY && !Teleported)
		{
			Vector3 position = trackingSpace.position;
			Quaternion rotation = trackingSpace.rotation;
			base.transform.rotation = Quaternion.Euler(0f, centerEyeAnchor.rotation.eulerAngles.y, 0f);
			trackingSpace.position = position;
			trackingSpace.rotation = rotation;
		}
		UpdateController();
		if (this.TransformUpdated != null)
		{
			this.TransformUpdated(trackingSpace);
		}
	}

	public bool Jump()
	{
		if (!Controller.isGrounded)
		{
			return false;
		}
		MoveThrottle += new Vector3(0f, base.transform.lossyScale.y * JumpForce, 0f);
		return true;
	}

	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0f;
	}

	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	public void ResetOrientation()
	{
		if (HmdResetsY && !HmdRotatesY)
		{
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.y = InitialYRotation;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}
}
