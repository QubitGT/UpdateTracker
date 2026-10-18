using System;
using System.Collections;
using UnityEngine;

namespace Pathfinding;

[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_procedural_grid_mover.php")]
public class ProceduralGridMover : VersionedMonoBehaviour
{
	public float updateDistance = 10f;

	public Transform target;

	private GridNodeBase[] buffer;

	public GridGraph graph;

	[HideInInspector]
	public int graphIndex;

	public bool updatingGraph { get; private set; }

	private void Start()
	{
		if (AstarPath.active == null)
		{
			throw new Exception("There is no AstarPath object in the scene");
		}
		if (graph == null)
		{
			if (graphIndex < 0)
			{
				throw new Exception("Graph index should not be negative");
			}
			if (graphIndex >= AstarPath.active.data.graphs.Length)
			{
				throw new Exception("The ProceduralGridMover was configured to use graph index " + graphIndex + ", but only " + AstarPath.active.data.graphs.Length + " graphs exist");
			}
			graph = AstarPath.active.data.graphs[graphIndex] as GridGraph;
			if (graph == null)
			{
				throw new Exception("The ProceduralGridMover was configured to use graph index " + graphIndex + " but that graph either does not exist or is not a GridGraph or LayerGridGraph");
			}
		}
		UpdateGraph();
	}

	private void Update()
	{
		if (graph != null)
		{
			Vector3 a = PointToGraphSpace(graph.center);
			Vector3 b = PointToGraphSpace(target.position);
			if (VectorMath.SqrDistanceXZ(a, b) > updateDistance * updateDistance)
			{
				UpdateGraph();
			}
		}
	}

	private Vector3 PointToGraphSpace(Vector3 p)
	{
		return graph.transform.InverseTransform(p);
	}

	public void UpdateGraph()
	{
		if (updatingGraph)
		{
			return;
		}
		updatingGraph = true;
		IEnumerator ie = UpdateGraphCoroutine();
		AstarPath.active.AddWorkItem(new AstarWorkItem(delegate(IWorkItemContext context, bool force)
		{
			if (force)
			{
				while (ie.MoveNext())
				{
				}
			}
			bool flag;
			try
			{
				flag = !ie.MoveNext();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
				flag = true;
			}
			if (flag)
			{
				updatingGraph = false;
			}
			return flag;
		}));
	}

	private IEnumerator UpdateGraphCoroutine()
	{
		Vector3 vector = PointToGraphSpace(target.position) - PointToGraphSpace(graph.center);
		vector.x = Mathf.Round(vector.x);
		vector.z = Mathf.Round(vector.z);
		vector.y = 0f;
		if (vector == Vector3.zero)
		{
			yield break;
		}
		Int2 offset = new Int2(-Mathf.RoundToInt(vector.x), -Mathf.RoundToInt(vector.z));
		graph.center += graph.transform.TransformVector(vector);
		graph.UpdateTransform();
		int width = graph.width;
		int depth = graph.depth;
		int layers = graph.LayerCount;
		GridNodeBase[] nodes = ((!(graph is LayerGridGraph layerGridGraph)) ? graph.nodes : layerGridGraph.nodes);
		if (buffer == null || buffer.Length != width * depth)
		{
			buffer = new GridNodeBase[width * depth];
		}
		int counter;
		int yieldEvery2;
		if (Mathf.Abs(offset.x) <= width && Mathf.Abs(offset.y) <= depth)
		{
			IntRect recalculateRect = new IntRect(0, 0, offset.x, offset.y);
			if (recalculateRect.xmin > recalculateRect.xmax)
			{
				int xmax = recalculateRect.xmax;
				recalculateRect.xmax = width + recalculateRect.xmin;
				recalculateRect.xmin = width + xmax;
			}
			if (recalculateRect.ymin > recalculateRect.ymax)
			{
				int ymax = recalculateRect.ymax;
				recalculateRect.ymax = depth + recalculateRect.ymin;
				recalculateRect.ymin = depth + ymax;
			}
			IntRect connectionRect2 = recalculateRect.Expand(1);
			connectionRect2 = IntRect.Intersection(connectionRect2, new IntRect(0, 0, width, depth));
			for (int k = 0; k < layers; k++)
			{
				int layerOffset = k * width * depth;
				for (int n = 0; n < depth; n++)
				{
					int num = n * width;
					int num2 = (n + offset.y + depth) % depth * width;
					for (int num3 = 0; num3 < width; num3++)
					{
						buffer[num2 + (num3 + offset.x + width) % width] = nodes[layerOffset + num + num3];
					}
				}
				yield return null;
				for (int num4 = 0; num4 < depth; num4++)
				{
					int num5 = num4 * width;
					for (int num6 = 0; num6 < width; num6++)
					{
						int num7 = num5 + num6;
						GridNodeBase gridNodeBase = buffer[num7];
						if (gridNodeBase != null)
						{
							gridNodeBase.NodeInGridIndex = num7;
						}
						nodes[layerOffset + num7] = gridNodeBase;
					}
					int num8;
					int num9;
					if (num4 >= recalculateRect.ymin && num4 < recalculateRect.ymax)
					{
						num8 = 0;
						num9 = depth;
					}
					else
					{
						num8 = recalculateRect.xmin;
						num9 = recalculateRect.xmax;
					}
					for (int num10 = num8; num10 < num9; num10++)
					{
						buffer[num5 + num10]?.ClearConnections(alsoReverse: false);
					}
				}
				yield return null;
			}
			yieldEvery2 = 1000;
			int num11 = Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)) * Mathf.Max(width, depth);
			yieldEvery2 = Mathf.Max(yieldEvery2, num11 / 10);
			counter = 0;
			for (int k = 0; k < depth; k++)
			{
				int num12;
				int num13;
				if (k >= recalculateRect.ymin && k < recalculateRect.ymax)
				{
					num12 = 0;
					num13 = width;
				}
				else
				{
					num12 = recalculateRect.xmin;
					num13 = recalculateRect.xmax;
				}
				for (int num14 = num12; num14 < num13; num14++)
				{
					graph.RecalculateCell(num14, k, resetPenalties: false, resetTags: false);
				}
				counter += num13 - num12;
				if (counter > yieldEvery2)
				{
					counter = 0;
					yield return null;
				}
			}
			for (int k = 0; k < depth; k++)
			{
				int num15;
				int num16;
				if (k >= connectionRect2.ymin && k < connectionRect2.ymax)
				{
					num15 = 0;
					num16 = width;
				}
				else
				{
					num15 = connectionRect2.xmin;
					num16 = connectionRect2.xmax;
				}
				for (int num17 = num15; num17 < num16; num17++)
				{
					graph.CalculateConnections(num17, k);
				}
				counter += num16 - num15;
				if (counter > yieldEvery2)
				{
					counter = 0;
					yield return null;
				}
			}
			yield return null;
			for (int num18 = 0; num18 < depth; num18++)
			{
				for (int num19 = 0; num19 < width; num19++)
				{
					if (num19 == 0 || num18 == 0 || num19 == width - 1 || num18 == depth - 1)
					{
						graph.CalculateConnections(num19, num18);
					}
				}
			}
			yield break;
		}
		counter = Mathf.Max(depth * width / 20, 1000);
		yieldEvery2 = 0;
		for (int k = 0; k < depth; k++)
		{
			for (int num20 = 0; num20 < width; num20++)
			{
				graph.RecalculateCell(num20, k);
			}
			yieldEvery2 += width;
			if (yieldEvery2 > counter)
			{
				yieldEvery2 = 0;
				yield return null;
			}
		}
		for (int k = 0; k < depth; k++)
		{
			for (int num21 = 0; num21 < width; num21++)
			{
				graph.CalculateConnections(num21, k);
			}
			yieldEvery2 += width;
			if (yieldEvery2 > counter)
			{
				yieldEvery2 = 0;
				yield return null;
			}
		}
	}
}
