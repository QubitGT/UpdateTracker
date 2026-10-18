using UnityEngine.Rendering;

namespace Drawing;

internal static class MeshLayouts
{
	internal static readonly VertexAttributeDescriptor[] MeshLayout = new VertexAttributeDescriptor[4]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Normal),
		new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
	};

	internal static readonly VertexAttributeDescriptor[] MeshLayoutText = new VertexAttributeDescriptor[3]
	{
		new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
		new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
	};
}
