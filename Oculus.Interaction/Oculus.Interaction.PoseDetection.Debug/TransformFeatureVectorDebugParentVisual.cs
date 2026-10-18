using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class TransformFeatureVectorDebugParentVisual : MonoBehaviour
{
	[SerializeField]
	private TransformRecognizerActiveState _transformRecognizerActiveState;

	[SerializeField]
	private GameObject _vectorVisualPrefab;

	public void GetTransformFeatureVectorAndWristPos(TransformFeature feature, bool isHandVector, ref Vector3? featureVec, ref Vector3? wristPos)
	{
		_transformRecognizerActiveState.GetFeatureVectorAndWristPos(feature, isHandVector, ref featureVec, ref wristPos);
	}

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		foreach (TransformFeatureConfig featureConfig in _transformRecognizerActiveState.FeatureConfigs)
		{
			TransformFeature feature = featureConfig.Feature;
			CreateVectorDebugView(feature, trackingHandVector: false);
			CreateVectorDebugView(feature, trackingHandVector: true);
		}
	}

	private void CreateVectorDebugView(TransformFeature feature, bool trackingHandVector)
	{
		TransformFeatureVectorDebugVisual component = Object.Instantiate(_vectorVisualPrefab, base.transform).GetComponent<TransformFeatureVectorDebugVisual>();
		component.Initialize(feature, trackingHandVector, this, trackingHandVector ? Color.blue : Color.black);
		Transform obj = component.transform;
		obj.localRotation = Quaternion.identity;
		obj.localPosition = Vector3.zero;
	}
}
