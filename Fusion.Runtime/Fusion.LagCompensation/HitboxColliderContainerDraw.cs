using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion.LagCompensation;

public class HitboxColliderContainerDraw : IEnumerable<ColliderDrawInfo>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__2 : IEnumerator<ColliderDrawInfo>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private ColliderDrawInfo _003C_003E2__current;

		public HitboxColliderContainerDraw _003C_003E4__this;

		private int _003Ci_003E5__1;

		ColliderDrawInfo IEnumerator<ColliderDrawInfo>.Current
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
		public _003CGetEnumerator_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
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
				goto IL_00bd;
			}
			_003C_003E1__state = -1;
			_003C_003E4__this._drawInfo.SetContainer(_003C_003E4__this._container);
			_003Ci_003E5__1 = 1;
			goto IL_00cd;
			IL_00bd:
			_003Ci_003E5__1++;
			goto IL_00cd;
			IL_00cd:
			if (_003Ci_003E5__1 <= _003C_003E4__this._container.CollidersCount)
			{
				if (!_003C_003E4__this._container.GetCollider(_003Ci_003E5__1).Active || !_003C_003E4__this._container.GetCollider(_003Ci_003E5__1).Used)
				{
					goto IL_00bd;
				}
				_003C_003E2__current = _003C_003E4__this._drawInfo.FromHitboxCollider(_003Ci_003E5__1);
				_003C_003E1__state = 1;
				return true;
			}
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
	}

	internal HitboxBuffer.HitboxSnapshot _container;

	private ColliderDrawInfo _drawInfo = new ColliderDrawInfo();

	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__2))]
	public IEnumerator<ColliderDrawInfo> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__2(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
