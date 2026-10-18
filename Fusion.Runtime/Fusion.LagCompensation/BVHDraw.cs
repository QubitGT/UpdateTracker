using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion.LagCompensation;

public class BVHDraw : IEnumerable<BVHNodeDrawInfo>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__4 : IEnumerator<BVHNodeDrawInfo>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private BVHNodeDrawInfo _003C_003E2__current;

		public BVHDraw _003C_003E4__this;

		private BVHNode _003Cnode_003E5__1;

		BVHNodeDrawInfo IEnumerator<BVHNodeDrawInfo>.Current
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
		public _003CGetEnumerator_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cnode_003E5__1 = default(BVHNode);
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
				_003C_003E4__this._reusableStack.Clear();
				_003C_003E4__this._reusableStack.Push(_003C_003E4__this._drawInfo.Buffer.BVH.rootBVH);
				break;
			case 1:
				_003C_003E1__state = -1;
				if (_003Cnode_003E5__1.HasLeft)
				{
					_003C_003E4__this._reusableStack.Push(_003Cnode_003E5__1.GetLeft(_003C_003E4__this._drawInfo.Buffer.BVH));
				}
				if (_003Cnode_003E5__1.HasRight)
				{
					_003C_003E4__this._reusableStack.Push(_003Cnode_003E5__1.GetRight(_003C_003E4__this._drawInfo.Buffer.BVH));
				}
				_003Cnode_003E5__1 = default(BVHNode);
				break;
			}
			while (_003C_003E4__this._reusableStack.Count > 0)
			{
				_003Cnode_003E5__1 = _003C_003E4__this._reusableStack.Pop();
				if (!_003Cnode_003E5__1.IsValid)
				{
					continue;
				}
				_003C_003E2__current = _003C_003E4__this._drawInfo.FromBVHNode(ref _003Cnode_003E5__1);
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

	internal HitboxBuffer _buffer;

	private BVHNodeDrawInfo _drawInfo;

	private Stack<BVHNode> _reusableStack = new Stack<BVHNode>();

	internal BVHDraw(HitboxBuffer buffer)
	{
		_buffer = buffer;
		_drawInfo = new BVHNodeDrawInfo(_buffer);
	}

	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__4))]
	public IEnumerator<BVHNodeDrawInfo> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__4(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
