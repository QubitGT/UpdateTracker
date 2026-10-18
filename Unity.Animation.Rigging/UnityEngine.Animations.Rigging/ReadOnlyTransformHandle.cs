using System;

namespace UnityEngine.Animations.Rigging;

public struct ReadOnlyTransformHandle
{
	private TransformStreamHandle m_StreamHandle;

	private TransformSceneHandle m_SceneHandle;

	private byte m_InStream;

	public Vector3 GetLocalPosition(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetLocalPosition(stream);
		}
		return m_StreamHandle.GetLocalPosition(stream);
	}

	public Quaternion GetLocalRotation(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetLocalRotation(stream);
		}
		return m_StreamHandle.GetLocalRotation(stream);
	}

	public Vector3 GetLocalScale(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetLocalScale(stream);
		}
		return m_StreamHandle.GetLocalScale(stream);
	}

	public void GetLocalTRS(AnimationStream stream, out Vector3 position, out Quaternion rotation, out Vector3 scale)
	{
		if (m_InStream == 1)
		{
			m_StreamHandle.GetLocalTRS(stream, out position, out rotation, out scale);
		}
		else
		{
			m_SceneHandle.GetLocalTRS(stream, out position, out rotation, out scale);
		}
	}

	public Matrix4x4 GetLocalToParentMatrix(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetLocalToParentMatrix(stream);
		}
		return m_StreamHandle.GetLocalToParentMatrix(stream);
	}

	public Vector3 GetPosition(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetPosition(stream);
		}
		return m_StreamHandle.GetPosition(stream);
	}

	public Quaternion GetRotation(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetRotation(stream);
		}
		return m_StreamHandle.GetRotation(stream);
	}

	public void GetGlobalTR(AnimationStream stream, out Vector3 position, out Quaternion rotation)
	{
		if (m_InStream == 1)
		{
			m_StreamHandle.GetGlobalTR(stream, out position, out rotation);
		}
		else
		{
			m_SceneHandle.GetGlobalTR(stream, out position, out rotation);
		}
	}

	public Matrix4x4 GetLocalToWorldMatrix(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.GetLocalToWorldMatrix(stream);
		}
		return m_StreamHandle.GetLocalToWorldMatrix(stream);
	}

	public bool IsResolved(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return true;
		}
		return m_StreamHandle.IsResolved(stream);
	}

	public bool IsValid(AnimationStream stream)
	{
		if (m_InStream != 1)
		{
			return m_SceneHandle.IsValid(stream);
		}
		return m_StreamHandle.IsValid(stream);
	}

	public void Resolve(AnimationStream stream)
	{
		if (m_InStream == 1)
		{
			m_StreamHandle.Resolve(stream);
		}
	}

	public static ReadOnlyTransformHandle Bind(Animator animator, Transform transform)
	{
		ReadOnlyTransformHandle result = default(ReadOnlyTransformHandle);
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		result.m_InStream = (byte)(transform.IsChildOf(animator.avatarRoot) ? 1u : 0u);
		if (result.m_InStream == 1)
		{
			result.m_StreamHandle = animator.BindStreamTransform(transform);
		}
		else
		{
			result.m_SceneHandle = animator.BindSceneTransform(transform);
		}
		return result;
	}
}
