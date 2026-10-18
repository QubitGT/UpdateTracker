using System;
using System.Collections.Generic;
using Liv.Lck.DependencyInjection;

namespace Liv.Lck;

public static class LckModuleLoader
{
	private static readonly List<Action<LckDiContainer>> _moduleConfigurators = new List<Action<LckDiContainer>>();

	public static void RegisterModule(Action<LckDiContainer> configure, string name)
	{
		LckLog.Log("LCK: Registered module - " + name, "RegisterModule", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckModuleLoader.cs", 16);
		_moduleConfigurators.Add(configure);
	}

	internal static void Configure(LckDiContainer container)
	{
		foreach (Action<LckDiContainer> moduleConfigurator in _moduleConfigurators)
		{
			moduleConfigurator(container);
		}
	}
}
