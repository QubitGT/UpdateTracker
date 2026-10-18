using Meta.XR.Util;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[HelpURL("https://developer.oculus.com/documentation/unity/move-face-tracking/")]
[Feature(Feature.FaceTracking)]
public class OVRFace : MonoBehaviour
{
	public interface IMeshWeightsProvider
	{
		void UpdateWeights(OVRFaceExpressions faceExpressions);

		bool GetWeightValue(int blendshapeIndex, out float weightValue);
	}

	[SerializeField]
	[Tooltip("The OVRFaceExpressions Component to fetch the Face Tracking weights from that are to be applied")]
	protected internal OVRFaceExpressions _faceExpressions;

	[SerializeField]
	[Tooltip("A multiplier to the weights read from the OVRFaceExpressions to exaggerate facial expressions")]
	protected internal float _blendShapeStrengthMultiplier = 100f;

	[SerializeField]
	[Tooltip("Optional component that contains IMeshWeightsProvider.")]
	protected internal GameObject _meshWeightsProviderObject;

	private SkinnedMeshRenderer _skinnedMeshRenderer;

	private IMeshWeightsProvider _meshWeightsProvider;

	public OVRFaceExpressions FaceExpressions
	{
		get
		{
			return _faceExpressions;
		}
		set
		{
			_faceExpressions = value;
		}
	}

	public float BlendShapeStrengthMultiplier
	{
		get
		{
			return _blendShapeStrengthMultiplier;
		}
		set
		{
			_blendShapeStrengthMultiplier = value;
		}
	}

	protected SkinnedMeshRenderer SkinnedMesh => _skinnedMeshRenderer;

	internal SkinnedMeshRenderer RetrieveSkinnedMeshRenderer()
	{
		return GetComponent<SkinnedMeshRenderer>();
	}

	internal OVRFaceExpressions SearchFaceExpressions()
	{
		return base.gameObject.GetComponentInParent<OVRFaceExpressions>();
	}

	protected virtual void Awake()
	{
		if (_faceExpressions == null)
		{
			_faceExpressions = SearchFaceExpressions();
			Debug.Log("Found OVRFaceExpression reference in " + _faceExpressions.name + " due to unassigned field.");
		}
		if (_meshWeightsProviderObject != null)
		{
			_meshWeightsProvider = _meshWeightsProviderObject.GetComponent<IMeshWeightsProvider>();
		}
	}

	private void OnEnable()
	{
		OVRManager oVRManager = Object.FindAnyObjectByType<OVRManager>();
		if (oVRManager != null && oVRManager.SimultaneousHandsAndControllersEnabled)
		{
			Debug.LogWarning("Please note that currently, face tracking and simultaneous hands and controllers cannot be enabled at the same time on Quest 2", this);
		}
	}

	protected virtual void Start()
	{
		_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		_ = _meshWeightsProviderObject != null;
	}

	protected virtual void Update()
	{
		if (!_faceExpressions.FaceTrackingEnabled || !_faceExpressions.enabled)
		{
			return;
		}
		if (_meshWeightsProvider != null)
		{
			_meshWeightsProvider.UpdateWeights(_faceExpressions);
		}
		if (!_faceExpressions.ValidExpressions)
		{
			return;
		}
		int blendShapeCount = _skinnedMeshRenderer.sharedMesh.blendShapeCount;
		for (int i = 0; i < blendShapeCount; i++)
		{
			if (GetWeightValue(i, out var weightValue))
			{
				_skinnedMeshRenderer.SetBlendShapeWeight(i, Mathf.Clamp(weightValue, 0f, 100f));
			}
		}
	}

	protected internal virtual OVRFaceExpressions.FaceExpression GetFaceExpression(int blendShapeIndex)
	{
		return OVRFaceExpressions.FaceExpression.Invalid;
	}

	protected internal virtual bool GetWeightValue(int blendShapeIndex, out float weightValue)
	{
		if (_meshWeightsProvider != null)
		{
			bool weightValue2 = _meshWeightsProvider.GetWeightValue(blendShapeIndex, out weightValue);
			weightValue *= _blendShapeStrengthMultiplier;
			return weightValue2;
		}
		OVRFaceExpressions.FaceExpression faceExpression = GetFaceExpression(blendShapeIndex);
		if (faceExpression >= OVRFaceExpressions.FaceExpression.Max || faceExpression < OVRFaceExpressions.FaceExpression.BrowLowererL)
		{
			weightValue = 0f;
			return false;
		}
		weightValue = _faceExpressions[faceExpression] * _blendShapeStrengthMultiplier;
		return true;
	}
}
