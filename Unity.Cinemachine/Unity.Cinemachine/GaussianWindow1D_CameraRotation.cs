using UnityEngine;

namespace Unity.Cinemachine;

internal class GaussianWindow1D_CameraRotation : GaussianWindow1d<Vector2>
{
	public GaussianWindow1D_CameraRotation(float sigma, int maxKernelRadius = 10)
		: base(sigma, maxKernelRadius)
	{
	}

	protected override Vector2 Compute(int windowPos)
	{
		Vector2 zero = Vector2.zero;
		Vector2 vector = m_Data[m_CurrentPos];
		for (int i = 0; i < base.KernelSize; i++)
		{
			Vector2 vector2 = m_Data[windowPos] - vector;
			if (vector2.y > 180f)
			{
				vector2.y -= 360f;
			}
			if (vector2.y < -180f)
			{
				vector2.y += 360f;
			}
			zero += vector2 * m_Kernel[i];
			if (++windowPos == base.KernelSize)
			{
				windowPos = 0;
			}
		}
		return vector + zero;
	}
}
