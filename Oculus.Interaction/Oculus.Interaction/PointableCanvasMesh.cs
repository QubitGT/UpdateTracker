using Oculus.Interaction.UnityCanvas;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class PointableCanvasMesh : PointableElement
{
	[Tooltip("This CanvasMesh determines the Pose of PointerEvents.")]
	[SerializeField]
	[FormerlySerializedAs("_canvasRenderTextureMesh")]
	private CanvasMesh _canvasMesh;

	protected override void Start()
	{
		base.Start();
	}

	public override void ProcessPointerEvent(PointerEvent evt)
	{
		Vector3 position = _canvasMesh.ImposterToCanvasTransformPoint(evt.Pose.position);
		base.ProcessPointerEvent(new PointerEvent(pose: new Pose(position, evt.Pose.rotation), identifier: evt.Identifier, type: evt.Type, data: evt.Data));
	}

	public void InjectAllCanvasMeshPointable(CanvasMesh canvasMesh)
	{
		InjectCanvasMesh(canvasMesh);
	}

	public void InjectCanvasMesh(CanvasMesh canvasMesh)
	{
		_canvasMesh = canvasMesh;
	}
}
