using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace g3;

public class SVGWriter
{
	public struct Style
	{
		public string fill;

		public string stroke;

		public float stroke_width;

		public static readonly Style Default = new Style
		{
			fill = "none",
			stroke = "black",
			stroke_width = 1f
		};

		public static Style Filled(string fillCol, string strokeCol = "", float strokeWidth = 0f)
		{
			Style result = default(Style);
			result.fill = fillCol;
			result.stroke = strokeCol;
			result.stroke_width = strokeWidth;
			return result;
		}

		public static Style Outline(string strokeCol, float strokeWidth)
		{
			Style result = default(Style);
			result.fill = "none";
			result.stroke = strokeCol;
			result.stroke_width = strokeWidth;
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (fill.Length > 0)
			{
				stringBuilder.Append("fill:");
				stringBuilder.Append(fill);
				stringBuilder.Append(';');
			}
			if (stroke.Length > 0)
			{
				stringBuilder.Append("stroke:");
				stringBuilder.Append(stroke);
				stringBuilder.Append(';');
			}
			if (stroke_width > 0f)
			{
				stringBuilder.Append("stroke-width:");
				stringBuilder.Append(stroke_width);
				stringBuilder.Append(";");
			}
			return stringBuilder.ToString();
		}
	}

	public bool FlipY = true;

	private Dictionary<object, Style> Styles = new Dictionary<object, Style>();

	public Style DefaultPolygonStyle;

	public Style DefaultPolylineStyle;

	public Style DefaultDGraphStyle;

	public Style DefaultCircleStyle;

	public Style DefaultArcStyle;

	public Style DefaultLineStyle;

	private List<object> Objects;

	private AxisAlignedBox2d Bounds;

	public int Precision = 3;

	public double BoundsPad = 10.0;

	public SVGWriter()
	{
		Objects = new List<object>();
		Bounds = AxisAlignedBox2d.Empty;
		DefaultPolygonStyle = Style.Outline("grey", 1f);
		DefaultPolylineStyle = Style.Outline("cyan", 1f);
		DefaultCircleStyle = Style.Filled("green", "black", 1f);
		DefaultArcStyle = Style.Outline("magenta", 1f);
		DefaultLineStyle = Style.Outline("black", 1f);
		DefaultDGraphStyle = Style.Outline("blue", 1f);
	}

	public void SetDefaultLineWidth(float width)
	{
		DefaultPolygonStyle.stroke_width = width;
		DefaultPolylineStyle.stroke_width = width;
		DefaultCircleStyle.stroke_width = width;
		DefaultArcStyle.stroke_width = width;
		DefaultLineStyle.stroke_width = width;
		DefaultDGraphStyle.stroke_width = width;
	}

	public void AddPolygon(Polygon2d poly)
	{
		Objects.Add(poly);
		Bounds.Contain(poly.Bounds);
	}

	public void AddPolygon(Polygon2d poly, Style style)
	{
		Objects.Add(poly);
		Styles[poly] = style;
		Bounds.Contain(poly.Bounds);
	}

	public void AddBox(AxisAlignedBox2d box)
	{
		AddBox(box, DefaultPolygonStyle);
	}

	public void AddBox(AxisAlignedBox2d box, Style style)
	{
		Polygon2d polygon2d = new Polygon2d();
		for (int i = 0; i < 4; i++)
		{
			polygon2d.AppendVertex(box.GetCorner(i));
		}
		AddPolygon(polygon2d, style);
	}

	public void AddPolyline(PolyLine2d poly)
	{
		Objects.Add(poly);
		Bounds.Contain(poly.Bounds);
	}

	public void AddPolyline(PolyLine2d poly, Style style)
	{
		Objects.Add(poly);
		Styles[poly] = style;
		Bounds.Contain(poly.Bounds);
	}

	public void AddGraph(DGraph2 graph)
	{
		Objects.Add(graph);
		Bounds.Contain(graph.GetBounds());
	}

	public void AddGraph(DGraph2 graph, Style style)
	{
		Objects.Add(graph);
		Styles[graph] = style;
		Bounds.Contain(graph.GetBounds());
	}

	public void AddCircle(Circle2d circle)
	{
		Objects.Add(circle);
		Bounds.Contain(circle.Bounds);
	}

	public void AddCircle(Circle2d circle, Style style)
	{
		Objects.Add(circle);
		Styles[circle] = style;
		Bounds.Contain(circle.Bounds);
	}

	public void AddArc(Arc2d arc)
	{
		Objects.Add(arc);
		Bounds.Contain(arc.Bounds);
	}

	public void AddArc(Arc2d arc, Style style)
	{
		Objects.Add(arc);
		Styles[arc] = style;
		Bounds.Contain(arc.Bounds);
	}

	public void AddLine(Segment2d segment)
	{
		Objects.Add(new Segment2dBox(segment));
		Bounds.Contain(segment.P0);
		Bounds.Contain(segment.P1);
	}

	public void AddLine(Segment2d segment, Style style)
	{
		Segment2dBox segment2dBox = new Segment2dBox(segment);
		Objects.Add(segment2dBox);
		Styles[segment2dBox] = style;
		Bounds.Contain(segment.P0);
		Bounds.Contain(segment.P1);
	}

	public void AddComplex(PlanarComplex complex)
	{
		Objects.Add(complex);
		Bounds.Contain(complex.Bounds());
	}

	public IOWriteResult Write(string sFilename)
	{
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			using (StreamWriter streamWriter = new StreamWriter(sFilename))
			{
				if (streamWriter.BaseStream == null)
				{
					return new IOWriteResult(IOCode.FileAccessError, "Could not open file " + sFilename + " for writing");
				}
				write_header_1_1(streamWriter);
				foreach (object @object in Objects)
				{
					if (@object is Polygon2d)
					{
						write_polygon(@object as Polygon2d, streamWriter);
						continue;
					}
					if (@object is PolyLine2d)
					{
						write_polyline(@object as PolyLine2d, streamWriter);
						continue;
					}
					if (@object is Circle2d)
					{
						write_circle(@object as Circle2d, streamWriter);
						continue;
					}
					if (@object is Arc2d)
					{
						write_arc(@object as Arc2d, streamWriter);
						continue;
					}
					if (@object is Segment2dBox)
					{
						write_line(@object as Segment2dBox, streamWriter);
						continue;
					}
					if (@object is DGraph2)
					{
						write_graph(@object as DGraph2, streamWriter);
						continue;
					}
					if (@object is PlanarComplex)
					{
						write_complex(@object as PlanarComplex, streamWriter);
						continue;
					}
					throw new Exception("SVGWriter.Write: unknown object type " + @object.GetType().ToString());
				}
				streamWriter.WriteLine("</svg>");
			}
			Thread.CurrentThread.CurrentCulture = currentCulture;
			return IOWriteResult.Ok;
		}
		catch (Exception ex)
		{
			Thread.CurrentThread.CurrentCulture = currentCulture;
			return new IOWriteResult(IOCode.WriterError, "Unknown error : exception : " + ex.Message);
		}
	}

	public static void QuickWrite(List<GeneralPolygon2d> polygons, string sPath, double line_width = 1.0)
	{
		SVGWriter sVGWriter = new SVGWriter();
		Style style = Style.Outline("black", 2f * (float)line_width);
		Style style2 = Style.Outline("green", 2f * (float)line_width);
		Style style3 = Style.Outline("red", (float)line_width);
		foreach (GeneralPolygon2d polygon in polygons)
		{
			if (polygon.Outer.IsClockwise)
			{
				sVGWriter.AddPolygon(polygon.Outer, style);
			}
			else
			{
				sVGWriter.AddPolygon(polygon.Outer, style2);
			}
			foreach (Polygon2d hole in polygon.Holes)
			{
				sVGWriter.AddPolygon(hole, style3);
			}
		}
		sVGWriter.Write(sPath);
	}

	public static void QuickWrite(DGraph2 graph, string sPath, double line_width = 1.0)
	{
		SVGWriter sVGWriter = new SVGWriter();
		Style style = Style.Outline("black", (float)line_width);
		sVGWriter.AddGraph(graph, style);
		sVGWriter.Write(sPath);
	}

	public static void QuickWrite(List<GeneralPolygon2d> polygons1, string color1, float width1, List<GeneralPolygon2d> polygons2, string color2, float width2, string sPath)
	{
		SVGWriter sVGWriter = new SVGWriter();
		Style style = Style.Outline(color1, width1);
		Style style2 = Style.Outline(color1, width1 / 2f);
		foreach (GeneralPolygon2d item in polygons1)
		{
			sVGWriter.AddPolygon(item.Outer, style);
			foreach (Polygon2d hole in item.Holes)
			{
				sVGWriter.AddPolygon(hole, style2);
			}
		}
		Style style3 = Style.Outline(color2, width2);
		Style style4 = Style.Outline(color2, width2 / 2f);
		foreach (GeneralPolygon2d item2 in polygons2)
		{
			sVGWriter.AddPolygon(item2.Outer, style3);
			foreach (Polygon2d hole2 in item2.Holes)
			{
				sVGWriter.AddPolygon(hole2, style4);
			}
		}
		sVGWriter.Write(sPath);
	}

	protected virtual Vector2d MapPt(Vector2d v)
	{
		if (FlipY)
		{
			return new Vector2d(v.x, Bounds.Min.y + (Bounds.Max.y - v.y));
		}
		return v;
	}

	private void write_header_1_1(StreamWriter w)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<svg ");
		stringBuilder.Append("version=\"1.1\" ");
		stringBuilder.Append("xmlns=\"http://www.w3.org/2000/svg\" ");
		stringBuilder.Append("xmlns:xlink=\"http://www.w3.org/1999/xlink\" ");
		stringBuilder.Append("x=\"0px\" y=\"0px\" ");
		stringBuilder.Append($"viewBox=\"{Math.Round(Bounds.Min.x - BoundsPad, Precision)} {Math.Round(Bounds.Min.y - BoundsPad, Precision)} {Math.Round(Bounds.Width + 2.0 * BoundsPad, Precision)} {Math.Round(Bounds.Height + 2.0 * BoundsPad, Precision)}\" ");
		stringBuilder.Append('>');
		w.WriteLine(stringBuilder);
	}

	private void write_polygon(Polygon2d poly, StreamWriter w)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<polygon points=\"");
		for (int i = 0; i < poly.VertexCount; i++)
		{
			Vector2d vector2d = MapPt(poly[i]);
			stringBuilder.Append(Math.Round(vector2d.x, Precision));
			stringBuilder.Append(',');
			stringBuilder.Append(Math.Round(vector2d.y, Precision));
			if (i < poly.VertexCount - 1)
			{
				stringBuilder.Append(' ');
			}
		}
		stringBuilder.Append("\" ");
		append_style(stringBuilder, poly, ref DefaultPolygonStyle);
		stringBuilder.Append(" />");
		w.WriteLine(stringBuilder);
	}

	private void write_polyline(PolyLine2d poly, StreamWriter w)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<polyline points=\"");
		for (int i = 0; i < poly.VertexCount; i++)
		{
			Vector2d vector2d = MapPt(poly[i]);
			stringBuilder.Append(Math.Round(vector2d.x, Precision));
			stringBuilder.Append(',');
			stringBuilder.Append(Math.Round(vector2d.y, Precision));
			if (i < poly.VertexCount - 1)
			{
				stringBuilder.Append(' ');
			}
		}
		stringBuilder.Append("\" ");
		append_style(stringBuilder, poly, ref DefaultPolylineStyle);
		stringBuilder.Append(" />");
		w.WriteLine(stringBuilder);
	}

	private void write_graph(DGraph2 graph, StreamWriter w)
	{
		string value = get_style(graph, ref DefaultDGraphStyle);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int item in graph.EdgeIndices())
		{
			Segment2d edgeSegment = graph.GetEdgeSegment(item);
			stringBuilder.Append("<line ");
			Vector2d vector2d = MapPt(edgeSegment.P0);
			Vector2d vector2d2 = MapPt(edgeSegment.P1);
			append_property("x1", vector2d.x, stringBuilder);
			append_property("y1", vector2d.y, stringBuilder);
			append_property("x2", vector2d2.x, stringBuilder);
			append_property("y2", vector2d2.y, stringBuilder);
			stringBuilder.Append(value);
			stringBuilder.Append(" />");
			stringBuilder.AppendLine();
		}
		w.WriteLine(stringBuilder);
	}

	private void write_circle(Circle2d circle, StreamWriter w)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<circle ");
		Vector2d vector2d = MapPt(circle.Center);
		append_property("cx", vector2d.x, stringBuilder);
		append_property("cy", vector2d.y, stringBuilder);
		append_property("r", circle.Radius, stringBuilder);
		append_style(stringBuilder, circle, ref DefaultCircleStyle);
		stringBuilder.Append(" />");
		w.WriteLine(stringBuilder);
	}

	private void write_arc(Arc2d arc, StreamWriter w)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Vector2d vector2d = MapPt(arc.P0);
		Vector2d vector2d2 = MapPt(arc.P1);
		stringBuilder.Append("<path ");
		stringBuilder.Append("d=\"");
		stringBuilder.Append("M");
		stringBuilder.Append(Math.Round(vector2d.x, Precision));
		stringBuilder.Append(",");
		stringBuilder.Append(Math.Round(vector2d.y, Precision));
		stringBuilder.Append(" ");
		stringBuilder.Append("A");
		stringBuilder.Append(Math.Round(arc.Radius, Precision));
		stringBuilder.Append(",");
		stringBuilder.Append(Math.Round(arc.Radius, Precision));
		stringBuilder.Append(" ");
		stringBuilder.Append("0 ");
		int value = ((arc.AngleEndDeg - arc.AngleStartDeg > 180.0) ? 1 : 0);
		int value2 = (arc.IsReversed ? 1 : 0);
		stringBuilder.Append(value);
		stringBuilder.Append(",");
		stringBuilder.Append(value2);
		stringBuilder.Append(Math.Round(vector2d2.x, Precision));
		stringBuilder.Append(",");
		stringBuilder.Append(Math.Round(vector2d2.y, Precision));
		stringBuilder.Append("\" ");
		append_style(stringBuilder, arc, ref DefaultArcStyle);
		stringBuilder.Append(" />");
		w.WriteLine(stringBuilder);
	}

	private void write_line(Segment2dBox segbox, StreamWriter w)
	{
		Segment2d segment2d = segbox;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<line ");
		Vector2d vector2d = MapPt(segment2d.P0);
		Vector2d vector2d2 = MapPt(segment2d.P1);
		append_property("x1", vector2d.x, stringBuilder);
		append_property("y1", vector2d.y, stringBuilder);
		append_property("x2", vector2d2.x, stringBuilder);
		append_property("y2", vector2d2.y, stringBuilder);
		append_style(stringBuilder, segbox, ref DefaultLineStyle);
		stringBuilder.Append(" />");
		w.WriteLine(stringBuilder);
	}

	private void write_complex(PlanarComplex complex, StreamWriter w)
	{
		foreach (PlanarComplex.Element item in complex.ElementsItr())
		{
			foreach (IParametricCurve2d item2 in CurveUtils2.Flatten(item.source))
			{
				if (item2 is Segment2d)
				{
					write_line(new Segment2dBox((Segment2d)(object)item2), w);
				}
				else if (item2 is Circle2d)
				{
					write_circle(item2 as Circle2d, w);
				}
				else if (item2 is Polygon2DCurve)
				{
					write_polygon((item2 as Polygon2DCurve).Polygon, w);
				}
				else if (item2 is PolyLine2DCurve)
				{
					write_polyline((item2 as PolyLine2DCurve).Polyline, w);
				}
				else if (item2 is Arc2d)
				{
					write_arc(item2 as Arc2d, w);
				}
			}
		}
	}

	private void append_property(string name, double val, StringBuilder b, bool trailSpace = true)
	{
		b.Append(name);
		b.Append("=\"");
		b.Append(Math.Round(val, Precision));
		if (trailSpace)
		{
			b.Append("\" ");
		}
		else
		{
			b.Append("\"");
		}
	}

	private void append_style(StringBuilder b, object o, ref Style defaultStyle)
	{
		if (!Styles.TryGetValue(o, out var value))
		{
			value = defaultStyle;
		}
		b.Append("style=\"");
		b.Append(value.ToString());
		b.Append("\"");
	}

	private string get_style(object o, ref Style defaultStyle)
	{
		if (!Styles.TryGetValue(o, out var value))
		{
			value = defaultStyle;
		}
		return "style=\"" + value.ToString() + "\"";
	}
}
