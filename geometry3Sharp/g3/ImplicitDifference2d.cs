namespace g3;

public class ImplicitDifference2d : ImplicitNAryOp2d
{
	public override float Value(float fX, float fY)
	{
		if (m_vChildren.Count <= 0)
		{
			return 0f;
		}
		float num = m_vChildren[0].Value(fX, fY);
		for (int i = 1; i < m_vChildren.Count; i++)
		{
			float num2 = 1f - m_vChildren[i].Value(fX, fY);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public override void Gradient(float fX, float fY, ref float fGX, ref float fGY)
	{
		if (m_vChildren.Count <= 0)
		{
			fGX = (fGY = 0f);
			return;
		}
		int num = 0;
		float num2 = m_vChildren[0].Value(fX, fY);
		for (int i = 1; i < m_vChildren.Count; i++)
		{
			float num3 = 1f - m_vChildren[i].Value(fX, fY);
			if (num3 < num2)
			{
				num = i;
				num2 = num3;
			}
		}
		m_vChildren[num].Gradient(fX, fY, ref fGX, ref fGY);
		if (num > 0)
		{
			fGX = 0f - fGX;
			fGY = 0f - fGY;
		}
	}
}
