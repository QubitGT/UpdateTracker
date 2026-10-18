using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class FirstPersonLocomotor : MonoBehaviour, ILocomotionEventHandler, IDeltaTimeConsumer, ITimeConsumer
{
	[Header("Character")]
	[SerializeField]
	[Tooltip("The CharacterController reprensenting the character that is used to move the player around the scene.")]
	private CharacterController _characterController;

	[Header("VR Player")]
	[SerializeField]
	[Tooltip("Root of the actual VR player so it can be sync with with the CharacterController. If you provided a _playerEyes you must also provide a _playerOrigin.")]
	private Transform _playerOrigin;

	[SerializeField]
	[Tooltip("Eyes of the actual VR player so it can be sync with the capsule. If you provided a _playerOrigin you must also provide a _playerEyes.")]
	private Transform _playerEyes;

	[SerializeField]
	[Tooltip("After the player penetrates the head inside a collider (for example a wall), the maximum distance before the player gets reset to the capsule position when trying to move synthetically.")]
	private float _maxWallPenetrationDistance = 0.3f;

	[SerializeField]
	[Tooltip("After using LocomotionEvent.TranslationType.AbsoluteEyeLevel that disables the ground checks. What is the maximum deviation of the player before the physics are re-enabled.")]
	private float _exitHotspotDistance = 0.3f;

	[SerializeField]
	[Tooltip("When _playerOrigin and _playerEyes are present. This will force the capsule height to update using the actual player height, instead of using _defaultHeight")]
	private bool _autoUpdateHeight = true;

	[Header("Parameters")]
	[SerializeField]
	[Tooltip("Height of the character capsule when standing normally. This might be overriden by _autoUpdateHeight")]
	private float _defaultHeight = 1.4f;

	[SerializeField]
	[Tooltip("General height offset applied to the capsule.")]
	private float _heightOffset;

	[SerializeField]
	[Tooltip("Height offset added while crouching.")]
	private float _crouchHeightOffset = -0.5f;

	[SerializeField]
	[Tooltip("Speed multiplier applied while moving normally.")]
	private float _speedFactor = 30f;

	[SerializeField]
	[Tooltip("Speed multiplier applied while crouching.")]
	private float _crouchSpeedFactor = 10f;

	[SerializeField]
	[Tooltip("Speed multiplier applied while running.")]
	private float _runningSpeedFactor = 50f;

	[SerializeField]
	[Tooltip("The rate of acceleration during movement.")]
	private float _acceleration = 5f;

	[SerializeField]
	[Tooltip("The rate of damping on movement while grounded.")]
	private float _groundDamping = 40f;

	[SerializeField]
	[Tooltip("The rate of damping on the vertical movement while jumping.")]
	private float _jumpDamping;

	[SerializeField]
	[Tooltip("The rate of damping on the horizontal movement while in the air.")]
	private float _airDamping = 1f;

	[SerializeField]
	[Tooltip("The force applied to the character when jumping.")]
	private float _jumpForce = 2.5f;

	[SerializeField]
	[Tooltip("Modifies the strength of gravity.")]
	private float _gravityFactor = 1f;

	[SerializeField]
	[Tooltip("Extra time after starting to fall to allow jumping.")]
	private float _coyoteTime;

	[SerializeField]
	[Tooltip("Correct the input velocity so it always points in the XZ plane.Use with the _inputVelocityStabilization curve to adjust the range")]
	private bool _flattenInputVelocity = true;

	[SerializeField]
	[Tooltip("When the input velocity points too far up or down the forward direction will be slerped between the .forward and the .up using this curve for the final forward to be stable. x: from -1 to 1, represents the dot product of forward.worldUp. y: 0 represents the real forward, 1 the up direction and -1 the down direction.")]
	[ConditionalHide("_flattenInputVelocity", true, ConditionalHideAttribute.DisplayMode.ShowIfTrue)]
	private AnimationCurve _inputVelocityStabilization = AnimationCurve.EaseInOut(-1f, 0f, 1f, 0f);

	[SerializeField]
	[Tooltip("When Velocity is ignored the character will not try to catch up to the player and the character won't slide or fall.It is preferred to re-enable the movement by calling EnableMovement instead of setting this variable to false directly.")]
	private bool _velocityDisabled;

	[SerializeField]
	[Optional]
	[Min(-1f)]
	[Tooltip("If no ground is detected below this distance in meters on Start, it will disable the velocity to prevent falling. Negative numbers disable this behavior.")]
	private float _maxStartGroundDistance = 10f;

	[SerializeField]
	[Optional]
	private Context _context;

	private Func<float> _deltaTimeProvider = () => Time.deltaTime;

	private Func<float> _timeProvider = () => Time.time;

	protected Action<LocomotionEvent, Pose> _whenLocomotionEventHandled = delegate
	{
	};

	private Pose _accumulatedDeltaFrame;

	private Vector3 _velocity;

	private bool _isHeadInHotspot;

	private Vector3? _headHotspotCenter;

	private float _leftGroundTime;

	private bool _isRunning;

	private bool _isCrouching;

	private const float _sellionToTopOfHead = 0.1085f;

	private const float _sellionToBackOfHeadHalf = 0.0965f;

	private Queue<LocomotionEvent> _deferredLocomotionEvent = new Queue<LocomotionEvent>();

	private YieldInstruction _endOfFrame = new WaitForEndOfFrame();

	private Coroutine _endOfFrameRoutine;

	private bool _jumpThisFrame;

	private bool _endedFrameGrounded = true;

	protected bool _started;

	public float MaxWallPenetrationDistance
	{
		get
		{
			return _maxWallPenetrationDistance;
		}
		set
		{
			_maxWallPenetrationDistance = value;
		}
	}

	public float ExitHotspotDistance
	{
		get
		{
			return _exitHotspotDistance;
		}
		set
		{
			_exitHotspotDistance = value;
		}
	}

	public bool AutoUpdateHeight
	{
		get
		{
			return _autoUpdateHeight;
		}
		set
		{
			_autoUpdateHeight = value;
		}
	}

	public float DefaultHeight
	{
		get
		{
			return _defaultHeight;
		}
		set
		{
			_defaultHeight = value;
		}
	}

	public float HeightOffset
	{
		get
		{
			return _heightOffset;
		}
		set
		{
			_heightOffset = value;
		}
	}

	public float CrouchHeightOffset
	{
		get
		{
			return _crouchHeightOffset;
		}
		set
		{
			_crouchHeightOffset = value;
		}
	}

	public float SpeedFactor
	{
		get
		{
			return _speedFactor;
		}
		set
		{
			_speedFactor = value;
		}
	}

	public float CrouchSpeedFactor
	{
		get
		{
			return _crouchSpeedFactor;
		}
		set
		{
			_crouchSpeedFactor = value;
		}
	}

	public float RunningSpeedFactor
	{
		get
		{
			return _runningSpeedFactor;
		}
		set
		{
			_runningSpeedFactor = value;
		}
	}

	public float Acceleration
	{
		get
		{
			return _acceleration;
		}
		set
		{
			_acceleration = value;
		}
	}

	public float GroundDamping
	{
		get
		{
			return _groundDamping;
		}
		set
		{
			_groundDamping = value;
		}
	}

	public float JumpDamping
	{
		get
		{
			return _jumpDamping;
		}
		set
		{
			_jumpDamping = value;
		}
	}

	public float AirDamping
	{
		get
		{
			return _airDamping;
		}
		set
		{
			_airDamping = value;
		}
	}

	public float JumpForce
	{
		get
		{
			return _jumpForce;
		}
		set
		{
			_jumpForce = value;
		}
	}

	public float GravityFactor
	{
		get
		{
			return _gravityFactor;
		}
		set
		{
			_gravityFactor = value;
		}
	}

	public float CoyoteTime
	{
		get
		{
			return _coyoteTime;
		}
		set
		{
			_coyoteTime = value;
		}
	}

	public bool FlattenInputVelocity
	{
		get
		{
			return _flattenInputVelocity;
		}
		set
		{
			_flattenInputVelocity = value;
		}
	}

	public AnimationCurve InputVelocityStabilization
	{
		get
		{
			return _inputVelocityStabilization;
		}
		set
		{
			_inputVelocityStabilization = value;
		}
	}

	public bool IsGrounded => _characterController.IsGrounded;

	public bool IsRunning => _isRunning;

	public bool IsCrouching => _isCrouching;

	public bool IgnoringVelocity
	{
		get
		{
			if (!_velocityDisabled)
			{
				return _isHeadInHotspot;
			}
			return true;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	public event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled
	{
		add
		{
			_whenLocomotionEventHandled = (Action<LocomotionEvent, Pose>)Delegate.Combine(_whenLocomotionEventHandled, value);
		}
		remove
		{
			_whenLocomotionEventHandled = (Action<LocomotionEvent, Pose>)Delegate.Remove(_whenLocomotionEventHandled, value);
		}
	}

	public void SetDeltaTimeProvider(Func<float> deltaTimeProvider)
	{
		_deltaTimeProvider = deltaTimeProvider;
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (!_velocityDisabled && _maxStartGroundDistance >= 0f && !_characterController.TryGround(_maxStartGroundDistance))
		{
			DisableMovement();
		}
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_endOfFrameRoutine = StartCoroutine(EndOfFrameCoroutine());
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_accumulatedDeltaFrame = Pose.identity;
			StopCoroutine(_endOfFrameRoutine);
			_endOfFrameRoutine = null;
		}
	}

	protected virtual void Update()
	{
		TryExitHotspot();
		UpdateCharacterHeight();
		CatchUpCharacterToPlayer();
		if (!IgnoringVelocity)
		{
			UpdateVelocity();
			Pose from = _characterController.Pose;
			Vector3 delta = _velocity * _deltaTimeProvider();
			_characterController.Move(delta);
			Pose to = _characterController.Pose;
			AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
			if (_endedFrameGrounded && !IsGrounded)
			{
				_leftGroundTime = _timeProvider();
			}
		}
	}

	protected virtual void LateUpdate()
	{
		ConsumeDeferredLocomotionEvents();
	}

	protected virtual void LastUpdate()
	{
		CatchUpPlayerToCharacter(_accumulatedDeltaFrame, GetCharacterFeet().y);
		_accumulatedDeltaFrame = Pose.identity;
		if (!_jumpThisFrame)
		{
			_endedFrameGrounded = IsGrounded;
		}
		_jumpThisFrame = false;
	}

	public void Jump()
	{
		bool flag = _coyoteTime > 0f && _timeProvider() - _leftGroundTime <= _coyoteTime;
		if (!IsGrounded && !flag)
		{
			return;
		}
		if (_isCrouching)
		{
			Crouch(crouch: false);
			return;
		}
		TryExitHotspot(force: true);
		if (flag && _velocity.y < 0f)
		{
			_velocity.y = 0f;
		}
		_velocity += Vector3.up * _jumpForce;
		_leftGroundTime = 0f;
		_endedFrameGrounded = false;
		_jumpThisFrame = true;
	}

	public void ToggleCrouch()
	{
		Crouch(!_isCrouching);
	}

	public void Crouch(bool crouch)
	{
		if (_isCrouching != crouch)
		{
			_isCrouching = crouch;
		}
	}

	public void ToggleRun()
	{
		Run(!_isRunning);
	}

	public void Run(bool run)
	{
		if (_isRunning != run)
		{
			_isRunning = run;
			TryExitHotspot(force: true);
		}
	}

	public void DisableMovement()
	{
		_velocityDisabled = true;
	}

	public void EnableMovement()
	{
		if (IgnoringVelocity)
		{
			_velocityDisabled = false;
			_isHeadInHotspot = false;
			_headHotspotCenter = null;
			_velocity = Vector3.zero;
			_characterController.TryGround();
		}
	}

	public void ResetPlayerToCharacter()
	{
		Pose pose = _characterController.Pose;
		Vector3 characterFeet = GetCharacterFeet();
		Vector3 vector = _playerOrigin.position - GetPlayerHead();
		vector.y = 0f;
		_playerOrigin.position = characterFeet + vector;
		_accumulatedDeltaFrame = Pose.identity;
		_characterController.SetPosition(pose.position);
		_characterController.SetRotation(pose.rotation);
	}

	public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Velocity)
		{
			Vector3 vector = locomotionEvent.Pose.position;
			if (_flattenInputVelocity)
			{
				Quaternion rotation = Quaternion.LookRotation(vector.normalized, locomotionEvent.Pose.up);
				vector = Vector3.ProjectOnPlane(FlattenForwardOffset(rotation) * vector, Vector3.up).normalized * vector.magnitude;
			}
			AddVelocity(vector);
			if (IsHeadFarFromPoint(GetCharacterHead(), _maxWallPenetrationDistance))
			{
				ResetPlayerToCharacter();
			}
			_whenLocomotionEventHandled(locomotionEvent, locomotionEvent.Pose);
		}
		else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.None && locomotionEvent.Rotation == LocomotionEvent.RotationType.None)
		{
			if (LocomotionActionsBroadcaster.TryGetLocomotionActions(locomotionEvent, out var action, _context))
			{
				TryPerformLocomotionActions(action);
			}
		}
		else
		{
			if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute || locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel || locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
			{
				_velocity = Vector3.zero;
			}
			_deferredLocomotionEvent.Enqueue(locomotionEvent);
		}
	}

	private void ConsumeDeferredLocomotionEvents()
	{
		if (_deferredLocomotionEvent.Count != 0)
		{
			Pose from = _characterController.Pose;
			while (_deferredLocomotionEvent.Count > 0)
			{
				LocomotionEvent locomotionEvent = _deferredLocomotionEvent.Dequeue();
				HandleDeferredLocomotionEvent(locomotionEvent);
			}
			Pose to = _characterController.Pose;
			AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
		}
	}

	private void HandleDeferredLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		Pose from = _characterController.Pose;
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute)
		{
			MoveAbsoluteFeet(locomotionEvent.Pose.position);
		}
		else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel)
		{
			MoveAbsoluteHead(locomotionEvent.Pose.position);
		}
		else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
		{
			MoveRelative(locomotionEvent.Pose.position);
		}
		if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Absolute)
		{
			RotateAbsolute(locomotionEvent.Pose.rotation);
		}
		else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Relative)
		{
			RotateRelative(locomotionEvent.Pose.rotation);
		}
		else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Velocity)
		{
			RotateVelocity(locomotionEvent.Pose.rotation);
		}
		Pose to = _characterController.Pose;
		Pose accumulator = Pose.identity;
		AccumulateDelta(ref accumulator, in from, in to);
		_whenLocomotionEventHandled(locomotionEvent, accumulator);
	}

	private bool TryPerformLocomotionActions(LocomotionActionsBroadcaster.LocomotionAction action)
	{
		switch (action)
		{
		case LocomotionActionsBroadcaster.LocomotionAction.Crouch:
			Crouch(crouch: true);
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.StandUp:
			Crouch(crouch: false);
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.ToggleCrouch:
			ToggleCrouch();
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.Run:
			Run(run: true);
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.Walk:
			Run(run: false);
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.ToggleRun:
			ToggleRun();
			return true;
		case LocomotionActionsBroadcaster.LocomotionAction.Jump:
			Jump();
			return true;
		default:
			return false;
		}
	}

	private void AccumulateDelta(ref Pose accumulator, in Pose from, in Pose to)
	{
		accumulator.position = accumulator.position + to.position - from.position;
		accumulator.rotation = Quaternion.Inverse(from.rotation) * to.rotation * accumulator.rotation;
	}

	private void AddVelocity(Vector3 velocity)
	{
		TryExitHotspot(force: true);
		_velocity += velocity * GetModifiedSpeedFactor();
	}

	private void MoveAbsoluteFeet(Vector3 target)
	{
		TryExitHotspot(force: true);
		Vector3 characterFeet = GetCharacterFeet();
		Vector3 vector = target - characterFeet;
		Vector3 position = _characterController.Pose.position + vector;
		_characterController.SetPosition(position);
		_characterController.TryGround(_characterController.MaxStep);
	}

	private void MoveAbsoluteHead(Vector3 target)
	{
		Vector3 characterHead = GetCharacterHead();
		Vector3 vector = target - characterHead;
		Vector3 position = _characterController.Pose.position + vector;
		_characterController.SetPosition(position);
		_isHeadInHotspot = true;
		_headHotspotCenter = GetCharacterHead();
	}

	private void MoveRelative(Vector3 offset)
	{
		if (_characterController.IsGrounded)
		{
			TryExitHotspot(force: true);
			_velocity = Vector3.zero;
			_characterController.Move(offset);
		}
	}

	private void RotateAbsolute(Quaternion target)
	{
		_characterController.SetRotation(target);
	}

	private void RotateRelative(Quaternion target)
	{
		target *= _characterController.Pose.rotation;
		_characterController.SetRotation(target);
	}

	private void RotateVelocity(Quaternion target)
	{
		target.ToAngleAxis(out var angle, out var axis);
		angle *= _deltaTimeProvider();
		target = Quaternion.AngleAxis(angle, axis) * _characterController.Pose.rotation;
		_characterController.SetRotation(target);
	}

	private bool TryExitHotspot(bool force = false)
	{
		if (_isHeadInHotspot && _headHotspotCenter.HasValue && (force || IsHeadFarFromPoint(_headHotspotCenter.Value, _exitHotspotDistance)))
		{
			_isHeadInHotspot = false;
			_headHotspotCenter = null;
			_velocity = Vector3.zero;
			_characterController.TryGround();
			return true;
		}
		return false;
	}

	private void UpdateCharacterHeight()
	{
		float desiredHeight = Mathf.Max(_heightOffset + (_isCrouching ? _crouchHeightOffset : 0f) + (_autoUpdateHeight ? (GetPlayerHeadTop().y - _playerOrigin.position.y) : _defaultHeight), _characterController.Radius * 2f);
		_characterController.TrySetHeight(desiredHeight);
	}

	private void CatchUpCharacterToPlayer()
	{
		Vector3 delta = Vector3.ProjectOnPlane(GetPlayerHead() - _characterController.Pose.position, Vector3.up);
		Vector3 forward = Vector3.ProjectOnPlane(_playerEyes.forward, Vector3.up);
		_characterController.Move(delta);
		_characterController.SetRotation(Quaternion.LookRotation(forward, Vector3.up));
	}

	private void CatchUpPlayerToCharacter(Pose delta, float feetHeight)
	{
		Pose pose = _characterController.Pose;
		Vector3 playerHead = GetPlayerHead();
		_playerOrigin.rotation = delta.rotation * _playerOrigin.rotation;
		_playerOrigin.position = _playerOrigin.position + playerHead - GetPlayerHead();
		Vector3 vector = Vector3.ProjectOnPlane(delta.position, Vector3.up);
		Vector3 position = _playerOrigin.position + vector;
		position.y = feetHeight + (_isCrouching ? _crouchHeightOffset : 0f) + _heightOffset;
		_playerOrigin.position = position;
		_characterController.SetPosition(pose.position);
		_characterController.SetRotation(pose.rotation);
	}

	private void UpdateVelocity()
	{
		float num = _deltaTimeProvider();
		if (IsGrounded && _velocity.y <= 0f)
		{
			_velocity *= 1f / (1f + _groundDamping * num);
			_velocity.y = 0f;
			return;
		}
		float num2 = 1f / (1f + _airDamping * num);
		_velocity.x *= num2;
		_velocity.z *= num2;
		if (_velocity.y > 0f)
		{
			_velocity.y *= 1f / (1f + _jumpDamping * num);
		}
		_velocity += Physics.gravity * _gravityFactor * num;
	}

	private float GetModifiedSpeedFactor()
	{
		if (!IsGrounded || _velocity.y > 0f)
		{
			return 0f;
		}
		return _acceleration * (_isCrouching ? _crouchSpeedFactor : (_isRunning ? _runningSpeedFactor : _speedFactor)) * _deltaTimeProvider();
	}

	private Quaternion FlattenForwardOffset(Quaternion rotation)
	{
		Vector3 vector = rotation * Vector3.forward;
		Vector3 vector2 = rotation * Vector3.up;
		Vector3 up = Vector3.up;
		float time = Vector3.Dot(vector, up);
		float f = _inputVelocityStabilization.Evaluate(time);
		vector = Vector3.Slerp(vector, vector2 * (0f - Mathf.Sign(f)), Mathf.Abs(f));
		vector = Vector3.ProjectOnPlane(vector, up).normalized;
		return Quaternion.FromToRotation(rotation * Vector3.forward, vector);
	}

	private Vector3 GetCharacterFeet()
	{
		return _characterController.Pose.position + Vector3.down * (_characterController.Height * 0.5f + _characterController.SkinWidth);
	}

	private Vector3 GetCharacterHead()
	{
		return _characterController.Pose.position + Vector3.up * (_characterController.Height * 0.5f - 0.1085f + _characterController.SkinWidth);
	}

	private Vector3 GetPlayerHead()
	{
		return _playerEyes.position - _playerEyes.forward * 0.0965f;
	}

	private Vector3 GetPlayerHeadTop()
	{
		return GetPlayerHead() + Vector3.up * 0.1085f;
	}

	private bool IsHeadFarFromPoint(Vector3 point, float maxDistance)
	{
		return Vector3.ProjectOnPlane(GetPlayerHead() - point, Vector3.up).sqrMagnitude >= maxDistance * maxDistance;
	}

	private IEnumerator EndOfFrameCoroutine()
	{
		while (true)
		{
			yield return _endOfFrame;
			LastUpdate();
		}
	}

	public void InjectAllFirstPersonLocomotor(CharacterController characterController, Transform playerEyes, Transform playerOrigin)
	{
		InjectCharacterController(characterController);
		InjectPlayerEyes(playerEyes);
		InjectPlayerOrigin(playerOrigin);
	}

	public void InjectCharacterController(CharacterController characterController)
	{
		_characterController = characterController;
	}

	public void InjectPlayerEyes(Transform playerEyes)
	{
		_playerEyes = playerEyes;
	}

	public void InjectPlayerOrigin(Transform playerOrigin)
	{
		_playerOrigin = playerOrigin;
	}

	public void InjectOptionalMaxStartGroundDistance(float maxStartGroundDistance)
	{
		_maxStartGroundDistance = maxStartGroundDistance;
	}

	public void InjectOptionalContext(Context context)
	{
		_context = context;
	}
}
