namespace UnityEngine.Rendering.RadeonRays;

internal static class Common
{
	public static uint CeilDivide(uint val, uint div)
	{
		return (val + div - 1) / div;
	}

	public static void EnableKeyword(CommandBuffer cmd, ComputeShader shader, string keyword, bool enable)
	{
		if (enable)
		{
			LocalKeyword keyword2 = new LocalKeyword(shader, keyword);
			cmd.EnableKeyword(shader, in keyword2);
		}
		else
		{
			LocalKeyword keyword2 = new LocalKeyword(shader, keyword);
			cmd.DisableKeyword(shader, in keyword2);
		}
	}
}
