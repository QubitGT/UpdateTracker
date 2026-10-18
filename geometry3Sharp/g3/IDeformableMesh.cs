namespace g3;

public interface IDeformableMesh : IMesh, IPointSet
{
	void SetVertex(int vID, Vector3d vNewPos);

	void SetVertexNormal(int vid, Vector3f vNewNormal);

	void SetVertexUV(int i, Vector2f uv);
}
