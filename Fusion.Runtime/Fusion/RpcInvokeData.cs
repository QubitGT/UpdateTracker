namespace Fusion;

public struct RpcInvokeData
{
	public int Key;

	public int Sources;

	public int Targets;

	public RpcInvokeDelegate Delegate;

	public override string ToString()
	{
		return $"[{Key}, {Sources}, {Targets}, {Delegate}]";
	}
}
