using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.Experimental.Rendering;

internal class XRLayoutStack : IDisposable
{
	private readonly Stack<XRLayout> m_Stack = new Stack<XRLayout>();

	public XRLayout top => m_Stack.Peek();

	public XRLayout New()
	{
		GenericPool<XRLayout>.Get(out var value);
		m_Stack.Push(value);
		return value;
	}

	public void Release()
	{
		if (!m_Stack.TryPop(out var result))
		{
			throw new InvalidOperationException("Calling Release without calling New first.");
		}
		result.Clear();
		GenericPool<XRLayout>.Release(result);
	}

	public void Dispose()
	{
		if (m_Stack.Count != 0)
		{
			throw new Exception("Stack is not empty. Did you skip a call to Release?");
		}
	}
}
