using System;
using System.Collections.Generic;

namespace Oculus.Interaction;

public class ListLayout
{
	public class ListElement
	{
		public int id;

		public float pos;

		public float halfSize;

		public ListElement prev;

		public ListElement next;

		public ListElement(int id, float size)
		{
			this.id = id;
			halfSize = size / 2f;
			pos = 0f;
			prev = null;
			next = null;
		}
	}

	private ListElement _root;

	private Dictionary<int, ListElement> _elements;

	public Action<int> WhenElementAdded;

	public Action<int, bool> WhenElementUpdated;

	public Action<int> WhenElementRemoved;

	private bool _sizeUpdate;

	private int _moveElement = -1;

	private float _size;

	public float Size => _size;

	public ListLayout()
	{
		_root = null;
		_elements = new Dictionary<int, ListElement>();
		WhenElementAdded = delegate
		{
		};
		WhenElementUpdated = delegate
		{
		};
		WhenElementRemoved = delegate
		{
		};
	}

	public void AddElement(int id, float size, float target = float.MaxValue)
	{
		if (_elements.ContainsKey(id))
		{
			return;
		}
		ListElement listElement = new ListElement(id, size);
		_size += size;
		_elements[id] = listElement;
		WhenElementAdded(id);
		if (_root == null)
		{
			_elements[id] = listElement;
			_root = listElement;
			UpdatePositionsFromRoot();
			return;
		}
		ListElement listElement2 = _root;
		while (listElement2.next != null)
		{
			listElement2 = listElement2.next;
		}
		listElement2.next = listElement;
		listElement.prev = listElement2;
		UpdatePositionsFromRoot();
		MoveElement(id, target);
		UpdatePos(listElement, listElement.pos, force: true);
	}

	public void RemoveElement(int id)
	{
		if (!_elements.TryGetValue(id, out var value))
		{
			return;
		}
		if (value.prev != null)
		{
			value.prev.next = value.next;
		}
		if (value.next != null)
		{
			value.next.prev = value.prev;
		}
		if (_root == value)
		{
			if (value.next != null)
			{
				_root = value.next;
			}
			else
			{
				_root = null;
			}
		}
		_size -= value.halfSize * 2f;
		UpdatePositionsFromRoot();
		_elements.Remove(id);
		WhenElementRemoved(id);
	}

	private void UpdatePos(ListElement element, float pos, bool force = false)
	{
		if (pos != element.pos || force)
		{
			element.pos = pos;
			WhenElementUpdated(element.id, _sizeUpdate || _moveElement == element.id || force);
		}
	}

	private void UpdatePositionsFromRoot()
	{
		if (_root != null)
		{
			UpdatePos(_root, _root.halfSize - _size / 2f);
			UpdatePositionsRight(_root);
		}
	}

	private void UpdatePositionsRight(ListElement current)
	{
		while (current.next != null)
		{
			UpdatePos(current.next, current.pos + current.halfSize + current.next.halfSize);
			current = current.next;
		}
	}

	private void SwapWithNext(ListElement element)
	{
		if (element.prev != null)
		{
			element.prev.next = element.next;
		}
		if (element.next.next != null)
		{
			element.next.next.prev = element;
		}
		element.next.prev = element.prev;
		element.prev = element.next;
		element.next = element.prev.next;
		element.prev.next = element;
		if (element == _root || element.prev == _root)
		{
			_root = ((element == _root) ? element.prev : element);
			UpdatePositionsFromRoot();
		}
		else
		{
			UpdatePos(element.prev, element.prev.prev.pos + element.prev.prev.halfSize + element.prev.halfSize);
			UpdatePos(element, element.prev.pos + element.prev.halfSize + element.halfSize);
		}
	}

	private void SwapWithPrev(ListElement element)
	{
		SwapWithNext(element.prev);
	}

	public void MoveElement(int id, float target)
	{
		_moveElement = id;
		if (!_elements.TryGetValue(id, out var value))
		{
			_moveElement = -1;
			return;
		}
		if (target > value.pos)
		{
			while (value.next != null)
			{
				float num = value.pos + (value.halfSize + value.next.halfSize) / 2f;
				if (target < num)
				{
					break;
				}
				SwapWithNext(value);
			}
		}
		else
		{
			while (value.prev != null)
			{
				float num2 = value.pos - (value.halfSize + value.prev.halfSize) / 2f;
				if (target > num2)
				{
					break;
				}
				SwapWithPrev(value);
			}
		}
		_moveElement = -1;
	}

	public void UpdateElementSize(int id, float size)
	{
		if (_elements.TryGetValue(id, out var value))
		{
			_sizeUpdate = true;
			float num = size - value.halfSize * 2f;
			_size += num;
			value.halfSize = size / 2f;
			UpdatePositionsFromRoot();
			_sizeUpdate = false;
		}
	}

	public float GetElementPosition(int id)
	{
		if (!_elements.TryGetValue(id, out var value))
		{
			return 0f;
		}
		return value.pos;
	}

	public float GetElementSize(int id)
	{
		if (!_elements.TryGetValue(id, out var value))
		{
			return 0f;
		}
		return value.halfSize * 2f;
	}

	public float GetTargetPosition(int id, float target, float size)
	{
		if (_elements.ContainsKey(id))
		{
			return GetElementPosition(id);
		}
		if (_root == null)
		{
			return 0f;
		}
		float num = (0f - (_size + size)) / 2f + size / 2f;
		for (ListElement listElement = _root; listElement != null; listElement = listElement.next)
		{
			float num2 = size / 2f + listElement.halfSize;
			float num3 = num + num2 / 2f;
			if (target < num3)
			{
				break;
			}
			num += num2;
		}
		return num;
	}
}
