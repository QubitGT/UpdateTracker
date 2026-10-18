using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class DistantInteractionTubeVisual : DistantInteractionLineVisual
{
	[SerializeField]
	private TubeRenderer _tubeRenderer;

	private TubePoint[] _tubePoints;

	protected override void Start()
	{
		base.Start();
	}

	protected override void RenderLine(Vector3[] linePoints)
	{
		InitializeArcPoints(linePoints);
		_tubeRenderer.RenderTube(_tubePoints, Space.World);
	}

	protected override void HideLine()
	{
		_tubeRenderer.Hide();
	}

	private void InitializeArcPoints(Vector3[] linePoints)
	{
		if (_tubePoints == null || _tubePoints.Length < linePoints.Length)
		{
			_tubePoints = new TubePoint[linePoints.Length];
		}
		float num = 0f;
		for (int i = 1; i < linePoints.Length; i++)
		{
			num += (linePoints[i] - linePoints[i - 1]).magnitude;
		}
		for (int j = 0; j < linePoints.Length; j++)
		{
			Vector3 forward = ((j == 0) ? (linePoints[j + 1] - linePoints[j]) : (linePoints[j] - linePoints[j - 1]));
			_tubePoints[j].position = linePoints[j];
			_tubePoints[j].rotation = Quaternion.LookRotation(forward);
			_tubePoints[j].relativeLength = ((j == 0) ? 0f : (_tubePoints[j - 1].relativeLength + forward.magnitude / num));
		}
	}

	public void InjectAllDistantInteractionPolylineVisual(IDistanceInteractor interactor)
	{
		InjectDistanceInteractor(interactor);
	}
}
