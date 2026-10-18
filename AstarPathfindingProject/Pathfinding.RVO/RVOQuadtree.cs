using System;
using Pathfinding.RVO.Sampled;
using UnityEngine;

namespace Pathfinding.RVO;

public class RVOQuadtree
{
	private struct Node
	{
		public int child00;

		public Agent linkedList;

		public byte count;

		public float maxSpeed;

		public void Add(Agent agent)
		{
			agent.next = linkedList;
			linkedList = agent;
		}

		public void Distribute(Node[] nodes, Rect r)
		{
			Vector2 center = r.center;
			while (linkedList != null)
			{
				Agent next = linkedList.next;
				int num = child00 + ((linkedList.position.x > center.x) ? 2 : 0) + ((linkedList.position.y > center.y) ? 1 : 0);
				nodes[num].Add(linkedList);
				linkedList = next;
			}
			count = 0;
		}

		public float CalculateMaxSpeed(Node[] nodes, int index)
		{
			if (child00 == index)
			{
				for (Agent next = linkedList; next != null; next = next.next)
				{
					maxSpeed = Math.Max(maxSpeed, next.CalculatedSpeed);
				}
			}
			else
			{
				maxSpeed = Math.Max(nodes[child00].CalculateMaxSpeed(nodes, child00), nodes[child00 + 1].CalculateMaxSpeed(nodes, child00 + 1));
				maxSpeed = Math.Max(maxSpeed, nodes[child00 + 2].CalculateMaxSpeed(nodes, child00 + 2));
				maxSpeed = Math.Max(maxSpeed, nodes[child00 + 3].CalculateMaxSpeed(nodes, child00 + 3));
			}
			return maxSpeed;
		}
	}

	private struct QuadtreeQuery
	{
		public Vector2 p;

		public float speed;

		public float timeHorizon;

		public float agentRadius;

		public float maxRadius;

		public Agent agent;

		public Node[] nodes;

		public void QueryRec(int i, Rect r)
		{
			float num = Math.Min(Math.Max((nodes[i].maxSpeed + speed) * timeHorizon, agentRadius) + agentRadius, maxRadius);
			if (nodes[i].child00 == i)
			{
				for (Agent agent = nodes[i].linkedList; agent != null; agent = agent.next)
				{
					float num2 = this.agent.InsertAgentNeighbour(agent, num * num);
					if (num2 < maxRadius * maxRadius)
					{
						maxRadius = Mathf.Sqrt(num2);
					}
				}
				return;
			}
			Vector2 center = r.center;
			if (p.x - num < center.x)
			{
				if (p.y - num < center.y)
				{
					QueryRec(nodes[i].child00, Rect.MinMaxRect(r.xMin, r.yMin, center.x, center.y));
					num = Math.Min(num, maxRadius);
				}
				if (p.y + num > center.y)
				{
					QueryRec(nodes[i].child00 + 1, Rect.MinMaxRect(r.xMin, center.y, center.x, r.yMax));
					num = Math.Min(num, maxRadius);
				}
			}
			if (p.x + num > center.x)
			{
				if (p.y - num < center.y)
				{
					QueryRec(nodes[i].child00 + 2, Rect.MinMaxRect(center.x, r.yMin, r.xMax, center.y));
					num = Math.Min(num, maxRadius);
				}
				if (p.y + num > center.y)
				{
					QueryRec(nodes[i].child00 + 3, Rect.MinMaxRect(center.x, center.y, r.xMax, r.yMax));
				}
			}
		}
	}

	private const int LeafSize = 15;

	private float maxRadius;

	private Node[] nodes = new Node[16];

	private int filledNodes = 1;

	private Rect bounds;

	public void Clear()
	{
		nodes[0] = default(Node);
		filledNodes = 1;
		maxRadius = 0f;
	}

	public void SetBounds(Rect r)
	{
		bounds = r;
	}

	private int GetNodeIndex()
	{
		if (filledNodes + 4 >= nodes.Length)
		{
			Node[] array = new Node[nodes.Length * 2];
			for (int i = 0; i < nodes.Length; i++)
			{
				array[i] = nodes[i];
			}
			nodes = array;
		}
		nodes[filledNodes] = default(Node);
		nodes[filledNodes].child00 = filledNodes;
		filledNodes++;
		nodes[filledNodes] = default(Node);
		nodes[filledNodes].child00 = filledNodes;
		filledNodes++;
		nodes[filledNodes] = default(Node);
		nodes[filledNodes].child00 = filledNodes;
		filledNodes++;
		nodes[filledNodes] = default(Node);
		nodes[filledNodes].child00 = filledNodes;
		filledNodes++;
		return filledNodes - 4;
	}

	public void Insert(Agent agent)
	{
		int num = 0;
		Rect r = bounds;
		Vector2 vector = new Vector2(agent.position.x, agent.position.y);
		agent.next = null;
		maxRadius = Math.Max(agent.radius, maxRadius);
		int num2 = 0;
		while (true)
		{
			num2++;
			if (nodes[num].child00 == num)
			{
				if (nodes[num].count < 15 || num2 > 10)
				{
					break;
				}
				nodes[num].child00 = GetNodeIndex();
				nodes[num].Distribute(nodes, r);
			}
			if (nodes[num].child00 == num)
			{
				continue;
			}
			Vector2 center = r.center;
			if (vector.x > center.x)
			{
				if (vector.y > center.y)
				{
					num = nodes[num].child00 + 3;
					r = Rect.MinMaxRect(center.x, center.y, r.xMax, r.yMax);
				}
				else
				{
					num = nodes[num].child00 + 2;
					r = Rect.MinMaxRect(center.x, r.yMin, r.xMax, center.y);
				}
			}
			else if (vector.y > center.y)
			{
				num = nodes[num].child00 + 1;
				r = Rect.MinMaxRect(r.xMin, center.y, center.x, r.yMax);
			}
			else
			{
				num = nodes[num].child00;
				r = Rect.MinMaxRect(r.xMin, r.yMin, center.x, center.y);
			}
		}
		nodes[num].Add(agent);
		nodes[num].count++;
	}

	public void CalculateSpeeds()
	{
		nodes[0].CalculateMaxSpeed(nodes, 0);
	}

	public void Query(Vector2 p, float speed, float timeHorizon, float agentRadius, Agent agent)
	{
		QuadtreeQuery quadtreeQuery = default(QuadtreeQuery);
		quadtreeQuery.p = p;
		quadtreeQuery.speed = speed;
		quadtreeQuery.timeHorizon = timeHorizon;
		quadtreeQuery.maxRadius = float.PositiveInfinity;
		quadtreeQuery.agentRadius = agentRadius;
		quadtreeQuery.agent = agent;
		quadtreeQuery.nodes = nodes;
		quadtreeQuery.QueryRec(0, bounds);
	}

	public void DebugDraw()
	{
		DebugDrawRec(0, bounds);
	}

	private void DebugDrawRec(int i, Rect r)
	{
		Debug.DrawLine(new Vector3(r.xMin, 0f, r.yMin), new Vector3(r.xMax, 0f, r.yMin), Color.white);
		Debug.DrawLine(new Vector3(r.xMax, 0f, r.yMin), new Vector3(r.xMax, 0f, r.yMax), Color.white);
		Debug.DrawLine(new Vector3(r.xMax, 0f, r.yMax), new Vector3(r.xMin, 0f, r.yMax), Color.white);
		Debug.DrawLine(new Vector3(r.xMin, 0f, r.yMax), new Vector3(r.xMin, 0f, r.yMin), Color.white);
		if (nodes[i].child00 != i)
		{
			Vector2 center = r.center;
			DebugDrawRec(nodes[i].child00 + 3, Rect.MinMaxRect(center.x, center.y, r.xMax, r.yMax));
			DebugDrawRec(nodes[i].child00 + 2, Rect.MinMaxRect(center.x, r.yMin, r.xMax, center.y));
			DebugDrawRec(nodes[i].child00 + 1, Rect.MinMaxRect(r.xMin, center.y, center.x, r.yMax));
			DebugDrawRec(nodes[i].child00, Rect.MinMaxRect(r.xMin, r.yMin, center.x, center.y));
		}
		for (Agent agent = nodes[i].linkedList; agent != null; agent = agent.next)
		{
			Vector2 position = nodes[i].linkedList.position;
			Debug.DrawLine(new Vector3(position.x, 0f, position.y) + Vector3.up, new Vector3(agent.position.x, 0f, agent.position.y) + Vector3.up, new Color(1f, 1f, 0f, 0.5f));
		}
	}
}
