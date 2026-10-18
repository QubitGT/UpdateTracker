using System;
using System.Collections.Generic;
using System.Linq;
using Modio.Mods;

namespace Modio.Users;

public class ModRepository : IDisposable
{
	private readonly HashSet<Mod> _created = new HashSet<Mod>();

	private readonly HashSet<Mod> _subscribed = new HashSet<Mod>();

	private readonly HashSet<Mod> _purchased = new HashSet<Mod>();

	private readonly HashSet<Mod> _disabled = new HashSet<Mod>();

	public bool HasGotSubscriptions { get; internal set; }

	internal event Action OnContentsChanged;

	public IEnumerable<Mod> GetCreatedMods()
	{
		return _created;
	}

	public IEnumerable<Mod> GetSubscribed()
	{
		return _subscribed;
	}

	public IEnumerable<Mod> GetPurchased()
	{
		return _purchased;
	}

	public IEnumerable<Mod> GetDisabled()
	{
		return _disabled;
	}

	internal ModRepository()
	{
		Mod.AddChangeListener(ModChangeType.IsSubscribed, OnModSubscriptionChange);
		Mod.AddChangeListener(ModChangeType.IsEnabled, OnModEnabledChange);
		Mod.AddChangeListener(ModChangeType.IsPurchased, OnModPurchasedChange);
		ModioClient.OnShutdown += Dispose;
	}

	private void OnModSubscriptionChange(Mod mod, ModChangeType changeType)
	{
		bool flag = false;
		if (mod.IsSubscribed)
		{
			flag |= _subscribed.Add(mod);
		}
		else
		{
			flag |= _subscribed.Remove(mod);
			flag |= _disabled.Remove(mod);
		}
		if (flag)
		{
			this.OnContentsChanged?.Invoke();
		}
	}

	private void OnModEnabledChange(Mod mod, ModChangeType changeType)
	{
		bool flag = false;
		if (mod.IsEnabled ? (flag | _disabled.Remove(mod)) : (flag | _disabled.Add(mod)))
		{
			this.OnContentsChanged?.Invoke();
		}
	}

	private void OnModPurchasedChange(Mod mod, ModChangeType changeType)
	{
		bool flag = false;
		if ((!mod.IsPurchased) ? (flag | _purchased.Remove(mod)) : (flag | _purchased.Add(mod)))
		{
			this.OnContentsChanged?.Invoke();
		}
	}

	public bool IsSubscribed(ModId modId)
	{
		return _subscribed.Any((Mod mod) => mod.Id == modId);
	}

	public bool IsDisabled(ModId modId)
	{
		return _disabled.Any((Mod mod) => mod.Id == modId);
	}

	public bool IsPurchased(ModId modId)
	{
		return _purchased.Any((Mod mod) => mod.Id == modId);
	}

	public void Dispose()
	{
		Mod.RemoveChangeListener(ModChangeType.IsSubscribed, OnModSubscriptionChange);
		Mod.RemoveChangeListener(ModChangeType.IsEnabled, OnModEnabledChange);
		Mod.RemoveChangeListener(ModChangeType.IsPurchased, OnModPurchasedChange);
		ModioClient.OnShutdown -= Dispose;
	}
}
