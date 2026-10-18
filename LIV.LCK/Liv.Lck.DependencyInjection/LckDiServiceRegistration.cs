using System;

namespace Liv.Lck.DependencyInjection;

public class LckDiServiceRegistration
{
	public enum ServiceLifetime
	{
		Transient,
		Singleton
	}

	public Type ServiceType { get; }

	public ServiceLifetime Lifetime { get; }

	public Type ImplementationType { get; }

	public object Instance { get; private set; }

	public Func<LckServiceProvider, object> Factory { get; }

	public Type ForwardToServiceType { get; }

	public LckDiServiceRegistration(Type serviceType, ServiceLifetime lifetime, Type implementationType)
	{
		ServiceType = serviceType;
		Lifetime = lifetime;
		ImplementationType = implementationType;
	}

	public LckDiServiceRegistration(Type serviceType, object instance)
	{
		ServiceType = serviceType;
		Lifetime = ServiceLifetime.Singleton;
		Instance = instance;
	}

	public LckDiServiceRegistration(Type serviceType, ServiceLifetime lifetime, Func<LckServiceProvider, object> factory)
	{
		ServiceType = serviceType;
		Lifetime = lifetime;
		Factory = factory;
	}

	public LckDiServiceRegistration(Type serviceType, Type forwardToServiceType)
	{
		ServiceType = serviceType;
		Lifetime = ServiceLifetime.Singleton;
		ForwardToServiceType = forwardToServiceType;
	}

	public void SetInstance(object instance)
	{
		if (Lifetime == ServiceLifetime.Singleton)
		{
			Instance = instance;
		}
	}
}
