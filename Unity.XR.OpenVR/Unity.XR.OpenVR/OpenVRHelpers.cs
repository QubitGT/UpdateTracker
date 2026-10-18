using System;
using System.Linq;
using System.Reflection;

namespace Unity.XR.OpenVR;

public class OpenVRHelpers
{
	public static bool IsUsingSteamVRInput()
	{
		return DoesTypeExist("SteamVR_Input");
	}

	public static bool DoesTypeExist(string className, bool fullname = false)
	{
		return GetType(className, fullname) != null;
	}

	public static Type GetType(string className, bool fullname = false)
	{
		Type type2 = null;
		if (fullname)
		{
			return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
				from type in assembly.GetTypes()
				where type.FullName == className
				select type).FirstOrDefault();
		}
		return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			from type in assembly.GetTypes()
			where type.Name == className
			select type).FirstOrDefault();
	}

	public static string GetActionManifestPathFromPlugin()
	{
		return (string)GetType("SteamVR_Input").GetMethod("GetActionsFilePath").Invoke(null, new object[1] { false });
	}

	public static string GetActionManifestNameFromPlugin()
	{
		return (string)GetType("SteamVR_Input").GetMethod("GetActionsFileName").Invoke(null, null);
	}

	public static string GetEditorAppKeyFromPlugin()
	{
		return (string)GetType("SteamVR_Input").GetMethod("GetEditorAppKey").Invoke(null, null);
	}
}
