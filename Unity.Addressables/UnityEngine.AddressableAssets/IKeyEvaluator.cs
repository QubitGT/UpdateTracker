namespace UnityEngine.AddressableAssets;

public interface IKeyEvaluator
{
	object RuntimeKey { get; }

	bool RuntimeKeyIsValid();
}
