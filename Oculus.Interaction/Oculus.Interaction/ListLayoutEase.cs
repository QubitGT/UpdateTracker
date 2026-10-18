using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class ListLayoutEase
{
	private class ListElementEase
	{
		private AnimationCurve _curve;

		private float _curveTime;

		private float _startTime;

		private float _start;

		private float _target;

		public float position;

		public ListElementEase(AnimationCurve curve, float easeTime, float position)
		{
			_curve = curve;
			_curveTime = easeTime;
			_start = (_target = (this.position = position));
		}

		public void SetTarget(float target, float time, bool skipEase)
		{
			_target = target;
			if (!skipEase)
			{
				_start = position;
				_startTime = time;
			}
			else
			{
				_start = target;
				position = target;
			}
		}

		public void UpdateTime(float time)
		{
			float time2 = Mathf.Clamp01((time - _startTime) / _curveTime);
			float num = _curve.Evaluate(time2);
			position = (_target - _start) * num + _start;
		}
	}

	private ListLayout _listLayout;

	private Dictionary<int, ListElementEase> _elementDict;

	private AnimationCurve _curve;

	private float _curveTime;

	private float _time;

	public ListLayoutEase(ListLayout layout, float curveTime = 0.3f, AnimationCurve curve = null)
	{
		_curve = curve;
		_curveTime = curveTime;
		if (_curve == null)
		{
			_curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
		_elementDict = new Dictionary<int, ListElementEase>();
		_listLayout = layout;
		ListLayout listLayout = _listLayout;
		listLayout.WhenElementAdded = (Action<int>)Delegate.Combine(listLayout.WhenElementAdded, new Action<int>(HandleElementAdded));
		ListLayout listLayout2 = _listLayout;
		listLayout2.WhenElementUpdated = (Action<int, bool>)Delegate.Combine(listLayout2.WhenElementUpdated, new Action<int, bool>(HandleElementUpdated));
		ListLayout listLayout3 = _listLayout;
		listLayout3.WhenElementRemoved = (Action<int>)Delegate.Combine(listLayout3.WhenElementRemoved, new Action<int>(HandleElementRemoved));
	}

	private void HandleElementAdded(int id)
	{
		float elementPosition = _listLayout.GetElementPosition(id);
		_elementDict.Add(id, new ListElementEase(_curve, _curveTime, elementPosition));
	}

	private void HandleElementUpdated(int id, bool sizeUpdate)
	{
		_elementDict[id].SetTarget(_listLayout.GetElementPosition(id), _time, sizeUpdate);
	}

	private void HandleElementRemoved(int id)
	{
		_elementDict.Remove(id);
	}

	public void UpdateTime(float time)
	{
		_time = time;
		foreach (ListElementEase value in _elementDict.Values)
		{
			value.UpdateTime(_time);
		}
	}

	public float GetPosition(int id)
	{
		return _elementDict[id].position;
	}
}
