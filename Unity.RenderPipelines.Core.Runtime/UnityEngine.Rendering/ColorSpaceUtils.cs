using Unity.Mathematics;

namespace UnityEngine.Rendering;

public static class ColorSpaceUtils
{
	public static readonly float3x3 Rec709ToRec2020Mat = new float3x3(0.627402f, 0.329292f, 0.043306f, 0.069095f, 0.919544f, 0.01136f, 0.016394f, 0.088028f, 0.895578f);

	public static readonly float3x3 Rec709ToP3D65Mat = math.float3x3(0.822462f, 0.177538f, 0f, 0.033194f, 0.966806f, 0f, 0.017083f, 0.072397f, 0.91052f);

	public static readonly float3x3 Rec2020ToRec709Mat = math.float3x3(1.660496f, -0.587656f, -0.07284f, -0.124547f, 1.132895f, -0.008348f, -0.018154f, -0.100597f, 1.118751f);

	public static readonly float3x3 Rec2020ToP3D65Mat = math.float3x3(1.343578f, -0.28218f, -0.0613986f, -0.065298f, 1.075788f, -0.010491f, 0.002822f, -0.019599f, 1.016777f);

	public static readonly float3x3 P3D65ToRec2020Mat = math.float3x3(0.753833f, 0.198597f, 0.04757f, 0.045744f, 0.941777f, 0.012479f, -0.00121f, 0.017602f, 0.983609f);
}
