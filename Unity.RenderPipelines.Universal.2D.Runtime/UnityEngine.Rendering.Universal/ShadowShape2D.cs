using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

public abstract class ShadowShape2D
{
	public enum OutlineTopology
	{
		Lines,
		Triangles
	}

	public enum WindingOrder
	{
		Clockwise,
		CounterClockwise
	}

	public abstract void SetFlip(bool flipX, bool flipY);

	public abstract void GetFlip(out bool flipX, out bool flipY);

	public abstract void SetDefaultTrim(float trim);

	public abstract void SetShape(NativeArray<Vector3> vertices, NativeArray<int> indices, NativeArray<float> radii, Matrix4x4 transform, WindingOrder windingOrder = WindingOrder.Clockwise, bool allowContraction = true, bool createInteriorGeometry = false);

	public abstract void SetShape(NativeArray<Vector3> vertices, NativeArray<int> indices, OutlineTopology outlineTopology, WindingOrder windingOrder = WindingOrder.Clockwise, bool allowContraction = true, bool createInteriorGeometry = false);
}
