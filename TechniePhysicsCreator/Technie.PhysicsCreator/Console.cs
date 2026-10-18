using UnityEngine;

namespace Technie.PhysicsCreator;

public static class Console
{
	public const bool IS_DEBUG_OUTPUT_ENABLED = false;

	public const bool SHOW_SHADOW_HIERARCHY = false;

	public const bool ENABLE_JOINT_SUPPORT = false;

	public static string Technie;

	public static Logger output;

	static Console()
	{
		Technie = "Technie.PhysicsCreator";
		output = new Logger(Debug.unityLogger.logHandler);
		output.logEnabled = false;
	}
}
