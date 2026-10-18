namespace g3;

public interface IParametricCurve2d
{
	bool IsClosed { get; }

	double ParamLength { get; }

	bool HasArcLength { get; }

	double ArcLength { get; }

	bool IsTransformable { get; }

	Vector2d SampleT(double t);

	Vector2d TangentT(double t);

	Vector2d SampleArcLength(double a);

	void Reverse();

	void Transform(ITransform2 xform);

	IParametricCurve2d Clone();
}
