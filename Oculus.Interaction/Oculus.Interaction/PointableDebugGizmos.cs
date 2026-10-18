using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class PointableDebugGizmos : MonoBehaviour
{
	private class PointData
	{
		public Pose Pose { get; set; }

		public bool Selecting { get; set; }
	}

	[SerializeField]
	[Interface(typeof(IPointable), new Type[] { })]
	private UnityEngine.Object _pointable;

	[SerializeField]
	private float _radius = 0.01f;

	[SerializeField]
	private Color _hoverColor = Color.blue;

	[SerializeField]
	private Color _selectColor = Color.green;

	[SerializeField]
	private bool _drawAxes = true;

	private Dictionary<int, PointData> _points;

	private IPointable Pointable;

	protected bool _started;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Color HoverColor
	{
		get
		{
			return _hoverColor;
		}
		set
		{
			_hoverColor = value;
		}
	}

	public Color SelectColor
	{
		get
		{
			return _selectColor;
		}
		set
		{
			_selectColor = value;
		}
	}

	public bool DrawAxes
	{
		get
		{
			return _drawAxes;
		}
		set
		{
			_drawAxes = value;
		}
	}

	private void Reset()
	{
		IPointable component = GetComponent<IPointable>();
		InjectAllPointableDebugGizmos(component);
	}

	protected virtual void Awake()
	{
		Pointable = _pointable as IPointable;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_points = new Dictionary<int, PointData>();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised += HandlePointerEventRaised;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
		}
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Hover:
			_points.Add(evt.Identifier, new PointData
			{
				Pose = evt.Pose,
				Selecting = false
			});
			break;
		case PointerEventType.Select:
			_points[evt.Identifier].Selecting = true;
			break;
		case PointerEventType.Move:
			_points[evt.Identifier].Pose = evt.Pose;
			break;
		case PointerEventType.Unselect:
			if (_points.ContainsKey(evt.Identifier))
			{
				_points[evt.Identifier].Selecting = false;
			}
			break;
		case PointerEventType.Unhover:
		case PointerEventType.Cancel:
			_points.Remove(evt.Identifier);
			break;
		}
	}

	protected virtual void LateUpdate()
	{
		foreach (PointData value in _points.Values)
		{
			DebugGizmos.LineWidth = _radius;
			DebugGizmos.Color = (value.Selecting ? _selectColor : _hoverColor);
			DebugGizmos.DrawPoint(value.Pose.position);
			if (_drawAxes)
			{
				DebugGizmos.LineWidth = _radius / 2f;
				DebugGizmos.DrawAxis(value.Pose.position, value.Pose.rotation, _radius * 2f);
			}
		}
	}

	public void InjectAllPointableDebugGizmos(IPointable pointable)
	{
		InjectPointable(pointable);
	}

	public void InjectPointable(IPointable pointable)
	{
		_pointable = pointable as UnityEngine.Object;
		Pointable = pointable;
	}
}
