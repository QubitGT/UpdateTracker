using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class SyntheticControllerInHand : Controller
{
	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	[Optional]
	private UnityEngine.Object _rawHand;

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	[Optional]
	private UnityEngine.Object _syntheticHand;

	private Pose _handToController = Pose.identity;

	private Pose _rootToPointer = Pose.identity;

	private IHand RawHand { get; set; }

	private IHand SyntheticHand { get; set; }

	protected virtual void Awake()
	{
		if (RawHand == null)
		{
			RawHand = _rawHand as IHand;
		}
		if (SyntheticHand == null)
		{
			SyntheticHand = _syntheticHand as IHand;
		}
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_ = _rawHand != null;
		_ = _syntheticHand != null;
		this.EndStart(ref _started);
	}

	protected override void LateUpdate()
	{
		if (_applyModifier)
		{
			UpdateOffsets(ModifyDataFromSource.GetData());
		}
		base.LateUpdate();
	}

	protected override void Apply(ControllerDataAsset data)
	{
		ApplyOffsets(data);
	}

	private void UpdateOffsets(ControllerDataAsset data)
	{
		if (TryGetTrackingRoot(RawHand, data, out var root))
		{
			_handToController = PoseUtils.Delta(in root, in data.RootPose);
			_rootToPointer = PoseUtils.Delta(in data.RootPose, in data.PointerPose);
		}
	}

	private void ApplyOffsets(ControllerDataAsset data)
	{
		if (TryGetTrackingRoot(SyntheticHand, data, out var root))
		{
			PoseUtils.Multiply(in root, in _handToController, ref data.RootPose);
			PoseUtils.Multiply(in data.RootPose, in _rootToPointer, ref data.PointerPose);
		}
	}

	private bool TryGetTrackingRoot(IHand hand, ControllerDataAsset controller, out Pose root)
	{
		if (hand != null && hand.GetRootPose(out root))
		{
			ITrackingToWorldTransformer trackingToWorldTransformer = controller.Config.TrackingToWorldTransformer;
			if (trackingToWorldTransformer != null)
			{
				root = trackingToWorldTransformer.ToTrackingPose(in root);
			}
			return true;
		}
		root = Pose.identity;
		return false;
	}

	public void InjectAllSyntheticControllerInHand(UpdateModeFlags updateMode, IDataSource updateAfter, IDataSource<ControllerDataAsset> modifyDataFromSource, bool applyModifier)
	{
		InjectAllController(updateMode, updateAfter, modifyDataFromSource, applyModifier);
	}

	public void InjectOptionalRawHand(IHand rawHand)
	{
		_rawHand = rawHand as UnityEngine.Object;
		RawHand = rawHand;
	}

	public void InjectOptionalSyntheticHand(IHand syntheticHand)
	{
		_syntheticHand = syntheticHand as UnityEngine.Object;
		SyntheticHand = syntheticHand;
	}
}
