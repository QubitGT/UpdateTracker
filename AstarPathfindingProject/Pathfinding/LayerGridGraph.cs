using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Preserve]
public class LayerGridGraph : GridGraph, IUpdatableGraph
{
	protected struct HeightSample
	{
		public Vector3 position;

		public RaycastHit hit;

		public float height;

		public bool walkable;
	}

	private class HitComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit a, RaycastHit b)
		{
			return a.distance.CompareTo(b.distance);
		}
	}

	[JsonMember]
	internal int layerCount;

	[JsonMember]
	public float mergeSpanRange = 0.5f;

	[JsonMember]
	public float characterHeight = 0.4f;

	internal int lastScannedWidth;

	internal int lastScannedDepth;

	private static readonly HitComparer comparer = new HitComparer();

	private static HeightSample[] heightSampleBuffer = new HeightSample[4];

	public override bool uniformWidthDepthGrid => false;

	public override int LayerCount => layerCount;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveGridGraphFromStatic();
	}

	private void RemoveGridGraphFromStatic()
	{
		LevelGridNode.SetGridGraph(active.data.GetGraphIndex(this), null);
	}

	public override int CountNodes()
	{
		if (nodes == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes == null)
		{
			return;
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] != null)
			{
				action(nodes[i]);
			}
		}
	}

	protected override List<GraphNode> GetNodesInRegion(Bounds b, GraphUpdateShape shape)
	{
		IntRect rectFromBounds = GetRectFromBounds(b);
		if (nodes == null || !rectFromBounds.IsValid() || nodes.Length != width * depth * layerCount)
		{
			return ListPool<GraphNode>.Claim();
		}
		List<GraphNode> list = ListPool<GraphNode>.Claim(rectFromBounds.Width * rectFromBounds.Height * layerCount);
		for (int i = 0; i < layerCount; i++)
		{
			int num = i * width * depth;
			for (int j = rectFromBounds.xmin; j <= rectFromBounds.xmax; j++)
			{
				for (int k = rectFromBounds.ymin; k <= rectFromBounds.ymax; k++)
				{
					int num2 = num + k * width + j;
					GraphNode graphNode = nodes[num2];
					if (graphNode != null && b.Contains((Vector3)graphNode.position) && (shape == null || shape.Contains((Vector3)graphNode.position)))
					{
						list.Add(graphNode);
					}
				}
			}
		}
		return list;
	}

	public override List<GraphNode> GetNodesInRegion(IntRect rect)
	{
		List<GraphNode> list = ListPool<GraphNode>.Claim();
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth * layerCount)
		{
			return list;
		}
		for (int i = 0; i < layerCount; i++)
		{
			int num = i * base.Width * base.Depth;
			for (int j = rect.ymin; j <= rect.ymax; j++)
			{
				int num2 = num + j * base.Width;
				for (int k = rect.xmin; k <= rect.xmax; k++)
				{
					GridNodeBase gridNodeBase = nodes[num2 + k];
					if (gridNodeBase != null)
					{
						list.Add(gridNodeBase);
					}
				}
			}
		}
		return list;
	}

	public override int GetNodesInRegion(IntRect rect, GridNodeBase[] buffer)
	{
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth * layerCount)
		{
			return 0;
		}
		int num = 0;
		try
		{
			for (int i = 0; i < layerCount; i++)
			{
				int num2 = i * base.Width * base.Depth;
				for (int j = rect.ymin; j <= rect.ymax; j++)
				{
					int num3 = num2 + j * base.Width;
					for (int k = rect.xmin; k <= rect.xmax; k++)
					{
						GridNodeBase gridNodeBase = nodes[num3 + k];
						if (gridNodeBase != null)
						{
							buffer[num] = gridNodeBase;
							num++;
						}
					}
				}
			}
			return num;
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException("Buffer is too small");
		}
	}

	public override GridNodeBase GetNode(int x, int z)
	{
		if (x < 0 || z < 0 || x >= width || z >= depth)
		{
			return null;
		}
		return nodes[x + z * width];
	}

	public GridNodeBase GetNode(int x, int z, int layer)
	{
		if (x < 0 || z < 0 || x >= width || z >= depth || layer < 0 || layer >= layerCount)
		{
			return null;
		}
		return nodes[x + z * width + layer * width * depth];
	}

	void IUpdatableGraph.UpdateArea(GraphUpdateObject o)
	{
		if (nodes == null || nodes.Length != width * depth * layerCount)
		{
			Debug.LogWarning("The Grid Graph is not scanned, cannot update area ");
			return;
		}
		CalculateAffectedRegions(o, out var originalRect, out var affectRect, out var physicsRect, out var willChangeWalkability, out var erosion);
		bool flag = o is LayerGridGraphUpdate && ((LayerGridGraphUpdate)o).recalculateNodes;
		bool flag2 = ((o is LayerGridGraphUpdate) ? ((LayerGridGraphUpdate)o).preserveExistingNodes : (!o.resetPenaltyOnPhysics));
		if (o.trackChangedNodes && flag)
		{
			Debug.LogError("Cannot track changed nodes when creating or deleting nodes.\nWill not update LayerGridGraph");
			return;
		}
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect = IntRect.Intersection(affectRect, b);
		if (!flag)
		{
			for (int i = intRect.xmin; i <= intRect.xmax; i++)
			{
				for (int j = intRect.ymin; j <= intRect.ymax; j++)
				{
					for (int k = 0; k < layerCount; k++)
					{
						o.WillUpdateNode(nodes[k * width * depth + j * width + i]);
					}
				}
			}
		}
		if (o.updatePhysics && !o.modifyWalkability)
		{
			collision.Initialize(base.transform, nodeSize);
			intRect = IntRect.Intersection(physicsRect, b);
			for (int l = intRect.xmin; l <= intRect.xmax; l++)
			{
				for (int m = intRect.ymin; m <= intRect.ymax; m++)
				{
					RecalculateCell(l, m, !flag2, resetTags: false);
				}
			}
			for (int n = intRect.xmin; n <= intRect.xmax; n++)
			{
				for (int num = intRect.ymin; num <= intRect.ymax; num++)
				{
					CalculateConnections(n, num);
				}
			}
		}
		intRect = IntRect.Intersection(originalRect, b);
		for (int num2 = intRect.xmin; num2 <= intRect.xmax; num2++)
		{
			for (int num3 = intRect.ymin; num3 <= intRect.ymax; num3++)
			{
				for (int num4 = 0; num4 < layerCount; num4++)
				{
					int num5 = num4 * width * depth + num3 * width + num2;
					GridNodeBase gridNodeBase = nodes[num5];
					if (gridNodeBase == null)
					{
						continue;
					}
					if (willChangeWalkability)
					{
						gridNodeBase.Walkable = gridNodeBase.WalkableErosion;
						if (o.bounds.Contains((Vector3)gridNodeBase.position))
						{
							o.Apply(gridNodeBase);
						}
						gridNodeBase.WalkableErosion = gridNodeBase.Walkable;
					}
					else if (o.bounds.Contains((Vector3)gridNodeBase.position))
					{
						o.Apply(gridNodeBase);
					}
				}
			}
		}
		if (willChangeWalkability && erosion == 0)
		{
			intRect = IntRect.Intersection(affectRect, b);
			for (int num6 = intRect.xmin; num6 <= intRect.xmax; num6++)
			{
				for (int num7 = intRect.ymin; num7 <= intRect.ymax; num7++)
				{
					CalculateConnections(num6, num7);
				}
			}
		}
		else
		{
			if (!willChangeWalkability || erosion <= 0)
			{
				return;
			}
			IntRect a = IntRect.Union(originalRect, physicsRect).Expand(erosion);
			IntRect a2 = a.Expand(erosion);
			a = IntRect.Intersection(a, b);
			a2 = IntRect.Intersection(a2, b);
			for (int num8 = a2.xmin; num8 <= a2.xmax; num8++)
			{
				for (int num9 = a2.ymin; num9 <= a2.ymax; num9++)
				{
					for (int num10 = 0; num10 < layerCount; num10++)
					{
						int num11 = num10 * width * depth + num9 * width + num8;
						GridNodeBase gridNodeBase2 = nodes[num11];
						if (gridNodeBase2 != null)
						{
							bool walkable = gridNodeBase2.Walkable;
							gridNodeBase2.Walkable = gridNodeBase2.WalkableErosion;
							if (!a.Contains(num8, num9))
							{
								gridNodeBase2.TmpWalkable = walkable;
							}
						}
					}
				}
			}
			for (int num12 = a2.xmin; num12 <= a2.xmax; num12++)
			{
				for (int num13 = a2.ymin; num13 <= a2.ymax; num13++)
				{
					CalculateConnections(num12, num13);
				}
			}
			ErodeWalkableArea(a2.xmin, a2.ymin, a2.xmax + 1, a2.ymax + 1);
			for (int num14 = a2.xmin; num14 <= a2.xmax; num14++)
			{
				for (int num15 = a2.ymin; num15 <= a2.ymax; num15++)
				{
					if (a.Contains(num14, num15))
					{
						continue;
					}
					for (int num16 = 0; num16 < layerCount; num16++)
					{
						int num17 = num16 * width * depth + num15 * width + num14;
						GridNodeBase gridNodeBase3 = nodes[num17];
						if (gridNodeBase3 != null)
						{
							gridNodeBase3.Walkable = gridNodeBase3.TmpWalkable;
						}
					}
				}
			}
			for (int num18 = a2.xmin; num18 <= a2.xmax; num18++)
			{
				for (int num19 = a2.ymin; num19 <= a2.ymax; num19++)
				{
					CalculateConnections(num18, num19);
				}
			}
		}
	}

	protected override IEnumerable<Progress> ScanInternal()
	{
		if (nodeSize <= 0f)
		{
			yield break;
		}
		UpdateTransform();
		if (width > 1024 || depth > 1024)
		{
			Debug.LogError("One of the grid's sides is longer than 1024 nodes");
			yield break;
		}
		lastScannedWidth = width;
		lastScannedDepth = depth;
		SetUpOffsetsAndCosts();
		LevelGridNode.SetGridGraph((int)graphIndex, this);
		maxClimb = Mathf.Clamp(maxClimb, 0f, characterHeight);
		collision = collision ?? new GraphCollision();
		collision.Initialize(base.transform, nodeSize);
		int progressCounter = 0;
		layerCount = 1;
		LayerGridGraph layerGridGraph = this;
		GridNodeBase[] array = new LevelGridNode[width * depth * layerCount];
		layerGridGraph.nodes = array;
		for (int z2 = 0; z2 < depth; z2++)
		{
			if (progressCounter >= 1000)
			{
				progressCounter = 0;
				yield return new Progress(Mathf.Lerp(0f, 0.8f, (float)z2 / (float)depth), "Creating nodes");
			}
			progressCounter += width;
			for (int i = 0; i < width; i++)
			{
				RecalculateCell(i, z2);
			}
		}
		for (int z2 = 0; z2 < depth; z2++)
		{
			if (progressCounter >= 1000)
			{
				progressCounter = 0;
				yield return new Progress(Mathf.Lerp(0.8f, 0.9f, (float)z2 / (float)depth), "Calculating connections");
			}
			progressCounter += width;
			for (int j = 0; j < width; j++)
			{
				CalculateConnections(j, z2);
			}
		}
		yield return new Progress(0.95f, "Calculating Erosion");
		for (int k = 0; k < nodes.Length; k++)
		{
			if (nodes[k] is LevelGridNode levelGridNode && !levelGridNode.HasAnyGridConnections())
			{
				levelGridNode.Walkable = false;
				levelGridNode.WalkableErosion = levelGridNode.Walkable;
			}
		}
		ErodeWalkableArea();
	}

	protected static HeightSample[] SampleHeights(GraphCollision collision, float mergeSpanRange, Vector3 position, out int numHits)
	{
		int numHits2;
		RaycastHit[] array = collision.CheckHeightAll(position, out numHits2);
		Array.Sort(array, 0, numHits2, comparer);
		if (numHits2 > heightSampleBuffer.Length)
		{
			heightSampleBuffer = new HeightSample[Mathf.Max(heightSampleBuffer.Length * 2, numHits2)];
		}
		HeightSample[] array2 = heightSampleBuffer;
		if (numHits2 == 0)
		{
			array2[0] = new HeightSample
			{
				position = position,
				height = float.PositiveInfinity,
				walkable = (!collision.unwalkableWhenNoGround && collision.Check(position))
			};
			numHits = 1;
			return array2;
		}
		int num = 0;
		for (int num2 = numHits2 - 1; num2 >= 0; num2--)
		{
			if (num2 > 0 && array[num2].distance - array[num2 - 1].distance <= mergeSpanRange)
			{
				num2--;
			}
			array2[num] = new HeightSample
			{
				position = array[num2].point,
				hit = array[num2],
				walkable = collision.Check(array[num2].point),
				height = ((num2 > 0) ? (array[num2].distance - array[num2 - 1].distance) : float.PositiveInfinity)
			};
			num++;
		}
		numHits = num;
		return array2;
	}

	public override void RecalculateCell(int x, int z, bool resetPenalties = true, bool resetTags = true)
	{
		float num = Mathf.Cos(maxSlope * (MathF.PI / 180f));
		int numHits;
		HeightSample[] array = SampleHeights(collision, mergeSpanRange, base.transform.Transform(new Vector3((float)x + 0.5f, 0f, (float)z + 0.5f)), out numHits);
		if (numHits > layerCount)
		{
			if (numHits > 255)
			{
				Debug.LogError("Too many layers, a maximum of " + 255 + " are allowed (required " + numHits + ")");
				return;
			}
			AddLayers(numHits - layerCount);
		}
		int i;
		for (i = 0; i < numHits; i++)
		{
			HeightSample heightSample = array[i];
			int num2 = z * width + x + width * depth * i;
			LevelGridNode levelGridNode = nodes[num2] as LevelGridNode;
			bool num3 = levelGridNode == null;
			if (num3)
			{
				if (nodes[num2] != null)
				{
					nodes[num2].Destroy();
				}
				levelGridNode = new LevelGridNode(active);
				nodes[num2] = levelGridNode;
				levelGridNode.NodeInGridIndex = z * width + x;
				levelGridNode.LayerCoordinateInGrid = i;
				levelGridNode.GraphIndex = graphIndex;
			}
			levelGridNode.position = (Int3)heightSample.position;
			levelGridNode.Walkable = heightSample.walkable;
			levelGridNode.WalkableErosion = levelGridNode.Walkable;
			if (num3 || resetPenalties)
			{
				levelGridNode.Penalty = initialPenalty;
				if (penaltyPosition)
				{
					levelGridNode.Penalty += (uint)Mathf.RoundToInt(((float)levelGridNode.position.y - penaltyPositionOffset) * penaltyPositionFactor);
				}
			}
			if (num3 || resetTags)
			{
				levelGridNode.Tag = 0u;
			}
			if (heightSample.hit.normal != Vector3.zero && (penaltyAngle || num > 0.0001f))
			{
				float num4 = Vector3.Dot(heightSample.hit.normal.normalized, collision.up);
				if (resetTags && penaltyAngle)
				{
					levelGridNode.Penalty += (uint)Mathf.RoundToInt((1f - num4) * penaltyAngleFactor);
				}
				if (num4 < num)
				{
					levelGridNode.Walkable = false;
				}
			}
			if (heightSample.height < characterHeight)
			{
				levelGridNode.Walkable = false;
			}
			levelGridNode.WalkableErosion = levelGridNode.Walkable;
		}
		for (; i < layerCount; i++)
		{
			int num5 = z * width + x + width * depth * i;
			if (nodes[num5] != null)
			{
				nodes[num5].Destroy();
			}
			nodes[num5] = null;
		}
	}

	private void AddLayers(int count)
	{
		int num = layerCount + count;
		if (num > 255)
		{
			Debug.LogError("Too many layers, a maximum of " + 255 + " are allowed (required " + num + ")");
		}
		else
		{
			GridNodeBase[] array = nodes;
			nodes = new GridNodeBase[width * depth * num];
			array.CopyTo(nodes, 0);
			layerCount = num;
		}
	}

	protected override bool ErosionAnyFalseConnections(GraphNode baseNode)
	{
		LevelGridNode levelGridNode = baseNode as LevelGridNode;
		if (neighbours == NumNeighbours.Six)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!levelGridNode.HasConnectionInDirection(GridGraph.hexagonNeighbourIndices[i]))
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				if (!levelGridNode.HasConnectionInDirection(j))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void CalculateConnections(GridNodeBase baseNode)
	{
		LevelGridNode levelGridNode = baseNode as LevelGridNode;
		CalculateConnections(levelGridNode.XCoordinateInGrid, levelGridNode.ZCoordinateInGrid, levelGridNode.LayerCoordinateInGrid);
	}

	[Obsolete("Use CalculateConnections(x,z,layerIndex) or CalculateConnections(node) instead")]
	public void CalculateConnections(int x, int z, int layerIndex, LevelGridNode node)
	{
		CalculateConnections(x, z, layerIndex);
	}

	public override void CalculateConnections(int x, int z)
	{
		for (int i = 0; i < layerCount; i++)
		{
			CalculateConnections(x, z, i);
		}
	}

	public void CalculateConnections(int x, int z, int layerIndex)
	{
		if (!(nodes[z * width + x + width * depth * layerIndex] is LevelGridNode levelGridNode))
		{
			return;
		}
		levelGridNode.ResetAllGridConnections();
		if (!levelGridNode.Walkable)
		{
			return;
		}
		Vector3 vector = (Vector3)levelGridNode.position;
		Vector3 rhs = base.transform.WorldUpAtGraphPosition(vector);
		float num = Vector3.Dot(vector, rhs);
		float num2 = ((layerIndex != layerCount - 1 && nodes[levelGridNode.NodeInGridIndex + width * depth * (layerIndex + 1)] != null) ? Math.Abs(num - Vector3.Dot((Vector3)nodes[levelGridNode.NodeInGridIndex + width * depth * (layerIndex + 1)].position, rhs)) : float.PositiveInfinity);
		for (int i = 0; i < 4; i++)
		{
			int num3 = x + neighbourXOffsets[i];
			int num4 = z + neighbourZOffsets[i];
			if (num3 < 0 || num4 < 0 || num3 >= width || num4 >= depth)
			{
				continue;
			}
			int num5 = num4 * width + num3;
			int value = 255;
			for (int j = 0; j < layerCount; j++)
			{
				GraphNode graphNode = nodes[num5 + width * depth * j];
				if (graphNode != null && graphNode.Walkable)
				{
					float num6 = Vector3.Dot((Vector3)graphNode.position, rhs);
					float num7 = ((j != layerCount - 1 && nodes[num5 + width * depth * (j + 1)] != null) ? Math.Abs(num6 - Vector3.Dot((Vector3)nodes[num5 + width * depth * (j + 1)].position, rhs)) : float.PositiveInfinity);
					float num8 = Mathf.Max(num6, num);
					if (Mathf.Min(num6 + num7, num + num2) - num8 >= characterHeight && Mathf.Abs(num6 - num) <= maxClimb)
					{
						value = j;
					}
				}
			}
			levelGridNode.SetConnectionValue(i, value);
		}
	}

	public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		if (nodes == null || depth * width * layerCount != nodes.Length)
		{
			return default(NNInfoInternal);
		}
		Vector3 vector = base.transform.InverseTransform(position);
		float x = vector.x;
		float z = vector.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		GridNodeBase nearestNode = GetNearestNode(position, num, num2, null);
		NNInfoInternal result = new NNInfoInternal(nearestNode);
		float y = base.transform.InverseTransform((Vector3)nearestNode.position).y;
		result.clampedPosition = base.transform.Transform(new Vector3(Mathf.Clamp(x, num, (float)num + 1f), y, Mathf.Clamp(z, num2, (float)num2 + 1f)));
		return result;
	}

	protected override GridNodeBase GetNearestFromGraphSpace(Vector3 positionGraphSpace)
	{
		if (nodes == null || depth * width * layerCount != nodes.Length)
		{
			return null;
		}
		float x = positionGraphSpace.x;
		float z = positionGraphSpace.z;
		int x2 = Mathf.Clamp((int)x, 0, width - 1);
		int z2 = Mathf.Clamp((int)z, 0, depth - 1);
		Vector3 position = base.transform.Transform(positionGraphSpace);
		return GetNearestNode(position, x2, z2, null);
	}

	private GridNodeBase GetNearestNode(Vector3 position, int x, int z, NNConstraint constraint)
	{
		int num = width * z + x;
		float num2 = float.PositiveInfinity;
		GridNodeBase result = null;
		for (int i = 0; i < layerCount; i++)
		{
			GridNodeBase gridNodeBase = nodes[num + width * depth * i];
			if (gridNodeBase != null)
			{
				float sqrMagnitude = ((Vector3)gridNodeBase.position - position).sqrMagnitude;
				if (sqrMagnitude < num2 && (constraint == null || constraint.Suitable(gridNodeBase)))
				{
					num2 = sqrMagnitude;
					result = gridNodeBase;
				}
			}
		}
		return result;
	}

	[Obsolete("Use node.HasConnectionInDirection instead")]
	public static bool CheckConnection(LevelGridNode node, int dir)
	{
		return node.HasConnectionInDirection(dir);
	}

	protected override void SerializeExtraInfo(GraphSerializationContext ctx)
	{
		if (nodes == null)
		{
			ctx.writer.Write(-1);
			return;
		}
		ctx.writer.Write(nodes.Length);
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] == null)
			{
				ctx.writer.Write(-1);
				continue;
			}
			ctx.writer.Write(0);
			nodes[i].SerializeNode(ctx);
		}
	}

	protected override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		if (num == -1)
		{
			nodes = null;
			return;
		}
		GridNodeBase[] array = new LevelGridNode[num];
		nodes = array;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (ctx.reader.ReadInt32() != -1)
			{
				nodes[i] = new LevelGridNode(active);
				nodes[i].DeserializeNode(ctx);
			}
			else
			{
				nodes[i] = null;
			}
		}
	}

	protected override void PostDeserialization(GraphSerializationContext ctx)
	{
		UpdateTransform();
		lastScannedWidth = width;
		lastScannedDepth = depth;
		SetUpOffsetsAndCosts();
		LevelGridNode.SetGridGraph((int)graphIndex, this);
		if (nodes == null || nodes.Length == 0)
		{
			return;
		}
		if (width * depth * layerCount != nodes.Length)
		{
			Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
			GridNodeBase[] array = new LevelGridNode[0];
			nodes = array;
			return;
		}
		for (int i = 0; i < layerCount; i++)
		{
			for (int j = 0; j < depth; j++)
			{
				for (int k = 0; k < width; k++)
				{
					if (nodes[j * width + k + width * depth * i] is LevelGridNode levelGridNode)
					{
						levelGridNode.NodeInGridIndex = j * width + k;
						levelGridNode.LayerCoordinateInGrid = i;
					}
				}
			}
		}
	}
}
