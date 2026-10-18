using UnityEngine;

namespace Liv.Lck.Smoothing;

public class KalmanFilterQuaternion
{
	private float _estimationErrorCovariance;

	private float _kalmanGain;

	private Quaternion _filteredValue;

	public KalmanFilterQuaternion(Quaternion initialEstimate, float initialCovariance = 1f)
	{
		_filteredValue = initialEstimate;
		_estimationErrorCovariance = initialCovariance;
	}

	public Quaternion Update(Quaternion measurement, float deltaTime, float smoothing)
	{
		float num = Mathf.Lerp(10f, 0f, smoothing);
		float num2 = Mathf.Lerp(0f, 10f, smoothing);
		float num3 = Mathf.Max(deltaTime, 0.0001f);
		float num4 = num * num3;
		_estimationErrorCovariance += num4;
		_kalmanGain = _estimationErrorCovariance / (_estimationErrorCovariance + num2);
		_filteredValue = Quaternion.Slerp(_filteredValue, measurement, _kalmanGain);
		_estimationErrorCovariance = (1f - _kalmanGain) * _estimationErrorCovariance;
		return _filteredValue;
	}
}
