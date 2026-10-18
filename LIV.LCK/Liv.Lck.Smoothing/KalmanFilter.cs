using UnityEngine;

namespace Liv.Lck.Smoothing;

public class KalmanFilter
{
	private float _estimationErrorCovariance;

	private float _filteredValue;

	private float _kalmanGain;

	public KalmanFilter(float initialEstimate = 0f, float initialCovariance = 1f)
	{
		_filteredValue = initialEstimate;
		_estimationErrorCovariance = initialCovariance;
	}

	public float Update(float measurement, float deltaTime, float smoothing)
	{
		float num = Mathf.Lerp(10f, 0f, smoothing);
		float num2 = Mathf.Lerp(0f, 10f, smoothing);
		float num3 = Mathf.Max(deltaTime, 0.0001f);
		float num4 = num * num3;
		_estimationErrorCovariance += num4;
		_kalmanGain = _estimationErrorCovariance / (_estimationErrorCovariance + num2);
		_filteredValue += _kalmanGain * (measurement - _filteredValue);
		_estimationErrorCovariance = (1f - _kalmanGain) * _estimationErrorCovariance;
		return _filteredValue;
	}
}
