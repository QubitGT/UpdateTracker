using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class Controller : DataModifier<ControllerDataAsset>, IController
{
	public virtual Handedness Handedness => GetData().Config.Handedness;

	public virtual bool IsConnected
	{
		get
		{
			ControllerDataAsset data = GetData();
			if (data.IsDataValid)
			{
				return data.IsConnected;
			}
			return false;
		}
	}

	public virtual bool IsPoseValid
	{
		get
		{
			ControllerDataAsset data = GetData();
			if (data.IsDataValid)
			{
				return data.RootPoseOrigin != PoseOrigin.None;
			}
			return false;
		}
	}

	public virtual bool IsPointerPoseValid
	{
		get
		{
			ControllerDataAsset data = GetData();
			if (data.IsDataValid)
			{
				return data.PointerPoseOrigin != PoseOrigin.None;
			}
			return false;
		}
	}

	public virtual ControllerInput ControllerInput => GetData().Input;

	private ITrackingToWorldTransformer TrackingToWorldTransformer => GetData().Config.TrackingToWorldTransformer;

	public virtual float Scale
	{
		get
		{
			if (TrackingToWorldTransformer == null)
			{
				return 1f;
			}
			return TrackingToWorldTransformer.Transform.lossyScale.x;
		}
	}

	public virtual event Action WhenUpdated = delegate
	{
	};

	public virtual bool IsButtonUsageAnyActive(ControllerButtonUsage buttonUsage)
	{
		ControllerDataAsset data = GetData();
		if (data.IsDataValid)
		{
			return (buttonUsage & data.Input.ButtonUsageMask) != 0;
		}
		return false;
	}

	public virtual bool IsButtonUsageAllActive(ControllerButtonUsage buttonUsage)
	{
		ControllerDataAsset data = GetData();
		if (data.IsDataValid)
		{
			return (buttonUsage & data.Input.ButtonUsageMask) == buttonUsage;
		}
		return false;
	}

	public virtual bool TryGetPose(out Pose pose)
	{
		if (!IsPoseValid)
		{
			pose = Pose.identity;
			return false;
		}
		pose = GetData().Config.TrackingToWorldTransformer.ToWorldPose(GetData().RootPose);
		return true;
	}

	public virtual bool TryGetPointerPose(out Pose pose)
	{
		if (!IsPointerPoseValid)
		{
			pose = Pose.identity;
			return false;
		}
		pose = GetData().Config.TrackingToWorldTransformer.ToWorldPose(GetData().PointerPose);
		return true;
	}

	public override void MarkInputDataRequiresUpdate()
	{
		base.MarkInputDataRequiresUpdate();
		if (base.Started)
		{
			this.WhenUpdated();
		}
	}

	protected override void Apply(ControllerDataAsset data)
	{
	}

	public void InjectAllController(UpdateModeFlags updateMode, IDataSource updateAfter, IDataSource<ControllerDataAsset> modifyDataFromSource, bool applyModifier)
	{
		InjectAllDataModifier(updateMode, updateAfter, modifyDataFromSource, applyModifier);
	}
}
