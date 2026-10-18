namespace g3;

public class DenseUVMesh
{
	public DVector<Vector2f> UVs;

	public DVector<Index3i> TriangleUVs;

	public DenseUVMesh()
	{
		UVs = new DVector<Vector2f>();
		TriangleUVs = new DVector<Index3i>();
	}

	public int AppendUV(Vector2f uv)
	{
		int length = UVs.Length;
		UVs.Add(uv);
		return length;
	}
}
