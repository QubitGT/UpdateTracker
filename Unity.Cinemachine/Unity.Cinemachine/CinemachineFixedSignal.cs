using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulseFixedSignals.html")]
public class CinemachineFixedSignal : SignalSourceAsset
{
	[Tooltip("The raw signal shape along the X axis")]
	[FormerlySerializedAs("m_XCurve")]
	public AnimationCurve XCurve;

	[Tooltip("The raw signal shape along the Y axis")]
	[FormerlySerializedAs("m_YCurve")]
	public AnimationCurve YCurve;

	[Tooltip("The raw signal shape along the Z axis")]
	[FormerlySerializedAs("m_ZCurve")]
	public AnimationCurve ZCurve;

	public override float SignalDuration => Mathf.Max(AxisDuration(XCurve), Mathf.Max(AxisDuration(YCurve), AxisDuration(ZCurve)));

	private float AxisDuration(AnimationCurve axis)
	{
		float result = 0f;
		if (axis != null && axis.length > 1)
		{
			float time = axis[0].time;
			result = axis[axis.length - 1].time - time;
		}
		return result;
	}

	public override void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot)
	{
		rot = Quaternion.identity;
		pos = new Vector3(AxisValue(XCurve, timeSinceSignalStart), AxisValue(YCurve, timeSinceSignalStart), AxisValue(ZCurve, timeSinceSignalStart));
	}

	private float AxisValue(AnimationCurve axis, float time)
	{
		if (axis == null || axis.length == 0)
		{
			return 0f;
		}
		return axis.Evaluate(time);
	}
}
