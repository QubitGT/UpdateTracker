using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Scripting;

namespace Fusion;

[Preserve]
public class FusionGlobalScriptableObjectAddressAttribute : FusionGlobalScriptableObjectSourceAttribute
{
	public string Address { get; }

	public FusionGlobalScriptableObjectAddressAttribute(Type objectType, string address)
		: base(objectType)
	{
		Address = address;
	}

	public override FusionGlobalScriptableObjectLoadResult Load(Type type)
	{
		AsyncOperationHandle<FusionGlobalScriptableObject> op = Addressables.LoadAssetAsync<FusionGlobalScriptableObject>(Address);
		FusionGlobalScriptableObject obj = op.WaitForCompletion();
		if (op.Status == AsyncOperationStatus.Succeeded)
		{
			return new FusionGlobalScriptableObjectLoadResult(obj, delegate
			{
				Addressables.Release(op);
			});
		}
		return default(FusionGlobalScriptableObjectLoadResult);
	}
}
