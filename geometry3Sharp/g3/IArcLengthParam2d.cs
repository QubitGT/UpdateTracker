namespace g3;

public interface IArcLengthParam2d
{
	double ArcLength { get; }

	CurveSample2d Sample(double fArcLen);
}
