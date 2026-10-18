using System;
using UnityEngine;

namespace Oculus.Interaction;

public class SecondaryInteractorConnection : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IInteractorView), new Type[] { })]
	private UnityEngine.Object _primaryInteractor;

	[SerializeField]
	[Interface(typeof(IInteractorView), new Type[] { })]
	private UnityEngine.Object _secondaryInteractor;

	public IInteractorView PrimaryInteractor { get; private set; }

	public IInteractorView SecondaryInteractor { get; private set; }

	protected virtual void Awake()
	{
		PrimaryInteractor = _primaryInteractor as IInteractorView;
		SecondaryInteractor = _secondaryInteractor as IInteractorView;
	}

	protected virtual void Start()
	{
	}

	public void InjectAllSecondaryInteractorConnection(IInteractorView primaryInteractor, IInteractorView secondaryInteractor)
	{
		InjectPrimaryInteractor(primaryInteractor);
		InjectSecondaryInteractorConnection(secondaryInteractor);
	}

	public void InjectPrimaryInteractor(IInteractorView interactorView)
	{
		PrimaryInteractor = interactorView;
		_primaryInteractor = interactorView as UnityEngine.Object;
	}

	public void InjectSecondaryInteractorConnection(IInteractorView interactorView)
	{
		SecondaryInteractor = interactorView;
		_secondaryInteractor = interactorView as UnityEngine.Object;
	}
}
