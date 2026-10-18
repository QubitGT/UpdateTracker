using UnityEngine;

namespace Valve.VR;

public struct HmdMatrix34_t
{
	public float m0;

	public float m1;

	public float m2;

	public float m3;

	public float m4;

	public float m5;

	public float m6;

	public float m7;

	public float m8;

	public float m9;

	public float m10;

	public float m11;

	public Vector3 GetPosition()
	{
		return new Vector3(m3, m7, 0f - m11);
	}

	public bool IsRotationValid()
	{
		if (m2 != 0f || m6 != 0f || m10 != 0f)
		{
			if (m1 == 0f && m5 == 0f)
			{
				return m9 != 0f;
			}
			return true;
		}
		return false;
	}

	public Quaternion GetRotation()
	{
		if (IsRotationValid())
		{
			float w = Mathf.Sqrt(Mathf.Max(0f, 1f + m0 + m5 + m10)) / 2f;
			float sizeval = Mathf.Sqrt(Mathf.Max(0f, 1f + m0 - m5 - m10)) / 2f;
			float sizeval2 = Mathf.Sqrt(Mathf.Max(0f, 1f - m0 + m5 - m10)) / 2f;
			float sizeval3 = Mathf.Sqrt(Mathf.Max(0f, 1f - m0 - m5 + m10)) / 2f;
			_copysign(ref sizeval, 0f - m9 - (0f - m6));
			_copysign(ref sizeval2, 0f - m2 - (0f - m8));
			_copysign(ref sizeval3, m4 - m1);
			return new Quaternion(sizeval, sizeval2, sizeval3, w);
		}
		return Quaternion.identity;
	}

	private static void _copysign(ref float sizeval, float signval)
	{
		if (signval > 0f != sizeval > 0f)
		{
			sizeval = 0f - sizeval;
		}
	}
}
