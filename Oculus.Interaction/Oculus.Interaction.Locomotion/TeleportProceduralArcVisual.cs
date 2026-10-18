using System;
using Oculus.Interaction.DistanceReticles;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TeleportProceduralArcVisual : MonoBehaviour
{
	[SerializeField]
	private TeleportInteractor _interactor;

	[SerializeField]
	private TubeRenderer _tubeRenderer;

	[SerializeField]
	[Optional]
	private PinchPointerVisual _pointer;

	[SerializeField]
	[Optional]
	private Transform _pointerAnchor;

	[SerializeField]
	[Optional]
	[Interface(typeof(IAxis1D), new Type[] { })]
	private UnityEngine.Object _progress;

	private IAxis1D Progress;

	[SerializeField]
	[Min(2f)]
	private int _arcPointsCount = 30;

	[SerializeField]
	private Color _noDestinationTint = Color.red;

	private TubePoint[] _arcPoints;

	private IReticleData _reticleData;

	protected bool _started;

	public int ArcPointsCount
	{
		get
		{
			return _arcPointsCount;
		}
		set
		{
			_arcPointsCount = value;
		}
	}

	public Color NoDestinationTint
	{
		get
		{
			return _noDestinationTint;
		}
		set
		{
			_noDestinationTint = value;
		}
	}

	protected virtual void Awake()
	{
		Progress = _progress as IAxis1D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_ = _progress != null;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_interactor.WhenPostprocessed += HandleInteractorPostProcessed;
			_interactor.WhenStateChanged += HandleInteractorStateChanged;
			_interactor.WhenInteractableSet.Action += HandleInteractableSet;
			_interactor.WhenInteractableUnset.Action += HandleInteractableUnset;
			_tubeRenderer.Hide();
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_interactor.WhenPostprocessed -= HandleInteractorPostProcessed;
			_interactor.WhenStateChanged -= HandleInteractorStateChanged;
			_interactor.WhenInteractableSet.Action -= HandleInteractableSet;
			_interactor.WhenInteractableUnset.Action -= HandleInteractableUnset;
			_tubeRenderer.Hide();
		}
	}

	private void HandleInteractableSet(TeleportInteractable interactable)
	{
		if (interactable != null)
		{
			_reticleData = interactable.GetComponent<IReticleData>();
		}
	}

	private void HandleInteractableUnset(TeleportInteractable obj)
	{
		_reticleData = null;
	}

	private void HandleInteractorStateChanged(InteractorStateChangeArgs stateChange)
	{
		if (stateChange.NewState == InteractorState.Disabled)
		{
			_tubeRenderer.Hide();
		}
	}

	private void HandleInteractorPostProcessed()
	{
		if (_interactor.State != InteractorState.Disabled)
		{
			Color tint = Color.white;
			if (!_interactor.HasValidDestination())
			{
				tint = _noDestinationTint;
			}
			Vector3 vector = _interactor.ArcEnd.Point;
			if (_reticleData != null)
			{
				vector = _reticleData.ProcessHitPoint(vector);
			}
			UpdateVisualArcPoints(_interactor.ArcOrigin, vector);
			_tubeRenderer.Tint = tint;
			_tubeRenderer.Progress = ((Progress != null) ? Progress.Value() : 0f);
			_tubeRenderer.RenderTube(_arcPoints, Space.World);
			UpdatePointer(tint, vector);
		}
	}

	private void UpdatePointer(Color tint, Vector3 target)
	{
		if (!(_pointer == null))
		{
			_pointer.Tint = tint;
			Vector3 vector = ((_pointerAnchor != null) ? _pointerAnchor.position : _interactor.ArcOrigin.position);
			Quaternion rotation = Quaternion.LookRotation(target - vector);
			_pointer.SetPositionAndRotation(vector, rotation);
		}
	}

	private void UpdateVisualArcPoints(Pose origin, Vector3 target)
	{
		if (_arcPoints == null || _arcPoints.Length != ArcPointsCount)
		{
			_arcPoints = new TubePoint[ArcPointsCount];
		}
		float num = CalculateMidpointFactor(Vector3.Dot(origin.forward, Vector3.up));
		float magnitude = Vector3.ProjectOnPlane(target - origin.position, Vector3.up).magnitude;
		Vector3 middle = origin.position + origin.forward * magnitude * num;
		Vector3 vector = origin.position - origin.forward;
		Vector3 b = new Vector3(1f / base.transform.lossyScale.x, 1f / base.transform.lossyScale.y, 1f / base.transform.lossyScale.z);
		float num2 = 0f;
		for (int i = 0; i < ArcPointsCount; i++)
		{
			float t = (float)i / ((float)ArcPointsCount - 1f);
			Vector3 vector2 = EvaluateBezierArc(origin.position, middle, target, t);
			Vector3 vector3 = vector2 - vector;
			_arcPoints[i].position = Vector3.Scale(vector2, b);
			_arcPoints[i].rotation = Quaternion.LookRotation(vector3.normalized);
			if (i > 0)
			{
				num2 += vector3.magnitude;
			}
			vector = vector2;
		}
		for (int j = 1; j < ArcPointsCount; j++)
		{
			float magnitude2 = (_arcPoints[j - 1].position - _arcPoints[j].position).magnitude;
			_arcPoints[j].relativeLength = _arcPoints[j - 1].relativeLength + magnitude2 / num2;
		}
	}

	private static Vector3 EvaluateBezierArc(Vector3 start, Vector3 middle, Vector3 end, float t)
	{
		t = Mathf.Clamp01(t);
		float num = 1f - t;
		return num * num * start + 2f * num * t * middle + t * t * end;
	}

	private static float CalculateMidpointFactor(float pitchDot)
	{
		return Mathf.Pow(1f - pitchDot * pitchDot, -0.25f) - 0.5f;
	}

	public void InjectAllTeleportProceduralArcVisual(TeleportInteractor interactor)
	{
		InjectTeleportInteractor(interactor);
	}

	public void InjectTeleportInteractor(TeleportInteractor interactor)
	{
		_interactor = interactor;
	}

	public void InjectOptionalProgress(IAxis1D progress)
	{
		_progress = progress as UnityEngine.Object;
		Progress = progress;
	}

	public void InjectOptionalPointer(PinchPointerVisual pointer)
	{
		_pointer = pointer;
	}

	public void InjectOptionalPointerAnchor(Transform pointerAnchor)
	{
		_pointerAnchor = pointerAnchor;
	}
}
