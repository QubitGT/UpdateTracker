using System.Collections.Concurrent;

namespace VYaml.Parser;

internal class ScalarPool
{
	public static readonly ScalarPool Shared = new ScalarPool();

	private readonly ConcurrentQueue<Scalar> queue = new ConcurrentQueue<Scalar>();

	public Scalar Rent()
	{
		if (queue.TryDequeue(out Scalar result))
		{
			return result;
		}
		return new Scalar(256);
	}

	public void Return(Scalar scalar)
	{
		scalar.Clear();
		queue.Enqueue(scalar);
	}
}
