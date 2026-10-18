namespace g3;

public interface ITransform2
{
	Vector2d TransformP(Vector2d p);

	Vector2d TransformN(Vector2d n);

	double TransformScalar(double s);
}
