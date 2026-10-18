using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion.LagCompensation;

public class SnapshotHistoryDraw : IEnumerable<HitboxColliderContainerDraw>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__3 : IEnumerator<HitboxColliderContainerDraw>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private HitboxColliderContainerDraw _003C_003E2__current;

		public SnapshotHistoryDraw _003C_003E4__this;

		private int _003Ci_003E5__1;

		HitboxColliderContainerDraw IEnumerator<HitboxColliderContainerDraw>.Current
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
		public _003CGetEnumerator_003Ed__3(int _003C_003E1__state)
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
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__1 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__1++;
				break;
			}
			if (_003Ci_003E5__1 < _003C_003E4__this._buffer.Length)
			{
				_003C_003E4__this._containerDraw._container = _003C_003E4__this._buffer._buffer[_003Ci_003E5__1];
				_003C_003E2__current = _003C_003E4__this._containerDraw;
				_003C_003E1__state = 1;
				return true;
			}
			_003C_003E4__this._containerDraw._container = null;
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

	private HitboxBuffer _buffer;

	private HitboxColliderContainerDraw _containerDraw = new HitboxColliderContainerDraw();

	internal SnapshotHistoryDraw(HitboxBuffer buffer)
	{
		_buffer = buffer;
	}

	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__3))]
	public IEnumerator<HitboxColliderContainerDraw> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__3(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
