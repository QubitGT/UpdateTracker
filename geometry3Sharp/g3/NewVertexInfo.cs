namespace g3;

public struct NewVertexInfo
{
	public Vector3d v;

	public Vector3f n;

	public Vector3f c;

	public Vector2f uv;

	public bool bHaveN;

	public bool bHaveUV;

	public bool bHaveC;

	public NewVertexInfo(Vector3d v)
	{
		this.v = v;
		n = (c = Vector3f.Zero);
		uv = Vector2f.Zero;
		bHaveN = (bHaveC = (bHaveUV = false));
	}

	public NewVertexInfo(Vector3d v, Vector3f n)
	{
		this.v = v;
		this.n = n;
		c = Vector3f.Zero;
		uv = Vector2f.Zero;
		bHaveN = true;
		bHaveC = (bHaveUV = false);
	}

	public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c)
	{
		this.v = v;
		this.n = n;
		this.c = c;
		uv = Vector2f.Zero;
		bHaveN = (bHaveC = true);
		bHaveUV = false;
	}

	public NewVertexInfo(Vector3d v, Vector3f n, Vector3f c, Vector2f uv)
	{
		this.v = v;
		this.n = n;
		this.c = c;
		this.uv = uv;
		bHaveN = (bHaveC = (bHaveUV = true));
	}
}
