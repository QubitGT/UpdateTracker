using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class WallPenetrationTunneling : MonoBehaviour
{
	[SerializeField]
	private Transform _trackedPosition;

	[SerializeField]
	private Transform _logicalPosition;

	[SerializeField]
	private TunnelingEffect _tunneling;

	[SerializeField]
	private AnimationCurve _penetrationFov;

	[SerializeField]
	private float _extraDistance = 0.22f;

	[SerializeField]
	[Min(1f)]
	private int _maxCollidersCheck = 5;

	[SerializeField]
	[Optional]
	private string _ignoreTag = "Player";

	[SerializeField]
	private LayerMask _layerMask = -1;

	private RaycastHit[] _hits;

	protected bool _started;

	public AnimationCurve PenetrationFov
	{
		get
		{
			return _penetrationFov;
		}
		set
		{
			_penetrationFov = value;
		}
	}

	public float ExtraDistance
	{
		get
		{
			return _extraDistance;
		}
		set
		{
			_extraDistance = value;
		}
	}

	public string IgnoreTag
	{
		get
		{
			return _ignoreTag;
		}
		set
		{
			_ignoreTag = value;
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

	protected virtual void Awake()
	{
		_hits = new RaycastHit[_maxCollidersCheck];
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void LateUpdate()
	{
		float distance;
		bool headBlocked = CalculatePenetration(out distance);
		UpdateTunneling(headBlocked, distance);
	}

	private bool CalculatePenetration(out float distance)
	{
		Vector3 position = _logicalPosition.position;
		Vector3 vector = _trackedPosition.position - position;
		float num = vector.magnitude + _extraDistance;
		int num2 = Physics.RaycastNonAlloc(new Ray(position, vector.normalized), _hits, num, _layerMask.value);
		if (num2 > 0)
		{
			if (string.IsNullOrEmpty(_ignoreTag))
			{
				distance = Mathf.Max(0f, num - _hits[0].distance);
				return true;
			}
			for (int i = 0; i < num2; i++)
			{
				if (_ignoreTag != _hits[i].collider.tag)
				{
					distance = Mathf.Max(0f, num - _hits[i].distance);
					return true;
				}
			}
		}
		distance = 0f;
		return false;
	}

	private void UpdateTunneling(bool headBlocked, float penetrationDistance)
	{
		float num = _penetrationFov.Evaluate(penetrationDistance);
		if (!headBlocked || num >= 360f)
		{
			_tunneling.enabled = false;
			_tunneling.UserFOV = 360f;
			return;
		}
		Vector3 normalized = (_logicalPosition.position - _trackedPosition.position).normalized;
		_tunneling.enabled = true;
		_tunneling.UseAimingTarget = true;
		_tunneling.AimingDirection = normalized;
		_tunneling.UserFOV = num;
	}

	public void InjectAllWallPenetrationTunneling(Transform trackedPosition, Transform logicalPosition, TunnelingEffect tunneling, int maxCollidersCheck)
	{
		InjectTrackedPosition(trackedPosition);
		InjectLogicalPosition(logicalPosition);
		InjectTunneling(tunneling);
		InjectMaxCollidersCheck(maxCollidersCheck);
	}

	public void InjectTrackedPosition(Transform trackedPosition)
	{
		_trackedPosition = trackedPosition;
	}

	public void InjectLogicalPosition(Transform logicalPosition)
	{
		_logicalPosition = logicalPosition;
	}

	public void InjectTunneling(TunnelingEffect tunneling)
	{
		_tunneling = tunneling;
	}

	public void InjectMaxCollidersCheck(int maxCollidersCheck)
	{
		_maxCollidersCheck = maxCollidersCheck;
	}
}
