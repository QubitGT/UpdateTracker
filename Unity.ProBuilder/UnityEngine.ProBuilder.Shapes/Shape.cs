using System;

namespace UnityEngine.ProBuilder.Shapes;

[Serializable]
public abstract class Shape
{
	public virtual Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		return mesh.mesh.bounds;
	}

	public abstract Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation);

	public abstract void CopyShape(Shape shape);

	internal abstract void SetParametersToBuiltInShape();
}
