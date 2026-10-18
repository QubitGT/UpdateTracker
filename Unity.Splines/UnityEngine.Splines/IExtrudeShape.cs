using Unity.Mathematics;

namespace UnityEngine.Splines;

public interface IExtrudeShape
{
	int SideCount { get; }

	void Setup(ISpline path, int segmentCount)
	{
	}

	void SetSegment(int index, float t, float3 position, float3 tangent, float3 up)
	{
	}

	float2 GetPosition(float t, int index);
}
