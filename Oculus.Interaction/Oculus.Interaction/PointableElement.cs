using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class PointableElement : MonoBehaviour, IPointableElement, IPointable
{
	[Tooltip("If checked, if you’re selecting an object with one hand and then select it with the other hand, the original hand is forced to release the object.")]
	[SerializeField]
	private bool _transferOnSecondSelection;

	[Tooltip("If checked, when you select an object, that hand’s Vector3 points are added to the beginning of the list of Vector3 points instead of the end. This property has very unique usecases, so in most cases you should use the Transfer on Second Selection property instead.")]
	[SerializeField]
	private bool _addNewPointsToFront;

	[Tooltip("Events will be forwarded to this element. Can be used to chain multiple PointableElements together. However, we recommend using the Interactable's Forward Element field instead.")]
	[SerializeField]
	[Interface(typeof(IPointableElement), new Type[] { })]
	[Optional]
	private UnityEngine.Object _forwardElement;

	protected List<Pose> _points;

	protected List<int> _pointIds;

	protected List<Pose> _selectingPoints;

	protected List<int> _selectingPointIds;

	protected bool _started;

	public IPointableElement ForwardElement { get; private set; }

	public bool TransferOnSecondSelection
	{
		get
		{
			return _transferOnSecondSelection;
		}
		set
		{
			_transferOnSecondSelection = value;
		}
	}

	public bool AddNewPointsToFront
	{
		get
		{
			return _addNewPointsToFront;
		}
		set
		{
			_addNewPointsToFront = value;
		}
	}

	public List<Pose> Points => _points;

	public int PointsCount => _points.Count;

	public List<Pose> SelectingPoints => _selectingPoints;

	public int SelectingPointsCount => _selectingPoints.Count;

	public event Action<PointerEvent> WhenPointerEventRaised = delegate
	{
	};

	protected virtual void Awake()
	{
		ForwardElement = _forwardElement as IPointableElement;
		_points = new List<Pose>();
		_pointIds = new List<int>();
		_selectingPoints = new List<Pose>();
		_selectingPointIds = new List<int>();
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_ = (bool)_forwardElement;
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started && ForwardElement != null)
		{
			ForwardElement.WhenPointerEventRaised += HandlePointerEventRaised;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			while (_selectingPoints.Count > 0)
			{
				Cancel(new PointerEvent(_selectingPointIds[0], PointerEventType.Cancel, _selectingPoints[0]));
			}
			if (ForwardElement != null)
			{
				ForwardElement.WhenPointerEventRaised -= HandlePointerEventRaised;
			}
		}
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		if (evt.Type == PointerEventType.Cancel)
		{
			ProcessPointerEvent(evt);
		}
	}

	public virtual void ProcessPointerEvent(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Hover:
			Hover(evt);
			break;
		case PointerEventType.Unhover:
			Unhover(evt);
			break;
		case PointerEventType.Move:
			Move(evt);
			break;
		case PointerEventType.Select:
			Select(evt);
			break;
		case PointerEventType.Unselect:
			Unselect(evt);
			break;
		case PointerEventType.Cancel:
			Cancel(evt);
			break;
		}
	}

	private void Hover(PointerEvent evt)
	{
		if (_addNewPointsToFront)
		{
			_pointIds.Insert(0, evt.Identifier);
			_points.Insert(0, evt.Pose);
		}
		else
		{
			_pointIds.Add(evt.Identifier);
			_points.Add(evt.Pose);
		}
		PointableElementUpdated(evt);
	}

	private void Move(PointerEvent evt)
	{
		int num = _pointIds.IndexOf(evt.Identifier);
		if (num != -1)
		{
			_points[num] = evt.Pose;
			num = _selectingPointIds.IndexOf(evt.Identifier);
			if (num != -1)
			{
				_selectingPoints[num] = evt.Pose;
			}
			PointableElementUpdated(evt);
		}
	}

	private void Unhover(PointerEvent evt)
	{
		int num = _pointIds.IndexOf(evt.Identifier);
		if (num != -1)
		{
			_pointIds.RemoveAt(num);
			_points.RemoveAt(num);
			PointableElementUpdated(evt);
		}
	}

	private void Select(PointerEvent evt)
	{
		if (_selectingPoints.Count == 1 && _transferOnSecondSelection)
		{
			Cancel(new PointerEvent(_selectingPointIds[0], PointerEventType.Cancel, _selectingPoints[0]));
		}
		if (_addNewPointsToFront)
		{
			_selectingPointIds.Insert(0, evt.Identifier);
			_selectingPoints.Insert(0, evt.Pose);
		}
		else
		{
			_selectingPointIds.Add(evt.Identifier);
			_selectingPoints.Add(evt.Pose);
		}
		PointableElementUpdated(evt);
	}

	private void Unselect(PointerEvent evt)
	{
		int num = _selectingPointIds.IndexOf(evt.Identifier);
		if (num != -1)
		{
			_selectingPointIds.RemoveAt(num);
			_selectingPoints.RemoveAt(num);
			PointableElementUpdated(evt);
		}
	}

	private void Cancel(PointerEvent evt)
	{
		int num = _selectingPointIds.IndexOf(evt.Identifier);
		if (num != -1)
		{
			_selectingPointIds.RemoveAt(num);
			_selectingPoints.RemoveAt(num);
		}
		num = _pointIds.IndexOf(evt.Identifier);
		if (num != -1)
		{
			_pointIds.RemoveAt(num);
			_points.RemoveAt(num);
			PointableElementUpdated(evt);
		}
	}

	protected virtual void PointableElementUpdated(PointerEvent evt)
	{
		if (ForwardElement != null)
		{
			ForwardElement.ProcessPointerEvent(evt);
		}
		this.WhenPointerEventRaised(evt);
	}

	public void InjectOptionalForwardElement(IPointableElement forwardElement)
	{
		ForwardElement = forwardElement;
		_forwardElement = forwardElement as UnityEngine.Object;
	}
}
