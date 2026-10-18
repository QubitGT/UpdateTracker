using UnityEngine;

namespace Oculus.Interaction;

public class SnapInteractorFollowVisual : MonoBehaviour
{
	[SerializeField]
	private SnapInteractor _snapInteractor;

	[SerializeField]
	private float _hoverOffset;

	[SerializeField]
	private ProgressCurve _easeCurve = new ProgressCurve(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), 0.1f);

	[SerializeField]
	[Optional]
	private Transform _transform;

	protected bool _started;

	private Pose _from;

	private Pose _to;

	public float HoverOffset
	{
		get
		{
			return _hoverOffset;
		}
		set
		{
			_hoverOffset = value;
		}
	}

	public ProgressCurve EaseCurve
	{
		get
		{
			return _easeCurve;
		}
		set
		{
			_easeCurve = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (_transform == null)
		{
			_transform = base.transform;
		}
		_from = (_to = ComputeTargetPose());
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_snapInteractor.WhenStateChanged += HandleStateChanged;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_snapInteractor.WhenStateChanged -= HandleStateChanged;
		}
	}

	private void HandleStateChanged(InteractorStateChangeArgs args)
	{
		_from = base.transform.GetPose();
		_to = ComputeTargetPose();
		_easeCurve.Start();
	}

	protected virtual Pose ComputeTargetPose()
	{
		if (_snapInteractor.HasInteractable && _snapInteractor.Interactable.PoseForInteractor(_snapInteractor, out var result))
		{
			if (_snapInteractor.State == InteractorState.Hover)
			{
				result.position += _hoverOffset * result.forward;
			}
			return result;
		}
		return _snapInteractor.transform.GetPose();
	}

	protected virtual void Update()
	{
		_to = ComputeTargetPose();
		float t = _easeCurve.Progress();
		Pose from = _from;
		from.Lerp(in _to, t);
		_transform.position = from.position;
		_transform.rotation = from.rotation;
	}

	public void InjectAllSnapInteractorFollowVisual(SnapInteractor snapInteractor)
	{
		_snapInteractor = snapInteractor;
	}

	public void InjectOptionalTransform(Transform transform)
	{
		_transform = transform;
	}
}
