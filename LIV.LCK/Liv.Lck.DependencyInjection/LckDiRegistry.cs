using System;
using System.Collections.Generic;

namespace Liv.Lck.DependencyInjection;

public class LckDiRegistry
{
	private static LckDiRegistry _instance;

	private LckDiCollection _collection = new LckDiCollection();

	private LckServiceProvider _provider;

	private LckMonoBehaviourDependencyInjector _lckMonoBehaviourDependencyInjector;

	public static LckDiRegistry Instance => _instance ?? (_instance = new LckDiRegistry());

	public void AddTransient<TService, TImplementation>() where TService : class where TImplementation : TService
	{
		_collection.AddTransient<TService, TImplementation>();
	}

	public void AddTransientFactory<TService>(Func<LckServiceProvider, TService> factory) where TService : class
	{
		_collection.AddTransientFactory(factory);
	}

	public void AddSingleton<TService, TImplementation>() where TService : class where TImplementation : TService
	{
		_collection.AddSingleton<TService, TImplementation>();
	}

	public void AddSingleton<TService>(TService instance) where TService : class
	{
		_collection.AddSingleton(instance);
	}

	public void AddSingletonFactory<TService>(Func<LckServiceProvider, TService> factory) where TService : class
	{
		_collection.AddSingletonFactory(factory);
	}

	public void AddSingletonForward<TService, TForwardTo>() where TService : class where TForwardTo : class, TService
	{
		_collection.AddSingletonForward<TService, TForwardTo>();
	}

	public T GetService<T>() where T : class
	{
		try
		{
			if (_provider == null)
			{
				LckLog.Log("Service provider not built yet, building now.", "GetService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 51);
				Build();
			}
			LckServiceProvider provider = _provider;
			return (provider != null) ? provider.GetService<T>() : null;
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: GetService failed for type " + typeof(T).Name + ". Exception: " + ex.Message, "GetService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 59);
			return null;
		}
	}

	public bool HasService<T>() where T : class
	{
		try
		{
			LckServiceProvider provider = _provider;
			return ((provider != null) ? provider.GetService<T>() : null) != null;
		}
		catch
		{
			return false;
		}
	}

	public LckMonoBehaviourDependencyInjector GetInjector()
	{
		return _lckMonoBehaviourDependencyInjector;
	}

	public void Build()
	{
		try
		{
			_provider = _collection.Build();
			_lckMonoBehaviourDependencyInjector = new LckMonoBehaviourDependencyInjector(_provider);
			LckLog.Log("LCK DI provider built successfully.", "Build", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 87);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Failed to build the service provider. Exception: " + ex.Message, "Build", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 91);
		}
	}

	public Dictionary<Type, LckDiServiceRegistration> GetRegistrations()
	{
		return _collection.GetRegistrations();
	}

	public void Reset()
	{
		try
		{
			_provider?.Dispose();
			_collection = new LckDiCollection();
			_provider = null;
			_lckMonoBehaviourDependencyInjector = null;
			LckLog.Log("LCK DI registry has been reset.", "Reset", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 109);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Failed to reset the DI registry. Exception: " + ex.Message, "Reset", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckDiRegistry.cs", 113);
		}
	}
}
