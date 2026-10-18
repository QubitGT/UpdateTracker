using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_MeshBakerGrouperCluster : MB3_MeshBakerGrouperBehaviour
{
	public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
	{
		Dictionary<string, List<Renderer>> dictionary = new Dictionary<string, List<Renderer>>();
		for (int i = 0; i < d._clustersToDraw.Count; i++)
		{
			MB3_AgglomerativeClustering.ClusterNode clusterNode = d._clustersToDraw[i];
			List<Renderer> list = new List<Renderer>();
			for (int j = 0; j < clusterNode.leafs.Length; j++)
			{
				Renderer component = d.cluster.clusters[clusterNode.leafs[j]].leaf.go.GetComponent<Renderer>();
				if (component is MeshRenderer || component is SkinnedMeshRenderer)
				{
					list.Add(component);
				}
			}
			dictionary.Add("Cluster_" + i, list);
		}
		return dictionary;
	}

	public void BuildClusters(List<GameObject> gos, ProgressUpdateCancelableDelegate progFunc, GrouperData d)
	{
		if (gos.Count == 0)
		{
			Debug.LogWarning("No objects to cluster. Add some objects to the list of Objects To Combine.");
			return;
		}
		if (d.cluster == null)
		{
			d.cluster = new MB3_AgglomerativeClustering();
		}
		List<MB3_AgglomerativeClustering.item_s> list = new List<MB3_AgglomerativeClustering.item_s>();
		int i;
		for (i = 0; i < gos.Count; i++)
		{
			if (gos[i] != null && list.Find((MB3_AgglomerativeClustering.item_s x) => x.go == gos[i]) == null)
			{
				Renderer component = gos[i].GetComponent<Renderer>();
				if (component != null && (component is MeshRenderer || component is SkinnedMeshRenderer))
				{
					MB3_AgglomerativeClustering.item_s item_s = new MB3_AgglomerativeClustering.item_s();
					item_s.go = gos[i];
					item_s.coord = component.bounds.center;
					list.Add(item_s);
				}
			}
		}
		d.cluster.items = list;
		d.cluster.agglomerate(progFunc);
		if (!d.cluster.wasCanceled)
		{
			_BuildListOfClustersToDraw(progFunc, out var smallest, out var largest, d);
			d.maxDistBetweenClusters = Mathf.Lerp(smallest, largest, 0.9f);
		}
	}

	public void _BuildListOfClustersToDraw(ProgressUpdateCancelableDelegate progFunc, out float smallest, out float largest, GrouperData d)
	{
		if (d._clustersToDraw == null)
		{
			d._clustersToDraw = new List<MB3_AgglomerativeClustering.ClusterNode>();
		}
		d._clustersToDraw.Clear();
		if (d.cluster.clusters == null || d.cluster.clusters.Length == 0)
		{
			smallest = 1f;
			largest = 10f;
		}
		else
		{
			progFunc?.Invoke("Building Clusters To Draw A:", 0f);
			largest = 1f;
			smallest = 10000000f;
			for (int i = 0; i < d.cluster.clusters.Length; i++)
			{
				MB3_AgglomerativeClustering.ClusterNode clusterNode = d.cluster.clusters[i];
				if (clusterNode.distToMergedCentroid <= d.maxDistBetweenClusters)
				{
					if (d.includeCellsWithOnlyOneRenderer)
					{
						d._clustersToDraw.Add(clusterNode);
					}
					else if (clusterNode.leaf == null)
					{
						d._clustersToDraw.Add(clusterNode);
					}
				}
				if (clusterNode.distToMergedCentroid > largest)
				{
					largest = clusterNode.distToMergedCentroid;
				}
				if (clusterNode.height > 0 && clusterNode.distToMergedCentroid < smallest)
				{
					smallest = clusterNode.distToMergedCentroid;
				}
			}
		}
		progFunc?.Invoke("Building Clusters To Draw B:", 0f);
		List<MB3_AgglomerativeClustering.ClusterNode> list = new List<MB3_AgglomerativeClustering.ClusterNode>();
		for (int j = 0; j < d._clustersToDraw.Count; j++)
		{
			list.Add(d._clustersToDraw[j].cha);
			list.Add(d._clustersToDraw[j].chb);
		}
		for (int k = 0; k < list.Count; k++)
		{
			d._clustersToDraw.Remove(list[k]);
		}
		d._radii = new float[d._clustersToDraw.Count];
		progFunc?.Invoke("Building Clusters To Draw C:", 0f);
		for (int l = 0; l < d._radii.Length; l++)
		{
			MB3_AgglomerativeClustering.ClusterNode clusterNode2 = d._clustersToDraw[l];
			Bounds bounds = new Bounds(clusterNode2.centroid, Vector3.one);
			for (int m = 0; m < clusterNode2.leafs.Length; m++)
			{
				Renderer component = d.cluster.clusters[clusterNode2.leafs[m]].leaf.go.GetComponent<Renderer>();
				if (component != null)
				{
					bounds.Encapsulate(component.bounds);
				}
			}
			d._radii[l] = bounds.extents.magnitude;
		}
		progFunc?.Invoke("Building Clusters To Draw D:", 0f);
		if (smallest >= largest)
		{
			Debug.LogError("The smallest distance between clusters is greater than the largest distance between clusters. This should not happen.");
			smallest = 1E-05f;
			if (largest < 10f)
			{
				largest = 10f;
			}
		}
		d._ObjsExtents = largest + 1f;
		d._minDistBetweenClusters = 0.1f * smallest;
		if (d._ObjsExtents < 2f)
		{
			d._ObjsExtents = 2f;
		}
	}

	public override void DrawGizmos(Bounds sceneObjectBounds, GrouperData d)
	{
		if (d.cluster != null && d.cluster.clusters != null)
		{
			Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
			for (int i = 0; i < d._clustersToDraw.Count; i++)
			{
				Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
				Gizmos.DrawWireSphere(d._clustersToDraw[i].centroid, d._radii[i]);
			}
		}
	}

	public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
	{
		return MB3_MeshBakerGrouper.ClusterType.agglomerative;
	}
}
