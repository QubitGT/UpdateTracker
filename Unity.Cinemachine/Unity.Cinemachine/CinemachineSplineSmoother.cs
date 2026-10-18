using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[ExecuteAlways]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Spline Smoother")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineSmoother.html")]
[RequireComponent(typeof(SplineContainer))]
public class CinemachineSplineSmoother : MonoBehaviour
{
	[Tooltip("If checked, the spline will be automatically smoothed whenever it is modified (editor only).")]
	public bool AutoSmooth = true;

	public void SmoothSplineNow()
	{
		if (!TryGetComponent<SplineContainer>(out var component) || component.Spline == null)
		{
			return;
		}
		Spline spline = component.Spline;
		int count = spline.Count;
		if (count >= 3)
		{
			float3[] ctrl = new float3[count];
			float3[] ctrl2 = new float3[count];
			float3[] knot = new float3[count];
			for (int i = 0; i < count; i++)
			{
				knot[i] = spline[i].Position;
			}
			if (spline.Closed)
			{
				SplineHelpers.ComputeSmoothControlPointsLooped(ref knot, ref ctrl, ref ctrl2);
			}
			else
			{
				SplineHelpers.ComputeSmoothControlPoints(ref knot, ref ctrl, ref ctrl2);
			}
			for (int j = 0; j < count; j++)
			{
				spline.SetTangentMode(j, TangentMode.Mirrored);
				BezierKnot value = spline[j];
				float3 up = math.mul(value.Rotation, new float3(0f, 1f, 0f));
				float3 @float = ctrl[j] - knot[j];
				float num = (math.length(ctrl2[(j > 0) ? (j - 1) : (count - 1)] - knot[j]) + math.length(@float)) * 0.5f;
				value.Rotation = quaternion.LookRotationSafe(@float, up);
				value.TangentIn = ((j == 0 && !spline.Closed) ? default(float3) : new float3(0f, 0f, 0f - num));
				value.TangentOut = ((j == count - 1 && !spline.Closed) ? default(float3) : new float3(0f, 0f, num));
				spline[j] = value;
			}
		}
	}
}
