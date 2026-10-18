using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class PokeInteractableVisual : MonoBehaviour
{
	[Tooltip("The Poke Interactable.")]
	[SerializeField]
	private PokeInteractable _pokeInteractable;

	[Tooltip("Acts as the limit of the button (the point where it's fully depressed).")]
	[SerializeField]
	private Transform _buttonBaseTransform;

	private float _maxOffsetAlongNormal;

	private Vector2 _planarOffset;

	private HashSet<PokeInteractor> _pokeInteractors;

	private PokeInteractor _postProcessInteractor;

	protected bool _started;

	private Action _postProcessHandler => UpdateComponentPosition;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_pokeInteractors = new HashSet<PokeInteractor>();
		_maxOffsetAlongNormal = Vector3.Dot(base.transform.position - _buttonBaseTransform.position, -1f * _buttonBaseTransform.forward);
		Vector3 vector = base.transform.position - _maxOffsetAlongNormal * _buttonBaseTransform.forward;
		_planarOffset = new Vector2(Vector3.Dot(vector - _buttonBaseTransform.position, _buttonBaseTransform.right), Vector3.Dot(vector - _buttonBaseTransform.position, _buttonBaseTransform.up));
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_pokeInteractors.Clear();
			_pokeInteractors.UnionWith(_pokeInteractable.Interactors);
			_pokeInteractable.WhenInteractorAdded.Action += HandleInteractorAdded;
			_pokeInteractable.WhenInteractorRemoved.Action += HandleInteractorRemoved;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_pokeInteractors.Clear();
			_pokeInteractable.WhenInteractorAdded.Action -= HandleInteractorAdded;
			_pokeInteractable.WhenInteractorRemoved.Action -= HandleInteractorRemoved;
			if ((bool)_postProcessInteractor)
			{
				_postProcessInteractor.WhenPostprocessed -= _postProcessHandler;
				_postProcessInteractor = null;
				UpdateComponentPosition();
			}
		}
	}

	private void HandleInteractorAdded(PokeInteractor pokeInteractor)
	{
		_pokeInteractors.Add(pokeInteractor);
		if (_postProcessInteractor == null)
		{
			_postProcessInteractor = pokeInteractor;
			_postProcessInteractor.WhenPostprocessed += _postProcessHandler;
		}
	}

	private void HandleInteractorRemoved(PokeInteractor pokeInteractor)
	{
		_pokeInteractors.Remove(pokeInteractor);
		if (!(pokeInteractor == _postProcessInteractor))
		{
			return;
		}
		_postProcessInteractor.WhenPostprocessed -= _postProcessHandler;
		using HashSet<PokeInteractor>.Enumerator enumerator = _pokeInteractors.GetEnumerator();
		if (enumerator.MoveNext() && enumerator.Current != null)
		{
			_postProcessInteractor = enumerator.Current;
			_postProcessInteractor.WhenPostprocessed += _postProcessHandler;
		}
		else
		{
			_postProcessInteractor = null;
			UpdateComponentPosition();
		}
	}

	private void UpdateComponentPosition()
	{
		float num = _maxOffsetAlongNormal;
		foreach (PokeInteractor pokeInteractor in _pokeInteractors)
		{
			float num2 = Vector3.Dot(pokeInteractor.Origin - _buttonBaseTransform.position, -1f * _buttonBaseTransform.forward);
			num2 -= pokeInteractor.Radius;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			num = Math.Min(num2, num);
		}
		base.transform.position = _buttonBaseTransform.position + _buttonBaseTransform.forward * (-1f * num) + _buttonBaseTransform.right * _planarOffset.x + _buttonBaseTransform.up * _planarOffset.y;
	}

	public void InjectAllPokeInteractableVisual(PokeInteractable pokeInteractable, Transform buttonBaseTransform)
	{
		InjectPokeInteractable(pokeInteractable);
		InjectButtonBaseTransform(buttonBaseTransform);
	}

	public void InjectPokeInteractable(PokeInteractable pokeInteractable)
	{
		_pokeInteractable = pokeInteractable;
	}

	public void InjectButtonBaseTransform(Transform buttonBaseTransform)
	{
		_buttonBaseTransform = buttonBaseTransform;
	}
}
