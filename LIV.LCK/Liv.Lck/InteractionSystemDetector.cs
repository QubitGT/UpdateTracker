using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Liv.Lck;

internal class InteractionSystemDetector
{
	internal enum InteractionSystem
	{
		XRInteractionToolkit,
		OculusInteraction
	}

	private static bool _scanned;

	private static readonly List<InteractionSystem> _detectedSystems = new List<InteractionSystem>();

	private static readonly string[] _xrInteractionToolkitTypeNames = new string[3] { "UnityEngine.XR.Interaction.Toolkit.XRInteractionManager", "UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor", "UnityEngine.XR.Interaction.Toolkit.XRDirectInteractor" };

	private static readonly string[] _oculusInteractionTypeNames = new string[4] { "Oculus.Interaction.Interactor", "Oculus.Interaction.HandGrab.HandGrabInteractable", "Oculus.Interaction.Interactable", "Oculus.Interaction.Input.Hand" };

	public static IReadOnlyCollection<InteractionSystem> GetAvailableInteractionSystems()
	{
		EnsureScanned();
		return _detectedSystems;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void EnsureScanned()
	{
		if (!_scanned)
		{
			_scanned = true;
			if (AnyTypeExists(_xrInteractionToolkitTypeNames))
			{
				_detectedSystems.Add(InteractionSystem.XRInteractionToolkit);
			}
			if (AnyTypeExists(_oculusInteractionTypeNames))
			{
				_detectedSystems.Add(InteractionSystem.OculusInteraction);
			}
		}
	}

	private static bool AnyTypeExists(string[] typeNames)
	{
		return typeNames.Any(TypeExists);
	}

	private static bool TypeExists(string fullTypeName)
	{
		if (!(Type.GetType(fullTypeName, throwOnError: false) != null))
		{
			return AppDomain.CurrentDomain.GetAssemblies().Any((Assembly assembly) => TypeExistsInAssembly(fullTypeName, assembly));
		}
		return true;
	}

	private static bool TypeExistsInAssembly(string fullTypeName, Assembly assembly)
	{
		try
		{
			if (assembly.GetType(fullTypeName, throwOnError: false) != null)
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}
}
