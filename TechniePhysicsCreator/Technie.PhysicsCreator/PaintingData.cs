using System.Collections.Generic;
using Technie.PhysicsCreator.Rigid;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class PaintingData : ScriptableObject, IEditorData
{
	public HullData hullData;

	public Mesh sourceMesh;

	public Hash160 sourceMeshHash;

	public int activeHull = -1;

	public float faceThickness = 0.1f;

	public List<Hull> hulls = new List<Hull>();

	public AutoHullPreset autoHullPreset = AutoHullPreset.Medium;

	public VhacdParameters vhacdParams = new VhacdParameters();

	public bool hasLastVhacdTimings;

	public AutoHullPreset lastVhacdPreset = AutoHullPreset.Medium;

	public float lastVhacdDurationSecs;

	public bool suppressMeshModificationWarning;

	public int TotalOutputColliders
	{
		get
		{
			int num = 0;
			foreach (Hull hull in hulls)
			{
				num = ((hull.type != HullType.Auto) ? (num + 1) : (num + ((hull.autoMeshes != null) ? hull.autoMeshes.Length : 0)));
			}
			return num;
		}
	}

	public Hash160 CachedHash
	{
		get
		{
			return sourceMeshHash;
		}
		set
		{
			sourceMeshHash = value;
		}
	}

	public bool HasCachedData
	{
		get
		{
			if (sourceMeshHash != null)
			{
				return sourceMeshHash.IsValid();
			}
			return false;
		}
	}

	public Mesh SourceMesh => sourceMesh;

	public IHull[] Hulls => hulls.ToArray();

	public bool HasSuppressMeshModificationWarning => suppressMeshModificationWarning;

	public void AddHull(HullType type, PhysicsMaterial material, bool isChild, bool isTrigger)
	{
		hulls.Add(new Hull());
		hulls[hulls.Count - 1].name = "Hull " + hulls.Count;
		activeHull = hulls.Count - 1;
		hulls[hulls.Count - 1].colour = GizmoUtils.GetHullColour(activeHull);
		hulls[hulls.Count - 1].type = type;
		hulls[hulls.Count - 1].material = material;
		hulls[hulls.Count - 1].isTrigger = isTrigger;
		hulls[hulls.Count - 1].isChildCollider = isChild;
	}

	public void RemoveHull(int index)
	{
		if (index >= 0 && index < hulls.Count)
		{
			hulls[index].Destroy();
			hulls.RemoveAt(index);
		}
	}

	public void RemoveAllHulls()
	{
		for (int i = 0; i < hulls.Count; i++)
		{
			hulls[i].Destroy();
		}
		hulls.Clear();
	}

	public bool HasActiveHull()
	{
		if (activeHull >= 0)
		{
			return activeHull < hulls.Count;
		}
		return false;
	}

	public Hull GetActiveHull()
	{
		if (activeHull < 0 || activeHull >= hulls.Count)
		{
			return null;
		}
		return hulls[activeHull];
	}

	public bool ContainsMesh(Mesh m)
	{
		foreach (Hull hull in hulls)
		{
			if (hull.collisionMesh == m)
			{
				return true;
			}
			if (hull.faceCollisionMesh == m)
			{
				return true;
			}
			if (hull.autoMeshes == null)
			{
				continue;
			}
			Mesh[] autoMeshes = hull.autoMeshes;
			for (int i = 0; i < autoMeshes.Length; i++)
			{
				if (autoMeshes[i] == m)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAutoHulls()
	{
		foreach (Hull hull in hulls)
		{
			if (hull.type == HullType.Auto)
			{
				return true;
			}
		}
		return false;
	}

	public void SetAssetDirty()
	{
	}
}
