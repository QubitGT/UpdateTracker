namespace g3;

public class SculptCurveSmooth : StandardSculptCurveDeformation
{
	public SculptCurveSmooth()
	{
		DeformF = null;
		SmoothAlpha = 0.10000000149011612;
		SmoothIterations = 3;
	}
}
