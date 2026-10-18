using Unity.Mathematics;
using UnityEngine.Splines.Interpolators;

namespace UnityEngine.Splines;

public static class InterpolatorUtility
{
	private static readonly IInterpolator<float> s_LerpFloat = default(LerpFloat);

	private static readonly IInterpolator<float2> s_LerpFloat2 = default(LerpFloat2);

	private static readonly IInterpolator<float3> s_LerpFloat3 = default(LerpFloat3);

	private static readonly IInterpolator<float4> s_LerpFloat4 = default(LerpFloat4);

	private static readonly IInterpolator<float2> s_SlerpFloat2 = default(SlerpFloat2);

	private static readonly IInterpolator<float3> s_SlerpFloat3 = default(SlerpFloat3);

	private static readonly IInterpolator<quaternion> s_LerpQuaternion = default(LerpQuaternion);

	private static readonly IInterpolator<Color> s_LerpColor = default(LerpColor);

	private static readonly IInterpolator<float> s_SmoothStepFloat = default(SmoothStepFloat);

	private static readonly IInterpolator<float2> s_SmoothStepFloat2 = default(SmoothStepFloat2);

	private static readonly IInterpolator<float3> s_SmoothStepFloat3 = default(SmoothStepFloat3);

	private static readonly IInterpolator<float4> s_SmoothStepFloat4 = default(SmoothStepFloat4);

	private static readonly IInterpolator<quaternion> s_SlerpQuaternion = default(SlerpQuaternion);

	public static IInterpolator<float> LerpFloat => s_LerpFloat;

	public static IInterpolator<float2> LerpFloat2 => s_LerpFloat2;

	public static IInterpolator<float3> LerpFloat3 => s_LerpFloat3;

	public static IInterpolator<float4> LerpFloat4 => s_LerpFloat4;

	public static IInterpolator<float2> SlerpFloat2 => s_SlerpFloat2;

	public static IInterpolator<float3> SlerpFloat3 => s_SlerpFloat3;

	public static IInterpolator<quaternion> LerpQuaternion => s_LerpQuaternion;

	public static IInterpolator<Color> LerpColor => s_LerpColor;

	public static IInterpolator<float> SmoothStepFloat => s_SmoothStepFloat;

	public static IInterpolator<float2> SmoothStepFloat2 => s_SmoothStepFloat2;

	public static IInterpolator<float3> SmoothStepFloat3 => s_SmoothStepFloat3;

	public static IInterpolator<float4> SmoothStepFloat4 => s_SmoothStepFloat4;

	public static IInterpolator<quaternion> SlerpQuaternion => s_SlerpQuaternion;
}
