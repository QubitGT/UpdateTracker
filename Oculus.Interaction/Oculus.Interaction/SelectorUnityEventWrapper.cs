using System;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class SelectorUnityEventWrapper : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(ISelector), new Type[] { })]
	private UnityEngine.Object _selector;

	private ISelector Selector;

	[SerializeField]
	private UnityEvent _whenSelected;

	[SerializeField]
	private UnityEvent _whenUnselected;

	protected bool _started;

	public UnityEvent WhenSelected => _whenSelected;

	public UnityEvent WhenUnselected => _whenUnselected;

	protected virtual void Awake()
	{
		Selector = _selector as ISelector;
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
			Selector.WhenSelected += HandleSelected;
			Selector.WhenUnselected += HandleUnselected;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Selector.WhenSelected -= HandleSelected;
			Selector.WhenUnselected -= HandleUnselected;
		}
	}

	private void HandleSelected()
	{
		_whenSelected.Invoke();
	}

	private void HandleUnselected()
	{
		_whenUnselected.Invoke();
	}

	public void InjectAllSelectorUnityEventWrapper(ISelector selector)
	{
		InjectSelector(selector);
	}

	public void InjectSelector(ISelector selector)
	{
		_selector = selector as UnityEngine.Object;
		Selector = selector;
	}
}
