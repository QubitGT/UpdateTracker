using System.Threading;

namespace Fusion;

public struct AtomicInt
{
	private volatile int _value;

	public int Value => Thread.VolatileRead(ref _value);

	public AtomicInt(int value)
	{
		_value = value;
	}

	public int IncrementPost()
	{
		return Interlocked.Increment(ref _value) - 1;
	}

	public int IncrementPre()
	{
		return Interlocked.Increment(ref _value);
	}

	public int Decrement()
	{
		return Interlocked.Decrement(ref _value);
	}

	public int Exchange(int value)
	{
		return Interlocked.Exchange(ref _value, value);
	}

	public int CompareExchange(int value, int assumed)
	{
		return Interlocked.CompareExchange(ref _value, value, assumed);
	}
}
