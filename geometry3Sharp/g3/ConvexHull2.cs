using System;
using System.Collections.Generic;

namespace g3;

public class ConvexHull2
{
	protected class Edge
	{
		public Vector2i V;

		public Edge E0;

		public Edge E1;

		public int Sign;

		public int Time;

		public Edge(int v0, int v1)
		{
			Sign = 0;
			Time = -1;
			V[0] = v0;
			V[1] = v1;
			E0 = null;
			E1 = null;
		}

		public int GetSign(int i, Query2 query)
		{
			if (i != Time)
			{
				Time = i;
				Sign = query.ToLine(i, V[0], V[1]);
			}
			return Sign;
		}

		public void Insert(Edge adj0, Edge adj1)
		{
			adj0.E1 = this;
			adj1.E0 = this;
			E0 = adj0;
			E1 = adj1;
		}

		public void DeleteSelf()
		{
			if (E0 != null)
			{
				E0.E1 = null;
			}
			if (E1 != null)
			{
				E1.E0 = null;
			}
		}

		public void GetIndices(ref int numIndices, ref int[] indices)
		{
			numIndices = 0;
			Edge edge = this;
			do
			{
				numIndices++;
				edge = edge.E1;
			}
			while (edge != this);
			indices = new int[numIndices];
			numIndices = 0;
			edge = this;
			do
			{
				indices[numIndices] = edge.V[0];
				numIndices++;
				edge = edge.E1;
			}
			while (edge != this);
		}
	}

	private IList<Vector2d> mVertices;

	private int mNumVertices;

	private int mDimension;

	private int mNumSimplices;

	private double mEpsilon;

	private Vector2d[] mSVertices;

	private int[] mIndices;

	private Query2 mQuery;

	private Vector2d mLineOrigin;

	private Vector2d mLineDirection;

	public int Dimension => mDimension;

	public int NumSimplices => mNumSimplices;

	public int[] HullIndices => mIndices;

	public ConvexHull2(IList<Vector2d> vertices, double epsilon, QueryNumberType queryType)
	{
		mVertices = vertices;
		mNumVertices = vertices.Count;
		mDimension = 0;
		mNumSimplices = 0;
		mIndices = null;
		mSVertices = null;
		mEpsilon = epsilon;
		mQuery = null;
		mLineOrigin = Vector2d.Zero;
		mLineDirection = Vector2d.Zero;
		Vector2d.GetInformation(mVertices, mEpsilon, out var info);
		if (info.mDimension == 0)
		{
			mDimension = 0;
			mIndices = null;
			return;
		}
		if (info.mDimension == 1)
		{
			mDimension = 1;
			mLineOrigin = info.mOrigin;
			mLineDirection = info.mDirection0;
			return;
		}
		mDimension = 2;
		int num = info.mExtreme[0];
		int num2 = info.mExtreme[1];
		int num3 = info.mExtreme[2];
		mSVertices = new Vector2d[mNumVertices];
		if (queryType != QueryNumberType.QT_RATIONAL && queryType != QueryNumberType.QT_FILTERED)
		{
			Vector2d vector2d = new Vector2d(info.mMin[0], info.mMin[1]);
			double num4 = 1.0 / info.mMaxRange;
			for (int i = 0; i < mNumVertices; i++)
			{
				mSVertices[i] = (mVertices[i] - vector2d) * num4;
			}
			double num5;
			switch (queryType)
			{
			case QueryNumberType.QT_INT64:
				num5 = 1048576.0;
				mQuery = new Query2Int64(mSVertices);
				break;
			case QueryNumberType.QT_INTEGER:
				throw new NotImplementedException("ConvexHull2: Query type QT_INTEGER not currently supported");
			default:
				num5 = 1.0;
				mQuery = new Query2d(mSVertices);
				break;
			}
			for (int j = 0; j < mNumVertices; j++)
			{
				mSVertices[j] *= num5;
			}
			Edge edge = null;
			Edge edge2 = null;
			Edge edge3 = null;
			if (info.mExtremeCCW)
			{
				edge = new Edge(num, num2);
				edge2 = new Edge(num2, num3);
				edge3 = new Edge(num3, num);
			}
			else
			{
				edge = new Edge(num, num3);
				edge2 = new Edge(num3, num2);
				edge3 = new Edge(num2, num);
			}
			edge.Insert(edge3, edge2);
			edge2.Insert(edge, edge3);
			edge3.Insert(edge2, edge);
			Edge hull = edge;
			int num6 = 0;
			do
			{
				if (!Update(ref hull, num6))
				{
					return;
				}
				num6 = (num6 + 31337) % mNumVertices;
			}
			while (num6 != 0);
			hull.GetIndices(ref mNumSimplices, ref mIndices);
			return;
		}
		throw new NotImplementedException("ConvexHull2: Query type QT_RATIONAL/QT_FILTERED not currently supported");
	}

	public void Get1DHullInfo(out Vector2d origin, out Vector2d direction)
	{
		origin = mLineOrigin;
		direction = mLineDirection;
	}

	public Polygon2d GetHullPolygon()
	{
		if (mIndices == null)
		{
			return null;
		}
		Polygon2d polygon2d = new Polygon2d();
		for (int i = 0; i < mIndices.Length; i++)
		{
			polygon2d.AppendVertex(mVertices[mIndices[i]]);
		}
		return polygon2d;
	}

	private bool Update(ref Edge hull, int i)
	{
		Edge edge = null;
		Edge edge2 = hull;
		do
		{
			if (edge2.GetSign(i, mQuery) > 0)
			{
				edge = edge2;
				break;
			}
			edge2 = edge2.E1;
		}
		while (edge2 != hull);
		if (edge == null)
		{
			return true;
		}
		Edge e = edge.E0;
		if (e == null)
		{
			return false;
		}
		Edge e2 = edge.E1;
		if (e2 == null)
		{
			return false;
		}
		edge.DeleteSelf();
		while (e.GetSign(i, mQuery) > 0)
		{
			hull = e;
			e = e.E0;
			if (e == null)
			{
				return false;
			}
			e.E1.DeleteSelf();
		}
		while (e2.GetSign(i, mQuery) > 0)
		{
			hull = e2;
			e2 = e2.E1;
			if (e2 == null)
			{
				return false;
			}
			e2.E0.DeleteSelf();
		}
		Edge edge3 = new Edge(e.V[1], i);
		Edge edge4 = new Edge(i, e2.V[0]);
		edge3.Insert(e, edge4);
		edge4.Insert(edge3, e2);
		hull = edge3;
		return true;
	}
}
