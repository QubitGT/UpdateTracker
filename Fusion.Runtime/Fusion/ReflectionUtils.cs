using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fusion;

public static class ReflectionUtils
{
	[CompilerGenerated]
	private sealed class _003CGetAllNetworkBehaviourTypes_003Ed__5 : IEnumerable<Type>, IEnumerable, IEnumerator<Type>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Type _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Type> _003C_003Es__1;

		private Type _003Ctype_003E5__2;

		Type IEnumerator<Type>.Current
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
		public _003CGetAllNetworkBehaviourTypes_003Ed__5(int _003C_003E1__state)
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
			_003Ctype_003E5__2 = null;
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
					goto IL_008c;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = GetAllSimulationBehaviourTypes().GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_0094;
				IL_008c:
				_003Ctype_003E5__2 = null;
				goto IL_0094;
				IL_0094:
				if (_003C_003Es__1.MoveNext())
				{
					_003Ctype_003E5__2 = _003C_003Es__1.Current;
					if (typeof(NetworkBehaviour).IsAssignableFrom(_003Ctype_003E5__2))
					{
						_003C_003E2__current = _003Ctype_003E5__2;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_008c;
				}
				_003C_003Em__Finally1();
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
			if (_003C_003Es__1 != null)
			{
				_003C_003Es__1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllNetworkBehaviourTypes_003Ed__5(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Type>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetAllSimulationBehaviourTypes_003Ed__3 : IEnumerable<Type>, IEnumerable, IEnumerator<Type>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Type _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Assembly[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Assembly _003Casm_003E5__3;

		private Type[] _003C_003Es__4;

		private int _003C_003Es__5;

		private Type _003Ctype_003E5__6;

		Type IEnumerator<Type>.Current
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
		public _003CGetAllSimulationBehaviourTypes_003Ed__3(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Casm_003E5__3 = null;
			_003C_003Es__4 = null;
			_003Ctype_003E5__6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_00ba;
			}
			_003C_003E1__state = -1;
			_003C_003Es__1 = AppDomain.CurrentDomain.GetAssemblies();
			_003C_003Es__2 = 0;
			goto IL_00fd;
			IL_00d0:
			if (_003C_003Es__5 < _003C_003Es__4.Length)
			{
				_003Ctype_003E5__6 = _003C_003Es__4[_003C_003Es__5];
				if (typeof(SimulationBehaviour).IsAssignableFrom(_003Ctype_003E5__6))
				{
					_003C_003E2__current = _003Ctype_003E5__6;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_00ba;
			}
			_003C_003Es__4 = null;
			_003Casm_003E5__3 = null;
			_003C_003Es__2++;
			goto IL_00fd;
			IL_00ba:
			_003Ctype_003E5__6 = null;
			_003C_003Es__5++;
			goto IL_00d0;
			IL_00fd:
			if (_003C_003Es__2 < _003C_003Es__1.Length)
			{
				_003Casm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
				_003C_003Es__4 = _003Casm_003E5__3.GetTypes();
				_003C_003Es__5 = 0;
				goto IL_00d0;
			}
			_003C_003Es__1 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllSimulationBehaviourTypes_003Ed__3(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Type>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetAllWeavedAssemblies_003Ed__2 : IEnumerable<Assembly>, IEnumerable, IEnumerator<Assembly>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Assembly _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Assembly[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Assembly _003Casm_003E5__3;

		private NetworkAssemblyWeavedAttribute _003Cattr_003E5__4;

		Assembly IEnumerator<Assembly>.Current
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
		public _003CGetAllWeavedAssemblies_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Casm_003E5__3 = null;
			_003Cattr_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_008a;
			}
			_003C_003E1__state = -1;
			_003C_003Es__1 = AppDomain.CurrentDomain.GetAssemblies();
			_003C_003Es__2 = 0;
			goto IL_00a7;
			IL_008a:
			_003Cattr_003E5__4 = null;
			_003Casm_003E5__3 = null;
			_003C_003Es__2++;
			goto IL_00a7;
			IL_00a7:
			if (_003C_003Es__2 < _003C_003Es__1.Length)
			{
				_003Casm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
				_003Cattr_003E5__4 = _003Casm_003E5__3.GetCustomAttribute<NetworkAssemblyWeavedAttribute>();
				if (_003Cattr_003E5__4 != null)
				{
					_003C_003E2__current = _003Casm_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_008a;
			}
			_003C_003Es__1 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Assembly> IEnumerable<Assembly>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllWeavedAssemblies_003Ed__2(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Assembly>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetAllWeavedNetworkBehaviourTypes_003Ed__6 : IEnumerable<Type>, IEnumerable, IEnumerator<Type>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Type _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Type> _003C_003Es__1;

		private Type _003Ctype_003E5__2;

		Type IEnumerator<Type>.Current
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
		public _003CGetAllWeavedNetworkBehaviourTypes_003Ed__6(int _003C_003E1__state)
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
			_003Ctype_003E5__2 = null;
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
					goto IL_008c;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = GetAllWeavedSimulationBehaviourTypes().GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_0094;
				IL_008c:
				_003Ctype_003E5__2 = null;
				goto IL_0094;
				IL_0094:
				if (_003C_003Es__1.MoveNext())
				{
					_003Ctype_003E5__2 = _003C_003Es__1.Current;
					if (typeof(NetworkBehaviour).IsAssignableFrom(_003Ctype_003E5__2))
					{
						_003C_003E2__current = _003Ctype_003E5__2;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_008c;
				}
				_003C_003Em__Finally1();
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
			if (_003C_003Es__1 != null)
			{
				_003C_003Es__1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllWeavedNetworkBehaviourTypes_003Ed__6(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Type>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetAllWeavedSimulationBehaviourTypes_003Ed__4 : IEnumerable<Type>, IEnumerable, IEnumerator<Type>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Type _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Assembly> _003C_003Es__1;

		private Assembly _003Casm_003E5__2;

		private Type[] _003C_003Es__3;

		private int _003C_003Es__4;

		private Type _003Ctype_003E5__5;

		Type IEnumerator<Type>.Current
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
		public _003CGetAllWeavedSimulationBehaviourTypes_003Ed__4(int _003C_003E1__state)
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
			_003Casm_003E5__2 = null;
			_003C_003Es__3 = null;
			_003Ctype_003E5__5 = null;
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
					goto IL_00c1;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = GetAllWeavedAssemblies().GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00f6;
				IL_00d7:
				if (_003C_003Es__4 < _003C_003Es__3.Length)
				{
					_003Ctype_003E5__5 = _003C_003Es__3[_003C_003Es__4];
					if (typeof(SimulationBehaviour).IsAssignableFrom(_003Ctype_003E5__5))
					{
						_003C_003E2__current = _003Ctype_003E5__5;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_00c1;
				}
				_003C_003Es__3 = null;
				_003Casm_003E5__2 = null;
				goto IL_00f6;
				IL_00c1:
				_003Ctype_003E5__5 = null;
				_003C_003Es__4++;
				goto IL_00d7;
				IL_00f6:
				if (_003C_003Es__1.MoveNext())
				{
					_003Casm_003E5__2 = _003C_003Es__1.Current;
					_003C_003Es__3 = _003Casm_003E5__2.GetTypes();
					_003C_003Es__4 = 0;
					goto IL_00d7;
				}
				_003C_003Em__Finally1();
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
			if (_003C_003Es__1 != null)
			{
				_003C_003Es__1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllWeavedSimulationBehaviourTypes_003Ed__4(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Type>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetAllWeaverGeneratedTypes_003Ed__7 : IEnumerable<Type>, IEnumerable, IEnumerator<Type>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Type _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Assembly> _003C_003Es__1;

		private Assembly _003Casm_003E5__2;

		private Type[] _003C_003Es__3;

		private int _003C_003Es__4;

		private Type _003Ctype_003E5__5;

		private WeaverGeneratedAttribute _003CweaverGen_003E5__6;

		Type IEnumerator<Type>.Current
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
		public _003CGetAllWeaverGeneratedTypes_003Ed__7(int _003C_003E1__state)
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
			_003Casm_003E5__2 = null;
			_003C_003Es__3 = null;
			_003Ctype_003E5__5 = null;
			_003CweaverGen_003E5__6 = null;
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
					goto IL_00c6;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = GetAllWeavedAssemblies().GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_0102;
				IL_00e3:
				if (_003C_003Es__4 < _003C_003Es__3.Length)
				{
					_003Ctype_003E5__5 = _003C_003Es__3[_003C_003Es__4];
					_003CweaverGen_003E5__6 = _003Ctype_003E5__5.GetCustomAttribute<WeaverGeneratedAttribute>();
					if (_003CweaverGen_003E5__6 != null)
					{
						_003C_003E2__current = _003Ctype_003E5__5;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_00c6;
				}
				_003C_003Es__3 = null;
				_003Casm_003E5__2 = null;
				goto IL_0102;
				IL_00c6:
				_003CweaverGen_003E5__6 = null;
				_003Ctype_003E5__5 = null;
				_003C_003Es__4++;
				goto IL_00e3;
				IL_0102:
				if (_003C_003Es__1.MoveNext())
				{
					_003Casm_003E5__2 = _003C_003Es__1.Current;
					_003C_003Es__3 = _003Casm_003E5__2.GetTypes();
					_003C_003Es__4 = 0;
					goto IL_00e3;
				}
				_003C_003Em__Finally1();
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
			if (_003C_003Es__1 != null)
			{
				_003C_003Es__1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CGetAllWeaverGeneratedTypes_003Ed__7(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Type>)this).GetEnumerator();
		}
	}

	public static T GetCustomAttributeOrThrow<T>(this MemberInfo member, bool inherit) where T : Attribute
	{
		object[] customAttributes = member.GetCustomAttributes(typeof(T), inherit);
		if (customAttributes.Length == 0)
		{
			throw new ArgumentOutOfRangeException("T", $"{member} has no attribute {typeof(T)}");
		}
		if (customAttributes.Length > 1)
		{
			throw new InvalidOperationException($"{member} has more than one attribute {typeof(T)}");
		}
		return (T)customAttributes[0];
	}

	public static NetworkBehaviourWeavedAttribute GetWeavedAttributeOrThrow(Type type)
	{
		try
		{
			return type.GetCustomAttributeOrThrow<NetworkBehaviourWeavedAttribute>(inherit: false);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new InvalidOperationException(string.Format("Type {0} has not been weaved. Has the assembly {1} been added to {2}?", type, type.Assembly.GetName().Name, "NetworkProjectConfig"));
		}
	}

	[IteratorStateMachine(typeof(_003CGetAllWeavedAssemblies_003Ed__2))]
	public static IEnumerable<Assembly> GetAllWeavedAssemblies()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllWeavedAssemblies_003Ed__2(-2);
	}

	[IteratorStateMachine(typeof(_003CGetAllSimulationBehaviourTypes_003Ed__3))]
	public static IEnumerable<Type> GetAllSimulationBehaviourTypes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllSimulationBehaviourTypes_003Ed__3(-2);
	}

	[IteratorStateMachine(typeof(_003CGetAllWeavedSimulationBehaviourTypes_003Ed__4))]
	public static IEnumerable<Type> GetAllWeavedSimulationBehaviourTypes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllWeavedSimulationBehaviourTypes_003Ed__4(-2);
	}

	[IteratorStateMachine(typeof(_003CGetAllNetworkBehaviourTypes_003Ed__5))]
	public static IEnumerable<Type> GetAllNetworkBehaviourTypes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllNetworkBehaviourTypes_003Ed__5(-2);
	}

	[IteratorStateMachine(typeof(_003CGetAllWeavedNetworkBehaviourTypes_003Ed__6))]
	public static IEnumerable<Type> GetAllWeavedNetworkBehaviourTypes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllWeavedNetworkBehaviourTypes_003Ed__6(-2);
	}

	[IteratorStateMachine(typeof(_003CGetAllWeaverGeneratedTypes_003Ed__7))]
	public static IEnumerable<Type> GetAllWeaverGeneratedTypes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllWeaverGeneratedTypes_003Ed__7(-2);
	}
}
