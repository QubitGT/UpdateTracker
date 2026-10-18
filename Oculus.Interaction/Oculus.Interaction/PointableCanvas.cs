using UnityEngine;

namespace Oculus.Interaction;

public class PointableCanvas : PointableElement, IPointableCanvas, IPointableElement, IPointable
{
	[Tooltip("PointerEvents will be forwarded to this Unity Canvas.")]
	[SerializeField]
	private Canvas _canvas;

	private bool _registered;

	public Canvas Canvas => _canvas;

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	private void Register()
	{
		PointableCanvasModule.RegisterPointableCanvas(this);
		_registered = true;
	}

	private void Unregister()
	{
		if (_registered)
		{
			PointableCanvasModule.UnregisterPointableCanvas(this);
			_registered = false;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_started)
		{
			Register();
		}
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			Unregister();
		}
		base.OnDisable();
	}

	public void InjectAllPointableCanvas(Canvas canvas)
	{
		InjectCanvas(canvas);
	}

	public void InjectCanvas(Canvas canvas)
	{
		_canvas = canvas;
	}
}
