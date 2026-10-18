using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Voxels;

public struct MeshVertexData
{
	public float3 position;

	public float3 normal;

	public float4 tangent;

	public float4 materials;

	public float4 blend;

	public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout = new VertexAttributeDescriptor[5]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Normal),
		new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4)
	};

	public MeshVertexData(float3 position, float3 normal, float4 tangent, float4 materials, float4 blend)
	{
		this.position = position;
		this.normal = normal;
		this.tangent = tangent;
		this.materials = materials;
		this.blend = blend;
	}

	public override string ToString()
	{
		return $"({position:F2} x {normal:F2})";
	}
}
