using System;
using System.Buffers;
using System.Collections.Generic;

namespace VYaml.Internal;

internal class ReusableByteSequenceBuilder
{
	private readonly Stack<ReusableByteSequenceSegment> segmentPool = new Stack<ReusableByteSequenceSegment>();

	private readonly List<ReusableByteSequenceSegment> segments = new List<ReusableByteSequenceSegment>();

	public void Add(ReadOnlyMemory<byte> buffer, bool returnToPool)
	{
		if (!segmentPool.TryPop(out ReusableByteSequenceSegment result))
		{
			result = new ReusableByteSequenceSegment();
		}
		result.SetBuffer(buffer, returnToPool);
		segments.Add(result);
	}

	public bool TryGetSingleMemory(out ReadOnlyMemory<byte> memory)
	{
		if (segments.Count == 1)
		{
			memory = segments[0].Memory;
			return true;
		}
		memory = default(ReadOnlyMemory<byte>);
		return false;
	}

	public ReadOnlySequence<byte> Build()
	{
		if (segments.Count == 0)
		{
			return ReadOnlySequence<byte>.Empty;
		}
		if (segments.Count == 1)
		{
			return new ReadOnlySequence<byte>(segments[0].Memory);
		}
		long num = 0L;
		for (int i = 0; i < segments.Count; i++)
		{
			ReusableByteSequenceSegment nextSegment = ((i < segments.Count - 1) ? segments[i + 1] : null);
			segments[i].SetRunningIndexAndNext(num, nextSegment);
			num += segments[i].Memory.Length;
		}
		ReusableByteSequenceSegment startSegment = segments[0];
		List<ReusableByteSequenceSegment> list = segments;
		ReusableByteSequenceSegment reusableByteSequenceSegment = list[list.Count - 1];
		return new ReadOnlySequence<byte>(startSegment, 0, reusableByteSequenceSegment, reusableByteSequenceSegment.Memory.Length);
	}

	public void Reset()
	{
		foreach (ReusableByteSequenceSegment segment in segments)
		{
			segment.Reset();
			segmentPool.Push(segment);
		}
		segments.Clear();
	}
}
