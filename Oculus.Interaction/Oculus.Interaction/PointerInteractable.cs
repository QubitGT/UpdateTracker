using System;
using UnityEngine;

namespace Oculus.Interaction;

public abstract class PointerInteractable<TInteractor, TInteractable> : Interactable<TInteractor, TInteractable>, IPointable where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : PointerInteractable<TInteractor, TInteractable>
{
	[SerializeField]
	[Interface(typeof(IPointableElement), new Type[] { })]
	[Optional(OptionalAttribute.Flag.DontHide)]
	private UnityEngine.Object _pointableElement;

	public IPointableElement PointableElement { get; protected set; }

	public event Action<PointerEvent> WhenPointerEventRaised = delegate
	{
	};

	public void PublishPointerEvent(PointerEvent evt)
	{
		if (PointableElement != null)
		{
			PointableElement.ProcessPointerEvent(evt);
		}
		this.WhenPointerEventRaised(evt);
	}

	protected override void Awake()
	{
		base.Awake();
		PointableElement = _pointableElement as IPointableElement;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		_ = _pointableElement != null;
		this.EndStart(ref _started);
	}

	public void InjectOptionalPointableElement(IPointableElement pointableElement)
	{
		PointableElement = pointableElement;
		_pointableElement = pointableElement as UnityEngine.Object;
	}
}
