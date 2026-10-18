using System;
using System.Collections.Generic;

namespace Photon.Voice;

public class Framer<T>
{
	private T[] frame;

	private int sizeofT;

	private int framePos;

	public Framer(int frameSize)
	{
		frame = new T[frameSize];
		T[] array = new T[1];
		if (array[0] is byte)
		{
			sizeofT = 1;
			return;
		}
		if (array[0] is short)
		{
			sizeofT = 2;
			return;
		}
		if (array[0] is float)
		{
			sizeofT = 4;
			return;
		}
		throw new Exception("Input data type is not supported: " + array[0].GetType());
	}

	public int Count(int bufLen)
	{
		return (bufLen + framePos) / frame.Length;
	}

	public IEnumerable<T[]> Frame(T[] buf)
	{
		if (frame.Length == buf.Length && framePos == 0)
		{
			yield return buf;
			yield break;
		}
		int bufPos = 0;
		while (frame.Length - framePos <= buf.Length - bufPos)
		{
			int num = frame.Length - framePos;
			Buffer.BlockCopy(buf, bufPos * sizeofT, frame, framePos * sizeofT, num * sizeofT);
			bufPos += num;
			framePos = 0;
			yield return frame;
		}
		if (bufPos != buf.Length)
		{
			int num2 = buf.Length - bufPos;
			Buffer.BlockCopy(buf, bufPos * sizeofT, frame, framePos * sizeofT, num2 * sizeofT);
			framePos += num2;
		}
	}
}
