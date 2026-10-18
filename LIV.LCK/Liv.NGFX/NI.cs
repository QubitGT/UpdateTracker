using System;
using System.Runtime.InteropServices;

namespace Liv.NGFX;

public class NI
{
	[DllImport("ngfx_rs")]
	public static extern IntPtr GetPluginEventFunction();

	[DllImport("ngfx_rs")]
	public static extern uint AllocResource(IntPtr resource_ctx);

	[DllImport("ngfx_rs")]
	public static extern void SetGlobalLogLevel(LogLevel level, bool enableGLMessages);

	[DllImport("ngfx_rs")]
	public static extern IntPtr ngfx_create_context();

	[DllImport("ngfx_rs")]
	public static extern void ngfx_destroy_context(IntPtr ctx);

	[DllImport("ngfx_rs")]
	public static extern int ngfx_get_graphics_api();
}
