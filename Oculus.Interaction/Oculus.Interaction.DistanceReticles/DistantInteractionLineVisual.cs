using System;
using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public abstract class DistantInteractionLineVisual : MonoBehaviour
{
	private class DummyPointReticle : IReticleData
	{
		public Transform Target { get; set; }

		public Vector3 ProcessHitPoint(Vector3 hitPoint)
		{
			return Target.position;
		}
	}

	[Tooltip("The distance interactor used as the origin of the line visual.")]
	[SerializeField]
	[Interface(typeof(IDistanceInteractor), new Type[] { })]
	private UnityEngine.Object _distanceInteractor;

	[Tooltip("Where the line visual begins relative to the hand or controller. The lower the value, the closer the line.")]
	[SerializeField]
	private float _visualOffset = 0.07f;

	private Vector3[] _linePoints;

	[Tooltip("Should the line be visible when the distance interactor is in a normal state (not selecting, hovering, or disabled)?")]
	[SerializeField]
	private bool _visibleDuringNormal;

	private IReticleData _target;

	[Tooltip("The number of segments that make up the line. The more segments, the smoother the line.")]
	[SerializeField]
	private int _numLinePoints = 20;

	[Tooltip("The length of the line when the interactor is in a normal state. Only visible if the \"Visible during normal\" checkbox is also selected.")]
	[SerializeField]
	private float _targetlessLength = 0.5f;

	protected bool _started;

	private bool _shouldDrawLine;

	private DummyPointReticle _dummyTarget = new DummyPointReticle();

	public IDistanceInteractor DistanceInteractor { get; protected set; }

	public float VisualOffset
	{
		get
		{
			return _visualOffset;
		}
		set
		{
			_visualOffset = value;
		}
	}

	protected int NumLinePoints => _numLinePoints;

	protected float TargetlessLength => _targetlessLength;

	private void Awake()
	{
		DistanceInteractor = _distanceInteractor as IDistanceInteractor;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_linePoints = new Vector3[NumLinePoints];
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			DistanceInteractor.WhenStateChanged += HandleStateChanged;
			DistanceInteractor.WhenPostprocessed += HandlePostProcessed;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			DistanceInteractor.WhenStateChanged -= HandleStateChanged;
			DistanceInteractor.WhenPostprocessed -= HandlePostProcessed;
		}
	}

	private void HandleStateChanged(InteractorStateChangeArgs args)
	{
		switch (args.NewState)
		{
		case InteractorState.Normal:
			if (args.PreviousState != InteractorState.Disabled)
			{
				InteractableUnset();
			}
			break;
		case InteractorState.Hover:
			if (args.PreviousState == InteractorState.Normal)
			{
				InteractableSet(DistanceInteractor.DistanceInteractable);
			}
			break;
		}
		if (args.NewState == InteractorState.Select || args.NewState == InteractorState.Disabled || args.PreviousState == InteractorState.Disabled)
		{
			_shouldDrawLine = false;
		}
		else if (args.NewState == InteractorState.Hover)
		{
			_shouldDrawLine = true;
		}
		else if (args.NewState == InteractorState.Normal)
		{
			_shouldDrawLine = _visibleDuringNormal;
		}
	}

	private void HandlePostProcessed()
	{
		if (_shouldDrawLine)
		{
			UpdateLine();
		}
		else
		{
			HideLine();
		}
	}

	protected virtual void InteractableSet(IRelativeToRef interactable)
	{
		Component component = interactable as Component;
		if (component == null)
		{
			_target = null;
		}
		else if (!component.TryGetComponent<IReticleData>(out _target))
		{
			_dummyTarget.Target = interactable.RelativeTo;
			_target = _dummyTarget;
		}
	}

	protected virtual void InteractableUnset()
	{
		_target = null;
	}

	private void UpdateLine()
	{
		Vector3 forward = DistanceInteractor.Origin.forward;
		Vector3 vector = DistanceInteractor.Origin.position + forward * VisualOffset;
		Vector3 vector2 = TargetHit(DistanceInteractor.HitPoint);
		Vector3 middle = vector + forward * Vector3.Distance(vector, vector2) * 0.5f;
		for (int i = 0; i < NumLinePoints; i++)
		{
			float t = (float)i / ((float)NumLinePoints - 1f);
			Vector3 vector3 = EvaluateBezier(vector, middle, vector2, t);
			_linePoints[i] = vector3;
		}
		RenderLine(_linePoints);
	}

	protected abstract void RenderLine(Vector3[] linePoints);

	protected abstract void HideLine();

	protected Vector3 TargetHit(Vector3 hitPoint)
	{
		if (_target != null)
		{
			return _target.ProcessHitPoint(hitPoint);
		}
		return DistanceInteractor.Origin.position + DistanceInteractor.Origin.forward * _targetlessLength;
	}

	protected static Vector3 EvaluateBezier(Vector3 start, Vector3 middle, Vector3 end, float t)
	{
		t = Mathf.Clamp01(t);
		float num = 1f - t;
		return num * num * start + 2f * num * t * middle + t * t * end;
	}

	public void InjectAllDistantInteractionLineVisual(IDistanceInteractor interactor)
	{
		InjectDistanceInteractor(interactor);
	}

	public void InjectDistanceInteractor(IDistanceInteractor interactor)
	{
		_distanceInteractor = interactor as UnityEngine.Object;
		DistanceInteractor = interactor;
	}
}
