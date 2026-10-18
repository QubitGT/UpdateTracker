using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

[Obsolete("Use FirstPersonLocomotor instead")]
public class CapsuleLocomotionHandler : MonoBehaviour, ILocomotionEventHandler, IDeltaTimeConsumer
{
	[Header("Character")]
	[SerializeField]
	[Tooltip("Capsule collider that represents the character and will be moved by the locomotor.")]
	private CapsuleCollider _capsule;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Extra offset added to the radius of the capsule for soft collisions.")]
	private float _skinWidth = 0.02f;

	[SerializeField]
	[Tooltip("LayerMask check for collisions when moving.")]
	private LayerMask _layerMask = -1;

	[Header("VR Player (Optional)")]
	[SerializeField]
	[Optional]
	[Tooltip("Optional. Root of the actual VR player so it can be sync with with capsule. If you provided a _playerEyes you must also provide a _playerOrigin.")]
	private Transform _playerOrigin;

	[SerializeField]
	[Optional]
	[Tooltip("Optional. Eyes of the actual VR player so it can be sync with the capsule. If you provided a _playerOrigin you must also provide a _playerEyes.")]
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
	[Range(0f, 90f)]
	[Tooltip("Max climbable slope angle in degrees.")]
	private float _maxSlopeAngle = 45f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Max climbable height for steps.")]
	private float _maxStep = 0.1f;

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
	private float _speedFactor = 1f;

	[SerializeField]
	[Tooltip("Speed multiplier applied while crouching.")]
	private float _crouchSpeedFactor = 0.5f;

	[SerializeField]
	[Tooltip("Speed multiplier applied while running.")]
	private float _runningSpeedFactor = 2f;

	[SerializeField]
	[Tooltip("The rate of acceleration during movement.")]
	private float _acceleration = 70f;

	[SerializeField]
	[Tooltip("The rate of damping on movement while grounded.")]
	private float _groundDamping = 30f;

	[SerializeField]
	[Tooltip("The rate of damping on the vertical movement while jumping.")]
	private float _jumpDamping = 30f;

	[SerializeField]
	[Tooltip("The rate of damping on the horizontal movement while in the air.")]
	private float _airDamping = 5f;

	[SerializeField]
	[Tooltip("The force applied to the character when jumping.")]
	private float _jumpForce = 100f;

	[SerializeField]
	[Tooltip("Modifies the strength of gravity.")]
	private float _gravityFactor = 1f;

	[SerializeField]
	[Min(1f)]
	[Tooltip("Max iterations for sliding the delta movement after colliding with an obstacle.")]
	private int _maxReboundSteps = 3;

	[SerializeField]
	[Tooltip("When Velocity is ignored the character will not try to catch up to the player and the character won't slide or fall.It is preferred to re-enable the movement by calling EnableMovement instead of setting this variable to false directly.")]
	private bool _velocityDisabled;

	[Header("Anchors")]
	[SerializeField]
	[Optional]
	[Tooltip("Optional. This transform pose will be updated with the pose of the character head.")]
	private Transform _logicalHead;

	[SerializeField]
	[Optional]
	[Tooltip("Optional. This transform pose will be updated with the pose of the character feet.")]
	private Transform _logicalFeet;

	private Func<float> _deltaTimeProvider = () => Time.deltaTime;

	protected Action<LocomotionEvent, Pose> _whenLocomotionEventHandled = delegate
	{
	};

	private Pose _accumulatedDeltaFrame;

	private Vector3 _velocity;

	private bool _isHeadInHotspot;

	private Vector3? _headHotspotCenter;

	private RaycastHit _groundHit;

	private bool _isGrounded;

	private bool _isRunning;

	private bool _isCrouching;

	private const float _sellionToTopOfHead = 0.1085f;

	private const float _sellionToBackOfHeadHalf = 0.0965f;

	private const float _cornerHitEpsilon = 0.001f;

	private Queue<LocomotionEvent> _deferredLocomotionEvent = new Queue<LocomotionEvent>();

	private YieldInstruction _endOfFrame = new WaitForEndOfFrame();

	private Coroutine _endOfFrameRoutine;

	protected bool _started;

	public float SkinWidth
	{
		get
		{
			return _skinWidth;
		}
		set
		{
			_skinWidth = value;
		}
	}

	public LayerMask LayerMask
	{
		get
		{
			return _layerMask;
		}
		set
		{
			_layerMask = value;
		}
	}

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

	public float MaxSlopeAngle
	{
		get
		{
			return _maxSlopeAngle;
		}
		set
		{
			_maxSlopeAngle = value;
		}
	}

	public float MaxStep
	{
		get
		{
			return _maxStep;
		}
		set
		{
			_maxStep = value;
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

	public int MaxReboundSteps
	{
		get
		{
			return _maxReboundSteps;
		}
		set
		{
			_maxReboundSteps = value;
		}
	}

	public bool IsGrounded => _isGrounded;

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

	private bool ControllingPlayer
	{
		get
		{
			if (_playerOrigin != null)
			{
				return _playerEyes != null;
			}
			return false;
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

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (!(_playerOrigin != null))
		{
			_ = _playerEyes != null;
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
		if (!IgnoringVelocity)
		{
			CatchUpCharacterToPlayer();
			UpdateVelocity();
			Pose from = _capsule.transform.GetPose();
			Vector3 delta = _velocity * _deltaTimeProvider();
			MoveCharacter(delta);
			Pose to = _capsule.transform.GetPose();
			AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
		}
		UpdateAnchorPoints();
	}

	protected virtual void LateUpdate()
	{
		ConsumeDeferredLocomotionEvents();
	}

	protected virtual void LastUpdate()
	{
		CatchUpPlayerToCharacter(_accumulatedDeltaFrame, GetCharacterFeet().y);
		_accumulatedDeltaFrame = Pose.identity;
	}

	public void Jump()
	{
		if (_isGrounded)
		{
			if (_isCrouching)
			{
				Crouch(crouch: false);
				return;
			}
			TryExitHotspot(force: true);
			_velocity += Vector3.up * _jumpForce;
		}
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

	public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Velocity)
		{
			AddVelocity(locomotionEvent.Pose.position);
			if (IsHeadFarFromPoint(GetCharacterHead(), _maxWallPenetrationDistance))
			{
				ResetPlayerToCharacter();
			}
			_whenLocomotionEventHandled(locomotionEvent, locomotionEvent.Pose);
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
			Pose from = _capsule.transform.GetPose();
			while (_deferredLocomotionEvent.Count > 0)
			{
				LocomotionEvent locomotionEvent = _deferredLocomotionEvent.Dequeue();
				HandleDeferredLocomotionEvent(locomotionEvent);
			}
			Pose to = _capsule.transform.GetPose();
			AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
		}
	}

	private void HandleDeferredLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		Pose from = _capsule.transform.GetPose();
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
		Pose to = _capsule.transform.GetPose();
		Pose accumulator = Pose.identity;
		AccumulateDelta(ref accumulator, in from, in to);
		_whenLocomotionEventHandled(locomotionEvent, accumulator);
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
		_capsule.transform.position += vector;
		if (CheckMoveCharacter(Vector3.down * _maxStep, out var movement))
		{
			_capsule.transform.position += movement;
			UpdateGrounded(forceGrounded: true);
		}
		else
		{
			UpdateGrounded();
		}
	}

	private void MoveAbsoluteHead(Vector3 target)
	{
		Vector3 characterHead = GetCharacterHead();
		Vector3 vector = target - characterHead;
		_capsule.transform.position += vector;
		_isHeadInHotspot = true;
		_headHotspotCenter = GetCharacterHead();
	}

	private void MoveRelative(Vector3 offset)
	{
		if (_isGrounded)
		{
			TryExitHotspot(force: true);
			_velocity = Vector3.zero;
			MoveCharacter(offset);
		}
	}

	private void RotateAbsolute(Quaternion target)
	{
		_capsule.transform.rotation = target;
	}

	private void RotateRelative(Quaternion target)
	{
		_capsule.transform.rotation = target * _capsule.transform.rotation;
	}

	private void RotateVelocity(Quaternion target)
	{
		target.ToAngleAxis(out var angle, out var axis);
		angle *= _deltaTimeProvider();
		_capsule.transform.rotation = Quaternion.AngleAxis(angle, axis) * _capsule.transform.rotation;
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
			if (CalculateGround(out var groundHit) && IsFlat(groundHit.normal))
			{
				Vector3 position = _capsule.transform.position;
				RaycastHitPlane(groundHit, position, Vector3.down, out var enter);
				position.y = position.y - enter + _capsule.height * 0.5f + _skinWidth;
				_capsule.transform.position = position;
			}
		}
	}

	private bool TryExitHotspot(bool force = false)
	{
		if (_isHeadInHotspot && _headHotspotCenter.HasValue && (force || IsHeadFarFromPoint(_headHotspotCenter.Value, _exitHotspotDistance)))
		{
			EnableMovement();
			return true;
		}
		return false;
	}

	private void UpdateCharacterHeight()
	{
		float height = _capsule.height;
		float a = _heightOffset + (_isCrouching ? _crouchHeightOffset : 0f) + ((ControllingPlayer && _autoUpdateHeight) ? (GetPlayerHeadTop().y - _playerOrigin.position.y) : _defaultHeight);
		float skinWidth = _skinWidth;
		float num = Mathf.Max(a, _capsule.radius * 2f) - height;
		if (num > skinWidth && CheckMoveCharacter(Vector3.up * num, out var movement))
		{
			num = Mathf.Max(0f, movement.y - _skinWidth);
		}
		if (!(Mathf.Abs(num) <= skinWidth))
		{
			_capsule.height = height + num;
			_capsule.transform.position += Vector3.up * num * 0.5f;
		}
	}

	private void CatchUpCharacterToPlayer()
	{
		if (ControllingPlayer)
		{
			Vector3 delta = Vector3.ProjectOnPlane(GetPlayerHead() - _capsule.transform.position, Vector3.up);
			MoveCharacter(delta);
			Vector3 forward = Vector3.ProjectOnPlane(_playerEyes.forward, Vector3.up);
			_capsule.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
	}

	private void CatchUpPlayerToCharacter(Pose delta, float feetHeight)
	{
		if (ControllingPlayer)
		{
			Pose pose = _capsule.transform.GetPose();
			Vector3 playerHead = GetPlayerHead();
			_playerOrigin.rotation = delta.rotation * _playerOrigin.rotation;
			_playerOrigin.position = _playerOrigin.position + playerHead - GetPlayerHead();
			Vector3 vector = Vector3.ProjectOnPlane(delta.position, Vector3.up);
			Vector3 position = _playerOrigin.position + vector;
			position.y = feetHeight + (_isCrouching ? _crouchHeightOffset : 0f) + _heightOffset;
			_playerOrigin.position = position;
			_capsule.transform.SetPose(in pose);
		}
	}

	public void ResetPlayerToCharacter()
	{
		if (ControllingPlayer)
		{
			Pose pose = _capsule.transform.GetPose();
			Vector3 characterFeet = GetCharacterFeet();
			Vector3 vector = _playerOrigin.position - GetPlayerHead();
			vector.y = 0f;
			_playerOrigin.position = characterFeet + vector;
			_accumulatedDeltaFrame = Pose.identity;
			_capsule.transform.SetPose(in pose);
		}
	}

	private void UpdateVelocity()
	{
		float num = _deltaTimeProvider();
		if (_isGrounded && _velocity.y <= 0f)
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

	private void MoveCharacter(Vector3 delta)
	{
		if (_isGrounded)
		{
			delta = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(delta, Vector3.up), _groundHit.normal) + Vector3.up * delta.y;
		}
		Vector3 vector = Rebound(delta, _maxReboundSteps);
		_capsule.transform.position += vector;
		UpdateGrounded(delta.y < 0f && delta.y < vector.y && Mathf.Abs(vector.y) < 0.001f);
	}

	private Vector3 Rebound(Vector3 delta, int bounces)
	{
		Vector3 vector = Vector3.up * Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
		Vector3 capsuleTop2 = _capsule.transform.position + vector;
		Vector3 capsuleBase2 = _capsule.transform.position - vector;
		Vector3 originalFlatDelta2 = Vector3.ProjectOnPlane(delta, Vector3.up);
		return ReboundRecursive(capsuleBase2, capsuleTop2, _capsule.radius, delta, originalFlatDelta2, bounces);
		Vector3 ReboundRecursive(Vector3 capsuleBase, Vector3 capsuleTop, float radius, Vector3 delta, Vector3 originalFlatDelta, int bounceStep)
		{
			if (bounceStep <= 0 || Mathf.Approximately(delta.sqrMagnitude, 0f))
			{
				return Vector3.zero;
			}
			Vector3 zero = Vector3.zero;
			Vector3 delta2 = Vector3.zero;
			RaycastHit? moveHit = null;
			RaycastHit? stepHit = null;
			if (MoveCapsuleCollides(capsuleBase, capsuleTop, radius, delta, out moveHit))
			{
				(delta, delta2) = DecomposeDelta(delta, moveHit.Value);
			}
			capsuleBase += delta;
			capsuleTop += delta;
			zero += delta;
			if (_isGrounded && moveHit.HasValue && moveHit.Value.point.y - (capsuleBase.y - radius - _skinWidth) <= _maxStep && ClimbStep(capsuleBase, capsuleTop, radius, delta2, out var climbDelta, out stepHit))
			{
				if (stepHit.HasValue)
				{
					delta2 = DecomposeDelta(delta2, stepHit.Value).Item2;
					delta2 = SlideDelta(delta2, originalFlatDelta, stepHit.Value);
				}
				else
				{
					delta2 = Vector3.zero;
				}
				capsuleBase += climbDelta;
				capsuleTop += climbDelta;
				zero += climbDelta;
			}
			if (moveHit.HasValue && !stepHit.HasValue)
			{
				delta2 = SlideDelta(delta2, originalFlatDelta, moveHit.Value);
			}
			return zero + ReboundRecursive(capsuleBase, capsuleTop, radius, delta2, originalFlatDelta, bounceStep - 1);
		}
	}

	private bool ClimbStep(Vector3 capsuleBase, Vector3 capsuleTop, float radius, Vector3 delta, out Vector3 climbDelta, out RaycastHit? stepHit)
	{
		stepHit = null;
		climbDelta = Vector3.zero;
		float num = Mathf.Min(_maxStep, capsuleTop.y - capsuleBase.y);
		float num2 = Mathf.Max(0f, _maxStep - num);
		Vector3 vector = capsuleBase + Vector3.up * num;
		Vector3 capsuleTop2 = capsuleTop + Vector3.up * num2;
		if (MoveCapsuleCollides(vector, capsuleTop2, radius, delta, out var moveHit))
		{
			stepHit = moveHit;
			Vector3 vector2 = capsuleTop - capsuleBase;
			if (Mathf.Approximately(vector2.sqrMagnitude, 0f) || Mathf.Abs(Vector3.Dot(moveHit.Value.normal, vector2.normalized)) > 0.001f)
			{
				Vector3 vector3 = -moveHit.Value.normal;
				Ray ray = new Ray(moveHit.Value.point - vector3 * moveHit.Value.distance, vector3);
				if (moveHit.Value.collider.Raycast(ray, out var hitInfo, moveHit.Value.distance + 0.001f))
				{
					moveHit = hitInfo;
				}
			}
			delta = DecomposeDelta(delta, moveHit.Value).Item1;
		}
		if (CalculateGround(capsuleTop + delta, radius, _capsule.height - radius, out var groundHit) && RaycastSphere(groundHit.point, Vector3.up, vector + delta, radius + _skinWidth, out var distance) && groundHit.point.y - (capsuleBase.y - radius) <= _maxStep && IsFlat(groundHit.normal))
		{
			delta.y = Mathf.Max(delta.y, num - distance);
			Vector3 delta2 = Vector3.up * delta.y;
			if (MoveCapsuleCollides(capsuleBase, capsuleTop, radius, delta2, out var _))
			{
				return false;
			}
			climbDelta = delta;
			return true;
		}
		return false;
	}

	private bool CheckMoveCharacter(Vector3 delta, out Vector3 movement)
	{
		Vector3 vector = Vector3.up * Mathf.Max(0f, _capsule.height * 0.5f - _capsule.radius);
		Vector3 capsuleTop = _capsule.transform.position + vector;
		Vector3 capsuleBase = _capsule.transform.position - vector;
		float radius = _capsule.radius;
		if (MoveCapsuleCollides(capsuleBase, capsuleTop, radius, delta, out var moveHit))
		{
			delta = DecomposeDelta(delta, moveHit.Value).Item1;
			movement = delta;
			return true;
		}
		movement = Vector3.zero;
		return false;
	}

	private bool MoveCapsuleCollides(Vector3 capsuleBase, Vector3 capsuleTop, float radius, Vector3 delta, out RaycastHit? moveHit)
	{
		float sqrMagnitude = delta.sqrMagnitude;
		if (Mathf.Approximately(sqrMagnitude, 0f))
		{
			moveHit = null;
			return false;
		}
		float maxDistance = ((sqrMagnitude < _skinWidth * _skinWidth) ? _skinWidth : Mathf.Sqrt(sqrMagnitude));
		RaycastHit hitInfo;
		bool flag = Physics.CapsuleCast(capsuleBase, capsuleTop, radius, delta.normalized, out hitInfo, maxDistance, _layerMask.value, QueryTriggerInteraction.Ignore);
		moveHit = (flag ? new RaycastHit?(hitInfo) : null);
		return flag;
	}

	private (Vector3, Vector3) DecomposeDelta(Vector3 delta, RaycastHit hit)
	{
		Vector3 normalized = delta.normalized;
		float num = Mathf.Max(0f, Vector3.Dot(normalized, -hit.normal)) * _skinWidth;
		Vector3 vector = normalized * Mathf.Max(0f, hit.distance - num);
		Vector3 item = delta - vector;
		return (vector, item);
	}

	private Vector3 SlideDelta(Vector3 delta, Vector3 originalFlatDelta, RaycastHit hit)
	{
		Vector3 vector = hit.normal;
		if (!IsFlat(vector))
		{
			vector = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
		}
		Vector3 vector2 = Vector3.ProjectOnPlane(delta, Vector3.up);
		vector2 = Vector3.ProjectOnPlane(vector2, vector);
		if (Vector3.Dot(vector2, originalFlatDelta) <= 0f)
		{
			vector2 = Vector3.zero;
		}
		Vector3 vector3 = Vector3.up * delta.y;
		vector3 = Vector3.ProjectOnPlane(vector3, hit.normal);
		return vector2 + vector3;
	}

	private bool IsFlat(Vector3 groundNormal)
	{
		return Vector3.Angle(Vector3.up, groundNormal) <= _maxSlopeAngle;
	}

	private void UpdateGrounded(bool forceGrounded = false)
	{
		_isGrounded = CalculateGround(out _groundHit) && IsFlat(_groundHit.normal);
		if (!_isGrounded && forceGrounded)
		{
			_isGrounded = true;
			_groundHit.normal = Vector3.up;
			_groundHit.point = _capsule.transform.position + Vector3.down * (_capsule.height * 0.5f + _skinWidth);
		}
	}

	private bool CalculateGround(out RaycastHit groundHit)
	{
		Vector3 origin = _capsule.transform.position + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
		if (CalculateGround(origin, _capsule.radius + _skinWidth, _capsule.radius + _skinWidth, out groundHit))
		{
			return true;
		}
		return CalculateGround(_capsule.transform.position, _capsule.radius + _skinWidth, _capsule.height * 0.5f + _skinWidth, out groundHit);
	}

	private bool CalculateGround(Vector3 origin, float radius, float distance, out RaycastHit groundHit)
	{
		Vector3 down = Vector3.down;
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(origin, down, out hitInfo, distance, _layerMask.value, QueryTriggerInteraction.Ignore);
		RaycastHit hitInfo2;
		bool flag2 = Physics.SphereCast(origin, radius, down, out hitInfo2, distance - radius, _layerMask.value, QueryTriggerInteraction.Ignore);
		if (flag2 && Physics.Raycast(hitInfo2.point - down * 0.01f, down, out var hitInfo3, 0.011f, _layerMask.value, QueryTriggerInteraction.Ignore))
		{
			hitInfo2.normal = hitInfo3.normal;
		}
		if (flag2 && flag)
		{
			groundHit = ((hitInfo2.normal.y > hitInfo.normal.y) ? hitInfo2 : hitInfo);
			groundHit.distance = Vector3.Project(groundHit.point - origin, down).magnitude;
			return true;
		}
		if (flag2 || flag)
		{
			groundHit = (flag2 ? hitInfo2 : hitInfo);
			groundHit.normal = (flag ? hitInfo.normal : hitInfo2.normal);
			groundHit.distance = Vector3.Project(groundHit.point - origin, down).magnitude;
			return true;
		}
		groundHit = default(RaycastHit);
		return false;
	}

	private void UpdateAnchorPoints()
	{
		if (_logicalHead != null)
		{
			_logicalHead.transform.SetPositionAndRotation(GetCharacterHead(), _capsule.transform.rotation);
		}
		if (_logicalFeet != null)
		{
			_logicalFeet.transform.SetPositionAndRotation(GetCharacterFeet(), _capsule.transform.rotation);
		}
	}

	private float GetModifiedSpeedFactor()
	{
		if (!_isGrounded || _velocity.y > 0f)
		{
			return 0f;
		}
		return _acceleration * (_isCrouching ? _crouchSpeedFactor : (_isRunning ? _runningSpeedFactor : _speedFactor)) * _deltaTimeProvider();
	}

	private Vector3 GetCharacterFeet()
	{
		return _capsule.transform.position + Vector3.down * (_capsule.height * 0.5f + _skinWidth);
	}

	private Vector3 GetCharacterHead()
	{
		return _capsule.transform.position + Vector3.up * (_capsule.height * 0.5f - 0.1085f + _skinWidth);
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
		return Vector3.ProjectOnPlane((ControllingPlayer ? GetPlayerHead() : GetCharacterHead()) - point, Vector3.up).sqrMagnitude >= maxDistance * maxDistance;
	}

	private IEnumerator EndOfFrameCoroutine()
	{
		while (true)
		{
			yield return _endOfFrame;
			LastUpdate();
		}
	}

	private static bool RaycastSphere(Vector3 origin, Vector3 direction, Vector3 sphereCenter, float radius, out float distance)
	{
		distance = float.MaxValue;
		Vector3 vector = origin - sphereCenter;
		float num = Vector3.Dot(direction, direction);
		float num2 = 2f * Vector3.Dot(vector, direction);
		float num3 = Vector3.Dot(vector, vector) - radius * radius;
		float num4 = num2 * num2 - 4f * num * num3;
		if (num4 < 0f)
		{
			return false;
		}
		distance = (0f - num2 - (float)Math.Sqrt(num4)) / (2f * num);
		return true;
	}

	private bool RaycastHitPlane(RaycastHit hit, Vector3 origin, Vector3 direction, out float enter)
	{
		enter = 0f;
		float num = Vector3.Dot(hit.normal, hit.point) - Vector3.Dot(origin, hit.normal);
		float num2 = Vector3.Dot(direction, hit.normal);
		if (!Mathf.Approximately(num2, 0f))
		{
			enter = num / num2;
			return true;
		}
		return false;
	}

	public void InjectAllCapsuleLocomotionHandler(CapsuleCollider capsule)
	{
		InjectCapsule(capsule);
	}

	public void InjectCapsule(CapsuleCollider capsule)
	{
		_capsule = capsule;
	}

	public void InjectOptionalPlayerEyes(Transform playerEyes)
	{
		_playerEyes = playerEyes;
	}

	public void InjectOptionalPlayerOrigin(Transform playerOrigin)
	{
		_playerOrigin = playerOrigin;
	}
}
