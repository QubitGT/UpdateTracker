namespace UnityEngine.ProBuilder;

internal sealed class MeshHandle
{
	private Transform m_Transform;

	private Mesh m_Mesh;

	public Mesh mesh => m_Mesh;

	public MeshHandle(Transform transform, Mesh mesh)
	{
		m_Transform = transform;
		m_Mesh = mesh;
	}

	public void DrawMeshNow(int submeshIndex)
	{
		if (!(m_Transform == null) && !(m_Mesh == null))
		{
			Graphics.DrawMeshNow(m_Mesh, m_Transform.localToWorldMatrix, submeshIndex);
		}
	}
}
