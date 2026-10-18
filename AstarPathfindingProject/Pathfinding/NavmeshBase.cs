using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public abstract class NavmeshBase : NavGraph, INavmesh, INavmeshHolder, ITransformedGraph, IRaycastableGraph
{
	public const int VertexIndexMask = 4095;

	public const int TileIndexMask = 524287;

	public const int TileIndexOffset = 12;

	[JsonMember]
	public Vector3 forcedBoundsSize = new Vector3(100f, 40f, 100f);

	[JsonMember]
	public bool showMeshOutline = true;

	[JsonMember]
	public bool showNodeConnections;

	[JsonMember]
	public bool showMeshSurface = true;

	public int tileXCount;

	public int tileZCount;

	protected NavmeshTile[] tiles;

	[JsonMember]
	public bool nearestSearchOnlyXZ;

	[JsonMember]
	public bool enableNavmeshCutting = true;

	internal readonly NavmeshUpdates.NavmeshUpdateSettings navmeshUpdateData;

	private bool batchTileUpdate;

	private List<int> batchUpdatedTiles = new List<int>();

	private List<MeshNode> batchNodesToDestroy = new List<MeshNode>();

	public GraphTransform transform = new GraphTransform(Matrix4x4.identity);

	public Action<NavmeshTile[]> OnRecalculatedTiles;

	private static readonly NNConstraint NNConstraintDistanceXZ;

	private Dictionary<int, int> nodeRecyclingHashBuffer = new Dictionary<int, int>();

	private static readonly NNConstraint NNConstraintNoneXZ;

	private static readonly byte[] LinecastShapeEdgeLookup;

	public abstract float TileWorldSizeX { get; }

	public abstract float TileWorldSizeZ { get; }

	protected abstract float MaxTileConnectionEdgeDistance { get; }

	GraphTransform ITransformedGraph.transform => transform;

	protected abstract bool RecalculateNormals { get; }

	public abstract GraphTransform CalculateTransform();

	public NavmeshTile GetTile(int x, int z)
	{
		return tiles[x + z * tileXCount];
	}

	public Int3 GetVertex(int index)
	{
		int num = (index >> 12) & 0x7FFFF;
		return tiles[num].GetVertex(index);
	}

	public Int3 GetVertexInGraphSpace(int index)
	{
		int num = (index >> 12) & 0x7FFFF;
		return tiles[num].GetVertexInGraphSpace(index);
	}

	public static int GetTileIndex(int index)
	{
		return (index >> 12) & 0x7FFFF;
	}

	public int GetVertexArrayIndex(int index)
	{
		return index & 0xFFF;
	}

	public void GetTileCoordinates(int tileIndex, out int x, out int z)
	{
		z = tileIndex / tileXCount;
		x = tileIndex - z * tileXCount;
	}

	public NavmeshTile[] GetTiles()
	{
		return tiles;
	}

	public Bounds GetTileBounds(IntRect rect)
	{
		return GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
	}

	public Bounds GetTileBounds(int x, int z, int width = 1, int depth = 1)
	{
		return transform.Transform(GetTileBoundsInGraphSpace(x, z, width, depth));
	}

	public Bounds GetTileBoundsInGraphSpace(IntRect rect)
	{
		return GetTileBoundsInGraphSpace(rect.xmin, rect.ymin, rect.Width, rect.Height);
	}

	public Bounds GetTileBoundsInGraphSpace(int x, int z, int width = 1, int depth = 1)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(new Vector3((float)x * TileWorldSizeX, 0f, (float)z * TileWorldSizeZ), new Vector3((float)(x + width) * TileWorldSizeX, forcedBoundsSize.y, (float)(z + depth) * TileWorldSizeZ));
		return result;
	}

	public Int2 GetTileCoordinates(Vector3 position)
	{
		position = transform.InverseTransform(position);
		position.x /= TileWorldSizeX;
		position.z /= TileWorldSizeZ;
		return new Int2((int)position.x, (int)position.z);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TriangleMeshNode.SetNavmeshHolder(active.data.GetGraphIndex(this), null);
		if (tiles != null)
		{
			for (int i = 0; i < tiles.Length; i++)
			{
				ObjectPool<BBTree>.Release(ref tiles[i].bbTree);
			}
		}
	}

	public override void RelocateNodes(Matrix4x4 deltaMatrix)
	{
		RelocateNodes(deltaMatrix * transform);
	}

	public void RelocateNodes(GraphTransform newTransform)
	{
		transform = newTransform;
		if (tiles == null)
		{
			return;
		}
		for (int i = 0; i < tiles.Length; i++)
		{
			NavmeshTile navmeshTile = tiles[i];
			if (navmeshTile != null)
			{
				navmeshTile.vertsInGraphSpace.CopyTo(navmeshTile.verts, 0);
				transform.Transform(navmeshTile.verts);
				for (int j = 0; j < navmeshTile.nodes.Length; j++)
				{
					navmeshTile.nodes[j].UpdatePositionFromVertices();
				}
				navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
			}
		}
	}

	protected NavmeshTile NewEmptyTile(int x, int z)
	{
		return new NavmeshTile
		{
			x = x,
			z = z,
			w = 1,
			d = 1,
			verts = new Int3[0],
			vertsInGraphSpace = new Int3[0],
			tris = new int[0],
			nodes = new TriangleMeshNode[0],
			bbTree = ObjectPool<BBTree>.Claim(),
			graph = this
		};
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (tiles == null)
		{
			return;
		}
		for (int i = 0; i < tiles.Length; i++)
		{
			if (tiles[i] == null || tiles[i].x + tiles[i].z * tileXCount != i)
			{
				continue;
			}
			TriangleMeshNode[] nodes = tiles[i].nodes;
			if (nodes != null)
			{
				for (int j = 0; j < nodes.Length; j++)
				{
					action(nodes[j]);
				}
			}
		}
	}

	public IntRect GetTouchingTiles(Bounds bounds, float margin = 0f)
	{
		bounds = transform.InverseTransform(bounds);
		return IntRect.Intersection(new IntRect(Mathf.FloorToInt((bounds.min.x - margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.min.z - margin) / TileWorldSizeZ), Mathf.FloorToInt((bounds.max.x + margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.max.z + margin) / TileWorldSizeZ)), new IntRect(0, 0, tileXCount - 1, tileZCount - 1));
	}

	public IntRect GetTouchingTilesInGraphSpace(Rect rect)
	{
		return IntRect.Intersection(new IntRect(Mathf.FloorToInt(rect.xMin / TileWorldSizeX), Mathf.FloorToInt(rect.yMin / TileWorldSizeZ), Mathf.FloorToInt(rect.xMax / TileWorldSizeX), Mathf.FloorToInt(rect.yMax / TileWorldSizeZ)), new IntRect(0, 0, tileXCount - 1, tileZCount - 1));
	}

	public IntRect GetTouchingTilesRound(Bounds bounds)
	{
		bounds = transform.InverseTransform(bounds);
		return IntRect.Intersection(new IntRect(Mathf.RoundToInt(bounds.min.x / TileWorldSizeX), Mathf.RoundToInt(bounds.min.z / TileWorldSizeZ), Mathf.RoundToInt(bounds.max.x / TileWorldSizeX) - 1, Mathf.RoundToInt(bounds.max.z / TileWorldSizeZ) - 1), new IntRect(0, 0, tileXCount - 1, tileZCount - 1));
	}

	protected void ConnectTileWithNeighbours(NavmeshTile tile, bool onlyUnflagged = false)
	{
		if (tile.w != 1 || tile.d != 1)
		{
			throw new ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
		}
		for (int i = -1; i <= 1; i++)
		{
			int num = tile.z + i;
			if (num < 0 || num >= tileZCount)
			{
				continue;
			}
			for (int j = -1; j <= 1; j++)
			{
				int num2 = tile.x + j;
				if (num2 >= 0 && num2 < tileXCount && j == 0 != (i == 0))
				{
					NavmeshTile navmeshTile = tiles[num2 + num * tileXCount];
					if (!onlyUnflagged || !navmeshTile.flag)
					{
						ConnectTiles(navmeshTile, tile);
					}
				}
			}
		}
	}

	protected void RemoveConnectionsFromTile(NavmeshTile tile)
	{
		if (tile.x > 0)
		{
			int num = tile.x - 1;
			for (int i = tile.z; i < tile.z + tile.d; i++)
			{
				RemoveConnectionsFromTo(tiles[num + i * tileXCount], tile);
			}
		}
		if (tile.x + tile.w < tileXCount)
		{
			int num2 = tile.x + tile.w;
			for (int j = tile.z; j < tile.z + tile.d; j++)
			{
				RemoveConnectionsFromTo(tiles[num2 + j * tileXCount], tile);
			}
		}
		if (tile.z > 0)
		{
			int num3 = tile.z - 1;
			for (int k = tile.x; k < tile.x + tile.w; k++)
			{
				RemoveConnectionsFromTo(tiles[k + num3 * tileXCount], tile);
			}
		}
		if (tile.z + tile.d < tileZCount)
		{
			int num4 = tile.z + tile.d;
			for (int l = tile.x; l < tile.x + tile.w; l++)
			{
				RemoveConnectionsFromTo(tiles[l + num4 * tileXCount], tile);
			}
		}
	}

	protected void RemoveConnectionsFromTo(NavmeshTile a, NavmeshTile b)
	{
		if (a == null || b == null || a == b)
		{
			return;
		}
		int num = b.x + b.z * tileXCount;
		for (int i = 0; i < a.nodes.Length; i++)
		{
			TriangleMeshNode triangleMeshNode = a.nodes[i];
			if (triangleMeshNode.connections == null)
			{
				continue;
			}
			for (int j = 0; j < triangleMeshNode.connections.Length; j++)
			{
				if (triangleMeshNode.connections[j].node is TriangleMeshNode triangleMeshNode2 && ((triangleMeshNode2.GetVertexIndex(0) >> 12) & 0x7FFFF) == num)
				{
					triangleMeshNode.RemoveConnection(triangleMeshNode.connections[j].node);
					j--;
				}
			}
		}
	}

	public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		return GetNearestForce(position, (constraint != null && constraint.distanceXZ) ? NNConstraintDistanceXZ : null);
	}

	public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
	{
		if (tiles == null)
		{
			return default(NNInfoInternal);
		}
		Int2 tileCoordinates = GetTileCoordinates(position);
		tileCoordinates.x = Mathf.Clamp(tileCoordinates.x, 0, tileXCount - 1);
		tileCoordinates.y = Mathf.Clamp(tileCoordinates.y, 0, tileZCount - 1);
		int num = Math.Max(tileXCount, tileZCount);
		NNInfoInternal nNInfoInternal = default(NNInfoInternal);
		float distance = float.PositiveInfinity;
		bool flag = nearestSearchOnlyXZ || (constraint?.distanceXZ ?? false);
		for (int i = 0; i < num && !(distance < (float)(i - 2) * Math.Max(TileWorldSizeX, TileWorldSizeX)); i++)
		{
			int num2 = Math.Min(i + tileCoordinates.y + 1, tileZCount);
			for (int j = Math.Max(-i + tileCoordinates.y, 0); j < num2; j++)
			{
				int num3 = Math.Abs(i - Math.Abs(j - tileCoordinates.y));
				int num4 = num3;
				do
				{
					int num5 = -num4 + tileCoordinates.x;
					if (num5 >= 0 && num5 < tileXCount)
					{
						NavmeshTile navmeshTile = tiles[num5 + j * tileXCount];
						if (navmeshTile != null)
						{
							nNInfoInternal = ((!flag) ? navmeshTile.bbTree.QueryClosest(position, constraint, ref distance, nNInfoInternal) : navmeshTile.bbTree.QueryClosestXZ(position, constraint, ref distance, nNInfoInternal));
						}
					}
					num4 = -num4;
				}
				while (num4 != num3);
			}
		}
		nNInfoInternal.node = nNInfoInternal.constrainedNode;
		nNInfoInternal.constrainedNode = null;
		nNInfoInternal.clampedPosition = nNInfoInternal.constClampedPosition;
		return nNInfoInternal;
	}

	public GraphNode PointOnNavmesh(Vector3 position, NNConstraint constraint)
	{
		if (tiles == null)
		{
			return null;
		}
		Int2 tileCoordinates = GetTileCoordinates(position);
		if (tileCoordinates.x < 0 || tileCoordinates.y < 0 || tileCoordinates.x >= tileXCount || tileCoordinates.y >= tileZCount)
		{
			return null;
		}
		return GetTile(tileCoordinates.x, tileCoordinates.y)?.bbTree.QueryInside(position, constraint);
	}

	protected void FillWithEmptyTiles()
	{
		for (int i = 0; i < tileZCount; i++)
		{
			for (int j = 0; j < tileXCount; j++)
			{
				tiles[i * tileXCount + j] = NewEmptyTile(j, i);
			}
		}
	}

	protected static void CreateNodeConnections(TriangleMeshNode[] nodes)
	{
		List<Connection> list = ListPool<Connection>.Claim();
		Dictionary<Int2, int> obj = ObjectPoolSimple<Dictionary<Int2, int>>.Claim();
		obj.Clear();
		for (int i = 0; i < nodes.Length; i++)
		{
			TriangleMeshNode triangleMeshNode = nodes[i];
			int vertexCount = triangleMeshNode.GetVertexCount();
			for (int j = 0; j < vertexCount; j++)
			{
				Int2 key = new Int2(triangleMeshNode.GetVertexIndex(j), triangleMeshNode.GetVertexIndex((j + 1) % vertexCount));
				if (!obj.ContainsKey(key))
				{
					obj.Add(key, i);
				}
			}
		}
		foreach (TriangleMeshNode triangleMeshNode2 in nodes)
		{
			list.Clear();
			int vertexCount2 = triangleMeshNode2.GetVertexCount();
			for (int l = 0; l < vertexCount2; l++)
			{
				int vertexIndex = triangleMeshNode2.GetVertexIndex(l);
				int vertexIndex2 = triangleMeshNode2.GetVertexIndex((l + 1) % vertexCount2);
				if (!obj.TryGetValue(new Int2(vertexIndex2, vertexIndex), out var value))
				{
					continue;
				}
				TriangleMeshNode triangleMeshNode3 = nodes[value];
				int vertexCount3 = triangleMeshNode3.GetVertexCount();
				for (int m = 0; m < vertexCount3; m++)
				{
					if (triangleMeshNode3.GetVertexIndex(m) == vertexIndex2 && triangleMeshNode3.GetVertexIndex((m + 1) % vertexCount3) == vertexIndex)
					{
						list.Add(new Connection(triangleMeshNode3, (uint)(triangleMeshNode2.position - triangleMeshNode3.position).costMagnitude, (byte)l));
						break;
					}
				}
			}
			triangleMeshNode2.connections = list.ToArrayFromPool();
			triangleMeshNode2.SetConnectivityDirty();
		}
		obj.Clear();
		ObjectPoolSimple<Dictionary<Int2, int>>.Release(ref obj);
		ListPool<Connection>.Release(ref list);
	}

	protected void ConnectTiles(NavmeshTile tile1, NavmeshTile tile2)
	{
		if (tile1 == null || tile2 == null)
		{
			return;
		}
		if (tile1.nodes == null)
		{
			throw new ArgumentException("tile1 does not contain any nodes");
		}
		if (tile2.nodes == null)
		{
			throw new ArgumentException("tile2 does not contain any nodes");
		}
		int num = Mathf.Clamp(tile2.x, tile1.x, tile1.x + tile1.w - 1);
		int num2 = Mathf.Clamp(tile1.x, tile2.x, tile2.x + tile2.w - 1);
		int num3 = Mathf.Clamp(tile2.z, tile1.z, tile1.z + tile1.d - 1);
		int num4 = Mathf.Clamp(tile1.z, tile2.z, tile2.z + tile2.d - 1);
		int i;
		int i2;
		int num5;
		int num6;
		float num7;
		if (num == num2)
		{
			i = 2;
			i2 = 0;
			num5 = num3;
			num6 = num4;
			num7 = TileWorldSizeZ;
		}
		else
		{
			if (num3 != num4)
			{
				throw new ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
			}
			i = 0;
			i2 = 2;
			num5 = num;
			num6 = num2;
			num7 = TileWorldSizeX;
		}
		if (Math.Abs(num5 - num6) != 1)
		{
			throw new ArgumentException("Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '" + num5 + "' and '" + num6 + "')");
		}
		int num8 = (int)Math.Round((float)Math.Max(num5, num6) * num7 * 1000f);
		TriangleMeshNode[] nodes = tile1.nodes;
		TriangleMeshNode[] nodes2 = tile2.nodes;
		TriangleMeshNode[] array = ArrayPool<TriangleMeshNode>.Claim(nodes2.Length);
		int num9 = 0;
		for (int j = 0; j < nodes2.Length; j++)
		{
			TriangleMeshNode triangleMeshNode = nodes2[j];
			int vertexCount = triangleMeshNode.GetVertexCount();
			for (int k = 0; k < vertexCount; k++)
			{
				Int3 vertexInGraphSpace = triangleMeshNode.GetVertexInGraphSpace(k);
				Int3 vertexInGraphSpace2 = triangleMeshNode.GetVertexInGraphSpace((k + 1) % vertexCount);
				if (Math.Abs(vertexInGraphSpace[i] - num8) < 2 && Math.Abs(vertexInGraphSpace2[i] - num8) < 2)
				{
					array[num9] = nodes2[j];
					num9++;
					break;
				}
			}
		}
		foreach (TriangleMeshNode triangleMeshNode2 in nodes)
		{
			int vertexCount2 = triangleMeshNode2.GetVertexCount();
			for (int m = 0; m < vertexCount2; m++)
			{
				Int3 vertexInGraphSpace3 = triangleMeshNode2.GetVertexInGraphSpace(m);
				Int3 vertexInGraphSpace4 = triangleMeshNode2.GetVertexInGraphSpace((m + 1) % vertexCount2);
				if (Math.Abs(vertexInGraphSpace3[i] - num8) >= 2 || Math.Abs(vertexInGraphSpace4[i] - num8) >= 2)
				{
					continue;
				}
				int num10 = Math.Min(vertexInGraphSpace3[i2], vertexInGraphSpace4[i2]);
				int num11 = Math.Max(vertexInGraphSpace3[i2], vertexInGraphSpace4[i2]);
				if (num10 == num11)
				{
					continue;
				}
				for (int n = 0; n < num9; n++)
				{
					TriangleMeshNode triangleMeshNode3 = array[n];
					int vertexCount3 = triangleMeshNode3.GetVertexCount();
					for (int num12 = 0; num12 < vertexCount3; num12++)
					{
						Int3 vertexInGraphSpace5 = triangleMeshNode3.GetVertexInGraphSpace(num12);
						Int3 vertexInGraphSpace6 = triangleMeshNode3.GetVertexInGraphSpace((num12 + 1) % vertexCount3);
						if (Math.Abs(vertexInGraphSpace5[i] - num8) < 2 && Math.Abs(vertexInGraphSpace6[i] - num8) < 2)
						{
							int num13 = Math.Min(vertexInGraphSpace5[i2], vertexInGraphSpace6[i2]);
							int num14 = Math.Max(vertexInGraphSpace5[i2], vertexInGraphSpace6[i2]);
							if (num13 != num14 && num11 > num13 && num10 < num14 && ((vertexInGraphSpace3 == vertexInGraphSpace5 && vertexInGraphSpace4 == vertexInGraphSpace6) || (vertexInGraphSpace3 == vertexInGraphSpace6 && vertexInGraphSpace4 == vertexInGraphSpace5) || VectorMath.SqrDistanceSegmentSegment((Vector3)vertexInGraphSpace3, (Vector3)vertexInGraphSpace4, (Vector3)vertexInGraphSpace5, (Vector3)vertexInGraphSpace6) < MaxTileConnectionEdgeDistance * MaxTileConnectionEdgeDistance))
							{
								uint costMagnitude = (uint)(triangleMeshNode2.position - triangleMeshNode3.position).costMagnitude;
								triangleMeshNode2.AddConnection(triangleMeshNode3, costMagnitude, (byte)m);
								triangleMeshNode3.AddConnection(triangleMeshNode2, costMagnitude, (byte)num12);
							}
						}
					}
				}
			}
		}
		ArrayPool<TriangleMeshNode>.Release(ref array);
	}

	public void StartBatchTileUpdate()
	{
		if (batchTileUpdate)
		{
			throw new InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
		}
		batchTileUpdate = true;
	}

	private void DestroyNodes(List<MeshNode> nodes)
	{
		for (int i = 0; i < batchNodesToDestroy.Count; i++)
		{
			batchNodesToDestroy[i].TemporaryFlag1 = true;
		}
		for (int j = 0; j < batchNodesToDestroy.Count; j++)
		{
			MeshNode meshNode = batchNodesToDestroy[j];
			for (int k = 0; k < meshNode.connections.Length; k++)
			{
				GraphNode node = meshNode.connections[k].node;
				if (!node.TemporaryFlag1)
				{
					node.RemoveConnection(meshNode);
				}
			}
			ArrayPool<Connection>.Release(ref meshNode.connections, allowNonPowerOfTwo: true);
			meshNode.Destroy();
		}
	}

	private void TryConnect(int tileIdx1, int tileIdx2)
	{
		if (!tiles[tileIdx1].flag || !tiles[tileIdx2].flag || tileIdx1 < tileIdx2)
		{
			ConnectTiles(tiles[tileIdx1], tiles[tileIdx2]);
		}
	}

	public void EndBatchTileUpdate()
	{
		if (!batchTileUpdate)
		{
			throw new InvalidOperationException("Calling EndBatchTileUpdate when batching had not yet been started");
		}
		batchTileUpdate = false;
		DestroyNodes(batchNodesToDestroy);
		batchNodesToDestroy.ClearFast();
		for (int i = 0; i < batchUpdatedTiles.Count; i++)
		{
			tiles[batchUpdatedTiles[i]].flag = true;
		}
		for (int j = 0; j < batchUpdatedTiles.Count; j++)
		{
			int num = batchUpdatedTiles[j] % tileXCount;
			int num2 = batchUpdatedTiles[j] / tileXCount;
			if (num > 0)
			{
				TryConnect(batchUpdatedTiles[j], batchUpdatedTiles[j] - 1);
			}
			if (num < tileXCount - 1)
			{
				TryConnect(batchUpdatedTiles[j], batchUpdatedTiles[j] + 1);
			}
			if (num2 > 0)
			{
				TryConnect(batchUpdatedTiles[j], batchUpdatedTiles[j] - tileXCount);
			}
			if (num2 < tileZCount - 1)
			{
				TryConnect(batchUpdatedTiles[j], batchUpdatedTiles[j] + tileXCount);
			}
		}
		for (int k = 0; k < batchUpdatedTiles.Count; k++)
		{
			tiles[batchUpdatedTiles[k]].flag = false;
		}
		batchUpdatedTiles.ClearFast();
	}

	protected void ClearTile(int x, int z)
	{
		if (!batchTileUpdate)
		{
			throw new Exception("Must be called during a batch update. See StartBatchTileUpdate");
		}
		NavmeshTile tile = GetTile(x, z);
		if (tile == null)
		{
			return;
		}
		TriangleMeshNode[] nodes = tile.nodes;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] != null)
			{
				batchNodesToDestroy.Add(nodes[i]);
			}
		}
		ObjectPool<BBTree>.Release(ref tile.bbTree);
		tiles[x + z * tileXCount] = null;
	}

	private void PrepareNodeRecycling(int x, int z, Int3[] verts, int[] tris, TriangleMeshNode[] recycledNodeBuffer)
	{
		NavmeshTile tile = GetTile(x, z);
		if (tile == null || tile.nodes.Length == 0)
		{
			return;
		}
		TriangleMeshNode[] nodes = tile.nodes;
		Dictionary<int, int> dictionary = nodeRecyclingHashBuffer;
		int num = 0;
		int num2 = 0;
		while (num < tris.Length)
		{
			dictionary[verts[tris[num]].GetHashCode() + verts[tris[num + 1]].GetHashCode() + verts[tris[num + 2]].GetHashCode()] = num2;
			num += 3;
			num2++;
		}
		List<Connection> list = ListPool<Connection>.Claim();
		for (int i = 0; i < nodes.Length; i++)
		{
			TriangleMeshNode triangleMeshNode = nodes[i];
			triangleMeshNode.GetVerticesInGraphSpace(out var v, out var v2, out var v3);
			int key = v.GetHashCode() + v2.GetHashCode() + v3.GetHashCode();
			if (!dictionary.TryGetValue(key, out var value) || !(verts[tris[3 * value]] == v) || !(verts[tris[3 * value + 1]] == v2) || !(verts[tris[3 * value + 2]] == v3))
			{
				continue;
			}
			recycledNodeBuffer[value] = triangleMeshNode;
			nodes[i] = null;
			for (int j = 0; j < triangleMeshNode.connections.Length; j++)
			{
				if (triangleMeshNode.connections[j].node.GraphIndex != triangleMeshNode.GraphIndex)
				{
					list.Add(triangleMeshNode.connections[j]);
				}
			}
			ArrayPool<Connection>.Release(ref triangleMeshNode.connections, allowNonPowerOfTwo: true);
			if (list.Count > 0)
			{
				triangleMeshNode.connections = list.ToArrayFromPool();
				triangleMeshNode.SetConnectivityDirty();
				list.Clear();
			}
		}
		dictionary.Clear();
		ListPool<Connection>.Release(ref list);
	}

	public void ReplaceTile(int x, int z, Int3[] verts, int[] tris)
	{
		int num = 1;
		int num2 = 1;
		if (x + num > tileXCount || z + num2 > tileZCount || x < 0 || z < 0)
		{
			throw new ArgumentException("Tile is placed at an out of bounds position or extends out of the graph bounds (" + x + ", " + z + " [" + num + ", " + num2 + "] " + tileXCount + " " + tileZCount + ")");
		}
		if (tris.Length % 3 != 0)
		{
			throw new ArgumentException("Triangle array's length must be a multiple of 3 (tris)");
		}
		if (verts.Length > 4095)
		{
			Debug.LogError("Too many vertices in the tile (" + verts.Length + " > " + 4095 + ")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit. Or you can use a smaller tile size to reduce the likelihood of this happening.");
			verts = new Int3[0];
			tris = new int[0];
		}
		bool flag = !batchTileUpdate;
		if (flag)
		{
			StartBatchTileUpdate();
		}
		NavmeshTile navmeshTile = new NavmeshTile
		{
			x = x,
			z = z,
			w = num,
			d = num2,
			tris = tris,
			bbTree = ObjectPool<BBTree>.Claim(),
			graph = this
		};
		if (!Mathf.Approximately((float)x * TileWorldSizeX * 1000f, (float)Math.Round((float)x * TileWorldSizeX * 1000f)))
		{
			Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
		}
		if (!Mathf.Approximately((float)z * TileWorldSizeZ * 1000f, (float)Math.Round((float)z * TileWorldSizeZ * 1000f)))
		{
			Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
		}
		Int3 @int = (Int3)new Vector3((float)x * TileWorldSizeX, 0f, (float)z * TileWorldSizeZ);
		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] += @int;
		}
		navmeshTile.vertsInGraphSpace = verts;
		navmeshTile.verts = (Int3[])verts.Clone();
		transform.Transform(navmeshTile.verts);
		TriangleMeshNode[] array = (navmeshTile.nodes = new TriangleMeshNode[tris.Length / 3]);
		PrepareNodeRecycling(x, z, navmeshTile.vertsInGraphSpace, tris, navmeshTile.nodes);
		ClearTile(x, z);
		tiles[x + z * tileXCount] = navmeshTile;
		batchUpdatedTiles.Add(x + z * tileXCount);
		CreateNodes(array, navmeshTile.tris, x + z * tileXCount, (uint)active.data.GetGraphIndex(this));
		navmeshTile.bbTree.RebuildFrom(array);
		CreateNodeConnections(navmeshTile.nodes);
		if (flag)
		{
			EndBatchTileUpdate();
		}
	}

	protected void CreateNodes(TriangleMeshNode[] buffer, int[] tris, int tileIndex, uint graphIndex)
	{
		if (buffer == null || buffer.Length < tris.Length / 3)
		{
			throw new ArgumentException("buffer must be non null and at least as large as tris.Length/3");
		}
		tileIndex <<= 12;
		for (int i = 0; i < buffer.Length; i++)
		{
			TriangleMeshNode triangleMeshNode = buffer[i];
			if (triangleMeshNode == null)
			{
				triangleMeshNode = (buffer[i] = new TriangleMeshNode(active));
			}
			triangleMeshNode.Walkable = true;
			triangleMeshNode.Tag = 0u;
			triangleMeshNode.Penalty = initialPenalty;
			triangleMeshNode.GraphIndex = graphIndex;
			triangleMeshNode.v0 = tris[i * 3] | tileIndex;
			triangleMeshNode.v1 = tris[i * 3 + 1] | tileIndex;
			triangleMeshNode.v2 = tris[i * 3 + 2] | tileIndex;
			if (RecalculateNormals && !VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertexInGraphSpace(0), triangleMeshNode.GetVertexInGraphSpace(1), triangleMeshNode.GetVertexInGraphSpace(2)))
			{
				Memory.Swap(ref tris[i * 3], ref tris[i * 3 + 2]);
				Memory.Swap(ref triangleMeshNode.v0, ref triangleMeshNode.v2);
			}
			triangleMeshNode.UpdatePositionFromVertices();
		}
	}

	public NavmeshBase()
	{
		navmeshUpdateData = new NavmeshUpdates.NavmeshUpdateSettings(this);
	}

	public bool Linecast(Vector3 origin, Vector3 end)
	{
		return Linecast(origin, end, null);
	}

	public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast(this, origin, end, hint, out hit, null);
	}

	public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint)
	{
		GraphHitInfo hit;
		return Linecast(this, origin, end, hint, out hit, null);
	}

	public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
	{
		return Linecast(this, origin, end, hint, out hit, trace);
	}

	public bool Linecast(Vector3 origin, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace, Func<GraphNode, bool> filter)
	{
		return Linecast(this, origin, end, null, out hit, trace, filter);
	}

	public static bool Linecast(NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast(graph, origin, end, hint, out hit, null);
	}

	static NavmeshBase()
	{
		NNConstraintDistanceXZ = new NNConstraint
		{
			distanceXZ = true
		};
		NNConstraintNoneXZ = new NNConstraint
		{
			constrainWalkability = false,
			constrainArea = false,
			constrainTags = false,
			constrainDistance = false,
			graphMask = -1,
			distanceXZ = true
		};
		LinecastShapeEdgeLookup = new byte[64];
		Side[] array = new Side[3];
		for (int i = 0; i < LinecastShapeEdgeLookup.Length; i++)
		{
			array[0] = (Side)((uint)i & 3u);
			array[1] = (Side)((uint)(i >> 2) & 3u);
			array[2] = (Side)((uint)(i >> 4) & 3u);
			LinecastShapeEdgeLookup[i] = byte.MaxValue;
			if (array[0] == (Side)3 || array[1] == (Side)3 || array[2] == (Side)3)
			{
				continue;
			}
			int num = int.MaxValue;
			for (int j = 0; j < 3; j++)
			{
				if ((array[j] == Side.Left || array[j] == Side.Colinear) && (array[(j + 1) % 3] == Side.Right || array[(j + 1) % 3] == Side.Colinear))
				{
					int num2 = ((array[j] == Side.Colinear) ? 1 : 0) + ((array[(j + 1) % 3] == Side.Colinear) ? 1 : 0);
					if (num2 < num)
					{
						LinecastShapeEdgeLookup[i] = (byte)j;
						num = num2;
					}
				}
			}
		}
	}

	public static bool Linecast(NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace, Func<GraphNode, bool> filter = null)
	{
		hit = default(GraphHitInfo);
		if (float.IsNaN(origin.x + origin.y + origin.z))
		{
			throw new ArgumentException("origin is NaN");
		}
		if (float.IsNaN(end.x + end.y + end.z))
		{
			throw new ArgumentException("end is NaN");
		}
		TriangleMeshNode triangleMeshNode = hint as TriangleMeshNode;
		if (triangleMeshNode == null)
		{
			triangleMeshNode = graph.GetNearest(origin, NNConstraintNoneXZ).node as TriangleMeshNode;
			if (triangleMeshNode == null)
			{
				Debug.LogError("Could not find a valid node to start from");
				hit.origin = origin;
				hit.point = origin;
				return true;
			}
		}
		Int3 @int = triangleMeshNode.ClosestPointOnNodeXZInGraphSpace(origin);
		hit.origin = graph.transform.Transform((Vector3)@int);
		if (!triangleMeshNode.Walkable || (filter != null && !filter(triangleMeshNode)))
		{
			hit.node = triangleMeshNode;
			hit.point = hit.origin;
			hit.tangentOrigin = hit.origin;
			return true;
		}
		Int3 int2 = (Int3)graph.transform.InverseTransform(end);
		if (@int == int2)
		{
			hit.point = hit.origin;
			hit.node = triangleMeshNode;
			trace?.Add(triangleMeshNode);
			return false;
		}
		int num = 0;
		Int3 v;
		Int3 v2;
		Int3 v3;
		int num3;
		bool flag;
		do
		{
			num++;
			if (num > 2000)
			{
				Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
				return true;
			}
			trace?.Add(triangleMeshNode);
			triangleMeshNode.GetVerticesInGraphSpace(out v, out v2, out v3);
			int num2 = (int)VectorMath.SideXZ(@int, int2, v);
			num2 |= (int)((uint)VectorMath.SideXZ(@int, int2, v2) << 2);
			num2 |= (int)((uint)VectorMath.SideXZ(@int, int2, v3) << 4);
			num3 = LinecastShapeEdgeLookup[num2];
			if (VectorMath.SideXZ(num3 switch
			{
				1 => v2, 
				0 => v, 
				_ => v3, 
			}, num3 switch
			{
				1 => v3, 
				0 => v2, 
				_ => v, 
			}, int2) != Side.Left)
			{
				hit.point = end;
				hit.node = triangleMeshNode;
				TriangleMeshNode triangleMeshNode2 = graph.GetNearest(end, NNConstraintNoneXZ).node as TriangleMeshNode;
				if (triangleMeshNode2 == triangleMeshNode || triangleMeshNode2 == null)
				{
					return false;
				}
				return true;
			}
			if (num3 == 255)
			{
				Debug.LogError("Line does not intersect node at all");
				hit.node = triangleMeshNode;
				hit.point = (hit.tangentOrigin = hit.origin);
				return true;
			}
			flag = false;
			Connection[] connections = triangleMeshNode.connections;
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].shapeEdge != num3 || !(connections[i].node is TriangleMeshNode { Walkable: not false } triangleMeshNode3) || (filter != null && !filter(triangleMeshNode3)))
				{
					continue;
				}
				Connection[] connections2 = triangleMeshNode3.connections;
				int num4 = -1;
				for (int j = 0; j < connections2.Length; j++)
				{
					if (connections2[j].node == triangleMeshNode)
					{
						num4 = connections2[j].shapeEdge;
						break;
					}
				}
				if (num4 != -1)
				{
					Side side = VectorMath.SideXZ(@int, int2, triangleMeshNode3.GetVertexInGraphSpace(num4));
					Side side2 = VectorMath.SideXZ(@int, int2, triangleMeshNode3.GetVertexInGraphSpace((num4 + 1) % 3));
					flag = (side == Side.Right || side == Side.Colinear) && (side2 == Side.Left || side2 == Side.Colinear);
					if (flag)
					{
						triangleMeshNode = triangleMeshNode3;
						break;
					}
				}
			}
		}
		while (flag);
		Vector3 vector = (Vector3)(num3 switch
		{
			1 => v2, 
			0 => v, 
			_ => v3, 
		});
		Vector3 vector2 = (Vector3)(num3 switch
		{
			1 => v3, 
			0 => v2, 
			_ => v, 
		});
		Vector3 point = VectorMath.LineIntersectionPointXZ(vector, vector2, (Vector3)@int, (Vector3)int2);
		hit.point = graph.transform.Transform(point);
		hit.node = triangleMeshNode;
		Vector3 vector3 = graph.transform.Transform(vector);
		Vector3 vector4 = graph.transform.Transform(vector2);
		hit.tangent = vector4 - vector3;
		hit.tangentOrigin = vector3;
		return true;
	}

	public override void OnDrawGizmos(RetainedGizmos gizmos, bool drawNodes)
	{
		if (!drawNodes)
		{
			return;
		}
		using (GraphGizmoHelper graphGizmoHelper = gizmos.GetSingleFrameGizmoHelper(active))
		{
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(Vector3.zero, forcedBoundsSize);
			graphGizmoHelper.builder.DrawWireCube(CalculateTransform(), bounds, Color.white);
		}
		if (tiles != null && (showMeshSurface || showMeshOutline || showNodeConnections))
		{
			RetainedGizmos.Hasher hasher = new RetainedGizmos.Hasher(active);
			hasher.AddHash(showMeshOutline ? 1 : 0);
			hasher.AddHash(showMeshSurface ? 1 : 0);
			hasher.AddHash(showNodeConnections ? 1 : 0);
			int num = 0;
			RetainedGizmos.Hasher hasher2 = hasher;
			int num2 = 0;
			for (int i = 0; i < tiles.Length; i++)
			{
				if (tiles[i] == null)
				{
					continue;
				}
				TriangleMeshNode[] nodes = tiles[i].nodes;
				for (int j = 0; j < nodes.Length; j++)
				{
					hasher2.HashNode(nodes[j]);
				}
				num2 += nodes.Length;
				if (num2 <= 1024 && i % tileXCount != tileXCount - 1 && i != tiles.Length - 1)
				{
					continue;
				}
				if (!gizmos.Draw(hasher2))
				{
					using GraphGizmoHelper graphGizmoHelper2 = gizmos.GetGizmoHelper(active, hasher2);
					if (showMeshSurface || showMeshOutline)
					{
						CreateNavmeshSurfaceVisualization(tiles, num, i + 1, graphGizmoHelper2);
						CreateNavmeshOutlineVisualization(tiles, num, i + 1, graphGizmoHelper2);
					}
					if (showNodeConnections)
					{
						for (int k = num; k <= i; k++)
						{
							if (tiles[k] != null)
							{
								TriangleMeshNode[] nodes2 = tiles[k].nodes;
								for (int l = 0; l < nodes2.Length; l++)
								{
									graphGizmoHelper2.DrawConnections(nodes2[l]);
								}
							}
						}
					}
				}
				gizmos.Draw(hasher2);
				num = i + 1;
				hasher2 = hasher;
				num2 = 0;
			}
		}
		if (active.showUnwalkableNodes)
		{
			DrawUnwalkableNodes(active.unwalkableNodeDebugSize);
		}
	}

	private void CreateNavmeshSurfaceVisualization(NavmeshTile[] tiles, int startTile, int endTile, GraphGizmoHelper helper)
	{
		int num = 0;
		for (int i = startTile; i < endTile; i++)
		{
			if (tiles[i] != null)
			{
				num += tiles[i].nodes.Length;
			}
		}
		Vector3[] array = ArrayPool<Vector3>.Claim(num * 3);
		Color[] array2 = ArrayPool<Color>.Claim(num * 3);
		int num2 = 0;
		for (int j = startTile; j < endTile; j++)
		{
			NavmeshTile navmeshTile = tiles[j];
			if (navmeshTile != null)
			{
				for (int k = 0; k < navmeshTile.nodes.Length; k++)
				{
					TriangleMeshNode triangleMeshNode = navmeshTile.nodes[k];
					triangleMeshNode.GetVertices(out var v, out var v2, out var v3);
					int num3 = num2 + k * 3;
					array[num3] = (Vector3)v;
					array[num3 + 1] = (Vector3)v2;
					array[num3 + 2] = (Vector3)v3;
					Color color = helper.NodeColor(triangleMeshNode);
					array2[num3] = (array2[num3 + 1] = (array2[num3 + 2] = color));
				}
				num2 += navmeshTile.nodes.Length * 3;
			}
		}
		if (showMeshSurface)
		{
			helper.DrawTriangles(array, array2, num);
		}
		if (showMeshOutline)
		{
			helper.DrawWireTriangles(array, array2, num);
		}
		ArrayPool<Vector3>.Release(ref array);
		ArrayPool<Color>.Release(ref array2);
	}

	private static void CreateNavmeshOutlineVisualization(NavmeshTile[] tiles, int startTile, int endTile, GraphGizmoHelper helper)
	{
		bool[] array = new bool[3];
		for (int i = startTile; i < endTile; i++)
		{
			NavmeshTile navmeshTile = tiles[i];
			if (navmeshTile == null)
			{
				continue;
			}
			for (int j = 0; j < navmeshTile.nodes.Length; j++)
			{
				bool flag;
				array[1] = (flag = (array[2] = false));
				array[0] = flag;
				TriangleMeshNode triangleMeshNode = navmeshTile.nodes[j];
				for (int k = 0; k < triangleMeshNode.connections.Length; k++)
				{
					if (!(triangleMeshNode.connections[k].node is TriangleMeshNode triangleMeshNode2) || triangleMeshNode2.GraphIndex != triangleMeshNode.GraphIndex)
					{
						continue;
					}
					for (int l = 0; l < 3; l++)
					{
						for (int m = 0; m < 3; m++)
						{
							if (triangleMeshNode.GetVertexIndex(l) == triangleMeshNode2.GetVertexIndex((m + 1) % 3) && triangleMeshNode.GetVertexIndex((l + 1) % 3) == triangleMeshNode2.GetVertexIndex(m))
							{
								array[l] = true;
								l = 3;
								break;
							}
						}
					}
				}
				Color color = helper.NodeColor(triangleMeshNode);
				for (int n = 0; n < 3; n++)
				{
					if (!array[n])
					{
						helper.builder.DrawLine((Vector3)triangleMeshNode.GetVertex(n), (Vector3)triangleMeshNode.GetVertex((n + 1) % 3), color);
					}
				}
			}
		}
	}

	protected override void SerializeExtraInfo(GraphSerializationContext ctx)
	{
		BinaryWriter writer = ctx.writer;
		if (tiles == null)
		{
			writer.Write(-1);
			return;
		}
		writer.Write(tileXCount);
		writer.Write(tileZCount);
		for (int i = 0; i < tileZCount; i++)
		{
			for (int j = 0; j < tileXCount; j++)
			{
				NavmeshTile navmeshTile = tiles[j + i * tileXCount];
				if (navmeshTile == null)
				{
					throw new Exception("NULL Tile");
				}
				writer.Write(navmeshTile.x);
				writer.Write(navmeshTile.z);
				if (navmeshTile.x == j && navmeshTile.z == i)
				{
					writer.Write(navmeshTile.w);
					writer.Write(navmeshTile.d);
					writer.Write(navmeshTile.tris.Length);
					for (int k = 0; k < navmeshTile.tris.Length; k++)
					{
						writer.Write(navmeshTile.tris[k]);
					}
					writer.Write(navmeshTile.verts.Length);
					for (int l = 0; l < navmeshTile.verts.Length; l++)
					{
						ctx.SerializeInt3(navmeshTile.verts[l]);
					}
					writer.Write(navmeshTile.vertsInGraphSpace.Length);
					for (int m = 0; m < navmeshTile.vertsInGraphSpace.Length; m++)
					{
						ctx.SerializeInt3(navmeshTile.vertsInGraphSpace[m]);
					}
					writer.Write(navmeshTile.nodes.Length);
					for (int n = 0; n < navmeshTile.nodes.Length; n++)
					{
						navmeshTile.nodes[n].SerializeNode(ctx);
					}
				}
			}
		}
	}

	protected override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		BinaryReader reader = ctx.reader;
		tileXCount = reader.ReadInt32();
		if (tileXCount < 0)
		{
			return;
		}
		tileZCount = reader.ReadInt32();
		transform = CalculateTransform();
		tiles = new NavmeshTile[tileXCount * tileZCount];
		TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);
		for (int i = 0; i < tileZCount; i++)
		{
			for (int j = 0; j < tileXCount; j++)
			{
				int num = j + i * tileXCount;
				int num2 = reader.ReadInt32();
				if (num2 < 0)
				{
					throw new Exception("Invalid tile coordinates (x < 0)");
				}
				int num3 = reader.ReadInt32();
				if (num3 < 0)
				{
					throw new Exception("Invalid tile coordinates (z < 0)");
				}
				if (num2 != j || num3 != i)
				{
					tiles[num] = tiles[num3 * tileXCount + num2];
					continue;
				}
				NavmeshTile[] array = tiles;
				int num4 = num;
				NavmeshTile obj = new NavmeshTile
				{
					x = num2,
					z = num3,
					w = reader.ReadInt32(),
					d = reader.ReadInt32(),
					bbTree = ObjectPool<BBTree>.Claim(),
					graph = this
				};
				NavmeshTile navmeshTile = obj;
				array[num4] = obj;
				NavmeshTile navmeshTile2 = navmeshTile;
				int num5 = reader.ReadInt32();
				if (num5 % 3 != 0)
				{
					throw new Exception("Corrupt data. Triangle indices count must be divisable by 3. Read " + num5);
				}
				navmeshTile2.tris = new int[num5];
				for (int k = 0; k < navmeshTile2.tris.Length; k++)
				{
					navmeshTile2.tris[k] = reader.ReadInt32();
				}
				navmeshTile2.verts = new Int3[reader.ReadInt32()];
				for (int l = 0; l < navmeshTile2.verts.Length; l++)
				{
					navmeshTile2.verts[l] = ctx.DeserializeInt3();
				}
				if (ctx.meta.version.Major >= 4)
				{
					navmeshTile2.vertsInGraphSpace = new Int3[reader.ReadInt32()];
					if (navmeshTile2.vertsInGraphSpace.Length != navmeshTile2.verts.Length)
					{
						throw new Exception("Corrupt data. Array lengths did not match");
					}
					for (int m = 0; m < navmeshTile2.verts.Length; m++)
					{
						navmeshTile2.vertsInGraphSpace[m] = ctx.DeserializeInt3();
					}
				}
				else
				{
					navmeshTile2.vertsInGraphSpace = new Int3[navmeshTile2.verts.Length];
					navmeshTile2.verts.CopyTo(navmeshTile2.vertsInGraphSpace, 0);
					transform.InverseTransform(navmeshTile2.vertsInGraphSpace);
				}
				int num6 = reader.ReadInt32();
				navmeshTile2.nodes = new TriangleMeshNode[num6];
				num <<= 12;
				for (int n = 0; n < navmeshTile2.nodes.Length; n++)
				{
					TriangleMeshNode triangleMeshNode = new TriangleMeshNode(active);
					navmeshTile2.nodes[n] = triangleMeshNode;
					triangleMeshNode.DeserializeNode(ctx);
					triangleMeshNode.v0 = navmeshTile2.tris[n * 3] | num;
					triangleMeshNode.v1 = navmeshTile2.tris[n * 3 + 1] | num;
					triangleMeshNode.v2 = navmeshTile2.tris[n * 3 + 2] | num;
					triangleMeshNode.UpdatePositionFromVertices();
				}
				navmeshTile2.bbTree.RebuildFrom(navmeshTile2.nodes);
			}
		}
	}

	protected override void PostDeserialization(GraphSerializationContext ctx)
	{
		if (ctx.meta.version < AstarSerializer.V4_1_0 && tiles != null)
		{
			Dictionary<TriangleMeshNode, Connection[]> conns = tiles.SelectMany((NavmeshTile s) => s.nodes).ToDictionary((TriangleMeshNode n) => n, (TriangleMeshNode n) => n.connections ?? new Connection[0]);
			NavmeshTile[] array = tiles;
			for (int i = 0; i < array.Length; i++)
			{
				CreateNodeConnections(array[i].nodes);
			}
			array = tiles;
			foreach (NavmeshTile tile in array)
			{
				ConnectTileWithNeighbours(tile);
			}
			GetNodes(delegate(GraphNode node)
			{
				TriangleMeshNode triNode = node as TriangleMeshNode;
				foreach (Connection item in conns[triNode].Where((Connection conn) => !triNode.ContainsConnection(conn.node)).ToList())
				{
					triNode.AddConnection(item.node, item.cost, item.shapeEdge);
				}
			});
		}
		transform = CalculateTransform();
	}
}
