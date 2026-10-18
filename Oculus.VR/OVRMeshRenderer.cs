using UnityEngine;

public class OVRMeshRenderer : MonoBehaviour
{
	public interface IOVRMeshRendererDataProvider
	{
		MeshRendererData GetMeshRendererData();
	}

	public struct MeshRendererData
	{
		public bool IsDataValid { get; set; }

		public bool IsDataHighConfidence { get; set; }

		public bool ShouldUseSystemGestureMaterial { get; set; }
	}

	public enum ConfidenceBehavior
	{
		None,
		ToggleRenderer
	}

	public enum SystemGestureBehavior
	{
		None,
		SwapMaterial
	}

	[SerializeField]
	private IOVRMeshRendererDataProvider _dataProvider;

	[SerializeField]
	private OVRMesh _ovrMesh;

	[SerializeField]
	private OVRSkeleton _ovrSkeleton;

	[SerializeField]
	private ConfidenceBehavior _confidenceBehavior = ConfidenceBehavior.ToggleRenderer;

	[SerializeField]
	private SystemGestureBehavior _systemGestureBehavior = SystemGestureBehavior.SwapMaterial;

	[SerializeField]
	private Material _systemGestureMaterial;

	private Material _originalMaterial;

	private SkinnedMeshRenderer _skinnedMeshRenderer;

	private static readonly Matrix4x4 _openXRFixup = Matrix4x4.Rotate(new Quaternion(0f, 1f, 0f, 0f));

	public bool IsInitialized { get; private set; }

	public bool IsDataValid { get; private set; }

	public bool IsDataHighConfidence { get; private set; }

	public bool ShouldUseSystemGestureMaterial { get; private set; }

	private void Awake()
	{
		if (_dataProvider == null)
		{
			_dataProvider = GetComponent<IOVRMeshRendererDataProvider>();
		}
		if (_ovrMesh == null)
		{
			_ovrMesh = GetComponent<OVRMesh>();
		}
		if (_ovrSkeleton == null)
		{
			_ovrSkeleton = GetComponent<OVRSkeleton>();
		}
	}

	private void Start()
	{
		if (_ovrMesh == null)
		{
			base.enabled = false;
		}
		else if (ShouldInitialize())
		{
			Initialize();
		}
	}

	private bool ShouldInitialize()
	{
		if (IsInitialized)
		{
			return false;
		}
		if (_ovrMesh == null || (_ovrMesh != null && !_ovrMesh.IsInitialized) || (_ovrSkeleton != null && !_ovrSkeleton.IsInitialized))
		{
			return false;
		}
		return true;
	}

	public void ForceRebind()
	{
		IsInitialized = false;
		Initialize();
	}

	private void Initialize()
	{
		_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		if (!_skinnedMeshRenderer)
		{
			_skinnedMeshRenderer = base.gameObject.AddComponent<SkinnedMeshRenderer>();
		}
		_skinnedMeshRenderer.sharedMesh = _ovrMesh.Mesh;
		_originalMaterial = _skinnedMeshRenderer.sharedMaterial;
		if (_ovrSkeleton != null)
		{
			OVRSkeleton.SkeletonType skeletonType = _ovrSkeleton.GetSkeletonType();
			int currentNumSkinnableBones = _ovrSkeleton.GetCurrentNumSkinnableBones();
			Matrix4x4[] array = new Matrix4x4[currentNumSkinnableBones];
			Transform[] array2 = new Transform[currentNumSkinnableBones];
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			for (int i = 0; i < currentNumSkinnableBones && i < _ovrSkeleton.Bones.Count; i++)
			{
				array2[i] = _ovrSkeleton.Bones[i].Transform;
				array[i] = _ovrSkeleton.BindPoses[i].Transform.worldToLocalMatrix * localToWorldMatrix;
				if (skeletonType.IsOpenXRHandSkeleton())
				{
					array[i] *= _openXRFixup;
				}
			}
			_ovrMesh.Mesh.bindposes = array;
			_skinnedMeshRenderer.bones = array2;
			_skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		IsInitialized = true;
	}

	private void Update()
	{
		IsDataValid = false;
		IsDataHighConfidence = false;
		ShouldUseSystemGestureMaterial = false;
		if (!IsInitialized)
		{
			return;
		}
		bool flag = false;
		if (_dataProvider != null)
		{
			MeshRendererData meshRendererData = _dataProvider.GetMeshRendererData();
			IsDataValid = meshRendererData.IsDataValid;
			IsDataHighConfidence = meshRendererData.IsDataHighConfidence;
			ShouldUseSystemGestureMaterial = meshRendererData.ShouldUseSystemGestureMaterial;
			flag = meshRendererData.IsDataValid && meshRendererData.IsDataHighConfidence;
		}
		if (_confidenceBehavior == ConfidenceBehavior.ToggleRenderer && _skinnedMeshRenderer != null && _skinnedMeshRenderer.enabled != flag)
		{
			_skinnedMeshRenderer.enabled = flag;
		}
		if (_systemGestureBehavior == SystemGestureBehavior.SwapMaterial && _skinnedMeshRenderer != null)
		{
			if (ShouldUseSystemGestureMaterial && _systemGestureMaterial != null && _skinnedMeshRenderer.sharedMaterial != _systemGestureMaterial)
			{
				_skinnedMeshRenderer.sharedMaterial = _systemGestureMaterial;
			}
			else if (!ShouldUseSystemGestureMaterial && _originalMaterial != null && _skinnedMeshRenderer.sharedMaterial != _originalMaterial)
			{
				_skinnedMeshRenderer.sharedMaterial = _originalMaterial;
			}
		}
	}
}
