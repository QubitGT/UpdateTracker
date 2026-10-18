using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class CharacterController : MonoBehaviour
{
	[Header("Character")]
	[SerializeField]
	[Tooltip("Capsule collider that represents the character and will be moved by the locomotor.")]
	private CapsuleCollider _capsule;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Extra offset added to the radius of the capsule for soft collisions.")]
	private float _skinWidth = 0.005f;

	[SerializeField]
	[Tooltip("LayerMask check for collisions when moving.")]
	private LayerMask _layerMask = -1;

	[SerializeField]
	[Range(0f, 90f)]
	[Tooltip("Max climbable slope angle in degrees.")]
	private float _maxSlopeAngle = 50f;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Max climbable height for steps.")]
	private float _maxStep = 0.3f;

	[SerializeField]
	[Min(1f)]
	[Tooltip("Max iterations for sliding the delta movement after colliding with an obstacle.")]
	private int _maxReboundSteps = 3;

	[Header("Anchors")]
	[SerializeField]
	[Optional]
	[Tooltip("Optional. This transform pose will be updated with the pose of the character top.")]
	private Transform _headAnchor;

	[SerializeField]
	[Optional]
	[Tooltip("Optional. This transform pose will be updated with the pose of the character base.")]
	private Transform _feetAnchor;

	protected RaycastHit _groundHit;

	protected bool _isGrounded;

	protected bool _started;

	private const float _cornerHitEpsilon = 0.001f;

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

	public float Height => _capsule.height;

	public float Radius => _capsule.radius;

	public Pose Pose => _capsule.transform.GetPose();

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			UpdateAnchorPoints();
		}
	}

	public bool TrySetHeight(float desiredHeight)
	{
		float height = _capsule.height;
		float skinWidth = _skinWidth;
		float num = desiredHeight - height;
		if (num > skinWidth && CheckMoveCharacter(Vector3.up * num, out var movement))
		{
			num = Mathf.Max(0f, movement.y - _skinWidth);
		}
		if (Mathf.Abs(num) <= skinWidth)
		{
			return false;
		}
		_capsule.height = height + num;
		_capsule.transform.position += Vector3.up * num * 0.5f;
		UpdateAnchorPoints();
		return true;
	}

	public bool TryGround(float extraDistance = 0f)
	{
		if (CalculateGround(out var groundHit, extraDistance) && IsFlat(groundHit.normal))
		{
			Vector3 position = _capsule.transform.position;
			RaycastHitPlane(groundHit, position, Vector3.down, out var enter);
			position.y = position.y - enter + _capsule.height * 0.5f + _skinWidth;
			_capsule.transform.position = position;
			_groundHit = groundHit;
			_isGrounded = true;
			UpdateAnchorPoints();
			return true;
		}
		return false;
	}

	public void SetRotation(Quaternion rotation)
	{
		_capsule.transform.rotation = rotation;
		UpdateAnchorPoints();
	}

	public void SetPosition(Vector3 position)
	{
		_capsule.transform.position = position;
		UpdateAnchorPoints();
	}

	public void Move(Vector3 delta)
	{
		if (_isGrounded)
		{
			delta = Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(delta, Vector3.up), _groundHit.normal) + Vector3.up * delta.y;
		}
		Vector3 vector = Rebound(delta, _maxReboundSteps);
		_capsule.transform.position += vector;
		UpdateGrounded(delta.y < 0f && delta.y < vector.y && Mathf.Abs(vector.y) < 0.001f);
		UpdateAnchorPoints();
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
			if (_isGrounded && _maxStep > 0f && moveHit.HasValue && moveHit.Value.point.y - (capsuleBase.y - radius - _skinWidth) <= _maxStep && ClimbStep(capsuleBase, capsuleTop, radius, delta2, out var climbDelta, out stepHit))
			{
				capsuleBase += climbDelta;
				capsuleTop += climbDelta;
				zero += climbDelta;
				if (stepHit.HasValue)
				{
					delta2 = DecomposeDelta(climbDelta, stepHit.Value).Item2;
					delta2 = SlideDelta(delta2, originalFlatDelta, stepHit.Value);
				}
				else
				{
					delta2 = Vector3.zero;
				}
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
		delta = Vector3.ProjectOnPlane(delta, Vector3.up);
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
		float num = Mathf.Max(0.1f, Vector3.Dot(normalized, -hit.normal)) * _skinWidth;
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

	private bool CalculateGround(out RaycastHit groundHit, float extraDistance = 0f)
	{
		Vector3 origin = _capsule.transform.position + Vector3.down * (_capsule.height * 0.5f - _capsule.radius);
		if (CalculateGround(origin, _capsule.radius + _skinWidth, _capsule.radius + _skinWidth + extraDistance, out groundHit))
		{
			return true;
		}
		return CalculateGround(_capsule.transform.position, _capsule.radius + _skinWidth, _capsule.height * 0.5f + _skinWidth + extraDistance, out groundHit);
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
		Vector3 vector = Vector3.up * Mathf.Max(0f, _capsule.height * 0.5f + _skinWidth);
		Vector3 position = _capsule.transform.position + vector;
		Vector3 position2 = _capsule.transform.position - vector;
		Quaternion rotation = _capsule.transform.rotation;
		if (_headAnchor != null)
		{
			_headAnchor.transform.SetPositionAndRotation(position, rotation);
		}
		if (_feetAnchor != null)
		{
			_feetAnchor.transform.SetPositionAndRotation(position2, rotation);
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

	public void InjectAllCharacterController(CapsuleCollider capsule)
	{
		InjectCapsule(capsule);
	}

	public void InjectCapsule(CapsuleCollider capsule)
	{
		_capsule = capsule;
	}

	public void InjectOptionalFeetAnchor(Transform feetAnchor)
	{
		_feetAnchor = feetAnchor;
	}

	public void InjectOptionalHeadAnchor(Transform headAnchor)
	{
		_headAnchor = headAnchor;
	}
}
