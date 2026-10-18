using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TeleportArcVisual : MonoBehaviour
{
	[SerializeField]
	private TeleportInteractor _interactor;

	[SerializeField]
	private LineRenderer _arcRenderer;

	private Vector3[] _positions;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_interactor.WhenPostprocessed += HandleInteractorPostProcessed;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_interactor.WhenPostprocessed -= HandleInteractorPostProcessed;
		}
	}

	protected virtual void HandleInteractorPostProcessed()
	{
		int pointsCount = _interactor.TeleportArc.PointsCount;
		if (_positions == null || _positions.Length != pointsCount)
		{
			_positions = new Vector3[pointsCount];
			_arcRenderer.positionCount = pointsCount;
		}
		for (int i = 0; i < pointsCount; i++)
		{
			_positions[i] = _interactor.TeleportArc.PointAtIndex(i);
		}
		_arcRenderer.SetPositions(_positions);
	}

	public void InjectAllTeleportArcVisual(TeleportInteractor interactor, LineRenderer arcRenderer)
	{
		InjectInteractor(interactor);
		InjectArcRenderer(arcRenderer);
	}

	public void InjectInteractor(TeleportInteractor interactor)
	{
		_interactor = interactor;
	}

	public void InjectArcRenderer(LineRenderer arcRenderer)
	{
		_arcRenderer = arcRenderer;
	}
}
