using UnityEngine;

namespace Liv.Lck.Cosmetics;

public class LckLocalCosmeticDependantPlayerIdSupplier : MonoBehaviour, ILckCosmeticDependantPlayerIdSupplier
{
	[SerializeField]
	private string _playerId;

	public event PlayerIdUpdatedEvent PlayerIdUpdated;

	private void Start()
	{
		this.PlayerIdUpdated?.Invoke();
	}

	public virtual string GetPlayerId()
	{
		return _playerId;
	}

	public void UpdatePlayerId()
	{
		this.PlayerIdUpdated?.Invoke();
	}
}
