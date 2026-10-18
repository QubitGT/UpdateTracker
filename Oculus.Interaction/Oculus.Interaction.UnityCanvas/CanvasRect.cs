using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public class CanvasRect : CanvasMesh
{
	protected override Vector3 MeshInverseTransform(Vector3 localPosition)
	{
		return localPosition;
	}

	protected override void GenerateMesh(out List<Vector3> verts, out List<int> tris, out List<Vector2> uvs)
	{
		verts = new List<Vector3>();
		tris = new List<int>();
		uvs = new List<Vector2>();
		Vector2Int baseResolutionToUse = _canvasRenderTexture.GetBaseResolutionToUse();
		Vector2 vector = new Vector2(_canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(baseResolutionToUse.x)), _canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(baseResolutionToUse.y))) / base.transform.lossyScale;
		float num = vector.x * 0.5f;
		float x = 0f - num;
		float num2 = vector.y * 0.5f;
		float y = 0f - num2;
		verts.Add(new Vector3(x, y, 0f));
		verts.Add(new Vector3(x, num2, 0f));
		verts.Add(new Vector3(num, num2, 0f));
		verts.Add(new Vector3(num, y, 0f));
		tris.Add(0);
		tris.Add(1);
		tris.Add(2);
		tris.Add(0);
		tris.Add(2);
		tris.Add(3);
		uvs.Add(new Vector2(0f, 0f));
		uvs.Add(new Vector2(0f, 1f));
		uvs.Add(new Vector2(1f, 1f));
		uvs.Add(new Vector2(1f, 0f));
	}

	public void InjectAllCanvasRect(CanvasRenderTexture canvasRenderTexture, MeshFilter meshFilter)
	{
		InjectAllCanvasMesh(canvasRenderTexture, meshFilter);
	}
}
