using System;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleMeshDrawer : InteractorReticle<ReticleDataMesh>
{
	[Tooltip("The hand grab interactor that uses the reticle.")]
	[FormerlySerializedAs("_handGrabber")]
	[SerializeField]
	[Interface(typeof(IHandGrabInteractor), new Type[] { typeof(IInteractorView) })]
	private UnityEngine.Object _handGrabInteractor;

	[Tooltip("The ReticleMesh prefab's mesh filter.")]
	[SerializeField]
	private MeshFilter _filter;

	[Tooltip("The ReticleMesh prefab's mesh renderer.")]
	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private PoseTravelData _travelData = PoseTravelData.FAST;

	private Tween _tween;

	private IHandGrabInteractor HandGrabInteractor { get; set; }

	public PoseTravelData TravelData
	{
		get
		{
			return _travelData;
		}
		set
		{
			_travelData = value;
		}
	}

	protected override IInteractorView Interactor { get; set; }

	protected override Component InteractableComponent => HandGrabInteractor.TargetInteractable as Component;

	protected virtual void Reset()
	{
		_filter = GetComponent<MeshFilter>();
		_renderer = GetComponent<MeshRenderer>();
	}

	protected virtual void Awake()
	{
		HandGrabInteractor = _handGrabInteractor as IHandGrabInteractor;
		Interactor = _handGrabInteractor as IInteractorView;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void Draw(ReticleDataMesh dataMesh)
	{
		_filter.sharedMesh = dataMesh.Filter.sharedMesh;
		_filter.transform.localScale = dataMesh.Filter.transform.lossyScale;
		_renderer.enabled = true;
		Pose to = DestinationPose(dataMesh, HandGrabInteractor.GetTargetGrabPose());
		ref PoseTravelData travelData = ref _travelData;
		Pose from = dataMesh.Target.GetPose();
		_tween = travelData.CreateTween(in from, in to);
	}

	protected override void Hide()
	{
		_tween = null;
		_renderer.enabled = false;
	}

	protected override void Align(ReticleDataMesh data)
	{
		Pose target = DestinationPose(data, HandGrabInteractor.GetTargetGrabPose());
		_tween.UpdateTarget(target);
		_tween.Tick();
		Transform obj = _filter.transform;
		Pose pose = _tween.Pose;
		obj.SetPose(in pose);
	}

	private Pose DestinationPose(ReticleDataMesh data, Pose worldSnapPose)
	{
		Pose to = data.Target.GetPose();
		Pose b = PoseUtils.Delta(in worldSnapPose, in to);
		HandGrabInteractor.HandGrabApi.Hand.GetRootPose(out var pose);
		to = HandGrabInteractor.WristToGrabPoseOffset;
		pose.Premultiply(in to);
		pose.Premultiply(in b);
		return pose;
	}

	public void InjectAllReticleMeshDrawer(IHandGrabInteractor handGrabInteractor, MeshFilter filter, MeshRenderer renderer)
	{
		InjectHandGrabInteractor(handGrabInteractor);
		InjectFilter(filter);
		InjectRenderer(renderer);
	}

	public void InjectHandGrabInteractor(IHandGrabInteractor handGrabInteractor)
	{
		_handGrabInteractor = handGrabInteractor as UnityEngine.Object;
		HandGrabInteractor = handGrabInteractor;
		Interactor = handGrabInteractor as IInteractorView;
	}

	public void InjectFilter(MeshFilter filter)
	{
		_filter = filter;
	}

	public void InjectRenderer(MeshRenderer renderer)
	{
		_renderer = renderer;
	}
}
