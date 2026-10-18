using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandVisual : MonoBehaviour, IHandVisual
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	[SerializeField]
	private bool _updateRootPose = true;

	[SerializeField]
	private bool _updateRootScale = true;

	[SerializeField]
	private bool _updateVisibility = true;

	[HideInInspector]
	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	[HideInInspector]
	[SerializeField]
	[Optional]
	private Transform _root;

	[HideInInspector]
	[SerializeField]
	[Optional]
	private MaterialPropertyBlockEditor _handMaterialPropertyBlockEditor;

	[HideInInspector]
	[SerializeField]
	private List<Transform> _jointTransforms = new List<Transform>();

	[SerializeField]
	private SkinnedMeshRenderer _openXRSkinnedMeshRenderer;

	[SerializeField]
	[Optional]
	private Transform _openXRRoot;

	[SerializeField]
	[Optional]
	private MaterialPropertyBlockEditor _openXRHandMaterialPropertyBlockEditor;

	[HideInInspector]
	[SerializeField]
	private List<Transform> _openXRJointTransforms = new List<Transform>();

	private int _wristScalePropertyId;

	private bool _forceOffVisibility;

	private bool _started;

	public IHand Hand { get; private set; }

	public bool IsVisible
	{
		get
		{
			if (SkinnedMeshRenderer != null)
			{
				return SkinnedMeshRenderer.enabled;
			}
			return false;
		}
	}

	public IList<Transform> Joints => _openXRJointTransforms;

	public Transform Root
	{
		get
		{
			return _openXRRoot;
		}
		private set
		{
			_openXRRoot = value;
		}
	}

	private SkinnedMeshRenderer SkinnedMeshRenderer
	{
		get
		{
			return _openXRSkinnedMeshRenderer;
		}
		set
		{
			_openXRSkinnedMeshRenderer = value;
		}
	}

	private MaterialPropertyBlockEditor HandMaterialPropertyBlockEditor
	{
		get
		{
			return _openXRHandMaterialPropertyBlockEditor;
		}
		set
		{
			_openXRHandMaterialPropertyBlockEditor = value;
		}
	}

	public bool ForceOffVisibility
	{
		get
		{
			return _forceOffVisibility;
		}
		set
		{
			_forceOffVisibility = value;
			if (_started)
			{
				UpdateVisibility();
			}
		}
	}

	public event Action WhenHandVisualUpdated = delegate
	{
	};

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
		if (Root == null && Joints.Count > 0 && Joints[0] != null)
		{
			Root = Joints[0].parent;
		}
		if (_root != null)
		{
			_root.gameObject.SetActive(value: false);
		}
		if (_openXRRoot != null)
		{
			_openXRRoot.gameObject.SetActive(value: true);
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		if (HandMaterialPropertyBlockEditor != null)
		{
			_wristScalePropertyId = Shader.PropertyToID("_WristScale");
		}
		UpdateVisibility();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += UpdateSkeleton;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started && _hand != null)
		{
			Hand.WhenHandUpdated -= UpdateSkeleton;
		}
	}

	private void UpdateVisibility()
	{
		if (!_updateVisibility)
		{
			return;
		}
		if (!Hand.IsTrackedDataValid)
		{
			if (IsVisible || ForceOffVisibility)
			{
				SkinnedMeshRenderer.enabled = false;
			}
		}
		else if (!IsVisible && !ForceOffVisibility)
		{
			SkinnedMeshRenderer.enabled = true;
		}
		else if (IsVisible && ForceOffVisibility)
		{
			SkinnedMeshRenderer.enabled = false;
		}
	}

	public void UpdateSkeleton()
	{
		UpdateVisibility();
		if (!Hand.IsTrackedDataValid)
		{
			this.WhenHandVisualUpdated();
			return;
		}
		if (_updateRootPose && Root != null && Hand.GetRootPose(out var pose))
		{
			Root.position = pose.position;
			Root.rotation = pose.rotation;
		}
		if (_updateRootScale && Root != null)
		{
			float num = ((Root.parent != null) ? Root.parent.lossyScale.x : 1f);
			Root.localScale = Hand.Scale / num * Vector3.one;
		}
		if (!Hand.GetJointPosesLocal(out var localJointPoses))
		{
			return;
		}
		for (int i = 0; i < 26; i++)
		{
			if (!(Joints[i] == null))
			{
				Transform obj = Joints[i];
				Pose pose2 = localJointPoses[i];
				obj.SetPose(in pose2, Space.Self);
			}
		}
		if (HandMaterialPropertyBlockEditor != null)
		{
			HandMaterialPropertyBlockEditor.MaterialPropertyBlock.SetFloat(_wristScalePropertyId, Hand.Scale);
			HandMaterialPropertyBlockEditor.UpdateMaterialPropertyBlock();
		}
		this.WhenHandVisualUpdated();
	}

	public Transform GetTransformByHandJointId(HandJointId handJointId)
	{
		return Joints[(int)handJointId];
	}

	public Pose GetJointPose(HandJointId jointId, Space space)
	{
		return GetTransformByHandJointId(jointId).GetPose(space);
	}

	public void InjectAllHandSkeletonVisual(IHand hand, SkinnedMeshRenderer skinnedMeshRenderer)
	{
		InjectHand(hand);
		InjectSkinnedMeshRenderer(skinnedMeshRenderer);
	}

	public void InjectHand(IHand hand)
	{
		_hand = hand as UnityEngine.Object;
		Hand = hand;
	}

	public void InjectSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
	{
		SkinnedMeshRenderer = skinnedMeshRenderer;
	}

	public void InjectOptionalUpdateRootPose(bool updateRootPose)
	{
		_updateRootPose = updateRootPose;
	}

	public void InjectOptionalUpdateRootScale(bool updateRootScale)
	{
		_updateRootScale = updateRootScale;
	}

	public void InjectOptionalRoot(Transform root)
	{
		Root = root;
	}

	public void InjectOptionalMaterialPropertyBlockEditor(MaterialPropertyBlockEditor editor)
	{
		HandMaterialPropertyBlockEditor = editor;
	}
}
