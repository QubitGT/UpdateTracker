namespace g3;

public struct CurveSample
{
	public Vector3d position;

	public Vector3d tangent;

	public CurveSample(Vector3d p, Vector3d t)
	{
		position = p;
		tangent = t;
	}
}
