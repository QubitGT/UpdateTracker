using System;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.PoseDetection;

public class PoseFromBody : MonoBehaviour, IBodyPose
{
	[Tooltip("The IBodyPose will be derived from this IBody.")]
	[SerializeField]
	[Interface(typeof(IBody), new Type[] { })]
	private UnityEngine.Object _body;

	private IBody Body;

	[Tooltip("If true, this component will track the provided IBody as its data is updated. If false, you must call UpdatePose to update joint data.")]
	[SerializeField]
	private bool _autoUpdate = true;

	protected bool _started;

	private Dictionary<BodyJointId, Pose> _jointPosesLocal;

	private Dictionary<BodyJointId, Pose> _jointPosesFromRoot;

	public bool AutoUpdate
	{
		get
		{
			return _autoUpdate;
		}
		set
		{
			_autoUpdate = value;
		}
	}

	public ISkeletonMapping SkeletonMapping => Body.SkeletonMapping;

	public event Action WhenBodyPoseUpdated = delegate
	{
	};

	public bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose)
	{
		return _jointPosesLocal.TryGetValue(bodyJointId, out pose);
	}

	public bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose)
	{
		return _jointPosesFromRoot.TryGetValue(bodyJointId, out pose);
	}

	protected virtual void Awake()
	{
		_jointPosesLocal = new Dictionary<BodyJointId, Pose>();
		_jointPosesFromRoot = new Dictionary<BodyJointId, Pose>();
		Body = _body as IBody;
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
			Body.WhenBodyUpdated += Body_WhenBodyUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Body.WhenBodyUpdated -= Body_WhenBodyUpdated;
		}
	}

	private void Body_WhenBodyUpdated()
	{
		if (_autoUpdate)
		{
			UpdatePose();
		}
	}

	public void UpdatePose()
	{
		_jointPosesLocal.Clear();
		_jointPosesFromRoot.Clear();
		foreach (BodyJointId joint in Body.SkeletonMapping.Joints)
		{
			if (Body.GetJointPoseLocal(joint, out var pose))
			{
				_jointPosesLocal[joint] = pose;
			}
			if (Body.GetJointPoseFromRoot(joint, out var pose2))
			{
				_jointPosesFromRoot[joint] = pose2;
			}
		}
		this.WhenBodyPoseUpdated();
	}

	public void InjectAllPoseFromBody(IBody body)
	{
		InjectBody(body);
	}

	public void InjectBody(IBody body)
	{
		_body = body as UnityEngine.Object;
		Body = body;
	}
}
