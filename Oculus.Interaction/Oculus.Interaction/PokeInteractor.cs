using System;
using System.Collections.Generic;
using Oculus.Interaction.Surfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class PokeInteractor : PointerInteractor<PokeInteractor, PokeInteractable>, ITimeConsumer
{
	private class SurfaceHitCache
	{
		private readonly struct HitInfo
		{
			public readonly bool IsValid;

			public readonly SurfaceHit Hit;

			public HitInfo(bool isValid, SurfaceHit hit)
			{
				IsValid = isValid;
				Hit = hit;
			}
		}

		private Dictionary<PokeInteractable, HitInfo> _surfacePatchHitCache;

		private Dictionary<PokeInteractable, HitInfo> _backingSurfaceHitCache;

		private Vector3 _origin;

		public bool GetPatchHit(PokeInteractable interactable, out SurfaceHit hit)
		{
			if (!_surfacePatchHitCache.ContainsKey(interactable))
			{
				SurfaceHit hit2;
				bool isValid = interactable.SurfacePatch.ClosestSurfacePoint(in _origin, out hit2);
				HitInfo value = new HitInfo(isValid, hit2);
				_surfacePatchHitCache.Add(interactable, value);
			}
			hit = _surfacePatchHitCache[interactable].Hit;
			return _surfacePatchHitCache[interactable].IsValid;
		}

		public bool GetBackingHit(PokeInteractable interactable, out SurfaceHit hit)
		{
			if (!_backingSurfaceHitCache.ContainsKey(interactable))
			{
				SurfaceHit hit2;
				bool isValid = interactable.SurfacePatch.BackingSurface.ClosestSurfacePoint(in _origin, out hit2);
				HitInfo value = new HitInfo(isValid, hit2);
				_backingSurfaceHitCache.Add(interactable, value);
			}
			hit = _backingSurfaceHitCache[interactable].Hit;
			return _backingSurfaceHitCache[interactable].IsValid;
		}

		public SurfaceHitCache()
		{
			_surfacePatchHitCache = new Dictionary<PokeInteractable, HitInfo>();
			_backingSurfaceHitCache = new Dictionary<PokeInteractable, HitInfo>();
		}

		public void Reset(Vector3 origin)
		{
			_origin = origin;
			_surfacePatchHitCache.Clear();
			_backingSurfaceHitCache.Clear();
		}
	}

	private struct CachedInteractable
	{
		public PokeInteractable interactable;

		public SurfaceHit backingHit;

		public SurfaceHit patchHit;
	}

	[SerializeField]
	[Tooltip("The poke origin tracks the provided transform.")]
	private Transform _pointTransform;

	[SerializeField]
	[Tooltip("(Meters, World) The radius of the sphere positioned at the origin.")]
	private float _radius = 0.005f;

	[SerializeField]
	[Tooltip("(Meters, World) A poke unselect fires when the poke origin surpasses this distance above a surface.")]
	private float _touchReleaseThreshold = 0.002f;

	[FormerlySerializedAs("_zThreshold")]
	[SerializeField]
	[Tooltip("(Meters, World) The threshold below which distances to a surface will use tiebreaker score to decide candidate.")]
	private float _equalDistanceThreshold = 0.001f;

	private Vector3 _previousPokeOrigin;

	private PokeInteractable _previousCandidate;

	private PokeInteractable _hitInteractable;

	private PokeInteractable _recoilInteractable;

	private Vector3 _previousSurfacePointLocal;

	private Vector3 _firstTouchPointLocal;

	private Vector3 _targetTouchPointLocal;

	private Vector3 _easeTouchPointLocal;

	private bool _isRecoiled;

	private bool _isDragging;

	private ProgressCurve _dragEaseCurve;

	private ProgressCurve _pinningResyncCurve;

	private Vector3 _dragCompareSurfacePointLocal;

	private float _maxDistanceFromFirstTouchPoint;

	private float _recoilVelocityExpansion;

	private float _selectMaxDepth;

	private float _reEnterDepth;

	private float _lastUpdateTime;

	private Func<float> _timeProvider = () => Time.time;

	private bool _isPassedSurface;

	public Action<bool> WhenPassedSurfaceChanged = delegate
	{
	};

	private SurfaceHitCache _hitCache;

	private Dictionary<PokeInteractable, Matrix4x4> _previousSurfaceTransformMap;

	private float _previousDragCurveProgress;

	private float _previousPinningCurveProgress;

	private List<CachedInteractable> _cachedInteractablesInRange = new List<CachedInteractable>();

	public Vector3 ClosestPoint { get; private set; }

	public Vector3 TouchPoint { get; private set; }

	public Vector3 TouchNormal { get; private set; }

	public float Radius => _radius;

	public Vector3 Origin { get; private set; }

	public bool IsPassedSurface
	{
		get
		{
			return _isPassedSurface;
		}
		set
		{
			bool isPassedSurface = _isPassedSurface;
			_isPassedSurface = value;
			if (value != isPassedSurface)
			{
				WhenPassedSurfaceChanged(value);
			}
		}
	}

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected override void Awake()
	{
		base.Awake();
		_nativeId = 5795969328217354098uL;
	}

	protected override void Start()
	{
		base.Start();
		_dragEaseCurve = new ProgressCurve();
		_pinningResyncCurve = new ProgressCurve();
		_hitCache = new SurfaceHitCache();
		_previousSurfaceTransformMap = new Dictionary<PokeInteractable, Matrix4x4>();
	}

	protected override void DoPreprocess()
	{
		base.DoPreprocess();
		_previousPokeOrigin = Origin;
		Origin = _pointTransform.position;
		_hitCache.Reset(Origin);
	}

	protected override void DoPostprocess()
	{
		base.DoPostprocess();
		foreach (PokeInteractable item in Interactable<PokeInteractor, PokeInteractable>.Registry.List(this))
		{
			_previousSurfaceTransformMap[item] = item.SurfacePatch.BackingSurface.Transform.worldToLocalMatrix;
		}
		_lastUpdateTime = _timeProvider();
	}

	protected override bool ComputeShouldSelect()
	{
		if (_recoilInteractable != null)
		{
			float num = ComputePokeDepth(_recoilInteractable, Origin);
			_reEnterDepth = Mathf.Min(num + _recoilInteractable.RecoilAssist.ReEnterDistance, _reEnterDepth);
			_hitInteractable = ((num > _reEnterDepth) ? _recoilInteractable : null);
		}
		return _hitInteractable != null;
	}

	protected override bool ComputeShouldUnselect()
	{
		return _hitInteractable == null;
	}

	private bool GetBackingHit(PokeInteractable interactable, out SurfaceHit hit)
	{
		return _hitCache.GetBackingHit(interactable, out hit);
	}

	private bool GetPatchHit(PokeInteractable interactable, out SurfaceHit hit)
	{
		return _hitCache.GetPatchHit(interactable, out hit);
	}

	private bool InteractableInRange(PokeInteractable interactable)
	{
		if (!_previousSurfaceTransformMap.ContainsKey(interactable))
		{
			return true;
		}
		Vector3 position = _previousSurfaceTransformMap[interactable].MultiplyPoint(_previousPokeOrigin);
		Vector3 b = interactable.SurfacePatch.BackingSurface.Transform.TransformPoint(position);
		float num = ((interactable == base.Interactable) ? Mathf.Max(interactable.ExitHoverTangent, interactable.ExitHoverNormal) : Mathf.Max(interactable.EnterHoverTangent, interactable.EnterHoverNormal));
		float maxDistance = Vector3.Distance(Origin, b) + Radius + num + _equalDistanceThreshold + interactable.CloseDistanceThreshold;
		ISurfacePatch surfacePatch = interactable.SurfacePatch;
		Vector3 point = Origin;
		SurfaceHit hit;
		return surfacePatch.ClosestSurfacePoint(in point, out hit, maxDistance);
	}

	protected override void DoHoverUpdate()
	{
		if (_interactable != null && GetBackingHit(_interactable, out var hit))
		{
			TouchPoint = hit.Point;
			TouchNormal = hit.Normal;
		}
		if (_recoilInteractable != null)
		{
			if (!SurfaceUpdate(_recoilInteractable))
			{
				_isRecoiled = false;
				_recoilInteractable = null;
				_recoilVelocityExpansion = 0f;
				IsPassedSurface = false;
			}
			else if (ShouldCancel(_recoilInteractable))
			{
				GeneratePointerEvent(PointerEventType.Cancel, _recoilInteractable);
				_previousPokeOrigin = Origin;
				_previousCandidate = null;
				_hitInteractable = null;
				_recoilInteractable = null;
				_recoilVelocityExpansion = 0f;
				IsPassedSurface = false;
				_isRecoiled = false;
			}
		}
	}

	protected override PokeInteractable ComputeCandidate()
	{
		if (_recoilInteractable != null)
		{
			return _recoilInteractable;
		}
		if (_hitInteractable != null)
		{
			return _hitInteractable;
		}
		UpdateInteractablesInRange(ref _cachedInteractablesInRange);
		PokeInteractable pokeInteractable = ComputeSelectCandidate(_cachedInteractablesInRange);
		if (pokeInteractable != null)
		{
			_hitInteractable = pokeInteractable;
			_previousCandidate = pokeInteractable;
			return _hitInteractable;
		}
		return _previousCandidate = ComputeHoverCandidate(_cachedInteractablesInRange);
	}

	protected override int ComputeCandidateTiebreaker(PokeInteractable a, PokeInteractable b)
	{
		int num = base.ComputeCandidateTiebreaker(a, b);
		if (num != 0)
		{
			return num;
		}
		return a.TiebreakerScore.CompareTo(b.TiebreakerScore);
	}

	private void UpdateInteractablesInRange(ref List<CachedInteractable> cachedInteractables)
	{
		cachedInteractables.Clear();
		foreach (PokeInteractable item in Interactable<PokeInteractor, PokeInteractable>.Registry.List(this))
		{
			if (InteractableInRange(item) && GetBackingHit(item, out var hit) && GetPatchHit(item, out var hit2))
			{
				cachedInteractables.Add(new CachedInteractable
				{
					interactable = item,
					backingHit = hit,
					patchHit = hit2
				});
			}
		}
	}

	private PokeInteractable ComputeSelectCandidate(List<CachedInteractable> interactables)
	{
		PokeInteractable pokeInteractable = null;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		foreach (CachedInteractable interactable3 in interactables)
		{
			PokeInteractable interactable = interactable3.interactable;
			Vector3 position = (_previousSurfaceTransformMap.ContainsKey(interactable) ? _previousSurfaceTransformMap[interactable] : interactable.SurfacePatch.BackingSurface.Transform.worldToLocalMatrix).MultiplyPoint(_previousPokeOrigin);
			Vector3 vector = interactable.SurfacePatch.BackingSurface.Transform.TransformPoint(position);
			if (!PassesEnterHoverDistanceCheck(vector, interactable))
			{
				continue;
			}
			Vector3 vector2 = Origin - vector;
			float magnitude = vector2.magnitude;
			if (magnitude == 0f)
			{
				continue;
			}
			vector2 /= magnitude;
			Ray ray = new Ray(vector, vector2);
			SurfaceHit backingHit = interactable3.backingHit;
			Vector3 normal = backingHit.Normal;
			if (!(Vector3.Dot(vector2, normal) < 0f))
			{
				continue;
			}
			SurfaceHit hit;
			bool flag = interactable.SurfacePatch.BackingSurface.Raycast(in ray, out hit) && hit.Distance <= magnitude;
			if (!flag)
			{
				float num3 = ComputeDistanceAbove(interactable, Origin);
				if (num3 <= 0f)
				{
					Vector3 point = backingHit.Point;
					flag = true;
					SurfaceHit surfaceHit = default(SurfaceHit);
					surfaceHit.Point = point;
					surfaceHit.Normal = backingHit.Normal;
					surfaceHit.Distance = num3;
					hit = surfaceHit;
				}
			}
			if (!flag)
			{
				continue;
			}
			float num4 = ComputeTangentDistance(interactable, hit.Point);
			if (num4 > ((interactable != _previousCandidate) ? interactable.EnterHoverTangent : interactable.ExitHoverTangent))
			{
				continue;
			}
			float num5 = Vector3.Dot(vector - hit.Point, hit.Normal);
			if (Mathf.Abs(num5 - num) < _equalDistanceThreshold)
			{
				int num6 = ComputeCandidateTiebreaker(interactable, pokeInteractable);
				if (num6 > 0)
				{
					num = num5;
					num2 = num4;
					pokeInteractable = interactable;
				}
				if (num6 != 0)
				{
					continue;
				}
			}
			if (!(num5 > num + interactable.CloseDistanceThreshold))
			{
				if (pokeInteractable == null || num5 < num - pokeInteractable.CloseDistanceThreshold)
				{
					num = num5;
					num2 = num4;
					pokeInteractable = interactable;
				}
				else if (num4 < num2)
				{
					num = num5;
					num2 = num4;
					pokeInteractable = interactable;
				}
			}
		}
		if (pokeInteractable != null)
		{
			GetBackingHit(pokeInteractable, out var hit2);
			GetPatchHit(pokeInteractable, out var hit3);
			ClosestPoint = hit3.Point;
			TouchPoint = hit2.Point;
			TouchNormal = hit2.Normal;
			foreach (CachedInteractable item in _cachedInteractablesInRange)
			{
				PokeInteractable interactable2 = item.interactable;
				if (interactable2 == pokeInteractable)
				{
					continue;
				}
				Vector3 position2 = (_previousSurfaceTransformMap.ContainsKey(interactable2) ? _previousSurfaceTransformMap[interactable2] : interactable2.SurfacePatch.BackingSurface.Transform.worldToLocalMatrix).MultiplyPoint(_previousPokeOrigin);
				Vector3 position3 = interactable2.SurfacePatch.BackingSurface.Transform.TransformPoint(position2);
				if (!PassesEnterHoverDistanceCheck(position3, interactable2))
				{
					continue;
				}
				SurfaceHit backingHit2 = item.backingHit;
				float num7 = Vector3.Dot(TouchPoint - backingHit2.Point, backingHit2.Normal);
				if ((!(Mathf.Abs(num7) < _equalDistanceThreshold) || ComputeCandidateTiebreaker(pokeInteractable, interactable2) <= 0) && !(num7 <= 0f) && !(num7 > interactable2.CloseDistanceThreshold))
				{
					float num8 = ComputeTangentDistance(interactable2, TouchPoint);
					if (!(num8 > interactable2.EnterHoverTangent) && !(num8 > num2))
					{
						return null;
					}
				}
			}
		}
		return pokeInteractable;
	}

	private bool PassesEnterHoverDistanceCheck(Vector3 position, PokeInteractable interactable)
	{
		if (interactable == _previousCandidate)
		{
			return true;
		}
		float num = 0f;
		if (interactable.MinThresholds.Enabled)
		{
			num = Mathf.Min(interactable.MinThresholds.MinNormal, MinPokeDepth(interactable));
		}
		return ComputeDistanceAbove(interactable, position) > num;
	}

	public float MinPokeDepth(PokeInteractable interactable)
	{
		float num = interactable.ExitHoverNormal;
		foreach (PokeInteractor interactor in interactable.Interactors)
		{
			num = Mathf.Min(ComputePokeDepth(interactable, interactor.Origin), num);
		}
		return num;
	}

	private PokeInteractable ComputeHoverCandidate(List<CachedInteractable> interactables)
	{
		PokeInteractable pokeInteractable = null;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		foreach (CachedInteractable interactable2 in interactables)
		{
			PokeInteractable interactable = interactable2.interactable;
			if (!PassesEnterHoverDistanceCheck(Origin, interactable) && !PassesEnterHoverDistanceCheck(_previousPokeOrigin, interactable))
			{
				continue;
			}
			SurfaceHit backingHit = interactable2.backingHit;
			Vector3 point = backingHit.Point;
			Vector3 normal = backingHit.Normal;
			Vector3 lhs = Origin - point;
			if (lhs.magnitude == 0f || !(Vector3.Dot(lhs, normal) > 0f))
			{
				continue;
			}
			float num3 = ComputeDistanceAbove(interactable, Origin);
			if (num3 > ((_previousCandidate != interactable) ? interactable.EnterHoverNormal : interactable.ExitHoverNormal))
			{
				continue;
			}
			float num4 = ComputeTangentDistance(interactable, Origin);
			if (num4 > ((_previousCandidate != interactable) ? interactable.EnterHoverTangent : interactable.ExitHoverTangent))
			{
				continue;
			}
			if (Mathf.Abs(num3 - num) < _equalDistanceThreshold && pokeInteractable != null)
			{
				int num5 = ComputeCandidateTiebreaker(interactable, pokeInteractable);
				if (num5 > 0)
				{
					pokeInteractable = interactable;
					num = num3;
					num2 = num4;
				}
				if (num5 != 0)
				{
					continue;
				}
			}
			if (!(num3 > num + interactable.CloseDistanceThreshold))
			{
				if (pokeInteractable == null || num3 < num - pokeInteractable.CloseDistanceThreshold)
				{
					pokeInteractable = interactable;
					num = num3;
					num2 = num4;
				}
				else if (num4 < num2)
				{
					pokeInteractable = interactable;
					num = num3;
					num2 = num4;
				}
			}
		}
		if (pokeInteractable != null)
		{
			GetBackingHit(pokeInteractable, out var hit);
			GetPatchHit(pokeInteractable, out var hit2);
			ClosestPoint = hit2.Point;
			TouchPoint = hit.Point;
			TouchNormal = hit.Normal;
		}
		return pokeInteractable;
	}

	protected override void InteractableSelected(PokeInteractable interactable)
	{
		if (interactable != null && GetBackingHit(interactable, out var hit))
		{
			_previousSurfacePointLocal = (_firstTouchPointLocal = (_easeTouchPointLocal = (_targetTouchPointLocal = interactable.SurfacePatch.BackingSurface.Transform.InverseTransformPoint(TouchPoint))));
			Vector3 point = hit.Point;
			_dragCompareSurfacePointLocal = interactable.SurfacePatch.BackingSurface.Transform.InverseTransformPoint(point);
			_dragEaseCurve.Copy(interactable.DragThresholds.DragEaseCurve);
			_pinningResyncCurve.Copy(interactable.PositionPinning.ResyncCurve);
			_isDragging = false;
			_isRecoiled = false;
			_maxDistanceFromFirstTouchPoint = 0f;
			_selectMaxDepth = 0f;
		}
		IsPassedSurface = true;
		base.InteractableSelected(interactable);
	}

	protected override void HandleDisabled()
	{
		_hitInteractable = null;
		IsPassedSurface = false;
		base.HandleDisabled();
	}

	protected override Pose ComputePointerPose()
	{
		if (base.Interactable == null)
		{
			return Pose.identity;
		}
		if (!base.Interactable.ClosestBackingSurfaceHit(TouchPoint, out var hit))
		{
			return Pose.identity;
		}
		return new Pose(TouchPoint, Quaternion.LookRotation(hit.Normal));
	}

	private float ComputeDistanceAbove(PokeInteractable interactable, Vector3 point)
	{
		return SurfaceUtils.ComputeDistanceAbove(interactable.SurfacePatch, point, _radius);
	}

	[Obsolete("This will be removed in a future version of Interaction SDK. Please use SurfaceUtils.ComputeDepth instead")]
	public float ComputeDepth(PokeInteractable interactable, Vector3 point)
	{
		return SurfaceUtils.ComputeDepth(interactable.SurfacePatch, point, _radius);
	}

	private float ComputePokeDepth(PokeInteractable interactable, Vector3 point)
	{
		return SurfaceUtils.ComputeDepth(interactable.SurfacePatch, point, _radius);
	}

	private float ComputeDistanceFrom(PokeInteractable interactable, Vector3 point)
	{
		return SurfaceUtils.ComputeDistanceFrom(interactable.SurfacePatch, point, _radius);
	}

	private float ComputeTangentDistance(PokeInteractable interactable, Vector3 point)
	{
		return SurfaceUtils.ComputeTangentDistance(interactable.SurfacePatch, point, _radius);
	}

	protected virtual bool SurfaceUpdate(PokeInteractable interactable)
	{
		if (interactable == null)
		{
			return false;
		}
		if (!GetBackingHit(interactable, out var hit))
		{
			return false;
		}
		if (ComputeDistanceAbove(interactable, Origin) > _touchReleaseThreshold)
		{
			return false;
		}
		bool isRecoiled = _isRecoiled;
		_isRecoiled = _hitInteractable == null && _recoilInteractable != null;
		Vector3 point = hit.Point;
		Vector3 vector = interactable.SurfacePatch.BackingSurface.Transform.InverseTransformPoint(point);
		if (interactable.DragThresholds.Enabled)
		{
			float num = Mathf.Abs(ComputePokeDepth(interactable, Origin) - ComputePokeDepth(interactable, _previousPokeOrigin));
			Vector3 vector2 = vector - _previousSurfacePointLocal;
			bool flag = num > interactable.SurfacePatch.BackingSurface.Transform.TransformVector(vector2).magnitude && num > interactable.DragThresholds.DragNormal;
			if (flag)
			{
				_dragCompareSurfacePointLocal = vector;
			}
			if (!_isDragging)
			{
				if (!flag)
				{
					Vector3 vector3 = vector - _dragCompareSurfacePointLocal;
					if (interactable.SurfacePatch.BackingSurface.Transform.TransformVector(vector3).magnitude > interactable.DragThresholds.DragTangent)
					{
						_isDragging = true;
						_dragEaseCurve.Start();
						_previousDragCurveProgress = 0f;
						_targetTouchPointLocal = vector;
					}
				}
			}
			else if (flag)
			{
				_isDragging = false;
			}
			else
			{
				_targetTouchPointLocal = vector;
			}
		}
		else
		{
			_targetTouchPointLocal = vector;
		}
		Vector3 vector4 = _targetTouchPointLocal;
		if (interactable.PositionPinning.Enabled)
		{
			if (!_isRecoiled)
			{
				Vector3 vector5 = vector4 - _firstTouchPointLocal;
				_maxDistanceFromFirstTouchPoint = Mathf.Max(interactable.SurfacePatch.BackingSurface.Transform.TransformVector(vector5).magnitude, _maxDistanceFromFirstTouchPoint);
				float num2 = 1f;
				if (interactable.PositionPinning.MaxPinDistance != 0f)
				{
					num2 = Mathf.Clamp01(_maxDistanceFromFirstTouchPoint / interactable.PositionPinning.MaxPinDistance);
					num2 = interactable.PositionPinning.PinningEaseCurve.Evaluate(num2);
				}
				vector4 = _firstTouchPointLocal + vector5 * num2;
			}
			else
			{
				if (!isRecoiled)
				{
					_pinningResyncCurve.Start();
					_previousPinningCurveProgress = 0f;
				}
				float num3 = _pinningResyncCurve.Progress();
				if (num3 != 1f)
				{
					float num4 = num3 - _previousPinningCurveProgress;
					Vector3 vector6 = vector4 - _easeTouchPointLocal;
					vector4 = _easeTouchPointLocal + num4 / (1f - _previousPinningCurveProgress) * vector6;
					_previousPinningCurveProgress = num3;
				}
			}
		}
		float num5 = _dragEaseCurve.Progress();
		if (num5 != 1f)
		{
			float num6 = num5 - _previousDragCurveProgress;
			Vector3 vector7 = vector4 - _easeTouchPointLocal;
			_easeTouchPointLocal += num6 / (1f - _previousDragCurveProgress) * vector7;
			_previousDragCurveProgress = num5;
		}
		else
		{
			_easeTouchPointLocal = vector4;
		}
		TouchPoint = interactable.SurfacePatch.BackingSurface.Transform.TransformPoint(_easeTouchPointLocal);
		interactable.ClosestBackingSurfaceHit(TouchPoint, out var hit2);
		TouchNormal = hit2.Normal;
		_previousSurfacePointLocal = vector;
		return true;
	}

	protected virtual bool ShouldCancel(PokeInteractable interactable)
	{
		if ((interactable.CancelSelectNormal > 0f && ComputePokeDepth(interactable, Origin) > interactable.CancelSelectNormal) || (interactable.CancelSelectTangent > 0f && ComputeTangentDistance(interactable, Origin) > interactable.CancelSelectTangent))
		{
			return true;
		}
		return false;
	}

	protected virtual bool ShouldRecoil(PokeInteractable interactable)
	{
		if (!interactable.RecoilAssist.Enabled)
		{
			return false;
		}
		float num = ComputePokeDepth(interactable, Origin);
		float num2 = _timeProvider() - _lastUpdateTime;
		float num3 = interactable.RecoilAssist.ExitDistance;
		if (interactable.RecoilAssist.UseVelocityExpansion)
		{
			Vector3 lhs = Origin - _previousPokeOrigin;
			float num4 = Mathf.Max(0f, Vector3.Dot(lhs, -TouchNormal));
			num4 = ((num2 > 0f) ? (num4 / num2) : 0f);
			float num5 = Mathf.Clamp01(Mathf.InverseLerp(interactable.RecoilAssist.VelocityExpansionMinSpeed, interactable.RecoilAssist.VelocityExpansionMaxSpeed, num4)) * interactable.RecoilAssist.VelocityExpansionDistance;
			if (num5 > _recoilVelocityExpansion)
			{
				_recoilVelocityExpansion = num5;
			}
			else
			{
				float num6 = interactable.RecoilAssist.VelocityExpansionDecayRate * num2;
				_recoilVelocityExpansion = Math.Max(num5, _recoilVelocityExpansion - num6);
			}
			num3 += _recoilVelocityExpansion;
		}
		if (num > _selectMaxDepth)
		{
			_selectMaxDepth = num;
		}
		else
		{
			if (interactable.RecoilAssist.UseDynamicDecay)
			{
				Vector3 vector = Origin - _previousPokeOrigin;
				Vector3 vector2 = Vector3.Project(vector, TouchNormal);
				float time = ((vector.sqrMagnitude > 1E-07f) ? (vector2.magnitude / vector.magnitude) : 1f);
				float num7 = interactable.RecoilAssist.DynamicDecayCurve.Evaluate(time);
				_selectMaxDepth = Mathf.Lerp(_selectMaxDepth, num, num7 * num2);
			}
			if (num < _selectMaxDepth - num3)
			{
				_reEnterDepth = num + interactable.RecoilAssist.ReEnterDistance;
				return true;
			}
		}
		return false;
	}

	protected override void DoSelectUpdate()
	{
		if (!SurfaceUpdate(_selectedInteractable))
		{
			_hitInteractable = null;
			IsPassedSurface = _recoilInteractable != null;
		}
		else if (ShouldCancel(_selectedInteractable))
		{
			GeneratePointerEvent(PointerEventType.Cancel, _selectedInteractable);
			_previousPokeOrigin = Origin;
			_previousCandidate = null;
			_hitInteractable = null;
			_recoilInteractable = null;
			_recoilVelocityExpansion = 0f;
			IsPassedSurface = false;
			_isRecoiled = false;
		}
		else if (ShouldRecoil(_selectedInteractable))
		{
			_hitInteractable = null;
			_recoilInteractable = _selectedInteractable;
			_selectMaxDepth = 0f;
		}
	}

	public void InjectAllPokeInteractor(Transform pointTransform, float radius = 0.005f)
	{
		InjectPointTransform(pointTransform);
		InjectRadius(radius);
	}

	public void InjectPointTransform(Transform pointTransform)
	{
		_pointTransform = pointTransform;
	}

	public void InjectRadius(float radius)
	{
		_radius = radius;
	}

	public void InjectOptionalTouchReleaseThreshold(float touchReleaseThreshold)
	{
		_touchReleaseThreshold = touchReleaseThreshold;
	}

	public void InjectOptionalEqualDistanceThreshold(float equalDistanceThreshold)
	{
		_equalDistanceThreshold = equalDistanceThreshold;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
