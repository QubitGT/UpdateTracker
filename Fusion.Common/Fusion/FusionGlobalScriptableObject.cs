#define DEBUG
#define TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion;

public abstract class FusionGlobalScriptableObject : FusionScriptableObject
{
	[CompilerGenerated]
	private sealed class _003CGetAssemblyAttributes_003Ed__0<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : Attribute
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Assembly[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Assembly _003Cassembly_003E5__3;

		private IEnumerator<T> _003C_003Es__4;

		private T _003Cattr_003E5__5;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetAssemblyAttributes_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003Es__1 = null;
			_003Cassembly_003E5__3 = null;
			_003C_003Es__4 = null;
			_003Cattr_003E5__5 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					_003Cattr_003E5__5 = null;
					goto IL_00b8;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = AppDomain.CurrentDomain.GetAssemblies();
				_003C_003Es__2 = 0;
				goto IL_00e9;
				IL_00b8:
				if (_003C_003Es__4.MoveNext())
				{
					_003Cattr_003E5__5 = _003C_003Es__4.Current;
					_003C_003E2__current = _003Cattr_003E5__5;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003Es__4 = null;
				_003Cassembly_003E5__3 = null;
				_003C_003Es__2++;
				goto IL_00e9;
				IL_00e9:
				if (_003C_003Es__2 < _003C_003Es__1.Length)
				{
					_003Cassembly_003E5__3 = _003C_003Es__1[_003C_003Es__2];
					_003C_003Es__4 = _003Cassembly_003E5__3.GetCustomAttributes<T>().GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00b8;
				}
				_003C_003Es__1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003Es__4 != null)
			{
				_003C_003Es__4.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAssemblyAttributes_003Ed__0<T>(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private static readonly Lazy<FusionGlobalScriptableObjectSourceAttribute[]> s_sourceAttributes = new Lazy<FusionGlobalScriptableObjectSourceAttribute[]>(() => (from x in GetAssemblyAttributes<FusionGlobalScriptableObjectSourceAttribute>()
		orderby x.Order
		select x).ToArray());

	internal static FusionGlobalScriptableObjectSourceAttribute[] SourceAttributes => s_sourceAttributes.Value;

	[IteratorStateMachine(typeof(_003CGetAssemblyAttributes_003Ed__0<>))]
	private static IEnumerable<T> GetAssemblyAttributes<T>() where T : Attribute
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAssemblyAttributes_003Ed__0<T>(-2);
	}
}
public abstract class FusionGlobalScriptableObject<T> : FusionGlobalScriptableObject where T : FusionGlobalScriptableObject<T>
{
	private static T s_instance;

	private static FusionGlobalScriptableObjectUnloadDelegate s_unloadHandler;

	public bool IsGlobal { get; private set; }

	private static string LogPrefix => "[Global " + typeof(T).Name + "]: ";

	protected static T GlobalInternal
	{
		get
		{
			T orLoadGlobalInstance = GetOrLoadGlobalInstance();
			if ((object)orLoadGlobalInstance == null)
			{
				throw new InvalidOperationException("Failed to load " + typeof(T).Name + ". If this happens in edit mode, make sure Fusion is properly installed in the Fusion HUB. Otherwise, if the default path does not exist or does not point to a Resource, you need to use FusionGlobalScriptableObjectAttribute attribute to point to a method that will perform the loading.");
			}
			return orLoadGlobalInstance;
		}
		set
		{
			if (!(value == s_instance))
			{
				SetGlobalInternal(value, null);
			}
		}
	}

	protected static bool IsGlobalLoadedInternal => s_instance != null;

	protected virtual void OnLoadedAsGlobal()
	{
	}

	protected virtual void OnUnloadedAsGlobal(bool destroyed)
	{
	}

	private static string AsId(FusionGlobalScriptableObject<T> obj)
	{
		return obj ? $"[IID:{obj.GetInstanceID()}]" : "null";
	}

	protected virtual void OnDisable()
	{
		if (!IsGlobal)
		{
			InternalLogStreams.LogTrace?.Log(LogPrefix + "OnDisable called for " + AsId(this) + ", but is not global");
			return;
		}
		if (s_unloadHandler != null)
		{
			InternalLogStreams.LogTrace?.Log(LogPrefix + "OnDisable called for " + AsId(this) + ", setting global instance to null. The unload handler is still set, not going to be used.");
		}
		else
		{
			InternalLogStreams.LogTrace?.Log(LogPrefix + "OnDisable called for " + AsId(this) + ", setting global instance to null.");
		}
		Assert.Check((object)this == s_instance, "Expected this to be the global instance");
		s_instance = null;
		s_unloadHandler = null;
		IsGlobal = false;
		OnUnloadedAsGlobal(destroyed: true);
	}

	protected static bool TryGetGlobalInternal(out T global)
	{
		T orLoadGlobalInstance = GetOrLoadGlobalInstance();
		if ((object)orLoadGlobalInstance == null)
		{
			global = null;
			return false;
		}
		global = orLoadGlobalInstance;
		return true;
	}

	protected static bool UnloadGlobalInternal()
	{
		T val = s_instance;
		if (!val)
		{
			return false;
		}
		Assert.Check(val.IsGlobal);
		try
		{
			if (s_unloadHandler != null)
			{
				InternalLogStreams.LogTrace?.Log(LogPrefix + " Unloading global instance " + AsId(val) + " with unloader");
				FusionGlobalScriptableObjectUnloadDelegate fusionGlobalScriptableObjectUnloadDelegate = s_unloadHandler;
				s_unloadHandler = null;
				fusionGlobalScriptableObjectUnloadDelegate(val);
			}
			else
			{
				InternalLogStreams.LogTrace?.Log(LogPrefix + " Instance " + AsId(val) + " has no unloader, simply nulling it out");
			}
		}
		finally
		{
			s_instance = null;
			if (val.IsGlobal)
			{
				val.IsGlobal = false;
				val.OnUnloadedAsGlobal(destroyed: false);
			}
		}
		return true;
	}

	private static T GetOrLoadGlobalInstance()
	{
		if ((bool)s_instance)
		{
			return s_instance;
		}
		T val = null;
		FusionGlobalScriptableObjectUnloadDelegate unloadHandler = null;
		val = LoadPlayerInstance(out unloadHandler);
		if ((bool)val)
		{
			SetGlobalInternal(val, unloadHandler);
		}
		return val;
	}

	private static T LoadPlayerInstance(out FusionGlobalScriptableObjectUnloadDelegate unloadHandler)
	{
		FusionGlobalScriptableObjectSourceAttribute[] sourceAttributes = FusionGlobalScriptableObject.SourceAttributes;
		foreach (FusionGlobalScriptableObjectSourceAttribute fusionGlobalScriptableObjectSourceAttribute in sourceAttributes)
		{
			if ((!Application.isEditor || Application.isPlaying || fusionGlobalScriptableObjectSourceAttribute.AllowEditMode) && (!(fusionGlobalScriptableObjectSourceAttribute.ObjectType != typeof(T)) || typeof(T).IsSubclassOf(fusionGlobalScriptableObjectSourceAttribute.ObjectType)))
			{
				FusionGlobalScriptableObjectLoadResult fusionGlobalScriptableObjectLoadResult = fusionGlobalScriptableObjectSourceAttribute.Load(typeof(T));
				if ((bool)fusionGlobalScriptableObjectLoadResult.Object)
				{
					T val = (T)fusionGlobalScriptableObjectLoadResult.Object;
					unloadHandler = fusionGlobalScriptableObjectLoadResult.Unloader;
					InternalLogStreams.LogTrace?.Log($"{LogPrefix} Loader {fusionGlobalScriptableObjectSourceAttribute} was used to load {AsId(val)}, has unloader: {unloadHandler != null}");
					return val;
				}
				if (!fusionGlobalScriptableObjectSourceAttribute.AllowFallback)
				{
					break;
				}
			}
		}
		InternalLogStreams.LogTrace?.Log(LogPrefix + " No source attribute was able to load the global instance");
		unloadHandler = null;
		return null;
	}

	private static void SetGlobalInternal(T value, FusionGlobalScriptableObjectUnloadDelegate unloadHandler)
	{
		if ((bool)s_instance)
		{
			throw new InvalidOperationException("Failed to set " + typeof(T).Name + " as global. A global instance is already loaded - it needs to be unloaded first");
		}
		Assert.Check(value, "Expected value to be non-null");
		if ((object)s_instance == null)
		{
			Assert.Check(s_unloadHandler == null, "Expected unload handler to be null");
		}
		if ((bool)value)
		{
			s_instance = value;
			s_unloadHandler = unloadHandler;
			s_instance.IsGlobal = true;
			s_instance.OnLoadedAsGlobal();
		}
	}
}
