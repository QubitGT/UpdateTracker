using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Throw;

[Obsolete("Use RANSACVelocityCalculator instead")]
public class StandardVelocityCalculator : MonoBehaviour, IVelocityCalculator, IThrowVelocityCalculator, ITimeConsumer
{
	[Serializable]
	public class BufferingParams
	{
		public float BufferLengthSeconds = 0.4f;

		public float SampleFrequency = 90f;

		public void Validate()
		{
		}
	}

	private struct SamplePoseData
	{
		public readonly Pose TransformPose;

		public readonly Vector3 LinearVelocity;

		public readonly Vector3 AngularVelocity;

		public readonly float Time;

		public SamplePoseData(Pose transformPose, Vector3 linearVelocity, Vector3 angularVelocity, float time)
		{
			TransformPose = transformPose;
			LinearVelocity = linearVelocity;
			AngularVelocity = angularVelocity;
			Time = time;
		}
	}

	[SerializeField]
	[Interface(typeof(IPoseInputDevice), new Type[] { })]
	private UnityEngine.Object _throwInputDevice;

	[SerializeField]
	[Tooltip("The reference position is the center of mass of the hand or controller. Use this offset this in case the computed center of mass is not entirely correct.")]
	private Vector3 _referenceOffset = Vector3.zero;

	[SerializeField]
	[Tooltip("Related to buffering velocities; used for final velocity calculation.")]
	private BufferingParams _bufferingParams;

	[SerializeField]
	[Tooltip("Influence of latest velocities upon release.")]
	[Range(0f, 1f)]
	private float _instantVelocityInfluence = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Influence of derived velocities trend upon release.")]
	private float _trendVelocityInfluence = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Influence of tangential velcities upon release, which can be affected by rotational motion.")]
	private float _tangentialVelocityInfluence = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Influence of external velocities upon release. For hands, this can include fingers.")]
	private float _externalVelocityInfluence;

	[SerializeField]
	[Tooltip("Time of anticipated release. Hand tracking might experience greater latency compared to controllers.")]
	private float _stepBackTime = 0.08f;

	[SerializeField]
	[Tooltip("Trend velocity uses a window of velocities, assuming not too many of those velocities are zero. If they exceed a max percentage then a last resort method is used.")]
	private float _maxPercentZeroSamplesTrendVeloc = 0.5f;

	[Header("Sampling filtering.")]
	[SerializeField]
	private OneEuroFilterPropertyBlock _filterProps = OneEuroFilterPropertyBlock.Default;

	private float _updateFrequency = -1f;

	private float _updateLatency = -1f;

	private float _lastUpdateTime = -1f;

	private IOneEuroFilter<Vector3> _linearVelocityFilter;

	private Func<float> _timeProvider = () => Time.time;

	private List<ReleaseVelocityInformation> _currentThrowVelocities = new List<ReleaseVelocityInformation>();

	private Vector3 _linearVelocity = Vector3.zero;

	private Vector3 _angularVelocity = Vector3.zero;

	private Vector3? _previousReferencePosition;

	private Quaternion? _previousReferenceRotation;

	private float _accumulatedDelta;

	private List<SamplePoseData> _bufferedPoses = new List<SamplePoseData>();

	private int _lastWritePos = -1;

	private int _bufferSize = -1;

	private List<SamplePoseData> _windowWithMovement = new List<SamplePoseData>();

	private List<SamplePoseData> _tempWindow = new List<SamplePoseData>();

	private const float _TREND_DOT_THRESHOLD = 0.6f;

	public IPoseInputDevice ThrowInputDevice { get; private set; }

	public float UpdateFrequency => _updateFrequency;

	public Vector3 ReferenceOffset
	{
		get
		{
			return _referenceOffset;
		}
		set
		{
			_referenceOffset = value;
		}
	}

	public float InstantVelocityInfluence
	{
		get
		{
			return _instantVelocityInfluence;
		}
		set
		{
			_instantVelocityInfluence = value;
		}
	}

	public float TrendVelocityInfluence
	{
		get
		{
			return _trendVelocityInfluence;
		}
		set
		{
			_trendVelocityInfluence = value;
		}
	}

	public float TangentialVelocityInfluence
	{
		get
		{
			return _tangentialVelocityInfluence;
		}
		set
		{
			_tangentialVelocityInfluence = value;
		}
	}

	public float ExternalVelocityInfluence
	{
		get
		{
			return _externalVelocityInfluence;
		}
		set
		{
			_externalVelocityInfluence = value;
		}
	}

	public float StepBackTime
	{
		get
		{
			return _stepBackTime;
		}
		set
		{
			_stepBackTime = value;
		}
	}

	public float MaxPercentZeroSamplesTrendVeloc
	{
		get
		{
			return _maxPercentZeroSamplesTrendVeloc;
		}
		set
		{
			_maxPercentZeroSamplesTrendVeloc = value;
		}
	}

	public Vector3 AddedInstantLinearVelocity { get; private set; }

	public Vector3 AddedTrendLinearVelocity { get; private set; }

	public Vector3 AddedTangentialLinearVelocity { get; private set; }

	public Vector3 AxisOfRotation { get; private set; }

	public Vector3 CenterOfMassToObject { get; private set; }

	public Vector3 TangentialDirection { get; private set; }

	public Vector3 AxisOfRotationOrigin { get; private set; }

	public event Action<List<ReleaseVelocityInformation>> WhenThrowVelocitiesChanged = delegate
	{
	};

	public event Action<ReleaseVelocityInformation> WhenNewSampleAvailable = delegate
	{
	};

	public void SetTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}

	protected virtual void Awake()
	{
		ThrowInputDevice = _throwInputDevice as IPoseInputDevice;
	}

	protected virtual void Start()
	{
		_bufferingParams.Validate();
		_bufferSize = Mathf.CeilToInt(_bufferingParams.BufferLengthSeconds * _bufferingParams.SampleFrequency);
		_bufferedPoses.Capacity = _bufferSize;
		_linearVelocityFilter = OneEuroFilter.CreateVector3();
	}

	public ReleaseVelocityInformation CalculateThrowVelocity(Transform objectThrown)
	{
		Vector3 linearVelocity = Vector3.zero;
		Vector3 angularVelocity = Vector3.zero;
		IncludeInstantVelocities(_timeProvider(), ref linearVelocity, ref angularVelocity);
		IncludeTrendVelocities(ref linearVelocity, ref angularVelocity);
		IncludeTangentialInfluence(ref linearVelocity, objectThrown.position);
		IncludeExternalVelocities(ref linearVelocity, ref angularVelocity);
		_currentThrowVelocities.Clear();
		int count = _bufferedPoses.Count;
		int num = _lastWritePos;
		for (int i = 0; i < count; i++)
		{
			if (num < 0)
			{
				num = count - 1;
			}
			SamplePoseData samplePoseData = _bufferedPoses[num];
			ReleaseVelocityInformation item = new ReleaseVelocityInformation(samplePoseData.LinearVelocity, samplePoseData.AngularVelocity, samplePoseData.TransformPose.position);
			_currentThrowVelocities.Add(item);
			num--;
		}
		ReleaseVelocityInformation releaseVelocityInformation = new ReleaseVelocityInformation(linearVelocity, angularVelocity, _previousReferencePosition.HasValue ? _previousReferencePosition.Value : Vector3.zero, isSelectedVelocity: true);
		_currentThrowVelocities.Add(releaseVelocityInformation);
		this.WhenThrowVelocitiesChanged(_currentThrowVelocities);
		_bufferedPoses.Clear();
		_lastWritePos = -1;
		_linearVelocityFilter.Reset();
		return releaseVelocityInformation;
	}

	private void IncludeInstantVelocities(float currentTime, ref Vector3 linearVelocity, ref Vector3 angularVelocity)
	{
		Vector3 linearVelocity2 = Vector3.zero;
		Vector3 angularVelocity2 = Vector3.zero;
		IncludeEstimatedReleaseVelocities(currentTime, ref linearVelocity2, ref angularVelocity2);
		AddedInstantLinearVelocity = linearVelocity2 * _instantVelocityInfluence;
		linearVelocity += AddedInstantLinearVelocity;
		angularVelocity += angularVelocity2 * _instantVelocityInfluence;
	}

	private void IncludeEstimatedReleaseVelocities(float currentTime, ref Vector3 linearVelocity, ref Vector3 angularVelocity)
	{
		linearVelocity = _linearVelocity;
		angularVelocity = _angularVelocity;
		if (!(_stepBackTime < Mathf.Epsilon))
		{
			float num = currentTime - _stepBackTime;
			var (num2, num3) = FindPoseIndicesAdjacentToTime(num);
			if (num2 >= 0 && num3 >= 0)
			{
				SamplePoseData samplePoseData = _bufferedPoses[num2];
				SamplePoseData samplePoseData2 = _bufferedPoses[num3];
				float time = samplePoseData.Time;
				float time2 = samplePoseData2.Time;
				float t = (num - time) / (time2 - time);
				Vector3 vector = Vector3.Lerp(samplePoseData.LinearVelocity, samplePoseData2.LinearVelocity, t);
				Quaternion a = VelocityCalculatorUtilMethods.AngularVelocityToQuat(samplePoseData.AngularVelocity);
				Quaternion b = VelocityCalculatorUtilMethods.AngularVelocityToQuat(samplePoseData2.AngularVelocity);
				Vector3 vector2 = VelocityCalculatorUtilMethods.QuatToAngularVeloc(Quaternion.Slerp(a, b, t));
				linearVelocity = vector;
				angularVelocity = vector2;
			}
		}
	}

	private void IncludeTrendVelocities(ref Vector3 linearVelocity, ref Vector3 angularVelocity)
	{
		(Vector3, Vector3) tuple = ComputeTrendVelocities();
		Vector3 item = tuple.Item1;
		Vector3 item2 = tuple.Item2;
		AddedTrendLinearVelocity = item * _trendVelocityInfluence;
		linearVelocity += AddedTrendLinearVelocity;
		angularVelocity += item2 * _trendVelocityInfluence;
	}

	private void IncludeTangentialInfluence(ref Vector3 linearVelocity, Vector3 interactablePosition)
	{
		Vector3 vector = CalculateTangentialVector(interactablePosition);
		AddedTangentialLinearVelocity = vector * _tangentialVelocityInfluence;
		linearVelocity += AddedTangentialLinearVelocity;
	}

	private void IncludeExternalVelocities(ref Vector3 linearVelocity, ref Vector3 angularVelocity)
	{
		(Vector3, Vector3) externalVelocities = ThrowInputDevice.GetExternalVelocities();
		Vector3 item = externalVelocities.Item1;
		Vector3 item2 = externalVelocities.Item2;
		float num = item.magnitude * _externalVelocityInfluence;
		linearVelocity += linearVelocity.normalized * num;
		float num2 = item2.magnitude * _externalVelocityInfluence;
		angularVelocity += angularVelocity.normalized * num2;
	}

	private (int, int) FindPoseIndicesAdjacentToTime(float time)
	{
		if (_lastWritePos < 0)
		{
			return (-1, -1);
		}
		int item = -1;
		int item2 = -1;
		int count = _bufferedPoses.Count;
		int num = _lastWritePos;
		for (int i = 0; i < count; i++)
		{
			if (num < 0)
			{
				num = count - 1;
			}
			int num2 = num - 1;
			if (num2 < 0)
			{
				num2 = count - 1;
			}
			SamplePoseData samplePoseData = _bufferedPoses[num];
			SamplePoseData samplePoseData2 = _bufferedPoses[num2];
			if (samplePoseData.Time > time && samplePoseData2.Time < time)
			{
				item = num2;
				item2 = num;
			}
			num--;
		}
		return (item, item2);
	}

	private (Vector3, Vector3) ComputeTrendVelocities()
	{
		Vector3 item = Vector3.zero;
		Vector3 item2 = Vector3.zero;
		if (_bufferedPoses.Count == 0)
		{
			return (Vector3.zero, Vector3.zero);
		}
		if (!BufferedVelocitiesValid())
		{
			(item, item2) = FindMostRecentBufferedSampleWithMovement();
		}
		else
		{
			FindLargestWindowWithMovement();
			int count = _windowWithMovement.Count;
			if (count == 0)
			{
				return (Vector3.zero, Vector3.zero);
			}
			foreach (SamplePoseData item3 in _windowWithMovement)
			{
				item += item3.LinearVelocity;
				item2 += item3.AngularVelocity;
			}
			item /= (float)count;
			item2 /= (float)count;
		}
		return (item, item2);
	}

	private bool BufferedVelocitiesValid()
	{
		int num = 0;
		foreach (SamplePoseData bufferedPose in _bufferedPoses)
		{
			if (bufferedPose.LinearVelocity.sqrMagnitude < Mathf.Epsilon)
			{
				num++;
			}
		}
		int count = _bufferedPoses.Count;
		if (!((float)num / (float)count > _maxPercentZeroSamplesTrendVeloc))
		{
			return true;
		}
		return false;
	}

	private void FindLargestWindowWithMovement()
	{
		int count = _bufferedPoses.Count;
		bool flag = false;
		_windowWithMovement.Clear();
		_tempWindow.Clear();
		Vector3 vector = Vector3.zero;
		int num = _lastWritePos;
		for (int i = 0; i < count; i++)
		{
			if (num < 0)
			{
				num = count - 1;
			}
			SamplePoseData item = _bufferedPoses[num];
			bool flag2 = item.LinearVelocity.sqrMagnitude > 0f;
			if (flag2)
			{
				if (!flag)
				{
					flag = true;
					_tempWindow.Clear();
					vector = item.LinearVelocity;
				}
				if (Vector3.Dot(vector.normalized, item.LinearVelocity.normalized) > 0.6f)
				{
					_tempWindow.Add(item);
				}
			}
			else if (!flag2 && flag)
			{
				flag = false;
				if (_tempWindow.Count > _windowWithMovement.Count)
				{
					TransferToDestBuffer(_tempWindow, _windowWithMovement);
				}
			}
			num--;
		}
		if (flag && _tempWindow.Count > _windowWithMovement.Count)
		{
			TransferToDestBuffer(_tempWindow, _windowWithMovement);
		}
	}

	private (Vector3, Vector3) FindMostRecentBufferedSampleWithMovement()
	{
		int count = _bufferedPoses.Count;
		Vector3 item = Vector3.zero;
		Vector3 item2 = Vector3.zero;
		int num = _lastWritePos;
		for (int i = 0; i < count; i++)
		{
			if (num < 0)
			{
				num = count - 1;
			}
			SamplePoseData samplePoseData = _bufferedPoses[num];
			Vector3 linearVelocity = samplePoseData.LinearVelocity;
			Vector3 angularVelocity = samplePoseData.AngularVelocity;
			if (linearVelocity.sqrMagnitude > Mathf.Epsilon && angularVelocity.sqrMagnitude > Mathf.Epsilon)
			{
				item = linearVelocity;
				item2 = angularVelocity;
				break;
			}
			num--;
		}
		return (item, item2);
	}

	private void TransferToDestBuffer(List<SamplePoseData> source, List<SamplePoseData> dest)
	{
		dest.Clear();
		foreach (SamplePoseData item in source)
		{
			dest.Add(item);
		}
	}

	private Vector3 CalculateTangentialVector(Vector3 objectPosition)
	{
		if (!_previousReferencePosition.HasValue)
		{
			return Vector3.zero;
		}
		float magnitude = _angularVelocity.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		Vector3 vector = objectPosition - _previousReferencePosition.Value;
		float magnitude2 = vector.magnitude;
		if (magnitude2 < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		Vector3 normalized = vector.normalized;
		Vector3 normalized2 = _angularVelocity.normalized;
		Vector3 vector2 = Vector3.Cross(normalized2, normalized);
		AxisOfRotation = normalized2;
		TangentialDirection = vector2;
		CenterOfMassToObject = normalized * magnitude2;
		AxisOfRotationOrigin = objectPosition;
		return vector2 * magnitude2 * magnitude;
	}

	public IReadOnlyList<ReleaseVelocityInformation> LastThrowVelocities()
	{
		return _currentThrowVelocities;
	}

	public void SetUpdateFrequency(float frequency)
	{
		if (frequency < Mathf.Epsilon)
		{
			Debug.LogError($"Provided frequency ${frequency} must be " + "greater than or equal to zero.");
			return;
		}
		_updateFrequency = frequency;
		_updateLatency = 1f / _updateFrequency;
	}

	protected virtual void LateUpdate()
	{
		float num = _timeProvider();
		if ((!(_updateLatency > 0f) || !(_lastUpdateTime > 0f) || !(num - _lastUpdateTime < _updateLatency)) && ThrowInputDevice.IsInputValid && ThrowInputDevice.IsHighConfidence && ThrowInputDevice.GetRootPose(out var pose))
		{
			float delta = num - _lastUpdateTime;
			_lastUpdateTime = num;
			pose = new Pose(_referenceOffset + pose.position, pose.rotation);
			CalculateLatestVelocitiesAndUpdateBuffer(delta, num, pose);
		}
	}

	private void CalculateLatestVelocitiesAndUpdateBuffer(float delta, float currentTime, Pose referencePose)
	{
		_accumulatedDelta += delta;
		UpdateLatestVelocitiesAndPoseValues(referencePose, _accumulatedDelta);
		_accumulatedDelta = 0f;
		int num = ((_lastWritePos >= 0) ? ((_lastWritePos + 1) % _bufferSize) : 0);
		SamplePoseData samplePoseData = new SamplePoseData(referencePose, _linearVelocity, _angularVelocity, currentTime);
		if (_bufferedPoses.Count <= num)
		{
			_bufferedPoses.Add(samplePoseData);
		}
		else
		{
			_bufferedPoses[num] = samplePoseData;
		}
		_lastWritePos = num;
	}

	private void UpdateLatestVelocitiesAndPoseValues(Pose referencePose, float delta)
	{
		(Vector3, Vector3) latestLinearAndAngularVelocities = GetLatestLinearAndAngularVelocities(referencePose, delta);
		_linearVelocity = latestLinearAndAngularVelocities.Item1;
		_angularVelocity = latestLinearAndAngularVelocities.Item2;
		_linearVelocity = _linearVelocityFilter.Step(_linearVelocity);
		ReleaseVelocityInformation obj = new ReleaseVelocityInformation(_linearVelocity, _angularVelocity, referencePose.position);
		this.WhenNewSampleAvailable(obj);
		_previousReferencePosition = referencePose.position;
		_previousReferenceRotation = referencePose.rotation;
	}

	private (Vector3, Vector3) GetLatestLinearAndAngularVelocities(Pose referencePose, float delta)
	{
		if (!_previousReferencePosition.HasValue || delta < Mathf.Epsilon)
		{
			return (Vector3.zero, Vector3.zero);
		}
		Vector3 item = (referencePose.position - _previousReferencePosition.Value) / delta;
		Vector3 item2 = VelocityCalculatorUtilMethods.ToAngularVelocity(_previousReferenceRotation.Value, referencePose.rotation, delta);
		return (item, item2);
	}

	public void InjectAllStandardVelocityCalculator(IPoseInputDevice poseInputDevice, BufferingParams bufferingParams)
	{
		InjectPoseInputDevice(poseInputDevice);
		InjectBufferingParams(bufferingParams);
	}

	public void InjectPoseInputDevice(IPoseInputDevice poseInputDevice)
	{
		_throwInputDevice = poseInputDevice as UnityEngine.Object;
		ThrowInputDevice = poseInputDevice;
	}

	public void InjectBufferingParams(BufferingParams bufferingParams)
	{
		_bufferingParams = bufferingParams;
	}

	[Obsolete("Use SetTimeProvider()")]
	public void InjectOptionalTimeProvider(Func<float> timeProvider)
	{
		_timeProvider = timeProvider;
	}
}
