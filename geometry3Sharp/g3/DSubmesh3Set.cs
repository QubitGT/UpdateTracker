using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class DSubmesh3Set : IEnumerable<DSubmesh3>, IEnumerable
{
	public DMesh3 Mesh;

	public IEnumerable<object> TriangleSetKeys;

	public Func<object, IEnumerable<int>> TriangleSetF;

	public List<DSubmesh3> Submeshes;

	public Dictionary<object, DSubmesh3> KeyToMesh;

	public DSubmesh3Set(DMesh3 mesh, IEnumerable<object> keys, Func<object, IEnumerable<int>> indexSetsF)
	{
		Mesh = mesh;
		TriangleSetKeys = keys;
		TriangleSetF = indexSetsF;
		ComputeSubMeshes();
	}

	public DSubmesh3Set(DMesh3 mesh, MeshConnectedComponents components)
	{
		Mesh = mesh;
		TriangleSetF = (object idx) => components.Components[(int)idx].Indices;
		List<object> list = new List<object>();
		for (int i = 0; i < components.Count; i++)
		{
			list.Add(i);
		}
		TriangleSetKeys = list;
		ComputeSubMeshes();
	}

	public IEnumerator<DSubmesh3> GetEnumerator()
	{
		return Submeshes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Submeshes.GetEnumerator();
	}

	protected virtual void ComputeSubMeshes()
	{
		Submeshes = new List<DSubmesh3>();
		KeyToMesh = new Dictionary<object, DSubmesh3>();
		SpinLock data_lock = default(SpinLock);
		gParallel.ForEach(TriangleSetKeys, delegate(object obj)
		{
			DSubmesh3 dSubmesh = new DSubmesh3(Mesh, TriangleSetF(obj));
			bool lockTaken = false;
			data_lock.Enter(ref lockTaken);
			Submeshes.Add(dSubmesh);
			KeyToMesh[obj] = dSubmesh;
			data_lock.Exit();
		});
	}
}
