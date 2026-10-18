using System;
using System.Collections;
using System.Collections.Generic;

namespace Oculus.Interaction;

public class InteractableRegistry<TInteractor, TInteractable> where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : Interactable<TInteractor, TInteractable>
{
	public struct InteractableSet : IEnumerable<TInteractable>, IEnumerable
	{
		public struct Enumerator : IEnumerator<TInteractable>, IEnumerator, IDisposable
		{
			private readonly InteractableSet _set;

			private int _position;

			private IReadOnlyList<TInteractable> Data => _set._data;

			public TInteractable Current
			{
				get
				{
					if (Data == null || _position < 0)
					{
						throw new InvalidOperationException();
					}
					return Data[_position];
				}
			}

			object IEnumerator.Current => Current;

			public Enumerator(in InteractableSet set)
			{
				_set = set;
				_position = -1;
			}

			public bool MoveNext()
			{
				if (Data == null)
				{
					return false;
				}
				do
				{
					_position++;
				}
				while (_position < Data.Count && !_set.Include(Data[_position]));
				return _position < Data.Count;
			}

			public void Reset()
			{
				_position = -1;
			}

			public void Dispose()
			{
			}
		}

		private readonly IReadOnlyList<TInteractable> _data;

		private readonly ISet<TInteractable> _onlyInclude;

		private readonly TInteractor _testAgainst;

		public InteractableSet(ISet<TInteractable> onlyInclude, TInteractor testAgainst)
		{
			_data = InteractableRegistry<TInteractor, TInteractable>._interactables;
			_onlyInclude = onlyInclude;
			_testAgainst = testAgainst;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(in this);
		}

		IEnumerator<TInteractable> IEnumerable<TInteractable>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private bool Include(TInteractable interactable)
		{
			if (_onlyInclude != null && !_onlyInclude.Contains(interactable))
			{
				return false;
			}
			if (_testAgainst != null)
			{
				if (!_testAgainst.CanSelect(interactable))
				{
					return false;
				}
				if (!interactable.CanBeSelectedBy(_testAgainst))
				{
					return false;
				}
			}
			return true;
		}
	}

	private static List<TInteractable> _interactables;

	public InteractableRegistry()
	{
		_interactables = new List<TInteractable>();
	}

	public virtual void Register(TInteractable interactable)
	{
		_interactables.Add(interactable);
	}

	public virtual void Unregister(TInteractable interactable)
	{
		_interactables.Remove(interactable);
	}

	protected InteractableSet List(TInteractor interactor, HashSet<TInteractable> onlyInclude)
	{
		return new InteractableSet(onlyInclude, interactor);
	}

	public virtual InteractableSet List(TInteractor interactor)
	{
		return new InteractableSet(null, interactor);
	}

	public virtual InteractableSet List()
	{
		return new InteractableSet(null, null);
	}
}
