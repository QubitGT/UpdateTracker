namespace g3;

public struct CurveSample2d
{
	public Vector2d position;

	public Vector2d tangent;

	public CurveSample2d(Vector2d p, Vector2d t)
	{
		position = p;
		tangent = t;
	}
}
