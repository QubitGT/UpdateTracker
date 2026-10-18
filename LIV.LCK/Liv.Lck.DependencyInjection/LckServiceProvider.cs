using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Liv.Lck.DependencyInjection;

public class LckServiceProvider : IDisposable
{
	private readonly Dictionary<Type, LckDiServiceRegistration> _registrations;

	private bool _disposed;

	internal LckServiceProvider(Dictionary<Type, LckDiServiceRegistration> registrations)
	{
		_registrations = registrations;
	}

	public T GetService<T>() where T : class
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("LckServiceProvider");
		}
		try
		{
			return (T)ProvideService(typeof(T));
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Failed to get service of type " + typeof(T).Name + ". Exception: " + ex.Message, "GetService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 29);
			return null;
		}
	}

	public object GetService(Type serviceType)
	{
		try
		{
			return ProvideService(serviceType);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Failed to get service of type " + serviceType.Name + ". Exception: " + ex.Message, "GetService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 42);
			return null;
		}
	}

	private object ProvideService(Type serviceType)
	{
		if (!_registrations.TryGetValue(serviceType, out var value))
		{
			LckLog.LogError("LCK Error: Service of type " + serviceType.Name + " has not been registered.", "ProvideService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 51);
			throw new InvalidOperationException("Service of type " + serviceType.Name + " has not been registered.");
		}
		try
		{
			if (value.ForwardToServiceType != null)
			{
				return ProvideService(value.ForwardToServiceType);
			}
			if (value.Instance != null)
			{
				return value.Instance;
			}
			if (value.Factory != null)
			{
				object obj = value.Factory(this);
				if (value.Lifetime == LckDiServiceRegistration.ServiceLifetime.Singleton)
				{
					value.SetInstance(obj);
				}
				return obj;
			}
			ConstructorInfo[] constructors = value.ImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (constructors.Length < 1)
			{
				LckLog.LogError($"LCK Error: {value.ImplementationType} has no public constructors.", "ProvideService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 80);
				return null;
			}
			return CreateInstance(constructors, value);
		}
		catch (Exception ex)
		{
			LckLog.LogError("LCK Error: Failed to provide service " + serviceType.Name + ". Exception: " + ex.Message, "ProvideService", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 88);
			throw;
		}
	}

	private object CreateInstance(ConstructorInfo[] constructors, LckDiServiceRegistration registration)
	{
		ConstructorInfo constructorInfo = constructors.OrderByDescending((ConstructorInfo c) => c.GetParameters().Length).First();
		ParameterInfo[] parameters = constructorInfo.GetParameters();
		List<object> list = new List<object>();
		try
		{
			foreach (ParameterInfo parameterInfo in parameters)
			{
				object obj = ProvideService(parameterInfo.ParameterType);
				if (obj == null)
				{
					LckLog.LogError("LCK Error: Failed to resolve parameter '" + parameterInfo.Name + "' of type '" + parameterInfo.ParameterType.Name + "' for '" + registration.ImplementationType.Name + "'.", "CreateInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 107);
					return null;
				}
				list.Add(obj);
			}
			object obj2 = constructorInfo.Invoke(list.ToArray());
			if (registration.Lifetime == LckDiServiceRegistration.ServiceLifetime.Singleton)
			{
				registration.SetInstance(obj2);
			}
			LckLog.Log("Successfully instantiated " + registration.ImplementationType.Name + ".", "CreateInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 120);
			return obj2;
		}
		catch (Exception ex)
		{
			LckLog.LogError($"LCK Error: {registration.ImplementationType} failed to instantiate. Exception: {ex.InnerException?.Message ?? ex.Message}", "CreateInstance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 125);
			return null;
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		LckLog.Log("Disposing LCK Service Provider and all disposable singleton services.", "Dispose", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 134);
		foreach (LckDiServiceRegistration value in _registrations.Values)
		{
			if (value.Lifetime == LckDiServiceRegistration.ServiceLifetime.Singleton && value.Instance != null && value.Instance is IDisposable disposable)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception ex)
				{
					LckLog.LogError("LCK Error: Failed to dispose service of type " + value.Instance.GetType().Name + ". Exception: " + ex.Message, "Dispose", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Util\\DependencyInjection\\LckServiceProvider.cs", 148);
				}
			}
		}
		_disposed = true;
	}
}
