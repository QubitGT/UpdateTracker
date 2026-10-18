using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Meta.XR.Acoustics;

[Serializable]
internal sealed class Spectrum : IEnumerable<Spectrum.Point>, IEnumerable
{
	[Serializable]
	internal struct Point : IComparable<Point>
	{
		[SerializeField]
		internal float frequency;

		[SerializeField]
		internal float data;

		internal Point(float frequency = 0f, float data = 0f)
		{
			this.frequency = frequency;
			this.data = data;
		}

		public int CompareTo(Point other)
		{
			return frequency.CompareTo(other.frequency);
		}

		public static implicit operator Point(Vector2 v)
		{
			return new Point(v.x, v.y);
		}

		public static implicit operator Vector2(Point point)
		{
			return new Vector2(point.frequency, point.data);
		}

		public override string ToString()
		{
			return $"({frequency}Hz, {data:0.00})";
		}
	}

	[SerializeField]
	internal int selection = int.MaxValue;

	[SerializeField]
	internal List<Point> points = new List<Point>();

	internal float this[float f]
	{
		get
		{
			if (points.Count > 0)
			{
				Point point = new Point(float.MinValue);
				Point point2 = new Point(float.MaxValue);
				foreach (Point point3 in points)
				{
					if (point3.frequency < f)
					{
						if (point3.frequency > point.frequency)
						{
							point = point3;
						}
					}
					else if (point3.frequency < point2.frequency)
					{
						point2 = point3;
					}
				}
				if (point.frequency == float.MinValue)
				{
					point.data = points.OrderBy((Point p) => p.frequency).First().data;
				}
				if (point2.frequency == float.MaxValue)
				{
					point2.data = points.OrderBy((Point p) => p.frequency).Last().data;
				}
				return Mathf.Lerp(point.data, point2.data, (f - point.frequency) / (point2.frequency - point.frequency));
			}
			return 0f;
		}
	}

	IEnumerator<Point> IEnumerable<Point>.GetEnumerator()
	{
		return points.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return points.GetEnumerator();
	}

	internal void Add(float frequency, float data)
	{
		points.Add(new Point(frequency, data));
	}

	internal Spectrum(Spectrum other = null)
	{
		if (other != null)
		{
			Clone(other);
		}
	}

	internal void Clone(Spectrum other)
	{
		if (this != other)
		{
			selection = other.selection;
			points = new List<Point>(other.points);
		}
	}

	internal void Sort()
	{
		if (points.Count != 0)
		{
			Point item = points[selection];
			points.Sort();
			selection = points.IndexOf(item);
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Point point in points)
		{
			stringBuilder.Append($"[{point.frequency}, {point.data}] ");
		}
		return stringBuilder.ToString();
	}
}
