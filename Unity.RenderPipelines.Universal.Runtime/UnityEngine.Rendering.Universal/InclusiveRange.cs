using System;

namespace UnityEngine.Rendering.Universal;

internal struct InclusiveRange
{
	public short start;

	public short end;

	public bool isEmpty => end < start;

	public static InclusiveRange empty => new InclusiveRange(short.MaxValue, short.MinValue);

	public InclusiveRange(short startEnd)
	{
		start = startEnd;
		end = startEnd;
	}

	public InclusiveRange(short start, short end)
	{
		this.start = start;
		this.end = end;
	}

	public void Expand(short index)
	{
		start = Math.Min(start, index);
		end = Math.Max(end, index);
	}

	public void Clamp(short min, short max)
	{
		start = Math.Max(min, start);
		end = Math.Min(max, end);
	}

	public bool Contains(short index)
	{
		if (index >= start)
		{
			return index <= end;
		}
		return false;
	}

	public static InclusiveRange Merge(InclusiveRange a, InclusiveRange b)
	{
		return new InclusiveRange(Math.Min(a.start, b.start), Math.Max(a.end, b.end));
	}

	public override string ToString()
	{
		return $"[{start}, {end}]";
	}
}
