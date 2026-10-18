using Unity.Mathematics;

namespace UnityEngine.Splines;

public abstract class SplineComponent : MonoBehaviour
{
	public enum AlignAxis
	{
		[InspectorName("Object X+")]
		XAxis,
		[InspectorName("Object Y+")]
		YAxis,
		[InspectorName("Object Z+")]
		ZAxis,
		[InspectorName("Object X-")]
		NegativeXAxis,
		[InspectorName("Object Y-")]
		NegativeYAxis,
		[InspectorName("Object Z-")]
		NegativeZAxis
	}

	private readonly float3[] m_AlignAxisToVector = new float3[6]
	{
		math.right(),
		math.up(),
		math.forward(),
		math.left(),
		math.down(),
		math.back()
	};

	protected float3 GetAxis(AlignAxis axis)
	{
		return m_AlignAxisToVector[(int)axis];
	}
}
