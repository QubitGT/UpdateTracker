using Meta.XR.ImmersiveDebugger;
using UnityEngine;

internal class UIEditorSetup : MonoBehaviour
{
	[DebugMember(DebugColor.Gray)]
	public float Float = 0.5f;

	[DebugMember(DebugColor.Gray)]
	public bool Bool = true;

	[DebugMember(DebugColor.Gray, Tweakable = true, Min = 0f, Max = 1f)]
	public float TweakableFloat = 0.5f;

	[DebugMember(DebugColor.Red, GizmoType = DebugGizmoType.Point)]
	public Vector3 Position = Vector3.one;

	[DebugMember(DebugColor.Gray)]
	public void Method()
	{
	}
}
