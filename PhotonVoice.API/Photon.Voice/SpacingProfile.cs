using System;
using System.Diagnostics;
using System.Linq;

namespace Photon.Voice;

internal class SpacingProfile
{
	private short[] buf;

	private bool[] info;

	private int capacity;

	private int ptr;

	private Stopwatch watch;

	private long watchLast;

	private bool flushed;

	public string Dump
	{
		get
		{
			if (watch == null)
			{
				return "Error: Profiler not started.";
			}
			string[] value = buf.Select((short v, int i) => (info[i] ? "-" : "") + v).ToArray();
			return "max=" + Max + " " + string.Join(",", value, ptr, buf.Length - ptr) + ", " + string.Join(",", value, 0, ptr);
		}
	}

	public int Max => buf.Select((short v) => Math.Abs(v)).Max();

	public SpacingProfile(int capacity)
	{
		this.capacity = capacity;
	}

	public void Start()
	{
		if (watch == null)
		{
			buf = new short[capacity];
			info = new bool[capacity];
			watch = Stopwatch.StartNew();
		}
	}

	public void Update(bool lost, bool flush)
	{
		if (watch != null)
		{
			if (flushed)
			{
				watchLast = watch.ElapsedMilliseconds;
			}
			long elapsedMilliseconds = watch.ElapsedMilliseconds;
			buf[ptr] = (short)(elapsedMilliseconds - watchLast);
			info[ptr] = lost;
			watchLast = elapsedMilliseconds;
			ptr++;
			if (ptr == buf.Length)
			{
				ptr = 0;
			}
			flushed = flush;
		}
	}
}
