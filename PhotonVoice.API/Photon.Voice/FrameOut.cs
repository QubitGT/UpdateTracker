namespace Photon.Voice;

public class FrameOut<T>
{
	public T[] Buf { get; private set; }

	public bool EndOfStream { get; private set; }

	public FrameOut(T[] buf, bool endOfStream)
	{
		Set(buf, endOfStream);
	}

	public FrameOut<T> Set(T[] buf, bool endOfStream)
	{
		Buf = buf;
		EndOfStream = endOfStream;
		return this;
	}
}
