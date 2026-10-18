using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class TransformFeatureVectorDebugVisual : MonoBehaviour
{
	[SerializeField]
	private LineRenderer _lineRenderer;

	[SerializeField]
	private float _lineWidth = 0.005f;

	[SerializeField]
	private float _lineScale = 0.1f;

	private bool _isInitialized;

	private TransformFeature _feature;

	private TransformFeatureVectorDebugParentVisual _parent;

	private bool _trackingHandVector;

	public IHand Hand { get; private set; }

	protected virtual void Awake()
	{
		_lineRenderer.enabled = false;
	}

	public void Initialize(TransformFeature feature, bool trackingHandVector, TransformFeatureVectorDebugParentVisual parent, Color lineColor)
	{
		_isInitialized = true;
		_lineRenderer.enabled = true;
		_lineRenderer.positionCount = 2;
		_lineRenderer.startColor = lineColor;
		_lineRenderer.endColor = lineColor;
		_feature = feature;
		_trackingHandVector = trackingHandVector;
		_parent = parent;
	}

	protected virtual void Update()
	{
		if (!_isInitialized)
		{
			return;
		}
		Vector3? featureVec = null;
		Vector3? wristPos = null;
		_parent.GetTransformFeatureVectorAndWristPos(_feature, _trackingHandVector, ref featureVec, ref wristPos);
		if (!featureVec.HasValue || !wristPos.HasValue)
		{
			if (_lineRenderer.enabled)
			{
				_lineRenderer.enabled = false;
			}
			return;
		}
		if (!_lineRenderer.enabled)
		{
			_lineRenderer.enabled = true;
		}
		if (Mathf.Abs(_lineRenderer.startWidth - _lineWidth) > Mathf.Epsilon)
		{
			_lineRenderer.startWidth = _lineWidth;
			_lineRenderer.endWidth = _lineWidth;
		}
		_lineRenderer.SetPosition(0, wristPos.Value);
		_lineRenderer.SetPosition(1, wristPos.Value + _lineScale * featureVec.Value);
	}
}
