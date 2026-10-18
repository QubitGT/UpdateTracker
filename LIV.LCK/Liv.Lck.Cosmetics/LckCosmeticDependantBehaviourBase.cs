using System.Collections.Generic;
using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.Cosmetics;

public abstract class LckCosmeticDependantBehaviourBase : MonoBehaviour, ILckCosmeticDependant
{
	[InjectLck]
	private ILckCosmeticsManager _cosmeticsManager;

	[SerializeField]
	[Tooltip("Assign the CosmeticType of this asset, provided as a LckCosmeticType SO.")]
	private LckCosmeticType _cosmeticType;

	[Tooltip("The player ID supplier implementing ILckCosmeticDependantPlayerIdSupplier.")]
	[SerializeField]
	private GameObject _playerIdSupplier;

	private ILckCosmeticDependantPlayerIdSupplier _lckCosmeticDependantPlayerIdSupplier;

	public abstract string PlayerId { get; set; }

	public string GetCosmeticType()
	{
		if (_cosmeticType == null)
		{
			LckLog.LogWarning("LCK: CosmeticType is not assigned on this dependant!", "GetCosmeticType", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticDependantBase.cs", 30);
			return string.Empty;
		}
		return _cosmeticType.TypeValue;
	}

	public abstract void OnCosmeticLoaded(List<Object> assets);

	public virtual void Awake()
	{
		_lckCosmeticDependantPlayerIdSupplier = _playerIdSupplier.GetComponent<ILckCosmeticDependantPlayerIdSupplier>();
		if (_lckCosmeticDependantPlayerIdSupplier == null)
		{
			LckLog.LogError("LCK: LckCosmeticDependantBehaviour has no _lckCosmeticDependantPlayerIdSupplier set. Cosmetic dependants will fail to load for: " + base.name, "Awake", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckCosmeticDependantBase.cs", 46);
			return;
		}
		_lckCosmeticDependantPlayerIdSupplier.PlayerIdUpdated += delegate
		{
			OnCosmeticReset();
			PlayerId = _lckCosmeticDependantPlayerIdSupplier.GetPlayerId();
			_cosmeticsManager?.RegisterDependant(this);
		};
	}

	public abstract void OnCosmeticReset();

	public virtual void OnDestroy()
	{
		_cosmeticsManager?.UnregisterDependant(this);
	}
}
