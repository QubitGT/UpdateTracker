using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class Hmd : DataModifier<HmdDataAsset>, IHmd
{
	public event Action WhenUpdated = delegate
	{
	};

	protected override void Apply(HmdDataAsset data)
	{
	}

	public override void MarkInputDataRequiresUpdate()
	{
		base.MarkInputDataRequiresUpdate();
		if (base.Started)
		{
			this.WhenUpdated();
		}
	}

	public bool TryGetRootPose(out Pose pose)
	{
		HmdDataAsset data = GetData();
		if (!data.IsTracked)
		{
			pose = Pose.identity;
			return false;
		}
		ITrackingToWorldTransformer trackingToWorldTransformer = GetData().Config.TrackingToWorldTransformer;
		pose = trackingToWorldTransformer.ToWorldPose(data.Root);
		return true;
	}
}
