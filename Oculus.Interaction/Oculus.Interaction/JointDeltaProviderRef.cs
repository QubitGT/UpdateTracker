using System;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction;

public class JointDeltaProviderRef : MonoBehaviour, IJointDeltaProvider
{
	[SerializeField]
	[Interface(typeof(IJointDeltaProvider), new Type[] { })]
	private UnityEngine.Object _jointDeltaProvider;

	public IJointDeltaProvider JointDeltaProvider { get; private set; }

	protected virtual void Awake()
	{
		JointDeltaProvider = _jointDeltaProvider as IJointDeltaProvider;
	}

	protected virtual void Start()
	{
	}

	public bool GetPositionDelta(HandJointId joint, out Vector3 delta)
	{
		return JointDeltaProvider.GetPositionDelta(joint, out delta);
	}

	public bool GetRotationDelta(HandJointId joint, out Quaternion delta)
	{
		return JointDeltaProvider.GetRotationDelta(joint, out delta);
	}

	public void RegisterConfig(JointDeltaConfig config)
	{
		JointDeltaProvider.RegisterConfig(config);
	}

	public void UnRegisterConfig(JointDeltaConfig config)
	{
		JointDeltaProvider.UnRegisterConfig(config);
	}

	public void InjectAllJointDeltaProviderRef(IJointDeltaProvider jointDeltaProvider)
	{
		InjectJointDeltaProvider(jointDeltaProvider);
	}

	public void InjectJointDeltaProvider(IJointDeltaProvider jointDeltaProvider)
	{
		_jointDeltaProvider = jointDeltaProvider as UnityEngine.Object;
		JointDeltaProvider = jointDeltaProvider;
	}
}
