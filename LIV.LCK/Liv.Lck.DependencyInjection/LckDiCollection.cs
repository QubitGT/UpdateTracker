using System;
using System.Collections.Generic;

namespace Liv.Lck.DependencyInjection;

public class LckDiCollection
{
	private readonly Dictionary<Type, LckDiServiceRegistration> _registrations = new Dictionary<Type, LckDiServiceRegistration>();

	public void AddTransient<TService, TImplementation>() where TService : class where TImplementation : TService
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), LckDiServiceRegistration.ServiceLifetime.Transient, typeof(TImplementation));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding transient " + typeof(TService).Name + ": " + ex.Message, "AddTransient", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 22);
		}
	}

	public void AddTransientFactory<TService>(Func<LckServiceProvider, TService> factory) where TService : class
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), LckDiServiceRegistration.ServiceLifetime.Transient, (LckServiceProvider p) => factory(p));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding transient factory " + typeof(TService).Name + ": " + ex.Message, "AddTransientFactory", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 38);
		}
	}

	public void AddSingleton<TService, TImplementation>() where TService : class where TImplementation : TService
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), LckDiServiceRegistration.ServiceLifetime.Singleton, typeof(TImplementation));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding singleton " + typeof(TService).Name + ": " + ex.Message, "AddSingleton", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 53);
		}
	}

	public void AddSingletonFactory<TService>(Func<LckServiceProvider, TService> factory) where TService : class
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), LckDiServiceRegistration.ServiceLifetime.Singleton, (LckServiceProvider p) => factory(p));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding singleton factory " + typeof(TService).Name + ": " + ex.Message, "AddSingletonFactory", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 69);
		}
	}

	public void AddSingleton<TService>(TService instance) where TService : class
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), instance);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding singleton instance " + typeof(TService).Name + ": " + ex.Message, "AddSingleton", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 84);
		}
	}

	public void AddSingletonForward<TService, TForwardTo>() where TService : class where TForwardTo : class, TService
	{
		try
		{
			_registrations[typeof(TService)] = new LckDiServiceRegistration(typeof(TService), typeof(TForwardTo));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error adding singleton forward for " + typeof(TService).Name + ": " + ex.Message, "AddSingletonForward", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 99);
		}
	}

	public Dictionary<Type, LckDiServiceRegistration> GetRegistrations()
	{
		return _registrations;
	}

	public LckDiServiceRegistration GetRegistration(Type serviceType)
	{
		_registrations.TryGetValue(serviceType, out var value);
		return value;
	}

	public LckServiceProvider Build()
	{
		LckLog.Log("Building LCK Service Provider.", "Build", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiCollection.cs", 116);
		return new LckServiceProvider(new Dictionary<Type, LckDiServiceRegistration>(_registrations));
	}
}
