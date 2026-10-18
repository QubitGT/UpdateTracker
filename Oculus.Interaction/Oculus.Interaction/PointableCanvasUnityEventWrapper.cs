using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class PointableCanvasUnityEventWrapper : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IPointableCanvas), new Type[] { })]
	private UnityEngine.Object _pointableCanvas;

	private IPointableCanvas PointableCanvas;

	[SerializeField]
	[Tooltip("Selection and hover events will not be fired while dragging.")]
	private bool _suppressWhileDragging = true;

	[SerializeField]
	[Tooltip("Raised when beginning hover of a uGUI selectable")]
	private UnityEvent _whenBeginHighlight;

	[SerializeField]
	[Tooltip("Raised when ending hover of a uGUI selectable")]
	private UnityEvent _whenEndHighlight;

	[SerializeField]
	[Tooltip("Raised when selecting a hovered uGUI selectable")]
	private UnityEvent _whenSelectedHovered;

	[SerializeField]
	[Tooltip("Raised when selecting with no uGUI selectable hovered")]
	private UnityEvent _whenSelectedEmpty;

	[SerializeField]
	[Tooltip("Raised when deselecting a hovered uGUI selectable")]
	private UnityEvent _whenUnselectedHovered;

	[SerializeField]
	[Tooltip("Raised when deselecting with no uGUI selectable hovered")]
	private UnityEvent _whenUnselectedEmpty;

	protected bool _started;

	private bool ShouldFireEvent(PointableCanvasEventArgs args)
	{
		if (args.Canvas != PointableCanvas.Canvas)
		{
			return false;
		}
		if (_suppressWhileDragging && args.Dragging)
		{
			return false;
		}
		return true;
	}

	private void PointableCanvasModule_WhenSelectableHoverEnter(PointableCanvasEventArgs args)
	{
		if (ShouldFireEvent(args))
		{
			_whenBeginHighlight.Invoke();
		}
	}

	private void PointableCanvasModule_WhenSelectableHoverExit(PointableCanvasEventArgs args)
	{
		if (ShouldFireEvent(args))
		{
			_whenEndHighlight.Invoke();
		}
	}

	private void PointableCanvasModule_WhenSelectableSelected(PointableCanvasEventArgs args)
	{
		if (ShouldFireEvent(args))
		{
			if (args.Hovered == null)
			{
				_whenSelectedEmpty.Invoke();
			}
			else
			{
				_whenSelectedHovered.Invoke();
			}
		}
	}

	private void PointableCanvasModule_WhenSelectableUnselected(PointableCanvasEventArgs args)
	{
		if (ShouldFireEvent(args))
		{
			if (args.Hovered == null)
			{
				_whenUnselectedEmpty.Invoke();
			}
			else
			{
				_whenUnselectedHovered.Invoke();
			}
		}
	}

	protected virtual void Awake()
	{
		PointableCanvas = _pointableCanvas as IPointableCanvas;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			PointableCanvasModule.WhenSelectableHovered += PointableCanvasModule_WhenSelectableHoverEnter;
			PointableCanvasModule.WhenSelectableUnhovered += PointableCanvasModule_WhenSelectableHoverExit;
			PointableCanvasModule.WhenSelected += PointableCanvasModule_WhenSelectableSelected;
			PointableCanvasModule.WhenUnselected += PointableCanvasModule_WhenSelectableUnselected;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			PointableCanvasModule.WhenSelectableHovered -= PointableCanvasModule_WhenSelectableHoverEnter;
			PointableCanvasModule.WhenSelectableUnhovered -= PointableCanvasModule_WhenSelectableHoverExit;
			PointableCanvasModule.WhenSelected -= PointableCanvasModule_WhenSelectableSelected;
			PointableCanvasModule.WhenUnselected -= PointableCanvasModule_WhenSelectableUnselected;
		}
	}
}
