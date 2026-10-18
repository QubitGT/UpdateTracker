using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Unity.Cinemachine;

internal class ClipperBase
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct IntersectListSort : IComparer<IntersectNode>
	{
		public int Compare(IntersectNode a, IntersectNode b)
		{
			if (a.pt.Y == b.pt.Y)
			{
				if (a.pt.X >= b.pt.X)
				{
					return 1;
				}
				return -1;
			}
			if (a.pt.Y <= b.pt.Y)
			{
				return 1;
			}
			return -1;
		}
	}

	private ClipType _cliptype;

	private FillRule _fillrule;

	private Active? _actives;

	private Active? _sel;

	private Joiner? _horzJoiners;

	private readonly List<LocalMinima> _minimaList;

	private readonly List<IntersectNode> _intersectList;

	private readonly List<Vertex> _vertexList;

	private readonly List<OutRec> _outrecList;

	private readonly List<Joiner?> _joinerList;

	private readonly List<long> _scanlineList;

	private int _currentLocMin;

	private long _currentBotY;

	private bool _isSortedMinimaList;

	private bool _hasOpenPaths;

	internal bool _using_polytree;

	internal bool _succeeded;

	public bool PreserveCollinear { get; set; }

	public bool ReverseSolution { get; set; }

	public ClipperBase()
	{
		_minimaList = new List<LocalMinima>();
		_intersectList = new List<IntersectNode>();
		_vertexList = new List<Vertex>();
		_outrecList = new List<OutRec>();
		_joinerList = new List<Joiner>();
		_scanlineList = new List<long>();
		PreserveCollinear = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsOdd(int val)
	{
		return (val & 1) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsHotEdge(Active ae)
	{
		return ae.outrec != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsOpen(Active ae)
	{
		return ae.localMin.isOpen;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsOpenEnd(Active ae)
	{
		if (ae.localMin.isOpen)
		{
			return IsOpenEnd(ae.vertexTop);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsOpenEnd(Vertex v)
	{
		return (v.flags & (VertexFlags)3) != 0;
	}

	private static Active? GetPrevHotEdge(Active ae)
	{
		Active prevInAEL = ae.prevInAEL;
		while (prevInAEL != null && (IsOpen(prevInAEL) || !IsHotEdge(prevInAEL)))
		{
			prevInAEL = prevInAEL.prevInAEL;
		}
		return prevInAEL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFront(Active ae)
	{
		return ae == ae.outrec.frontEdge;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double GetDx(Point64 pt1, Point64 pt2)
	{
		double num = pt2.Y - pt1.Y;
		if (num != 0.0)
		{
			return (double)(pt2.X - pt1.X) / num;
		}
		if (pt2.X > pt1.X)
		{
			return double.NegativeInfinity;
		}
		return double.PositiveInfinity;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long TopX(Active ae, long currentY)
	{
		if (currentY == ae.top.Y || ae.top.X == ae.bot.X)
		{
			return ae.top.X;
		}
		if (currentY == ae.bot.Y)
		{
			return ae.bot.X;
		}
		return ae.bot.X + (long)Math.Round(ae.dx * (double)(currentY - ae.bot.Y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsHorizontal(Active ae)
	{
		return ae.top.Y == ae.bot.Y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsHeadingRightHorz(Active ae)
	{
		return double.IsNegativeInfinity(ae.dx);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsHeadingLeftHorz(Active ae)
	{
		return double.IsPositiveInfinity(ae.dx);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SwapActives(ref Active ae1, ref Active ae2)
	{
		Active active = ae1;
		Active active2 = ae2;
		ae2 = active;
		ae1 = active2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static PathType GetPolyType(Active ae)
	{
		return ae.localMin.polytype;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsSamePolyType(Active ae1, Active ae2)
	{
		return ae1.localMin.polytype == ae2.localMin.polytype;
	}

	private static Point64 GetIntersectPoint(Active ae1, Active ae2)
	{
		if (ae1.dx == ae2.dx)
		{
			return ae1.top;
		}
		double num;
		if (ae1.dx == 0.0)
		{
			if (IsHorizontal(ae2))
			{
				return new Point64(ae1.bot.X, ae2.bot.Y);
			}
			num = (double)ae2.bot.Y - (double)ae2.bot.X / ae2.dx;
			return new Point64(ae1.bot.X, (long)Math.Round((double)ae1.bot.X / ae2.dx + num));
		}
		double num2;
		if (ae2.dx == 0.0)
		{
			if (IsHorizontal(ae1))
			{
				return new Point64(ae2.bot.X, ae1.bot.Y);
			}
			num2 = (double)ae1.bot.Y - (double)ae1.bot.X / ae1.dx;
			return new Point64(ae2.bot.X, (long)Math.Round((double)ae2.bot.X / ae1.dx + num2));
		}
		num2 = (double)ae1.bot.X - (double)ae1.bot.Y * ae1.dx;
		num = (double)ae2.bot.X - (double)ae2.bot.Y * ae2.dx;
		double num3 = (num - num2) / (ae1.dx - ae2.dx);
		if (!(Math.Abs(ae1.dx) < Math.Abs(ae2.dx)))
		{
			return new Point64((long)Math.Round(ae2.dx * num3 + num), (long)Math.Round(num3));
		}
		return new Point64((long)Math.Round(ae1.dx * num3 + num2), (long)Math.Round(num3));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetDx(Active ae)
	{
		ae.dx = GetDx(ae.bot, ae.top);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vertex NextVertex(Active ae)
	{
		if (ae.windDx > 0)
		{
			return ae.vertexTop.next;
		}
		return ae.vertexTop.prev;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Vertex PrevPrevVertex(Active ae)
	{
		if (ae.windDx > 0)
		{
			return ae.vertexTop.prev.prev;
		}
		return ae.vertexTop.next.next;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsMaxima(Vertex vertex)
	{
		return (vertex.flags & VertexFlags.LocalMax) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsMaxima(Active ae)
	{
		return IsMaxima(ae.vertexTop);
	}

	private Active? GetMaximaPair(Active ae)
	{
		for (Active nextInAEL = ae.nextInAEL; nextInAEL != null; nextInAEL = nextInAEL.nextInAEL)
		{
			if (nextInAEL.vertexTop == ae.vertexTop)
			{
				return nextInAEL;
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vertex? GetCurrYMaximaVertex(Active ae)
	{
		Vertex vertex = ae.vertexTop;
		if (ae.windDx > 0)
		{
			while (vertex.next.pt.Y == vertex.pt.Y)
			{
				vertex = vertex.next;
			}
		}
		else
		{
			while (vertex.prev.pt.Y == vertex.pt.Y)
			{
				vertex = vertex.prev;
			}
		}
		if (!IsMaxima(vertex))
		{
			vertex = null;
		}
		return vertex;
	}

	private static Active? GetHorzMaximaPair(Active horz, Vertex maxVert)
	{
		Active prevInAEL = horz.prevInAEL;
		while (prevInAEL != null && prevInAEL.curX >= maxVert.pt.X)
		{
			if (prevInAEL.vertexTop == maxVert)
			{
				return prevInAEL;
			}
			prevInAEL = prevInAEL.prevInAEL;
		}
		prevInAEL = horz.nextInAEL;
		while (prevInAEL != null && TopX(prevInAEL, horz.top.Y) <= maxVert.pt.X)
		{
			if (prevInAEL.vertexTop == maxVert)
			{
				return prevInAEL;
			}
			prevInAEL = prevInAEL.nextInAEL;
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetSides(OutRec outrec, Active startEdge, Active endEdge)
	{
		outrec.frontEdge = startEdge;
		outrec.backEdge = endEdge;
	}

	private static void SwapOutrecs(Active ae1, Active ae2)
	{
		OutRec outrec = ae1.outrec;
		OutRec outrec2 = ae2.outrec;
		if (outrec == outrec2)
		{
			Active frontEdge = outrec.frontEdge;
			outrec.frontEdge = outrec.backEdge;
			outrec.backEdge = frontEdge;
			return;
		}
		if (outrec != null)
		{
			if (ae1 == outrec.frontEdge)
			{
				outrec.frontEdge = ae2;
			}
			else
			{
				outrec.backEdge = ae2;
			}
		}
		if (outrec2 != null)
		{
			if (ae2 == outrec2.frontEdge)
			{
				outrec2.frontEdge = ae1;
			}
			else
			{
				outrec2.backEdge = ae1;
			}
		}
		ae1.outrec = outrec2;
		ae2.outrec = outrec;
	}

	private static double Area(OutPt op)
	{
		double num = 0.0;
		OutPt outPt = op;
		do
		{
			num += (double)(outPt.prev.pt.Y + outPt.pt.Y) * (double)(outPt.prev.pt.X - outPt.pt.X);
			outPt = outPt.next;
		}
		while (outPt != op);
		return num * 0.5;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double AreaTriangle(Point64 pt1, Point64 pt2, Point64 pt3)
	{
		return (double)(pt3.Y + pt1.Y) * (double)(pt3.X - pt1.X) + (double)(pt1.Y + pt2.Y) * (double)(pt1.X - pt2.X) + (double)(pt2.Y + pt3.Y) * (double)(pt2.X - pt3.X);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static OutRec? GetRealOutRec(OutRec? outRec)
	{
		while (outRec != null && outRec.pts == null)
		{
			outRec = outRec.owner;
		}
		return outRec;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void UncoupleOutRec(Active ae)
	{
		OutRec outrec = ae.outrec;
		if (outrec != null)
		{
			outrec.frontEdge.outrec = null;
			outrec.backEdge.outrec = null;
			outrec.frontEdge = null;
			outrec.backEdge = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool OutrecIsAscending(Active hotEdge)
	{
		return hotEdge == hotEdge.outrec.frontEdge;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SwapFrontBackSides(OutRec outrec)
	{
		Active frontEdge = outrec.frontEdge;
		outrec.frontEdge = outrec.backEdge;
		outrec.backEdge = frontEdge;
		outrec.pts = outrec.pts.next;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool EdgesAdjacentInAEL(IntersectNode inode)
	{
		if (inode.edge1.nextInAEL != inode.edge2)
		{
			return inode.edge1.prevInAEL == inode.edge2;
		}
		return true;
	}

	protected void ClearSolution()
	{
		while (_actives != null)
		{
			DeleteFromAEL(_actives);
		}
		_scanlineList.Clear();
		DisposeIntersectNodes();
		_joinerList.Clear();
		_horzJoiners = null;
		_outrecList.Clear();
	}

	public void Clear()
	{
		ClearSolution();
		_minimaList.Clear();
		_vertexList.Clear();
		_currentLocMin = 0;
		_isSortedMinimaList = false;
		_hasOpenPaths = false;
	}

	protected void Reset()
	{
		if (!_isSortedMinimaList)
		{
			_minimaList.Sort(default(LocMinSorter));
			_isSortedMinimaList = true;
		}
		_scanlineList.Capacity = _minimaList.Count;
		for (int num = _minimaList.Count - 1; num >= 0; num--)
		{
			_scanlineList.Add(_minimaList[num].vertex.pt.Y);
		}
		_currentBotY = 0L;
		_currentLocMin = 0;
		_actives = null;
		_sel = null;
		_succeeded = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void InsertScanline(long y)
	{
		int num = _scanlineList.BinarySearch(y);
		if (num < 0)
		{
			num = ~num;
			_scanlineList.Insert(num, y);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool PopScanline(out long y)
	{
		int num = _scanlineList.Count - 1;
		if (num < 0)
		{
			y = 0L;
			return false;
		}
		y = _scanlineList[num];
		_scanlineList.RemoveAt(num--);
		while (num >= 0 && y == _scanlineList[num])
		{
			_scanlineList.RemoveAt(num--);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool HasLocMinAtY(long y)
	{
		if (_currentLocMin < _minimaList.Count)
		{
			return _minimaList[_currentLocMin].vertex.pt.Y == y;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private LocalMinima PopLocalMinima()
	{
		return _minimaList[_currentLocMin++];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddLocMin(Vertex vert, PathType polytype, bool isOpen)
	{
		if ((vert.flags & VertexFlags.LocalMin) == 0)
		{
			vert.flags |= VertexFlags.LocalMin;
			LocalMinima item = new LocalMinima(vert, polytype, isOpen);
			_minimaList.Add(item);
		}
	}

	protected void AddPathsToVertexList(List<List<Point64>> paths, PathType polytype, bool isOpen)
	{
		int num = 0;
		foreach (List<Point64> path in paths)
		{
			num += path.Count;
		}
		_vertexList.Capacity = _vertexList.Count + num;
		foreach (List<Point64> path2 in paths)
		{
			Vertex vertex = null;
			Vertex vertex2 = null;
			foreach (Point64 item in path2)
			{
				if (vertex == null)
				{
					vertex = new Vertex(item, VertexFlags.None, null);
					_vertexList.Add(vertex);
					vertex2 = vertex;
				}
				else if (vertex2.pt != item)
				{
					Vertex vertex3 = new Vertex(item, VertexFlags.None, vertex2);
					_vertexList.Add(vertex3);
					vertex2.next = vertex3;
					vertex2 = vertex3;
				}
			}
			if (vertex2 == null || vertex2.prev == null)
			{
				continue;
			}
			if (!isOpen && vertex2.pt == vertex.pt)
			{
				vertex2 = vertex2.prev;
			}
			vertex2.next = vertex;
			vertex.prev = vertex2;
			if (!isOpen && vertex2.next == vertex2)
			{
				continue;
			}
			bool flag;
			if (isOpen)
			{
				Vertex vertex3 = vertex.next;
				while (vertex3 != vertex && vertex3.pt.Y == vertex.pt.Y)
				{
					vertex3 = vertex3.next;
				}
				flag = vertex3.pt.Y <= vertex.pt.Y;
				if (flag)
				{
					vertex.flags = VertexFlags.OpenStart;
					AddLocMin(vertex, polytype, isOpen: true);
				}
				else
				{
					vertex.flags = (VertexFlags)5;
				}
			}
			else
			{
				vertex2 = vertex.prev;
				while (vertex2 != vertex && vertex2.pt.Y == vertex.pt.Y)
				{
					vertex2 = vertex2.prev;
				}
				if (vertex2 == vertex)
				{
					continue;
				}
				flag = vertex2.pt.Y > vertex.pt.Y;
			}
			bool flag2 = flag;
			vertex2 = vertex;
			for (Vertex vertex3 = vertex.next; vertex3 != vertex; vertex3 = vertex3.next)
			{
				if (vertex3.pt.Y > vertex2.pt.Y && flag)
				{
					vertex2.flags |= VertexFlags.LocalMax;
					flag = false;
				}
				else if (vertex3.pt.Y < vertex2.pt.Y && !flag)
				{
					flag = true;
					AddLocMin(vertex2, polytype, isOpen);
				}
				vertex2 = vertex3;
			}
			if (isOpen)
			{
				vertex2.flags |= VertexFlags.OpenEnd;
				if (flag)
				{
					vertex2.flags |= VertexFlags.LocalMax;
				}
				else
				{
					AddLocMin(vertex2, polytype, isOpen);
				}
			}
			else if (flag != flag2)
			{
				if (flag2)
				{
					AddLocMin(vertex2, polytype, isOpen: false);
				}
				else
				{
					vertex2.flags |= VertexFlags.LocalMax;
				}
			}
		}
	}

	public void AddSubject(List<Point64> path)
	{
		AddPath(path, PathType.Subject);
	}

	public void AddOpenSubject(List<Point64> path)
	{
		AddPath(path, PathType.Subject, isOpen: true);
	}

	public void AddClip(List<Point64> path)
	{
		AddPath(path, PathType.Clip);
	}

	protected void AddPath(List<Point64> path, PathType polytype, bool isOpen = false)
	{
		List<List<Point64>> paths = new List<List<Point64>>(1) { path };
		AddPaths(paths, polytype, isOpen);
	}

	protected void AddPaths(List<List<Point64>> paths, PathType polytype, bool isOpen = false)
	{
		if (isOpen)
		{
			_hasOpenPaths = true;
		}
		_isSortedMinimaList = false;
		AddPathsToVertexList(paths, polytype, isOpen);
	}

	private bool IsContributingClosed(Active ae)
	{
		switch (_fillrule)
		{
		case FillRule.Positive:
			if (ae.windCount != 1)
			{
				return false;
			}
			break;
		case FillRule.Negative:
			if (ae.windCount != -1)
			{
				return false;
			}
			break;
		case FillRule.NonZero:
			if (Math.Abs(ae.windCount) != 1)
			{
				return false;
			}
			break;
		}
		switch (_cliptype)
		{
		case ClipType.Intersection:
			return _fillrule switch
			{
				FillRule.Positive => ae.windCount2 > 0, 
				FillRule.Negative => ae.windCount2 < 0, 
				_ => ae.windCount2 != 0, 
			};
		case ClipType.Union:
			return _fillrule switch
			{
				FillRule.Positive => ae.windCount2 <= 0, 
				FillRule.Negative => ae.windCount2 >= 0, 
				_ => ae.windCount2 == 0, 
			};
		case ClipType.Difference:
		{
			bool flag = _fillrule switch
			{
				FillRule.Positive => ae.windCount2 <= 0, 
				FillRule.Negative => ae.windCount2 >= 0, 
				_ => ae.windCount2 == 0, 
			};
			if (GetPolyType(ae) != 0)
			{
				return !flag;
			}
			return flag;
		}
		case ClipType.Xor:
			return true;
		default:
			return false;
		}
	}

	private bool IsContributingOpen(Active ae)
	{
		bool flag;
		bool flag2;
		switch (_fillrule)
		{
		case FillRule.Positive:
			flag = ae.windCount > 0;
			flag2 = ae.windCount2 > 0;
			break;
		case FillRule.Negative:
			flag = ae.windCount < 0;
			flag2 = ae.windCount2 < 0;
			break;
		default:
			flag = ae.windCount != 0;
			flag2 = ae.windCount2 != 0;
			break;
		}
		return _cliptype switch
		{
			ClipType.Intersection => flag2, 
			ClipType.Union => !flag && !flag2, 
			_ => !flag2, 
		};
	}

	private void SetWindCountForClosedPathEdge(Active ae)
	{
		Active prevInAEL = ae.prevInAEL;
		PathType polyType = GetPolyType(ae);
		while (prevInAEL != null && (GetPolyType(prevInAEL) != polyType || IsOpen(prevInAEL)))
		{
			prevInAEL = prevInAEL.prevInAEL;
		}
		if (prevInAEL == null)
		{
			ae.windCount = ae.windDx;
			prevInAEL = _actives;
		}
		else if (_fillrule == FillRule.EvenOdd)
		{
			ae.windCount = ae.windDx;
			ae.windCount2 = prevInAEL.windCount2;
			prevInAEL = prevInAEL.nextInAEL;
		}
		else
		{
			if (prevInAEL.windCount * prevInAEL.windDx < 0)
			{
				if (Math.Abs(prevInAEL.windCount) > 1)
				{
					if (prevInAEL.windDx * ae.windDx < 0)
					{
						ae.windCount = prevInAEL.windCount;
					}
					else
					{
						ae.windCount = prevInAEL.windCount + ae.windDx;
					}
				}
				else
				{
					ae.windCount = (IsOpen(ae) ? 1 : ae.windDx);
				}
			}
			else if (prevInAEL.windDx * ae.windDx < 0)
			{
				ae.windCount = prevInAEL.windCount;
			}
			else
			{
				ae.windCount = prevInAEL.windCount + ae.windDx;
			}
			ae.windCount2 = prevInAEL.windCount2;
			prevInAEL = prevInAEL.nextInAEL;
		}
		if (_fillrule == FillRule.EvenOdd)
		{
			while (prevInAEL != ae)
			{
				if (GetPolyType(prevInAEL) != polyType && !IsOpen(prevInAEL))
				{
					ae.windCount2 = ((ae.windCount2 == 0) ? 1 : 0);
				}
				prevInAEL = prevInAEL.nextInAEL;
			}
			return;
		}
		while (prevInAEL != ae)
		{
			if (GetPolyType(prevInAEL) != polyType && !IsOpen(prevInAEL))
			{
				ae.windCount2 += prevInAEL.windDx;
			}
			prevInAEL = prevInAEL.nextInAEL;
		}
	}

	private void SetWindCountForOpenPathEdge(Active ae)
	{
		Active active = _actives;
		if (_fillrule == FillRule.EvenOdd)
		{
			int num = 0;
			int num2 = 0;
			while (active != ae)
			{
				if (GetPolyType(active) == PathType.Clip)
				{
					num2++;
				}
				else if (!IsOpen(active))
				{
					num++;
				}
				active = active.nextInAEL;
			}
			ae.windCount = (IsOdd(num) ? 1 : 0);
			ae.windCount2 = (IsOdd(num2) ? 1 : 0);
			return;
		}
		while (active != ae)
		{
			if (GetPolyType(active) == PathType.Clip)
			{
				ae.windCount2 += active.windDx;
			}
			else if (!IsOpen(active))
			{
				ae.windCount += active.windDx;
			}
			active = active.nextInAEL;
		}
	}

	private bool IsValidAelOrder(Active resident, Active newcomer)
	{
		if (newcomer.curX != resident.curX)
		{
			return newcomer.curX > resident.curX;
		}
		double num = InternalClipper.CrossProduct(resident.top, newcomer.bot, newcomer.top);
		if (num != 0.0)
		{
			return num < 0.0;
		}
		if (!IsMaxima(resident) && resident.top.Y > newcomer.top.Y)
		{
			return InternalClipper.CrossProduct(newcomer.bot, resident.top, NextVertex(resident).pt) <= 0.0;
		}
		if (!IsMaxima(newcomer) && newcomer.top.Y > resident.top.Y)
		{
			return InternalClipper.CrossProduct(newcomer.bot, newcomer.top, NextVertex(newcomer).pt) >= 0.0;
		}
		long y = newcomer.bot.Y;
		bool isLeftBound = newcomer.isLeftBound;
		if (resident.bot.Y != y || resident.localMin.vertex.pt.Y != y)
		{
			return newcomer.isLeftBound;
		}
		if (resident.isLeftBound != isLeftBound)
		{
			return isLeftBound;
		}
		if (InternalClipper.CrossProduct(PrevPrevVertex(resident).pt, resident.bot, resident.top) == 0.0)
		{
			return true;
		}
		return InternalClipper.CrossProduct(PrevPrevVertex(resident).pt, newcomer.bot, PrevPrevVertex(newcomer).pt) > 0.0 == isLeftBound;
	}

	private void InsertLeftEdge(Active ae)
	{
		if (_actives == null)
		{
			ae.prevInAEL = null;
			ae.nextInAEL = null;
			_actives = ae;
			return;
		}
		if (!IsValidAelOrder(_actives, ae))
		{
			ae.prevInAEL = null;
			ae.nextInAEL = _actives;
			_actives.prevInAEL = ae;
			_actives = ae;
			return;
		}
		Active active = _actives;
		while (active.nextInAEL != null && IsValidAelOrder(active.nextInAEL, ae))
		{
			active = active.nextInAEL;
		}
		ae.nextInAEL = active.nextInAEL;
		if (active.nextInAEL != null)
		{
			active.nextInAEL.prevInAEL = ae;
		}
		ae.prevInAEL = active;
		active.nextInAEL = ae;
	}

	private void InsertRightEdge(Active ae, Active ae2)
	{
		ae2.nextInAEL = ae.nextInAEL;
		if (ae.nextInAEL != null)
		{
			ae.nextInAEL.prevInAEL = ae2;
		}
		ae2.prevInAEL = ae;
		ae.nextInAEL = ae2;
	}

	private void InsertLocalMinimaIntoAEL(long botY)
	{
		while (HasLocMinAtY(botY))
		{
			LocalMinima localMin = PopLocalMinima();
			Active ae;
			if ((localMin.vertex.flags & VertexFlags.OpenStart) != 0)
			{
				ae = null;
			}
			else
			{
				ae = new Active
				{
					bot = localMin.vertex.pt,
					curX = localMin.vertex.pt.X,
					windDx = -1,
					vertexTop = localMin.vertex.prev,
					top = localMin.vertex.prev.pt,
					outrec = null,
					localMin = localMin
				};
				SetDx(ae);
			}
			Active ae2;
			if ((localMin.vertex.flags & VertexFlags.OpenEnd) != 0)
			{
				ae2 = null;
			}
			else
			{
				ae2 = new Active
				{
					bot = localMin.vertex.pt,
					curX = localMin.vertex.pt.X,
					windDx = 1,
					vertexTop = localMin.vertex.next,
					top = localMin.vertex.next.pt,
					outrec = null,
					localMin = localMin
				};
				SetDx(ae2);
			}
			if (ae != null && ae2 != null)
			{
				if (IsHorizontal(ae))
				{
					if (IsHeadingRightHorz(ae))
					{
						SwapActives(ref ae, ref ae2);
					}
				}
				else if (IsHorizontal(ae2))
				{
					if (IsHeadingLeftHorz(ae2))
					{
						SwapActives(ref ae, ref ae2);
					}
				}
				else if (ae.dx < ae2.dx)
				{
					SwapActives(ref ae, ref ae2);
				}
			}
			else if (ae == null)
			{
				ae = ae2;
				ae2 = null;
			}
			ae.isLeftBound = true;
			InsertLeftEdge(ae);
			bool flag;
			if (IsOpen(ae))
			{
				SetWindCountForOpenPathEdge(ae);
				flag = IsContributingOpen(ae);
			}
			else
			{
				SetWindCountForClosedPathEdge(ae);
				flag = IsContributingClosed(ae);
			}
			if (ae2 != null)
			{
				ae2.windCount = ae.windCount;
				ae2.windCount2 = ae.windCount2;
				InsertRightEdge(ae, ae2);
				if (flag)
				{
					AddLocalMinPoly(ae, ae2, ae.bot, isNew: true);
					if (!IsHorizontal(ae) && TestJoinWithPrev1(ae, botY))
					{
						OutPt op = AddOutPt(ae.prevInAEL, ae.bot);
						AddJoin(op, ae.outrec.pts);
					}
				}
				while (ae2.nextInAEL != null && IsValidAelOrder(ae2.nextInAEL, ae2))
				{
					IntersectEdges(ae2, ae2.nextInAEL, ae2.bot);
					SwapPositionsInAEL(ae2, ae2.nextInAEL);
				}
				if (!IsHorizontal(ae2) && TestJoinWithNext1(ae2, botY))
				{
					OutPt op2 = AddOutPt(ae2.nextInAEL, ae2.bot);
					AddJoin(ae2.outrec.pts, op2);
				}
				if (IsHorizontal(ae2))
				{
					PushHorz(ae2);
				}
				else
				{
					InsertScanline(ae2.top.Y);
				}
			}
			else if (flag)
			{
				StartOpenPath(ae, ae.bot);
			}
			if (IsHorizontal(ae))
			{
				PushHorz(ae);
			}
			else
			{
				InsertScanline(ae.top.Y);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PushHorz(Active ae)
	{
		ae.nextInSEL = _sel;
		_sel = ae;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool PopHorz(out Active? ae)
	{
		ae = _sel;
		if (_sel == null)
		{
			return false;
		}
		_sel = _sel.nextInSEL;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TestJoinWithPrev1(Active e, long currY)
	{
		if (IsHotEdge(e) && !IsOpen(e) && e.prevInAEL != null && e.prevInAEL.curX == e.curX && IsHotEdge(e.prevInAEL) && !IsOpen(e.prevInAEL) && currY - e.top.Y > 1 && currY - e.prevInAEL.top.Y > 1)
		{
			return InternalClipper.CrossProduct(e.prevInAEL.top, e.bot, e.top) == 0.0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TestJoinWithPrev2(Active e, Point64 currPt)
	{
		if (IsHotEdge(e) && !IsOpen(e) && e.prevInAEL != null && !IsOpen(e.prevInAEL) && IsHotEdge(e.prevInAEL) && e.prevInAEL.top.Y < e.bot.Y && Math.Abs(TopX(e.prevInAEL, currPt.Y) - currPt.X) < 2)
		{
			return InternalClipper.CrossProduct(e.prevInAEL.top, currPt, e.top) == 0.0;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TestJoinWithNext1(Active e, long currY)
	{
		if (IsHotEdge(e) && !IsOpen(e) && e.nextInAEL != null && e.nextInAEL.curX == e.curX && IsHotEdge(e.nextInAEL) && !IsOpen(e.nextInAEL) && currY - e.top.Y > 1 && currY - e.nextInAEL.top.Y > 1)
		{
			return InternalClipper.CrossProduct(e.nextInAEL.top, e.bot, e.top) == 0.0;
		}
		return false;
	}

	private bool TestJoinWithNext2(Active e, Point64 currPt)
	{
		if (IsHotEdge(e) && !IsOpen(e) && e.nextInAEL != null && !IsOpen(e.nextInAEL) && IsHotEdge(e.nextInAEL) && e.nextInAEL.top.Y < e.bot.Y && Math.Abs(TopX(e.nextInAEL, currPt.Y) - currPt.X) < 2)
		{
			return InternalClipper.CrossProduct(e.nextInAEL.top, currPt, e.top) == 0.0;
		}
		return false;
	}

	private OutPt AddLocalMinPoly(Active ae1, Active ae2, Point64 pt, bool isNew = false)
	{
		OutRec outRec = new OutRec();
		_outrecList.Add(outRec);
		outRec.idx = _outrecList.Count - 1;
		outRec.pts = null;
		outRec.polypath = null;
		ae1.outrec = outRec;
		ae2.outrec = outRec;
		if (IsOpen(ae1))
		{
			outRec.owner = null;
			outRec.isOpen = true;
			if (ae1.windDx > 0)
			{
				SetSides(outRec, ae1, ae2);
			}
			else
			{
				SetSides(outRec, ae2, ae1);
			}
		}
		else
		{
			outRec.isOpen = false;
			Active prevHotEdge = GetPrevHotEdge(ae1);
			if (prevHotEdge != null)
			{
				outRec.owner = prevHotEdge.outrec;
				if (OutrecIsAscending(prevHotEdge) == isNew)
				{
					SetSides(outRec, ae2, ae1);
				}
				else
				{
					SetSides(outRec, ae1, ae2);
				}
			}
			else
			{
				outRec.owner = null;
				if (isNew)
				{
					SetSides(outRec, ae1, ae2);
				}
				else
				{
					SetSides(outRec, ae2, ae1);
				}
			}
		}
		return outRec.pts = new OutPt(pt, outRec);
	}

	private OutPt? AddLocalMaxPoly(Active ae1, Active ae2, Point64 pt)
	{
		if (IsFront(ae1) == IsFront(ae2))
		{
			if (IsOpenEnd(ae1))
			{
				SwapFrontBackSides(ae1.outrec);
			}
			else
			{
				if (!IsOpenEnd(ae2))
				{
					_succeeded = false;
					return null;
				}
				SwapFrontBackSides(ae2.outrec);
			}
		}
		OutPt outPt = AddOutPt(ae1, pt);
		if (ae1.outrec == ae2.outrec)
		{
			OutRec outrec = ae1.outrec;
			outrec.pts = outPt;
			UncoupleOutRec(ae1);
			if (!IsOpen(ae1))
			{
				CleanCollinear(outrec);
			}
			outPt = outrec.pts;
			outrec.owner = GetRealOutRec(outrec.owner);
			if (_using_polytree && outrec.owner != null && outrec.owner.frontEdge == null)
			{
				outrec.owner = GetRealOutRec(outrec.owner.owner);
			}
		}
		else if (IsOpen(ae1))
		{
			if (ae1.windDx < 0)
			{
				JoinOutrecPaths(ae1, ae2);
			}
			else
			{
				JoinOutrecPaths(ae2, ae1);
			}
		}
		else if (ae1.outrec.idx < ae2.outrec.idx)
		{
			JoinOutrecPaths(ae1, ae2);
		}
		else
		{
			JoinOutrecPaths(ae2, ae1);
		}
		return outPt;
	}

	private void JoinOutrecPaths(Active ae1, Active ae2)
	{
		OutPt pts = ae1.outrec.pts;
		OutPt pts2 = ae2.outrec.pts;
		OutPt next = pts.next;
		OutPt next2 = pts2.next;
		if (IsFront(ae1))
		{
			next2.prev = pts;
			pts.next = next2;
			pts2.next = next;
			next.prev = pts2;
			ae1.outrec.pts = pts2;
			ae1.outrec.frontEdge = ae2.outrec.frontEdge;
			if (ae1.outrec.frontEdge != null)
			{
				ae1.outrec.frontEdge.outrec = ae1.outrec;
			}
		}
		else
		{
			next.prev = pts2;
			pts2.next = next;
			pts.next = next2;
			next2.prev = pts;
			ae1.outrec.backEdge = ae2.outrec.backEdge;
			if (ae1.outrec.backEdge != null)
			{
				ae1.outrec.backEdge.outrec = ae1.outrec;
			}
		}
		if (ae2.outrec.owner != null && ae2.outrec.owner.idx < ae1.outrec.idx && (ae1.outrec.owner == null || ae2.outrec.owner.idx < ae1.outrec.owner.idx))
		{
			ae1.outrec.owner = ae2.outrec.owner;
		}
		ae2.outrec.frontEdge = null;
		ae2.outrec.backEdge = null;
		ae2.outrec.pts = null;
		ae2.outrec.owner = ae1.outrec;
		if (IsOpenEnd(ae1))
		{
			ae2.outrec.pts = ae1.outrec.pts;
			ae1.outrec.pts = null;
		}
		ae1.outrec = null;
		ae2.outrec = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private OutPt AddOutPt(Active ae, Point64 pt)
	{
		OutRec outrec = ae.outrec;
		bool flag = IsFront(ae);
		OutPt pts = outrec.pts;
		OutPt next = pts.next;
		OutPt outPt;
		if (flag && pt == pts.pt)
		{
			outPt = pts;
		}
		else if (!flag && pt == next.pt)
		{
			outPt = next;
		}
		else
		{
			outPt = (next.prev = new OutPt(pt, outrec));
			outPt.prev = pts;
			outPt.next = next;
			pts.next = outPt;
			if (flag)
			{
				outrec.pts = outPt;
			}
		}
		return outPt;
	}

	private OutPt StartOpenPath(Active ae, Point64 pt)
	{
		OutRec outRec = new OutRec();
		_outrecList.Add(outRec);
		outRec.idx = _outrecList.Count - 1;
		outRec.owner = null;
		outRec.isOpen = true;
		outRec.pts = null;
		outRec.polypath = null;
		if (ae.windDx > 0)
		{
			outRec.frontEdge = ae;
			outRec.backEdge = null;
		}
		else
		{
			outRec.frontEdge = null;
			outRec.backEdge = ae;
		}
		ae.outrec = outRec;
		return outRec.pts = new OutPt(pt, outRec);
	}

	private void UpdateEdgeIntoAEL(Active ae)
	{
		ae.bot = ae.top;
		ae.vertexTop = NextVertex(ae);
		ae.top = ae.vertexTop.pt;
		ae.curX = ae.bot.X;
		SetDx(ae);
		if (!IsHorizontal(ae))
		{
			InsertScanline(ae.top.Y);
			if (TestJoinWithPrev1(ae, ae.bot.Y))
			{
				OutPt op = AddOutPt(ae.prevInAEL, ae.bot);
				OutPt op2 = AddOutPt(ae, ae.bot);
				AddJoin(op, op2);
			}
		}
	}

	private Active? FindEdgeWithMatchingLocMin(Active e)
	{
		Active active;
		for (active = e.nextInAEL; active != null; active = ((IsHorizontal(active) || !(e.bot != active.bot)) ? active.nextInAEL : null))
		{
			if (active.localMin == e.localMin)
			{
				return active;
			}
		}
		for (active = e.prevInAEL; active != null; active = active.prevInAEL)
		{
			if (active.localMin == e.localMin)
			{
				return active;
			}
			if (!IsHorizontal(active) && e.bot != active.bot)
			{
				return null;
			}
		}
		return active;
	}

	private OutPt? IntersectEdges(Active ae1, Active ae2, Point64 pt)
	{
		OutPt outPt = null;
		if (_hasOpenPaths && (IsOpen(ae1) || IsOpen(ae2)))
		{
			if (IsOpen(ae1) && IsOpen(ae2))
			{
				return null;
			}
			if (IsOpen(ae2))
			{
				SwapActives(ref ae1, ref ae2);
			}
			if (_cliptype == ClipType.Union)
			{
				if (!IsHotEdge(ae2))
				{
					return null;
				}
			}
			else if (ae2.localMin.polytype == PathType.Subject)
			{
				return null;
			}
			switch (_fillrule)
			{
			case FillRule.Positive:
				if (ae2.windCount != 1)
				{
					return null;
				}
				break;
			case FillRule.Negative:
				if (ae2.windCount != -1)
				{
					return null;
				}
				break;
			default:
				if (Math.Abs(ae2.windCount) != 1)
				{
					return null;
				}
				break;
			}
			if (IsHotEdge(ae1))
			{
				outPt = AddOutPt(ae1, pt);
				if (IsFront(ae1))
				{
					ae1.outrec.frontEdge = null;
				}
				else
				{
					ae1.outrec.backEdge = null;
				}
				ae1.outrec = null;
			}
			else if (pt == ae1.localMin.vertex.pt && !IsOpenEnd(ae1.localMin.vertex))
			{
				Active active = FindEdgeWithMatchingLocMin(ae1);
				if (active != null && IsHotEdge(active))
				{
					ae1.outrec = active.outrec;
					if (ae1.windDx > 0)
					{
						SetSides(active.outrec, ae1, active);
					}
					else
					{
						SetSides(active.outrec, active, ae1);
					}
					return active.outrec.pts;
				}
				outPt = StartOpenPath(ae1, pt);
			}
			else
			{
				outPt = StartOpenPath(ae1, pt);
			}
			return outPt;
		}
		int windCount;
		if (ae1.localMin.polytype == ae2.localMin.polytype)
		{
			if (_fillrule == FillRule.EvenOdd)
			{
				windCount = ae1.windCount;
				ae1.windCount = ae2.windCount;
				ae2.windCount = windCount;
			}
			else
			{
				if (ae1.windCount + ae2.windDx == 0)
				{
					ae1.windCount = -ae1.windCount;
				}
				else
				{
					ae1.windCount += ae2.windDx;
				}
				if (ae2.windCount - ae1.windDx == 0)
				{
					ae2.windCount = -ae2.windCount;
				}
				else
				{
					ae2.windCount -= ae1.windDx;
				}
			}
		}
		else
		{
			if (_fillrule != 0)
			{
				ae1.windCount2 += ae2.windDx;
			}
			else
			{
				ae1.windCount2 = ((ae1.windCount2 == 0) ? 1 : 0);
			}
			if (_fillrule != 0)
			{
				ae2.windCount2 -= ae1.windDx;
			}
			else
			{
				ae2.windCount2 = ((ae2.windCount2 == 0) ? 1 : 0);
			}
		}
		int num;
		switch (_fillrule)
		{
		case FillRule.Positive:
			windCount = ae1.windCount;
			num = ae2.windCount;
			break;
		case FillRule.Negative:
			windCount = -ae1.windCount;
			num = -ae2.windCount;
			break;
		default:
			windCount = Math.Abs(ae1.windCount);
			num = Math.Abs(ae2.windCount);
			break;
		}
		bool flag = windCount == 0 || windCount == 1;
		bool flag2 = num == 0 || num == 1;
		if ((!IsHotEdge(ae1) && !flag) || (!IsHotEdge(ae2) && !flag2))
		{
			return null;
		}
		if (IsHotEdge(ae1) && IsHotEdge(ae2))
		{
			if ((windCount != 0 && windCount != 1) || (num != 0 && num != 1) || (ae1.localMin.polytype != ae2.localMin.polytype && _cliptype != ClipType.Xor))
			{
				outPt = AddLocalMaxPoly(ae1, ae2, pt);
			}
			else if (IsFront(ae1) || ae1.outrec == ae2.outrec)
			{
				outPt = AddLocalMaxPoly(ae1, ae2, pt);
				OutPt outPt2 = AddLocalMinPoly(ae1, ae2, pt);
				if (outPt != null && outPt.pt == outPt2.pt && !IsHorizontal(ae1) && !IsHorizontal(ae2) && InternalClipper.CrossProduct(ae1.bot, outPt.pt, ae2.bot) == 0.0)
				{
					AddJoin(outPt, outPt2);
				}
			}
			else
			{
				outPt = AddOutPt(ae1, pt);
				AddOutPt(ae2, pt);
				SwapOutrecs(ae1, ae2);
			}
		}
		else if (IsHotEdge(ae1))
		{
			outPt = AddOutPt(ae1, pt);
			SwapOutrecs(ae1, ae2);
		}
		else if (IsHotEdge(ae2))
		{
			outPt = AddOutPt(ae2, pt);
			SwapOutrecs(ae1, ae2);
		}
		else
		{
			long num2;
			long num3;
			switch (_fillrule)
			{
			case FillRule.Positive:
				num2 = ae1.windCount2;
				num3 = ae2.windCount2;
				break;
			case FillRule.Negative:
				num2 = -ae1.windCount2;
				num3 = -ae2.windCount2;
				break;
			default:
				num2 = Math.Abs(ae1.windCount2);
				num3 = Math.Abs(ae2.windCount2);
				break;
			}
			if (!IsSamePolyType(ae1, ae2))
			{
				outPt = AddLocalMinPoly(ae1, ae2, pt);
			}
			else if (windCount == 1 && num == 1)
			{
				outPt = null;
				switch (_cliptype)
				{
				case ClipType.Union:
					if (num2 > 0 && num3 > 0)
					{
						return null;
					}
					outPt = AddLocalMinPoly(ae1, ae2, pt);
					break;
				case ClipType.Difference:
					if ((GetPolyType(ae1) == PathType.Clip && num2 > 0 && num3 > 0) || (GetPolyType(ae1) == PathType.Subject && num2 <= 0 && num3 <= 0))
					{
						outPt = AddLocalMinPoly(ae1, ae2, pt);
					}
					break;
				case ClipType.Xor:
					outPt = AddLocalMinPoly(ae1, ae2, pt);
					break;
				default:
					if (num2 <= 0 || num3 <= 0)
					{
						return null;
					}
					outPt = AddLocalMinPoly(ae1, ae2, pt);
					break;
				}
			}
		}
		return outPt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DeleteFromAEL(Active ae)
	{
		Active prevInAEL = ae.prevInAEL;
		Active nextInAEL = ae.nextInAEL;
		if (prevInAEL != null || nextInAEL != null || ae == _actives)
		{
			if (prevInAEL != null)
			{
				prevInAEL.nextInAEL = nextInAEL;
			}
			else
			{
				_actives = nextInAEL;
			}
			if (nextInAEL != null)
			{
				nextInAEL.prevInAEL = prevInAEL;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AdjustCurrXAndCopyToSEL(long topY)
	{
		for (Active active = (_sel = _actives); active != null; active = active.nextInAEL)
		{
			active.prevInSEL = active.prevInAEL;
			active.nextInSEL = active.nextInAEL;
			active.jump = active.nextInSEL;
			active.curX = TopX(active, topY);
		}
	}

	protected void ExecuteInternal(ClipType ct, FillRule fillRule)
	{
		if (ct == ClipType.None)
		{
			return;
		}
		_fillrule = fillRule;
		_cliptype = ct;
		Reset();
		if (!PopScanline(out var y))
		{
			return;
		}
		while (_succeeded)
		{
			InsertLocalMinimaIntoAEL(y);
			Active ae;
			while (PopHorz(out ae))
			{
				DoHorizontal(ae);
			}
			ConvertHorzTrialsToJoins();
			_currentBotY = y;
			if (!PopScanline(out y))
			{
				break;
			}
			DoIntersections(y);
			DoTopOfScanbeam(y);
			while (PopHorz(out ae))
			{
				DoHorizontal(ae);
			}
		}
		if (_succeeded)
		{
			ProcessJoinList();
		}
	}

	private void DoIntersections(long topY)
	{
		if (BuildIntersectList(topY))
		{
			ProcessIntersectList();
			DisposeIntersectNodes();
		}
	}

	private void DisposeIntersectNodes()
	{
		_intersectList.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddNewIntersectNode(Active ae1, Active ae2, long topY)
	{
		Point64 pt = GetIntersectPoint(ae1, ae2);
		if (pt.Y > _currentBotY)
		{
			pt = ((!(Math.Abs(ae1.dx) < Math.Abs(ae2.dx))) ? new Point64(TopX(ae2, _currentBotY), _currentBotY) : new Point64(TopX(ae1, _currentBotY), _currentBotY));
		}
		else if (pt.Y < topY)
		{
			pt = ((ae1.top.Y == topY) ? new Point64(ae1.top.X, topY) : ((ae2.top.Y == topY) ? new Point64(ae2.top.X, topY) : ((!(Math.Abs(ae1.dx) < Math.Abs(ae2.dx))) ? new Point64(ae2.curX, topY) : new Point64(ae1.curX, topY))));
		}
		IntersectNode item = new IntersectNode(pt, ae1, ae2);
		_intersectList.Add(item);
	}

	private Active? ExtractFromSEL(Active ae)
	{
		Active nextInSEL = ae.nextInSEL;
		if (nextInSEL != null)
		{
			nextInSEL.prevInSEL = ae.prevInSEL;
		}
		ae.prevInSEL.nextInSEL = nextInSEL;
		return nextInSEL;
	}

	private void Insert1Before2InSEL(Active ae1, Active ae2)
	{
		ae1.prevInSEL = ae2.prevInSEL;
		if (ae1.prevInSEL != null)
		{
			ae1.prevInSEL.nextInSEL = ae1;
		}
		ae1.nextInSEL = ae2;
		ae2.prevInSEL = ae1;
	}

	private bool BuildIntersectList(long topY)
	{
		if (_actives == null || _actives.nextInAEL == null)
		{
			return false;
		}
		AdjustCurrXAndCopyToSEL(topY);
		Active active = _sel;
		while (active.jump != null)
		{
			Active active2 = null;
			while (active != null && active.jump != null)
			{
				Active active3 = active;
				Active active4 = active.jump;
				Active active5 = active4;
				Active active6 = (active.jump = active4.jump);
				while (active != active5 && active4 != active6)
				{
					if (active4.curX < active.curX)
					{
						Active prevInSEL = active4.prevInSEL;
						while (true)
						{
							AddNewIntersectNode(prevInSEL, active4, topY);
							if (prevInSEL == active)
							{
								break;
							}
							prevInSEL = prevInSEL.prevInSEL;
						}
						prevInSEL = active4;
						active4 = ExtractFromSEL(prevInSEL);
						active5 = active4;
						Insert1Before2InSEL(prevInSEL, active);
						if (active == active3)
						{
							active3 = prevInSEL;
							active3.jump = active6;
							if (active2 == null)
							{
								_sel = active3;
							}
							else
							{
								active2.jump = active3;
							}
						}
					}
					else
					{
						active = active.nextInSEL;
					}
				}
				active2 = active3;
				active = active6;
			}
			active = _sel;
		}
		return _intersectList.Count > 0;
	}

	private void ProcessIntersectList()
	{
		_intersectList.Sort(default(IntersectListSort));
		for (int i = 0; i < _intersectList.Count; i++)
		{
			if (!EdgesAdjacentInAEL(_intersectList[i]))
			{
				int j;
				for (j = i + 1; j < _intersectList.Count && !EdgesAdjacentInAEL(_intersectList[j]); j++)
				{
				}
				if (j < _intersectList.Count)
				{
					List<IntersectNode> intersectList = _intersectList;
					int index = j;
					List<IntersectNode> intersectList2 = _intersectList;
					int index2 = i;
					IntersectNode intersectNode = _intersectList[i];
					IntersectNode intersectNode2 = _intersectList[j];
					IntersectNode intersectNode4 = (intersectList[index] = intersectNode);
					intersectNode4 = (intersectList2[index2] = intersectNode2);
				}
			}
			IntersectNode intersectNode6 = _intersectList[i];
			IntersectEdges(intersectNode6.edge1, intersectNode6.edge2, intersectNode6.pt);
			SwapPositionsInAEL(intersectNode6.edge1, intersectNode6.edge2);
			if (TestJoinWithPrev2(intersectNode6.edge2, intersectNode6.pt))
			{
				OutPt outPt = AddOutPt(intersectNode6.edge2.prevInAEL, intersectNode6.pt);
				OutPt outPt2 = AddOutPt(intersectNode6.edge2, intersectNode6.pt);
				if (outPt != outPt2)
				{
					AddJoin(outPt, outPt2);
				}
			}
			else if (TestJoinWithNext2(intersectNode6.edge1, intersectNode6.pt))
			{
				OutPt outPt3 = AddOutPt(intersectNode6.edge1, intersectNode6.pt);
				OutPt outPt4 = AddOutPt(intersectNode6.edge1.nextInAEL, intersectNode6.pt);
				if (outPt3 != outPt4)
				{
					AddJoin(outPt3, outPt4);
				}
			}
		}
	}

	private void SwapPositionsInAEL(Active ae1, Active ae2)
	{
		Active nextInAEL = ae2.nextInAEL;
		if (nextInAEL != null)
		{
			nextInAEL.prevInAEL = ae1;
		}
		Active prevInAEL = ae1.prevInAEL;
		if (prevInAEL != null)
		{
			prevInAEL.nextInAEL = ae2;
		}
		ae2.prevInAEL = prevInAEL;
		ae2.nextInAEL = ae1;
		ae1.prevInAEL = ae2;
		ae1.nextInAEL = nextInAEL;
		if (ae2.prevInAEL == null)
		{
			_actives = ae2;
		}
	}

	private bool ResetHorzDirection(Active horz, Active? maxPair, out long leftX, out long rightX)
	{
		if (horz.bot.X == horz.top.X)
		{
			leftX = horz.curX;
			rightX = horz.curX;
			Active nextInAEL = horz.nextInAEL;
			while (nextInAEL != null && nextInAEL != maxPair)
			{
				nextInAEL = nextInAEL.nextInAEL;
			}
			return nextInAEL != null;
		}
		if (horz.curX < horz.top.X)
		{
			leftX = horz.curX;
			rightX = horz.top.X;
			return true;
		}
		leftX = horz.top.X;
		rightX = horz.curX;
		return false;
	}

	private bool HorzIsSpike(Active horz)
	{
		Point64 pt = NextVertex(horz).pt;
		return horz.bot.X < horz.top.X != horz.top.X < pt.X;
	}

	private bool TrimHorz(Active horzEdge, bool preserveCollinear)
	{
		bool flag = false;
		Point64 pt = NextVertex(horzEdge).pt;
		while (pt.Y == horzEdge.top.Y && (!preserveCollinear || pt.X < horzEdge.top.X == horzEdge.bot.X < horzEdge.top.X))
		{
			horzEdge.vertexTop = NextVertex(horzEdge);
			horzEdge.top = pt;
			flag = true;
			if (IsMaxima(horzEdge))
			{
				break;
			}
			pt = NextVertex(horzEdge).pt;
		}
		if (flag)
		{
			SetDx(horzEdge);
		}
		return flag;
	}

	private void DoHorizontal(Active horz)
	{
		bool flag = IsOpen(horz);
		long y = horz.bot.Y;
		Vertex vertex = null;
		Active active = null;
		if (!flag)
		{
			vertex = GetCurrYMaximaVertex(horz);
			if (vertex != null)
			{
				active = GetHorzMaximaPair(horz, vertex);
				if (vertex != horz.vertexTop)
				{
					TrimHorz(horz, PreserveCollinear);
				}
			}
		}
		long leftX;
		long rightX;
		bool flag2 = ResetHorzDirection(horz, active, out leftX, out rightX);
		if (IsHotEdge(horz))
		{
			AddOutPt(horz, new Point64(horz.curX, y));
		}
		OutPt outPt;
		while (true)
		{
			if (flag && IsMaxima(horz) && !IsOpenEnd(horz))
			{
				vertex = GetCurrYMaximaVertex(horz);
				if (vertex != null)
				{
					active = GetHorzMaximaPair(horz, vertex);
				}
			}
			Active active2 = ((!flag2) ? horz.prevInAEL : horz.nextInAEL);
			while (active2 != null)
			{
				if (active2 == active)
				{
					if (IsHotEdge(horz))
					{
						while (horz.vertexTop != active2.vertexTop)
						{
							AddOutPt(horz, horz.top);
							UpdateEdgeIntoAEL(horz);
						}
						outPt = AddLocalMaxPoly(horz, active2, horz.top);
						if (outPt != null && !IsOpen(horz) && outPt.pt == horz.top)
						{
							AddTrialHorzJoin(outPt);
						}
					}
					DeleteFromAEL(active2);
					DeleteFromAEL(horz);
					return;
				}
				Point64 pt;
				if (vertex != horz.vertexTop || IsOpenEnd(horz))
				{
					if ((flag2 && active2.curX > rightX) || (!flag2 && active2.curX < leftX))
					{
						break;
					}
					if (active2.curX == horz.top.X && !IsHorizontal(active2))
					{
						pt = NextVertex(horz).pt;
						if (flag2)
						{
							if (IsOpen(active2) && !IsSamePolyType(active2, horz) && !IsHotEdge(active2))
							{
								if (TopX(active2, pt.Y) > pt.X)
								{
									break;
								}
							}
							else if (TopX(active2, pt.Y) >= pt.X)
							{
								break;
							}
						}
						else if (IsOpen(active2) && !IsSamePolyType(active2, horz) && !IsHotEdge(active2))
						{
							if (TopX(active2, pt.Y) < pt.X)
							{
								break;
							}
						}
						else if (TopX(active2, pt.Y) <= pt.X)
						{
							break;
						}
					}
				}
				pt = new Point64(active2.curX, y);
				if (flag2)
				{
					outPt = IntersectEdges(horz, active2, pt);
					SwapPositionsInAEL(horz, active2);
					if (IsHotEdge(horz) && outPt != null && !IsOpen(horz) && outPt.pt == pt)
					{
						AddTrialHorzJoin(outPt);
					}
					if (!IsHorizontal(active2) && TestJoinWithPrev1(active2, y))
					{
						outPt = AddOutPt(active2.prevInAEL, pt);
						OutPt op = AddOutPt(active2, pt);
						AddJoin(outPt, op);
					}
					horz.curX = active2.curX;
					active2 = horz.nextInAEL;
				}
				else
				{
					outPt = IntersectEdges(active2, horz, pt);
					SwapPositionsInAEL(active2, horz);
					if (IsHotEdge(horz) && outPt != null && !IsOpen(horz) && outPt.pt == pt)
					{
						AddTrialHorzJoin(outPt);
					}
					if (!IsHorizontal(active2) && TestJoinWithNext1(active2, y))
					{
						outPt = AddOutPt(active2, pt);
						OutPt op2 = AddOutPt(active2.nextInAEL, pt);
						AddJoin(outPt, op2);
					}
					horz.curX = active2.curX;
					active2 = horz.prevInAEL;
				}
			}
			if (flag && IsOpenEnd(horz))
			{
				if (IsHotEdge(horz))
				{
					AddOutPt(horz, horz.top);
					if (IsFront(horz))
					{
						horz.outrec.frontEdge = null;
					}
					else
					{
						horz.outrec.backEdge = null;
					}
				}
				horz.outrec = null;
				DeleteFromAEL(horz);
				return;
			}
			if (NextVertex(horz).pt.Y != horz.top.Y)
			{
				break;
			}
			if (IsHotEdge(horz))
			{
				AddOutPt(horz, horz.top);
			}
			UpdateEdgeIntoAEL(horz);
			if (PreserveCollinear && HorzIsSpike(horz))
			{
				TrimHorz(horz, preserveCollinear: true);
			}
			flag2 = ResetHorzDirection(horz, active, out leftX, out rightX);
		}
		if (IsHotEdge(horz))
		{
			outPt = AddOutPt(horz, horz.top);
			if (!IsOpen(horz))
			{
				AddTrialHorzJoin(outPt);
			}
		}
		else
		{
			outPt = null;
		}
		if ((flag && !IsOpenEnd(horz)) || (!flag && vertex != horz.vertexTop))
		{
			UpdateEdgeIntoAEL(horz);
			if (!IsOpen(horz))
			{
				if (flag2 && TestJoinWithNext1(horz, y))
				{
					OutPt op3 = AddOutPt(horz.nextInAEL, horz.bot);
					AddJoin(outPt, op3);
				}
				else if (!flag2 && TestJoinWithPrev1(horz, y))
				{
					OutPt op4 = AddOutPt(horz.prevInAEL, horz.bot);
					AddJoin(op4, outPt);
				}
			}
		}
		else if (IsHotEdge(horz))
		{
			AddLocalMaxPoly(horz, active, horz.top);
		}
		else
		{
			DeleteFromAEL(active);
			DeleteFromAEL(horz);
		}
	}

	private void DoTopOfScanbeam(long y)
	{
		_sel = null;
		Active active = _actives;
		while (active != null)
		{
			if (active.top.Y == y)
			{
				active.curX = active.top.X;
				if (IsMaxima(active))
				{
					active = DoMaxima(active);
					continue;
				}
				if (IsHotEdge(active))
				{
					AddOutPt(active, active.top);
				}
				UpdateEdgeIntoAEL(active);
				if (IsHorizontal(active))
				{
					PushHorz(active);
				}
			}
			else
			{
				active.curX = TopX(active, y);
			}
			active = active.nextInAEL;
		}
	}

	private Active? DoMaxima(Active ae)
	{
		Active prevInAEL = ae.prevInAEL;
		Active nextInAEL = ae.nextInAEL;
		if (IsOpenEnd(ae))
		{
			if (IsHotEdge(ae))
			{
				AddOutPt(ae, ae.top);
			}
			if (!IsHorizontal(ae))
			{
				if (IsHotEdge(ae))
				{
					if (IsFront(ae))
					{
						ae.outrec.frontEdge = null;
					}
					else
					{
						ae.outrec.backEdge = null;
					}
					ae.outrec = null;
				}
				DeleteFromAEL(ae);
			}
			return nextInAEL;
		}
		Active maximaPair = GetMaximaPair(ae);
		if (maximaPair == null)
		{
			return nextInAEL;
		}
		while (nextInAEL != maximaPair)
		{
			IntersectEdges(ae, nextInAEL, ae.top);
			SwapPositionsInAEL(ae, nextInAEL);
			nextInAEL = ae.nextInAEL;
		}
		if (IsOpen(ae))
		{
			if (IsHotEdge(ae))
			{
				AddLocalMaxPoly(ae, maximaPair, ae.top);
			}
			DeleteFromAEL(maximaPair);
			DeleteFromAEL(ae);
			if (prevInAEL == null)
			{
				return _actives;
			}
			return prevInAEL.nextInAEL;
		}
		if (IsHotEdge(ae))
		{
			AddLocalMaxPoly(ae, maximaPair, ae.top);
		}
		DeleteFromAEL(ae);
		DeleteFromAEL(maximaPair);
		if (prevInAEL == null)
		{
			return _actives;
		}
		return prevInAEL.nextInAEL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsValidPath(OutPt op)
	{
		return op.next != op;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool AreReallyClose(Point64 pt1, Point64 pt2)
	{
		if (Math.Abs(pt1.X - pt2.X) < 2)
		{
			return Math.Abs(pt1.Y - pt2.Y) < 2;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsValidClosedPath(OutPt? op)
	{
		if (op != null && op.next != op && op.next != op.prev)
		{
			if (op.next.next == op.prev)
			{
				if (!AreReallyClose(op.pt, op.next.pt))
				{
					return !AreReallyClose(op.pt, op.prev.pt);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ValueBetween(long val, long end1, long end2)
	{
		if (val != end1 == (val != end2))
		{
			return val > end1 == val < end2;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ValueEqualOrBetween(long val, long end1, long end2)
	{
		if (val != end1 && val != end2)
		{
			return val > end1 == val < end2;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool PointBetween(Point64 pt, Point64 corner1, Point64 corner2)
	{
		if (ValueEqualOrBetween(pt.X, corner1.X, corner2.X))
		{
			return ValueEqualOrBetween(pt.Y, corner1.Y, corner2.Y);
		}
		return false;
	}

	private static bool CollinearSegsOverlap(Point64 seg1a, Point64 seg1b, Point64 seg2a, Point64 seg2b)
	{
		if (seg1a.X == seg1b.X)
		{
			if (seg2a.X != seg1a.X || seg2a.X != seg2b.X)
			{
				return false;
			}
		}
		else if (seg1a.X < seg1b.X)
		{
			if (seg2a.X < seg2b.X)
			{
				if (seg2a.X >= seg1b.X || seg2b.X <= seg1a.X)
				{
					return false;
				}
			}
			else if (seg2b.X >= seg1b.X || seg2a.X <= seg1a.X)
			{
				return false;
			}
		}
		else if (seg2a.X < seg2b.X)
		{
			if (seg2a.X >= seg1a.X || seg2b.X <= seg1b.X)
			{
				return false;
			}
		}
		else if (seg2b.X >= seg1a.X || seg2a.X <= seg1b.X)
		{
			return false;
		}
		if (seg1a.Y == seg1b.Y)
		{
			if (seg2a.Y != seg1a.Y || seg2a.Y != seg2b.Y)
			{
				return false;
			}
		}
		else if (seg1a.Y < seg1b.Y)
		{
			if (seg2a.Y < seg2b.Y)
			{
				if (seg2a.Y >= seg1b.Y || seg2b.Y <= seg1a.Y)
				{
					return false;
				}
			}
			else if (seg2b.Y >= seg1b.Y || seg2a.Y <= seg1a.Y)
			{
				return false;
			}
		}
		else if (seg2a.Y < seg2b.Y)
		{
			if (seg2a.Y >= seg1a.Y || seg2b.Y <= seg1b.Y)
			{
				return false;
			}
		}
		else if (seg2b.Y >= seg1a.Y || seg2a.Y <= seg1b.Y)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool HorzEdgesOverlap(long x1a, long x1b, long x2a, long x2b)
	{
		if (x1a > x1b + 2)
		{
			if (x2a > x2b + 2)
			{
				if (x1a > x2b)
				{
					return x2a > x1b;
				}
				return false;
			}
			if (x1a > x2a)
			{
				return x2b > x1b;
			}
			return false;
		}
		if (x1b > x1a + 2)
		{
			if (x2a > x2b + 2)
			{
				if (x1b > x2b)
				{
					return x2a > x1a;
				}
				return false;
			}
			if (x1b > x2a)
			{
				return x2b > x1a;
			}
			return false;
		}
		return false;
	}

	private Joiner? GetHorzTrialParent(OutPt op)
	{
		Joiner joiner = op.joiner;
		while (joiner != null)
		{
			if (joiner.op1 == op)
			{
				if (joiner.next1 != null && joiner.next1.idx < 0)
				{
					return joiner;
				}
				joiner = joiner.next1;
			}
			else
			{
				if (joiner.next2 != null && joiner.next2.idx < 0)
				{
					return joiner;
				}
				joiner = joiner.next1;
			}
		}
		return joiner;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool OutPtInTrialHorzList(OutPt op)
	{
		if (op.joiner != null)
		{
			if (op.joiner.idx >= 0)
			{
				return GetHorzTrialParent(op) != null;
			}
			return true;
		}
		return false;
	}

	private bool ValidateClosedPathEx(ref OutPt? op)
	{
		if (IsValidClosedPath(op))
		{
			return true;
		}
		if (op != null)
		{
			SafeDisposeOutPts(ref op);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static OutPt InsertOp(Point64 pt, OutPt insertAfter)
	{
		OutPt outPt = new OutPt(pt, insertAfter.outrec)
		{
			next = insertAfter.next
		};
		insertAfter.next.prev = outPt;
		insertAfter.next = outPt;
		outPt.prev = insertAfter;
		return outPt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static OutPt? DisposeOutPt(OutPt op)
	{
		OutPt? result = ((op.next == op) ? null : op.next);
		op.prev.next = op.next;
		op.next.prev = op.prev;
		return result;
	}

	private void SafeDisposeOutPts(ref OutPt op)
	{
		OutRec realOutRec = GetRealOutRec(op.outrec);
		if (realOutRec.frontEdge != null)
		{
			realOutRec.frontEdge.outrec = null;
		}
		if (realOutRec.backEdge != null)
		{
			realOutRec.backEdge.outrec = null;
		}
		op.prev.next = null;
		for (OutPt outPt = op; outPt != null; outPt = outPt.next)
		{
			SafeDeleteOutPtJoiners(outPt);
		}
		realOutRec.pts = null;
	}

	private void SafeDeleteOutPtJoiners(OutPt op)
	{
		Joiner joiner = op.joiner;
		if (joiner == null)
		{
			return;
		}
		while (joiner != null)
		{
			if (joiner.idx < 0)
			{
				DeleteTrialHorzJoin(op);
			}
			else if (_horzJoiners != null)
			{
				if (OutPtInTrialHorzList(joiner.op1))
				{
					DeleteTrialHorzJoin(joiner.op1);
				}
				if (OutPtInTrialHorzList(joiner.op2))
				{
					DeleteTrialHorzJoin(joiner.op2);
				}
				DeleteJoin(joiner);
			}
			else
			{
				DeleteJoin(joiner);
			}
			joiner = op.joiner;
		}
	}

	private void AddTrialHorzJoin(OutPt op)
	{
		if (!op.outrec.isOpen && !OutPtInTrialHorzList(op))
		{
			_horzJoiners = new Joiner(null, op, null, _horzJoiners);
		}
	}

	private static Joiner? FindTrialJoinParent(ref Joiner joiner, OutPt op)
	{
		Joiner joiner2 = joiner;
		while (joiner2 != null)
		{
			if (op == joiner2.op1)
			{
				if (joiner2.next1 != null && joiner2.next1.idx < 0)
				{
					joiner = joiner2.next1;
					return joiner2;
				}
				joiner2 = joiner2.next1;
			}
			else
			{
				if (joiner2.next2 != null && joiner2.next2.idx < 0)
				{
					joiner = joiner2.next2;
					return joiner2;
				}
				joiner2 = joiner2.next2;
			}
		}
		return null;
	}

	private void DeleteTrialHorzJoin(OutPt op)
	{
		if (_horzJoiners == null)
		{
			return;
		}
		Joiner joiner = op.joiner;
		Joiner joiner2 = null;
		while (joiner != null)
		{
			if (joiner.idx < 0)
			{
				if (joiner == _horzJoiners)
				{
					_horzJoiners = joiner.nextH;
				}
				else
				{
					Joiner joiner3 = _horzJoiners;
					while (joiner3.nextH != joiner)
					{
						joiner3 = joiner3.nextH;
					}
					joiner3.nextH = joiner.nextH;
				}
				if (joiner2 == null)
				{
					op.joiner = joiner.next1;
					joiner = op.joiner;
					continue;
				}
				if (op == joiner2.op1)
				{
					joiner2.next1 = joiner.next1;
				}
				else
				{
					joiner2.next2 = joiner.next1;
				}
				joiner = joiner2;
			}
			else
			{
				joiner2 = FindTrialJoinParent(ref joiner, op);
				if (joiner2 == null)
				{
					break;
				}
			}
		}
	}

	private bool GetHorzExtendedHorzSeg(ref OutPt op, out OutPt op2)
	{
		OutRec realOutRec = GetRealOutRec(op.outrec);
		op2 = op;
		if (realOutRec.frontEdge != null)
		{
			while (op.prev != realOutRec.pts && op.prev.pt.Y == op.pt.Y)
			{
				op = op.prev;
			}
			while (op2 != realOutRec.pts && op2.next.pt.Y == op2.pt.Y)
			{
				op2 = op2.next;
			}
			return op2 != op;
		}
		while (op.prev != op2 && op.prev.pt.Y == op.pt.Y)
		{
			op = op.prev;
		}
		while (op2.next != op && op2.next.pt.Y == op2.pt.Y)
		{
			op2 = op2.next;
		}
		if (op2 != op)
		{
			return op2.next != op;
		}
		return false;
	}

	private void ConvertHorzTrialsToJoins()
	{
		while (_horzJoiners != null)
		{
			Joiner horzJoiners = _horzJoiners;
			_horzJoiners = _horzJoiners.nextH;
			OutPt op = horzJoiners.op1;
			if (op.joiner == horzJoiners)
			{
				op.joiner = horzJoiners.next1;
			}
			else
			{
				Joiner joiner = FindJoinParent(horzJoiners, op);
				if (joiner.op1 == op)
				{
					joiner.next1 = horzJoiners.next1;
				}
				else
				{
					joiner.next2 = horzJoiners.next1;
				}
			}
			if (!GetHorzExtendedHorzSeg(ref op, out OutPt op2))
			{
				if (op.outrec.frontEdge == null)
				{
					CleanCollinear(op.outrec);
				}
				continue;
			}
			bool flag = false;
			for (horzJoiners = _horzJoiners; horzJoiners != null; horzJoiners = horzJoiners.nextH)
			{
				OutPt op3 = horzJoiners.op1;
				if (GetHorzExtendedHorzSeg(ref op3, out OutPt op4) && HorzEdgesOverlap(op.pt.X, op2.pt.X, op3.pt.X, op4.pt.X))
				{
					flag = true;
					if (op.pt == op4.pt)
					{
						AddJoin(op, op4);
					}
					else if (op2.pt == op3.pt)
					{
						AddJoin(op2, op3);
					}
					else if (op.pt == op3.pt)
					{
						AddJoin(op, op3);
					}
					else if (op2.pt == op4.pt)
					{
						AddJoin(op2, op4);
					}
					else if (ValueBetween(op.pt.X, op3.pt.X, op4.pt.X))
					{
						AddJoin(op, InsertOp(op.pt, op3));
					}
					else if (ValueBetween(op2.pt.X, op3.pt.X, op4.pt.X))
					{
						AddJoin(op2, InsertOp(op2.pt, op3));
					}
					else if (ValueBetween(op3.pt.X, op.pt.X, op2.pt.X))
					{
						AddJoin(op3, InsertOp(op3.pt, op));
					}
					else if (ValueBetween(op4.pt.X, op.pt.X, op2.pt.X))
					{
						AddJoin(op4, InsertOp(op4.pt, op));
					}
					break;
				}
			}
			if (!flag)
			{
				CleanCollinear(op.outrec);
			}
		}
	}

	private void AddJoin(OutPt op1, OutPt op2)
	{
		if (op1.outrec != op2.outrec || (op1 != op2 && (op1.next != op2 || op1 == op1.outrec.pts) && (op2.next != op1 || op2 == op1.outrec.pts)))
		{
			new Joiner(_joinerList, op1, op2, null);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Joiner FindJoinParent(Joiner joiner, OutPt op)
	{
		Joiner joiner2 = op.joiner;
		while (true)
		{
			if (op == joiner2.op1)
			{
				if (joiner2.next1 == joiner)
				{
					return joiner2;
				}
				joiner2 = joiner2.next1;
			}
			else
			{
				if (joiner2.next2 == joiner)
				{
					break;
				}
				joiner2 = joiner2.next2;
			}
		}
		return joiner2;
	}

	private void DeleteJoin(Joiner joiner)
	{
		OutPt op = joiner.op1;
		OutPt op2 = joiner.op2;
		if (op.joiner != joiner)
		{
			Joiner joiner2 = FindJoinParent(joiner, op);
			if (joiner2.op1 == op)
			{
				joiner2.next1 = joiner.next1;
			}
			else
			{
				joiner2.next2 = joiner.next1;
			}
		}
		else
		{
			op.joiner = joiner.next1;
		}
		if (op2.joiner != joiner)
		{
			Joiner joiner2 = FindJoinParent(joiner, op2);
			if (joiner2.op1 == op2)
			{
				joiner2.next1 = joiner.next2;
			}
			else
			{
				joiner2.next2 = joiner.next2;
			}
		}
		else
		{
			op2.joiner = joiner.next2;
		}
		_joinerList[joiner.idx] = null;
	}

	private void ProcessJoinList()
	{
		for (int i = 0; i < _joinerList.Count; i++)
		{
			Joiner joiner = _joinerList[i];
			if (joiner != null)
			{
				OutRec outrec = ProcessJoin(joiner);
				CleanCollinear(outrec);
			}
		}
		_joinerList.Clear();
	}

	private static bool CheckDisposeAdjacent(ref OutPt op, OutPt guard, OutRec outRec)
	{
		bool result = false;
		while (op.prev != op && op.pt == op.prev.pt && op != guard && op.prev.joiner != null && op.joiner == null)
		{
			if (op == outRec.pts)
			{
				outRec.pts = op.prev;
			}
			op = DisposeOutPt(op);
			op = op.prev;
		}
		while (op.next != op && op.pt == op.next.pt && op != guard && op.next.joiner != null && op.joiner == null)
		{
			if (op == outRec.pts)
			{
				outRec.pts = op.prev;
			}
			op = DisposeOutPt(op);
			op = op.prev;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double DistanceFromLineSqrd(Point64 pt, Point64 linePt1, Point64 linePt2)
	{
		double num = linePt1.Y - linePt2.Y;
		double num2 = linePt2.X - linePt1.X;
		double num3 = num * (double)linePt1.X + num2 * (double)linePt1.Y;
		double num4 = num * (double)pt.X + num2 * (double)pt.Y - num3;
		return num4 * num4 / (num * num + num2 * num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static double DistanceSqr(Point64 pt1, Point64 pt2)
	{
		return (double)(pt1.X - pt2.X) * (double)(pt1.X - pt2.X) + (double)(pt1.Y - pt2.Y) * (double)(pt1.Y - pt2.Y);
	}

	private OutRec ProcessJoin(Joiner j)
	{
		OutPt op = j.op1;
		OutPt op2 = j.op2;
		OutRec realOutRec = GetRealOutRec(op.outrec);
		OutRec realOutRec2 = GetRealOutRec(op2.outrec);
		DeleteJoin(j);
		if (realOutRec2.pts == null)
		{
			return realOutRec;
		}
		if (!IsValidClosedPath(op2))
		{
			SafeDisposeOutPts(ref op2);
			return realOutRec;
		}
		if (realOutRec.pts == null || !IsValidClosedPath(op))
		{
			SafeDisposeOutPts(ref op);
			return realOutRec2;
		}
		if (realOutRec == realOutRec2 && (op == op2 || op.next == op2 || op.prev == op2))
		{
			return realOutRec;
		}
		CheckDisposeAdjacent(ref op, op2, realOutRec);
		CheckDisposeAdjacent(ref op2, op, realOutRec2);
		if (op.next == op2 || op2.next == op)
		{
			return realOutRec;
		}
		OutRec result = realOutRec;
		while (true)
		{
			if (!IsValidPath(op) || !IsValidPath(op2) || (realOutRec == realOutRec2 && (op.prev == op2 || op.next == op2)))
			{
				return realOutRec;
			}
			if (op.prev.pt == op2.next.pt || (InternalClipper.CrossProduct(op.prev.pt, op.pt, op2.next.pt) == 0.0 && CollinearSegsOverlap(op.prev.pt, op.pt, op2.pt, op2.next.pt)))
			{
				if (realOutRec == realOutRec2)
				{
					if (op.prev.pt != op2.next.pt)
					{
						if (PointBetween(op.prev.pt, op2.pt, op2.next.pt))
						{
							op2.next = InsertOp(op.prev.pt, op2);
						}
						else
						{
							op.prev = InsertOp(op2.next.pt, op.prev);
						}
					}
					OutPt prev = op.prev;
					(prev.next = op2.next).prev = prev;
					op.prev = op2;
					op2.next = op;
					CompleteSplit(op, prev, realOutRec);
					break;
				}
				OutPt prev2 = op.prev;
				(prev2.next = op2.next).prev = prev2;
				op.prev = op2;
				op2.next = op;
				if (realOutRec.idx < realOutRec2.idx)
				{
					realOutRec.pts = op;
					realOutRec2.pts = null;
					if (realOutRec.owner != null && (realOutRec2.owner == null || realOutRec2.owner.idx < realOutRec.owner.idx))
					{
						realOutRec.owner = realOutRec2.owner;
					}
					realOutRec2.owner = realOutRec;
					break;
				}
				result = realOutRec2;
				realOutRec2.pts = op;
				realOutRec.pts = null;
				if (realOutRec2.owner != null && (realOutRec.owner == null || realOutRec.owner.idx < realOutRec2.owner.idx))
				{
					realOutRec2.owner = realOutRec.owner;
				}
				realOutRec.owner = realOutRec2;
				break;
			}
			if (op.next.pt == op2.prev.pt || (InternalClipper.CrossProduct(op.next.pt, op2.pt, op2.prev.pt) == 0.0 && CollinearSegsOverlap(op.next.pt, op.pt, op2.pt, op2.prev.pt)))
			{
				if (realOutRec == realOutRec2)
				{
					if (op2.prev.pt != op.next.pt)
					{
						if (PointBetween(op2.prev.pt, op.pt, op.next.pt))
						{
							op.next = InsertOp(op2.prev.pt, op);
						}
						else
						{
							op2.prev = InsertOp(op.next.pt, op2.prev);
						}
					}
					OutPt prev3 = op2.prev;
					(prev3.next = op.next).prev = prev3;
					op2.prev = op;
					op.next = op2;
					CompleteSplit(op, prev3, realOutRec);
					break;
				}
				OutPt next = op.next;
				(next.prev = op2.prev).next = next;
				op.next = op2;
				op2.prev = op;
				if (realOutRec.idx < realOutRec2.idx)
				{
					realOutRec.pts = op;
					realOutRec2.pts = null;
					if (realOutRec.owner != null && (realOutRec2.owner == null || realOutRec2.owner.idx < realOutRec.owner.idx))
					{
						realOutRec.owner = realOutRec2.owner;
					}
					realOutRec2.owner = realOutRec;
					break;
				}
				result = realOutRec2;
				realOutRec2.pts = op;
				realOutRec.pts = null;
				if (realOutRec2.owner != null && (realOutRec.owner == null || realOutRec.owner.idx < realOutRec2.owner.idx))
				{
					realOutRec2.owner = realOutRec.owner;
				}
				realOutRec.owner = realOutRec2;
				break;
			}
			if (PointBetween(op.next.pt, op2.pt, op2.prev.pt) && DistanceFromLineSqrd(op.next.pt, op2.pt, op2.prev.pt) < 2.01)
			{
				InsertOp(op.next.pt, op2.prev);
			}
			else if (PointBetween(op2.next.pt, op.pt, op.prev.pt) && DistanceFromLineSqrd(op2.next.pt, op.pt, op.prev.pt) < 2.01)
			{
				InsertOp(op2.next.pt, op.prev);
			}
			else if (PointBetween(op.prev.pt, op2.pt, op2.next.pt) && DistanceFromLineSqrd(op.prev.pt, op2.pt, op2.next.pt) < 2.01)
			{
				InsertOp(op.prev.pt, op2);
			}
			else if (PointBetween(op2.prev.pt, op.pt, op.next.pt) && DistanceFromLineSqrd(op2.prev.pt, op.pt, op.next.pt) < 2.01)
			{
				InsertOp(op2.prev.pt, op);
			}
			else
			{
				if (CheckDisposeAdjacent(ref op, op2, realOutRec) || CheckDisposeAdjacent(ref op2, op, realOutRec))
				{
					continue;
				}
				if (op.prev.pt != op2.next.pt && DistanceSqr(op.prev.pt, op2.next.pt) < 2.01)
				{
					op.prev.pt = op2.next.pt;
					continue;
				}
				if (!(op.next.pt != op2.prev.pt) || !(DistanceSqr(op.next.pt, op2.prev.pt) < 2.01))
				{
					realOutRec.pts = op;
					if (realOutRec2 != realOutRec)
					{
						realOutRec2.pts = op2;
						CleanCollinear(realOutRec2);
					}
					break;
				}
				op2.prev.pt = op.next.pt;
			}
		}
		return result;
	}

	private static void UpdateOutrecOwner(OutRec outrec)
	{
		OutPt outPt = outrec.pts;
		do
		{
			outPt.outrec = outrec;
			outPt = outPt.next;
		}
		while (outPt != outrec.pts);
	}

	private void CompleteSplit(OutPt? op1, OutPt? op2, OutRec outrec)
	{
		double num = Area(op1);
		double num2 = Area(op2);
		bool flag = num > 0.0 == num2 < 0.0;
		if (num == 0.0 || (flag && Math.Abs(num) < 2.0))
		{
			SafeDisposeOutPts(ref op1);
			outrec.pts = op2;
			return;
		}
		if (num2 == 0.0 || (flag && Math.Abs(num2) < 2.0))
		{
			SafeDisposeOutPts(ref op2);
			outrec.pts = op1;
			return;
		}
		OutRec outRec = new OutRec
		{
			idx = _outrecList.Count
		};
		_outrecList.Add(outRec);
		outRec.polypath = null;
		if (_using_polytree)
		{
			if (outrec.splits == null)
			{
				outrec.splits = new List<OutRec>();
			}
			outrec.splits.Add(outRec);
		}
		if (Math.Abs(num) >= Math.Abs(num2))
		{
			outrec.pts = op1;
			outRec.pts = op2;
		}
		else
		{
			outrec.pts = op2;
			outRec.pts = op1;
		}
		if (num > 0.0 == num2 > 0.0)
		{
			outRec.owner = outrec.owner;
		}
		else
		{
			outRec.owner = outrec;
		}
		UpdateOutrecOwner(outRec);
		CleanCollinear(outRec);
	}

	private void CleanCollinear(OutRec? outrec)
	{
		outrec = GetRealOutRec(outrec);
		if (outrec == null || outrec.isOpen || outrec.frontEdge != null || !ValidateClosedPathEx(ref outrec.pts))
		{
			return;
		}
		OutPt outPt = outrec.pts;
		OutPt op = outPt;
		while (true)
		{
			if (op.joiner != null)
			{
				return;
			}
			if (InternalClipper.CrossProduct(op.prev.pt, op.pt, op.next.pt) == 0.0 && (op.pt == op.prev.pt || op.pt == op.next.pt || !PreserveCollinear || InternalClipper.DotProduct(op.prev.pt, op.pt, op.next.pt) < 0.0))
			{
				if (op == outrec.pts)
				{
					outrec.pts = op.prev;
				}
				op = DisposeOutPt(op);
				if (!ValidateClosedPathEx(ref op))
				{
					outrec.pts = null;
					return;
				}
				outPt = op;
			}
			else
			{
				op = op.next;
				if (op == outPt)
				{
					break;
				}
			}
		}
		FixSelfIntersects(ref outrec.pts);
	}

	private OutPt DoSplitOp(ref OutPt outRecOp, OutPt splitOp)
	{
		OutPt prev = splitOp.prev;
		OutPt next = splitOp.next.next;
		InternalClipper.GetIntersectPoint(prev.pt, splitOp.pt, splitOp.next.pt, next.pt, out var ip);
		Point64 point = new Point64(ip);
		double num = Area(outRecOp);
		double num2 = AreaTriangle(point, splitOp.pt, splitOp.next.pt);
		if (point == prev.pt || point == next.pt)
		{
			next.prev = prev;
			prev.next = next;
		}
		else
		{
			prev.next = (next.prev = new OutPt(point, prev.outrec)
			{
				prev = prev,
				next = next
			});
		}
		SafeDeleteOutPtJoiners(splitOp.next);
		SafeDeleteOutPtJoiners(splitOp);
		if (Math.Abs(num2) >= 1.0 && (Math.Abs(num2) > Math.Abs(num) || num2 > 0.0 == num > 0.0))
		{
			OutRec outRec = new OutRec
			{
				idx = _outrecList.Count
			};
			_outrecList.Add(outRec);
			outRec.owner = prev.outrec.owner;
			outRec.polypath = null;
			splitOp.outrec = outRec;
			splitOp.next.outrec = outRec;
			OutPt next2 = (splitOp.prev = (outRec.pts = new OutPt(point, outRec)
			{
				prev = splitOp.next,
				next = splitOp
			}));
			splitOp.next.next = next2;
		}
		return prev;
	}

	private void FixSelfIntersects(ref OutPt op)
	{
		if (!IsValidClosedPath(op))
		{
			return;
		}
		OutPt outPt = op;
		while (outPt.prev != outPt.next.next)
		{
			if (InternalClipper.SegmentsIntersect(outPt.prev.pt, outPt.pt, outPt.next.pt, outPt.next.next.pt))
			{
				if (outPt == op || outPt.next == op)
				{
					op = outPt.prev;
				}
				outPt = (op = DoSplitOp(ref op, outPt));
			}
			else
			{
				outPt = outPt.next;
				if (outPt == op)
				{
					break;
				}
			}
		}
	}

	internal bool BuildPath(OutPt op, bool reverse, bool isOpen, List<Point64> path)
	{
		if (op.next == op || (!isOpen && op.next == op.prev))
		{
			return false;
		}
		path.Clear();
		Point64 pt;
		OutPt outPt;
		if (reverse)
		{
			pt = op.pt;
			outPt = op.prev;
		}
		else
		{
			op = op.next;
			pt = op.pt;
			outPt = op.next;
		}
		path.Add(pt);
		while (outPt != op)
		{
			if (outPt.pt != pt)
			{
				pt = outPt.pt;
				path.Add(pt);
			}
			outPt = ((!reverse) ? outPt.next : outPt.prev);
		}
		return true;
	}

	protected bool BuildPaths(List<List<Point64>> solutionClosed, List<List<Point64>> solutionOpen)
	{
		solutionClosed.Clear();
		solutionOpen.Clear();
		solutionClosed.Capacity = _outrecList.Count;
		solutionOpen.Capacity = _outrecList.Count;
		foreach (OutRec outrec in _outrecList)
		{
			if (outrec.pts == null)
			{
				continue;
			}
			List<Point64> list = new List<Point64>();
			if (outrec.isOpen)
			{
				if (BuildPath(outrec.pts, ReverseSolution, isOpen: true, list))
				{
					solutionOpen.Add(list);
				}
			}
			else if (BuildPath(outrec.pts, ReverseSolution, isOpen: false, list))
			{
				solutionClosed.Add(list);
			}
		}
		return true;
	}

	private bool Path1InsidePath2(OutRec or1, OutRec or2)
	{
		OutPt outPt = or1.pts;
		PointInPolygonResult pointInPolygonResult;
		do
		{
			pointInPolygonResult = InternalClipper.PointInPolygon(outPt.pt, or2.path);
			if (pointInPolygonResult != 0)
			{
				break;
			}
			outPt = outPt.next;
		}
		while (outPt != or1.pts);
		return pointInPolygonResult == PointInPolygonResult.IsInside;
	}

	private Rect64 GetBounds(List<Point64> path)
	{
		if (path.Count == 0)
		{
			return default(Rect64);
		}
		Rect64 result = new Rect64(long.MaxValue, long.MaxValue, -9223372036854775807L, -9223372036854775807L);
		foreach (Point64 item in path)
		{
			if (item.X < result.left)
			{
				result.left = item.X;
			}
			if (item.X > result.right)
			{
				result.right = item.X;
			}
			if (item.Y < result.top)
			{
				result.top = item.Y;
			}
			if (item.Y > result.bottom)
			{
				result.bottom = item.Y;
			}
		}
		return result;
	}

	private bool DeepCheckOwner(OutRec outrec, OutRec owner)
	{
		if (owner.bounds.IsEmpty())
		{
			owner.bounds = GetBounds(owner.path);
		}
		bool flag = owner.bounds.Contains(outrec.bounds);
		if (owner.splits != null)
		{
			foreach (OutRec split in owner.splits)
			{
				OutRec realOutRec = GetRealOutRec(split);
				if (realOutRec != null && realOutRec.idx > owner.idx && realOutRec != outrec)
				{
					if (realOutRec.splits != null && DeepCheckOwner(outrec, realOutRec))
					{
						return true;
					}
					if (realOutRec.path.Count == 0)
					{
						BuildPath(realOutRec.pts, ReverseSolution, isOpen: false, realOutRec.path);
					}
					if (realOutRec.bounds.IsEmpty())
					{
						realOutRec.bounds = GetBounds(realOutRec.path);
					}
					if (realOutRec.bounds.Contains(outrec.bounds) && Path1InsidePath2(outrec, realOutRec))
					{
						outrec.owner = realOutRec;
						return true;
					}
				}
			}
		}
		if (owner != outrec.owner)
		{
			return false;
		}
		while (true)
		{
			if (flag && Path1InsidePath2(outrec, outrec.owner))
			{
				return true;
			}
			outrec.owner = outrec.owner.owner;
			if (outrec.owner == null)
			{
				break;
			}
			flag = outrec.owner.bounds.Contains(outrec.bounds);
		}
		return false;
	}

	protected bool BuildTree(PolyPathBase polytree, List<List<Point64>> solutionOpen)
	{
		polytree.Clear();
		solutionOpen.Clear();
		solutionOpen.Capacity = _outrecList.Count;
		for (int i = 0; i < _outrecList.Count; i++)
		{
			OutRec outRec = _outrecList[i];
			if (outRec.pts == null)
			{
				continue;
			}
			if (outRec.isOpen)
			{
				List<Point64> list = new List<Point64>();
				if (BuildPath(outRec.pts, ReverseSolution, isOpen: true, list))
				{
					solutionOpen.Add(list);
				}
			}
			else
			{
				if (!BuildPath(outRec.pts, ReverseSolution, isOpen: false, outRec.path))
				{
					continue;
				}
				if (outRec.bounds.IsEmpty())
				{
					outRec.bounds = GetBounds(outRec.path);
				}
				outRec.owner = GetRealOutRec(outRec.owner);
				if (outRec.owner != null)
				{
					DeepCheckOwner(outRec, outRec.owner);
				}
				if (outRec.owner != null && outRec.owner.idx > outRec.idx)
				{
					int idx = outRec.owner.idx;
					outRec.owner.idx = i;
					outRec.idx = idx;
					_outrecList[i] = _outrecList[idx];
					_outrecList[idx] = outRec;
					outRec = _outrecList[i];
					outRec.owner = GetRealOutRec(outRec.owner);
					BuildPath(outRec.pts, ReverseSolution, isOpen: false, outRec.path);
					if (outRec.bounds.IsEmpty())
					{
						outRec.bounds = GetBounds(outRec.path);
					}
					if (outRec.owner != null)
					{
						DeepCheckOwner(outRec, outRec.owner);
					}
				}
				PolyPathBase polyPathBase = ((outRec.owner == null || outRec.owner.polypath == null) ? polytree : outRec.owner.polypath);
				outRec.polypath = polyPathBase.AddChild(outRec.path);
			}
		}
		return true;
	}

	public Rect64 GetBounds()
	{
		Rect64 maxInvalidRect = Clipper.MaxInvalidRect64;
		foreach (Vertex vertex2 in _vertexList)
		{
			Vertex vertex = vertex2;
			do
			{
				if (vertex.pt.X < maxInvalidRect.left)
				{
					maxInvalidRect.left = vertex.pt.X;
				}
				if (vertex.pt.X > maxInvalidRect.right)
				{
					maxInvalidRect.right = vertex.pt.X;
				}
				if (vertex.pt.Y < maxInvalidRect.top)
				{
					maxInvalidRect.top = vertex.pt.Y;
				}
				if (vertex.pt.Y > maxInvalidRect.bottom)
				{
					maxInvalidRect.bottom = vertex.pt.Y;
				}
				vertex = vertex.next;
			}
			while (vertex != vertex2);
		}
		if (maxInvalidRect.IsEmpty())
		{
			return new Rect64(0L, 0L, 0L, 0L);
		}
		return maxInvalidRect;
	}
}
