using System;
using UnityEngine;

namespace Oculus.Interaction.Throw;

[Obsolete("Use Grabbable instead")]
public class RANSACVelocityCalculator : MonoBehaviour, IThrowVelocityCalculator, ITimeConsumer
{
	private class RANSACOffsettedVelocity : RANSACVelocity
	{
		private Pose _offset = Pose.identity;

		[Obsolete("The minHighConfidenceSamples parameter will be ignored. Use the constructor without it")]
		public RANSACOffsettedVelocity(int samplesCount = 10, int samplesDeadZone = 2, int minHighConfidenceSamples = 2)
			: base(samplesCount, samplesDeadZone)
		{
		}

		public RANSACOffsettedVelocity(int samplesCount = 10, int samplesDeadZone = 2)
			: base(samplesCount, samplesDeadZone)
		{
		}

		public void GetOffsettedVelocities(Pose offset, out Vector3 velocity, out Vector3 torque)
		{
			_offset = offset;
			GetVelocities(out velocity, out torque);
			_offset = Pose.identity;
		}

		protected override Vector3 PositionOffset(Pose youngerPose, Pose olderPose)
		{
			return PoseUtils.Multiply(in youngerPose, in _offset).position - PoseUtils.Multiply(in olderPose, in _offset).position;
		}
	}

	[SerializeField]
	[Interface(typeof(IPoseInputDevice), new Type[] { })]
	private UnityEngine.Object _poseInputDevice;

	private Func<float> _timeProvider = () => Time.time;

	private float _previousPositionId;

	private RANSACOffsettedVelocity _ransac = new RANSACOffsettedVelocity(8, 2);

	public IPoseInputDevice PoseInputDevice { get; private set; }

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		PoseInputDevice = _poseInputDevice as IPoseInputDevice;
	}

	protected virtual void Start()
	{
		_ransac.Initialize();
	}

	private void Update()
	{
		ProcessInput();
	}

	public ReleaseVelocityInformation CalculateThrowVelocity(Transform objectThrown)
	{
		ProcessInput();
		return GetThrowInformation(objectThrown.GetPose());
	}

	private void ProcessInput()
	{
		PoseInputDevice.GetRootPose(out var pose);
		bool isHighConfidence = PoseInputDevice.IsInputValid && PoseInputDevice.IsHighConfidence && pose.position.sqrMagnitude != _previousPositionId;
		_ransac.Process(pose, _timeProvider(), isHighConfidence);
		_previousPositionId = pose.position.sqrMagnitude;
	}

	private ReleaseVelocityInformation GetThrowInformation(Pose grabPoint)
	{
		Vector3 position = grabPoint.position;
		PoseInputDevice.GetRootPose(out var pose);
		Pose offset = PoseUtils.Delta(in pose, in grabPoint);
		_ransac.GetOffsettedVelocities(offset, out var velocity, out var torque);
		return new ReleaseVelocityInformation(velocity, torque, position, isSelectedVelocity: true);
	}

	public void InjectAllRANSACVelocityCalculator(IPoseInputDevice poseInputDevice)
	{
		InjectPoseInputDevice(poseInputDevice);
	}

	public void InjectPoseInputDevice(IPoseInputDevice poseInputDevice)
	{
		PoseInputDevice = poseInputDevice;
		_poseInputDevice = poseInputDevice as UnityEngine.Object;
	}
}
