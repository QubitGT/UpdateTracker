using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public abstract class Style : ScriptableObject
{
	protected bool _instantiated;

	public bool Instantiated => _instantiated;

	private static string Path<T>() where T : Style
	{
		return "Styles/" + typeof(T).Name + "s/";
	}

	public static T Default<T>() where T : Style
	{
		return Resources.Load<T>(Path<T>() + "Default");
	}

	public static T Load<T>(string name) where T : Style
	{
		return Resources.Load<T>(Path<T>() + name) ?? Default<T>();
	}

	public static T Instantiate<T>(string name) where T : Style
	{
		T val = Object.Instantiate(Load<T>(name));
		val._instantiated = true;
		return val;
	}
}
