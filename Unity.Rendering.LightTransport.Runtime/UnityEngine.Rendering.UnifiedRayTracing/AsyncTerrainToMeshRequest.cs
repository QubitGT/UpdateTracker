using System.Collections.Generic;
using Unity.Jobs;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct AsyncTerrainToMeshRequest
{
	private JobHandle m_JobHandle;

	private ComputeTerrainMeshJob m_Job;

	public bool done => m_JobHandle.IsCompleted;

	internal AsyncTerrainToMeshRequest(ComputeTerrainMeshJob job, JobHandle jobHandle)
	{
		m_Job = job;
		m_JobHandle = jobHandle;
	}

	public Mesh GetMesh()
	{
		if (!done)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		mesh.indexFormat = IndexFormat.UInt32;
		mesh.SetVertices(m_Job.positions);
		mesh.SetUVs(0, m_Job.uvs);
		mesh.SetNormals(m_Job.normals);
		mesh.SetIndices(TriangleIndicesWithoutHoles().ToArray(), MeshTopology.Triangles, 0);
		m_Job.DisposeArrays();
		return mesh;
	}

	public void WaitForCompletion()
	{
		m_JobHandle.Complete();
	}

	private List<int> TriangleIndicesWithoutHoles()
	{
		List<int> list = new List<int>((m_Job.width - 1) * (m_Job.height - 1) * 6);
		for (int i = 0; i < m_Job.indices.Length; i += 3)
		{
			int num = m_Job.indices[i];
			int num2 = m_Job.indices[i + 1];
			int num3 = m_Job.indices[i + 2];
			if (num != 0 && num2 != 0 && num3 != 0)
			{
				list.Add(num);
				list.Add(num2);
				list.Add(num3);
			}
		}
		if (list.Count == 0)
		{
			list.Add(0);
			list.Add(0);
			list.Add(0);
		}
		return list;
	}
}
