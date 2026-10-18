using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Oculus.Interaction.Input;

public class ScrollInputProvider : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IAxis2D), new Type[] { })]
	[Tooltip("Input 2D Axis from which the horizontal and vertical axis will be extracted")]
	private UnityEngine.Object _axis2D;

	[SerializeField]
	[Interface(typeof(IInteractorView), new Type[] { })]
	private UnityEngine.Object _interactor;

	[SerializeField]
	[Optional]
	[Tooltip("The speed at which scrolling occurs.")]
	private float _scrollSpeed = 5f;

	[SerializeField]
	[Optional]
	[Tooltip("The dead zone threshold for input.")]
	private float _deadZone = 0.1f;

	[SerializeField]
	[Optional]
	[Tooltip("Enable or disable scrolling on the X axis.")]
	private bool _scrollXAxis = true;

	[SerializeField]
	[Optional]
	[Tooltip("Enable or disable scrolling on the Y axis.")]
	private bool _scrollYAxis = true;

	[SerializeField]
	[Optional]
	[Tooltip("Invert the X axis input.")]
	private bool _invertXAxis;

	[SerializeField]
	[Optional]
	[Tooltip("Invert the Y axis input.")]
	private bool _invertYAxis;

	private PointerEventData _pointerEventData;

	private PointableCanvasModule.Pointer _currentPointer;

	private bool _started;

	private IAxis2D Axis2D { get; set; }

	private IInteractorView InteractorView { get; set; }

	private void Awake()
	{
		Axis2D = _axis2D as IAxis2D;
		InteractorView = _interactor as IInteractorView;
	}

	private void Start()
	{
		this.BeginStart(ref _started);
		_pointerEventData = new PointerEventData(EventSystem.current);
		this.EndStart(ref _started);
	}

	private void OnEnable()
	{
		if (_started)
		{
			PointableCanvasModule.WhenPointerStarted += HandlePointerStarted;
		}
	}

	private void OnDisable()
	{
		if (_started)
		{
			PointableCanvasModule.WhenPointerStarted -= HandlePointerStarted;
			if (_currentPointer != null)
			{
				_currentPointer.WhenUpdated -= HandlePointerUpdated;
				_currentPointer = null;
			}
		}
	}

	private void HandlePointerStarted(PointableCanvasModule.Pointer pointer)
	{
		if (pointer.Identifier == InteractorView.Identifier)
		{
			if (_currentPointer != null)
			{
				_currentPointer.WhenUpdated -= HandlePointerUpdated;
			}
			pointer.WhenUpdated += HandlePointerUpdated;
			_currentPointer = pointer;
		}
	}

	private void HandlePointerUpdated(PointerEventData pointerEventData)
	{
		Vector2 vector = TryGetScrollData();
		if (vector != Vector2.zero)
		{
			_pointerEventData.scrollDelta = vector;
			_pointerEventData.position = pointerEventData.position;
			ExecuteEvents.ExecuteHierarchy(pointerEventData.pointerCurrentRaycast.gameObject, _pointerEventData, ExecuteEvents.scrollHandler);
		}
	}

	private Vector2 TryGetScrollData()
	{
		Vector2 input = Axis2D.Value();
		if (input.magnitude < _deadZone)
		{
			return Vector2.zero;
		}
		input = ApplyAxisSettings(input);
		return input * _scrollSpeed;
	}

	private Vector2 ApplyAxisSettings(Vector2 input)
	{
		input.x = ((!_scrollXAxis) ? 0f : (_invertXAxis ? (0f - input.x) : input.x));
		input.y = ((!_scrollYAxis) ? 0f : (_invertYAxis ? (0f - input.y) : input.y));
		return input;
	}

	public void InjectAll(IAxis2D axis2D, IInteractorView interactorView)
	{
		InjectAxis2D(axis2D);
		InjectInteractorView(interactorView);
	}

	public void InjectAxis2D(IAxis2D axis2D)
	{
		_axis2D = axis2D as UnityEngine.Object;
		Axis2D = axis2D;
	}

	public void InjectInteractorView(IInteractorView interactorView)
	{
		_interactor = interactorView as UnityEngine.Object;
		InteractorView = interactorView;
	}
}
