using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class DistantInteractionLineRendererVisual : DistantInteractionLineVisual
{
	[SerializeField]
	private LineRenderer _lineRenderer;

	protected override void Start()
	{
		base.Start();
		_lineRenderer.positionCount = base.NumLinePoints;
	}

	protected override void RenderLine(Vector3[] linePoints)
	{
		_lineRenderer.SetPositions(linePoints);
		_lineRenderer.enabled = true;
	}

	protected override void HideLine()
	{
		_lineRenderer.enabled = false;
	}

	public void InjectAllDistantInteractionLineRendererVisual(IDistanceInteractor interactor, LineRenderer lineRenderer)
	{
		InjectDistanceInteractor(interactor);
		InjectLineRenderer(lineRenderer);
	}

	public void InjectLineRenderer(LineRenderer lineRenderer)
	{
		_lineRenderer = lineRenderer;
	}
}
