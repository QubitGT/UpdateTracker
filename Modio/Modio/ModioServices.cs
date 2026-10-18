using System;
using System.Collections.Generic;
using System.Linq;

namespace Modio;

public static class ModioServices
{
	public interface IBindType<T>
	{
		Binding<T> FromInstance(T value, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null);

		Binding<T> FromMethod(Func<T> factory, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null);

		Binding<T> FromNew<TResolved>(ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null) where TResolved : T, new();

		Binding<T> FromNew(Type type, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null);

		Binding<T> WithOtherBinding<TOther>(Binding<TOther> binding, Func<bool> condition = null);

		IBindType<T> WithInterfaces<TI1>(Func<bool> condition = null);

		IBindType<T> WithInterfaces<TI1, TI2>(Func<bool> condition = null);

		IBindType<T> WithInterfaces<TI1, TI2, TI3>(Func<bool> condition = null);
	}

	public interface IResolveType<T>
	{
		event Action<T> OnNewBinding;

		T Resolve();

		bool TryResolve(out T value);

		IEnumerable<(T, ModioServicePriority)> ResolveAll();
	}

	private abstract class ServiceBindings
	{
		public abstract int BindingCount { get; }

		public abstract void RemoveAllWithPriority(ModioServicePriority priority);
	}

	public class Binding<T>
	{
		public readonly ModioServicePriority Priority;

		public readonly Func<bool> Condition;

		private readonly Func<T> _factory;

		private T _value;

		private bool _runningFactoryMethod;

		public Binding(T value, ModioServicePriority priority, Func<bool> condition = null)
		{
			_value = value;
			Priority = priority;
			Condition = condition;
		}

		public Binding(Func<T> factory, ModioServicePriority priority, Func<bool> condition = null)
		{
			_factory = factory;
			Priority = priority;
			Condition = condition;
		}

		public T Resolve()
		{
			if (_value != null || _factory == null)
			{
				return _value;
			}
			if (_runningFactoryMethod)
			{
				ModioLog.Error?.Log("Cyclic dependency detected when resolving type " + typeof(T).FullName + ". This will cause issues.");
				return default(T);
			}
			_runningFactoryMethod = true;
			try
			{
				_value = _factory();
			}
			finally
			{
				_runningFactoryMethod = false;
			}
			return _value;
		}
	}

	private class ServiceBindings<T> : ServiceBindings, IBindType<T>, IResolveType<T>
	{
		private class MultiBind : IBindType<T>
		{
			private readonly ServiceBindings<T> _coreBinding;

			private readonly Action<Binding<T>> _afterBinding;

			public MultiBind(ServiceBindings<T> coreBinding, Action<Binding<T>> afterBinding)
			{
				_coreBinding = coreBinding;
				_afterBinding = afterBinding;
			}

			public Binding<T> FromInstance(T value, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null)
			{
				return BindWith(_coreBinding.FromInstance(value, priority, condition));
			}

			public Binding<T> FromMethod(Func<T> factory, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null)
			{
				return BindWith(_coreBinding.FromMethod(factory, priority, condition));
			}

			public Binding<T> FromNew<TResolved>(ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null) where TResolved : T, new()
			{
				return BindWith(_coreBinding.FromNew<TResolved>(priority, condition));
			}

			public Binding<T> FromNew(Type type, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null)
			{
				return BindWith(_coreBinding.FromNew(type, priority, condition));
			}

			public Binding<T> WithOtherBinding<TOther>(Binding<TOther> binding, Func<bool> condition = null)
			{
				return BindWith(_coreBinding.WithOtherBinding(binding, condition));
			}

			public IBindType<T> WithInterfaces<TI1>(Func<bool> condition = null)
			{
				return new MultiBind(_coreBinding, delegate(Binding<T> b)
				{
					_afterBinding(b);
					Bind<TI1>().WithOtherBinding(b, condition);
				});
			}

			public IBindType<T> WithInterfaces<TI1, TI2>(Func<bool> condition = null)
			{
				return new MultiBind(_coreBinding, delegate(Binding<T> b)
				{
					_afterBinding(b);
					Bind<TI1>().WithOtherBinding(b, condition);
					Bind<TI2>().WithOtherBinding(b, condition);
				});
			}

			public IBindType<T> WithInterfaces<TI1, TI2, TI3>(Func<bool> condition = null)
			{
				return new MultiBind(_coreBinding, delegate(Binding<T> b)
				{
					_afterBinding(b);
					Bind<TI1>().WithOtherBinding(b, condition);
					Bind<TI2>().WithOtherBinding(b, condition);
					Bind<TI3>().WithOtherBinding(b, condition);
				});
			}

			private Binding<T> BindWith(Binding<T> core)
			{
				_afterBinding(core);
				return core;
			}
		}

		public readonly List<Binding<T>> Bindings = new List<Binding<T>>();

		public override int BindingCount => Bindings.Count;

		public event Action<T> OnNewBinding;

		public Binding<T> FromInstance(T value, ModioServicePriority priority = ModioServicePriority.DeveloperOverride, Func<bool> condition = null)
		{
			Binding<T> binding = new Binding<T>(value, priority, condition);
			Bindings.Add(binding);
			InvokeNewBindingIfHighestPriority(priority);
			return binding;
		}

		public Binding<T> FromMethod(Func<T> factory, ModioServicePriority priority, Func<bool> condition = null)
		{
			Binding<T> binding = new Binding<T>(factory, priority, condition);
			Bindings.Add(binding);
			InvokeNewBindingIfHighestPriority(priority);
			return binding;
		}

		public Binding<T> FromNew<TResolved>(ModioServicePriority priority, Func<bool> condition = null) where TResolved : T, new()
		{
			return FromMethod(() => (T)(object)new TResolved(), priority, condition);
		}

		public Binding<T> FromNew(Type type, ModioServicePriority priority, Func<bool> condition = null)
		{
			if (!typeof(T).IsAssignableFrom(type))
			{
				throw new ArgumentException("Type '" + type.FullName + "' is not assignable to '" + typeof(T).FullName + "'");
			}
			return FromMethod(() => (T)Activator.CreateInstance(type), priority, condition);
		}

		public Binding<T> WithOtherBinding<TOther>(Binding<TOther> binding, Func<bool> condition = null)
		{
			if (!typeof(T).IsAssignableFrom(typeof(TOther)))
			{
				throw new ArgumentException("Type '" + typeof(T).FullName + "' is not assignable to '" + typeof(TOther).FullName + "'");
			}
			if (condition == null)
			{
				condition = binding.Condition;
			}
			else if (binding.Condition != null)
			{
				condition = () => condition() && binding.Condition();
			}
			return FromMethod(() => (T)(object)binding.Resolve(), binding.Priority, condition);
		}

		public IBindType<T> WithInterfaces<TI1>(Func<bool> condition = null)
		{
			return new MultiBind(this, delegate(Binding<T> b)
			{
				Bind<TI1>().WithOtherBinding(b, condition);
			});
		}

		public IBindType<T> WithInterfaces<TI1, TI2>(Func<bool> condition = null)
		{
			return new MultiBind(this, delegate(Binding<T> b)
			{
				Bind<TI1>().WithOtherBinding(b, condition);
				Bind<TI2>().WithOtherBinding(b, condition);
			});
		}

		public IBindType<T> WithInterfaces<TI1, TI2, TI3>(Func<bool> condition = null)
		{
			return new MultiBind(this, delegate(Binding<T> b)
			{
				Bind<TI1>().WithOtherBinding(b, condition);
				Bind<TI2>().WithOtherBinding(b, condition);
				Bind<TI3>().WithOtherBinding(b, condition);
			});
		}

		public override void RemoveAllWithPriority(ModioServicePriority priority)
		{
			for (int num = Bindings.Count - 1; num >= 0; num--)
			{
				if (Bindings[num].Priority == priority)
				{
					Bindings.RemoveAt(num);
				}
			}
		}

		private void InvokeNewBindingIfHighestPriority(ModioServicePriority priority)
		{
			if (this.OnNewBinding == null)
			{
				return;
			}
			foreach (Binding<T> binding in Bindings)
			{
				if (binding.Priority > priority)
				{
					return;
				}
			}
			if (TryResolve(out var value))
			{
				this.OnNewBinding(value);
			}
		}

		public T Resolve()
		{
			if (!TryResolve(out var value))
			{
				throw new KeyNotFoundException("Could not resolve type " + typeof(T).FullName);
			}
			return value;
		}

		public bool TryResolve(out T value)
		{
			ModioServicePriority? modioServicePriority = null;
			Binding<T> binding = null;
			foreach (Binding<T> binding2 in Bindings)
			{
				if ((!modioServicePriority.HasValue || modioServicePriority.Value <= binding2.Priority) && (binding2.Condition == null || binding2.Condition()))
				{
					modioServicePriority = binding2.Priority;
					binding = binding2;
				}
			}
			if (!modioServicePriority.HasValue)
			{
				value = default(T);
				return false;
			}
			value = binding.Resolve();
			return true;
		}

		public IEnumerable<(T, ModioServicePriority)> ResolveAll()
		{
			return from b in Bindings
				where b.Condition == null || b.Condition()
				select (b.Resolve(), Priority: b.Priority);
		}
	}

	private static readonly Dictionary<Type, ServiceBindings> Bindings = new Dictionary<Type, ServiceBindings>();

	public static IBindType<T> Bind<T>()
	{
		if (!Bindings.TryGetValue(typeof(T), out var value))
		{
			value = (Bindings[typeof(T)] = new ServiceBindings<T>());
		}
		return (IBindType<T>)value;
	}

	public static void BindInstance<T>(T instance, ModioServicePriority priority = ModioServicePriority.DeveloperOverride)
	{
		Bind<T>().FromInstance(instance, priority);
	}

	public static void BindErrorMessage<T>(string message, ModioServicePriority priority = ModioServicePriority.Fallback)
	{
		Bind<T>().FromMethod(delegate
		{
			ModioLog.Error?.Log(message);
			throw new KeyNotFoundException("Could not resolve type " + typeof(T).FullName + ". " + message);
		}, priority);
	}

	internal static void RemoveAllBindingsWithPriority(ModioServicePriority priority)
	{
		foreach (Type item in new List<Type>(Bindings.Keys))
		{
			ServiceBindings serviceBindings = Bindings[item];
			serviceBindings.RemoveAllWithPriority(priority);
			if (serviceBindings.BindingCount == 0)
			{
				Bindings.Remove(item);
			}
		}
	}

	public static T Resolve<T>()
	{
		return GetBindings<T>().Resolve();
	}

	public static bool TryResolve<T>(out T result)
	{
		if (!Bindings.TryGetValue(typeof(T), out var value))
		{
			result = default(T);
			return false;
		}
		return ((ServiceBindings<T>)value).TryResolve(out result);
	}

	public static IResolveType<T> GetBindings<T>(bool createIfMissing = false)
	{
		if (!Bindings.TryGetValue(typeof(T), out var value))
		{
			if (!createIfMissing)
			{
				throw new KeyNotFoundException("Could not resolve type " + typeof(T).FullName);
			}
			value = (Bindings[typeof(T)] = new ServiceBindings<T>());
		}
		return (ServiceBindings<T>)value;
	}

	public static void AddBindingChangedListener<T>(Action<T> onNewValue, bool fireImmediatelyIfValueBound = true)
	{
		IResolveType<T> bindings = GetBindings<T>(createIfMissing: true);
		bindings.OnNewBinding += onNewValue;
		if (fireImmediatelyIfValueBound && bindings.TryResolve(out var value))
		{
			onNewValue(value);
		}
	}

	public static void RemoveBindingChangedListener<T>(Action<T> onNewValue)
	{
		GetBindings<T>(createIfMissing: true).OnNewBinding -= onNewValue;
	}
}
