using System;
using System.Reflection;
using System.Threading;
using Meta.Voice.Logging;
using Meta.WitAi.Utilities;

namespace Meta.WitAi;

[LogCategory("MatchIntent")]
internal static class MatchIntentRegistry
{
	private static DictionaryList<string, RegisteredMatchIntent> registeredMethods;

	public static IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger("MatchIntent");


	public static DictionaryList<string, RegisteredMatchIntent> RegisteredMethods
	{
		get
		{
			if (registeredMethods == null)
			{
				Initialize();
			}
			return registeredMethods;
		}
	}

	internal static void Initialize()
	{
		if (registeredMethods == null)
		{
			registeredMethods = new DictionaryList<string, RegisteredMatchIntent>();
			ThreadUtility.Background(Logger, RefreshAssemblies).WrapErrors();
		}
	}

	internal static void RefreshAssemblies()
	{
		if (Thread.CurrentThread.ThreadState == ThreadState.Aborted)
		{
			return;
		}
		DictionaryList<string, RegisteredMatchIntent> dictionaryList = new DictionaryList<string, RegisteredMatchIntent>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					try
					{
						MethodInfo[] methods = type.GetMethods();
						foreach (MethodInfo methodInfo in methods)
						{
							try
							{
								foreach (Attribute customAttribute in methodInfo.GetCustomAttributes(typeof(MatchIntent)))
								{
									try
									{
										MatchIntent matchIntent = (MatchIntent)customAttribute;
										dictionaryList[matchIntent.Intent].Add(new RegisteredMatchIntent
										{
											type = type,
											method = methodInfo,
											matchIntent = matchIntent
										});
									}
									catch (Exception ex)
									{
										Logger.Debug(ex.Message, null, null, null, null, "RefreshAssemblies", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\ResponseManager\\MatchIntentRegistry.cs", 84);
									}
								}
							}
							catch (Exception ex2)
							{
								Logger.Debug(ex2.Message, null, null, null, null, "RefreshAssemblies", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\ResponseManager\\MatchIntentRegistry.cs", 88);
							}
						}
					}
					catch (Exception ex3)
					{
						Logger.Debug(ex3.Message, null, null, null, null, "RefreshAssemblies", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\ResponseManager\\MatchIntentRegistry.cs", 92);
					}
				}
			}
			catch (Exception ex4)
			{
				Logger.Debug(ex4.Message, null, null, null, null, "RefreshAssemblies", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\ResponseManager\\MatchIntentRegistry.cs", 96);
			}
		}
		registeredMethods = dictionaryList;
	}
}
