using System;
using System.Collections;

namespace g3;

public class MarchingQuads
{
	private struct Cell
	{
		private uint nPosition;

		public float fValue;

		public int nLeftVertex;

		public int nTopVertex;

		public bool bTouched;

		public uint x
		{
			get
			{
				return nPosition & 0xFFFFu;
			}
			set
			{
				nPosition = (y << 16) | (value & 0xFFFFu);
			}
		}

		public uint y
		{
			get
			{
				return (nPosition >> 16) & 0xFFFFu;
			}
			set
			{
				nPosition = ((value & 0xFFFF) << 16) | x;
			}
		}

		public void Initialize(uint x, uint y)
		{
			this.x = x;
			this.y = y;
			fValue = s_fValueSentinel;
			nLeftVertex = (nTopVertex = -1);
			bTouched = false;
		}
	}

	private struct SeedPoint
	{
		public float x;

		public float y;

		public SeedPoint(float fX, float fY)
		{
			x = fX;
			y = fY;
		}
	}

	private DPolyLine2f m_stroke;

	private AxisAlignedBox2f m_bounds;

	private float m_fXShift;

	private float m_fYShift;

	private float m_fScale;

	private int m_nCells;

	private float m_fCellSize;

	private static float s_fValueSentinel = 9999999f;

	private float m_fIsoValue;

	private static int LEFT = 1;

	private static int TOP = 2;

	private static int RIGHT = 4;

	private static int BOTTOM = 8;

	private static int ALL = 15;

	private Cell[][] m_cells;

	private ArrayList m_seedPoints;

	private ImplicitField2d m_field;

	private ArrayList m_cellStack;

	private bool[] m_bEdgeSigns;

	public int Subdivisions
	{
		get
		{
			return m_nCells;
		}
		set
		{
			m_nCells = value;
			SetBounds(m_bounds);
			InitializeCells();
		}
	}

	public AxisAlignedBox2f Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			SetBounds(value);
		}
	}

	public DPolyLine2f Stroke => m_stroke;

	public MarchingQuads(int nSubdivisions, AxisAlignedBox2f bounds, float fIsoValue)
	{
		m_stroke = new DPolyLine2f();
		m_bounds = default(AxisAlignedBox2f);
		m_nCells = nSubdivisions;
		SetBounds(bounds);
		m_cells = null;
		InitializeCells();
		m_seedPoints = new ArrayList();
		m_cellStack = new ArrayList();
		m_bEdgeSigns = new bool[4];
		m_fIsoValue = fIsoValue;
	}

	public AxisAlignedBox2f GetBounds()
	{
		return m_bounds;
	}

	public void AddSeedPoint(float x, float y)
	{
		m_seedPoints.Add(new SeedPoint(x - m_fXShift, y - m_fYShift));
	}

	public void ClearSeedPoints()
	{
		m_seedPoints.Clear();
	}

	public void ClearStroke()
	{
		m_stroke.Clear();
	}

	public void Polygonize(ImplicitField2d field)
	{
		m_field = field;
		ResetCells();
		m_cellStack.Clear();
		for (int i = 0; i < m_seedPoints.Count; i++)
		{
			SeedPoint obj = (SeedPoint)m_seedPoints[i];
			int num = (int)(obj.x / m_fCellSize);
			int num2 = (int)(obj.y / m_fCellSize);
			bool flag = false;
			while (!flag && num2 > 0 && num2 < m_cells.Length - 1 && num > 0 && num < m_cells[0].Length - 1)
			{
				if (!m_cells[num2][num].bTouched)
				{
					if (ProcessCell(num, num2))
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				num--;
			}
			while (m_cellStack.Count != 0)
			{
				Cell cell = (Cell)m_cellStack[m_cellStack.Count - 1];
				m_cellStack.RemoveAt(m_cellStack.Count - 1);
				if (!m_cells[cell.y][cell.x].bTouched)
				{
					ProcessCell((int)cell.x, (int)cell.y);
				}
			}
		}
	}

	private void SubdivideStep(ref float fValue1, ref float fValue2, ref float fX1, ref float fY1, ref float fX2, ref float fY2, bool bVerticalEdge)
	{
		float num = 0.5f;
		float num2 = 0f;
		float num3 = 0f;
		if (bVerticalEdge)
		{
			num2 = fX1;
			num3 = num * fY1 + (1f - num) * fY2;
		}
		else
		{
			num2 = num * fX1 + (1f - num) * fX2;
			num3 = fY1;
		}
		float num4 = m_field.Value(num2, num3);
		if (num4 < m_fIsoValue)
		{
			fValue1 = num4;
			fX1 = num2;
			fY1 = num3;
		}
		else
		{
			fValue2 = num4;
			fX2 = num2;
			fY2 = num3;
		}
	}

	private int LerpAndAddStrokeVertex(float fValue1, float fValue2, int x1, int y1, int x2, int y2, bool bVerticalEdge)
	{
		if (fValue1 > fValue2)
		{
			int num = x1;
			x1 = x2;
			x2 = num;
			int num2 = y1;
			y1 = y2;
			y2 = num2;
			float num3 = fValue1;
			fValue1 = fValue2;
			fValue2 = num3;
		}
		float fValue3 = fValue1;
		float fValue4 = fValue2;
		float fX = (float)x1 * m_fCellSize + m_fXShift;
		float fY = (float)y1 * m_fCellSize + m_fYShift;
		float fX2 = (float)x2 * m_fCellSize + m_fXShift;
		float fY2 = (float)y2 * m_fCellSize + m_fYShift;
		for (int i = 0; i < 10; i++)
		{
			SubdivideStep(ref fValue3, ref fValue4, ref fX, ref fY, ref fX2, ref fY2, bVerticalEdge);
		}
		if (Math.Abs(fValue3) < Math.Abs(fValue4))
		{
			return m_stroke.AddVertex(fX, fY);
		}
		return m_stroke.AddVertex(fX2, fY2);
	}

	private int GetLeftEdgeVertex(int xi, int yi)
	{
		Cell cell = m_cells[yi][xi];
		if (cell.nLeftVertex != -1)
		{
			return cell.nLeftVertex;
		}
		m_cells[yi][xi].nLeftVertex = LerpAndAddStrokeVertex(cell.fValue, m_cells[yi + 1][xi].fValue, xi, yi, xi, yi + 1, bVerticalEdge: true);
		return m_cells[yi][xi].nLeftVertex;
	}

	private int GetRightEdgeVertex(int xi, int yi)
	{
		Cell cell = m_cells[yi][xi + 1];
		if (cell.nLeftVertex != -1)
		{
			return cell.nLeftVertex;
		}
		m_cells[yi][xi + 1].nLeftVertex = LerpAndAddStrokeVertex(cell.fValue, m_cells[yi + 1][xi + 1].fValue, xi + 1, yi, xi + 1, yi + 1, bVerticalEdge: true);
		return m_cells[yi][xi + 1].nLeftVertex;
	}

	private int GetTopEdgeVertex(int xi, int yi)
	{
		Cell cell = m_cells[yi][xi];
		if (cell.nTopVertex != -1)
		{
			return cell.nTopVertex;
		}
		m_cells[yi][xi].nTopVertex = LerpAndAddStrokeVertex(cell.fValue, m_cells[yi][xi + 1].fValue, xi, yi, xi + 1, yi, bVerticalEdge: false);
		return m_cells[yi][xi].nTopVertex;
	}

	private int GetBottomEdgeVertex(int xi, int yi)
	{
		Cell cell = m_cells[yi + 1][xi];
		if (cell.nTopVertex != -1)
		{
			return cell.nTopVertex;
		}
		m_cells[yi + 1][xi].nTopVertex = LerpAndAddStrokeVertex(cell.fValue, m_cells[yi + 1][xi + 1].fValue, xi, yi + 1, xi + 1, yi + 1, bVerticalEdge: false);
		return m_cells[yi + 1][xi].nTopVertex;
	}

	private bool ProcessCell(int xi, int yi)
	{
		m_cells[yi][xi].bTouched = true;
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			int num2 = xi + (i & 1);
			int num3 = yi + ((i >> 1) & 1);
			if (m_cells[num3][num2].fValue == s_fValueSentinel)
			{
				m_cells[num3][num2].fValue = m_field.Value((float)num2 * m_fCellSize + m_fXShift, (float)num3 * m_fCellSize + m_fYShift);
			}
			m_bEdgeSigns[i] = m_cells[num3][num2].fValue > m_fIsoValue;
			num |= (m_bEdgeSigns[i] ? 1 : 0) << i;
		}
		if (num == 0 || num == 15)
		{
			return false;
		}
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		if (m_bEdgeSigns[0] != m_bEdgeSigns[2])
		{
			num4 = GetLeftEdgeVertex(xi, yi);
		}
		if (m_bEdgeSigns[1] != m_bEdgeSigns[3])
		{
			num5 = GetRightEdgeVertex(xi, yi);
		}
		if (m_bEdgeSigns[0] != m_bEdgeSigns[1])
		{
			num6 = GetTopEdgeVertex(xi, yi);
		}
		if (m_bEdgeSigns[2] != m_bEdgeSigns[3])
		{
			num7 = GetBottomEdgeVertex(xi, yi);
		}
		float num8 = 0f;
		if (num == 6 || num == 9)
		{
			num8 = m_field.Value((float)xi * m_fCellSize + m_fCellSize / 2f + m_fXShift, (float)yi * m_fCellSize + m_fCellSize / 2f + m_fYShift);
		}
		int num9 = 0;
		switch (num)
		{
		case 1:
		case 14:
			m_stroke.AddEdge(num4, num6);
			num9 = LEFT | TOP;
			break;
		case 2:
		case 13:
			m_stroke.AddEdge(num6, num5);
			num9 = RIGHT | TOP;
			break;
		case 4:
		case 11:
			m_stroke.AddEdge(num7, num4);
			num9 = LEFT | BOTTOM;
			break;
		case 7:
		case 8:
			m_stroke.AddEdge(num5, num7);
			num9 = RIGHT | BOTTOM;
			break;
		case 3:
		case 12:
			m_stroke.AddEdge(num5, num4);
			num9 = LEFT | RIGHT;
			break;
		case 5:
		case 10:
			m_stroke.AddEdge(num6, num7);
			num9 = BOTTOM | TOP;
			break;
		case 9:
			if (num8 > m_fIsoValue)
			{
				m_stroke.AddEdge(num4, num7);
				m_stroke.AddEdge(num6, num5);
			}
			else
			{
				m_stroke.AddEdge(num4, num6);
				m_stroke.AddEdge(num7, num5);
			}
			num9 = ALL;
			break;
		case 6:
			if (num8 > m_fIsoValue)
			{
				m_stroke.AddEdge(num4, num6);
				m_stroke.AddEdge(num7, num5);
			}
			else
			{
				m_stroke.AddEdge(num4, num7);
				m_stroke.AddEdge(num6, num5);
			}
			num9 = ALL;
			break;
		}
		if ((num9 & LEFT) != 0 && xi - 1 >= 0 && !m_cells[yi][xi - 1].bTouched)
		{
			m_cellStack.Add(m_cells[yi][xi - 1]);
		}
		if ((num9 & RIGHT) != 0 && xi + 1 < m_nCells && !m_cells[yi][xi + 1].bTouched)
		{
			m_cellStack.Add(m_cells[yi][xi + 1]);
		}
		if ((num9 & BOTTOM) != 0 && yi + 1 < m_nCells && !m_cells[yi + 1][xi].bTouched)
		{
			m_cellStack.Add(m_cells[yi + 1][xi]);
		}
		if ((num9 & TOP) != 0 && yi - 1 >= 0 && !m_cells[yi - 1][xi].bTouched)
		{
			m_cellStack.Add(m_cells[yi - 1][xi]);
		}
		return true;
	}

	private void ResetCells()
	{
		for (uint num = 0u; num < m_cells.Length; num++)
		{
			for (uint num2 = 0u; num2 < m_cells.Length; num2++)
			{
				m_cells[num][num2].bTouched = false;
				m_cells[num][num2].nLeftVertex = (m_cells[num][num2].nTopVertex = -1);
			}
		}
	}

	private void InitializeCells()
	{
		m_cells = new Cell[m_nCells + 1][];
		for (uint num = 0u; num < m_cells.Length; num++)
		{
			m_cells[num] = new Cell[m_nCells + 1];
			for (uint num2 = 0u; num2 < m_cells.Length; num2++)
			{
				m_cells[num][num2].Initialize(num2, num);
			}
		}
	}

	private void SetBounds(AxisAlignedBox2f bounds)
	{
		m_bounds = bounds;
		m_fXShift = ((bounds.Min.x < 0f) ? bounds.Min.x : (0f - bounds.Min.x));
		m_fYShift = ((bounds.Min.y < 0f) ? bounds.Min.y : (0f - bounds.Min.y));
		m_fScale = ((bounds.Width > bounds.Height) ? bounds.Width : bounds.Height);
		m_fCellSize = m_fScale / (float)m_nCells;
	}
}
