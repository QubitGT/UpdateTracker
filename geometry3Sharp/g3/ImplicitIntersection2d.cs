using System;

namespace g3;

public class ImplicitIntersection2d : ImplicitNAryOp2d
{
	public override float Value(float fX, float fY)
	{
		float num = 1E+10f;
		foreach (ImplicitField2d vChild in m_vChildren)
		{
			num = Math.Min(num, vChild.Value(fX, fY));
		}
		return num;
	}

	public override void Gradient(float fX, float fY, ref float fGX, ref float fGY)
	{
		float num = 1E+10f;
		int num2 = -1;
		for (int i = 0; i < m_vChildren.Count; i++)
		{
			float num3 = m_vChildren[i].Value(fX, fY);
			if (num3 < num)
			{
				num2 = i;
			}
			num = num3;
		}
		if (num2 >= 0)
		{
			m_vChildren[num2].Gradient(fX, fY, ref fGX, ref fGY);
		}
		else
		{
			fGX = (fGY = 0f);
		}
	}
}
