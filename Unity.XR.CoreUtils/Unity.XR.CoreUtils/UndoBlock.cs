using System;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public class UndoBlock : IDisposable
{
	private int m_UndoGroup;

	private bool m_DisposedValue;

	public UndoBlock(string undoLabel, bool testMode = false)
	{
		m_UndoGroup = -1;
	}

	public void RegisterCreatedObject(UnityEngine.Object objectToUndo)
	{
	}

	public void RecordObject(UnityEngine.Object objectToUndo)
	{
	}

	public void SetTransformParent(Transform transform, Transform newParent)
	{
		transform.parent = newParent;
	}

	public T AddComponent<T>(GameObject gameObject) where T : Component
	{
		return gameObject.AddComponent<T>();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_DisposedValue)
		{
			if (disposing)
			{
				_ = m_UndoGroup;
				_ = -1;
			}
			m_DisposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
