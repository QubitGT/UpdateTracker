namespace g3;

public class ImplicitBlend2d : ImplicitNAryOp2d
{
	public override float Value(float fX, float fY)
	{
		float num = 0f;
		foreach (ImplicitField2d vChild in m_vChildren)
		{
			num += vChild.Value(fX, fY);
		}
		return num;
	}

	public override void Gradient(float fX, float fY, ref float fGX, ref float fGY)
	{
		fGX = (fGY = 0f);
		float fGX2 = 0f;
		float fGY2 = 0f;
		foreach (ImplicitField2d vChild in m_vChildren)
		{
			vChild.Gradient(fX, fY, ref fGX2, ref fGY2);
			fGX += fGX2;
			fGY += fGY2;
		}
	}
}
