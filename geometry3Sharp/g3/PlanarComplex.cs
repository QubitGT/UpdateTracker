using System;
using System.Collections.Generic;

namespace g3;

public class PlanarComplex
{
	public abstract class Element
	{
		public IParametricCurve2d source;

		public int ID;

		private Colorf color = Colorf.Black;

		private bool has_set_color;

		public Colorf Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				has_set_color = true;
			}
		}

		public bool HasSetColor => has_set_color;

		protected void copy_to(Element new_element)
		{
			new_element.ID = ID;
			new_element.color = color;
			new_element.has_set_color = has_set_color;
			if (source != null)
			{
				new_element.source = source.Clone();
			}
		}

		public abstract IEnumerable<Segment2d> SegmentItr();

		public abstract AxisAlignedBox2d Bounds();

		public abstract Element Clone();
	}

	public class SmoothCurveElement : Element
	{
		public PolyLine2d polyLine;

		public override IEnumerable<Segment2d> SegmentItr()
		{
			return polyLine.SegmentItr();
		}

		public override AxisAlignedBox2d Bounds()
		{
			return polyLine.GetBounds();
		}

		public override Element Clone()
		{
			SmoothCurveElement smoothCurveElement = new SmoothCurveElement();
			copy_to(smoothCurveElement);
			smoothCurveElement.polyLine = ((polyLine == source) ? (smoothCurveElement.source as PolyLine2d) : new PolyLine2d(polyLine));
			return smoothCurveElement;
		}
	}

	public class SmoothLoopElement : Element
	{
		public Polygon2d polygon;

		public override IEnumerable<Segment2d> SegmentItr()
		{
			return polygon.SegmentItr();
		}

		public override AxisAlignedBox2d Bounds()
		{
			return polygon.GetBounds();
		}

		public override Element Clone()
		{
			SmoothLoopElement smoothLoopElement = new SmoothLoopElement();
			copy_to(smoothLoopElement);
			smoothLoopElement.polygon = ((polygon == source) ? (smoothLoopElement.source as Polygon2d) : new Polygon2d(polygon));
			return smoothLoopElement;
		}
	}

	public class GeneralSolid
	{
		public Element Outer;

		public List<Element> Holes = new List<Element>();
	}

	public class SolidRegionInfo
	{
		public List<GeneralPolygon2d> Polygons;

		public List<PlanarSolid2d> Solids;

		public List<GeneralSolid> PolygonsSources;

		public AxisAlignedBox2d Bounds
		{
			get
			{
				AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
				foreach (GeneralPolygon2d polygon in Polygons)
				{
					empty.Contain(polygon.Bounds);
				}
				return empty;
			}
		}

		public double Area
		{
			get
			{
				double num = 0.0;
				foreach (GeneralPolygon2d polygon in Polygons)
				{
					num += polygon.Area;
				}
				return num;
			}
		}

		public double HolesArea
		{
			get
			{
				double num = 0.0;
				foreach (GeneralPolygon2d polygon in Polygons)
				{
					foreach (Polygon2d hole in polygon.Holes)
					{
						num += Math.Abs(hole.SignedArea);
					}
				}
				return num;
			}
		}
	}

	public struct FindSolidsOptions
	{
		public double SimplifyDeviationTolerance;

		public bool WantCurveSolids;

		public bool TrustOrientations;

		public bool AllowOverlappingHoles;

		public static readonly FindSolidsOptions Default = new FindSolidsOptions
		{
			SimplifyDeviationTolerance = 0.1,
			WantCurveSolids = true,
			TrustOrientations = false,
			AllowOverlappingHoles = false
		};

		public static readonly FindSolidsOptions SortPolygons = new FindSolidsOptions
		{
			SimplifyDeviationTolerance = 0.0,
			WantCurveSolids = false,
			TrustOrientations = true,
			AllowOverlappingHoles = false
		};
	}

	public class ClosedLoopsInfo
	{
		public List<Polygon2d> Polygons;

		public List<IParametricCurve2d> Loops;

		public AxisAlignedBox2d Bounds
		{
			get
			{
				AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
				foreach (Polygon2d polygon in Polygons)
				{
					empty.Contain(polygon.GetBounds());
				}
				return empty;
			}
		}
	}

	public class OpenCurvesInfo
	{
		public List<PolyLine2d> Polylines;

		public List<IParametricCurve2d> Curves;

		public AxisAlignedBox2d Bounds
		{
			get
			{
				AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
				foreach (PolyLine2d polyline in Polylines)
				{
					empty.Contain(polyline.GetBounds());
				}
				return empty;
			}
		}
	}

	public double DistanceAccuracy = 0.1;

	public double AngleAccuracyDeg = 5.0;

	public double SpacingT = 0.01;

	public bool MinimizeSampling;

	private int id_generator = 1;

	private List<Element> vElements;

	public int ElementCount => vElements.Count;

	public PlanarComplex()
	{
		vElements = new List<Element>();
	}

	public Element Add(IParametricCurve2d curve)
	{
		if (curve.IsClosed)
		{
			SmoothLoopElement smoothLoopElement = new SmoothLoopElement();
			smoothLoopElement.ID = id_generator++;
			smoothLoopElement.source = curve;
			UpdateSampling(smoothLoopElement);
			vElements.Add(smoothLoopElement);
			return smoothLoopElement;
		}
		SmoothCurveElement smoothCurveElement = new SmoothCurveElement();
		smoothCurveElement.ID = id_generator++;
		smoothCurveElement.source = curve;
		UpdateSampling(smoothCurveElement);
		vElements.Add(smoothCurveElement);
		return smoothCurveElement;
	}

	public Element Add(Polygon2d poly)
	{
		SmoothLoopElement smoothLoopElement = new SmoothLoopElement();
		smoothLoopElement.ID = id_generator++;
		smoothLoopElement.source = new Polygon2DCurve
		{
			Polygon = poly
		};
		smoothLoopElement.polygon = new Polygon2d(poly);
		vElements.Add(smoothLoopElement);
		return smoothLoopElement;
	}

	public Element Add(PolyLine2d pline)
	{
		SmoothCurveElement smoothCurveElement = new SmoothCurveElement();
		smoothCurveElement.ID = id_generator++;
		smoothCurveElement.source = new PolyLine2DCurve
		{
			Polyline = pline
		};
		smoothCurveElement.polyLine = new PolyLine2d(pline);
		vElements.Add(smoothCurveElement);
		return smoothCurveElement;
	}

	public void Remove(Element e)
	{
		vElements.Remove(e);
	}

	private void UpdateSampling(SmoothCurveElement c)
	{
		if (MinimizeSampling && c.source is Segment2d)
		{
			c.polyLine = new PolyLine2d();
			c.polyLine.AppendVertex(((Segment2d)(object)c.source).P0);
			c.polyLine.AppendVertex(((Segment2d)(object)c.source).P1);
		}
		else
		{
			c.polyLine = new PolyLine2d(CurveSampler2.AutoSample(c.source, DistanceAccuracy, SpacingT));
		}
	}

	private void UpdateSampling(SmoothLoopElement l)
	{
		l.polygon = new Polygon2d(CurveSampler2.AutoSample(l.source, DistanceAccuracy, SpacingT));
	}

	public void Reverse(SmoothCurveElement c)
	{
		c.source.Reverse();
		UpdateSampling(c);
	}

	public IEnumerable<ComplexSegment2d> AllSegmentsItr()
	{
		foreach (Element e in vElements)
		{
			ComplexSegment2d s = default(ComplexSegment2d);
			if (e is SmoothLoopElement)
			{
				s.isClosed = true;
			}
			else if (e is SmoothCurveElement)
			{
				s.isClosed = false;
			}
			foreach (Segment2d item in e.SegmentItr())
			{
				s.seg = item;
				s.element = e;
				yield return s;
			}
		}
	}

	public IEnumerable<Element> ElementsItr()
	{
		foreach (Element vElement in vElements)
		{
			yield return vElement;
		}
	}

	public IEnumerable<SmoothLoopElement> LoopsItr()
	{
		foreach (Element vElement in vElements)
		{
			if (vElement is SmoothLoopElement)
			{
				yield return vElement as SmoothLoopElement;
			}
		}
	}

	public IEnumerable<SmoothCurveElement> CurvesItr()
	{
		foreach (Element vElement in vElements)
		{
			if (vElement is SmoothCurveElement)
			{
				yield return vElement as SmoothCurveElement;
			}
		}
	}

	public bool HasOpenCurves()
	{
		foreach (Element vElement in vElements)
		{
			if (vElement is SmoothCurveElement)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<IParametricCurve2d> LoopLeafComponentsItr()
	{
		foreach (Element vElement in vElements)
		{
			if (!(vElement is SmoothLoopElement))
			{
				continue;
			}
			IParametricCurve2d source = vElement.source;
			if (source is IMultiCurve2d)
			{
				foreach (IParametricCurve2d item in CurveUtils2.LeafCurvesIteration(source))
				{
					yield return item;
				}
			}
			else
			{
				yield return source;
			}
		}
	}

	public IEnumerable<ComplexEndpoint2d> EndpointsItr()
	{
		foreach (Element vElement in vElements)
		{
			if (vElement is SmoothCurveElement)
			{
				SmoothCurveElement s = vElement as SmoothCurveElement;
				yield return new ComplexEndpoint2d
				{
					v = s.polyLine.Start,
					isStart = true,
					element = s
				};
				yield return new ComplexEndpoint2d
				{
					v = s.polyLine.End,
					isStart = false,
					element = s
				};
			}
		}
	}

	public AxisAlignedBox2d Bounds()
	{
		AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
		foreach (Element vElement in vElements)
		{
			empty.Contain(vElement.Bounds());
		}
		return empty;
	}

	public void SplitAllLoops()
	{
		List<Element> list = new List<Element>();
		List<IParametricCurve2d> list2 = new List<IParametricCurve2d>();
		foreach (SmoothLoopElement item in LoopsItr())
		{
			if (item.source is IMultiCurve2d)
			{
				list.Add(item);
				find_sub_elements(item.source as IMultiCurve2d, list2);
			}
		}
		foreach (Element item2 in list)
		{
			Remove(item2);
		}
		foreach (IParametricCurve2d item3 in list2)
		{
			Add(item3);
		}
	}

	private void find_sub_elements(IMultiCurve2d multicurve, List<IParametricCurve2d> vAdd)
	{
		foreach (IParametricCurve2d curf in multicurve.Curves)
		{
			if (curf is IMultiCurve2d)
			{
				find_sub_elements(curf as IMultiCurve2d, vAdd);
			}
			else
			{
				vAdd.Add(curf);
			}
		}
	}

	public bool JoinElements(ComplexEndpoint2d a, ComplexEndpoint2d b, double loop_tolerance = 1E-08)
	{
		if (a.element == b.element)
		{
			throw new Exception("PlanarComplex.ChainElements: same curve!!");
		}
		SmoothCurveElement element = a.element;
		SmoothCurveElement element2 = b.element;
		SmoothCurveElement smoothCurveElement = null;
		if (!a.isStart && b.isStart)
		{
			vElements.Remove(element2);
			append(element, element2);
			smoothCurveElement = element;
		}
		else if (a.isStart && !b.isStart)
		{
			vElements.Remove(element);
			append(element2, element);
			smoothCurveElement = element2;
		}
		else if (!a.isStart)
		{
			element2.source.Reverse();
			vElements.Remove(element2);
			append(element, element2);
			smoothCurveElement = element;
		}
		else if (a.isStart)
		{
			element.source.Reverse();
			vElements.Remove(element2);
			append(element, element2);
			smoothCurveElement = element;
		}
		if (smoothCurveElement != null)
		{
			if ((smoothCurveElement.polyLine.Start - smoothCurveElement.polyLine.End).Length < loop_tolerance)
			{
				if (!(smoothCurveElement.source is ParametricCurveSequence2))
				{
					throw new Exception("PlanarComplex.JoinElements: we have closed a loop but it is not a parametric seq??");
				}
				(smoothCurveElement.source as ParametricCurveSequence2).IsClosed = true;
				SmoothLoopElement smoothLoopElement = new SmoothLoopElement
				{
					ID = id_generator++,
					source = smoothCurveElement.source
				};
				vElements.Remove(smoothCurveElement);
				vElements.Add(smoothLoopElement);
				UpdateSampling(smoothLoopElement);
			}
			return true;
		}
		return false;
	}

	public void ConvertToLoop(SmoothCurveElement curve, double tolerance = 1E-08)
	{
		if (!((curve.polyLine.Start - curve.polyLine.End).Length < tolerance))
		{
			return;
		}
		if (curve.polyLine.VertexCount == 2)
		{
			vElements.Remove(curve);
			return;
		}
		if (curve.source is ParametricCurveSequence2)
		{
			(curve.source as ParametricCurveSequence2).IsClosed = true;
			SmoothLoopElement smoothLoopElement = new SmoothLoopElement
			{
				ID = id_generator++,
				source = curve.source
			};
			vElements.Remove(curve);
			vElements.Add(smoothLoopElement);
			UpdateSampling(smoothLoopElement);
			return;
		}
		throw new Exception("PlanarComplex.ConvertToLoop: we have closed a loop but it is not a parametric seq??");
	}

	private void append(SmoothCurveElement cTo, SmoothCurveElement cAppend)
	{
		ParametricCurveSequence2 parametricCurveSequence = null;
		if (cTo.source is ParametricCurveSequence2)
		{
			parametricCurveSequence = cTo.source as ParametricCurveSequence2;
		}
		else
		{
			parametricCurveSequence = new ParametricCurveSequence2();
			parametricCurveSequence.Append(cTo.source);
		}
		if (cAppend.source is ParametricCurveSequence2)
		{
			foreach (IParametricCurve2d curf in (cAppend.source as ParametricCurveSequence2).Curves)
			{
				parametricCurveSequence.Append(curf);
			}
		}
		else
		{
			parametricCurveSequence.Append(cAppend.source);
		}
		cTo.source = parametricCurveSequence;
		UpdateSampling(cTo);
	}

	public SolidRegionInfo FindSolidRegions(double fSimplifyDeviationTol = 0.1, bool bWantCurveSolids = true)
	{
		FindSolidsOptions @default = FindSolidsOptions.Default;
		@default.SimplifyDeviationTolerance = fSimplifyDeviationTol;
		@default.WantCurveSolids = bWantCurveSolids;
		return FindSolidRegions(@default);
	}

	public SolidRegionInfo FindSolidRegions(FindSolidsOptions options)
	{
		List<SmoothLoopElement> list = new List<SmoothLoopElement>(LoopsItr());
		int count = list.Count;
		int num = 0;
		foreach (SmoothLoopElement item in list)
		{
			num = Math.Max(num, item.ID + 1);
		}
		AxisAlignedBox2d[] bounds = new AxisAlignedBox2d[num];
		foreach (SmoothLoopElement item2 in list)
		{
			bounds[item2.ID] = item2.Bounds();
		}
		double num2 = 0.0;
		double simplifyDeviationTolerance = options.SimplifyDeviationTolerance;
		Polygon2d[] array = new Polygon2d[num];
		foreach (SmoothLoopElement item3 in list)
		{
			Polygon2d polygon2d = new Polygon2d(item3.polygon);
			if (num2 > 0.0 || simplifyDeviationTolerance > 0.0)
			{
				polygon2d.Simplify(num2, simplifyDeviationTolerance);
			}
			array[item3.ID] = polygon2d;
		}
		list.Sort((SmoothLoopElement x, SmoothLoopElement y) => (!bounds[x.ID].Contains(bounds[y.ID])) ? 1 : (-1));
		bool[] array2 = new bool[count];
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		Dictionary<int, List<int>> dictionary2 = new Dictionary<int, List<int>>();
		bool trustOrientations = options.TrustOrientations;
		bool wantCurveSolids = options.WantCurveSolids;
		bool bCheckContainment = !options.AllowOverlappingHoles;
		for (int i = 0; i < count; i++)
		{
			SmoothLoopElement smoothLoopElement = list[i];
			Polygon2d polygon2d2 = array[smoothLoopElement.ID];
			for (int j = 0; j < count; j++)
			{
				if (i == j)
				{
					continue;
				}
				SmoothLoopElement smoothLoopElement2 = list[j];
				Polygon2d o = array[smoothLoopElement2.ID];
				if ((!trustOrientations || smoothLoopElement2.polygon.IsClockwise != smoothLoopElement.polygon.IsClockwise) && bounds[smoothLoopElement.ID].Contains(bounds[smoothLoopElement2.ID]) && polygon2d2.Contains(o))
				{
					if (!dictionary.ContainsKey(i))
					{
						dictionary.Add(i, new List<int>());
					}
					dictionary[i].Add(j);
					array2[j] = true;
					if (!dictionary2.ContainsKey(j))
					{
						dictionary2.Add(j, new List<int>());
					}
					dictionary2[j].Add(i);
				}
			}
		}
		List<GeneralPolygon2d> list2 = new List<GeneralPolygon2d>();
		List<GeneralSolid> list3 = new List<GeneralSolid>();
		List<PlanarSolid2d> list4 = new List<PlanarSolid2d>();
		HashSet<SmoothLoopElement> hashSet = new HashSet<SmoothLoopElement>();
		Dictionary<SmoothLoopElement, int> dictionary3 = new Dictionary<SmoothLoopElement, int>();
		List<int> list5 = new List<int>();
		for (int k = 0; k < count; k++)
		{
			SmoothLoopElement smoothLoopElement3 = list[k];
			if (array2[k])
			{
				continue;
			}
			Polygon2d polygon2d3 = array[smoothLoopElement3.ID];
			IParametricCurve2d parametricCurve2d = (wantCurveSolids ? smoothLoopElement3.source.Clone() : null);
			if (!polygon2d3.IsClockwise)
			{
				polygon2d3.Reverse();
				if (wantCurveSolids)
				{
					parametricCurve2d.Reverse();
				}
			}
			GeneralPolygon2d generalPolygon2d = new GeneralPolygon2d();
			generalPolygon2d.Outer = polygon2d3;
			PlanarSolid2d planarSolid2d = new PlanarSolid2d();
			if (wantCurveSolids)
			{
				planarSolid2d.SetOuter(parametricCurve2d, bIsClockwise: true);
			}
			int count2 = list2.Count;
			dictionary3[smoothLoopElement3] = count2;
			hashSet.Add(smoothLoopElement3);
			if (dictionary.ContainsKey(k))
			{
				list5.Add(k);
			}
			list2.Add(generalPolygon2d);
			list3.Add(new GeneralSolid
			{
				Outer = smoothLoopElement3
			});
			if (wantCurveSolids)
			{
				list4.Add(planarSolid2d);
			}
		}
		while (list5.Count > 0)
		{
			List<int> list6 = new List<int>();
			foreach (int item4 in list5)
			{
				SmoothLoopElement key = list[item4];
				int index = dictionary3[key];
				foreach (int item5 in dictionary[item4])
				{
					SmoothLoopElement smoothLoopElement4 = list[item5];
					if (dictionary2[item5].Count > 1)
					{
						continue;
					}
					Polygon2d polygon2d4 = array[smoothLoopElement4.ID];
					IParametricCurve2d parametricCurve2d2 = (wantCurveSolids ? smoothLoopElement4.source.Clone() : null);
					if (polygon2d4.IsClockwise)
					{
						polygon2d4.Reverse();
						if (wantCurveSolids)
						{
							parametricCurve2d2.Reverse();
						}
					}
					try
					{
						list2[index].AddHole(polygon2d4, bCheckContainment);
						list3[index].Holes.Add(smoothLoopElement4);
						if (parametricCurve2d2 != null)
						{
							list4[index].AddHole(parametricCurve2d2);
						}
					}
					catch
					{
					}
					hashSet.Add(smoothLoopElement4);
					if (dictionary.ContainsKey(item5))
					{
						list6.Add(item5);
					}
				}
				list6.Add(item4);
			}
			foreach (int item6 in list6)
			{
				dictionary.Remove(item6);
				foreach (int item7 in new List<int>(dictionary2.Keys))
				{
					if (dictionary2[item7].Contains(item6))
					{
						dictionary2[item7].Remove(item6);
					}
				}
			}
			list5.Clear();
			for (int l = 0; l < count; l++)
			{
				SmoothLoopElement smoothLoopElement5 = list[l];
				if (hashSet.Contains(smoothLoopElement5) || !dictionary.ContainsKey(l) || dictionary2[l].Count > 0)
				{
					continue;
				}
				Polygon2d polygon2d5 = array[smoothLoopElement5.ID];
				IParametricCurve2d parametricCurve2d3 = (wantCurveSolids ? smoothLoopElement5.source.Clone() : null);
				if (!polygon2d5.IsClockwise)
				{
					polygon2d5.Reverse();
					if (wantCurveSolids)
					{
						parametricCurve2d3.Reverse();
					}
				}
				GeneralPolygon2d generalPolygon2d2 = new GeneralPolygon2d();
				generalPolygon2d2.Outer = polygon2d5;
				PlanarSolid2d planarSolid2d2 = new PlanarSolid2d();
				if (wantCurveSolids)
				{
					planarSolid2d2.SetOuter(parametricCurve2d3, bIsClockwise: true);
				}
				int count3 = list2.Count;
				dictionary3[smoothLoopElement5] = count3;
				hashSet.Add(smoothLoopElement5);
				if (dictionary.ContainsKey(l))
				{
					list5.Add(l);
				}
				list2.Add(generalPolygon2d2);
				list3.Add(new GeneralSolid
				{
					Outer = smoothLoopElement5
				});
				if (wantCurveSolids)
				{
					list4.Add(planarSolid2d2);
				}
			}
		}
		for (int m = 0; m < count; m++)
		{
			SmoothLoopElement smoothLoopElement6 = list[m];
			if (hashSet.Contains(smoothLoopElement6))
			{
				continue;
			}
			Polygon2d polygon2d6 = array[smoothLoopElement6.ID];
			IParametricCurve2d parametricCurve2d4 = (wantCurveSolids ? smoothLoopElement6.source.Clone() : null);
			if (!polygon2d6.IsClockwise)
			{
				polygon2d6.Reverse();
				if (wantCurveSolids)
				{
					parametricCurve2d4.Reverse();
				}
			}
			GeneralPolygon2d generalPolygon2d3 = new GeneralPolygon2d();
			generalPolygon2d3.Outer = polygon2d6;
			PlanarSolid2d planarSolid2d3 = new PlanarSolid2d();
			if (wantCurveSolids)
			{
				planarSolid2d3.SetOuter(parametricCurve2d4, bIsClockwise: true);
			}
			list2.Add(generalPolygon2d3);
			list3.Add(new GeneralSolid
			{
				Outer = smoothLoopElement6
			});
			if (wantCurveSolids)
			{
				list4.Add(planarSolid2d3);
			}
		}
		return new SolidRegionInfo
		{
			Polygons = list2,
			PolygonsSources = list3,
			Solids = (wantCurveSolids ? list4 : null)
		};
	}

	public ClosedLoopsInfo FindClosedLoops(double fSimplifyDeviationTol = 0.1)
	{
		List<SmoothLoopElement> list = new List<SmoothLoopElement>(LoopsItr());
		_ = list.Count;
		int num = 0;
		foreach (SmoothLoopElement item in list)
		{
			num = Math.Max(num, item.ID + 1);
		}
		double num2 = 0.0;
		Polygon2d[] array = new Polygon2d[num];
		IParametricCurve2d[] array2 = new IParametricCurve2d[num];
		foreach (SmoothLoopElement item2 in list)
		{
			Polygon2d polygon2d = new Polygon2d(item2.polygon);
			if (num2 > 0.0 || fSimplifyDeviationTol > 0.0)
			{
				polygon2d.Simplify(num2, fSimplifyDeviationTol);
			}
			array[item2.ID] = polygon2d;
			array2[item2.ID] = item2.source;
		}
		ClosedLoopsInfo closedLoopsInfo = new ClosedLoopsInfo
		{
			Polygons = new List<Polygon2d>(),
			Loops = new List<IParametricCurve2d>()
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].VertexCount > 0)
			{
				closedLoopsInfo.Polygons.Add(array[i]);
				closedLoopsInfo.Loops.Add(array2[i]);
			}
		}
		return closedLoopsInfo;
	}

	public OpenCurvesInfo FindOpenCurves(double fSimplifyDeviationTol = 0.1)
	{
		List<SmoothCurveElement> list = new List<SmoothCurveElement>(CurvesItr());
		_ = list.Count;
		int num = 0;
		foreach (SmoothCurveElement item in list)
		{
			num = Math.Max(num, item.ID + 1);
		}
		double num2 = 0.0;
		PolyLine2d[] array = new PolyLine2d[num];
		IParametricCurve2d[] array2 = new IParametricCurve2d[num];
		foreach (SmoothCurveElement item2 in list)
		{
			PolyLine2d polyLine2d = new PolyLine2d(item2.polyLine);
			if (num2 > 0.0 || fSimplifyDeviationTol > 0.0)
			{
				polyLine2d.Simplify(num2, fSimplifyDeviationTol);
			}
			array[item2.ID] = polyLine2d;
			array2[item2.ID] = item2.source;
		}
		OpenCurvesInfo openCurvesInfo = new OpenCurvesInfo
		{
			Polylines = new List<PolyLine2d>(),
			Curves = new List<IParametricCurve2d>()
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].VertexCount > 0)
			{
				openCurvesInfo.Polylines.Add(array[i]);
				openCurvesInfo.Curves.Add(array2[i]);
			}
		}
		return openCurvesInfo;
	}

	public PlanarComplex Clone()
	{
		PlanarComplex planarComplex = new PlanarComplex();
		planarComplex.DistanceAccuracy = DistanceAccuracy;
		planarComplex.AngleAccuracyDeg = AngleAccuracyDeg;
		planarComplex.SpacingT = SpacingT;
		planarComplex.MinimizeSampling = MinimizeSampling;
		planarComplex.id_generator = id_generator;
		planarComplex.vElements = new List<Element>(vElements.Count);
		foreach (Element vElement in vElements)
		{
			planarComplex.vElements.Add(vElement.Clone());
		}
		return planarComplex;
	}

	public void Append(PlanarComplex append)
	{
		foreach (Element vElement in append.vElements)
		{
			vElement.ID = id_generator++;
			vElements.Add(vElement);
		}
		append.vElements.Clear();
	}

	public void Transform(ITransform2 xform, bool bApplyToSources, bool bRecomputePolygons = false)
	{
		foreach (Element vElement in vElements)
		{
			if (vElement is SmoothLoopElement)
			{
				SmoothLoopElement smoothLoopElement = vElement as SmoothLoopElement;
				if (bApplyToSources && smoothLoopElement.source != smoothLoopElement.polygon)
				{
					smoothLoopElement.source.Transform(xform);
				}
				if (bRecomputePolygons)
				{
					UpdateSampling(smoothLoopElement);
				}
				else
				{
					smoothLoopElement.polygon.Transform(xform);
				}
			}
			else if (vElement is SmoothCurveElement)
			{
				SmoothCurveElement smoothCurveElement = vElement as SmoothCurveElement;
				if (bApplyToSources && smoothCurveElement.source != smoothCurveElement.polyLine)
				{
					smoothCurveElement.source.Transform(xform);
				}
				if (bRecomputePolygons)
				{
					UpdateSampling(smoothCurveElement);
				}
				else
				{
					smoothCurveElement.polyLine.Transform(xform);
				}
			}
		}
	}

	public void PrintStats(string label = "")
	{
		Console.WriteLine("PlanarComplex Stats {0}", label);
		List<SmoothLoopElement> list = new List<SmoothLoopElement>(LoopsItr());
		List<SmoothCurveElement> list2 = new List<SmoothCurveElement>(CurvesItr());
		AxisAlignedBox2d axisAlignedBox2d = Bounds();
		Console.WriteLine("  Bounding Box  w: {0} h: {1}  range {2} ", axisAlignedBox2d.Width, axisAlignedBox2d.Height, axisAlignedBox2d);
		List<ComplexEndpoint2d> list3 = new List<ComplexEndpoint2d>(EndpointsItr());
		Console.WriteLine("  Closed Loops {0}  Open Curves {1}   Open Endpoints {2}", list.Count, list2.Count, list3.Count);
		int num = CountType(typeof(Segment2d));
		int num2 = CountType(typeof(Arc2d));
		int num3 = CountType(typeof(Circle2d));
		int num4 = CountType(typeof(NURBSCurve2));
		int num5 = CountType(typeof(Ellipse2d));
		int num6 = CountType(typeof(EllipseArc2d));
		int num7 = CountType(typeof(ParametricCurveSequence2));
		Console.WriteLine("  [Type Counts]   // {0} multi-curves", num7);
		Console.WriteLine("    segments {0,4}  arcs     {1,4}  circles      {2,4}", num, num2, num3);
		Console.WriteLine("    nurbs    {0,4}  ellipses {1,4}  ellipse-arcs {2,4}", num4, num5, num6);
	}

	public int CountType(Type t)
	{
		int num = 0;
		foreach (Element vElement in vElements)
		{
			if (vElement.source.GetType() == t)
			{
				num++;
			}
			if (vElement.source is IMultiCurve2d)
			{
				num += CountType(vElement.source as IMultiCurve2d, t);
			}
		}
		return num;
	}

	public int CountType(IMultiCurve2d curve, Type t)
	{
		int num = 0;
		foreach (IParametricCurve2d curf in curve.Curves)
		{
			if (curf.GetType() == t)
			{
				num++;
			}
			if (curf is IMultiCurve2d)
			{
				num += CountType(curf as IMultiCurve2d, t);
			}
		}
		return num;
	}
}
