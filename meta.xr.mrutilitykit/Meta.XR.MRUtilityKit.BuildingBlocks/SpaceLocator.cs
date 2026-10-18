using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.XR.MRUtilityKit.BuildingBlocks;

public abstract class SpaceLocator : MonoBehaviour
{
	[Flags]
	public enum SurfaceOrientation
	{
		None = 0,
		Any = 1,
		Vertical = 2,
		HorizontalFaceUp = 4,
		HorizontalFaceDown = 8
	}

	public SurfaceOrientation PreferredSurfaceOrientation = SurfaceOrientation.Vertical | SurfaceOrientation.HorizontalFaceUp | SurfaceOrientation.HorizontalFaceDown;

	[Tooltip("Use CustomSize instead of local scale of Target")]
	[SerializeField]
	public bool UseCustomSize;

	[Tooltip("Size the of the space to locate")]
	[SerializeField]
	public Vector3 CustomSize = Vector3.one * 0.25f;

	[Space]
	[Tooltip("This event will trigger when a suitable space is located within user's physical environment")]
	[Space]
	[SerializeField]
	private UnityEvent<Pose, bool> _onSpaceLocateCompleted = new UnityEvent<Pose, bool>();

	private EnvironmentRaycastManager _raycastManager;

	private EnvironmentRaycastHit _raycastHit;

	private const float VerticalSurfaceAngleThreshold = 0.3f;

	private const float HorizontalSurfaceAngleThreshold = 0.7f;

	private const float NormalConfidenceThreshold = 0.4f;

	private Vector3 _sizeToLocate;

	protected virtual Transform RaycastOrigin { get; set; }

	protected virtual float MaxRaycastDistance { get; set; } = 100f;


	public UnityEvent<Pose, bool> OnSpaceLocateCompleted
	{
		get
		{
			return _onSpaceLocateCompleted;
		}
		set
		{
			_onSpaceLocateCompleted = value;
		}
	}

	public EnvironmentRaycastHit RaycastHitResult => _raycastHit;

	private void Start()
	{
		_raycastManager = UnityEngine.Object.FindFirstObjectByType<EnvironmentRaycastManager>();
	}

	protected internal abstract Ray GetRaycastRay();

	protected virtual bool TryLocateSpace(out Pose surfacePose)
	{
		surfacePose = default(Pose);
		Ray raycastRay = GetRaycastRay();
		if (!(_raycastManager.Raycast(raycastRay, out var hit, MaxRaycastDistance) & (hit.normalConfidence > 0.4f)))
		{
			OnSpaceLocateCompleted?.Invoke(default(Pose), arg1: false);
			return false;
		}
		if (PreferredSurfaceOrientation != SurfaceOrientation.Any && (GetSurfaceOrientation(hit.normal) & PreferredSurfaceOrientation) == 0)
		{
			OnSpaceLocateCompleted?.Invoke(default(Pose), arg1: false);
			return false;
		}
		bool flag = TryCalculateSurfacePose(hit, raycastRay, out surfacePose);
		OnSpaceLocateCompleted?.Invoke(surfacePose, flag);
		return flag;
	}

	private bool TryCalculateSurfacePose(EnvironmentRaycastHit hit, Ray ray, out Pose surfacePose)
	{
		surfacePose = default(Pose);
		Vector3 upwards = CalculateUpwardFromPlacementSide(hit, base.transform, ray);
		Vector3 vector = Utilities.GetPrefabBounds(base.transform.gameObject)?.size ?? (Vector3.one * 0.05f);
		Vector3 boxSize = (UseCustomSize ? CustomSize : vector);
		if (!_raycastManager.PlaceBox(ray, boxSize, upwards, out _raycastHit))
		{
			return false;
		}
		Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(_raycastHit.normal, Vector3.Cross(base.transform.forward, _raycastHit.normal).normalized), _raycastHit.normal);
		surfacePose = new Pose(_raycastHit.point, rotation);
		return true;
	}

	private Vector3 CalculateUpwardFromPlacementSide(EnvironmentRaycastHit hit, Transform rayOrigin, Ray ray)
	{
		if (!IsVertical(hit.normal))
		{
			return Vector3.ProjectOnPlane(rayOrigin.up, Vector3.Cross(ray.direction, Vector3.up));
		}
		return Vector3.up;
	}

	private static bool IsVertical(Vector3 normal)
	{
		return Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.3f;
	}

	private static bool IsHorizontalDown(Vector3 normal)
	{
		return Vector3.Dot(normal, Vector3.down) > 0.7f;
	}

	private static bool IsHorizontalUp(Vector3 normal)
	{
		return Vector3.Dot(normal, Vector3.up) > 0.7f;
	}

	private static SurfaceOrientation GetSurfaceOrientation(Vector3 normal)
	{
		SurfaceOrientation result = SurfaceOrientation.None;
		if (IsHorizontalDown(normal))
		{
			result = SurfaceOrientation.HorizontalFaceDown;
		}
		else if (IsHorizontalUp(normal))
		{
			result = SurfaceOrientation.HorizontalFaceUp;
		}
		else if (IsVertical(normal))
		{
			result = SurfaceOrientation.Vertical;
		}
		return result;
	}
}
