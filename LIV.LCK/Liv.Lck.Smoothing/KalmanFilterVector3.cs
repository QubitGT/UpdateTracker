using UnityEngine;

namespace Liv.Lck.Smoothing;

public class KalmanFilterVector3
{
	private KalmanFilter _filterX;

	private KalmanFilter _filterY;

	private KalmanFilter _filterZ;

	public KalmanFilterVector3(Vector3 initialEstimate = default(Vector3), float initialCovariance = 1f)
	{
		_filterX = new KalmanFilter(initialEstimate.x, initialCovariance);
		_filterY = new KalmanFilter(initialEstimate.y, initialCovariance);
		_filterZ = new KalmanFilter(initialEstimate.z, initialCovariance);
	}

	public Vector3 Update(Vector3 measurement, float deltaTime, float smoothing)
	{
		return new Vector3(_filterX.Update(measurement.x, deltaTime, smoothing), _filterY.Update(measurement.y, deltaTime, smoothing), _filterZ.Update(measurement.z, deltaTime, smoothing));
	}
}
