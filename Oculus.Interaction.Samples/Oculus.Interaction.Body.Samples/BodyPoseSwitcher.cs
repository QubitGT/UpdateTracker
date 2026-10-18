using System;
using Oculus.Interaction.Body.Input;
using Oculus.Interaction.Body.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction.Body.Samples;

public class BodyPoseSwitcher : MonoBehaviour, IBodyPose
{
	public enum PoseSource
	{
		PoseA,
		PoseB
	}

	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _poseA;

	private IBodyPose PoseA;

	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _poseB;

	private IBodyPose PoseB;

	[SerializeField]
	private PoseSource _source;

	protected bool _started;

	public ISkeletonMapping SkeletonMapping => GetPose().SkeletonMapping;

	public PoseSource Source
	{
		get
		{
			return _source;
		}
		set
		{
			bool num = value != _source;
			_source = value;
			if (num)
			{
				this.WhenBodyPoseUpdated();
			}
		}
	}

	public event Action WhenBodyPoseUpdated = delegate
	{
	};

	public bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose)
	{
		return GetPose().GetJointPoseFromRoot(bodyJointId, out pose);
	}

	public bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose)
	{
		return GetPose().GetJointPoseLocal(bodyJointId, out pose);
	}

	public void UsePoseA()
	{
		Source = PoseSource.PoseA;
	}

	public void UsePoseB()
	{
		Source = PoseSource.PoseB;
	}

	protected virtual void Awake()
	{
		PoseA = _poseA as IBodyPose;
		PoseB = _poseB as IBodyPose;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			PoseA.WhenBodyPoseUpdated += delegate
			{
				OnPoseUpdated(PoseSource.PoseA);
			};
			PoseB.WhenBodyPoseUpdated += delegate
			{
				OnPoseUpdated(PoseSource.PoseB);
			};
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			PoseA.WhenBodyPoseUpdated -= delegate
			{
				OnPoseUpdated(PoseSource.PoseA);
			};
			PoseB.WhenBodyPoseUpdated -= delegate
			{
				OnPoseUpdated(PoseSource.PoseB);
			};
		}
	}

	private void OnPoseUpdated(PoseSource source)
	{
		if (source == Source)
		{
			this.WhenBodyPoseUpdated();
		}
	}

	private IBodyPose GetPose()
	{
		PoseSource source = Source;
		if (source == PoseSource.PoseA || source != PoseSource.PoseB)
		{
			return PoseA;
		}
		return PoseB;
	}
}
