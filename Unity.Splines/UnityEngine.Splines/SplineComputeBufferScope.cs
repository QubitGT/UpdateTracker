using System;
using Unity.Collections;

namespace UnityEngine.Splines;

public struct SplineComputeBufferScope<T> : IDisposable where T : ISpline
{
	private T m_Spline;

	private int m_KnotCount;

	private ComputeBuffer m_CurveBuffer;

	private ComputeBuffer m_LengthBuffer;

	private ComputeShader m_Shader;

	private string m_Info;

	private string m_Curves;

	private string m_CurveLengths;

	private int m_Kernel;

	public Vector4 Info => new Vector4(m_Spline.Count, m_Spline.Closed ? 1 : 0, m_Spline.GetLength(), 0f);

	public ComputeBuffer Curves => m_CurveBuffer;

	public ComputeBuffer CurveLengths => m_LengthBuffer;

	public SplineComputeBufferScope(T spline)
	{
		m_Spline = spline;
		m_KnotCount = 0;
		m_CurveBuffer = (m_LengthBuffer = null);
		m_Shader = null;
		m_Info = (m_Curves = (m_CurveLengths = null));
		m_Kernel = 0;
		Upload();
	}

	public void Bind(ComputeShader shader, int kernel, string info, string curves, string lengths)
	{
		if (shader == null)
		{
			throw new ArgumentNullException("shader");
		}
		if (string.IsNullOrEmpty(info))
		{
			throw new ArgumentNullException("info");
		}
		if (string.IsNullOrEmpty(curves))
		{
			throw new ArgumentNullException("curves");
		}
		if (string.IsNullOrEmpty(lengths))
		{
			throw new ArgumentNullException("lengths");
		}
		m_Shader = shader;
		m_Info = info;
		m_Curves = curves;
		m_CurveLengths = lengths;
		m_Kernel = kernel;
		m_Shader.SetVector(m_Info, Info);
		m_Shader.SetBuffer(m_Kernel, m_Curves, Curves);
		m_Shader.SetBuffer(m_Kernel, m_CurveLengths, CurveLengths);
	}

	public void Dispose()
	{
		m_CurveBuffer?.Dispose();
		m_LengthBuffer?.Dispose();
	}

	public void Upload()
	{
		int count = m_Spline.Count;
		if (m_KnotCount != count)
		{
			m_KnotCount = m_Spline.Count;
			m_CurveBuffer?.Dispose();
			m_LengthBuffer?.Dispose();
			m_CurveBuffer = new ComputeBuffer(m_KnotCount, 48);
			m_LengthBuffer = new ComputeBuffer(m_KnotCount, 4);
		}
		NativeArray<BezierCurve> data = new NativeArray<BezierCurve>(m_KnotCount, Allocator.Temp);
		NativeArray<float> data2 = new NativeArray<float>(m_KnotCount, Allocator.Temp);
		for (int i = 0; i < m_KnotCount; i++)
		{
			data[i] = m_Spline.GetCurve(i);
			data2[i] = m_Spline.GetCurveLength(i);
		}
		if (!string.IsNullOrEmpty(m_Info))
		{
			m_Shader.SetVector(m_Info, Info);
		}
		m_CurveBuffer.SetData(data);
		m_LengthBuffer.SetData(data2);
		data.Dispose();
		data2.Dispose();
	}
}
