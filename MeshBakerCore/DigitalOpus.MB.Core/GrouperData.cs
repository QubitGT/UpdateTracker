using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class GrouperData
{
	public bool clusterOnLMIndex;

	public bool clusterByLODLevel;

	public Vector3 origin;

	public Vector3 cellSize = new Vector3(5f, 5f, 5f);

	public int pieNumSegments = 4;

	public Vector3 pieAxis = Vector3.up;

	public float ringSpacing = 100f;

	public bool combineSegmentsInInnermostRing;

	public bool includeCellsWithOnlyOneRenderer = true;

	public MB3_AgglomerativeClustering cluster;

	public float maxDistBetweenClusters = 1f;

	public float _lastMaxDistBetweenClusters;

	public float _ObjsExtents = 10f;

	public float _minDistBetweenClusters = 0.001f;

	public List<MB3_AgglomerativeClustering.ClusterNode> _clustersToDraw = new List<MB3_AgglomerativeClustering.ClusterNode>();

	public float[] _radii;
}
