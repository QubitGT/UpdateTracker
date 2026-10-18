using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[JsonOptIn]
[Preserve]
public class GridGraph : NavGraph, IUpdatableGraph, ITransformedGraph, IRaycastableGraph
{
	public class TextureData
	{
		public enum ChannelUse
		{
			None,
			Penalty,
			Position,
			WalkablePenalty
		}

		public bool enabled;

		public Texture2D source;

		public float[] factors = new float[3];

		public ChannelUse[] channels = new ChannelUse[3];

		private Color32[] data;

		public void Initialize()
		{
			if (!enabled || !(source != null))
			{
				return;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				if (channels[i] != 0)
				{
					try
					{
						data = source.GetPixels32();
						break;
					}
					catch (UnityException ex)
					{
						Debug.LogWarning(ex.ToString());
						data = null;
						break;
					}
				}
			}
		}

		public void Apply(GridNodeBase node, int x, int z)
		{
			if (enabled && data != null && x < source.width && z < source.height)
			{
				Color32 color = data[z * source.width + x];
				if (channels[0] != 0)
				{
					ApplyChannel(node, x, z, color.r, channels[0], factors[0]);
				}
				if (channels[1] != 0)
				{
					ApplyChannel(node, x, z, color.g, channels[1], factors[1]);
				}
				if (channels[2] != 0)
				{
					ApplyChannel(node, x, z, color.b, channels[2], factors[2]);
				}
				node.WalkableErosion = node.Walkable;
			}
		}

		private void ApplyChannel(GridNodeBase node, int x, int z, int value, ChannelUse channelUse, float factor)
		{
			switch (channelUse)
			{
			case ChannelUse.Penalty:
				node.Penalty += (uint)Mathf.RoundToInt((float)value * factor);
				break;
			case ChannelUse.Position:
				node.position = GridNode.GetGridGraph(node.GraphIndex).GraphPointToWorld(x, z, value);
				break;
			case ChannelUse.WalkablePenalty:
				if (value == 0)
				{
					node.Walkable = false;
				}
				else
				{
					node.Penalty += (uint)Mathf.RoundToInt((float)(value - 1) * factor);
				}
				break;
			}
		}
	}

	[JsonMember]
	public InspectorGridMode inspectorGridMode;

	[JsonMember]
	public InspectorGridHexagonNodeSize inspectorHexagonSizeMode;

	public int width;

	public int depth;

	[JsonMember]
	public float aspectRatio = 1f;

	[JsonMember]
	public float isometricAngle;

	public static readonly float StandardIsometricAngle = 90f - Mathf.Atan(1f / Mathf.Sqrt(2f)) * 57.29578f;

	public static readonly float StandardDimetricAngle = Mathf.Acos(0.5f) * 57.29578f;

	[JsonMember]
	public bool uniformEdgeCosts;

	[JsonMember]
	public Vector3 rotation;

	[JsonMember]
	public Vector3 center;

	[JsonMember]
	public Vector2 unclampedSize;

	[JsonMember]
	public float nodeSize = 1f;

	[JsonMember]
	public GraphCollision collision;

	[JsonMember]
	public float maxClimb = 0.4f;

	[JsonMember]
	public float maxSlope = 90f;

	[JsonMember]
	public int erodeIterations;

	[JsonMember]
	public bool erosionUseTags;

	[JsonMember]
	public int erosionFirstTag = 1;

	[JsonMember]
	public NumNeighbours neighbours = NumNeighbours.Eight;

	[JsonMember]
	public bool cutCorners = true;

	[JsonMember]
	public float penaltyPositionOffset;

	[JsonMember]
	public bool penaltyPosition;

	[JsonMember]
	public float penaltyPositionFactor = 1f;

	[JsonMember]
	public bool penaltyAngle;

	[JsonMember]
	public float penaltyAngleFactor = 100f;

	[JsonMember]
	public float penaltyAnglePower = 1f;

	[JsonMember]
	public bool useJumpPointSearch;

	[JsonMember]
	public bool showMeshOutline = true;

	[JsonMember]
	public bool showNodeConnections;

	[JsonMember]
	public bool showMeshSurface = true;

	[JsonMember]
	public TextureData textureData = new TextureData();

	[NonSerialized]
	public readonly int[] neighbourOffsets = new int[8];

	[NonSerialized]
	public readonly uint[] neighbourCosts = new uint[8];

	[NonSerialized]
	public readonly int[] neighbourXOffsets = new int[8];

	[NonSerialized]
	public readonly int[] neighbourZOffsets = new int[8];

	internal static readonly int[] hexagonNeighbourIndices = new int[6] { 0, 1, 5, 2, 3, 7 };

	public const int getNearestForceOverlap = 2;

	public GridNodeBase[] nodes;

	private const int FixedPrecisionScale = 1024;

	public virtual bool uniformWidthDepthGrid => true;

	public virtual int LayerCount => 1;

	protected bool useRaycastNormal => Math.Abs(90f - maxSlope) > float.Epsilon;

	public Vector2 size { get; protected set; }

	public GraphTransform transform { get; private set; }

	public bool is2D
	{
		get
		{
			return Quaternion.Euler(rotation) * Vector3.up == -Vector3.forward;
		}
		set
		{
			if (value != is2D)
			{
				rotation = (value ? new Vector3(rotation.y - 90f, 270f, 90f) : new Vector3(0f, rotation.x + 90f, 0f));
			}
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveGridGraphFromStatic();
	}

	protected override void DestroyAllNodes()
	{
		GetNodes(delegate(GraphNode node)
		{
			(node as GridNodeBase).ClearCustomConnections(alsoReverse: true);
			node.ClearConnections(alsoReverse: false);
			node.Destroy();
		});
	}

	private void RemoveGridGraphFromStatic()
	{
		GridNode.ClearGridGraph(active.data.GetGraphIndex(this), this);
	}

	public override int CountNodes()
	{
		if (nodes == null)
		{
			return 0;
		}
		return nodes.Length;
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				action(nodes[i]);
			}
		}
	}

	public GridGraph()
	{
		unclampedSize = new Vector2(10f, 10f);
		nodeSize = 1f;
		collision = new GraphCollision();
		transform = new GraphTransform(Matrix4x4.identity);
	}

	public override void RelocateNodes(Matrix4x4 deltaMatrix)
	{
		throw new Exception("This method cannot be used for Grid Graphs. Please use the other overload of RelocateNodes instead");
	}

	public void RelocateNodes(Vector3 center, Quaternion rotation, float nodeSize, float aspectRatio = 1f, float isometricAngle = 0f)
	{
		GraphTransform previousTransform = transform;
		this.center = center;
		this.rotation = rotation.eulerAngles;
		this.aspectRatio = aspectRatio;
		this.isometricAngle = isometricAngle;
		SetDimensions(width, depth, nodeSize);
		GetNodes(delegate(GraphNode node)
		{
			GridNodeBase gridNodeBase = node as GridNodeBase;
			float y = previousTransform.InverseTransform((Vector3)node.position).y;
			node.position = GraphPointToWorld(gridNodeBase.XCoordinateInGrid, gridNodeBase.ZCoordinateInGrid, y);
		});
	}

	public Int3 GraphPointToWorld(int x, int z, float height)
	{
		return (Int3)transform.Transform(new Vector3((float)x + 0.5f, height, (float)z + 0.5f));
	}

	public static float ConvertHexagonSizeToNodeSize(InspectorGridHexagonNodeSize mode, float value)
	{
		switch (mode)
		{
		case InspectorGridHexagonNodeSize.Diameter:
			value *= 1.5f / (float)Math.Sqrt(2.0);
			break;
		case InspectorGridHexagonNodeSize.Width:
			value *= (float)Math.Sqrt(1.5);
			break;
		}
		return value;
	}

	public static float ConvertNodeSizeToHexagonSize(InspectorGridHexagonNodeSize mode, float value)
	{
		switch (mode)
		{
		case InspectorGridHexagonNodeSize.Diameter:
			value *= (float)Math.Sqrt(2.0) / 1.5f;
			break;
		case InspectorGridHexagonNodeSize.Width:
			value *= (float)Math.Sqrt(0.6666666865348816);
			break;
		}
		return value;
	}

	public uint GetConnectionCost(int dir)
	{
		return neighbourCosts[dir];
	}

	[Obsolete("Use GridNode.HasConnectionInDirection instead")]
	public GridNode GetNodeConnection(GridNode node, int dir)
	{
		if (!node.HasConnectionInDirection(dir))
		{
			return null;
		}
		if (!node.EdgeNode)
		{
			return nodes[node.NodeInGridIndex + neighbourOffsets[dir]] as GridNode;
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return GetNodeConnection(nodeInGridIndex, x, num, dir);
	}

	[Obsolete("Use GridNode.HasConnectionInDirection instead")]
	public bool HasNodeConnection(GridNode node, int dir)
	{
		if (!node.HasConnectionInDirection(dir))
		{
			return false;
		}
		if (!node.EdgeNode)
		{
			return true;
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return HasNodeConnection(nodeInGridIndex, x, num, dir);
	}

	[Obsolete("Use GridNode.SetConnectionInternal instead")]
	public void SetNodeConnection(GridNode node, int dir, bool value)
	{
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		SetNodeConnection(nodeInGridIndex, x, num, dir, value);
	}

	[Obsolete("Use GridNode.HasConnectionInDirection instead")]
	private GridNode GetNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].HasConnectionInDirection(dir))
		{
			return null;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return null;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return null;
		}
		int num3 = index + neighbourOffsets[dir];
		return nodes[num3] as GridNode;
	}

	[Obsolete("Use GridNode.SetConnectionInternal instead")]
	public void SetNodeConnection(int index, int x, int z, int dir, bool value)
	{
		(nodes[index] as GridNode).SetConnectionInternal(dir, value);
	}

	[Obsolete("Use GridNode.HasConnectionInDirection instead")]
	public bool HasNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].HasConnectionInDirection(dir))
		{
			return false;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return false;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return false;
		}
		return true;
	}

	public void SetGridShape(InspectorGridMode shape)
	{
		switch (shape)
		{
		case InspectorGridMode.Grid:
			isometricAngle = 0f;
			aspectRatio = 1f;
			uniformEdgeCosts = false;
			if (neighbours == NumNeighbours.Six)
			{
				neighbours = NumNeighbours.Eight;
			}
			break;
		case InspectorGridMode.Hexagonal:
			isometricAngle = StandardIsometricAngle;
			aspectRatio = 1f;
			uniformEdgeCosts = true;
			neighbours = NumNeighbours.Six;
			break;
		case InspectorGridMode.IsometricGrid:
			uniformEdgeCosts = false;
			if (neighbours == NumNeighbours.Six)
			{
				neighbours = NumNeighbours.Eight;
			}
			isometricAngle = StandardIsometricAngle;
			break;
		}
		inspectorGridMode = shape;
	}

	public void SetDimensions(int width, int depth, float nodeSize)
	{
		unclampedSize = new Vector2(width, depth) * nodeSize;
		this.nodeSize = nodeSize;
		UpdateTransform();
	}

	[Obsolete("Use SetDimensions instead")]
	public void UpdateSizeFromWidthDepth()
	{
		SetDimensions(width, depth, nodeSize);
	}

	[Obsolete("This method has been renamed to UpdateTransform")]
	public void GenerateMatrix()
	{
		UpdateTransform();
	}

	public void UpdateTransform()
	{
		CalculateDimensions(out width, out depth, out nodeSize);
		transform = CalculateTransform();
	}

	public GraphTransform CalculateTransform()
	{
		CalculateDimensions(out var num, out var num2, out var num3);
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 45f, 0f), Vector3.one);
		matrix4x = Matrix4x4.Scale(new Vector3(Mathf.Cos(MathF.PI / 180f * isometricAngle), 1f, 1f)) * matrix4x;
		matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, -45f, 0f), Vector3.one) * matrix4x;
		return new GraphTransform(Matrix4x4.TRS((Matrix4x4.TRS(center, Quaternion.Euler(rotation), new Vector3(aspectRatio, 1f, 1f)) * matrix4x).MultiplyPoint3x4(-new Vector3((float)num * num3, 0f, (float)num2 * num3) * 0.5f), Quaternion.Euler(rotation), new Vector3(num3 * aspectRatio, 1f, num3)) * matrix4x);
	}

	private void CalculateDimensions(out int width, out int depth, out float nodeSize)
	{
		Vector2 vector = unclampedSize;
		vector.x *= Mathf.Sign(vector.x);
		vector.y *= Mathf.Sign(vector.y);
		nodeSize = Mathf.Max(this.nodeSize, vector.x / 1024f);
		nodeSize = Mathf.Max(this.nodeSize, vector.y / 1024f);
		vector.x = ((vector.x < nodeSize) ? nodeSize : vector.x);
		vector.y = ((vector.y < nodeSize) ? nodeSize : vector.y);
		size = vector;
		width = Mathf.FloorToInt(size.x / nodeSize);
		depth = Mathf.FloorToInt(size.y / nodeSize);
		if (Mathf.Approximately(size.x / nodeSize, Mathf.CeilToInt(size.x / nodeSize)))
		{
			width = Mathf.CeilToInt(size.x / nodeSize);
		}
		if (Mathf.Approximately(size.y / nodeSize, Mathf.CeilToInt(size.y / nodeSize)))
		{
			depth = Mathf.CeilToInt(size.y / nodeSize);
		}
	}

	public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return default(NNInfoInternal);
		}
		position = transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		NNInfoInternal result = new NNInfoInternal(nodes[num2 * width + num]);
		float y = transform.InverseTransform((Vector3)nodes[num2 * width + num].position).y;
		result.clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, num, (float)num + 1f), y, Mathf.Clamp(z, num2, (float)num2 + 1f)));
		return result;
	}

	protected virtual GridNodeBase GetNearestFromGraphSpace(Vector3 positionGraphSpace)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return null;
		}
		float x = positionGraphSpace.x;
		float z = positionGraphSpace.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		return nodes[num2 * width + num];
	}

	public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
	{
		if (nodes == null || depth * width * LayerCount != nodes.Length)
		{
			return default(NNInfoInternal);
		}
		Vector3 vector = position;
		position = transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		GridNodeBase gridNodeBase = null;
		bool flag = constraint?.distanceXZ ?? false;
		float num3 = ((constraint == null || constraint.constrainDistance) ? AstarPath.active.maxNearestNodeDistance : float.PositiveInfinity);
		float num4 = num3 * num3;
		int layerCount = LayerCount;
		int num5 = width * depth;
		for (int i = 0; i < layerCount; i++)
		{
			GridNodeBase gridNodeBase2 = nodes[num2 * width + num + num5 * i];
			if (gridNodeBase2 != null && (constraint == null || constraint.Suitable(gridNodeBase2)))
			{
				float num6 = (flag ? (nodeSize * nodeSize * (((float)num + 0.5f - x) * ((float)num + 0.5f - x) + ((float)num2 + 0.5f - z) * ((float)num2 + 0.5f - z))) : ((Vector3)gridNodeBase2.position - vector).sqrMagnitude);
				if (num6 <= num4)
				{
					num4 = num6;
					gridNodeBase = gridNodeBase2;
				}
			}
		}
		for (int j = 1; !(nodeSize * nodeSize * (float)j * (float)j > num4 * 2f); j++)
		{
			bool flag2 = false;
			int num7 = num + j;
			int num8 = num2;
			int num9 = -1;
			int num10 = 1;
			for (int k = 0; k < 4; k++)
			{
				for (int l = 0; l < j; l++)
				{
					if (num7 >= 0 && num8 >= 0 && num7 < width && num8 < depth)
					{
						flag2 = true;
						int num11 = num7 + num8 * width;
						for (int m = 0; m < layerCount; m++)
						{
							GridNodeBase gridNodeBase3 = nodes[num11 + num5 * m];
							if (gridNodeBase3 != null && (constraint == null || constraint.Suitable(gridNodeBase3)))
							{
								float num12 = (flag ? (nodeSize * nodeSize * (((float)num7 + 0.5f - x) * ((float)num7 + 0.5f - x) + ((float)num8 + 0.5f - z) * ((float)num8 + 0.5f - z))) : ((Vector3)gridNodeBase3.position - vector).sqrMagnitude);
								if (num12 <= num4)
								{
									num4 = num12;
									gridNodeBase = gridNodeBase3;
								}
							}
						}
					}
					num7 += num9;
					num8 += num10;
				}
				int num13 = -num10;
				int num14 = num9;
				num9 = num13;
				num10 = num14;
			}
			if (!flag2)
			{
				break;
			}
		}
		NNInfoInternal result = new NNInfoInternal(null);
		if (gridNodeBase != null)
		{
			int xCoordinateInGrid = gridNodeBase.XCoordinateInGrid;
			int zCoordinateInGrid = gridNodeBase.ZCoordinateInGrid;
			result.node = gridNodeBase;
			result.clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, xCoordinateInGrid, (float)xCoordinateInGrid + 1f), transform.InverseTransform((Vector3)gridNodeBase.position).y, Mathf.Clamp(z, zCoordinateInGrid, (float)zCoordinateInGrid + 1f)));
		}
		return result;
	}

	public virtual void SetUpOffsetsAndCosts()
	{
		neighbourOffsets[0] = -width;
		neighbourOffsets[1] = 1;
		neighbourOffsets[2] = width;
		neighbourOffsets[3] = -1;
		neighbourOffsets[4] = -width + 1;
		neighbourOffsets[5] = width + 1;
		neighbourOffsets[6] = width - 1;
		neighbourOffsets[7] = -width - 1;
		uint num = (uint)Mathf.RoundToInt(nodeSize * 1000f);
		uint num2 = (uniformEdgeCosts ? num : ((uint)Mathf.RoundToInt(nodeSize * Mathf.Sqrt(2f) * 1000f)));
		neighbourCosts[0] = num;
		neighbourCosts[1] = num;
		neighbourCosts[2] = num;
		neighbourCosts[3] = num;
		neighbourCosts[4] = num2;
		neighbourCosts[5] = num2;
		neighbourCosts[6] = num2;
		neighbourCosts[7] = num2;
		neighbourXOffsets[0] = 0;
		neighbourXOffsets[1] = 1;
		neighbourXOffsets[2] = 0;
		neighbourXOffsets[3] = -1;
		neighbourXOffsets[4] = 1;
		neighbourXOffsets[5] = 1;
		neighbourXOffsets[6] = -1;
		neighbourXOffsets[7] = -1;
		neighbourZOffsets[0] = -1;
		neighbourZOffsets[1] = 0;
		neighbourZOffsets[2] = 1;
		neighbourZOffsets[3] = 0;
		neighbourZOffsets[4] = -1;
		neighbourZOffsets[5] = 1;
		neighbourZOffsets[6] = 1;
		neighbourZOffsets[7] = -1;
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
		if (useJumpPointSearch)
		{
			Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
		}
		SetUpOffsetsAndCosts();
		GridNode.SetGridGraph((int)graphIndex, this);
		yield return new Progress(0.05f, "Creating nodes");
		GridGraph gridGraph = this;
		GridNodeBase[] array = new GridNode[width * depth];
		gridGraph.nodes = array;
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = i * width + j;
				GridNodeBase obj = (nodes[num] = new GridNode(active));
				obj.GraphIndex = graphIndex;
				obj.NodeInGridIndex = num;
			}
		}
		if (collision == null)
		{
			collision = new GraphCollision();
		}
		collision.Initialize(transform, nodeSize);
		textureData.Initialize();
		int progressCounter2 = 0;
		for (int z2 = 0; z2 < depth; z2++)
		{
			if (progressCounter2 >= 1000)
			{
				progressCounter2 = 0;
				yield return new Progress(Mathf.Lerp(0.1f, 0.7f, (float)z2 / (float)depth), "Calculating positions");
			}
			progressCounter2 += width;
			for (int k = 0; k < width; k++)
			{
				RecalculateCell(k, z2);
				textureData.Apply(nodes[z2 * width + k] as GridNode, k, z2);
			}
		}
		progressCounter2 = 0;
		for (int z2 = 0; z2 < depth; z2++)
		{
			if (progressCounter2 >= 1000)
			{
				progressCounter2 = 0;
				yield return new Progress(Mathf.Lerp(0.7f, 0.9f, (float)z2 / (float)depth), "Calculating connections");
			}
			progressCounter2 += width;
			for (int l = 0; l < width; l++)
			{
				CalculateConnections(l, z2);
			}
		}
		yield return new Progress(0.95f, "Calculating erosion");
		ErodeWalkableArea();
	}

	[Obsolete("Use RecalculateCell instead which works both for grid graphs and layered grid graphs")]
	public virtual void UpdateNodePositionCollision(GridNode node, int x, int z, bool resetPenalty = true)
	{
		RecalculateCell(x, z, resetPenalty, resetTags: false);
	}

	public virtual void RecalculateCell(int x, int z, bool resetPenalties = true, bool resetTags = true)
	{
		GridNodeBase gridNodeBase = nodes[z * width + x];
		gridNodeBase.position = GraphPointToWorld(x, z, 0f);
		RaycastHit hit;
		bool walkable;
		Vector3 vector = collision.CheckHeight((Vector3)gridNodeBase.position, out hit, out walkable);
		gridNodeBase.position = (Int3)vector;
		if (resetPenalties)
		{
			gridNodeBase.Penalty = initialPenalty;
			if (penaltyPosition)
			{
				gridNodeBase.Penalty += (uint)Mathf.RoundToInt(((float)gridNodeBase.position.y - penaltyPositionOffset) * penaltyPositionFactor);
			}
		}
		if (resetTags)
		{
			gridNodeBase.Tag = 0u;
		}
		if (walkable && useRaycastNormal && collision.heightCheck && hit.normal != Vector3.zero)
		{
			float num = Vector3.Dot(hit.normal.normalized, collision.up);
			if (penaltyAngle && resetPenalties)
			{
				gridNodeBase.Penalty += (uint)Mathf.RoundToInt((1f - Mathf.Pow(num, penaltyAnglePower)) * penaltyAngleFactor);
			}
			float num2 = Mathf.Cos(maxSlope * (MathF.PI / 180f));
			if (num < num2)
			{
				walkable = false;
			}
		}
		gridNodeBase.Walkable = walkable && collision.Check((Vector3)gridNodeBase.position);
		gridNodeBase.WalkableErosion = gridNodeBase.Walkable;
	}

	protected virtual bool ErosionAnyFalseConnections(GraphNode baseNode)
	{
		GridNode gridNode = baseNode as GridNode;
		if (neighbours == NumNeighbours.Six)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!gridNode.HasConnectionInDirection(hexagonNeighbourIndices[i]))
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				if (!gridNode.HasConnectionInDirection(j))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ErodeNode(GraphNode node)
	{
		if (node.Walkable && ErosionAnyFalseConnections(node))
		{
			node.Walkable = false;
		}
	}

	private void ErodeNodeWithTagsInit(GraphNode node)
	{
		if (node.Walkable && ErosionAnyFalseConnections(node))
		{
			node.Tag = (uint)erosionFirstTag;
		}
		else
		{
			node.Tag = 0u;
		}
	}

	private void ErodeNodeWithTags(GraphNode node, int iteration)
	{
		GridNodeBase gridNodeBase = node as GridNodeBase;
		if (!gridNodeBase.Walkable || gridNodeBase.Tag < erosionFirstTag || gridNodeBase.Tag >= erosionFirstTag + iteration)
		{
			return;
		}
		if (neighbours == NumNeighbours.Six)
		{
			for (int i = 0; i < 6; i++)
			{
				GridNodeBase neighbourAlongDirection = gridNodeBase.GetNeighbourAlongDirection(hexagonNeighbourIndices[i]);
				if (neighbourAlongDirection != null)
				{
					uint tag = neighbourAlongDirection.Tag;
					if (tag > erosionFirstTag + iteration || tag < erosionFirstTag)
					{
						neighbourAlongDirection.Tag = (uint)(erosionFirstTag + iteration);
					}
				}
			}
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			GridNodeBase neighbourAlongDirection2 = gridNodeBase.GetNeighbourAlongDirection(j);
			if (neighbourAlongDirection2 != null)
			{
				uint tag2 = neighbourAlongDirection2.Tag;
				if (tag2 > erosionFirstTag + iteration || tag2 < erosionFirstTag)
				{
					neighbourAlongDirection2.Tag = (uint)(erosionFirstTag + iteration);
				}
			}
		}
	}

	public virtual void ErodeWalkableArea()
	{
		ErodeWalkableArea(0, 0, Width, Depth);
	}

	public void ErodeWalkableArea(int xmin, int zmin, int xmax, int zmax)
	{
		if (erosionUseTags)
		{
			if (erodeIterations + erosionFirstTag > 31)
			{
				Debug.LogError("Too few tags available for " + erodeIterations + " erode iterations and starting with tag " + erosionFirstTag + " (erodeIterations+erosionFirstTag > 31)", active);
				return;
			}
			if (erosionFirstTag <= 0)
			{
				Debug.LogError("First erosion tag must be greater or equal to 1", active);
				return;
			}
		}
		if (erodeIterations == 0)
		{
			return;
		}
		IntRect rect = new IntRect(xmin, zmin, xmax - 1, zmax - 1);
		List<GraphNode> list = GetNodesInRegion(rect);
		int count = list.Count;
		for (int i = 0; i < erodeIterations; i++)
		{
			if (erosionUseTags)
			{
				if (i == 0)
				{
					for (int j = 0; j < count; j++)
					{
						ErodeNodeWithTagsInit(list[j]);
					}
				}
				else
				{
					for (int k = 0; k < count; k++)
					{
						ErodeNodeWithTags(list[k], i);
					}
				}
			}
			else
			{
				for (int l = 0; l < count; l++)
				{
					ErodeNode(list[l]);
				}
				for (int m = 0; m < count; m++)
				{
					CalculateConnections(list[m] as GridNodeBase);
				}
			}
		}
		ListPool<GraphNode>.Release(ref list);
	}

	public virtual bool IsValidConnection(GridNodeBase node1, GridNodeBase node2)
	{
		if (!node1.Walkable || !node2.Walkable)
		{
			return false;
		}
		if (maxClimb <= 0f || collision.use2D)
		{
			return true;
		}
		if (transform.onlyTranslational)
		{
			return (float)Math.Abs(node1.position.y - node2.position.y) <= maxClimb * 1000f;
		}
		Vector3 vector = (Vector3)node1.position;
		Vector3 rhs = (Vector3)node2.position;
		Vector3 lhs = transform.WorldUpAtGraphPosition(vector);
		return Math.Abs(Vector3.Dot(lhs, vector) - Vector3.Dot(lhs, rhs)) <= maxClimb;
	}

	public void CalculateConnectionsForCellAndNeighbours(int x, int z)
	{
		CalculateConnections(x, z);
		for (int i = 0; i < 8; i++)
		{
			int num = x + neighbourXOffsets[i];
			int num2 = z + neighbourZOffsets[i];
			if ((num >= 0 && num2 >= 0) & (num < width) & (num2 < depth))
			{
				CalculateConnections(num, num2);
			}
		}
	}

	[Obsolete("Use the instance function instead")]
	public static void CalculateConnections(GridNode node)
	{
		(AstarData.GetGraph(node) as GridGraph).CalculateConnections((GridNodeBase)node);
	}

	public virtual void CalculateConnections(GridNodeBase node)
	{
		int nodeInGridIndex = node.NodeInGridIndex;
		int x = nodeInGridIndex % width;
		int z = nodeInGridIndex / width;
		CalculateConnections(x, z);
	}

	[Obsolete("Use CalculateConnections(x,z) or CalculateConnections(node) instead")]
	public virtual void CalculateConnections(int x, int z, GridNode node)
	{
		CalculateConnections(x, z);
	}

	public virtual void CalculateConnections(int x, int z)
	{
		GridNode gridNode = nodes[z * width + x] as GridNode;
		if (!gridNode.Walkable)
		{
			gridNode.ResetConnectionsInternal();
			return;
		}
		int nodeInGridIndex = gridNode.NodeInGridIndex;
		if (neighbours == NumNeighbours.Four || neighbours == NumNeighbours.Eight)
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				int num2 = x + neighbourXOffsets[i];
				int num3 = z + neighbourZOffsets[i];
				if ((num2 >= 0 && num3 >= 0) & (num2 < width) & (num3 < depth))
				{
					GridNodeBase node = nodes[nodeInGridIndex + neighbourOffsets[i]];
					if (IsValidConnection(gridNode, node))
					{
						num |= 1 << i;
					}
				}
			}
			int num4 = 0;
			if (neighbours == NumNeighbours.Eight)
			{
				if (cutCorners)
				{
					for (int j = 0; j < 4; j++)
					{
						if ((((num >> j) | (num >> j + 1) | (num >> j + 1 - 4)) & 1) == 0)
						{
							continue;
						}
						int num5 = j + 4;
						int num6 = x + neighbourXOffsets[num5];
						int num7 = z + neighbourZOffsets[num5];
						if ((num6 >= 0 && num7 >= 0) & (num6 < width) & (num7 < depth))
						{
							GridNodeBase node2 = nodes[nodeInGridIndex + neighbourOffsets[num5]];
							if (IsValidConnection(gridNode, node2))
							{
								num4 |= 1 << num5;
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < 4; k++)
					{
						if (((uint)(num >> k) & (true ? 1u : 0u)) != 0 && ((uint)((num >> k + 1) | (num >> k + 1 - 4)) & (true ? 1u : 0u)) != 0)
						{
							GridNodeBase node3 = nodes[nodeInGridIndex + neighbourOffsets[k + 4]];
							if (IsValidConnection(gridNode, node3))
							{
								num4 |= 1 << k + 4;
							}
						}
					}
				}
			}
			gridNode.SetAllConnectionInternal(num | num4);
			return;
		}
		gridNode.ResetConnectionsInternal();
		for (int l = 0; l < hexagonNeighbourIndices.Length; l++)
		{
			int num8 = hexagonNeighbourIndices[l];
			int num9 = x + neighbourXOffsets[num8];
			int num10 = z + neighbourZOffsets[num8];
			if ((num9 >= 0 && num10 >= 0) & (num9 < width) & (num10 < depth))
			{
				GridNodeBase node4 = nodes[nodeInGridIndex + neighbourOffsets[num8]];
				gridNode.SetConnectionInternal(num8, IsValidConnection(gridNode, node4));
			}
		}
	}

	public override void OnDrawGizmos(RetainedGizmos gizmos, bool drawNodes)
	{
		using (GraphGizmoHelper graphGizmoHelper = gizmos.GetSingleFrameGizmoHelper(active))
		{
			CalculateDimensions(out var num, out var num2, out var _);
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(Vector3.zero, new Vector3(num, 0f, num2));
			GraphTransform graphTransform = CalculateTransform();
			graphGizmoHelper.builder.DrawWireCube(graphTransform, bounds, Color.white);
			int num4 = ((nodes != null) ? nodes.Length : (-1));
			if (this is LayerGridGraph)
			{
				num4 = (((this as LayerGridGraph).nodes != null) ? (this as LayerGridGraph).nodes.Length : (-1));
			}
			if (drawNodes && width * depth * LayerCount != num4)
			{
				Color color = new Color(1f, 1f, 1f, 0.2f);
				for (int i = 0; i < num2; i++)
				{
					graphGizmoHelper.builder.DrawLine(graphTransform.Transform(new Vector3(0f, 0f, i)), graphTransform.Transform(new Vector3(num, 0f, i)), color);
				}
				for (int j = 0; j < num; j++)
				{
					graphGizmoHelper.builder.DrawLine(graphTransform.Transform(new Vector3(j, 0f, 0f)), graphTransform.Transform(new Vector3(j, 0f, num2)), color);
				}
			}
		}
		if (!drawNodes)
		{
			return;
		}
		GridNodeBase[] array = ArrayPool<GridNodeBase>.Claim(1024 * LayerCount);
		for (int num5 = width / 32; num5 >= 0; num5--)
		{
			for (int num6 = depth / 32; num6 >= 0; num6--)
			{
				int nodesInRegion = GetNodesInRegion(new IntRect(num5 * 32, num6 * 32, (num5 + 1) * 32 - 1, (num6 + 1) * 32 - 1), array);
				RetainedGizmos.Hasher hasher = new RetainedGizmos.Hasher(active);
				hasher.AddHash(showMeshOutline ? 1 : 0);
				hasher.AddHash(showMeshSurface ? 1 : 0);
				hasher.AddHash(showNodeConnections ? 1 : 0);
				for (int k = 0; k < nodesInRegion; k++)
				{
					hasher.HashNode(array[k]);
				}
				if (!gizmos.Draw(hasher))
				{
					using GraphGizmoHelper graphGizmoHelper2 = gizmos.GetGizmoHelper(active, hasher);
					if (showNodeConnections)
					{
						for (int l = 0; l < nodesInRegion; l++)
						{
							if (array[l].Walkable)
							{
								graphGizmoHelper2.DrawConnections(array[l]);
							}
						}
					}
					if (showMeshSurface || showMeshOutline)
					{
						CreateNavmeshSurfaceVisualization(array, nodesInRegion, graphGizmoHelper2);
					}
				}
			}
		}
		ArrayPool<GridNodeBase>.Release(ref array);
		if (active.showUnwalkableNodes)
		{
			DrawUnwalkableNodes(nodeSize * 0.3f);
		}
	}

	private void CreateNavmeshSurfaceVisualization(GridNodeBase[] nodes, int nodeCount, GraphGizmoHelper helper)
	{
		int num = 0;
		for (int i = 0; i < nodeCount; i++)
		{
			if (nodes[i].Walkable)
			{
				num++;
			}
		}
		int[] array = ((neighbours == NumNeighbours.Six) ? hexagonNeighbourIndices : new int[4] { 0, 1, 2, 3 });
		float num2 = ((neighbours == NumNeighbours.Six) ? 0.333333f : 0.5f);
		int num3 = array.Length - 2;
		int num4 = 3 * num3;
		Vector3[] array2 = ArrayPool<Vector3>.Claim(num * num4);
		Color[] array3 = ArrayPool<Color>.Claim(num * num4);
		int num5 = 0;
		for (int j = 0; j < nodeCount; j++)
		{
			GridNodeBase gridNodeBase = nodes[j];
			if (!gridNodeBase.Walkable)
			{
				continue;
			}
			Color color = helper.NodeColor(gridNodeBase);
			if (color.a <= 0.001f)
			{
				continue;
			}
			for (int k = 0; k < array.Length; k++)
			{
				int num6 = array[k];
				int num7 = array[(k + 1) % array.Length];
				GridNodeBase gridNodeBase2 = null;
				GridNodeBase neighbourAlongDirection = gridNodeBase.GetNeighbourAlongDirection(num6);
				if (neighbourAlongDirection != null && neighbours != NumNeighbours.Six)
				{
					gridNodeBase2 = neighbourAlongDirection.GetNeighbourAlongDirection(num7);
				}
				GridNodeBase neighbourAlongDirection2 = gridNodeBase.GetNeighbourAlongDirection(num7);
				if (neighbourAlongDirection2 != null && gridNodeBase2 == null && neighbours != NumNeighbours.Six)
				{
					gridNodeBase2 = neighbourAlongDirection2.GetNeighbourAlongDirection(num6);
				}
				Vector3 point = new Vector3((float)gridNodeBase.XCoordinateInGrid + 0.5f, 0f, (float)gridNodeBase.ZCoordinateInGrid + 0.5f);
				point.x += (float)(neighbourXOffsets[num6] + neighbourXOffsets[num7]) * num2;
				point.z += (float)(neighbourZOffsets[num6] + neighbourZOffsets[num7]) * num2;
				point.y += transform.InverseTransform((Vector3)gridNodeBase.position).y;
				if (neighbourAlongDirection != null)
				{
					point.y += transform.InverseTransform((Vector3)neighbourAlongDirection.position).y;
				}
				if (neighbourAlongDirection2 != null)
				{
					point.y += transform.InverseTransform((Vector3)neighbourAlongDirection2.position).y;
				}
				if (gridNodeBase2 != null)
				{
					point.y += transform.InverseTransform((Vector3)gridNodeBase2.position).y;
				}
				point.y /= 1f + ((neighbourAlongDirection != null) ? 1f : 0f) + ((neighbourAlongDirection2 != null) ? 1f : 0f) + ((gridNodeBase2 != null) ? 1f : 0f);
				point = transform.Transform(point);
				array2[num5 + k] = point;
			}
			if (neighbours == NumNeighbours.Six)
			{
				array2[num5 + 6] = array2[num5];
				array2[num5 + 7] = array2[num5 + 2];
				array2[num5 + 8] = array2[num5 + 3];
				array2[num5 + 9] = array2[num5];
				array2[num5 + 10] = array2[num5 + 3];
				array2[num5 + 11] = array2[num5 + 5];
			}
			else
			{
				array2[num5 + 4] = array2[num5];
				array2[num5 + 5] = array2[num5 + 2];
			}
			for (int l = 0; l < num4; l++)
			{
				array3[num5 + l] = color;
			}
			for (int m = 0; m < array.Length; m++)
			{
				GridNodeBase neighbourAlongDirection3 = gridNodeBase.GetNeighbourAlongDirection(array[(m + 1) % array.Length]);
				if (neighbourAlongDirection3 == null || (showMeshOutline && gridNodeBase.NodeInGridIndex < neighbourAlongDirection3.NodeInGridIndex))
				{
					helper.builder.DrawLine(array2[num5 + m], array2[num5 + (m + 1) % array.Length], (neighbourAlongDirection3 == null) ? Color.black : color);
				}
			}
			num5 += num4;
		}
		if (showMeshSurface)
		{
			helper.DrawTriangles(array2, array3, num5 * num3 / num4);
		}
		ArrayPool<Vector3>.Release(ref array2);
		ArrayPool<Color>.Release(ref array3);
	}

	public IntRect GetRectFromBounds(Bounds bounds)
	{
		bounds = transform.InverseTransform(bounds);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		IntRect a = new IntRect(xmin, ymin, xmax, ymax);
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		return IntRect.Intersection(a, b);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(Bounds bounds)
	{
		return GetNodesInRegion(bounds);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(GraphUpdateShape shape)
	{
		return GetNodesInRegion(shape);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(Bounds bounds, GraphUpdateShape shape)
	{
		return GetNodesInRegion(bounds, shape);
	}

	public List<GraphNode> GetNodesInRegion(Bounds bounds)
	{
		return GetNodesInRegion(bounds, null);
	}

	public List<GraphNode> GetNodesInRegion(GraphUpdateShape shape)
	{
		return GetNodesInRegion(shape.GetBounds(), shape);
	}

	protected virtual List<GraphNode> GetNodesInRegion(Bounds bounds, GraphUpdateShape shape)
	{
		IntRect rectFromBounds = GetRectFromBounds(bounds);
		if (nodes == null || !rectFromBounds.IsValid() || nodes.Length != width * depth)
		{
			return ListPool<GraphNode>.Claim();
		}
		List<GraphNode> list = ListPool<GraphNode>.Claim(rectFromBounds.Width * rectFromBounds.Height);
		for (int i = rectFromBounds.xmin; i <= rectFromBounds.xmax; i++)
		{
			for (int j = rectFromBounds.ymin; j <= rectFromBounds.ymax; j++)
			{
				int num = j * width + i;
				GraphNode graphNode = nodes[num];
				if (bounds.Contains((Vector3)graphNode.position) && (shape == null || shape.Contains((Vector3)graphNode.position)))
				{
					list.Add(graphNode);
				}
			}
		}
		return list;
	}

	public virtual List<GraphNode> GetNodesInRegion(IntRect rect)
	{
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth)
		{
			return ListPool<GraphNode>.Claim(0);
		}
		List<GraphNode> list = ListPool<GraphNode>.Claim(rect.Width * rect.Height);
		for (int i = rect.ymin; i <= rect.ymax; i++)
		{
			int num = i * Width;
			for (int j = rect.xmin; j <= rect.xmax; j++)
			{
				list.Add(nodes[num + j]);
			}
		}
		return list;
	}

	public virtual int GetNodesInRegion(IntRect rect, GridNodeBase[] buffer)
	{
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth)
		{
			return 0;
		}
		if (buffer.Length < rect.Width * rect.Height)
		{
			throw new ArgumentException("Buffer is too small");
		}
		int num = 0;
		int num2 = rect.ymin;
		while (num2 <= rect.ymax)
		{
			Array.Copy(nodes, num2 * Width + rect.xmin, buffer, num, rect.Width);
			num2++;
			num += rect.Width;
		}
		return num;
	}

	public virtual GridNodeBase GetNode(int x, int z)
	{
		if (x < 0 || z < 0 || x >= width || z >= depth)
		{
			return null;
		}
		return nodes[x + z * width];
	}

	GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
	{
		return GraphUpdateThreading.UnityThread;
	}

	void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
	{
	}

	void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject o)
	{
	}

	protected void CalculateAffectedRegions(GraphUpdateObject o, out IntRect originalRect, out IntRect affectRect, out IntRect physicsRect, out bool willChangeWalkability, out int erosion)
	{
		Bounds bounds = transform.InverseTransform(o.bounds);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		originalRect = new IntRect(xmin, ymin, xmax, ymax);
		affectRect = originalRect;
		physicsRect = originalRect;
		erosion = (o.updateErosion ? erodeIterations : 0);
		willChangeWalkability = o.updatePhysics || o.modifyWalkability;
		if (o.updatePhysics && !o.modifyWalkability && collision.collisionCheck)
		{
			Vector3 vector = new Vector3(collision.diameter, 0f, collision.diameter) * 0.5f;
			min -= vector * 1.02f;
			max += vector * 1.02f;
			physicsRect = new IntRect(Mathf.RoundToInt(min.x - 0.5f), Mathf.RoundToInt(min.z - 0.5f), Mathf.RoundToInt(max.x - 0.5f), Mathf.RoundToInt(max.z - 0.5f));
			affectRect = IntRect.Union(physicsRect, affectRect);
		}
		if (willChangeWalkability || erosion > 0)
		{
			affectRect = affectRect.Expand(erosion + 1);
		}
	}

	void IUpdatableGraph.UpdateArea(GraphUpdateObject o)
	{
		if (nodes == null || nodes.Length != width * depth)
		{
			Debug.LogWarning("The Grid Graph is not scanned, cannot update area");
			return;
		}
		CalculateAffectedRegions(o, out var originalRect, out var affectRect, out var physicsRect, out var willChangeWalkability, out var erosion);
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect = IntRect.Intersection(affectRect, b);
		for (int i = intRect.ymin; i <= intRect.ymax; i++)
		{
			for (int j = intRect.xmin; j <= intRect.xmax; j++)
			{
				o.WillUpdateNode(nodes[i * width + j]);
			}
		}
		if (o.updatePhysics && !o.modifyWalkability)
		{
			collision.Initialize(transform, nodeSize);
			intRect = IntRect.Intersection(physicsRect, b);
			for (int k = intRect.ymin; k <= intRect.ymax; k++)
			{
				for (int l = intRect.xmin; l <= intRect.xmax; l++)
				{
					RecalculateCell(l, k, o.resetPenaltyOnPhysics, resetTags: false);
				}
			}
		}
		intRect = IntRect.Intersection(originalRect, b);
		for (int m = intRect.ymin; m <= intRect.ymax; m++)
		{
			for (int n = intRect.xmin; n <= intRect.xmax; n++)
			{
				int num = m * width + n;
				GridNodeBase gridNodeBase = nodes[num];
				if (o.bounds.Contains((Vector3)gridNodeBase.position))
				{
					if (willChangeWalkability)
					{
						gridNodeBase.Walkable = gridNodeBase.WalkableErosion;
						o.Apply(gridNodeBase);
						gridNodeBase.WalkableErosion = gridNodeBase.Walkable;
					}
					else
					{
						o.Apply(gridNodeBase);
					}
				}
			}
		}
		if (willChangeWalkability && erosion == 0)
		{
			intRect = IntRect.Intersection(affectRect, b);
			for (int num2 = intRect.xmin; num2 <= intRect.xmax; num2++)
			{
				for (int num3 = intRect.ymin; num3 <= intRect.ymax; num3++)
				{
					CalculateConnections(num2, num3);
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
			for (int num4 = a2.xmin; num4 <= a2.xmax; num4++)
			{
				for (int num5 = a2.ymin; num5 <= a2.ymax; num5++)
				{
					int num6 = num5 * width + num4;
					GridNodeBase gridNodeBase2 = nodes[num6];
					bool walkable = gridNodeBase2.Walkable;
					gridNodeBase2.Walkable = gridNodeBase2.WalkableErosion;
					if (!a.Contains(num4, num5))
					{
						gridNodeBase2.TmpWalkable = walkable;
					}
				}
			}
			for (int num7 = a2.xmin; num7 <= a2.xmax; num7++)
			{
				for (int num8 = a2.ymin; num8 <= a2.ymax; num8++)
				{
					CalculateConnections(num7, num8);
				}
			}
			ErodeWalkableArea(a2.xmin, a2.ymin, a2.xmax + 1, a2.ymax + 1);
			for (int num9 = a2.xmin; num9 <= a2.xmax; num9++)
			{
				for (int num10 = a2.ymin; num10 <= a2.ymax; num10++)
				{
					if (!a.Contains(num9, num10))
					{
						int num11 = num10 * width + num9;
						GridNodeBase obj = nodes[num11];
						obj.Walkable = obj.TmpWalkable;
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
		}
	}

	public bool Linecast(Vector3 from, Vector3 to)
	{
		GraphHitInfo hit;
		return Linecast(from, to, out hit, (List<GraphNode>)null, (Func<GraphNode, bool>)null);
	}

	[Obsolete("The hint parameter is deprecated")]
	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint)
	{
		GraphHitInfo hit;
		return Linecast(from, to, hint, out hit);
	}

	[Obsolete("The hint parameter is deprecated")]
	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast(from, to, hint, out hit, null);
	}

	protected static long CrossMagnitude(Int2 a, Int2 b)
	{
		return (long)a.x * (long)b.y - (long)b.x * (long)a.y;
	}

	protected bool ClipLineSegmentToBounds(Vector3 a, Vector3 b, out Vector3 outA, out Vector3 outB)
	{
		if (a.x < 0f || a.z < 0f || a.x > (float)width || a.z > (float)depth || b.x < 0f || b.z < 0f || b.x > (float)width || b.z > (float)depth)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			Vector3 vector2 = new Vector3(0f, 0f, depth);
			Vector3 vector3 = new Vector3(width, 0f, depth);
			Vector3 vector4 = new Vector3(width, 0f, 0f);
			int num = 0;
			Vector3 vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector, vector2, out var intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector, vector2, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector2, vector3, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector2, vector3, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector3, vector4, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector3, vector4, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector4, vector, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector4, vector, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			if (num == 0)
			{
				outA = Vector3.zero;
				outB = Vector3.zero;
				return false;
			}
		}
		outA = a;
		outB = b;
		return true;
	}

	[Obsolete("The hint parameter is deprecated")]
	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
	{
		return Linecast(from, to, out hit, trace, (Func<GraphNode, bool>)null);
	}

	public bool Linecast(Vector3 from, Vector3 to, out GraphHitInfo hit, List<GraphNode> trace = null, Func<GraphNode, bool> filter = null)
	{
		hit = default(GraphHitInfo);
		GridHitInfo hit2;
		bool num = Linecast(from, to, out hit2, trace, filter);
		hit.origin = from;
		hit.node = hit2.node;
		if (num)
		{
			int direction = hit2.direction;
			if (direction == -1 || hit2.node == null)
			{
				hit.point = ((hit2.node != null) ? ((Vector3)hit2.node.position) : from);
				hit.tangentOrigin = Vector3.zero;
				hit.tangent = Vector3.zero;
				return num;
			}
			Vector3 vector = transform.InverseTransform(from);
			Vector3 vector2 = transform.InverseTransform(to);
			Vector2 start = new Vector2(vector.x - 0.5f, vector.z - 0.5f);
			Vector2 end = new Vector2(vector2.x - 0.5f, vector2.z - 0.5f);
			Vector2 vector3 = new Vector2(neighbourXOffsets[direction], neighbourZOffsets[direction]);
			Vector2 vector4 = new Vector2(neighbourXOffsets[(direction - 1 + 4) & 3], neighbourZOffsets[(direction - 1 + 4) & 3]);
			Vector2 vector5 = new Vector2(neighbourXOffsets[(direction + 1) & 3], neighbourZOffsets[(direction + 1) & 3]);
			Vector2 vector6 = new Vector2(hit2.node.XCoordinateInGrid, hit2.node.ZCoordinateInGrid) + (vector3 + vector4) * 0.5f;
			Vector2 vector7 = VectorMath.LineIntersectionPoint(vector6, vector6 + vector5, start, end);
			Vector3 vector8 = transform.InverseTransform((Vector3)hit2.node.position);
			Vector3 point = new Vector3(vector7.x + 0.5f, vector8.y, vector7.y + 0.5f);
			Vector3 point2 = new Vector3(vector6.x + 0.5f, vector8.y, vector6.y + 0.5f);
			hit.point = transform.Transform(point);
			hit.tangentOrigin = transform.Transform(point2);
			hit.tangent = transform.TransformVector(new Vector3(vector5.x, 0f, vector5.y));
			return num;
		}
		hit = default(GraphHitInfo);
		return num;
	}

	[Obsolete("Use Linecast instead")]
	public bool SnappedLinecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast((Vector3)GetNearest(from, null).node.position, (Vector3)GetNearest(to, null).node.position, hint, out hit);
	}

	public bool Linecast(GridNodeBase fromNode, GridNodeBase toNode, Func<GraphNode, bool> filter = null)
	{
		GridHitInfo hit;
		return Linecast(fromNode, new Vector2(0.5f, 0.5f), toNode, new Vector2(0.5f, 0.5f), out hit, null, filter);
	}

	public bool Linecast(Vector3 from, Vector3 to, out GridHitInfo hit, List<GraphNode> trace = null, Func<GraphNode, bool> filter = null)
	{
		Vector3 vector = transform.InverseTransform(from);
		Vector3 vector2 = transform.InverseTransform(to);
		if (!ClipLineSegmentToBounds(vector, vector2, out var outA, out var outB))
		{
			hit = new GridHitInfo
			{
				node = null,
				direction = -1
			};
			return false;
		}
		if ((vector - outA).sqrMagnitude > 1.0000001E-06f)
		{
			hit = new GridHitInfo
			{
				node = null,
				direction = -1
			};
			return true;
		}
		bool continuePastEnd = (vector2 - outB).sqrMagnitude > 1.0000001E-06f;
		GridNodeBase nearestFromGraphSpace = GetNearestFromGraphSpace(outA);
		GridNodeBase nearestFromGraphSpace2 = GetNearestFromGraphSpace(outB);
		if (nearestFromGraphSpace == null || nearestFromGraphSpace2 == null)
		{
			hit = new GridHitInfo
			{
				node = null,
				direction = -1
			};
			return false;
		}
		return Linecast(nearestFromGraphSpace, new Vector2(outA.x - (float)nearestFromGraphSpace.XCoordinateInGrid, outA.z - (float)nearestFromGraphSpace.ZCoordinateInGrid), nearestFromGraphSpace2, new Vector2(outB.x - (float)nearestFromGraphSpace2.XCoordinateInGrid, outB.z - (float)nearestFromGraphSpace2.ZCoordinateInGrid), out hit, trace, filter, continuePastEnd);
	}

	public bool Linecast(GridNodeBase fromNode, Vector2 normalizedFromPoint, GridNodeBase toNode, Vector2 normalizedToPoint, out GridHitInfo hit, List<GraphNode> trace = null, Func<GraphNode, bool> filter = null, bool continuePastEnd = false)
	{
		Int2 fixedNormalizedFromPoint = new Int2((int)Mathf.Round(normalizedFromPoint.x * 1024f), (int)Mathf.Round(normalizedFromPoint.y * 1024f));
		Int2 fixedNormalizedToPoint = new Int2((int)Mathf.Round(normalizedToPoint.x * 1024f), (int)Mathf.Round(normalizedToPoint.y * 1024f));
		return Linecast(fromNode, fixedNormalizedFromPoint, toNode, fixedNormalizedToPoint, out hit, trace, filter, continuePastEnd);
	}

	public bool Linecast(GridNodeBase fromNode, Int2 fixedNormalizedFromPoint, GridNodeBase toNode, Int2 fixedNormalizedToPoint, out GridHitInfo hit, List<GraphNode> trace = null, Func<GraphNode, bool> filter = null, bool continuePastEnd = false)
	{
		if (fixedNormalizedFromPoint.x < 0 || fixedNormalizedFromPoint.x > 1024)
		{
			throw new ArgumentOutOfRangeException("normalizedFromPoint must be between 0 and 1");
		}
		if (fixedNormalizedToPoint.x < 0 || fixedNormalizedToPoint.x > 1024)
		{
			throw new ArgumentOutOfRangeException("normalizedToPoint must be between 0 and 1");
		}
		if (fromNode == null)
		{
			throw new ArgumentNullException("fromNode");
		}
		if (toNode == null)
		{
			throw new ArgumentNullException("toNode");
		}
		if ((filter != null && !filter(fromNode)) || !fromNode.Walkable)
		{
			hit = new GridHitInfo
			{
				node = fromNode,
				direction = -1
			};
			return true;
		}
		if (fromNode == toNode)
		{
			hit = new GridHitInfo
			{
				node = fromNode,
				direction = -1
			};
			return false;
		}
		Int2 @int = new Int2(fromNode.XCoordinateInGrid, fromNode.ZCoordinateInGrid);
		Int2 int2 = new Int2(toNode.XCoordinateInGrid, toNode.ZCoordinateInGrid);
		Int2 int3 = new Int2(@int.x * 1024, @int.y * 1024) + fixedNormalizedFromPoint;
		Int2 int4 = new Int2(int2.x * 1024, int2.y * 1024) + fixedNormalizedToPoint;
		Int2 int5 = int4 - int3;
		int num = Math.Abs(@int.x - int2.x) + Math.Abs(@int.y - int2.y);
		if (continuePastEnd)
		{
			num = int.MaxValue;
		}
		if (int3 == int4)
		{
			num = 0;
		}
		int num2 = 0;
		Int2 int6 = int5;
		if (int6.x == 0)
		{
			int6.x = Math.Sign(512 - fixedNormalizedToPoint.x);
		}
		if (int6.y == 0)
		{
			int6.y = Math.Sign(512 - fixedNormalizedToPoint.y);
		}
		if (int6.x <= 0 && int6.y > 0)
		{
			num2 = 1;
		}
		else if (int6.x < 0 && int6.y <= 0)
		{
			num2 = 2;
		}
		else if (int6.x >= 0 && int6.y < 0)
		{
			num2 = 3;
		}
		int num3 = (num2 + 1) & 3;
		int num4 = (num2 + 2) & 3;
		long num5 = CrossMagnitude(int5, new Int2(neighbourXOffsets[num4] + neighbourXOffsets[num3], neighbourZOffsets[num4] + neighbourZOffsets[num3]));
		long num6 = CrossMagnitude(b: new Int2(512, 512) - fixedNormalizedFromPoint, a: int5) * 2 / 1024;
		long num7 = -int5.y * 2;
		long num8 = int5.x * 2;
		int num9 = num4;
		int num10 = num3;
		if (CrossMagnitude(b: new Int2(int2.x * 1024, int2.y * 1024) + new Int2(512, 512) - int3, a: int5) < 0)
		{
			num9 = num3;
			num10 = num4;
		}
		GridNodeBase gridNodeBase = null;
		GridNodeBase gridNodeBase2 = null;
		while (num > 0)
		{
			trace?.Add(fromNode);
			long num11 = num6 + num5;
			int num12;
			GridNodeBase gridNodeBase3;
			if (num11 == 0L)
			{
				num12 = num9;
				gridNodeBase3 = fromNode.GetNeighbourAlongDirection(num12);
				if ((filter != null && gridNodeBase3 != null && !filter(gridNodeBase3)) || gridNodeBase3 == gridNodeBase)
				{
					gridNodeBase3 = null;
				}
				if (gridNodeBase3 == null)
				{
					num12 = num10;
					gridNodeBase3 = fromNode.GetNeighbourAlongDirection(num12);
					if ((filter != null && gridNodeBase3 != null && !filter(gridNodeBase3)) || gridNodeBase3 == gridNodeBase)
					{
						gridNodeBase3 = null;
					}
				}
			}
			else
			{
				num12 = ((num11 < 0) ? num4 : num3);
				gridNodeBase3 = fromNode.GetNeighbourAlongDirection(num12);
				if ((filter != null && gridNodeBase3 != null && !filter(gridNodeBase3)) || gridNodeBase3 == gridNodeBase)
				{
					gridNodeBase3 = null;
				}
			}
			if (gridNodeBase3 == null)
			{
				for (int i = -1; i <= 1; i += 2)
				{
					int num13 = (num12 + i + 4) & 3;
					if (num6 + num7 / 2 * (neighbourXOffsets[num12] + neighbourXOffsets[num13]) + num8 / 2 * (neighbourZOffsets[num12] + neighbourZOffsets[num13]) == 0L)
					{
						gridNodeBase3 = fromNode.GetNeighbourAlongDirection(num13);
						if ((filter != null && gridNodeBase3 != null && !filter(gridNodeBase3)) || gridNodeBase3 == gridNodeBase || gridNodeBase3 == gridNodeBase2)
						{
							gridNodeBase3 = null;
						}
						if (gridNodeBase3 != null)
						{
							num = 1 + Math.Abs(gridNodeBase3.XCoordinateInGrid - int2.x) + Math.Abs(gridNodeBase3.ZCoordinateInGrid - int2.y);
							num12 = num13;
							gridNodeBase = fromNode;
							gridNodeBase2 = gridNodeBase3;
						}
						break;
					}
				}
				if (gridNodeBase3 == null)
				{
					hit = new GridHitInfo
					{
						node = fromNode,
						direction = num12
					};
					return true;
				}
			}
			num6 += num7 * neighbourXOffsets[num12] + num8 * neighbourZOffsets[num12];
			fromNode = gridNodeBase3;
			num--;
		}
		hit = new GridHitInfo
		{
			node = fromNode,
			direction = -1
		};
		if (fromNode != toNode)
		{
			Int2 int7 = int4 - (new Int2(fromNode.XCoordinateInGrid * 1024, fromNode.ZCoordinateInGrid * 1024) + new Int2(512, 512));
			if (Mathf.Abs(int7.x) == 512 && Mathf.Abs(int7.y) == 512)
			{
				Int2 int8 = new Int2(int7.x * 2 / 1024, int7.y * 2 / 1024);
				int num14 = -1;
				for (int j = 0; j < 4; j++)
				{
					if (neighbourXOffsets[j] + neighbourXOffsets[(j + 1) & 3] == int8.x && neighbourZOffsets[j] + neighbourZOffsets[(j + 1) & 3] == int8.y)
					{
						num14 = j;
						break;
					}
				}
				int num15 = trace?.Count ?? 0;
				int num16 = num14;
				GridNodeBase gridNodeBase4 = fromNode;
				for (int k = 0; k < 3; k++)
				{
					if (gridNodeBase4 == toNode)
					{
						break;
					}
					trace?.Add(gridNodeBase4);
					gridNodeBase4 = gridNodeBase4.GetNeighbourAlongDirection(num16);
					if (gridNodeBase4 == null || (filter != null && !filter(gridNodeBase4)))
					{
						gridNodeBase4 = null;
						break;
					}
					num16 = (num16 + 1) & 3;
				}
				if (gridNodeBase4 != toNode)
				{
					trace?.RemoveRange(num15, trace.Count - num15);
					gridNodeBase4 = fromNode;
					num16 = (num14 + 1) & 3;
					for (int l = 0; l < 3; l++)
					{
						if (gridNodeBase4 == toNode)
						{
							break;
						}
						trace?.Add(gridNodeBase4);
						gridNodeBase4 = gridNodeBase4.GetNeighbourAlongDirection(num16);
						if (gridNodeBase4 == null || (filter != null && !filter(gridNodeBase4)))
						{
							gridNodeBase4 = null;
							break;
						}
						num16 = (num16 - 1 + 4) & 3;
					}
					if (gridNodeBase4 != toNode)
					{
						trace?.RemoveRange(num15, trace.Count - num15);
					}
				}
				fromNode = gridNodeBase4;
			}
		}
		trace?.Add(fromNode);
		return fromNode != toNode;
	}

	public bool CheckConnection(GridNode node, int dir)
	{
		if (neighbours == NumNeighbours.Eight || neighbours == NumNeighbours.Six || dir < 4)
		{
			return node.HasConnectionInDirection(dir);
		}
		int num = (dir - 4 - 1) & 3;
		int num2 = (dir - 4 + 1) & 3;
		if (!node.HasConnectionInDirection(num) || !node.HasConnectionInDirection(num2))
		{
			return false;
		}
		GridNodeBase gridNodeBase = nodes[node.NodeInGridIndex + neighbourOffsets[num]];
		GridNodeBase gridNodeBase2 = nodes[node.NodeInGridIndex + neighbourOffsets[num2]];
		if (!gridNodeBase.Walkable || !gridNodeBase2.Walkable)
		{
			return false;
		}
		if (!gridNodeBase2.HasConnectionInDirection(num) || !gridNodeBase.HasConnectionInDirection(num2))
		{
			return false;
		}
		return true;
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
		GridNodeBase[] array = new GridNode[num];
		nodes = array;
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = new GridNode(active);
			nodes[i].DeserializeNode(ctx);
		}
	}

	protected override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
	{
		base.DeserializeSettingsCompatibility(ctx);
		aspectRatio = ctx.reader.ReadSingle();
		rotation = ctx.DeserializeVector3();
		center = ctx.DeserializeVector3();
		unclampedSize = ctx.DeserializeVector3();
		nodeSize = ctx.reader.ReadSingle();
		collision.DeserializeSettingsCompatibility(ctx);
		maxClimb = ctx.reader.ReadSingle();
		ctx.reader.ReadInt32();
		maxSlope = ctx.reader.ReadSingle();
		erodeIterations = ctx.reader.ReadInt32();
		erosionUseTags = ctx.reader.ReadBoolean();
		erosionFirstTag = ctx.reader.ReadInt32();
		ctx.reader.ReadBoolean();
		neighbours = (NumNeighbours)ctx.reader.ReadInt32();
		cutCorners = ctx.reader.ReadBoolean();
		penaltyPosition = ctx.reader.ReadBoolean();
		penaltyPositionFactor = ctx.reader.ReadSingle();
		penaltyAngle = ctx.reader.ReadBoolean();
		penaltyAngleFactor = ctx.reader.ReadSingle();
		penaltyAnglePower = ctx.reader.ReadSingle();
		isometricAngle = ctx.reader.ReadSingle();
		uniformEdgeCosts = ctx.reader.ReadBoolean();
		useJumpPointSearch = ctx.reader.ReadBoolean();
	}

	protected override void PostDeserialization(GraphSerializationContext ctx)
	{
		UpdateTransform();
		SetUpOffsetsAndCosts();
		GridNode.SetGridGraph((int)graphIndex, this);
		if (nodes == null || nodes.Length == 0)
		{
			return;
		}
		if (width * depth != nodes.Length)
		{
			Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
			GridNodeBase[] array = new GridNode[0];
			nodes = array;
			return;
		}
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				GridNodeBase gridNodeBase = nodes[i * width + j];
				if (gridNodeBase == null)
				{
					Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator");
					return;
				}
				gridNodeBase.NodeInGridIndex = i * width + j;
			}
		}
	}
}
