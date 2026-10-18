using System;
using UnityEngine;

namespace Unity.Cinemachine;

internal abstract class GaussianWindow1d<T>
{
	protected T[] m_Data;

	protected float[] m_Kernel;

	protected int m_CurrentPos = -1;

	public float Sigma { get; private set; }

	public int KernelSize => m_Kernel.Length;

	public int BufferLength => m_Data.Length;

	private void GenerateKernel(float sigma, int maxKernelRadius)
	{
		int num = Math.Min(maxKernelRadius, Mathf.FloorToInt(Mathf.Abs(sigma) * 2.5f));
		m_Kernel = new float[2 * num + 1];
		if (num == 0)
		{
			m_Kernel[0] = 1f;
		}
		else
		{
			float num2 = 0f;
			for (int i = -num; i <= num; i++)
			{
				m_Kernel[i + num] = (float)(Math.Exp((float)(-(i * i)) / (2f * sigma * sigma)) / (Math.PI * 2.0 * (double)sigma * (double)sigma));
				num2 += m_Kernel[i + num];
			}
			for (int j = -num; j <= num; j++)
			{
				m_Kernel[j + num] /= num2;
			}
		}
		Sigma = sigma;
	}

	protected abstract T Compute(int windowPos);

	public GaussianWindow1d(float sigma, int maxKernelRadius = 10)
	{
		GenerateKernel(sigma, maxKernelRadius);
		m_Data = new T[KernelSize];
		m_CurrentPos = -1;
	}

	public void Reset()
	{
		m_CurrentPos = -1;
	}

	public bool IsEmpty()
	{
		return m_CurrentPos < 0;
	}

	public void AddValue(T v)
	{
		if (m_CurrentPos < 0)
		{
			for (int i = 0; i < KernelSize; i++)
			{
				m_Data[i] = v;
			}
			m_CurrentPos = Mathf.Min(1, KernelSize - 1);
		}
		m_Data[m_CurrentPos] = v;
		if (++m_CurrentPos == KernelSize)
		{
			m_CurrentPos = 0;
		}
	}

	public T Filter(T v)
	{
		if (KernelSize < 3)
		{
			return v;
		}
		AddValue(v);
		return Value();
	}

	public T Value()
	{
		return Compute(m_CurrentPos);
	}

	public void SetBufferValue(int index, T value)
	{
		m_Data[index] = value;
	}

	public T GetBufferValue(int index)
	{
		return m_Data[index];
	}
}
