using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[Feature(Feature.BodyTracking)]
public class OVRSkeletonRenderer : MonoBehaviour
{
	public interface IOVRSkeletonRendererDataProvider
	{
		SkeletonRendererData GetSkeletonRendererData();
	}

	public struct SkeletonRendererData
	{
		public float RootScale { get; set; }

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

	private class BoneVisualization
	{
		private GameObject BoneGO;

		private Transform BoneBegin;

		private Transform BoneEnd;

		private LineRenderer Line;

		private Material RenderMaterial;

		private Material SystemGestureMaterial;

		public BoneVisualization(GameObject rootGO, Material renderMat, Material systemGestureMat, float scale, Transform begin, Transform end)
		{
			RenderMaterial = renderMat;
			SystemGestureMaterial = systemGestureMat;
			BoneBegin = begin;
			BoneEnd = end;
			BoneGO = new GameObject(begin.name);
			BoneGO.transform.SetParent(rootGO.transform, worldPositionStays: false);
			Line = BoneGO.AddComponent<LineRenderer>();
			Line.sharedMaterial = RenderMaterial;
			Line.useWorldSpace = true;
			Line.positionCount = 2;
			Line.SetPosition(0, BoneBegin.position);
			Line.SetPosition(1, BoneEnd.position);
			Line.startWidth = 0.005f * scale;
			Line.endWidth = 0.005f * scale;
		}

		public void Update(float scale, bool shouldRender, bool shouldUseSystemGestureMaterial, ConfidenceBehavior confidenceBehavior, SystemGestureBehavior systemGestureBehavior)
		{
			Line.SetPosition(0, BoneBegin.position);
			Line.SetPosition(1, BoneEnd.position);
			Line.startWidth = 0.005f * scale;
			Line.endWidth = 0.005f * scale;
			if (confidenceBehavior == ConfidenceBehavior.ToggleRenderer)
			{
				Line.enabled = shouldRender;
			}
			if (systemGestureBehavior == SystemGestureBehavior.SwapMaterial)
			{
				if (shouldUseSystemGestureMaterial && Line.sharedMaterial != SystemGestureMaterial)
				{
					Line.sharedMaterial = SystemGestureMaterial;
				}
				else if (!shouldUseSystemGestureMaterial && Line.sharedMaterial != RenderMaterial)
				{
					Line.sharedMaterial = RenderMaterial;
				}
			}
		}
	}

	private class CapsuleVisualization
	{
		private GameObject CapsuleGO;

		private OVRBoneCapsule BoneCapsule;

		private Vector3 capsuleScale;

		private MeshRenderer Renderer;

		private Material RenderMaterial;

		private Material SystemGestureMaterial;

		public CapsuleVisualization(GameObject rootGO, Material renderMat, Material systemGestureMat, float scale, OVRBoneCapsule boneCapsule)
		{
			RenderMaterial = renderMat;
			SystemGestureMaterial = systemGestureMat;
			BoneCapsule = boneCapsule;
			CapsuleGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			Object.Destroy(CapsuleGO.GetComponent<CapsuleCollider>());
			Renderer = CapsuleGO.GetComponent<MeshRenderer>();
			Renderer.sharedMaterial = RenderMaterial;
			capsuleScale = Vector3.one;
			capsuleScale.y = boneCapsule.CapsuleCollider.height / 2f;
			capsuleScale.x = boneCapsule.CapsuleCollider.radius * 2f;
			capsuleScale.z = boneCapsule.CapsuleCollider.radius * 2f;
			CapsuleGO.transform.localScale = capsuleScale * scale;
		}

		public void Update(float scale, bool shouldRender, bool shouldUseSystemGestureMaterial, ConfidenceBehavior confidenceBehavior, SystemGestureBehavior systemGestureBehavior)
		{
			if (confidenceBehavior == ConfidenceBehavior.ToggleRenderer && CapsuleGO.activeSelf != shouldRender)
			{
				CapsuleGO.SetActive(shouldRender);
			}
			CapsuleGO.transform.rotation = BoneCapsule.CapsuleCollider.transform.rotation * _capsuleRotationOffset;
			CapsuleGO.transform.position = BoneCapsule.CapsuleCollider.transform.TransformPoint(BoneCapsule.CapsuleCollider.center);
			CapsuleGO.transform.localScale = capsuleScale * scale;
			if (systemGestureBehavior == SystemGestureBehavior.SwapMaterial)
			{
				if (shouldUseSystemGestureMaterial && Renderer.sharedMaterial != SystemGestureMaterial)
				{
					Renderer.sharedMaterial = SystemGestureMaterial;
				}
				else if (!shouldUseSystemGestureMaterial && Renderer.sharedMaterial != RenderMaterial)
				{
					Renderer.sharedMaterial = RenderMaterial;
				}
			}
		}
	}

	[SerializeField]
	private IOVRSkeletonRendererDataProvider _dataProvider;

	[SerializeField]
	private ConfidenceBehavior _confidenceBehavior = ConfidenceBehavior.ToggleRenderer;

	[SerializeField]
	private SystemGestureBehavior _systemGestureBehavior = SystemGestureBehavior.SwapMaterial;

	[SerializeField]
	private bool _renderPhysicsCapsules;

	[SerializeField]
	private Material _skeletonMaterial;

	private Material _skeletonDefaultMaterial;

	[SerializeField]
	private Material _capsuleMaterial;

	private Material _capsuleDefaultMaterial;

	[SerializeField]
	private Material _systemGestureMaterial;

	private Material _systemGestureDefaultMaterial;

	private const float LINE_RENDERER_WIDTH = 0.005f;

	private List<BoneVisualization> _boneVisualizations;

	private List<CapsuleVisualization> _capsuleVisualizations;

	private OVRSkeleton _ovrSkeleton;

	private GameObject _skeletonGO;

	private float _scale;

	private static readonly Quaternion _capsuleRotationOffset = Quaternion.Euler(0f, 0f, 90f);

	public bool IsInitialized { get; private set; }

	public bool IsDataValid { get; private set; }

	public bool IsDataHighConfidence { get; private set; }

	public bool ShouldUseSystemGestureMaterial { get; private set; }

	private void Awake()
	{
		if (_dataProvider == null)
		{
			_dataProvider = GetComponent<IOVRSkeletonRendererDataProvider>();
		}
		if (_ovrSkeleton == null)
		{
			_ovrSkeleton = GetComponent<OVRSkeleton>();
		}
	}

	private void Start()
	{
		if (_ovrSkeleton == null)
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
		return _ovrSkeleton.IsInitialized;
	}

	private void Initialize()
	{
		_boneVisualizations = new List<BoneVisualization>();
		_capsuleVisualizations = new List<CapsuleVisualization>();
		_ovrSkeleton = GetComponent<OVRSkeleton>();
		_skeletonGO = new GameObject("SkeletonRenderer");
		_skeletonGO.transform.SetParent(base.transform, worldPositionStays: false);
		if (_skeletonMaterial == null)
		{
			_skeletonDefaultMaterial = new Material(Shader.Find("Diffuse"));
			_skeletonMaterial = _skeletonDefaultMaterial;
		}
		if (_capsuleMaterial == null)
		{
			_capsuleDefaultMaterial = new Material(Shader.Find("Diffuse"));
			_capsuleMaterial = _capsuleDefaultMaterial;
		}
		if (_systemGestureMaterial == null)
		{
			_systemGestureDefaultMaterial = new Material(Shader.Find("Diffuse"));
			_systemGestureDefaultMaterial.color = Color.blue;
			_systemGestureMaterial = _systemGestureDefaultMaterial;
		}
		if (!_ovrSkeleton.IsInitialized)
		{
			return;
		}
		for (int i = 0; i < _ovrSkeleton.Bones.Count; i++)
		{
			BoneVisualization item = new BoneVisualization(_skeletonGO, _skeletonMaterial, _systemGestureMaterial, _scale, _ovrSkeleton.Bones[i].Transform, _ovrSkeleton.Bones[i].Transform.parent);
			_boneVisualizations.Add(item);
		}
		if (_renderPhysicsCapsules && _ovrSkeleton.Capsules != null)
		{
			for (int j = 0; j < _ovrSkeleton.Capsules.Count; j++)
			{
				CapsuleVisualization item2 = new CapsuleVisualization(_skeletonGO, _capsuleMaterial, _systemGestureMaterial, _scale, _ovrSkeleton.Capsules[j]);
				_capsuleVisualizations.Add(item2);
			}
		}
		IsInitialized = true;
	}

	public void Update()
	{
		IsDataValid = false;
		IsDataHighConfidence = false;
		ShouldUseSystemGestureMaterial = false;
		if (!IsInitialized)
		{
			return;
		}
		bool shouldRender = false;
		if (_dataProvider != null)
		{
			SkeletonRendererData skeletonRendererData = _dataProvider.GetSkeletonRendererData();
			IsDataValid = skeletonRendererData.IsDataValid;
			IsDataHighConfidence = skeletonRendererData.IsDataHighConfidence;
			ShouldUseSystemGestureMaterial = skeletonRendererData.ShouldUseSystemGestureMaterial;
			shouldRender = skeletonRendererData.IsDataValid && skeletonRendererData.IsDataHighConfidence;
			if (skeletonRendererData.IsDataValid)
			{
				_scale = skeletonRendererData.RootScale;
			}
		}
		for (int i = 0; i < _boneVisualizations.Count; i++)
		{
			_boneVisualizations[i].Update(_scale, shouldRender, ShouldUseSystemGestureMaterial, _confidenceBehavior, _systemGestureBehavior);
		}
		for (int j = 0; j < _capsuleVisualizations.Count; j++)
		{
			_capsuleVisualizations[j].Update(_scale, shouldRender, ShouldUseSystemGestureMaterial, _confidenceBehavior, _systemGestureBehavior);
		}
	}

	private void OnDestroy()
	{
		if (_skeletonDefaultMaterial != null)
		{
			Object.DestroyImmediate(_skeletonDefaultMaterial, allowDestroyingAssets: false);
		}
		if (_capsuleDefaultMaterial != null)
		{
			Object.DestroyImmediate(_capsuleDefaultMaterial, allowDestroyingAssets: false);
		}
		if (_systemGestureDefaultMaterial != null)
		{
			Object.DestroyImmediate(_systemGestureDefaultMaterial, allowDestroyingAssets: false);
		}
	}
}
