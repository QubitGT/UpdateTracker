namespace Fusion;

public interface INetworkAssetSource<T>
{
	bool IsCompleted { get; }

	string Description { get; }

	void Acquire(bool synchronous);

	void Release();

	T WaitForResult();
}
