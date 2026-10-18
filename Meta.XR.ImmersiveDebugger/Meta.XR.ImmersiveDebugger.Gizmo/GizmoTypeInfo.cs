using System;

namespace Meta.XR.ImmersiveDebugger.Gizmo;

internal struct GizmoTypeInfo
{
	public readonly Action<object> RenderDelegate;

	public GizmoTypeInfo(Action<object> renderDelegate)
	{
		RenderDelegate = renderDelegate;
	}
}
