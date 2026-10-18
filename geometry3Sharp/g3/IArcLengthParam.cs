namespace g3;

public interface IArcLengthParam
{
	double ArcLength { get; }

	CurveSample Sample(double fArcLen);
}
