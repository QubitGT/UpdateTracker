using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class JointRotationDebugVisual : MonoBehaviour
{
	[SerializeField]
	private JointRotationActiveState _jointRotation;

	[SerializeField]
	private Material _lineRendererMaterial;

	[SerializeField]
	private float _rendererLineWidth = 0.005f;

	[SerializeField]
	private float _rendererLineLength = 0.1f;

	private List<LineRenderer> _lineRenderers;

	private int _enabledRendererCount;

	protected bool _started;

	protected virtual void Awake()
	{
		_lineRenderers = new List<LineRenderer>();
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			ResetLines();
		}
	}

	protected virtual void Update()
	{
		ResetLines();
		foreach (JointRotationActiveState.JointRotationFeatureConfig featureConfig in _jointRotation.FeatureConfigs)
		{
			if (_jointRotation.Hand.GetJointPose(featureConfig.Feature, out var pose) && _jointRotation.FeatureStates.TryGetValue(featureConfig, out var value))
			{
				DrawDebugLine(pose.position, value.TargetAxis, value.Amount);
			}
		}
	}

	private void DrawDebugLine(Vector3 jointPos, Vector3 direction, float amount)
	{
		Vector3 vector = direction.normalized * _rendererLineLength;
		if (amount >= 1f)
		{
			AddLine(jointPos, jointPos + vector, Color.green);
			return;
		}
		Vector3 vector2 = Vector3.Lerp(jointPos, jointPos + vector, amount);
		AddLine(jointPos, vector2, Color.yellow);
		AddLine(vector2, jointPos + vector, Color.red);
	}

	private void ResetLines()
	{
		foreach (LineRenderer lineRenderer in _lineRenderers)
		{
			if (lineRenderer != null)
			{
				lineRenderer.enabled = false;
			}
		}
		_enabledRendererCount = 0;
	}

	private void AddLine(Vector3 start, Vector3 end, Color color)
	{
		LineRenderer lineRenderer;
		if (_enabledRendererCount == _lineRenderers.Count)
		{
			lineRenderer = new GameObject().AddComponent<LineRenderer>();
			lineRenderer.startWidth = _rendererLineWidth;
			lineRenderer.endWidth = _rendererLineWidth;
			lineRenderer.positionCount = 2;
			lineRenderer.material = _lineRendererMaterial;
			_lineRenderers.Add(lineRenderer);
		}
		else
		{
			lineRenderer = _lineRenderers[_enabledRendererCount];
		}
		_enabledRendererCount++;
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, start);
		lineRenderer.SetPosition(1, end);
		lineRenderer.startColor = color;
		lineRenderer.endColor = color;
	}
}
