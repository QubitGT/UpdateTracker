using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Gizmo;

internal class GizmoRenderer : MonoBehaviour
{
	private DebugGizmoType _gizmoType;

	private Color _gizmoColor;

	private object _dataSource;

	public void SetUpGizmo(DebugGizmoType gizmoType, Color gizmoColor)
	{
		_gizmoType = gizmoType;
		_gizmoColor = gizmoColor;
	}

	public void UpdateDataSource(object dataSource)
	{
		_dataSource = dataSource;
	}

	private void Start()
	{
		DebugGizmos.LineWidth = 0.01f;
	}

	private void Update()
	{
		using (new DebugGizmos.ColorScope(_gizmoColor))
		{
			GizmoTypesRegistry.RenderGizmo(_gizmoType, _dataSource);
		}
	}
}
