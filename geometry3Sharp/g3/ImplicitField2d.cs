namespace g3;

public interface ImplicitField2d
{
	AxisAlignedBox2f Bounds { get; }

	float Value(float fX, float fY);

	void Gradient(float fX, float fY, ref float fGX, ref float fGY);
}
