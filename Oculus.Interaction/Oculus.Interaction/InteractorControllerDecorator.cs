using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractorControllerDecorator : MonoBehaviour
{
	private class Decorator : ClassToClassDecorator<IInteractorView, IController>
	{
		private Decorator()
		{
		}

		public static Decorator GetFromContext(Context context)
		{
			return context.GetOrCreateSingleton(() => new Decorator());
		}
	}

	[SerializeField]
	[Interface(typeof(IInteractorView), new Type[] { })]
	[Tooltip("Individually-listed interactors to be associated with the specified IController via Context decoration")]
	private Component[] _interactors;

	[SerializeField]
	[Tooltip("Individually-listed GameObjects which are the roots of interactor hierarchies; on initialization, all IInteractorView instances hierarchically descended from these GameObjects will be associated with the specified IController via Context decoration")]
	private GameObject[] _interactorHierarchies;

	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	[Tooltip("The IController to be associated with the specified IInteractorViews via Context decoration.")]
	private Component _controller;

	public static bool TryGetControllerForInteractor(IInteractorView interactor, out IController controller)
	{
		return Decorator.GetFromContext(Context.Global.GetInstance()).TryGetDecoration(interactor, out controller);
	}

	private void Awake()
	{
		Decorator fromContext = Decorator.GetFromContext(Context.Global.GetInstance());
		IController decoration = _controller as IController;
		Component[] interactors = _interactors;
		for (int i = 0; i < interactors.Length; i++)
		{
			IInteractorView instance = interactors[i] as IInteractorView;
			fromContext.AddDecoration(instance, decoration);
		}
		GameObject[] interactorHierarchies = _interactorHierarchies;
		for (int i = 0; i < interactorHierarchies.Length; i++)
		{
			IInteractorView[] componentsInChildren = interactorHierarchies[i].GetComponentsInChildren<IInteractorView>();
			foreach (IInteractorView instance2 in componentsInChildren)
			{
				fromContext.AddDecoration(instance2, decoration);
			}
		}
	}
}
