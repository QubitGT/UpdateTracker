using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal struct PerLight2D
{
	internal float4x4 InvMatrix;

	internal float4 Color;

	internal float4 Position;

	internal float FalloffIntensity;

	internal float FalloffDistance;

	internal float OuterAngle;

	internal float InnerAngle;

	internal float InnerRadiusMult;

	internal float VolumeOpacity;

	internal float ShadowIntensity;

	internal int LightType;
}
